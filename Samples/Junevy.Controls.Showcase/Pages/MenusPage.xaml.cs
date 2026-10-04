using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Bar;
using Junevy.Controls.Controls.Menu;
using JvMenuItem = Junevy.Controls.Controls.Menu.MenuItem;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>菜单与导航展示页：SideMenu / ContextMenu / TabControl / 原生 Menu 菜单栏样式。</summary>
    public partial class MenusPage : UserControl
    {
        public MenusPage()
        {
            InitializeComponent();
        }

        private void OnSideMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is JvMenuItem item)
            {
                MessageBarService.Show("SideMenu", $"已选中：{item.Title}");
            }
        }

        private void OnTabClosing(object sender, TabCloseEventArgs e)
        {
            if (e.Tab.Header as string == "保护页")
            {
                e.Cancel = true;
                MessageBarService.Show(MessageBarAppearance.Warning, "TabControl", "「保护页」不允许关闭（TabClosing 已取消）");
            }
        }
    }
}
