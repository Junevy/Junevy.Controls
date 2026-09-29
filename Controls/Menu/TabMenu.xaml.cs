using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// 可关闭、可重命名的页签控件，继承 WPF <see cref="TabControl"/>。
    /// 页签容器为 <see cref="TabMenuItem"/>；关闭经 <see cref="CloseTabCommand"/> 或 <see cref="CloseTab"/>,
    /// 关闭前可经 <see cref="TabClosing"/> 拦截，关闭后派发 <see cref="TabClosed"/>。
    /// </summary>
    public class TabMenu : TabControl
    {
        public static readonly RoutedCommand CloseTabCommand = new(nameof(CloseTabCommand), typeof(TabMenu));

        public event EventHandler<TabCloseEventArgs>? TabClosing;

        public event EventHandler<TabCloseEventArgs>? TabClosed;

        static TabMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TabMenu),
                new FrameworkPropertyMetadata(typeof(TabMenu)));
        }

        public TabMenu()
        {
            CommandBindings.Add(new CommandBinding(CloseTabCommand, OnCloseTabCommand, OnCanCloseTabCommand));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TabMenuItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TabMenuItem;
        }

        public static readonly DependencyProperty CanCloseLastTabProperty =
            DependencyProperty.Register(nameof(CanCloseLastTab), typeof(bool), typeof(TabMenu), new PropertyMetadata(true));

        /// <summary>
        /// 是否在每个页签上显示关闭按钮。仅影响页签外观，页签仍可通过命令关闭。
        /// </summary>
        public static readonly DependencyProperty IsClosableProperty =
            DependencyProperty.Register(nameof(IsClosable), typeof(bool), typeof(TabMenu), new PropertyMetadata(true));

        /// <summary>
        /// 页签头圆角（模板只取其上两角与内容区衔接）。
        /// </summary>
        public static readonly DependencyProperty HeaderCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(HeaderCornerRadius),
                typeof(CornerRadius),
                typeof(TabMenu),
                new PropertyMetadata(new CornerRadius(4)));

        /// <summary>
        /// 内容区圆角（模板只取其下两角）。
        /// </summary>
        public static readonly DependencyProperty ContentCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(ContentCornerRadius),
                typeof(CornerRadius),
                typeof(TabMenu),
                new PropertyMetadata(new CornerRadius(8)));

        /// <summary>
        /// 关闭页签时是否释放内容元素的 <c>DataContext</c> 与 <c>Content</c>（仅当其实现 <see cref="IDisposable"/> 时调用 Dispose）。
        /// 默认 false：生命周期由调用方自行管理，库不做隐藏的清理副作用。
        /// </summary>
        public static readonly DependencyProperty DisposeContentOnCloseProperty =
            DependencyProperty.Register(nameof(DisposeContentOnClose), typeof(bool), typeof(TabMenu), new PropertyMetadata(false));

        public bool CanCloseLastTab
        {
            get => (bool)GetValue(CanCloseLastTabProperty);
            set => SetValue(CanCloseLastTabProperty, value);
        }

        public bool IsClosable
        {
            get => (bool)GetValue(IsClosableProperty);
            set => SetValue(IsClosableProperty, value);
        }

        public bool DisposeContentOnClose
        {
            get => (bool)GetValue(DisposeContentOnCloseProperty);
            set => SetValue(DisposeContentOnCloseProperty, value);
        }

        public CornerRadius HeaderCornerRadius
        {
            get => (CornerRadius)GetValue(HeaderCornerRadiusProperty);
            set => SetValue(HeaderCornerRadiusProperty, value);
        }

        public CornerRadius ContentCornerRadius
        {
            get => (CornerRadius)GetValue(ContentCornerRadiusProperty);
            set => SetValue(ContentCornerRadiusProperty, value);
        }

        public void CloseTab(TabMenuItem? tabItem)
        {
            if (tabItem is null)
            {
                throw new ArgumentNullException(nameof(tabItem));
            }

            if (!ContainsTab(tabItem))
            {
                return;
            }

            CloseTabInternal(tabItem);
        }

        private void OnCanCloseTabCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                TabMenuItem? tabItem = ResolveTabItem(e);
                e.CanExecute = tabItem is not null
                    && !tabItem.IsEditing
                    && (CanCloseLastTab || Items.Count > 1);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"TabMenu.CanCloseTab error: {ex}");
                e.CanExecute = false;
            }
        }

        private void OnCloseTabCommand(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                TabMenuItem? tabItem = ResolveTabItem(e);
                if (tabItem is null)
                {
                    return;
                }

                if (tabItem.IsEditing)
                {
                    e.Handled = true;
                    return;
                }

                CloseTabInternal(tabItem);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"TabMenu close command error: {ex}");
            }
        }

        private void CloseTabInternal(TabMenuItem tabItem)
        {
            try
            {
                if (!CanCloseLastTab && Items.Count <= 1)
                {
                    return;
                }

                if (!ContainsTab(tabItem))
                {
                    return;
                }

                TabCloseEventArgs args = new(tabItem);
                RaiseTabClosing(args);
                if (args.Cancel)
                {
                    return;
                }

                if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    PerformClose(tabItem, args);
                }
                else
                {
                    Dispatcher.BeginInvoke(
                        new Action(() => PerformClose(tabItem, args)),
                        DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"TabMenu close error: {ex}");
            }
        }

        private void PerformClose(TabMenuItem tabItem, TabCloseEventArgs args)
        {
            try
            {
                // 延迟派发期间页签可能已被移除，执行前需重新校验
                if (!ContainsTab(tabItem))
                {
                    return;
                }

                object itemToRemove = GetItemForTab(tabItem);

                if (tabItem.IsSelected && Items.Count > 1)
                {
                    int index = Items.IndexOf(itemToRemove);
                    if (index >= 0)
                    {
                        int newIndex = index > 0 ? index - 1 : Math.Min(1, Items.Count - 1);
                        if (newIndex >= 0 && newIndex < Items.Count)
                        {
                            SelectedIndex = newIndex;
                        }
                    }
                }

                if (!RemoveItem(itemToRemove))
                {
                    return;
                }

                if (ReferenceEquals(itemToRemove, tabItem))
                {
                    CleanupTabItem(tabItem, DisposeContentOnClose);
                }

                RaiseTabClosed(args);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"TabMenu close error: {ex}");
            }
        }

        private void RaiseTabClosing(TabCloseEventArgs args)
        {
            EventHandler<TabCloseEventArgs>? handler = TabClosing;
            if (handler is null)
            {
                return;
            }

            foreach (Delegate subscriber in handler.GetInvocationList())
            {
                try
                {
                    ((EventHandler<TabCloseEventArgs>)subscriber).Invoke(this, args);
                    if (args.Cancel)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"TabClosing handler error: {ex}");
                }
            }
        }

        private void RaiseTabClosed(TabCloseEventArgs args)
        {
            EventHandler<TabCloseEventArgs>? handler = TabClosed;
            if (handler is null)
            {
                return;
            }

            foreach (Delegate subscriber in handler.GetInvocationList())
            {
                try
                {
                    ((EventHandler<TabCloseEventArgs>)subscriber).Invoke(this, args);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"TabClosed handler error: {ex}");
                }
            }
        }

        private static TabMenuItem? ResolveTabItem(RoutedEventArgs e)
        {
            if (GetCommandParameter(e) is TabMenuItem param)
            {
                return param;
            }

            if (e.Source is TabMenuItem source)
            {
                return source;
            }

            return e.OriginalSource is DependencyObject original
                ? FindAncestor<TabMenuItem>(original)
                : null;
        }

        private static object? GetCommandParameter(RoutedEventArgs e)
        {
            return e switch
            {
                ExecutedRoutedEventArgs executed => executed.Parameter,
                CanExecuteRoutedEventArgs canExecute => canExecute.Parameter,
                _ => null
            };
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current is not null)
            {
                if (current is T match)
                {
                    return match;
                }

                current = VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);
            }

            return null;
        }

        private bool ContainsTab(TabMenuItem tabItem)
        {
            return Items.Contains(tabItem) || ItemContainerGenerator.ItemFromContainer(tabItem) != DependencyProperty.UnsetValue;
        }

        private object GetItemForTab(TabMenuItem tabItem)
        {
            object item = ItemContainerGenerator.ItemFromContainer(tabItem);
            return item == DependencyProperty.UnsetValue ? tabItem : item;
        }

        private bool RemoveItem(object item)
        {
            if (ItemsSource == null)
            {
                Items.Remove(item);
                return true;
            }

            if (ItemsSource is IList list)
            {
                if (!list.Contains(item))
                {
                    return false;
                }

                list.Remove(item);
                return true;
            }

            var removeMethod = ItemsSource.GetType()
                .GetMethods()
                .FirstOrDefault(method =>
                    method.Name == "Remove"
                    && method.GetParameters() is { Length: 1 } parameters
                    && parameters[0].ParameterType.IsInstanceOfType(item));

            if (removeMethod == null)
            {
                return false;
            }

            object? result = removeMethod.Invoke(ItemsSource, new[] { item });
            return result is not bool removed || removed;
        }

        /// <summary>
        /// 释放已关闭页签持有的引用。<paramref name="disposeContent"/> 为 true 时才对实现
        /// <see cref="IDisposable"/> 的 DataContext / Content 调用 Dispose——Dispose 是对消费者
        /// 对象的破坏性副作用，默认（<see cref="DisposeContentOnClose"/> = false）不做，由调用方自行管理。
        /// </summary>
        private static void CleanupTabItem(TabMenuItem tabItem, bool disposeContent)
        {
            try
            {
                if (tabItem.Content is FrameworkElement fe)
                {
                    if (disposeContent && fe.DataContext is IDisposable disposableDataContext)
                    {
                        try
                        {
                            disposableDataContext.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError($"TabMenu.Dispose error: {ex}");
                        }
                    }

                    fe.DataContext = null;
                }

                if (disposeContent && tabItem.Content is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"TabMenu.Dispose error: {ex}");
                    }
                }

                tabItem.Content = null;
                tabItem.DataContext = null;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"TabMenu.Cleanup error: {ex}");
            }
        }
    }

    public class TabCloseEventArgs : RoutedEventArgs
    {
        public TabMenuItem Tab { get; }

        public bool Cancel { get; set; }

        public TabCloseEventArgs(TabMenuItem tab)
        {
            Tab = tab ?? throw new ArgumentNullException(nameof(tab));
        }
    }
}
