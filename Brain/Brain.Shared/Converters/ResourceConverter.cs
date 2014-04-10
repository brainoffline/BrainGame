using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#if NETFX_CORE

#endif

#if WINDOWS_PHONE
using System.Windows;
using System.Windows.Data;
using System.Globalization;
#endif


namespace Brain.Converters
{
    public class ResourceConverter : IValueConverter
    {

#if NETFX_CORE
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Application.Current.Resources[value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
#endif

#if WINDOWS_PHONE
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Application.Current.Resources[value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
#endif

    }
}
