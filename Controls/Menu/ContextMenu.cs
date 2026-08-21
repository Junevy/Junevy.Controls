using System.Windows;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// Junevy-styled context menu. It keeps the native WPF menu behavior,
    /// including commands, keyboard navigation and nested submenus.
    /// </summary>
    public class ContextMenu : System.Windows.Controls.ContextMenu
    {
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
