using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Junevy.Controls.Controls.Menu;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// TreeView 的扩展附加属性。DisplayMode / NavigateCommand / 悬停与选中画刷统一经附加属性注册，
    /// 库内 <see cref="Junevy.Controls.Controls.Menu.TreeView"/> 以 AddOwner 暴露为同名实例依赖属性，
    /// 官方 <see cref="System.Windows.Controls.TreeView"/> 合并 Themes/Generic.xaml 后也可直接使用这些附加属性。
    /// </summary>
    public static class TreeViewAssist
    {
        /// <summary>显示模式：<see cref="DisplayMode.Normal"/> 显示展开箭头，<see cref="DisplayMode.Icon"/> 使用左侧层级指示器（有子项的节点显示）。</summary>
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.RegisterAttached("DisplayMode", typeof(DisplayMode), typeof(TreeViewAssist), new PropertyMetadata(DisplayMode.Normal));

        /// <summary>激活叶节点时执行的命令（双击或按 Enter，见 <see cref="ExpanderBehavior"/>），参数为该节点的数据对象。</summary>
        public static readonly DependencyProperty NavigateCommandProperty =
            DependencyProperty.RegisterAttached("NavigateCommand", typeof(ICommand), typeof(TreeViewAssist), new PropertyMetadata(null));

        /// <summary>节点悬停背景画刷（默认样式取 Surface.Hover；可逐实例覆盖）。</summary>
        public static readonly DependencyProperty ItemHoverBackgroundProperty =
            DependencyProperty.RegisterAttached("ItemHoverBackground", typeof(Brush), typeof(TreeViewAssist), new PropertyMetadata(null));

        /// <summary>选中节点背景画刷（默认样式取 Surface.Sunken 中性灰；可逐实例覆盖）。</summary>
        public static readonly DependencyProperty SelectedItemBackgroundProperty =
            DependencyProperty.RegisterAttached("SelectedItemBackground", typeof(Brush), typeof(TreeViewAssist), new PropertyMetadata(null));

        public static DisplayMode GetDisplayMode(DependencyObject obj)
        {
            return (DisplayMode)obj.GetValue(DisplayModeProperty);
        }

        public static void SetDisplayMode(DependencyObject obj, DisplayMode value)
        {
            obj.SetValue(DisplayModeProperty, value);
        }

        public static ICommand? GetNavigateCommand(DependencyObject obj)
        {
            return (ICommand?)obj.GetValue(NavigateCommandProperty);
        }

        public static void SetNavigateCommand(DependencyObject obj, ICommand? value)
        {
            obj.SetValue(NavigateCommandProperty, value);
        }

        public static Brush? GetItemHoverBackground(DependencyObject obj)
        {
            return (Brush?)obj.GetValue(ItemHoverBackgroundProperty);
        }

        public static void SetItemHoverBackground(DependencyObject obj, Brush? value)
        {
            obj.SetValue(ItemHoverBackgroundProperty, value);
        }

        public static Brush? GetSelectedItemBackground(DependencyObject obj)
        {
            return (Brush?)obj.GetValue(SelectedItemBackgroundProperty);
        }

        public static void SetSelectedItemBackground(DependencyObject obj, Brush? value)
        {
            obj.SetValue(SelectedItemBackgroundProperty, value);
        }
    }
}
