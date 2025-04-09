using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;

namespace Matteo.Views;

public partial class MainWindow : Window
{
    private readonly Random random = new();
    private Dictionary<string, int> problemStats = new();
    private readonly string statsFilePath = "problem_stats.json";
    private int currentScore;
    private int problemsSolved;
    private int firstNumber;
    private int secondNumber;
    private DispatcherTimer? timer;
    private TimeSpan remainingTime;
    private DispatcherTimer? backgroundTimer;
    private readonly List<TextBlock> backgroundElements = new();
    private readonly List<TextBlock> floatingNumbers = new();

    public MainWindow()
    {
        InitializeComponent();
        LoadStats();
        GenerateNewProblem();
        StartTimer();
        InitializeBackground();
        InitializeFloatingNumbers();

        // Handle key press for Enter key
        AnswerInput.KeyDown += (s, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                CheckAnswer();
            }
        };
    }

    private void InitializeBackground()
    {
        // Create initial background elements
        for (int i = 0; i < 20; i++)
        {
            AddBackgroundElement();
        }

        // Start background animation timer
        backgroundTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        backgroundTimer.Tick += BackgroundTimer_Tick;
        backgroundTimer.Start();
    }

    private void InitializeFloatingNumbers()
    {
        // Create floating numbers
        for (int i = 0; i < 10; i++)
        {
            AddFloatingNumber();
        }
    }

    private void AddFloatingNumber()
    {
        var number = random.Next(1, 10);
        var element = new TextBlock
        {
            Text = number.ToString(),
            FontSize = random.Next(20, 40),
            Foreground = new SolidColorBrush(new Color(77, 255, 255, 255)),
            RenderTransform = new TranslateTransform
            {
                X = random.Next((int)BackgroundCanvas.Bounds.Width),
                Y = random.Next((int)BackgroundCanvas.Bounds.Height)
            }
        };

        BackgroundCanvas.Children.Add(element);
        floatingNumbers.Add(element);
    }

    private void AddBackgroundElement()
    {
        var element = new TextBlock
        {
            Text = random.Next(2) == 0 ? "×" : "+",
            FontSize = random.Next(20, 40),
            Foreground = new SolidColorBrush(new Color(51, 255, 255, 255)),
            RenderTransform = new TranslateTransform
            {
                X = random.Next((int)BackgroundCanvas.Bounds.Width),
                Y = random.Next((int)BackgroundCanvas.Bounds.Height)
            }
        };

        BackgroundCanvas.Children.Add(element);
        backgroundElements.Add(element);
    }

    private void BackgroundTimer_Tick(object? sender, EventArgs e)
    {
        // Animate background elements
        foreach (var element in backgroundElements)
        {
            var transform = (TranslateTransform)element.RenderTransform;
            transform.Y -= 2;

            if (transform.Y < -50)
            {
                transform.Y = BackgroundCanvas.Bounds.Height + 50;
                transform.X = random.Next((int)BackgroundCanvas.Bounds.Width);
            }
        }

        // Animate floating numbers
        foreach (var number in floatingNumbers)
        {
            var transform = (TranslateTransform)number.RenderTransform;
            transform.Y -= 1;
            transform.X += Math.Sin(transform.Y / 50) * 2;

            if (transform.Y < -50)
            {
                transform.Y = BackgroundCanvas.Bounds.Height + 50;
                transform.X = random.Next((int)BackgroundCanvas.Bounds.Width);
            }
        }
    }

    private void LoadStats()
    {
        if (File.Exists(statsFilePath))
        {
            string json = File.ReadAllText(statsFilePath);
            var loadedStats = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
            if (loadedStats != null)
            {
                problemStats = loadedStats;
            }
        }
    }

    private void SaveStats()
    {
        string json = JsonConvert.SerializeObject(problemStats);
        File.WriteAllText(statsFilePath, json);
    }

    private void GenerateNewProblem()
    {
        // Prioritize problems that have been answered incorrectly
        var difficultProblems = new List<(int, int)>();
        foreach (var stat in problemStats)
        {
            if (stat.Value < 0) // Negative value means incorrect answers
            {
                var numbers = stat.Key.Split('x');
                difficultProblems.Add((int.Parse(numbers[0]), int.Parse(numbers[1])));
            }
        }

        if (difficultProblems.Count > 0 && random.Next(2) == 0) // 50% chance to get a difficult problem
        {
            var problem = difficultProblems[random.Next(difficultProblems.Count)];
            firstNumber = problem.Item1;
            secondNumber = problem.Item2;
        }
        else
        {
            firstNumber = random.Next(1, 11);
            secondNumber = random.Next(1, 11);
        }

        // Animate problem text
        if (ProblemText != null)
        {
            ProblemText.Foreground = new SolidColorBrush(Colors.White);
            ProblemText.RenderTransform = new ScaleTransform(1, 1);
            ProblemText.Text = $"{firstNumber} × {secondNumber} = ?";
            
            // Add floating numbers to background
            AddFloatingNumber();
            AddFloatingNumber();
        }

        if (AnswerInput != null)
        {
            AnswerInput.Text = "";
            AnswerInput.Focus();
        }
    }

    private void SubmitButton_Click(object? sender, RoutedEventArgs e)
    {
        CheckAnswer();
    }

    private void CheckAnswer()
    {
        if (int.TryParse(AnswerInput?.Text, out int answer))
        {
            string problemKey = $"{firstNumber}x{secondNumber}";
            int correctAnswer = firstNumber * secondNumber;

            if (answer == correctAnswer)
            {
                // Play correct answer animation
                if (ProblemText != null)
                {
                    ProblemText.Foreground = new SolidColorBrush(Colors.Lime);
                    ProblemText.RenderTransform = new ScaleTransform(1.2, 1.2);
                    
                    // Animate score increase
                    currentScore += 10;
                    problemsSolved++;
                    if (ScoreText != null)
                        ScoreText.Text = $"Poeng: {currentScore}";
                    if (ProgressBar != null)
                        ProgressBar.Value = problemsSolved;

                    // Update stats
                    if (!problemStats.ContainsKey(problemKey))
                        problemStats[problemKey] = 0;
                    problemStats[problemKey]++;

                    if (problemsSolved >= 10)
                    {
                        SaveStats();
                        ShowVictory();
                    }
                    else
                    {
                        // Delay next problem
                        DispatcherTimer.RunOnce(() =>
                        {
                            if (ProblemText != null)
                            {
                                ProblemText.RenderTransform = new ScaleTransform(1, 1);
                                GenerateNewProblem();
                            }
                        }, TimeSpan.FromMilliseconds(500));
                    }
                }
            }
            else
            {
                // Play wrong answer animation
                if (ProblemText != null)
                {
                    ProblemText.Foreground = new SolidColorBrush(Colors.Red);
                    ProblemText.RenderTransform = new TranslateTransform(10, 0);
                    
                    // Shake animation
                    DispatcherTimer.RunOnce(() =>
                    {
                        if (ProblemText != null)
                        {
                            ProblemText.RenderTransform = new TranslateTransform(-10, 0);
                        }
                    }, TimeSpan.FromMilliseconds(100));
                    
                    DispatcherTimer.RunOnce(() =>
                    {
                        if (ProblemText != null)
                        {
                            ProblemText.RenderTransform = new TranslateTransform(0, 0);
                            ProblemText.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }, TimeSpan.FromMilliseconds(200));
                }

                // Update stats for incorrect answer
                if (!problemStats.ContainsKey(problemKey))
                    problemStats[problemKey] = 0;
                problemStats[problemKey]--;

                var messageBox = MessageBoxManager.GetMessageBoxStandard("Feil svar", "Beklager, det var feil. Prøv igjen!", ButtonEnum.Ok);
                _ = messageBox.ShowAsync();
                if (AnswerInput != null)
                {
                    AnswerInput.Text = "";
                    AnswerInput.Focus();
                }
            }
        }
        else
        {
            // Show error animation
            if (AnswerInput != null)
            {
                AnswerInput.BorderBrush = new SolidColorBrush(Colors.Red);
                DispatcherTimer.RunOnce(() =>
                {
                    if (AnswerInput != null)
                    {
                        AnswerInput.BorderBrush = new SolidColorBrush(new Color(255, 76, 175, 80));
                    }
                }, TimeSpan.FromMilliseconds(500));
            }
        }
    }

    private void ShowVictory()
    {
        if (VictoryText != null)
        {
            VictoryText.IsVisible = true;
            VictoryText.Opacity = 1;
            VictoryText.RenderTransform = new ScaleTransform(1, 1);

            // Add confetti effect
            for (int i = 0; i < 50; i++)
            {
                AddFloatingNumber();
            }

            // Show message and close after animation
            DispatcherTimer.RunOnce(() =>
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Bra jobbet!", "Gratulerer! Du har løst nok oppgaver og kan bruke datamaskinen i 30 minutter!", ButtonEnum.Ok);
                _ = messageBox.ShowAsync();
                Close();
            }, TimeSpan.FromSeconds(3));
        }
    }

    private void StartTimer()
    {
        remainingTime = TimeSpan.FromMinutes(30);
        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
        if (TimeText != null)
            TimeText.Text = $"Tid igjen: {remainingTime.Minutes} min {remainingTime.Seconds} sek";

        if (remainingTime.TotalSeconds <= 0)
        {
            timer?.Stop();
            if (ProblemText != null)
            {
                ProblemText.Foreground = new SolidColorBrush(Colors.Red);
                ProblemText.Text = "Tiden er ute!";
            }
            DispatcherTimer.RunOnce(() => Close(), TimeSpan.FromSeconds(2));
        }
    }
}