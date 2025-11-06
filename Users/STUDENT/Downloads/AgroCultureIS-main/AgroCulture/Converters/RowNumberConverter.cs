using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace AgroCulture.Converters
{
    /// <summary>
    /// Конвертер для отображения номера строки в DataGrid
    /// </summary>
    public class RowNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ✅ value = объект данных (Users)
            // ✅ parameter = ссылка на DataGrid через x:Reference

            if (value == null || parameter == null)
                return "?";

            var dataGrid = parameter as DataGrid;
            if (dataGrid == null || dataGrid.Items == null)
                return "?";

            try
            {
                int index = dataGrid.Items.IndexOf(value);

                if (index >= 0)
                {
                    return (index + 1).ToString();
                }
            }
            catch
            {
                // Игнорируем ошибки при инициализации
            }

            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}