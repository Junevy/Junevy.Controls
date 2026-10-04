using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// 可关闭、可重命名的页签控件，继承 WPF <see cref="System.Windows.Controls.TabControl"/>。
    /// 页签容器为 <see cref="TabControlItem"/>；关闭经 <see cref="CloseTabCommand"/> 或 <see cref="CloseTab"/>，
    /// 关闭前派发可取消的 <see cref="TabClosing"/>，完成后派发 <see cref="TabClosed"/>。
    /// </summary>
    public class TabControl : System.Windows.Controls.TabControl
    {
        public static readonly RoutedCommand CloseTabCommand = new(nameof(CloseTabCommand), typeof(TabControl));

        #region Routed events

        /// <summary>页签即将关闭时触发，处理程序置 <see cref="TabCloseEventArgs.Cancel"/> 可阻止关闭。</summary>
        public static readonly RoutedEvent TabClosingEvent = EventManager.RegisterRoutedEvent(
            nameof(TabClosing), RoutingStrategy.Direct, typeof(TabCloseEventHandler), typeof(TabControl));

        /// <summary>页签已关闭后触发。<see cref="TabCloseEventArgs.Cancel"/> 在本事件中无效。</summary>
        public static readonly RoutedEvent TabClosedEvent = EventManager.RegisterRoutedEvent(
            nameof(TabClosed), RoutingStrategy.Direct, typeof(TabCloseEventHandler), typeof(TabControl));

        /// <summary>
        /// 采用 <see cref="RoutingStrategy.Direct"/> 而非库内其他事件的 Bubble：关闭语义只属于发起页签所在的
        /// 这台 <c>TabControl</c>。页签内容区里可以嵌套另一台 TabControl，冒泡会让父级收到子级页签的关闭事件，
        /// 父级处理程序里的 <c>Cancel</c> 便可能误拦与它无关的关闭。
        /// </summary>
        public event TabCloseEventHandler TabClosing
        {
            add => AddHandler(TabClosingEvent, value);
            remove => RemoveHandler(TabClosingEvent, value);
        }

        public event TabCloseEventHandler TabClosed
        {
            add => AddHandler(TabClosedEvent, value);
            remove => RemoveHandler(TabClosedEvent, value);
        }

        #endregion

        #region Dependency properties

        /// <summary>是否允许关闭最后一个页签。默认 <c>true</c>（最后一个页签也可被关闭，关闭后无选中页签）。</summary>
        public static readonly DependencyProperty CanCloseLastTabProperty =
            DependencyProperty.Register(nameof(CanCloseLastTab), typeof(bool), typeof(TabControl), new PropertyMetadata(true));

        /// <summary>
        /// 是否在每个页签上显示关闭按钮。仅影响页签外观，页签仍可通过 <see cref="CloseTab"/> 关闭。
        /// </summary>
        public static readonly DependencyProperty IsClosableProperty =
            DependencyProperty.Register(nameof(IsClosable), typeof(bool), typeof(TabControl), new PropertyMetadata(true));

        /// <summary>页签悬停背景画刷（默认 Surface.Hover；模板触发器经 AncestorType 绑定取值）。</summary>
        public static readonly DependencyProperty ItemHoverBackgroundProperty =
            DependencyProperty.Register(nameof(ItemHoverBackground), typeof(Brush), typeof(TabControl), new PropertyMetadata(null));

        /// <summary>选中页签背景画刷（默认 Background.App——选中页签与内容区视觉一体；可自定义）。</summary>
        public static readonly DependencyProperty SelectedItemBackgroundProperty =
            DependencyProperty.Register(nameof(SelectedItemBackground), typeof(Brush), typeof(TabControl), new PropertyMetadata(null));

        /// <summary>
        /// 页签头圆角（模板只取其上两角，与内容区衔接）。
        /// 注册为附加属性：除本控件外，也可挂在官方 <c>System.Windows.Controls.TabControl</c> 上，
        /// 由库主题里的官方 TabControl / 原生 TabItem 样式读取（未赋值时取默认值，不再有绑定失败）。
        /// </summary>
        public static readonly DependencyProperty HeaderCornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                nameof(HeaderCornerRadius),
                typeof(CornerRadius),
                typeof(TabControl),
                new PropertyMetadata(new CornerRadius(4)));

        /// <summary>
        /// 内容区圆角（模板只取其下两角）。附加属性注册的理由同 <see cref="HeaderCornerRadiusProperty"/>。
        /// </summary>
        public static readonly DependencyProperty ContentCornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                nameof(ContentCornerRadius),
                typeof(CornerRadius),
                typeof(TabControl),
                new PropertyMetadata(new CornerRadius(8)));

        /// <summary>
        /// 关闭「直接声明在 XAML 里的页签」（页签自身即数据项）时，是否清空其 <c>Content</c>/<c>DataContext</c>，
        /// 并对内容本身及其 <c>DataContext</c> 实现 <see cref="IDisposable"/> 的部分调用 Dispose。
        /// 默认 <see cref="DisposeContentOnClose"/> = false：页签内容原样保留，同一页签可被重新加回。
        /// </summary>
        public static readonly DependencyProperty DisposeContentOnCloseProperty =
            DependencyProperty.Register(nameof(DisposeContentOnClose), typeof(bool), typeof(TabControl), new PropertyMetadata(false));

        /// <summary>
        /// 控件级双击重命名开关（默认 <c>false</c>，即默认不开放双击页签标题改名）。
        /// 能否进入重命名由两级开关共同决定：控件级 <see cref="CanRename"/> 与条目级
        /// <see cref="TabControlItem.CanRename"/> 同时为 true 才允许。
        /// </summary>
        public static readonly DependencyProperty CanRenameProperty =
            DependencyProperty.Register(nameof(CanRename), typeof(bool), typeof(TabControl), new PropertyMetadata(false));

        #endregion

        static TabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TabControl),
                new FrameworkPropertyMetadata(typeof(TabControl)));
        }

        public TabControl()
        {
            CommandBindings.Add(new CommandBinding(CloseTabCommand, OnCloseTabCommand, OnCanCloseTabCommand));
        }

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

        public Brush ItemHoverBackground
        {
            get => (Brush)GetValue(ItemHoverBackgroundProperty);
            set => SetValue(ItemHoverBackgroundProperty, value);
        }

        public Brush SelectedItemBackground
        {
            get => (Brush)GetValue(SelectedItemBackgroundProperty);
            set => SetValue(SelectedItemBackgroundProperty, value);
        }

        public bool DisposeContentOnClose
        {
            get => (bool)GetValue(DisposeContentOnCloseProperty);
            set => SetValue(DisposeContentOnCloseProperty, value);
        }

        public bool CanRename
        {
            get => (bool)GetValue(CanRenameProperty);
            set => SetValue(CanRenameProperty, value);
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

        /// <summary>读取宿主元素上的页签头圆角（附加属性访问器，供官方 <c>TabControl</c> 复用本主题时使用）。</summary>
        public static CornerRadius GetHeaderCornerRadius(DependencyObject obj)
            => (CornerRadius)obj.GetValue(HeaderCornerRadiusProperty);

        /// <summary>设置页签头圆角；模板只取上两角。</summary>
        public static void SetHeaderCornerRadius(DependencyObject obj, CornerRadius value)
            => obj.SetValue(HeaderCornerRadiusProperty, value);

        /// <summary>读取宿主元素上的内容区圆角（附加属性访问器）。</summary>
        public static CornerRadius GetContentCornerRadius(DependencyObject obj)
            => (CornerRadius)obj.GetValue(ContentCornerRadiusProperty);

        /// <summary>设置内容区圆角；模板只取下两角。</summary>
        public static void SetContentCornerRadius(DependencyObject obj, CornerRadius value)
            => obj.SetValue(ContentCornerRadiusProperty, value);

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TabControlItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TabControlItem;
        }

        /// <summary>
        /// 关闭指定页签。页签不属于本控件时静默忽略；<see cref="TabClosing"/> 被取消时不移除。
        /// 编辑态（<see cref="TabControlItem.IsEditing"/>）的页签由命令路径拒绝关闭，本方法同样拒绝。
        /// </summary>
        public void CloseTab(TabControlItem? tabItem)
        {
            if (tabItem is null)
            {
                throw new ArgumentNullException(nameof(tabItem));
            }

            if (!ContainsTab(tabItem) || tabItem.IsEditing)
            {
                return;
            }

            CloseTabInternal(tabItem);
        }

        private void OnCanCloseTabCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            TabControlItem? tabItem = ResolveTabItem(e);
            e.CanExecute = tabItem is not null
                && !tabItem.IsEditing
                && (CanCloseLastTab || Items.Count > 1);
        }

        private void OnCloseTabCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TabControlItem? tabItem = ResolveTabItem(e);
            if (tabItem is null)
            {
                return;
            }

            if (!tabItem.IsEditing)
            {
                CloseTabInternal(tabItem);
            }

            e.Handled = true;
        }

        private void CloseTabInternal(TabControlItem tabItem)
        {
            if (!CanCloseLastTab && Items.Count <= 1)
            {
                return;
            }

            if (!ContainsTab(tabItem))
            {
                return;
            }

            TabCloseEventArgs closing = new(TabClosingEvent, this, tabItem);
            RaiseEvent(closing);
            if (closing.Cancel)
            {
                return;
            }

            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                PerformClose(tabItem);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => PerformClose(tabItem)), DispatcherPriority.Background);
            }
        }

        private void PerformClose(TabControlItem tabItem)
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
                    // 停在被关闭页签的位置上（后一个顶上来）；关的是最后一个页签时退回前一个。
                    // 移除后最大合法索引为 Items.Count - 2，故夹取到它即可同时覆盖两种情形。
                    int next = Math.Min(index, Items.Count - 2);
                    if (next >= 0)
                    {
                        SelectedIndex = next;
                    }
                }
            }

            bool removedSelfAsItem = ReferenceEquals(itemToRemove, tabItem);
            RemoveItem(itemToRemove);

            if (removedSelfAsItem && DisposeContentOnClose)
            {
                CleanupTabItem(tabItem);
            }

            RaiseEvent(new TabCloseEventArgs(TabClosedEvent, this, tabItem));
        }

        private static TabControlItem? ResolveTabItem(RoutedEventArgs e)
        {
            // CommandParameter 优先：宿主可显式指定要关闭的页签，绕开可视树定位
            if (GetCommandParameter(e) is TabControlItem param)
            {
                return param;
            }

            if (e.Source is TabControlItem source)
            {
                return source;
            }

            return e.OriginalSource is DependencyObject original
                ? FindAncestor<TabControlItem>(original)
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

        internal static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current is not null)
            {
                if (current is T match)
                {
                    return match;
                }

                current = GetParentChain(current);
            }

            return null;
        }

        /// <summary>
        /// <paramref name="source"/> 是否位于 <paramref name="ancestor"/> 的子树内（含自身）。
        /// 可视树走不通时回退逻辑树——模板部件与数据项之间可能只有逻辑父子关系。
        /// </summary>
        internal static bool IsWithinSubtree(DependencyObject? source, DependencyObject? ancestor)
        {
            while (source is not null)
            {
                if (ReferenceEquals(source, ancestor))
                {
                    return true;
                }

                source = GetParentChain(source);
            }

            return false;
        }

        private static DependencyObject? GetParentChain(DependencyObject current)
        {
            return VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);
        }

        private bool ContainsTab(TabControlItem tabItem)
        {
            // 生成器未就绪时 ItemFromContainer 返回 UnsetValue，此时回退到 Items 直查（自容器声明场景）
            return ItemContainerGenerator.ItemFromContainer(tabItem) != DependencyProperty.UnsetValue
                || Items.Contains(tabItem);
        }

        private object GetItemForTab(TabControlItem tabItem)
        {
            object item = ItemContainerGenerator.ItemFromContainer(tabItem);
            return item == DependencyProperty.UnsetValue ? tabItem : item;
        }

        /// <summary>
        /// 从数据源移除页签。不可写的 <c>ItemsSource</c>（数组、LINQ 投影等）抛出可定位的异常，
        /// 而不是静默失败——需要自管集合的宿主应在 <see cref="TabClosing"/> 里移除数据项并置 Cancel。
        /// </summary>
        private void RemoveItem(object item)
        {
            if (ItemsSource is null)
            {
                Items.Remove(item);
                return;
            }

            if (ItemsSource is IList { IsReadOnly: false, IsFixedSize: false } list)
            {
                list.Remove(item);
                return;
            }

            throw new InvalidOperationException(
                "无法关闭页签：ItemsSource 不可写。请改绑 ObservableCollection<T>，" +
                "或在 TabClosing 处理程序中从自己的集合移除数据项并置 e.Cancel = true。");
        }

        /// <summary>
        /// 仅在 <see cref="DisposeContentOnClose"/> 为 true 时调用：清空已关闭页签持有的内容引用，
        /// 并 Dispose 内容本身及其 DataContext。Dispose 是消费者对象的破坏性副作用，故默认不做。
        /// </summary>
        private static void CleanupTabItem(TabControlItem tabItem)
        {
            if (tabItem.Content is FrameworkElement { DataContext: IDisposable dataContext })
            {
                dataContext.Dispose();
            }

            if (tabItem.Content is IDisposable content)
            {
                content.Dispose();
            }

            tabItem.Content = null;
            tabItem.DataContext = null;
        }
    }

    /// <summary>页签关闭相关路由事件的委托。</summary>
    public delegate void TabCloseEventHandler(object sender, TabCloseEventArgs e);

    public class TabCloseEventArgs : RoutedEventArgs
    {
        public TabCloseEventArgs(RoutedEvent routedEvent, object source, TabControlItem tab)
            : base(routedEvent, source)
        {
            Tab = tab ?? throw new ArgumentNullException(nameof(tab));
        }

        public TabControlItem Tab { get; }

        /// <summary>在 <see cref="TabControl.TabClosingEvent"/> 中置 <c>true</c> 可阻止关闭；在 Closed 中忽略。</summary>
        public bool Cancel { get; set; }
    }
}
