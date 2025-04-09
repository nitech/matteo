using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace Matteo
{
    public partial class MainWindow : Window
    {
        private readonly Random random = new Random();
        private Dictionary<string, int> problemStats = new Dictionary<string, int>();
        private readonly string statsFilePath = "problem_stats.json";
        private int currentScore = 0;
        private int problemsSolved = 0;
        private int firstNumber;
        private int secondNumber;
        private DispatcherTimer? timer;
        private TimeSpan remainingTime;
        private DispatcherTimer? backgroundTimer;
        private readonly List<TextBlock> backgroundElements = new List<TextBlock>();

        public MainWindow()
        {
            InitializeComponent();
            LoadStats();
            GenerateNewProblem();
            StartTimer();
            InitializeBackground();
        }

        private void InitializeBackground()
        {
            // Create initial background elements
            for (int i = 0; i < 20; i++)
            {
                AddBackgroundElement();
            }

            // Start background animation timer
            backgroundTimer = new DispatcherTimer();
            backgroundTimer.Interval = TimeSpan.FromMilliseconds(50);
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
                RenderTransform = new TranslateTransform
                {
                    X = random.Next((int)BackgroundCanvas.ActualWidth),
                    Y = random.Next((int)BackgroundCanvas.ActualHeight)
                }
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
                    transform.Y = BackgroundCanvas.ActualHeight + 50;
                    transform.X = random.Next((int)BackgroundCanvas.ActualWidth);
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

            ProblemText.Text = $"{firstNumber} × {secondNumber} = ?";
            AnswerInput.Text = "";
            AnswerInput.Focus();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AnswerInput.Text, out int answer))
            {
                string problemKey = $"{firstNumber}x{secondNumber}";
                int correctAnswer = firstNumber * secondNumber;

                if (answer == correctAnswer)
                {
                    // Play correct answer animation
                    var storyboard = (Storyboard)FindResource("CorrectAnswerAnimation");
                    storyboard.Begin();

                    currentScore += 10;
                    problemsSolved++;
                    ProgressBar.Value = problemsSolved;
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
                    var storyboard = (Storyboard)FindResource("WrongAnswerAnimation");
                    storyboard.Begin();

                    // Update stats for incorrect answer
                    if (!problemStats.ContainsKey(problemKey))
                        problemStats[problemKey] = 0;
                    problemStats[problemKey]--;

                    MessageBox.Show("Beklager, det var feil. Prøv igjen!", "Feil svar");
                    AnswerInput.Text = "";
                    AnswerInput.Focus();
                }
            }
            else
            {
                MessageBox.Show("Vennligst skriv inn et gyldig tall!", "Ugyldig input");
            }
        }

        private void ShowVictory()
        {
            var storyboard = (Storyboard)FindResource("VictoryAnimation");
            storyboard.Completed += (s, e) =>
            {
                MessageBox.Show("Gratulerer! Du har løst nok oppgaver og kan bruke datamaskinen i 30 minutter!", "Bra jobbet!");
                Application.Current.Shutdown();
            };
            storyboard.Begin();
        }

        private void StartTimer()
        {
            remainingTime = TimeSpan.FromMinutes(30);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
            TimeText.Text = $"Tid igjen: {remainingTime.Minutes} min {remainingTime.Seconds} sek";

            if (remainingTime.TotalSeconds <= 0)
            {
                timer?.Stop();
                MessageBox.Show("Tiden er ute! Du må løse flere oppgaver for å fortsette å bruke datamaskinen.", "Tid er ute");
                Application.Current.Shutdown();
            }
        }
    }
} 