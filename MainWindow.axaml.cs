using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;

namespace Matteo.Views;

public partial class MainWindow : Window
{
    private readonly Random random = new Random();
    private Dictionary<string, int> problemStats = new Dictionary<string, int>();
    private readonly string statsFilePath = "problem_stats.json";
    private int currentScore = 0;
    private int problemsSolved = 0;
    private int firstNumber;
    private int secondNumber;
    private readonly List<TextBlock> backgroundElements = new();
    private readonly List<TextBlock> floatingNumbers = new();
    private readonly List<Image> enemies = new();
    private readonly List<string> enemyTypes = new() { "+", "-", "×", "÷" };
    private Bitmap? enemiesSprite;

    public MainWindow()
    {
        InitializeComponent();
        LoadSprites();
        LoadStats();
        GenerateNewProblem();
        InitializeBackground();
        InitializeFloatingNumbers();

        // Prevent window from closing until all problems are solved
        Closing += Window_Closing;

        // Handle key press for Enter key
        AnswerInput.KeyDown += (s, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                CheckAnswer();
            }
        };
    }

    private void LoadSprites()
    {
        try
        {
            enemiesSprite = new Bitmap("Assets/fiender.png");
        }
        catch (Exception)
        {
            // Handle sprite loading error
        }
    }

    private void InitializeBackground()
    {
        // Create initial background elements
        for (int i = 0; i < 30; i++)
        {
            AddBackgroundElement();
        }

        // Start background animation timer
        var backgroundTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };

        backgroundTimer.Tick += BackgroundTimer_Tick;
        backgroundTimer.Start();
    }

    private void BackgroundTimer_Tick(object? sender, EventArgs e)
    {
        var elementsToRemove = new List<TextBlock>();
        foreach (var element in BackgroundCanvas.Children.OfType<TextBlock>().ToList())
        {
            var currentTop = Canvas.GetTop(element);
            var currentLeft = Canvas.GetLeft(element);
            
            // Random horizontal movement
            var horizontalMove = new Random().Next(-1, 2);  // -1, 0, or 1
            Canvas.SetLeft(element, currentLeft + horizontalMove);
            
            // Upward movement with slight variation
            Canvas.SetTop(element, currentTop - new Random().Next(1, 3));

            // Remove if out of bounds
            if (currentTop < -100 || currentLeft < -100 || currentLeft > Bounds.Width + 100)
            {
                elementsToRemove.Add(element);
            }
        }

        foreach (var element in elementsToRemove)
        {
            BackgroundCanvas.Children.Remove(element);
            AddBackgroundElement();
        }

        // Occasionally add new elements to maintain density
        if (new Random().Next(100) < 10)  // 10% chance each tick
        {
            AddBackgroundElement();
        }
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
        var number = random.Next(1, 13);
        var element = new TextBlock
        {
            Text = number.ToString(),
            FontSize = random.Next(30, 80),
            Foreground = new SolidColorBrush(Color.FromArgb(
                (byte)random.Next(20, 51),
                255, 255, 255)),
            RenderTransform = new TranslateTransform
            {
                X = random.Next((int)BackgroundCanvas.Bounds.Width),
                Y = BackgroundCanvas.Bounds.Height + 100
            }
        };

        BackgroundCanvas.Children.Add(element);
        floatingNumbers.Add(element);
    }

    private void AddBackgroundElement()
    {
        var random = new Random();
        var symbols = new[] { "×", "+", "−", "÷", "=", "%", "π", "∑", "√" };
        var symbol = symbols[random.Next(symbols.Length)];
        
        // Generate pastel colors
        byte baseAlpha = (byte)random.Next(30, 70);
        var colors = new[]
        {
            Color.FromArgb(baseAlpha, 255, 182, 193),    // Pastel pink
            Color.FromArgb(baseAlpha, 176, 224, 230),    // Powder blue
            Color.FromArgb(baseAlpha, 144, 238, 144),    // Light green
            Color.FromArgb(baseAlpha, 230, 230, 250),    // Lavender
            Color.FromArgb(baseAlpha, 255, 218, 185),    // Peach
            Color.FromArgb(baseAlpha, 175, 238, 238)     // Pale turquoise
        };
        
        var textBlock = new TextBlock
        {
            Text = symbol,
            FontSize = random.Next(40, 120),
            Foreground = new SolidColorBrush(colors[random.Next(colors.Length)]),
            Opacity = random.Next(40, 80) / 100.0,
            // Position anywhere on the screen
            Margin = new Thickness(
                random.Next(0, (int)Bounds.Width),
                random.Next(0, (int)Bounds.Height),
                0, 0)
        };

        Canvas.SetLeft(textBlock, random.Next(0, (int)Bounds.Width));
        Canvas.SetTop(textBlock, random.Next(0, (int)Bounds.Height));
        BackgroundCanvas.Children.Add(textBlock);
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
                    
                    ShowCorrectAnswerEffects();

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
                ShowIncorrectAnswerEffects();

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

    private void ShowCorrectAnswerEffects()
    {
        // Add celebration effects
        for (int i = 0; i < 5; i++)
        {
            AddEnemy(enemyTypes[random.Next(enemyTypes.Count)]);
        }

        // Show motivational message
        Motivator?.ShowCorrectAnswer();
    }

    private void ShowIncorrectAnswerEffects()
    {
        // Show encouraging message
        Motivator?.ShowIncorrectAnswer();
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
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    "Gratulerer!", 
                    "Du har løst alle oppgavene! Nå kan du bruke datamaskinen i 30 minutter som belønning. Bra jobbet!",
                    ButtonEnum.Ok);
                _ = messageBox.ShowAsync();
                Close();
            }, TimeSpan.FromSeconds(3));
        }
    }

    private void AddEnemy(string type)
    {
        if (enemiesSprite == null) return;

        var enemy = new Image
        {
            Source = enemiesSprite,
            Width = 32,
            Height = 32
        };

        var startX = random.Next((int)GameCanvas.Bounds.Width);
        Canvas.SetLeft(enemy, startX);
        Canvas.SetTop(enemy, -50);

        GameCanvas.Children.Add(enemy);
        enemies.Add(enemy);

        // Animate enemy
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };

        var angle = 0.0;
        timer.Tick += (s, e) =>
        {
            var top = Canvas.GetTop(enemy);
            var left = Canvas.GetLeft(enemy);

            // Sinusoidal movement
            angle += 0.1;
            left += Math.Sin(angle) * 2;
            top += 2;

            Canvas.SetLeft(enemy, left);
            Canvas.SetTop(enemy, top);

            if (top > GameCanvas.Bounds.Height + 50)
            {
                GameCanvas.Children.Remove(enemy);
                enemies.Remove(enemy);
                timer.Stop();
            }
        };
        timer.Start();
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (problemsSolved < 10)
        {
            e.Cancel = true;
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                "Ikke ferdig ennå!", 
                "Du må løse alle oppgavene før du kan avslutte programmet.",
                ButtonEnum.Ok);
            _ = messageBox.ShowAsync();
        }
    }
}