using System.Windows;
using System.Windows.Media;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// MenuBase（<see cref="System.Windows.Controls.Menu"/> / <see cref="System.Windows.Controls.ContextMenu"/>）
    /// 的条目级共享画刷。条目样式的悬停触发器经 <c>AncestorType=MenuBase</c> 读取，因此
    /// <see cref="Junevy.Controls.Controls.Menu.ContextMenu"/> 与采用 <c>JunevyMenuBarStyle</c> 的原生
    /// <c>Menu</c> 都能给各自的下拉条目提供悬停背景——条目模板只有一份，不按宿主复制。
    /// </summary>
    public static class MenuAssist
    {
        /// <summary>菜单条目悬停/子菜单打开背景画刷。由宿主样式注入默认值（Surface.Hover），可逐实例覆盖。</summary>
        public static readonly DependencyProperty ItemHoverBackgroundProperty =
            DependencyProperty.RegisterAttached("ItemHoverBackground", typeof(Brush), typeof(MenuAssist), new PropertyMetadata(null));

        public static Brush? GetItemHoverBackground(DependencyObject obj)
        {
            return (Brush?)obj.GetValue(ItemHoverBackgroundProperty);
        }

        public static void SetItemHoverBackground(DependencyObject obj, Brush? value)
        {
            obj.SetValue(ItemHoverBackgroundProperty, value);
        }
    }
}
