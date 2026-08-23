using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Junevy.Controls.Converters
{
    /// <summary>
    /// Distinguishes GridView from other ListView views so a shared item
    /// template can display either grid columns or normal content.
    /// </summary>
    public sealed class IsGridViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is GridView;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
