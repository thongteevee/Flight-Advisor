// Converters/BoolToThemeIconConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FlightAdvisor.Converters
{
    public class BoolToThemeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDarkMode)
            {
                return isDarkMode ? "\u2600\uFE0F" : "\u1F319";
            }
            return "\u1F319";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToThemeTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDarkMode)
            {
                return isDarkMode ? "Light Mode" : "Dark Mode";
            }
            return "Toggle Theme";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}