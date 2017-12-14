using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DayEasy.MarkingTool.Core
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToHidden : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Visible;
            var b = System.Convert.ToBoolean(value);
            return b ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return true;
            return (Visibility)value != Visibility.Visible;
        }
    }
}
