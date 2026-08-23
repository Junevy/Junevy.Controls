using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Junevy.Controls.Converters
{
    /// <summary>
    /// Returns GridView columns without evaluating a Columns path on an
    /// unrelated ViewBase implementation.
    /// </summary>
    public sealed class GridViewColumnsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is GridView gridView
                ? gridView.Columns
                : DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
