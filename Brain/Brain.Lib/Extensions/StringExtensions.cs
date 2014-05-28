using System;
using Windows.UI;
using Windows.UI.Xaml.Media;
#if NETFX_CORE

#endif

#if WINDOWS_PHONE
using System.Windows.Media;
#endif


namespace Brain.Extensions
{
    public static class StringExtensions
    {
        public static Color ToColor(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Colors.Transparent;

            text = text.Replace("#", "")
           .Replace(";", "");

            if (text.Length == 6)
            {
                byte r = Convert.ToByte(text.Substring(0, 2), 16);
                byte g = Convert.ToByte(text.Substring(2, 2), 16);
                byte b = Convert.ToByte(text.Substring(4, 2), 16);
                return Color.FromArgb(255, r, g, b);
            }
            if (text.Length == 8)
            {
                byte a = Convert.ToByte(text.Substring(0, 2), 16);
                byte r = Convert.ToByte(text.Substring(2, 2), 16);
                byte g = Convert.ToByte(text.Substring(4, 2), 16);
                byte b = Convert.ToByte(text.Substring(6, 2), 16);
                return Color.FromArgb(a, r, g, b);
            }
            return Colors.Transparent;
        }

        public static Brush ToColorBrush(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return default(Brush);

            return new SolidColorBrush(ToColor(text));
        }

        public static string Reverse(this string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

    }
}
