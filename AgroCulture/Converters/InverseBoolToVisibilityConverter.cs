using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AgroCulture.Converters
{
    /// <summary>
    /// Инвертированный конвертер: True → Collapsed, False → Visible
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // ✅ ИНВЕРСИЯ: true = скрыто, false = видимо
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}