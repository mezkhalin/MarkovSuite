using MarkovSuite.TreeViewFileExplorer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSuite
{
    [Serializable]
    public struct LogEntry
    {
        public double LogTime { get; set; }
        public string Message { get; set; }

        public LogEntry (string msg)
        {
            LogTime = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Message = msg;
        }
    }

    [Serializable]
    public struct ChildWord
    {
        public string Data { get; set; }
        public string StrippedData { get { return Markov.Strip(Data); } }
        public bool IsEnding { get; set; }

        public ChildWord (string Word, bool isEnding)
        {
            Data = Word;
            IsEnding = isEnding;
        }
    }

    [Serializable]
    public class Word : INotifyPropertyChanged
    {
        public MarkovData Source { get; set; }
        public ObservableCollection<ChildWord> Children { get; set; }
        private string m_data = "";
        public string Data
        {
            get { return m_data; }
            set
            {
                if (m_data != value)
                {
                    m_data = value;
                    NotifyPropertyChanged("Data");
                }
            }
        }
        private bool m_isStarting;
        public bool IsStarting
        {
            get { return m_isStarting; }
            set
            {
                if (m_isStarting != value)
                {
                    m_isStarting = value;
                    NotifyPropertyChanged("IsStarting");
                }
            }
        }
        private bool m_isEnding;
        public bool IsEnding
        {
            get { return m_isEnding; }
            set
            {
                if (m_isEnding != value)
                {
                    m_isEnding = value;
                    NotifyPropertyChanged("IsEnding");
                }
            }
        }
        private int m_prevalence = 1;
        public int Prevalence
        {
            get { return m_prevalence; }
            set
            {
                if (m_prevalence != value)
                {
                    m_prevalence = value;
                    NotifyPropertyChanged("Prevalence");
                }
            }
        }
        
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            if (Source != null)
                Source.NotifyPropertyChanged("Words");
        }

        public Word(MarkovData source, string data, bool isStarting, bool isEnding)
        {
            Source = source;
            Data = data;
            IsStarting = isStarting;
            IsEnding = isEnding;
            Children = new ObservableCollection<ChildWord>();
            Children.CollectionChanged += Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Source.NotifyPropertyChanged("Words");
        }
    }

    [Serializable]
    public class MarkovData : INotifyPropertyChanged
    {
        private bool m_hasChanged = false;
        public bool HasChanged
        {
            get { return m_hasChanged; }
            set
            {
                if(m_hasChanged != value)
                {
                    m_hasChanged = value;
                    NotifyPropertyChanged("HasChanged");
                }
            }
        }
        public string FilePath { get; set; } = "";
        public string ChainName { get; set; } = "Untitled";
        public ObservableCollection<Word> Words { get; set; }
        public ObservableCollection<FileSystemObjectInfo> BatchFiles { get; set; }
        public ObservableCollection<LogEntry> Log { get; set; }
        
        public string LogString
        {
            get { return (Log.Count > 0) ? Log[Log.Count-1].Message : ""; }
        }
        public bool AutoClear { get; set; } = false;        // auto clear input box after learning
        public bool AutoRowbreak { get; set; } = false;     // auto rowbreak output box

        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            if(propName == "HasChanged") HasChanged = true;
        }

        public MarkovData()
        {
            Words = new ObservableCollection<Word>();
            BatchFiles = new ObservableCollection<FileSystemObjectInfo>();
            Log = new ObservableCollection<LogEntry>();
            Init();
        }

        public void Init ()
        {
            Words.CollectionChanged += CollectionChanged;
            BatchFiles.CollectionChanged += CollectionChanged;
            Log.CollectionChanged += LogChanged;
        }

        private void LogChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("LogString");
        }

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HasChanged = true;
        }

        public Word Find (string word)
        {
            return Words.FirstOrDefault(x => x.Data == word);
        }

        public List<Word> GetStarters ()
        {
            return Words.Where(x => x.IsStarting).ToList();
        }
    }
}
