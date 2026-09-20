using System.Windows.Controls;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>集合与数据展示页：ListBox / ListView / DataGrid（虚拟数据填充）。</summary>
    public partial class DataPage : UserControl
    {
        public DataPage()
        {
            InitializeComponent();
            DataContext = SampleData.Instance;
        }
    }
}
