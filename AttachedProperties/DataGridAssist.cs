using System.Windows;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// DataGrid 附加属性集。
    /// <para>
    /// 以附加属性形式增强 DataGrid，官方原生实例与 <c>jv:DataGrid</c> 派生实例均可直接设置，
    /// 无需派生新类型。
    /// </para>
    /// </summary>
    public class DataGridAssist
    {
        /// <summary>
        /// 空态提示文本：Items 为空时显示在表格内容区中央；为空（null/空白）时不显示提示。
        /// </summary>
        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.RegisterAttached("EmptyText", typeof(string), typeof(DataGridAssist), new PropertyMetadata(null));

        /// <summary>
        /// 读取指定 DataGrid 上的空态提示文本。
        /// </summary>
        public static string GetEmptyText(DependencyObject obj)
        {
            return (string)obj.GetValue(EmptyTextProperty);
        }

        /// <summary>
        /// 在指定 DataGrid 上设置空态提示文本。
        /// </summary>
        public static void SetEmptyText(DependencyObject obj, string value)
        {
            obj.SetValue(EmptyTextProperty, value);
        }
    }
}
