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

namespace MarkovSuite
{
    /// <summary>
    /// Interaction logic for EditEntryWindow.xaml
    /// </summary>
    public partial class EditEntryWindow : Window
    {
        public Word Target { get; private set; }
        private Word tmp;

        public EditEntryWindow(Word target)
        {
            InitializeComponent();
            Target = target;
            tmp = new Word(null, target.Data, target.IsStarting, target.IsEnding) { Prevalence = target.Prevalence };
            DataContext = tmp;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Target.Data = tmp.Data;
            Target.IsStarting = tmp.IsStarting;
            Target.IsEnding = tmp.IsEnding;
            Target.Prevalence = tmp.Prevalence;

            DialogResult = true;
        }
        
    }
}
