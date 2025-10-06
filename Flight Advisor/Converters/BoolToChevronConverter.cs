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
                // Unicode escape sequences for chevrons
                // Down arrow (expanded): \u25BC ?
                // Right arrow (collapsed): \u25B6 ?
                return isExpanded ? "\u25BC" : "\u25B6";
            }
            return "\u25B6";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}