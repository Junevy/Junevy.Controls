using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Junevy.Controls.AttachedProperties;

namespace Junevy.Controls.Converters
{
    /// <summary>
    /// 将 <see cref="TitleAssist.TitleAlignmentProperty"/> 的值映射为面板的 <see cref="HorizontalAlignment"/>，
    /// 供 <c>TextBox</c> / <c>ComboBox</c> / <c>PasswordBox</c> 模板把标题整组摆到标题区域的左/右/中。
    /// <para>
    /// 之所以不直接把附加属性声明成 <see cref="HorizontalAlignment"/>：那样宿主能写出
    /// <see cref="HorizontalAlignment.Stretch"/> 这类对标题无意义的取值并静默失效，
    /// 故用封闭枚举 + 本转换器一对一映射，无法识别的值回退为 <see cref="HorizontalAlignment.Left"/>
    /// （与附加属性的注册默认值一致）。
    /// </para>
    /// </summary>
    public class TitleAlignmentToHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is TitleAlignment alignment
                ? alignment switch
                {
                    TitleAlignment.Right => HorizontalAlignment.Right,
                    TitleAlignment.Center => HorizontalAlignment.Center,
                    _ => HorizontalAlignment.Left,
                }
                : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
