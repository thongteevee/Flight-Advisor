// Converters/ValueConverters.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FlightAdvisor.Models;

namespace FlightAdvisor.Converters
{
    /// <summary>
    /// Converts DecisionType to Color
    /// </summary>
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

    /// <summary>
    /// Converts DecisionType to Display Text
    /// </summary>
    public class DecisionToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DecisionType decision)
            {
                return decision switch
                {
                    DecisionType.Go => "✅ GO - Conditions Favorable",
                    DecisionType.Caution => "⚠️ CAUTION - Review Hazards",
                    DecisionType.NoGo => "❌ NO-GO - Flight Not Recommended",
                    _ => "ℹ️ Information Only"
                };
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts count to boolean (for visibility)
    /// </summary>
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

    /// <summary>
    /// Converts boolean to expand/collapse icon
    /// </summary>
    public class BoolToExpandIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
                return isExpanded ? "▼" : "▶";
            return "▶";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}