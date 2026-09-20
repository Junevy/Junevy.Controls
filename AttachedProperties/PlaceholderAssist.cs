using System.Windows;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// TextBox / ComboBox 的占位符附加属性。
    /// <para>
    /// 在输入控件内部显示一个占位内容，提示用户应输入或选择什么。
    /// 内容为任意 <see cref="object"/>，设为 iconfont 字形文本时需同时指定
    /// <see cref="Junevy.Controls.AttachedProperties.Icon.FontFamily"/>；
    /// 占位内容仅在控件无值时显示（TextBox 为文本为空且未聚焦，ComboBox 为未选中项），
    /// 有值后自动隐藏。
    /// </para>
    /// </summary>
    public class PlaceholderAssist
    {
        /// <summary>
        /// 标识 <see cref="GetPlaceholder"/>/<see cref="SetPlaceholder"/> 的附加属性。
        /// </summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached("Placeholder", typeof(object), typeof(PlaceholderAssist), new PropertyMetadata(null));

        /// <summary>
        /// 读取指定控件的占位内容；为 <see langword="null"/> 时不显示占位符。
        /// </summary>
        public static object GetPlaceholder(DependencyObject obj)
        {
            return obj.GetValue(PlaceholderProperty);
        }

        /// <summary>
        /// 在指定控件上设置占位内容（文本或 iconfont 字形）。
        /// </summary>
        public static void SetPlaceholder(DependencyObject obj, object value)
        {
            obj.SetValue(PlaceholderProperty, value);
        }
    }
}
