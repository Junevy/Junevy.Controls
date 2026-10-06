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
        /// 关闭页签时的内容释放开关（默认 false = 库不做任何清理，生命周期由调用方管理）。开启后：
        /// - **直接声明页签**（页签自身即数据项）：清空其 <c>Content</c>/<c>DataContext</c>，并对内容本身及其
        ///   <c>DataContext</c> 实现 <see cref="IDisposable"/> 的部分调用 Dispose（仅默认关闭态下同一页签可重新加回）；
        /// - **<c>ItemsSource</c> 条目**（条目即数据模型）：模型本身或条目元素 <c>DataContext</c>
        ///   实现 <see cref="IDisposable"/> 时调用 Dispose（容器随条目移除一并丢弃，无重新加回语义）。
        /// <para><b>处置 <c>DataContext</c> 不区分本地值与继承值</b>：未本地设置 <c>DataContext</c> 的直接声明页签，
        /// 其 <c>DataContext</c> 是从窗口继承来的视图模型——它一旦实现 <see cref="IDisposable"/>，
        /// 关掉任意一个页签就会把它销毁。需要本开关时，请给页签显式设本地 <c>DataContext</c>，
        /// 或确保视图模型不实现 <see cref="IDisposable"/>。</para>
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
        /// 关闭指定页签。页签不属于本控件、或 <see cref="TabControlItem.CanClose"/> 为 <c>false</c> 时静默忽略；
        /// <see cref="TabClosing"/> 被取消时不移除。
        /// 编辑态（<see cref="TabControlItem.IsEditing"/>）的页签由命令路径拒绝关闭，本方法同样拒绝。
        /// </summary>
        public void CloseTab(TabControlItem? tabItem)
        {
            if (tabItem is null)
            {
                throw new ArgumentNullException(nameof(tabItem));
            }

            if (!ContainsTab(tabItem) || tabItem.IsEditing || !tabItem.CanClose)
            {
                return;
            }

            CloseTabInternal(tabItem);
        }

        private void OnCanCloseTabCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            TabControlItem? tabItem = ResolveTabItem(e);
            e.CanExecute = tabItem is not null
                && tabItem.CanClose
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

            if (!ContainsTab(tabItem) || !tabItem.CanClose)
            {
                return;
            }

            // 数据项快照：ItemsSource 路径下条目移除后生成器会反准备容器，其 Content / DataContext
            // 不再指向数据项（读回的是 WPF 的未设置占位），之后 TabClosed 再也取不到条目本身，故先取住。
            object item = GetItemForTab(tabItem);

            TabCloseEventArgs closing = new(TabClosingEvent, this, tabItem, item);
            RaiseEvent(closing);
            if (closing.Cancel)
            {
                return;
            }

            // 宿主可能在 TabClosing 里自行移除了数据项而未置 Cancel：页签此时已消失，补派 TabClosed 收尾。
            // 判定不只看容器映射——生成器只在数据源发 INotifyCollectionChanged 通知后才更新映射，
            // 非通知型数据源（如只读 IList 包装）上它并不知道条目已走，故同时问一次集合本身。
            if (!TabStillPresent(tabItem, item))
            {
                RaiseEvent(new TabCloseEventArgs(TabClosedEvent, this, tabItem, item));
                return;
            }

            // 可写性必须赶在任何状态变更（选中转移）与延迟派发之前校验：否则不可写源会先跑完
            // TabClosing 处理程序、改掉选中，最后才抛异常；走延迟路径时异常还会抛在 Dispatcher 回调里，
            // 变成无人处理的应用程序级异常。此处抛出不影响「TabClosing + Cancel」自管集合模式——
            // 那种模式在上面的 Cancel 分支已经返回。
            EnsureRemovable();

            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                PerformClose(tabItem, item);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => PerformClose(tabItem, item)), DispatcherPriority.Background);
            }
        }

        private void PerformClose(TabControlItem tabItem, object itemToRemove)
        {
            // 延迟派发期间页签可能已被移除，执行前需重新校验
            if (!TabStillPresent(tabItem, itemToRemove))
            {
                return;
            }

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

            // ItemsSource 条目的释放必须在 RemoveItem 之前：条目移除时生成器会「反准备」容器，
            // 其 Content / DataContext 不再指向条目对象，之后将读不到条目（自容器路径的 Content 是宿主自设的，无此问题）。
            if (DisposeContentOnClose && !removedSelfAsItem)
            {
                CleanupItemContent(tabItem);
            }

            RemoveItem(itemToRemove);

            if (removedSelfAsItem && DisposeContentOnClose)
            {
                CleanupTabItem(tabItem);
            }

            RaiseEvent(new TabCloseEventArgs(TabClosedEvent, this, tabItem, itemToRemove));
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

        /// <summary>
        /// 页签容器与其数据项是否都还在。除容器映射外再问一次集合：数据源不发集合变更通知时
        /// 生成器会一直留着旧映射，只看它会把「条目其实已被宿主移除」误判成仍在。
        /// </summary>
        private bool TabStillPresent(TabControlItem tabItem, object item)
        {
            return ContainsTab(tabItem) && Items.Contains(item);
        }

        private object GetItemForTab(TabControlItem tabItem)
        {
            object item = ItemContainerGenerator.ItemFromContainer(tabItem);
            return item == DependencyProperty.UnsetValue ? tabItem : item;
        }

        /// <summary>
        /// 从数据源移除页签。自容器声明（<c>ItemsSource</c> 为 null）直接移除页签本身；
        /// <c>ItemsSource</c> 路径需数据源可写，由 <see cref="EnsureRemovable"/> 校验。
        /// </summary>
        private void RemoveItem(object item)
        {
            if (ItemsSource is null)
            {
                Items.Remove(item);
                return;
            }

            // 延迟派发期间 ItemsSource 可能被换成不可写集合，移除前再校验一次
            EnsureRemovable();
            ((IList)ItemsSource).Remove(item);
        }

        /// <summary>
        /// 校验 <c>ItemsSource</c> 可被控件直接移除。不可写源（数组、只读包装、LINQ 投影等）抛出可定位的异常，
        /// 而不是静默失败——需要自管集合的宿主走 <see cref="TabClosing"/> + <c>Cancel</c> 模式，那条路径不经过此校验。
        /// </summary>
        private void EnsureRemovable()
        {
            if (ItemsSource is not null && ItemsSource is not IList { IsReadOnly: false, IsFixedSize: false })
            {
                throw new InvalidOperationException(
                    "无法关闭页签：ItemsSource 不可写。请改绑 ObservableCollection<T>，" +
                    "或在 TabClosing 处理程序中从自己的集合移除数据项并置 e.Cancel = true。");
            }
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

        /// <summary>
        /// <c>ItemsSource</c> 条目的释放（<see cref="DisposeContentOnClose"/> = true）：条目即数据模型，
        /// 模型本身实现 <see cref="IDisposable"/> 时调用 Dispose；条目为元素时对其
        /// <c>DataContext</c> 中实现 <see cref="IDisposable"/> 的部分调用 Dispose。
        /// 容器随条目移除一并丢弃，不清空引用（无自容器「重新加回」语义）。
        /// </summary>
        private static void CleanupItemContent(TabControlItem tabItem)
        {
            if (tabItem.Content is FrameworkElement { DataContext: IDisposable dataContext })
            {
                dataContext.Dispose();
            }

            if (tabItem.Content is IDisposable content)
            {
                content.Dispose();
            }
        }
    }

    /// <summary>页签关闭相关路由事件的委托。</summary>
    public delegate void TabCloseEventHandler(object sender, TabCloseEventArgs e);

    /// <summary><see cref="TabControl.TabClosing"/> / <see cref="TabControl.TabClosed"/> 的事件参数。</summary>
    public class TabCloseEventArgs : RoutedEventArgs
    {
        /// <summary>构造事件参数，<see cref="Item"/> 取 <paramref name="tab"/>（仅适用于直接声明页签的用法）。</summary>
        public TabCloseEventArgs(RoutedEvent routedEvent, object source, TabControlItem tab)
            : this(routedEvent, source, tab, tab)
        {
        }

        /// <summary>构造事件参数。</summary>
        public TabCloseEventArgs(RoutedEvent routedEvent, object source, TabControlItem tab, object item)
            : base(routedEvent, source)
        {
            Tab = tab ?? throw new ArgumentNullException(nameof(tab));
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        /// <summary>页签容器。</summary>
        public TabControlItem Tab { get; }

        /// <summary>
        /// 被关闭的数据项（派发前的快照）。<c>ItemsSource</c> 模式下即被移除的集合元素：条目移除后生成器会
        /// 反准备容器，<see cref="Tab"/> 的 <c>Content</c> 与 <c>DataContext</c> 不再指向数据项（读回的是
        /// WPF 的未设置占位 <c>NamedObject</c>），只有这份快照能在 <see cref="TabControl.TabClosed"/>
        /// 里取回数据项本身。直接声明页签时数据项即容器，与 <see cref="Tab"/> 相同。
        /// </summary>
        public object Item { get; }

        /// <summary>在 <see cref="TabControl.TabClosingEvent"/> 中置 <c>true</c> 可阻止关闭；在 Closed 中忽略。</summary>
        public bool Cancel { get; set; }
    }
}
