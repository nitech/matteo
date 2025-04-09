using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Matteo.Controls
{
    public partial class MotivatorPig : UserControl
    {
        private readonly Random random = new Random();
        private readonly DispatcherTimer speechTimer;
        private readonly List<string> correctAnswerMessages = new List<string>
        {
            "Supert! Du er kjempeflink! 🌟",
            "Wow! Du er en mattegeni! 🎓",
            "Fantastisk! Fortsett slik! 💪",
            "Du er utrolig flink! 🏆",
            "Helt riktig! Du er en stjerne! ⭐",
            "Kjempebra! Du er super! 🎉"
        };

        private readonly List<string> incorrectAnswerMessages = new List<string>
        {
            "Nesten! Prøv en gang til! 💪",
            "Du er på rett vei! Fortsett å prøve! 🌱",
            "Ikke gi opp! Du klarer det! 🌟",
            "Det var nære på! Prøv igjen! 🎯",
            "Øvelse gjør mester! Du får det til! 🎓"
        };

        private readonly List<string> encouragementMessages = new List<string>
        {
            "Du er flink til å jobbe! 📚",
            "Jeg heier på deg! 🎉",
            "Du blir bedre og bedre! 📈",
            "Dette går kjempebra! 🌟",
            "Fortsett det gode arbeidet! 💪"
        };

        public MotivatorPig()
        {
            InitializeComponent();
            LoadPigImage();

            speechTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
            };
            speechTimer.Tick += (s, e) => HideSpeechBubble();

            // Start random encouragement timer
            var encouragementTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            encouragementTimer.Tick += (s, e) => ShowRandomEncouragement();
            encouragementTimer.Start();

            // Add bounce animation for the pig
            var bounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(2000)
            };
            bounceTimer.Tick += (s, e) => AnimatePig();
            bounceTimer.Start();
        }

        private void LoadPigImage()
        {
            try
            {
                var bitmap = new Bitmap("Assets/pig.png");
                PigImage.Source = bitmap;
            }
            catch (Exception)
            {
                // Handle image loading error
            }
        }

        private void AnimatePig()
        {
            var transform = (ScaleTransform)PigImage.RenderTransform;
            
            // Bounce animation
            DispatcherTimer.RunOnce(() =>
            {
                transform.ScaleY = 0.9;
                transform.ScaleX = 1.1;
            }, TimeSpan.FromMilliseconds(0));

            DispatcherTimer.RunOnce(() =>
            {
                transform.ScaleY = 1.1;
                transform.ScaleX = 0.9;
            }, TimeSpan.FromMilliseconds(200));

            DispatcherTimer.RunOnce(() =>
            {
                transform.ScaleY = 1.0;
                transform.ScaleX = 1.0;
            }, TimeSpan.FromMilliseconds(400));
        }

        public void ShowCorrectAnswer()
        {
            var message = correctAnswerMessages[random.Next(correctAnswerMessages.Count)];
            ShowSpeechBubble(message);
            AnimatePig();
        }

        public void ShowIncorrectAnswer()
        {
            var message = incorrectAnswerMessages[random.Next(incorrectAnswerMessages.Count)];
            ShowSpeechBubble(message);
            AnimatePig();
        }

        private void ShowRandomEncouragement()
        {
            if (!SpeechBubble.IsVisible)
            {
                var message = encouragementMessages[random.Next(encouragementMessages.Count)];
                ShowSpeechBubble(message);
                AnimatePig();
            }
        }

        private void ShowSpeechBubble(string message)
        {
            SpeechText.Text = message;
            SpeechBubble.IsVisible = true;
            
            // Animate the speech bubble
            SpeechBubble.Opacity = 1;
            var transform = (ScaleTransform)SpeechBubble.RenderTransform;
            transform.ScaleX = 1;
            transform.ScaleY = 1;
            
            speechTimer.Stop();
            speechTimer.Start();
        }

        private void HideSpeechBubble()
        {
            // Animate out
            SpeechBubble.Opacity = 0;
            var transform = (ScaleTransform)SpeechBubble.RenderTransform;
            transform.ScaleX = 0.8;
            transform.ScaleY = 0.8;

            // Hide after animation
            DispatcherTimer.RunOnce(() =>
            {
                SpeechBubble.IsVisible = false;
            }, TimeSpan.FromMilliseconds(300));
            
            speechTimer.Stop();
        }
    }
} 