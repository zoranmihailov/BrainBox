using BrainBox.Models;
using BrainBox.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for CombinationWindow.xaml
    /// </summary>
    public partial class CombinationWindow : Window
    {
        private PlayerProfile _profile;
        private List<string> _symbols = new() { "★", "♥", "♠", "♦", "♣", "●" };
        private List<string> _secret = new();
        private List<string> _currentGuess = new();
        private int _attemptsLeft = 6;
        private int _score = 0;
        private DispatcherTimer _timer;
        private int _timeLeft = 60;

        public CombinationWindow(PlayerProfile profile)
        {
            InitializeComponent();
            _profile = profile;
            GenerateSecret();
            BuildSymbolButtons();
            UpdateCurrentGuessUI();
            UpdateUI();
            StartTimer();
        }

        private void GenerateSecret()
        {
            var rng = new Random();
            _secret = _symbols.OrderBy(_ => rng.Next()).Take(4).ToList();
        }

        private void BuildSymbolButtons()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int i = 0; i < _symbols.Count; i++)
            {
                int row = i / 3;
                int col = i % 3;

                var btn = new Button
                {
                    Width = 55,
                    Height = 55,
                    Margin = new Thickness(2),
                    Background = new SolidColorBrush(Color.FromRgb(45, 45, 68)),
                    Foreground = Brushes.White,
                    Tag = _symbols[i],
                    Padding = new Thickness(0)
                };
                btn.Template = MakeButtonTemplate();
                btn.Click += SymbolClick;

                var container = new Grid
                {
                    Width = 55,
                    Height = 55
                };

                var number = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.LightGray),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(3, 2, 0, 0)
                };

                var symbol = new TextBlock
                {
                    Text = _symbols[i],
                    FontSize = 20,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                container.Children.Add(symbol);
                container.Children.Add(number);
                btn.Content = container;

                Grid.SetRow(btn, row);
                Grid.SetColumn(btn, col);
                grid.Children.Add(btn);
            }

            symbolPanel.Children.Add(grid);
        }

        private void SymbolClick(object sender, RoutedEventArgs e)
        {
            if (_currentGuess.Count >= 4) return;
            var sym = ((Button)sender).Tag.ToString();
            _currentGuess.Add(sym);
            UpdateCurrentGuessUI();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            int index = e.Key switch
            {
                Key.D1 => 0,
                Key.D2 => 1,
                Key.D3 => 2,
                Key.D4 => 3,
                Key.D5 => 4,
                Key.D6 => 5,
                _ => -1
            };

            if (index >= 0 && _currentGuess.Count < 4)
            {
                _currentGuess.Add(_symbols[index]);
                UpdateCurrentGuessUI();
            }
            else if (e.Key == Key.Enter)
            {
                ConfirmGuess(null, null);
            }
            else if (e.Key == Key.Back)
            {
                if (_currentGuess.Count > 0)
                {
                    _currentGuess.RemoveAt(_currentGuess.Count - 1);
                    UpdateCurrentGuessUI();
                }
            }
            else if (e.Key == Key.Delete)
            {
                _currentGuess.Clear();
                UpdateCurrentGuessUI();
            }
        }

        private void RemoveLast(object sender, RoutedEventArgs e)
        {
            if (_currentGuess.Count > 0)
            {
                _currentGuess.RemoveAt(_currentGuess.Count - 1);
                UpdateCurrentGuessUI();
            }
        }

        private void UpdateCurrentGuessUI()
        {
            currentGuess.Children.Clear();
            for (int i = 0; i < 4; i++)
            {
                var border = new Border
                {
                    Width = 44,
                    Height = 44,
                    Margin = new Thickness(2),
                    CornerRadius = new CornerRadius(8),
                    Background = i < _currentGuess.Count
                        ? new SolidColorBrush(Color.FromRgb(70, 70, 100))
                        : new SolidColorBrush(Color.FromRgb(35, 35, 55)),
                    Child = new TextBlock
                    {
                        Text = i < _currentGuess.Count ? _currentGuess[i] : "",
                        FontSize = 20,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                currentGuess.Children.Add(border);
            }
        }

        private void ConfirmGuess(object sender, RoutedEventArgs e)
        {
            if (_currentGuess.Count < 4)
            {
                MessageBox.Show("Избери 4 симболи!", "BrainBox");
                return;
            }

            AddAttemptRow();
            _attemptsLeft--;

            bool isCorrect = _currentGuess.SequenceEqual(_secret);

            if (isCorrect)
            {
                _timer.Stop();
                _score = CalculateScore();
                EndGame(true);
                return;
            }

            if (_attemptsLeft <= 0)
            {
                _timer.Stop();
                EndGame(false);
                return;
            }

            _currentGuess.Clear();
            UpdateCurrentGuessUI();
            UpdateUI();
        }

        private void AddAttemptRow()
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };

            for (int i = 0; i < 4; i++)
            {
                string sym = _currentGuess[i];
                Color bg;

                if (sym == _secret[i])
                    bg = Colors.Green;
                else if (_secret.Contains(sym))
                    bg = Colors.Goldenrod;
                else
                    bg = Colors.Red;

                var border = new Border
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(3),
                    CornerRadius = new CornerRadius(8),
                    Background = new SolidColorBrush(bg),
                    Child = new TextBlock
                    {
                        Text = sym,
                        FontSize = 22,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                panel.Children.Add(border);
            }

            attemptsPanel.Children.Add(panel);
        }

        private int CalculateScore()
        {
            int attemptBonus = _attemptsLeft * 2;
            int timeBonus = _timeLeft / 5;
            return Math.Min(20, 10 + attemptBonus + timeBonus);
        }

        private void ClearGuess(object sender, RoutedEventArgs e)
        {
            _currentGuess.Clear();
            UpdateCurrentGuessUI();
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                _timeLeft--;
                txtTimer.Text = $"{_timeLeft}s";
                if (_timeLeft <= 0)
                {
                    _timer.Stop();
                    EndGame(false);
                }
            };
            _timer.Start();
        }

        private void UpdateUI()
        {
            txtTimer.Text = $"{_timeLeft}s";
            txtAttempts.Text = $"Обиди: {_attemptsLeft} / 6";
            txtScore.Text = $"Поени: {_score}";
        }

        private void EndGame(bool won)
        {
            string msg = won
                ? $"Точно! Освоивте {_score} / 20 поени!"
                : $"Играта заврши! Тајната комбинација беше: {string.Join(" ", _secret)}";

            _profile.Scores["Kombinacija"].UpdateScore(_score);
            ScoreManager.Save(_profile);

            MessageBox.Show(msg, "Комбинација — Крај");
            Close();
        }

        private ControlTemplate MakeButtonTemplate()
        {
            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetBinding(Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            var content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(content);
            template.VisualTree = border;
            return template;
        }
    }
}
