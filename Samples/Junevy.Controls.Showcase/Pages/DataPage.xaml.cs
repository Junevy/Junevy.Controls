using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>集合与数据展示页：ListBox / ListView / DataGrid（虚拟数据填充）。</summary>
    public partial class DataPage : UserControl
    {
        /// <summary>
        /// 分页演示（Placement=Left）的独立数据源：默认视图按数据源单例，
        /// 两个启用分页的控件共享同一集合会争抢同一视图过滤器，必须各自持有一份。
        /// </summary>
        public ObservableCollection<SampleData.PagedRowItem> ListBoxPagedRows { get; }

        public DataPage()
        {
            InitializeComponent();
            DataContext = SampleData.Instance;

            ListBoxPagedRows = [];
            for (int i = 1; i <= 20; i++)
            {
                ListBoxPagedRows.Add(new SampleData.PagedRowItem { Name = $"列表条目 {i:00}" });
            }
        }
    }
}
