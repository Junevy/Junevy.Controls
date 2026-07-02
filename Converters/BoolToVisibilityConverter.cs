using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Junevy.Controls.Converters
{
    /// <summary>
    /// bool值转换为Visibility的Converter
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            return value switch
            {
                bool boolValue => boolValue ? Visibility.Visible : Visibility.Collapsed,
                string text => string.IsNullOrWhiteSpace(text) ? Visibility.Collapsed : Visibility.Visible,
                _ => Visibility.Visible
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
