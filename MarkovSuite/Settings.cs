using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSuite
{
    [Serializable]
    public class Settings
    {
        public static Settings Instance { get; set; }

        public static string DefaultAppSavePath { get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MarkovSuite\\"; } }
        public static string SettingsPath { get { return DefaultAppSavePath + "settings.xml"; } }
        public static Settings Defaults
        {
            get
            {
                return new Settings()
                {
                    SavePath = DefaultAppSavePath,
                    RowbreakChars = new[]{ "\\r", "\\n" },
                    TerminationChars = new[]{ '.', '!', '?' },
                    StripChars = new[]{ '.', ',', '!', '?' }
                };
            }
        }

        private string m_savePath;

        public string SavePath
        {
            get { return m_savePath; }
            set
            {
                if (m_savePath != value)
                    m_savePath = value;
            }
        }

        public string[] RowbreakChars { get; set; }

        public char[] TerminationChars { get; set; }

        public char[] StripChars { get; set; }
    }
}
