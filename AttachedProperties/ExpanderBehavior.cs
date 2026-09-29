using Junevy.Controls.Controls.Button;
using Junevy.Controls.Controls.Menu;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// TreeViewItem 的双击/Enter 行为：非叶节点切换展开/收起，叶节点触发最近
    /// <see cref="Junevy.Controls.Controls.Menu.TreeMenu"/> 的 <c>NavigateCommand</c>，
    /// 命令参数为该容器的数据对象（对数据类型无要求）。
    /// </summary>
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
            if (d is TreeViewItem item)
            {
                if ((bool)e.NewValue)
                {
                    item.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                    item.KeyDown += OnKeyDown;
                }
                else
                {
                    item.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                    item.KeyDown -= OnKeyDown;
                }
            }
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2 || sender is not TreeViewItem item)
                return;

            // 展开箭头本身是 ToggleButton，单击即切换展开状态；
            // 双击箭头时避免再切换一次，造成“展开后立刻收起”的抖动。
            if (FindAncestor<ToggleButton>(e.OriginalSource as DependencyObject) != null)
                return;

            // 只处理当前容器（避免父节点被子节点冒泡的事件误触发）。
            if (!ReferenceEquals(FindAncestor<TreeViewItem>(e.OriginalSource as DependencyObject), item))
                return;

            ToggleOrActivate(item);
            e.Handled = true;
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || sender is not TreeViewItem item)
                return;

            ToggleOrActivate(item);
            e.Handled = true;
        }

        private static void ToggleOrActivate(TreeViewItem item)
        {
            // 叶/枝由容器自身的 HasItems 判断，不依赖具体数据类型；
            // 展开状态经默认容器样式与数据模型的 IsExpanded 双向绑定落回模型。
            if (item.HasItems)
            {
                item.IsExpanded = !item.IsExpanded;
                return;
            }

            // 叶节点：激活，交给最近 TreeMenu 的 NavigateCommand，参数为该节点的数据对象。
            if (FindAncestor<TreeMenu>(item)?.NavigateCommand is ICommand command)
            {
                command.Execute(item.DataContext);
            }
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
