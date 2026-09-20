using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Bar;
using Junevy.Controls.Controls.Menu;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>菜单与导航展示页：ContextMenu / TreeMenu / TabMenu / ToolBar / Toolbox。</summary>
    public partial class MenusPage : UserControl
    {
        /// <summary>TreeMenu 虚拟导航树。</summary>
        public ObservableCollection<TreeMenuItem> NavTree { get; } = [];

        public MenusPage()
        {
            InitializeComponent();

            NavTree.Add(new TreeMenuItem
            {
                Title = "相机",
                Icon = "\uE66B",
                Childrens =
                {
                    new TreeMenuItem { Title = "实时预览", TargetType = typeof(Window) },
                    new TreeMenuItem { Title = "参数设置", TargetType = typeof(Window) },
                    new TreeMenuItem
                    {
                        Title = "标定",
                        Icon = "\uE60F",
                        Childrens = { new TreeMenuItem { Title = "九点标定" }, new TreeMenuItem { Title = "手眼标定" } }
                    }
                }
            });
            NavTree.Add(new TreeMenuItem { Title = "日志", Icon = "\uE651" });
            NavTree.Add(new TreeMenuItem { Title = "帮助", Icon = "\uE932" });

            TreeNav.ItemsSource = NavTree;
            TreeNav.NavigateCommand = new RelayCommand(() =>
                MessageBarService.Show("TreeMenu", "叶节点已激活（NavigateCommand）"));
        }

        private void OnTabClosing(object sender, TabCloseEventArgs e)
        {
            if (e.Tab.Header as string == "保护页")
            {
                e.Cancel = true;
                MessageBarService.Show(MessageBarAppearance.Warning, "TabMenu", "「保护页」不允许关闭（TabClosing 已取消）");
            }
        }
    }
}
