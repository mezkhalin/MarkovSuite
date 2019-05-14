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
                }
            }
        }
        
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public Word(string data, bool isStarter, bool isEnding)
        {
            Data = data;
            IsStarting = isStarter;
            IsEnding = isEnding;
            Children = new ObservableCollection<ChildWord>();
        }
    }

    [Serializable]
    public class MarkovData /*: INotifyPropertyChanged*/
    {
        public bool HasChanged { get; set; } = false;
        public string FileName { get; set; } = "";
        public string ChainName { get; set; } = "Untitled";
        public ObservableCollection<Word> Words { get; set; }

        /*private string m_chainName = "Untitled";
        public string ChainName
        {
            get { return m_chainName; }
            set
            {
                if(m_chainName != value)
                {
                    m_chainName = value;
                    NotifyPropertyChanged("ChainName");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            HasChanged = true;
        }*/

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
