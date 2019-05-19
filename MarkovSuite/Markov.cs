using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSuite
{
    public static class Markov
    {
        private static Random m_Rnd;

        public static void Train (MarkovData context, string data)
        {
            data = data.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
            string[] words = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            Word cur, prev = null;
            bool isStarter = true, isEnding;
            string w;
            foreach(string word in words)
            {
                w = word.ToLowerInvariant();
                isEnding = w.EndsWith(".") || w.EndsWith("!") || w.EndsWith("?");

                if (prev != null && !isStarter)
                    prev.Children.Add(new ChildWord(w, isEnding));

                if ((cur = context.Find(Strip(w))) != null)
                {
                    cur.Prevalence++;
                }
                else
                {
                    cur = new Word(context, Strip(w), isStarter, isEnding);
                    context.Words.Add(cur);
                }

                prev = cur;
                if (isEnding)   // determines if next word is starter
                    isStarter = true;
                else
                    isStarter = false;
            }
        }

        public static string Generate (MarkovData context)
        {
            if(m_Rnd == null)
                m_Rnd = new Random();

            List<Word> _starters = context.GetStarters();
            List<Word> starters = new List<Word>();
            foreach(Word w in _starters)
            {
                for (int i = 0; i < w.Prevalence; i++)
                    starters.Add(w);
            }
            Word prev = starters[m_Rnd.Next(0, starters.Count)];
            string rtn = prev.Data + " ";
            while(true)
            {
                ChildWord w = prev.Children[m_Rnd.Next(0, prev.Children.Count)];
                rtn += w.Data;
                prev = context.Find(Strip(w.Data));
                if(w.IsEnding)
                {
                    break;
                }
                else
                {
                    rtn += " ";
                }
            }
            return rtn;
        }

        private static string Strip (string word)
        {
            return word.Replace(".", "").Replace("!", "").Replace("?", "");
        }
    }
}
