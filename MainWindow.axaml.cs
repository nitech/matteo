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

    public MainWindow()
    {
        InitializeComponent();
        LoadStats();
        GenerateNewProblem();
        StartTimer();
        InitializeBackground();

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

    private void AddBackgroundElement()
    {
        var element = new TextBlock
        {
            Text = random.Next(2) == 0 ? "×" : "+",
            FontSize = random.Next(20, 40),
            Foreground = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
            RenderTransform = new TranslateTransform(
                random.Next((int)BackgroundCanvas.Bounds.Width),
                random.Next((int)BackgroundCanvas.Bounds.Height)
            )
        };

        BackgroundCanvas.Children.Add(element);
        backgroundElements.Add(element);
    }

    private void BackgroundTimer_Tick(object? sender, EventArgs e)
    {
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

        if (ProblemText != null)
        {
            ProblemText.Text = $"{firstNumber} × {secondNumber} = ?";
            if (AnswerInput != null)
            {
                AnswerInput.Text = "";
                AnswerInput.Focus();
            }
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
                ProblemText.Foreground = new SolidColorBrush(Colors.Lime);
                ProblemText.RenderTransform = new ScaleTransform(1.2, 1.2);

                currentScore += 10;
                problemsSolved++;
                
                if (ProgressBar != null)
                    ProgressBar.Value = problemsSolved;
                if (ScoreText != null)
                    ScoreText.Text = $"Poeng: {currentScore}";

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
                    GenerateNewProblem();
                }
            }
            else
            {
                // Play wrong answer animation
                ProblemText.Foreground = new SolidColorBrush(Colors.Red);
                ProblemText.RenderTransform = new TranslateTransform(10, 0);

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
            var messageBox = MessageBoxManager.GetMessageBoxStandard("Ugyldig input", "Vennligst skriv inn et gyldig tall!", ButtonEnum.Ok);
            _ = messageBox.ShowAsync();
        }
    }

    private void ShowVictory()
    {
        if (VictoryText != null)
        {
            VictoryText.IsVisible = true;
            VictoryText.Opacity = 1;
            VictoryText.RenderTransform = new ScaleTransform(1, 1);

            // Show message and close after animation
            DispatcherTimer.RunOnce(() =>
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Bra jobbet!", "Gratulerer! Du har løst nok oppgaver og kan bruke datamaskinen i 30 minutter!", ButtonEnum.Ok);
                _ = messageBox.ShowAsync();
                Close();
            }, TimeSpan.FromSeconds(1));
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
            var messageBox = MessageBoxManager.GetMessageBoxStandard("Tid er ute", "Tiden er ute! Du må løse flere oppgaver for å fortsette å bruke datamaskinen.", ButtonEnum.Ok);
            _ = messageBox.ShowAsync();
            Close();
        }
    }
}