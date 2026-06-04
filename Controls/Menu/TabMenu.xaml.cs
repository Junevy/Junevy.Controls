using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Junevy.Controls.Controls.Menu
{
    public class TabMenu : TabControl
    {
        public static readonly RoutedCommand CloseTabCommand = new(nameof(CloseTabCommand), typeof(TabMenu));
        //public static readonly RoutedCommand TestCommand = new(nameof(TestCommand), typeof(TabMenu));

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
            //CommandBindings.Add(new CommandBinding(TestCommand, OnTest, OnCanCloseTabCommand));
        }

        //private void OnTest(object sender, ExecutedRoutedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        public static readonly DependencyProperty CanCloseLastTabProperty =
            DependencyProperty.Register(nameof(CanCloseLastTab), typeof(bool), typeof(TabMenu), new PropertyMetadata(true));

        public bool CanCloseLastTab
        {
            get { return (bool)GetValue(CanCloseLastTabProperty); }
            set { SetValue(CanCloseLastTabProperty, value); }
        }


        public void CloseTab(TabMenuItem? tabItem)
        {
            if (tabItem is null)
            {
                throw new ArgumentNullException(nameof(tabItem));
            }

            if (!Items.Contains(tabItem))
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
                if (tabItem is null)
                {
                    e.CanExecute = false;
                    return;
                }

                if (tabItem.IsEditing)
                {
                    e.CanExecute = false;
                    return;
                }

                if (!CanCloseLastTab && Items.Count <= 1)
                {
                    e.CanExecute = false;
                    return;
                }

                e.CanExecute = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TabMenu.CanCloseTab error: {ex}");
                e.CanExecute = false;
            }
        }

        private void OnCloseTabCommand(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                TabMenuItem? tabItem = ResolveTabItem(e);
                if (tabItem is null) return;

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
                System.Diagnostics.Debug.WriteLine($"TabMenu close command error: {ex}");
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


                if (!Items.Contains(tabItem))
                {
                    return;
                }

                //if (ItemsSource.)

                TabCloseEventArgs args = new(tabItem);
                RaiseTabClosing(args);  // �����ر����¼�
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
                System.Diagnostics.Debug.WriteLine($"TabMenu close error: {ex}");
            }
        }

        /// <summary>
        /// ִ��tabmenuItem�Ĺر�Command
        /// </summary>
        /// <param name="tabItem"></param>
        /// <param name="args"></param>
        private void PerformClose(TabMenuItem tabItem, TabCloseEventArgs args)
        {
            try
            {
                if (tabItem.IsSelected && Items.Count > 1)
                {
                    int index = Items.IndexOf(tabItem);
                    if (index >= 0)
                    {
                        int newIndex = index > 0 ? index - 1 : Math.Min(1, Items.Count - 1);
                        if (newIndex >= 0 && newIndex < Items.Count)
                        {
                            SelectedIndex = newIndex;
                        }
                    }
                }

                if (ItemsSource == null)
                {
                    Items.Remove(tabItem);
                }
                else
                {
                    var context = tabItem.DataContext;
                    if (context == null) return;

                    Type type = context.GetType();
                    PropertyInfo[] property = type.GetProperties();
                    foreach (PropertyInfo prop in property)
                    {
                        if (prop.PropertyType.GetInterfaces().Any(i => i == typeof (IList<TabMenuItem>)))
                        {
                            if (prop.GetValue(context) is IList<TabMenuItem> items && items.Contains(tabItem))
                                items.Remove(tabItem); break;
                        }
                    }
                }
                CleanupTabItem(tabItem);    // ������Դ
                RaiseTabClosed(args);   // Raise �رպ��¼�
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TabMenu close error: {ex}");
            }
        }

        /// <summary>
        /// Raise tabmenu item�ر����¼�
        /// </summary>
        /// <param name="args">�ر��¼�����</param>
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
                    System.Diagnostics.Debug.WriteLine($"TabClosing handler error: {ex}");
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
                    System.Diagnostics.Debug.WriteLine($"TabClosed handler error: {ex}");
                }
            }
        }

        /// <summary>
        /// ���¼�������Command Parameter�л�ȡTabMenuItem���󣨵�ǰ�رն���
        /// </summary>
        /// <param name="e">����CommandʱЯ�����¼����� <see cref="ExecutedRoutedEventArgs"/></param>
        /// <returns></returns>
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

            if (e.OriginalSource is DependencyObject original)
            {
                return FindAncestor<TabMenuItem>(original);
            }

            return null;
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

        private static void CleanupTabItem(TabMenuItem tabItem)
        {
            try
            {
                if (tabItem.Content is FrameworkElement fe)
                {
                    fe.DataContext = null;
                }

                if (tabItem.Content is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"TabMenu.Dispose error: {ex}");
                    }
                }

                tabItem.Content = null;
                tabItem.DataContext = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TabMenu.Cleanup error: {ex}");
            }
        }
    }

    /// <summary>
    /// Tabmenu Item �ر��¼�����
    /// </summary>
    public class TabCloseEventArgs : RoutedEventArgs
    {
        public TabMenuItem Tab { get; }

        public bool Cancel { get; set; }

        public TabCloseEventArgs(TabMenuItem tab)
            : base()
        {
            Tab = tab ?? throw new ArgumentNullException(nameof(tab));
        }
    }
}
