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

namespace MarkovSuite.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Settings tmp;

        public SettingsWindow()
        {
            InitializeComponent();

            tmp = new Settings();
            tmp.RowbreakChars = Settings.Instance.RowbreakChars;
            tmp.SavePath = Settings.Instance.SavePath;
            tmp.StripChars = Settings.Instance.StripChars;
            tmp.TerminationChars = Settings.Instance.TerminationChars;

            Console.WriteLine("> " + tmp.SavePath);

            DataContext = tmp;
        }

        private void DefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            tmp = Settings.Defaults;
            DataContext = tmp;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance = tmp;
            DialogResult = true;
        }
    }
}
