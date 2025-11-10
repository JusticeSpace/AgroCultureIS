using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AgroCulture.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive
                    ? new SolidColorBrush(Color.FromRgb(21, 128, 61))   // Зеленый (#15803d)
                    : new SolidColorBrush(Color.FromRgb(220, 38, 38));  // Красный (#dc2626)
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
