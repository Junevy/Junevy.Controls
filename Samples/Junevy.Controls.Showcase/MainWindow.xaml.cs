using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private static readonly ImageSource AppbarIconLight = CreateAppbarIconSource("github_light.png");
        private static readonly ImageSource AppbarIconDark = CreateAppbarIconSource("github_dark.png");

        private readonly Dictionary<string, UserControl> _pageCache = [];

        public MainWindow()
        {
            InitializeComponent();

            // MessageBarService 是应用级静态服务：注册宿主后可在任意位置弹出通知
            MessageBarService.SetPresenter(NotificationPresenter);

            // 默认进入第一个分类
            NavMenu.SelectedIndex = 0;

            // 主题可在窗口创建前被外部切换，图标与当前主题保持同步
            UpdateAppbarIcon();
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
                "菜单与导航" => new MenusPage(),
                "栏与工具" => new BarsPage(),
                "通知" => new NotifyPage(),
                "进度条" => new ProgressPage(),
                "布局控件" => new LayoutPage(),
                "窗口与对话框" => new WindowPage(),
                "图像" => new ImagePage(),
                "图标字体" => new IconsPage(),
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

        private void OnGoHomeClick(object sender, RoutedEventArgs e)
        {
            NavMenu.SelectedIndex = 0;
        }

        private void OnShowNotificationClick(object sender, RoutedEventArgs e)
        {
            MessageBarService.Show(MessageBarAppearance.Informational, "MessageBarService", "这是一条来自 AppBar 工具栏的通知演示");
        }

        private void OnToggleWindowStateClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void OnToggleThemeClick(object sender, RoutedEventArgs e)
        {
            ThemeManager.ToggleTheme();
            UpdateAppbarIcon();
            MessageBarService.Show(MessageBarAppearance.Informational, "ThemeManager", "已切换浅色/深色主题");
        }

        /// <summary>
        /// AppBar 应用图标随主题切换：浅色用白底字标（github_light），深色用暗底字标（github_dark）。
        /// </summary>
        private void UpdateAppbarIcon()
        {
            AppbarIcon.Source = ThemeManager.CurrentTheme == AppTheme.Dark ? AppbarIconDark : AppbarIconLight;
        }

        private static ImageSource CreateAppbarIconSource(string fileName)
        {
            var source = new BitmapImage();
            source.BeginInit();
            source.UriSource = new Uri($"pack://application:,,,/Junevy.Controls;component/Resources/Pictures/{fileName}", UriKind.Absolute);
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.EndInit();
            source.Freeze();
            return source;
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
