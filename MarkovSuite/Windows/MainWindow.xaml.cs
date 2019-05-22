﻿using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MarkovSuite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string m_appSavePath;
        public static string AppSavePath
        {
            get
            {
                if(m_appSavePath == null || m_appSavePath == "")
                    m_appSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MarkovSuite\\";

                System.IO.Directory.CreateDirectory(m_appSavePath);
                return m_appSavePath;
            }
        }

        private MarkovData Context;

        public MainWindow()
        {
            InitializeComponent();
            InitContext(false);
            RowbreakCheckBox.Click += RowbreakCheckBox_Click;
        }

        private void RowbreakCheckBox_Click(object sender, RoutedEventArgs e)
        {
            updateContextValues();
        }

        private void InitContext (bool clear = true)
        {
            if(clear) LearnTextBox.Text = OutputTextBox.Text =  "";
            Context = new MarkovData();
            DataContext = Context;
            updateContextValues();

            Context.StatusString = "Initialized";
            Context.HasChanged = false; // ugly fix for when haschanged = true on new context
        }

        /// <summary>
        /// Explicitly updates window objects based on context data
        /// </summary>
        private void updateContextValues ()
        {
            switch (RowbreakCheckBox.IsChecked)
            {
                case false:
                    OutputTextBox.TextWrapping = TextWrapping.NoWrap;
                    break;
                case true:
                    OutputTextBox.TextWrapping = TextWrapping.WrapWithOverflow;
                    break;
            }
        }

        private MessageBoxResult AskForSave()
        {
            if (!Context.HasChanged)
                return MessageBoxResult.No;

            string msg = "Changes have been made. Do you want to save them?";
            MessageBoxButton button = MessageBoxButton.YesNoCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;

            return MessageBox.Show(msg, "Save changes?", button, icon);
        }

        private void LearnButton_Click(object sender, RoutedEventArgs e)
        {
            Markov.Train(Context, LearnTextBox.Text);
            if (Context.AutoClear)
                LearnTextBox.Text = "";
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string output = "";
            for(int i = 0; i < NumSentences.Value; i++)
            {
                output += Markov.Generate(Context).FirstCharToUpper() + " ";
            }
            output += "\r\n\r\n";
            OutputTextBox.Text += output;

            Context.StatusString = "Generated " + NumSentences.Value + " sentences";
        }

        private void RootListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(RootListBox.SelectedItem != null)
            {
                ChildListBox.ItemsSource = (RootListBox.SelectedItem as Word).Children;
            }
        }

        private void CommonCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Context == null)
                e.CanExecute = false;
            else
                e.CanExecute = Context.HasChanged;
        }

        private void CommonEntryCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (RootListBox == null) return;
            e.CanExecute = RootListBox.SelectedItem != null;
        }

        private void DeleteChildCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ChildListBox == null || RootListBox == null) return;
            e.CanExecute = ChildListBox.SelectedItem != null && RootListBox.SelectedItem != null;
        }

        private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult result = AskForSave();
            switch(result)
            {
                case MessageBoxResult.Cancel:
                    Context.StatusString = "Canceled by user";
                    return;
                case MessageBoxResult.No:
                    InitContext();
                    return;
                case MessageBoxResult.Yes:
                    SaveCommandBinding_Executed(sender, e);
                    InitContext();
                    return;
            }
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Context.StatusString = "Opening file...";
            MessageBoxResult sresult = AskForSave();
            switch (sresult)
            {
                case MessageBoxResult.Cancel:
                    Context.StatusString = "Canceled by user";
                    return;
                case MessageBoxResult.Yes:
                    SaveCommandBinding_Executed(sender, e);
                    break;
                case MessageBoxResult.No:
                    break;
            }

            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Markov Suite Chain|*.chain";
            openDialog.Title = "Open chain";
            openDialog.InitialDirectory = AppSavePath;

            bool? result = openDialog.ShowDialog();
            if (result == true && openDialog.FileName != "")
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(openDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                MarkovData data = (MarkovData)formatter.Deserialize(stream);
                stream.Close();

                Context = data;
                DataContext = Context;

                Context.StatusString = "Opened file " + Context.FilePath;
            }
        }

        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Context.FilePath = "";
            SaveCommandBinding_Executed(sender, e);
        }

        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Context.StatusString = "Saving file...";
            bool? result;

            if (Context.FilePath != "" || Context.FilePath == null)     // context already has a file path
            {
                result = true;
            }
            else
            {                                                           // otherwise ask user for a file path
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.FileName = Context.ChainName;
                saveDialog.Filter = "Markov Suite Chain|*.chain";
                saveDialog.Title = "Save chain";
                saveDialog.InitialDirectory = AppSavePath;

                result = saveDialog.ShowDialog();
                if(result == true) Context.FilePath = saveDialog.FileName;
            }

            if (result == true)
            {
                Context.StatusString = "Saving file...";

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(Context.FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                Context.HasChanged = false;
                formatter.Serialize(stream, Context);
                stream.Close();

                Context.StatusString = "Saved file to " + Context.FilePath;
            }
        }

        private void EditEntryCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditEntryWindow dialog = new EditEntryWindow((Word)RootListBox.SelectedItem);
            dialog.ShowDialog();
        }

        private void DeleteEntryCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure? Deleting this entry will also delete all of its children.", "Delete entry?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Cancel) return;

            ChildListBox.ItemsSource = null;
            Context.Words.Remove((Word)RootListBox.SelectedItem);
        }

        private void DeleteChildCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult result;
            ChildWord target = (ChildWord)ChildListBox.SelectedItem;

            result = MessageBox.Show("Are you sure you want to delete this child?", "Delete child?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            result = MessageBox.Show("Do you want to delete the associated root word?\r\n(" + target.StrippedData + ")", "Delete root word?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            Word root = (Word)RootListBox.SelectedItem;
            root.Children.Remove(target);

            if(result == MessageBoxResult.Yes)
            {
                try
                {
                    root = Context.Words.Single(x => x.Data == target.StrippedData);
                    Context.Words.Remove(root);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error:\r\n" + ex.Message, "Error happens", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class TitleStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)values[0] + ((bool)values[1] ? "*" : "") + " | MarkovSuite";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
