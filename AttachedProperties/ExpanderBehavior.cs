using Junevy.Controls.Controls.Button;
using Junevy.Controls.Controls.Menu;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.AttachedProperties
{
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
            if (item.DataContext is not TreeMenuItem vm)
                return;

            if (vm.IsLeaf)
            {
                FindAncestor<TreeMenu>(item)?.NavigateCommand?.Execute(vm);
            }
            else
            {
                item.IsExpanded = !item.IsExpanded;
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
