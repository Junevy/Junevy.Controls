using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Bar;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>栏与工具展示页：ToolBar / Toolbox / AppBar / InfoBar。</summary>
    public partial class BarsPage : UserControl
    {
        /// <summary>InfoBar 菜单列表(经 ItemsSource 绑定的演示数据)。</summary>
        public System.Collections.Generic.IEnumerable<string> InfoBarMenuItems { get; } =
            new[] { "应用设置", "个人资料", "切换语言", "退出登录" };

        public BarsPage()
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
    }
}
