using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Bar;
using Junevy.Controls.Controls.Menu;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>菜单与导航展示页：ContextMenu / TabMenu / ToolBar / Toolbox。</summary>
    public partial class MenusPage : UserControl
    {
        /// <summary>InfoBar 菜单列表(经 ItemsSource 绑定的演示数据)。</summary>
        public System.Collections.Generic.IEnumerable<string> InfoBarMenuItems { get; } =
            new[] { "应用设置", "个人资料", "切换语言", "退出登录" };

        public MenusPage()
        {
            InitializeComponent();
        }

        private void OnInfoBarSettingsClick(object sender, RoutedEventArgs e)
        {
            MessageBarService.Show("InfoBar", "已点击设置按钮（SettingsClick 路由事件）");
        }

        private void OnInfoBarMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement container && container.DataContext is string item)
            {
                MessageBarService.Show("InfoBar", $"已点击菜单项：{item}");
            }
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
