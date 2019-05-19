﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSuite
{
    [Serializable]
    public struct ChildWord
    {
        public string Data { get; set; }
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
        public string Data { get; set; }
        public bool IsStarting { get; set; }
        public bool IsEnding { get; set; }
        public ObservableCollection<ChildWord> Children { get; set; }
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
                    Source.NotifyPropertyChanged("Words");
                }
            }
        }
        
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
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

        [NonSerialized]
        private string m_statusString = "Idle...";
        public string StatusString
        {
            get { return m_statusString; }
            set
            {
                m_statusString = value;
                NotifyPropertyChanged("StatusString");
            }
        }
        public bool AutoClear { get; set; } = false;        // auto clear input box after learning
        public bool AutoRowbreak { get; set; } = false;     // auto rowbreak output box

        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            if(propName != "HasChanged") HasChanged = true;
        }

        public MarkovData()
        {
            Words = new ObservableCollection<Word>();
            Init();
        }

        public void Init ()
        {
            Words.CollectionChanged += Words_CollectionChanged;
        }

        private void Words_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
