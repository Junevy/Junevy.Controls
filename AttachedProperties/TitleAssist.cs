using System.Windows;
using System.Windows.Media;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// TextBox / ComboBox 的标题附加属性集。
    /// <para>
    /// 在输入控件外侧（上/下/左/右）显示一个标题，提示该输入框的用途。
    /// 标题内容为任意 <see cref="object"/>，设为 iconfont 字形文本时需同时指定
    /// <see cref="TitleFontFamily"/>；标题位置由 <see cref="TitlePlacement"/> 控制，
    /// 字体样式可通过配套附加属性自定义，未设置的样式项自动继承控件自身取值。
    /// 通过 <see cref="TitleWidth"/> 可为标题区域指定固定宽度，用于表单式布局中输入框整列对齐。
    /// </para>
    /// </summary>
    public class TitleAssist
    {
        /// <summary>
        /// 标识 <see cref="GetTitle"/>/<see cref="SetTitle"/> 的附加属性。
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(object), typeof(TitleAssist), new PropertyMetadata(null));

        /// <summary>
        /// 标识 <see cref="GetTitlePlacement"/>/<see cref="SetTitlePlacement"/> 的附加属性。
        /// </summary>
        public static readonly DependencyProperty TitlePlacementProperty =
            DependencyProperty.RegisterAttached("TitlePlacement", typeof(TitlePlacement), typeof(TitleAssist), new PropertyMetadata(TitlePlacement.Top));

        /// <summary>
        /// 标识 <see cref="GetTitleFontFamily"/>/<see cref="SetTitleFontFamily"/> 的附加属性。
        /// </summary>
        public static readonly DependencyProperty TitleFontFamilyProperty =
            DependencyProperty.RegisterAttached("TitleFontFamily", typeof(FontFamily), typeof(TitleAssist), new PropertyMetadata(null));

        /// <summary>
        /// 标识 <see cref="GetTitleFontSize"/>/<see cref="SetTitleFontSize"/> 的附加属性。
        /// </summary>
        public static readonly DependencyProperty TitleFontSizeProperty =
            DependencyProperty.RegisterAttached("TitleFontSize", typeof(double), typeof(TitleAssist), new PropertyMetadata(double.NaN));

        /// <summary>
        /// 标识 <see cref="GetTitleWidth"/>/<see cref="SetTitleWidth"/> 的附加属性。
        /// </summary>
        public static readonly DependencyProperty TitleWidthProperty =
            DependencyProperty.RegisterAttached("TitleWidth", typeof(double), typeof(TitleAssist), new PropertyMetadata(double.NaN));

        /// <summary>
        /// 标识 <see cref="GetTitleForeground"/>/<see cref="SetTitleForeground"/> 的附加属性。
        /// </summary>
        public static readonly DependencyProperty TitleForegroundProperty =
            DependencyProperty.RegisterAttached("TitleForeground", typeof(Brush), typeof(TitleAssist), new PropertyMetadata(null));

        /// <summary>
        /// 标识 <see cref="GetTitleFontWeight"/>/<see cref="SetTitleFontWeight"/> 的附加属性。
        /// </summary>
        public static readonly DependencyProperty TitleFontWeightProperty =
            DependencyProperty.RegisterAttached("TitleFontWeight", typeof(FontWeight), typeof(TitleAssist), new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// 读取指定控件的标题内容；为 <see langword="null"/> 时不显示标题。
        /// </summary>
        public static object GetTitle(DependencyObject obj)
        {
            return obj.GetValue(TitleProperty);
        }

        /// <summary>
        /// 在指定控件上设置标题内容（文本或 iconfont 字形）。
        /// </summary>
        public static void SetTitle(DependencyObject obj, object value)
        {
            obj.SetValue(TitleProperty, value);
        }

        /// <summary>
        /// 读取指定控件的标题位置，默认 <see cref="TitlePlacement.Top"/>。
        /// </summary>
        public static TitlePlacement GetTitlePlacement(DependencyObject obj)
        {
            return (TitlePlacement)obj.GetValue(TitlePlacementProperty);
        }

        /// <summary>
        /// 在指定控件上设置标题位置。
        /// </summary>
        public static void SetTitlePlacement(DependencyObject obj, TitlePlacement value)
        {
            obj.SetValue(TitlePlacementProperty, value);
        }

        /// <summary>
        /// 读取标题字体族；为 <see langword="null"/> 时继承控件自身字体。
        /// </summary>
        public static FontFamily GetTitleFontFamily(DependencyObject obj)
        {
            return (FontFamily)obj.GetValue(TitleFontFamilyProperty);
        }

        /// <summary>
        /// 在指定控件上设置标题字体族；标题为 iconfont 字形时需设置为 iconfont。
        /// </summary>
        public static void SetTitleFontFamily(DependencyObject obj, FontFamily value)
        {
            obj.SetValue(TitleFontFamilyProperty, value);
        }

        /// <summary>
        /// 读取标题字号；<see cref="double.NaN"/> 时继承控件自身字号。
        /// </summary>
        public static double GetTitleFontSize(DependencyObject obj)
        {
            return (double)obj.GetValue(TitleFontSizeProperty);
        }

        /// <summary>
        /// 在指定控件上设置标题字号。
        /// </summary>
        public static void SetTitleFontSize(DependencyObject obj, double value)
        {
            obj.SetValue(TitleFontSizeProperty, value);
        }

        /// <summary>
        /// 读取标题区域固定宽度；<see cref="double.NaN"/> 时自适应标题内容。
        /// </summary>
        public static double GetTitleWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(TitleWidthProperty);
        }

        /// <summary>
        /// 在指定控件上设置标题区域固定宽度（DIP），四个方位统一生效；
        /// 用于表单式布局中使不同长度的标题仍保持一致的标题—输入框间距、输入框整列对齐。
        /// </summary>
        public static void SetTitleWidth(DependencyObject obj, double value)
        {
            obj.SetValue(TitleWidthProperty, value);
        }

        /// <summary>
        /// 读取标题前景色；为 <see langword="null"/> 时由默认样式提供主题次级文本色。
        /// </summary>
        public static Brush GetTitleForeground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(TitleForegroundProperty);
        }

        /// <summary>
        /// 在指定控件上设置标题前景色。
        /// </summary>
        public static void SetTitleForeground(DependencyObject obj, Brush value)
        {
            obj.SetValue(TitleForegroundProperty, value);
        }

        /// <summary>
        /// 读取标题字重，默认 <see cref="FontWeights.Normal"/>。
        /// </summary>
        public static FontWeight GetTitleFontWeight(DependencyObject obj)
        {
            return (FontWeight)obj.GetValue(TitleFontWeightProperty);
        }

        /// <summary>
        /// 在指定控件上设置标题字重（如 <see cref="FontWeights.Bold"/>）。
        /// </summary>
        public static void SetTitleFontWeight(DependencyObject obj, FontWeight value)
        {
            obj.SetValue(TitleFontWeightProperty, value);
        }
    }
}
