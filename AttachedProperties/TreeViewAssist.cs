using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Junevy.Controls.Controls.Menu;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// TreeView 的扩展附加属性。DisplayMode / IndentSize / AutoExpandAncestors / NavigateCommand / 悬停与选中画刷
    /// 统一经附加属性注册，库内 <see cref="Junevy.Controls.Controls.Menu.TreeView"/> 以 AddOwner 暴露为同名实例依赖属性，
    /// 官方 <see cref="System.Windows.Controls.TreeView"/> 合并 Themes/Generic.xaml 后也可直接使用这些附加属性。
    /// </summary>
    public static class TreeViewAssist
    {
        /// <summary>行首观感：<see cref="TreeViewDisplayMode.Chevron"/> 显示展开箭头，<see cref="TreeViewDisplayMode.Indicator"/> 折叠箭头并由左侧 accent 指示条标示选中的有子项节点。</summary>
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.RegisterAttached("DisplayMode", typeof(TreeViewDisplayMode), typeof(TreeViewAssist), new PropertyMetadata(TreeViewDisplayMode.Chevron));

        /// <summary>
        /// 树导航命令，参数为节点的数据对象。语义按宿主分：
        /// <c>jv:TreeView</c>（实例属性与其同源）为**选中驱动**——选中项变化即执行（见
        /// <see cref="Junevy.Controls.Controls.Menu.TreeView"/>）；官方 <c>&lt;TreeView&gt;</c> 上为**叶激活**旧语义
        /// ——双击/Enter 叶节点时执行（见 <see cref="ExpanderBehavior"/>）。
        /// </summary>
        public static readonly DependencyProperty NavigateCommandProperty =
            DependencyProperty.RegisterAttached("NavigateCommand", typeof(ICommand), typeof(TreeViewAssist), new PropertyMetadata(null));

        /// <summary>节点悬停背景画刷（默认样式取 Surface.Hover；可逐实例覆盖）。</summary>
        public static readonly DependencyProperty ItemHoverBackgroundProperty =
            DependencyProperty.RegisterAttached("ItemHoverBackground", typeof(Brush), typeof(TreeViewAssist), new PropertyMetadata(null));

        /// <summary>选中节点背景画刷（默认样式取 Surface.Sunken 中性灰；可逐实例覆盖）。</summary>
        public static readonly DependencyProperty SelectedItemBackgroundProperty =
            DependencyProperty.RegisterAttached("SelectedItemBackground", typeof(Brush), typeof(TreeViewAssist), new PropertyMetadata(null));

        public static TreeViewDisplayMode GetDisplayMode(DependencyObject obj)
        {
            return (TreeViewDisplayMode)obj.GetValue(DisplayModeProperty);
        }

        public static void SetDisplayMode(DependencyObject obj, TreeViewDisplayMode value)
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

        /// <summary>子级相对本级的缩进宽度（DIP），默认 10。缩进只作用于子级承载区，本级行首观感不受影响。</summary>
        public static readonly DependencyProperty IndentSizeProperty =
            DependencyProperty.RegisterAttached("IndentSize", typeof(double), typeof(TreeViewAssist), new PropertyMetadata(10.0));

        public static double GetIndentSize(DependencyObject obj)
        {
            return (double)obj.GetValue(IndentSizeProperty);
        }

        public static void SetIndentSize(DependencyObject obj, double value)
        {
            obj.SetValue(IndentSizeProperty, value);
        }

        /// <summary>
        /// 选中节点时是否自动展开其所有祖先，默认 true。置 false 后宿主需自行展开祖先，
        /// 否则被选中的节点可能位于收起的分支内而不可见。
        /// </summary>
        public static readonly DependencyProperty AutoExpandAncestorsProperty =
            DependencyProperty.RegisterAttached("AutoExpandAncestors", typeof(bool), typeof(TreeViewAssist), new PropertyMetadata(true));

        public static bool GetAutoExpandAncestors(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoExpandAncestorsProperty);
        }

        public static void SetAutoExpandAncestors(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoExpandAncestorsProperty, value);
        }
    }
}
