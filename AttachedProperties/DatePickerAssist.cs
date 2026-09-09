using System.Windows;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// DatePicker 附加属性集。
    /// <para>
    /// 以附加属性形式增强 DatePicker，官方原生实例与派生实例均可直接设置，
    /// 无需派生新类型（DatePickerTextBox.Watermark 为内部属性，外部模板无法绑定，
    /// 占位文案统一经由此附加属性提供）。
    /// </para>
    /// </summary>
    public class DatePickerAssist
    {
        /// <summary>
        /// 占位提示文本：未选择日期且文本为空时显示在输入区；为空（null/空白）时不显示。
        /// </summary>
        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.RegisterAttached("PlaceHolder", typeof(string), typeof(DatePickerAssist), new PropertyMetadata(null));

        /// <summary>
        /// 读取指定 DatePicker 上的占位提示文本。
        /// </summary>
        public static string GetPlaceHolder(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceHolderProperty);
        }

        /// <summary>
        /// 在指定 DatePicker 上设置占位提示文本。
        /// </summary>
        public static void SetPlaceHolder(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceHolderProperty, value);
        }
    }
}
