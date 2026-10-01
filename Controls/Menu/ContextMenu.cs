using System.Windows;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// Junevy-styled context menu. It keeps the native WPF menu behavior,
    /// including commands, keyboard navigation and nested submenus.
    /// </summary>
    public class ContextMenu : System.Windows.Controls.ContextMenu
    {
        /// <summary>菜单项悬停/子菜单打开背景画刷(默认 Surface.Hover;可自定义,模板触发器经 AncestorType 绑定)。</summary>
        public Brush ItemHoverBackground
        {
            get { return (Brush)GetValue(ItemHoverBackgroundProperty); }
            set { SetValue(ItemHoverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ItemHoverBackgroundProperty =
            DependencyProperty.Register("ItemHoverBackground", typeof(Brush), typeof(ContextMenu), new PropertyMetadata(null));

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
