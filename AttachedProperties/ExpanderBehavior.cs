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
                    item.PreviewMouseLeftButtonDown += OnDoubleClick;
                else
                    item.PreviewMouseLeftButtonDown -= OnDoubleClick;
            }
        }

        private static void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (sender is not TreeViewItem item) return;
            ////if (e.ClickCount != 2) return;


            ////if (e.OriginalSource is not TreeView) return;

            //if (!item.IsMouseOver) return;

            //if (item.DataContext is TreeMenuItem ti)
            //{
            //    if (ti.IsLeaf)
            //    {
            //        ti.Command?.Execute(ti);
            //    }
            //    else
            //    {
            //        item.IsExpanded = !item.IsExpanded;
            //    }
            //}

            //e.Handled = true;

            if (e.ClickCount != 2)
                return;

            // 找到真正点击的 TreeViewItem
            var clickedItem = FindAncestor<TreeViewItem>(e.OriginalSource as DependencyObject);

            if (clickedItem == null)
                return;

            // 只处理当前 sender（避免父节点触发）
            if (!ReferenceEquals(clickedItem, sender))
                return;

            if (clickedItem.DataContext is TreeMenuItem vm)
            {
                if (!vm.IsLeaf)
                {
                    clickedItem.IsExpanded = !clickedItem.IsExpanded;
                }
                else
                {
                    //if (vm.Command?.CanExecute(vm) == true)
                        vm.Command?.Execute(vm);
                }
            }

            e.Handled = true;
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
