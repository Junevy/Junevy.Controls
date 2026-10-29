using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>
    /// 集合与数据展示页：ListBox / ListView / DataGrid（虚拟数据填充），
    /// 以及宿主过滤器与 PagingAssist 分页共存的演示。
    /// </summary>
    public partial class DataPage : UserControl
    {
        private readonly CollectionViewSource _deviceFilterView;
        private readonly CollectionViewSource _deviceSideView;

        /// <summary>
        /// 分页演示（Placement=Left）的独立数据源：两个启用分页的控件不可共享同一「视图」
        /// （默认视图按数据源单例，会争抢同一过滤器）。此处各持一份集合；同源多控件也可用
        /// <see cref="CollectionViewSource"/> 为其一建独立视图，见下方筛选演示。
        /// </summary>
        public ObservableCollection<SampleData.PagedRowItem> ListBoxPagedRows { get; }

        /// <summary>状态筛选下拉选项；首项「全部」表示不限制状态。</summary>
        public string[] StatusOptions { get; } = ["全部", "在线", "离线", "告警"];

        public DataPage()
        {
            InitializeComponent();
            DataContext = SampleData.Instance;

            ListBoxPagedRows = [];
            for (int i = 1; i <= 20; i++)
            {
                ListBoxPagedRows.Add(new SampleData.PagedRowItem { Name = $"列表条目 {i:00}" });
            }

            _deviceFilterView = (CollectionViewSource)Resources["DeviceFilterView"];
            _deviceSideView = (CollectionViewSource)Resources["DeviceSideView"];

            // Resources 里的 CVS 靠 DataContext 继承绑 Source，而继承要等元素入视觉树后才解析：
            // 构造函数里 View 仍是 null，所以谓词挂到 Loaded 而不是这里。
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // 谓词只挂一次，之后改条件只需 Refresh()：分页协调器每轮重读 view.Filter，
            // 因此换页与改筛选条件互不覆盖。
            _deviceFilterView.View.Filter = MatchDevice;
            _deviceSideView.View.Filter = MatchSideDevice;
        }

        // InitializeComponent 期间 SelectedIndex="0" 就会触发 SelectionChanged，
        // 此时两个视图字段尚未赋值，故用 null 条件运算。
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => _deviceFilterView?.View?.Refresh();

        private void OnStatusSelectionChanged(object sender, SelectionChangedEventArgs e) => _deviceFilterView?.View?.Refresh();

        private void OnOnlineOnlyChanged(object sender, RoutedEventArgs e) => _deviceSideView?.View?.Refresh();

        private bool MatchDevice(object obj)
        {
            if (obj is not DeviceItem item)
            {
                return false;
            }

            string keyword = SearchBox.Text.Trim();
            if (keyword.Length > 0
                && !item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                && !item.IP.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string? status = StatusBox.SelectedItem as string;
            return status is null || status == "全部" || item.Status == status;
        }

        private bool MatchSideDevice(object obj)
            => obj is DeviceItem item
               && (OnlineOnlyBox.IsChecked != true || item.Status == "在线");
    }
}