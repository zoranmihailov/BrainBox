using BrainBox.Models;
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
    /// Interaction logic for AssociationWindow.xaml
    /// </summary>
    public partial class AssociationWindow : Window
    {
        private PlayerProfile _profile;
        public AssociationWindow(PlayerProfile profile)
        {
            InitializeComponent();
            _profile = profile;
        }
    }
}
