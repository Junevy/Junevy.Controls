using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Junevy.Controls.Converters
{
    /// <summary>
    /// 将 <see langword="null"/>（或 <see cref="double.NaN"/>）转换为绑定表达式的
    /// <see cref="DependencyProperty.UnsetValue"/>，使目标属性回退到继承值或主题默认值。
    /// <para>
    /// 用于模板中绑定「未设置即继承」的可选附加属性（如 <c>TitleAssist.TitleFontFamily</c>、
    /// <c>TitleAssist.TitleFontSize</c>、<c>TitleAssist.TitleForeground</c>），
    /// 避免把空值显式写到文本元素上破坏字体继承链。
    /// </para>
    /// </summary>
    public class NullToUnsetValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }

            if (value is double size && double.IsNaN(size))
            {
                return DependencyProperty.UnsetValue;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
