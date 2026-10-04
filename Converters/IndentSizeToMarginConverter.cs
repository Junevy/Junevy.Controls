using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Junevy.Controls.Converters
{
    /// <summary>
    /// 缩进宽度转 Margin：把 TreeView 的 IndentSize（DIP）转为子级承载区的左内缩 Margin(l,0,0,0)。
    /// 仅作用于子级 ItemsPresenter，因此本级行首观感不受影响。
    /// </summary>
    public class IndentSizeToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double size = value is double d && d >= 0d ? d : 0d;
            return new Thickness(size, 0d, 0d, 0d);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
