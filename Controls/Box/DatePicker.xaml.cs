using System.Windows;

namespace Junevy.Controls.Controls.Box
{
    /// <summary>
    /// 日期选择控件：继承 WPF <see cref="System.Windows.Controls.DatePicker"/>，
    /// 应用控件库主题样式（卡片输入框、日历弹层、悬停/聚焦/禁用状态）。
    /// 页面中亦可直接使用官方 <c>&lt;DatePicker&gt;</c> 写法，二者外观一致。
    /// </summary>
    public class DatePicker : System.Windows.Controls.DatePicker
    {
        static DatePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DatePicker),
                new FrameworkPropertyMetadata(typeof(DatePicker)));
        }
    }
}
