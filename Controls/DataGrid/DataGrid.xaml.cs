using System.Windows;

namespace Junevy.Controls.Controls.DataGrid
{
    /// <summary>
    /// 主题化数据表格控件，继承自 WPF <see cref="System.Windows.Controls.DataGrid"/>。
    /// <para>
    /// 提供库内统一的卡片式外观与主题令牌配色：列标题、单元格、行悬停/选中、
    /// 空态提示（<c>atc:DataGridAssist.EmptyText</c>）以及官方宿主链
    /// ScrollViewer → ItemsPresenter → DataGridRowsPresenter：
    /// 模板中的 <see cref="ItemsPresenter"/> 实例化 DataGrid 默认 ItemsPanel
    /// （运行时命名 PART_RowsPresenter 的 <see cref="DataGridRowsPresenter"/>），
    /// 由 ScrollViewer 模板内的 PART_ScrollContentPresenter 呈现；
    /// 列头 PART_ColumnHeadersPresenter 位于 ScrollViewer 模板内，垂直滚动时保持固定。
    /// </para>
    /// <para>
    /// 通过主题作用域隐式样式，官方原生写法 <c>&lt;DataGrid&gt;</c> 在合并
    /// <c>Themes/Generic.xaml</c> 后同样自动获得该视觉与模板。
    /// </para>
    /// </summary>
    public class DataGrid : System.Windows.Controls.DataGrid
    {
        static DataGrid()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DataGrid),
                new FrameworkPropertyMetadata(typeof(DataGrid)));
        }
    }
}
