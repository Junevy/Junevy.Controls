using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// TreeViewItem 的双击 / Enter 行为：分支节点切换展开或收起。
    /// 导航命令的执行分宿主：<see cref="Junevy.Controls.Controls.Menu.TreeView"/> 上
    /// <see cref="TreeViewAssist.NavigateCommand"/> 为**选中驱动**（本类不执行，避免与选中路径双发）；
    /// 官方 <see cref="System.Windows.Controls.TreeView"/> 上保留**叶激活**旧语义——双击/Enter 叶节点时执行。
    /// 容器自身被选中时按其所属树的 <see cref="TreeViewAssist.AutoExpandAncestorsProperty"/> 展开该容器的所有祖先。
    /// </summary>
    /// <remarks>
    /// 叶 / 枝由容器自身的 <see cref="TreeViewItem.HasItems"/> 判定，与条目的数据类型无关；
    /// 但展开、选中状态要持久化到数据模型，依赖条目暴露 <c>IsExpanded</c> / <c>IsSelected</c>
    /// ——默认容器样式已把它们与 <see cref="Junevy.Controls.Controls.Menu.TreeMenuItem"/> 的同名属性双向绑定，
    /// 换用自定义数据类型时必须在 <c>ItemContainerStyle</c> 内自行绑定这两条，否则状态只存在于容器上，收起即丢。
    /// 输入接管口径：Enter 只在容器自身发起时接管（子控件的 Enter 留给该控件）；双击跳过展开箭头、
    /// 文本 / 下拉 / 列表类子控件以及不在本容器子树内的来源；只有真的做了事（切换展开或执行命令）才置 <c>e.Handled</c>。
    /// </remarks>
    public static class ExpanderBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached("Enable", typeof(bool), typeof(ExpanderBehavior), new PropertyMetadata(false, OnChanged));

        public static bool GetEnable(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableProperty);
        }

        public static void SetEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableProperty, value);
        }

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TreeViewItem item) return;

            if ((bool)e.NewValue)
            {
                item.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                item.KeyDown += OnKeyDown;
                item.Selected += OnSelected;
            }
            else
            {
                item.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                item.KeyDown -= OnKeyDown;
                item.Selected -= OnSelected;
            }
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2 || sender is not TreeViewItem item) return;

            // 展开箭头是模板内 ToggleButton，单击已切换，双击不再重复处理。
            if (FindAncestor<ToggleButton>(e.OriginalSource as DependencyObject) != null) return;

            // 文本类/选择类内容自己消费双击（选词、展开下拉、切换项），不接管——否则宿主在节点里放编辑控件时双击毫无反应。
            // WPF 无 ComboBoxBase（那是 WinUI 类型），ComboBox/ListBox/ListView 均由 Selector 覆盖。
            if (FindAncestor<Control>(e.OriginalSource as DependencyObject) is { } control
                && (control is TextBoxBase or PasswordBox or Selector))
                return;

            // 只处理当前容器（子行的双击不该由父/祖先行接管）。
            if (!ReferenceEquals(FindAncestor<TreeViewItem>(e.OriginalSource as DependencyObject), item)) return;

            if (ToggleOrActivate(item)) e.Handled = true;
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || sender is not TreeViewItem item) return;

            // Enter 只在容器自身持有焦点/发起时接管；子控件（编辑框、下拉、按钮）的 Enter 留给该控件。
            if (!ReferenceEquals(e.OriginalSource, item)) return;

            if (ToggleOrActivate(item)) e.Handled = true;
        }

        private static void OnSelected(object sender, RoutedEventArgs e)
        {
            // Selected 是冒泡路由事件：子孙容器被选中时同样会走到祖先容器的处理器上，
            // 只处理来源就是本容器的事件，避免祖先替子孙展开、抢在子孙自身的展开时机之前。
            if (sender is not TreeViewItem item || !ReferenceEquals(e.OriginalSource, sender)) return;
            if (!ShouldAutoExpandAncestors(item)) return;

            // 沿容器父链展开祖先：可见的选中容器必然已生成，其祖先容器同样已生成，
            // 因此这里遍历容器是安全的（整树展开则走数据模型，见 jv:TreeView.ExpandAll）。
            for (var parent = ItemsControl.ItemsControlFromItemContainer(item);
                 parent is TreeViewItem parentItem;
                 parent = ItemsControl.ItemsControlFromItemContainer(parentItem))
            {
                parentItem.IsExpanded = true;   // 经默认容器样式的双向绑定落回数据模型
            }
        }

        private static bool ShouldAutoExpandAncestors(TreeViewItem item)
        {
            // AutoExpandAncestors 注册时未启用属性继承，宿主按文档在树上设值（jv:TreeView 实例属性，或官方
            // <TreeView> 经 atc:TreeViewAssist）不会自动下发到容器，故显式读最近的树——与 NavigateCommand 同一
            // 读取口径；容器不在树上时（尚未挂到可视树）退回其自身值，即 DP 默认值 true。
            return TreeViewAssist.GetAutoExpandAncestors(FindAncestor<TreeView>(item) ?? (DependencyObject)item);
        }

        private static bool ToggleOrActivate(TreeViewItem item)
        {
            if (item.HasItems)
            {
                item.IsExpanded = !item.IsExpanded;
                return true;
            }

            // jv:TreeView 的导航已改为选中驱动（选中变化即执行 NavigateCommand，见
            // TreeView.OnSelectedItemChanged），双击/Enter 在这里再执行会与选中路径「双发」——
            // 故 jv 宿主上叶节点不再激活，Enter/双击只承担分支展开切换。
            if (FindAncestor<Junevy.Controls.Controls.Menu.TreeView>(item) != null)
            {
                return false;
            }

            // 官方 <TreeView>（经 atc:TreeViewAssist 挂命令）保留叶激活旧语义：双击/Enter 执行命令。
            if (FindAncestor<System.Windows.Controls.TreeView>(item)?.GetValue(TreeViewAssist.NavigateCommandProperty) is ICommand command)
            {
                command.Execute(item.DataContext);
                return true;
            }

            return false;
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T target)
                    return target;

                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
