using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Junevy.Controls.AttachedProperties;

namespace Junevy.Controls.Converters
{
    /// <summary>
    /// 决定「标题区域」本身在模板行/列中的锚定方式，供 <c>TextBox</c> / <c>ComboBox</c> / <c>PasswordBox</c>
    /// 模板的上、下标题外层容器使用。
    /// <para>
    /// 规则：<see cref="TitleAssist.TitleWidthProperty"/> 未设置（<see cref="double.NaN"/>）时返回
    /// <see cref="HorizontalAlignment.Stretch"/>，让标题区域铺满输入区整行，
    /// 于是 <see cref="TitleAlignment"/> 的 Right 能把标题摆到输入框右端；
    /// 设置了固定宽度时返回 <see cref="HorizontalAlignment.Left"/>，把该宽度的区域锚在行首，
    /// 靠齐才是在「这段固定宽度内部」发生，而不是跑到行的另一头。
    /// </para>
    /// <para>
    /// 为什么不能一律用 <see cref="HorizontalAlignment.Stretch"/>：WPF 中显式 <c>Width</c> 会覆盖
    /// Stretch 的填充效果并把元素在槽位内居中，固定宽度反而失去左端基准。
    /// 左/右方位的外层容器位于 Auto 列，列宽恒等于区域自身宽度（没有多余空间），因此不需要本映射。
    /// </para>
    /// </summary>
    public class TitleWidthToAreaAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is double width && !double.IsNaN(width)
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Stretch;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
