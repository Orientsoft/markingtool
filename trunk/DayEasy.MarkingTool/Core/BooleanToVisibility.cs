using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DayEasy.MarkingTool.Core
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            var b = System.Convert.ToBoolean(value);
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            return (Visibility)value == Visibility.Visible;
        }
    }
}
