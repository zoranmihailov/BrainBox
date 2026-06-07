using BrainBox.Models;
using BrainBox.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BrainBox.Views
{
    public partial class WordleWindow : Window
    {
        private PlayerProfile _profile;
        private List<string> _words;
        private string _secret;
        private string _currentGuess = "";
        private int _currentRow = 0;
        private int _maxRows = 6;
        private int _wordLength = 10;
        private int _score = 0;
        private int _timeLeft = 90;
        private DispatcherTimer _timer;
        private List<List<Border>> _grid = new();
        private Dictionary<string, Border> _keyBorders = new();

        private string[] _keyboardRows = new[]
        {
            "А Б В Г Д Ѓ Е Ж З Ѕ И",
            "Ј К Л Љ М Н Њ О П Р С",
            "Т Ќ У Ф Х Ц Ч Џ Ш"
        };

        private Dictionary<string, string> _latinToMk = new()
        {
            {"A","А"}, {"B","Б"}, {"V","В"}, {"G","Г"}, {"D","Д"},
            {"]","Ѓ"}, {"E","Е"}, {"\\","Ж"}, {"Z","З"}, {"Y","Ѕ"},
            {"I","И"}, {"J","Ј"}, {"K","К"}, {"L","Л"}, {"Q","Љ"},
            {"M","М"}, {"N","Н"}, {"W","Њ"}, {"O","О"}, {"P","П"},
            {"R","Р"}, {"S","С"}, {"T","Т"}, {"'","Ќ"}, {"U","У"},
            {"F","Ф"}, {"H","Х"}, {"C","Ц"}, {";","Ч"}, {"X","Џ"},
            {"[","Ш"}
        };

        public WordleWindow(PlayerProfile profile)
        {
            InitializeComponent();
            _profile = profile;
            LoadWords();
            BuildGrid();
            BuildKeyboard();
            StartTimer();

            this.Focusable = true;
            this.Focus();

            this.PreviewTextInput += (s, e) =>
            {
                var raw = e.Text.ToUpper();
                foreach (var ch in raw)
                {
                    string key = ch.ToString();
                    string letter = _latinToMk.ContainsKey(key) ? _latinToMk[key] : key;
                    if (_keyBorders.ContainsKey(letter))
                        TypeLetter(letter);
                }
                e.Handled = true;
            };
        }

        private void LoadWords()
        {
            _words = File.ReadAllLines("Data/words.txt")
                         .Select(w => w.Trim().ToUpper())
                         .Where(w => w.Length == 10)
                         .ToList();
            var rng = new Random();
            _secret = _words[rng.Next(_words.Count)];
        }

        private void BuildGrid()
        {
            for (int row = 0; row < _maxRows; row++)
            {
                var rowPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 3, 0, 3)
                };
                var rowBorders = new List<Border>();

                for (int col = 0; col < _wordLength; col++)
                {
                    var border = new Border
                    {
                        Width = 52,
                        Height = 52,
                        Margin = new Thickness(2),
                        CornerRadius = new CornerRadius(6),
                        Background = new SolidColorBrush(Color.FromRgb(35, 35, 55)),
                        Child = new TextBlock
                        {
                            FontSize = 20,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.White,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    };
                    rowBorders.Add(border);
                    rowPanel.Children.Add(border);
                }

                _grid.Add(rowBorders);
                gridPanel.Children.Add(rowPanel);
            }
        }

        private void BuildKeyboard()
        {
            foreach (var row in _keyboardRows)
            {
                var rowPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 3, 0, 3)
                };

                foreach (var letter in row.Split(' '))
                {
                    string l = letter.Trim();
                    if (string.IsNullOrEmpty(l)) continue;

                    var border = new Border
                    {
                        Width = 42,
                        Height = 42,
                        Margin = new Thickness(2),
                        CornerRadius = new CornerRadius(5),
                        Background = new SolidColorBrush(Color.FromRgb(70, 70, 100)),
                        Child = new TextBlock
                        {
                            Text = l,
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.White,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        Cursor = Cursors.Hand
                    };

                    string captured = l;
                    border.MouseLeftButtonUp += (s, e) =>
                    {
                        TypeLetter(captured);
                        this.Focus();
                    };

                    _keyBorders[l] = border;
                    rowPanel.Children.Add(border);
                }

                keyboardPanel.Children.Add(rowPanel);
            }
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
            txtTimer.Text = $"{_timeLeft}s";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (_currentGuess.Length > 0)
                    _currentGuess = _currentGuess[..^1];
                UpdateCurrentRow();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                SubmitGuess();
                e.Handled = true;
            }
        }

        private void TypeLetter(string letter)
        {
            if (_currentGuess.Length < _wordLength)
            {
                _currentGuess += letter;
                UpdateCurrentRow();
            }
        }

        private void UpdateCurrentRow()
        {
            var row = _grid[_currentRow];
            for (int i = 0; i < _wordLength; i++)
            {
                var tb = (TextBlock)row[i].Child;
                tb.Text = i < _currentGuess.Length ? _currentGuess[i].ToString() : "";
            }
        }

        private void SubmitGuess()
        {
            if (_currentGuess.Length < _wordLength)
            {
                MessageBox.Show("Внеси 10 букви!", "Wordle");
                return;
            }

            var row = _grid[_currentRow];
            var secretChars = _secret.ToCharArray();
            var guessChars = _currentGuess.ToCharArray();

            for (int i = 0; i < _wordLength; i++)
            {
                string letter = guessChars[i].ToString();
                Color bg;

                if (letter == secretChars[i].ToString())
                {
                    bg = Colors.Green;
                    UpdateKey(letter, Colors.Green);
                }
                else if (_secret.Contains(letter))
                {
                    bg = Colors.Goldenrod;
                    UpdateKey(letter, Colors.Goldenrod);
                }
                else
                {
                    bg = Color.FromRgb(80, 80, 80);
                    UpdateKey(letter, Colors.Red);
                }

                row[i].Background = new SolidColorBrush(bg);
            }

            if (_currentGuess == _secret)
            {
                _timer.Stop();
                _score = CalculateScore();
                EndGame(true);
                return;
            }

            _currentRow++;
            _currentGuess = "";

            if (_currentRow >= _maxRows)
            {
                _timer.Stop();
                EndGame(false);
            }
        }

        private void UpdateKey(string letter, Color color)
        {
            if (_keyBorders.ContainsKey(letter))
            {
                var current = ((SolidColorBrush)_keyBorders[letter].Background).Color;
                if (current == Colors.Green) return;
                if (current == Colors.Goldenrod && color != Colors.Green) return;
                _keyBorders[letter].Background = new SolidColorBrush(color);
            }
        }

        private int CalculateScore()
        {
            int attemptBonus = (_maxRows - _currentRow) * 2;
            int timeBonus = _timeLeft / 5;
            return Math.Min(20, 8 + attemptBonus + timeBonus);
        }

        private void EndGame(bool won)
        {
            string msg = won
                ? $"Точно! Зборот беше: {_secret}\nОсвоивте {_score} / 20 поени!"
                : $"Играта заврши! Зборот беше: {_secret}";

            _profile.Scores["Wordle"].UpdateScore(_score);
            ScoreManager.Save(_profile);

            MessageBox.Show(msg, "Wordle — Крај");
            Close();
        }
    }
}