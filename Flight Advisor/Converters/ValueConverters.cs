using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FlightAdvisor.Models;

namespace FlightAdvisor.Converters
{
    public class DecisionToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DecisionType decision)
            {
                return decision switch
                {
                    DecisionType.Go => Color.Parse("#22c55e"),
                    DecisionType.Caution => Color.Parse("#f59e0b"),
                    DecisionType.NoGo => Color.Parse("#ef4444"),
                    _ => Color.Parse("#6b7280")
                };
            }
            return Color.Parse("#6b7280");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DecisionToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DecisionType decision)
            {
                return decision switch
                {
                    DecisionType.Go => "\u2705 GO - Conditions Favorable",
                    DecisionType.Caution => "\u26A0\uFE0F CAUTION - Review Hazards",
                    DecisionType.NoGo => "\u274C NO-GO - Flight Not Recommended",
                    _ => "\u2139\uFE0F Information Only"
                };
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
                return count > 0;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToExpandIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
                return isExpanded ? "\u25BC" : "\u25B6";
            return "\u25B6";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}