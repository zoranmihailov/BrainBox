using BrainBox.Models;
using BrainBox.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BrainBox.Views
{
    /// <summary>
    /// Interaction logic for QuizWindow.xaml
    /// </summary>
    public partial class QuizWindow : Window
    {
        private PlayerProfile _profile;
        private List<QuizQuestion> _questions;
        private QuizQuestion _currentQuestion;
        private int _currentIndex = 0;
        private int _score = 0;
        private DispatcherTimer _timer;
        private int _timeLeft = 11;

        public QuizWindow(PlayerProfile profile)
        {
            InitializeComponent();
            _profile = profile;
            LoadQuestions();
            StartTimer();
            ShowQuestion();
        }

        private void LoadQuestions()
        {
            var json = File.ReadAllText("Data/quiz.json");
            var data = JsonSerializer.Deserialize<QuizData>(json);
            var rng = new Random();
            _questions = data.questions.OrderBy(_ => rng.Next()).Take(10).ToList();
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += TimerTick;
            _timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _timeLeft--;
            txtTimer.Text = $"{_timeLeft}s";

            if (_timeLeft <= 0)
            {
                _timer.Stop();
                NextQuestion();
            }
        }

        private void ShowQuestion()
        {
            if (_currentIndex >= _questions.Count)
            {
                EndGame();
                return;
            }

            _currentQuestion = _questions[_currentIndex];
            _timeLeft = 11;
            txtTimer.Text = "11s";

            txtQuestionNum.Text = $"Прашање {_currentIndex + 1} / 10";
            txtCategory.Text = _currentQuestion.category;
            txtQuestion.Text = _currentQuestion.question;
            txtScore.Text = $"Поени: {_score}";

            var buttons = new[] { btnA, btnB, btnC, btnD };
            for (int i = 0; i < 4; i++)
            {
                buttons[i].Content = _currentQuestion.options[i];
                buttons[i].Background = new SolidColorBrush(Color.FromRgb(45, 45, 68));
                buttons[i].IsEnabled = true;
            }
        }

        private void AnswerClick(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            var btn = (Button)sender;
            string selected = btn.Content.ToString();

            if (selected == _currentQuestion.answer)
            {
                btn.Background = new SolidColorBrush(Colors.Green);
                _score += 5;
            }
            else
            {
                btn.Background = new SolidColorBrush(Colors.Red);
                _score -= 5;
                if (_score < 0) _score = 0;

                // Pokazi go tocniot odgovor
                var buttons = new[] { btnA, btnB, btnC, btnD };
                foreach (var b in buttons)
                    if (b.Content.ToString() == _currentQuestion.answer)
                        b.Background = new SolidColorBrush(Colors.Green);
            }

            // Onevozmozi gi site buttons
            btnA.IsEnabled = false;
            btnB.IsEnabled = false;
            btnC.IsEnabled = false;
            btnD.IsEnabled = false;

            txtScore.Text = $"Поени: {_score}";

            // Pocekaj 1 sekunda pa odi na sledno prasanje
            var delay = new DispatcherTimer();
            delay.Interval = TimeSpan.FromSeconds(1);
            delay.Tick += (s, ev) =>
            {
                delay.Stop();
                _currentIndex++;
                _timer.Start();
                ShowQuestion();
            };
            delay.Start();
        }

        private void NextQuestion()
        {
            _currentIndex++;
            ShowQuestion();
        }

        private void EndGame()
        {
            _profile.Scores["Prasanja"].UpdateScore(_score);
            ScoreManager.Save(_profile);

            MessageBox.Show($"Играта заврши!\nОсвоени поени: {_score} / 50",
                "Прашања — Крај", MessageBoxButton.OK);
            Close();
        }
    }

    // Klasi za deserijalizacija na JSON
    public class QuizQuestion
    {
        public int id { get; set; }
        public string category { get; set; }
        public string question { get; set; }
        public List<string> options { get; set; }
        public string answer { get; set; }
    }

    public class QuizData
    {
        public List<QuizQuestion> questions { get; set; }
    }
}

