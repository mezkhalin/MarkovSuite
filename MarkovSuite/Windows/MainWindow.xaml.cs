using MarkovSuite.TreeViewFileExplorer;
using MarkovSuite.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

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
            InitSettings();
            InitContext(false);
            InitializeFileSystemObjects();
            RowbreakCheckBox.Click += RowbreakCheckBox_Click;
        }

        #region Settings

        private void InitSettings ()
        {
            if (!File.Exists(Settings.SettingsPath))    // no settings file found, create default
            {
                Settings.Instance = Settings.Defaults;
                SaveSettings();
            }
            else
            {       // file found, load the values
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                serializer.UnknownAttribute += Serializer_UnknownAttribute;
                serializer.UnknownNode += Serializer_UnknownNode;

                FileStream fs = new FileStream(Settings.SettingsPath, FileMode.Open);
                Settings.Instance = (Settings)serializer.Deserialize(fs);
                fs.Close();
            }
        }

        private void Serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown node >> " + e.Name + "\t" + e.Text); ;
        }

        private void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute >> " +
            attr.Name + " = '" + attr.Value + "'");
        }

        private void SaveSettings ()
        {
            if (Settings.Instance == null) return;

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            TextWriter writer = new StreamWriter(Settings.SettingsPath);
            serializer.Serialize(writer, Settings.Instance);
            writer.Close();
        }

        #endregion

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

        #region TreeView Control

        /// <summary>
        /// init for the treeview file explorer control
        /// </summary>
        private void InitializeFileSystemObjects()
        {
            treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
            BatchListBox.SelectionChanged += BatchListBox_SelectedItemChanged;
            Context.BatchFiles.CollectionChanged += BatchFiles_CollectionChanged;

            var drives = DriveInfo.GetDrives();
            DriveInfo.GetDrives().ToList().ForEach(drive =>
            {
                var fileSystemObject = new FileSystemObjectInfo(drive);
                fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
                treeView.Items.Add(fileSystemObject);
            });
        }

        private void FileSystemObject_AfterExplore(object sender, System.EventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void FileSystemObject_BeforeExplore(object sender, System.EventArgs e)
        {
            Cursor = Cursors.Wait;
        }

        private void TreeView_SelectedItemChanged(object sender,  EventArgs e)
        {
            Batch_AddButton.IsEnabled = treeView.SelectedItem != null;
        }

        private void BatchListBox_SelectedItemChanged(object sender, EventArgs e)
        {
            Batch_RemoveButton.IsEnabled = BatchListBox.SelectedItem != null;
        }

        private void BatchFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Batch_ClearButton.IsEnabled = Context.BatchFiles.Count > 0;
        }

        #endregion

        #region GUI interaction

        private void Batch_AddButton_Click (object sender, RoutedEventArgs e)
        {
            Context.BatchFiles.Add((FileSystemObjectInfo)treeView.SelectedItem);
        }

        private void Batch_RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            object[] selections = new object[BatchListBox.SelectedItems.Count];
            BatchListBox.SelectedItems.CopyTo(selections, 0);

            foreach(object selection in selections)
            {
                Context.BatchFiles.Remove((FileSystemObjectInfo)selection);
            }
        }

        private void Batch_ClearButton_Click(object sender, RoutedEventArgs e)
        {
            //////       should ask user before
            Context.BatchFiles.Clear();
        }

        private void LearnManualButton_Click(object sender, RoutedEventArgs e)
        {
            Markov.Train(Context, LearnTextBox.Text);
            if (Context.AutoClear)
                LearnTextBox.Text = "";
        }

        private void LearnBatchButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(FileSystemObjectInfo info in Context.BatchFiles)
            {
                recursiveLearnFromFile(info);
            }
        }

        private void recursiveLearnFromFile (FileSystemObjectInfo info, bool recurse = false)
        {
            if((info.FileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var files = Directory.EnumerateFiles(info.FileSystemInfo.FullName, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(s => s.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

                foreach (string path in files)
                {
                    FileInfo fi = new FileInfo(path);
                    FileSystemObjectInfo fsoi = new FileSystemObjectInfo(fi);
                    recursiveLearnFromFile(fsoi);
                }
            }
            else
            {
                try
                {
                    using (StreamReader sr = new StreamReader(info.FileSystemInfo.FullName))
                    {
                        string line = sr.ReadToEnd();
                        Markov.Train(Context, line);
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(ex.Message);
                }
            }
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow window = new SettingsWindow();
            bool? result = window.ShowDialog();
            if (result == true)
                SaveSettings();
        }

        private void RowbreakCheckBox_Click(object sender, RoutedEventArgs e)
        {
            updateContextValues();
        }

        private void RootListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(RootListBox.SelectedItem != null)
            {
                ChildListBox.ItemsSource = (RootListBox.SelectedItem as Word).Children;
            }
        }

        #endregion

        #region Commands

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

        #endregion
    }
}
