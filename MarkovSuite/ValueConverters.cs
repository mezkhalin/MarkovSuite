using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MarkovSuite
{
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

    public class CharArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new string((char[])value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).ToCharArray();
        }
    }

    public class RowbreakConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] parts = ((string[])value);
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < parts.Length; i++)
            {
                sb.Append(parts[i] + (i < parts.Length - 1 ? " " : ""));
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).Split(' ');
        }
    }
}
