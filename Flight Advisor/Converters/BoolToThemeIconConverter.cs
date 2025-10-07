// Converters/BoolToThemeIconConverter.cs
// These converters are now OPTIONAL since we have ThemeIcon/ThemeText properties in ViewModel
// You can delete this file if you update your XAML bindings to use the ViewModel properties directly

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FlightAdvisor.Converters
{
    /// <summary>
    /// Converter for theme icon - OPTIONAL, use ViewModel.ThemeIcon instead
    /// </summary>
    public class BoolToThemeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDarkMode)
            {
                // Sun icon when in dark mode (clicking switches to light)
                // Moon icon when in light mode (clicking switches to dark)
                return isDarkMode ? "\u2600\uFE0F" : "\u263E";
            }
            return "\u263E";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for theme text - OPTIONAL, use ViewModel.ThemeText instead
    /// </summary>
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