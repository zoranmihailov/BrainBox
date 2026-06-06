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

namespace BrainBox.Views
{
    /// <summary>
    /// Interaction logic for MainMenuWindow.xaml
    /// </summary>
    public partial class MainMenuWindow : Window
    {
        private PlayerProfile _profile;

        public MainMenuWindow()
        {
            InitializeComponent();
            _profile = ScoreManager.Load();

            if (_profile.Name == "Player")
            {
                var name = Microsoft.VisualBasic.Interaction.InputBox(
                    "Внеси го твоето име:", "BrainBox", "Player");
                if (!string.IsNullOrWhiteSpace(name))
                    _profile.Name = name;
                ScoreManager.Save(_profile);
            }

            txtPlayerName.Text = $"Добредојде, {_profile.Name}!";
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            var wordle = new WordleWindow(_profile);
            wordle.ShowDialog();
            ScoreManager.Save(_profile);

            var math = new MathWindow(_profile);
            math.ShowDialog();
            ScoreManager.Save(_profile);

            var combo = new CombinationWindow(_profile);
            combo.ShowDialog();
            ScoreManager.Save(_profile);

            var quiz = new QuizWindow(_profile);
            quiz.ShowDialog();
            ScoreManager.Save(_profile);

            var assoc = new AssociationWindow(_profile);
            assoc.ShowDialog();
            ScoreManager.Save(_profile);

            MessageBox.Show("Сесијата е завршена! Провери ја статистиката.", "BrainBox");
        }

        private void OpenStats(object sender, RoutedEventArgs e)
        {
            new StatsWindow(_profile).ShowDialog();
        }
    }
}
