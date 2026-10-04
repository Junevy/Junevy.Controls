using System.Windows;
using System.Windows.Media;
using Junevy.Controls.AttachedProperties;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// Junevy-styled context menu. It keeps the native WPF menu behavior,
    /// including commands, keyboard navigation and nested submenus.
    /// </summary>
    public class ContextMenu : System.Windows.Controls.ContextMenu
    {
        /// <summary>
        /// 菜单项悬停/子菜单打开背景画刷（默认 Surface.Hover，由默认样式注入）。
        /// 本属性是 <see cref="MenuAssist.ItemHoverBackground"/> 在 <see cref="ContextMenu"/> 上的同源代理——
        /// 条目模板经 <c>AncestorType=MenuBase</c> 读取共享附加属性，因此原生 <c>Menu</c>
        /// （JunevyMenuBarStyle）的下拉条目同样消费该画刷（经 atc:MenuAssist 设置）。
        /// </summary>
        public Brush ItemHoverBackground
        {
            get { return (Brush)GetValue(ItemHoverBackgroundProperty); }
            set { SetValue(ItemHoverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ItemHoverBackgroundProperty =
            MenuAssist.ItemHoverBackgroundProperty.AddOwner(typeof(ContextMenu));

        static ContextMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ContextMenu),
                new FrameworkPropertyMetadata(typeof(ContextMenu)));
        }
    }

    /// <summary>
    /// A named Junevy menu item. WPF's native MenuItem already provides the
    /// Header, Icon, Command and nested Items properties.
    /// </summary>
    public class ContextMenuItem : System.Windows.Controls.MenuItem
    {
        static ContextMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ContextMenuItem),
                new FrameworkPropertyMetadata(typeof(ContextMenuItem)));
        }

    }
}
