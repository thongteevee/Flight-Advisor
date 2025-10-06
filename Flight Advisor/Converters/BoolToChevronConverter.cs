// Converters/BoolToChevronConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FlightAdvisor.Converters
{
    public class BoolToChevronConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                // Unicode hex codes for chevrons
                // Down arrow (expanded): U+25BC
                // Right arrow (collapsed): U+25B6
                return isExpanded ? "&#x25BC;" : "&#x25B6;";
            }
            return "&#x25B6;";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}