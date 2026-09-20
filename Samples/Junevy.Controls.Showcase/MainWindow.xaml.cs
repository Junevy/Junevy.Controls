using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Bar;
using Junevy.Controls.Showcase.Pages;
using Junevy.Controls.Themes;
using JvMenuItem = Junevy.Controls.Controls.Menu.MenuItem;

namespace Junevy.Controls.Showcase
{
    /// <summary>
    /// 展示程序主窗口：AppBar（默认模板）+ SideMenu 分类导航 + 各控件分类展示页。
    /// 附带 SidePanel 设置抽屉与 MessageBarService 通知宿主。
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, UserControl> _pageCache = [];

        public MainWindow()
        {
            InitializeComponent();

            // MessageBarService 是应用级静态服务：注册宿主后可在任意位置弹出通知
            MessageBarService.SetPresenter(NotificationPresenter);

            // 默认进入第一个分类
            NavMenu.SelectedIndex = 0;
        }

        private UserControl GetPage(string title)
        {
            if (_pageCache.TryGetValue(title, out UserControl? cached))
            {
                return cached;
            }

            UserControl page = title switch
            {
                "按钮控件" => new ButtonsPage(),
                "输入与选择" => new InputsPage(),
                "集合与数据" => new DataPage(),
                "文本与状态" => new TextStatePage(),
                "通知" => new NotifyPage(),
                "布局控件" => new LayoutPage(),
                "菜单与导航" => new MenusPage(),
                "窗口与图像" => new WindowImagePage(),
                _ => new ButtonsPage()
            };
            _pageCache[title] = page;
            return page;
        }

        private void OnNavSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavMenu.SelectedItem is JvMenuItem item && item.Title is string title)
            {
                PageHost.Content = GetPage(title);
            }
        }

        private void OnToggleThemeClick(object sender, RoutedEventArgs e)
        {
            ThemeManager.ToggleTheme();
            MessageBarService.Show(MessageBarAppearance.Informational, "ThemeManager", "已切换浅色/深色主题");
        }

        private void OnOpenSettingsClick(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Toggle();
        }

        private void OnCloseSettingsClick(object sender, RoutedEventArgs e)
        {
            SettingsPanel.IsOpen = false;
        }
    }
}
