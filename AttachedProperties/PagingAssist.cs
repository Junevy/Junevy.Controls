using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Junevy.Controls.Common;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// 条目控件客户端分页附加属性集。对任意 <see cref="ItemsControl"/>（含 ListBox / ListView /
    /// DataGrid 及其官方原生实例）生效：设置 <see cref="PageSize"/> 后按页过滤条目，
    /// 并通过 <see cref="PageCount"/> / <see cref="TotalCount"/> / <see cref="CurrentPage"/>
    /// 向页码控件（如 <c>jv:DataPager</c>，已内置于 ListBox / ListView / DataGrid 模板页脚）暴露分页状态。
    /// <para>
    /// 实现机制：视图 Filter 槽位由 <c>CollectionViewCoordinator</c> 独占，按
    /// 「宿主过滤器 → 当前页成员」组合；宿主过滤器每轮重读，因此宿主在分页启用后
    /// 更换过滤条件同样生效。排序发生在过滤之前，
    /// 因此 DataGrid 列头排序为「全局排序后再分页」的正确语义。
    /// </para>
    /// <para>
    /// 注意：这是客户端分页——数据需整体在内存中；宿主过滤器与分页可以共存，
    /// 分页启用后更换过滤条件同样生效；两台启用分页的控件不可共享同一视图（同一集合或同一
    /// <see cref="ICollectionView"/>）：可见数会被另一台污染，<see cref="TotalCount"/> 不受影响；
    /// 同源多控件时用 <see cref="CollectionViewSource"/> 为其一建独立视图即可，或各持一份集合；
    /// 丢弃启用分页的控件前请先设 <see cref="PageSize"/>=0，否则页窗口会留在共享视图上，
    /// 之后同一数据源上的新控件会显示空列表；
    /// 翻页会触发视图刷新，但滚动偏移按条目索引保留，不会跳回顶部。
    /// </para>
    /// </summary>
    public class PagingAssist
    {
        private static readonly DependencyProperty BehaviorProperty =
            DependencyProperty.RegisterAttached(
                "Behavior",
                typeof(PagingBehavior),
                typeof(PagingAssist),
                new PropertyMetadata(null));

        /// <summary>每页条数；0（默认）表示不启用分页，负数非法。</summary>
        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.RegisterAttached(
                "PageSize",
                typeof(int),
                typeof(PagingAssist),
                new PropertyMetadata(0, OnPageSizeChanged),
                value => (int)value >= 0);

        /// <summary>当前页码（从 1 起；外部可写以程序化翻页，越界自动钳制）。</summary>
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.RegisterAttached(
                "CurrentPage",
                typeof(int),
                typeof(PagingAssist),
                new PropertyMetadata(1, OnCurrentPageChanged));

        /// <summary>总页数（只读，随数据与 PageSize 自动计算）。</summary>
        private static readonly DependencyPropertyKey PageCountPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "PageCount",
                typeof(int),
                typeof(PagingAssist),
                new PropertyMetadata(1));

        /// <summary>标识 <see cref="GetPageCount"/> 只读附加属性。</summary>
        public static readonly DependencyProperty PageCountProperty = PageCountPropertyKey.DependencyProperty;

        /// <summary>条目总数（只读，宿主过滤后的总数）。</summary>
        private static readonly DependencyPropertyKey TotalCountPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "TotalCount",
                typeof(int),
                typeof(PagingAssist),
                new PropertyMetadata(0));

        /// <summary>标识 <see cref="GetTotalCount"/> 只读附加属性。</summary>
        public static readonly DependencyProperty TotalCountProperty = TotalCountPropertyKey.DependencyProperty;

        /// <summary>页码栏停靠方位（Bottom/Top/Left/Right），由模板页脚读取。</summary>
        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.RegisterAttached(
                "Placement",
                typeof(PagingPlacement),
                typeof(PagingAssist),
                new PropertyMetadata(PagingPlacement.Bottom));

        /// <summary>分页是否启用（只读，PageSize 大于 0 时为 true，模板页脚据此显隐）。</summary>
        private static readonly DependencyPropertyKey HasPagerPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "HasPager",
                typeof(bool),
                typeof(PagingAssist),
                new PropertyMetadata(false));

        /// <summary>标识 <see cref="GetHasPager"/> 只读附加属性。</summary>
        public static readonly DependencyProperty HasPagerProperty = HasPagerPropertyKey.DependencyProperty;

        public static int GetPageSize(DependencyObject obj)
        {
            return (int)obj.GetValue(PageSizeProperty);
        }

        public static void SetPageSize(DependencyObject obj, int value)
        {
            obj.SetValue(PageSizeProperty, value);
        }

        public static int GetCurrentPage(DependencyObject obj)
        {
            return (int)obj.GetValue(CurrentPageProperty);
        }

        public static void SetCurrentPage(DependencyObject obj, int value)
        {
            obj.SetValue(CurrentPageProperty, value);
        }

        public static int GetPageCount(DependencyObject obj)
        {
            return (int)obj.GetValue(PageCountProperty);
        }

        private static void SetPageCount(DependencyObject obj, int value)
        {
            obj.SetValue(PageCountPropertyKey, value);
        }

        public static int GetTotalCount(DependencyObject obj)
        {
            return (int)obj.GetValue(TotalCountProperty);
        }

        private static void SetTotalCount(DependencyObject obj, int value)
        {
            obj.SetValue(TotalCountPropertyKey, value);
        }

        public static PagingPlacement GetPlacement(DependencyObject obj)
        {
            return (PagingPlacement)obj.GetValue(PlacementProperty);
        }

        public static void SetPlacement(DependencyObject obj, PagingPlacement value)
        {
            obj.SetValue(PlacementProperty, value);
        }

        public static bool GetHasPager(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasPagerProperty);
        }

        private static void SetHasPager(DependencyObject obj, bool value)
        {
            obj.SetValue(HasPagerPropertyKey, value);
        }

        private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int pageSize = (int)e.NewValue;
            SetHasPager(d, pageSize > 0);

            if (d is not ItemsControl control)
            {
                return;
            }

            if (pageSize > 0)
            {
                GetOrCreateBehavior(control).Attach(pageSize);
            }
            else
            {
                GetBehavior(control)?.Detach();
                SetBehavior(control, null);
            }
        }

        private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 页码已在附加属性上生效（含外部程序化翻页），此处仅触发重算
            GetBehavior(d)?.Recompute();
        }

        private static PagingBehavior? GetBehavior(DependencyObject d)
        {
            return (PagingBehavior?)d.GetValue(BehaviorProperty);
        }

        private static PagingBehavior GetOrCreateBehavior(ItemsControl control)
        {
            if (GetBehavior(control) is not PagingBehavior behavior)
            {
                behavior = new PagingBehavior(control);
                control.SetValue(BehaviorProperty, behavior);
            }

            return behavior;
        }

        private static void SetBehavior(DependencyObject d, PagingBehavior? value)
        {
            d.SetValue(BehaviorProperty, value);
        }

        /// <summary>
        /// 单个条目控件的分页状态机：监听 ItemsSource / 数据变化，维护页码钳制与只读状态
        /// （PageCount / TotalCount）。视图 Filter 槽位的所有权归
        /// <see cref="CollectionViewCoordinator"/>，本类作为 <see cref="IPagingFilterStage"/> 向其注册。
        /// <para>
        /// 订阅分两层：<b>生命周期层</b>（Loaded / Unloaded）只在分页启用期间挂载，关闭分页必须
        /// 一并解除，否则控件再次进入视觉树时会由旧实例复活过滤；<b>数据层</b>
        /// （ItemsSource 描述符）在卸载期间解除以避免 DependencyPropertyDescriptor 泄漏，
        /// 视图的 CollectionChanged 保留用于记账，重新加载时仅在数据确实变过的情况下重算。
        /// </para>
        /// </summary>
        internal sealed class PagingBehavior : IPagingFilterStage
        {
            private readonly ItemsControl _control;
            private readonly DependencyPropertyDescriptor _itemsSourceDescriptor;

            private CollectionViewCoordinator? _coordinator;
            private object? _attachedSource;

            // 页成员按引用相等判定：值相等（装箱 struct / string / record）的不同实例
            // 不得因 Equals 相同而被窗口外的副本连带放行，否则一页会超出 PageSize
            private HashSet<object>? _pageItems = new(ReferenceEqualityComparer.Instance);
            private int _pageSize;
            private bool _lifecycleHooked;
            private bool _sourceHooked;

            // 源内容脏标记：INotifyCollectionChanged 的所有增删重置（无论是否被当前
            // 分页过滤器放行）都置位，重算成功后清零。Loaded 时据此跳过无变化的重复重算，
            // 而被过滤器挡住、视图不抛 CollectionChanged 的变化仍能在重进视觉树时被补算
            // （探针 R2 普通形态 / R10 CVS 形态；「无变化重载不白跑」由 T5-dirty 钉住）。
            private INotifyCollectionChanged? _sourceNotifier;
            private bool _sourceDirty = true;

            internal PagingBehavior(ItemsControl control)
            {
                _control = control;
                _pageSize = GetPageSize(control);
                _itemsSourceDescriptor = DependencyPropertyDescriptor.FromProperty(
                    ItemsControl.ItemsSourceProperty, typeof(ItemsControl));
            }

            /// <summary>启用（或调整）分页：挂载订阅并按当前状态重算。</summary>
            internal void Attach(int pageSize)
            {
                _pageSize = pageSize;
                HookLifecycle();
                HookSourceChanges();
                Recompute();
            }

            /// <summary>关闭分页：解除全部订阅并把视图过滤器交还宿主。</summary>
            internal void Detach()
            {
                UnhookLifecycle();
                UnhookSourceChanges();
                UnhookSourceNotifier();
                ReleaseCoordinator();

                _pageItems = null;
                SetPageCount(_control, 1);
                SetTotalCount(_control, 0);
            }

            /// <summary>解除对当前视图的接管。协调器在最后一个参与者注销时交还过滤器。</summary>
            private void ReleaseCoordinator()
            {
                if (_coordinator is null)
                {
                    return;
                }

                _coordinator.Unregister(this);
                _coordinator = null;
            }

            private void HookLifecycle()
            {
                if (_lifecycleHooked)
                {
                    return;
                }

                _lifecycleHooked = true;
                _control.Loaded += OnControlLoaded;
                _control.Unloaded += OnControlUnloaded;
            }

            private void UnhookLifecycle()
            {
                if (!_lifecycleHooked)
                {
                    return;
                }

                _lifecycleHooked = false;
                _control.Loaded -= OnControlLoaded;
                _control.Unloaded -= OnControlUnloaded;
            }

            private void HookSourceChanges()
            {
                if (_sourceHooked)
                {
                    return;
                }

                _sourceHooked = true;
                _itemsSourceDescriptor.AddValueChanged(_control, OnItemsSourceChanged);
                EnsureCoordinator();
            }

            private void UnhookSourceChanges()
            {
                if (!_sourceHooked)
                {
                    return;
                }

                // 只摘 ItemsSource 描述符；源内容脏标记订阅保留——卸载期间的增删
                // （尤其被分页过滤器挡住、视图不通知的）仍要置脏，重进视觉树才能补算
                _sourceHooked = false;
                _itemsSourceDescriptor.RemoveValueChanged(_control, OnItemsSourceChanged);
            }

            private void OnControlLoaded(object? sender, RoutedEventArgs e)
            {
                HookSourceChanges();

                // 重新进入视觉树时仅在源内容确实变过的情况下重算：
                // 视图对未通过当前过滤器的增删并不保证抛出 CollectionChanged（本库过滤器
                // 就把它挡在外面），这一盲区由源脏标记兜住（探针 R2 / R10）；
                // 无变化的重算会白跑两趟视图刷新（探针 T5-dirty：0 趟 vs 变异后 2 趟）。
                if (_sourceDirty)
                {
                    Recompute();
                }
            }

            private void OnControlUnloaded(object? sender, RoutedEventArgs e)
            {
                UnhookSourceChanges();
            }

            private void OnItemsSourceChanged(object? sender, EventArgs e)
            {
                // 数据源更换：旧视图连同其上的接管一起废弃，由 EnsureCoordinator 重建
                ReleaseCoordinator();
                Recompute();
            }

            private void EnsureCoordinator()
            {
                var source = _control.ItemsSource;
                if (source is null)
                {
                    ReleaseCoordinator();
                    UnhookSourceNotifier();
                    _sourceDirty = true;
                    return;
                }

                // 守卫必须比较「接管时记录的 source 引用」，不能用 view.SourceCollection：
                // 宿主绑 ICollectionView（如 CVS.View）时 SourceCollection 是底层集合、
                // 与 ItemsSource 不是同一对象，用后者会让守卫在 CVS 形态下恒假，每轮重算
                // 都多余地走 Unregister（交还过滤器）+ Register 接管 + Refresh，徒增视图刷新。
                // 探针 R12 钉住这条：CVS 形态一次换页 2 趟 Reset，退回 SourceCollection 比较后变 3 趟
                // （变异 M1 实测）；P10a 只钉住引用身份关系本身，普通形态下两个引用相等抓不到。
                if (_coordinator is not null && ReferenceEquals(_attachedSource, source))
                {
                    return;
                }

                ReleaseCoordinator();
                _attachedSource = source;
                _coordinator = CollectionViewCoordinator.For(CollectionViewSource.GetDefaultView(source));
                _coordinator.Register(this);

                // 新视图尚未按当前页统计过：挂源内容脏标记并强制下一轮重算
                UnhookSourceNotifier();
                _sourceDirty = true;
                HookSourceNotifier();
            }

            private void HookSourceNotifier()
            {
                // 脏标记必须问底层源、不能问视图：被分页过滤器拒绝的增删，视图不抛
                // CollectionChanged（CVS 形态下 ItemsSource 就是视图本身）。探针 R10 钉住
                // 「视图不抛、源抛」这一事实（viewEvents=0 sourceEvents=30）；订阅目标本身
                // 由 T5-dirty 钉住——变异 M12 实测：退回订阅视图后 R10 仍绿、只有 T5-dirty
                // 转红，源内容没变的重载从 0 趟 Reset 变 2 趟，「未变化不重算」的优化失效。
                // 读码推断（未由用例钉住）：视图自己第二趟写 Filter 引发的 Reset 会把
                // Prepare 刚清掉的脏标记重新置位。
                if (_coordinator?.View.SourceCollection is INotifyCollectionChanged notifier && !ReferenceEquals(_sourceNotifier, notifier))
                {
                    UnhookSourceNotifier();
                    _sourceNotifier = notifier;
                    notifier.CollectionChanged += OnSourceContentChanged;
                }
            }

            private void UnhookSourceNotifier()
            {
                if (_sourceNotifier is null)
                {
                    return;
                }

                _sourceNotifier.CollectionChanged -= OnSourceContentChanged;
                _sourceNotifier = null;
            }

            private void OnSourceContentChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                // 仅记账不重算：多数变化会经视图 CollectionChanged 走 Recompute；
                // 被分页过滤器挡住的变化（视图不通知）由该标记在 Loaded 时补算
                _sourceDirty = true;
            }

            internal void Recompute()
            {
                EnsureCoordinator();

                if (_coordinator is null)
                {
                    SetTotalCount(_control, 0);
                    SetPageCount(_control, 1);
                    _pageItems = new HashSet<object>(ReferenceEqualityComparer.Instance);
                    return;
                }

                _coordinator.Refresh();
            }

            /// <summary>
            /// 分页第一趟：视图已应用宿主过滤且保持排序，据此统计总数、钳制页码、收集当前页成员。
            /// 页成员按引用相等判定：值相等（装箱 struct / string / record）的不同实例
            /// 不得因 Equals 相同而被窗口外的副本连带放行，否则一页会超出 PageSize。
            /// </summary>
            public void Prepare()
            {
                var view = _coordinator!.View;
                int pageSize = Math.Max(1, _pageSize);

                int total = 0;
                foreach (object item in view)
                {
                    total++;
                }

                int pageCount = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
                int currentPage = Math.Max(1, GetCurrentPage(_control));
                if (currentPage > pageCount)
                {
                    currentPage = pageCount;

                    // SetCurrentValue 而非 SetValue：写值不占用本地值优先级，
                    // 宿主对 CurrentPage 的（含 OneWay）绑定在钳制之后仍能继续驱动翻页
                    _control.SetCurrentValue(CurrentPageProperty, currentPage);
                }

                int start = (currentPage - 1) * pageSize;
                var pageItems = new HashSet<object>(ReferenceEqualityComparer.Instance);
                int index = 0;
                foreach (object item in view)
                {
                    if (index >= start && index < start + pageSize)
                    {
                        pageItems.Add(item);
                    }

                    index++;
                    if (index >= start + pageSize)
                    {
                        break;
                    }
                }

                _pageItems = pageItems;
                SetTotalCount(_control, total);
                SetPageCount(_control, pageCount);
                _sourceDirty = false;
            }

            /// <summary>分页谓词：该条目是否落在当前页窗口内。</summary>
            public bool Accept(object item)
            {
                return _pageItems?.Contains(item) ?? false;
            }
        }

        /// <summary>
        /// 引用相等比较器：页成员判定必须按实例身份而非值相等，
        /// 否则值相等（装箱 struct / string / record）的窗外副本会被 HashSet 连带放行。
        /// </summary>
        internal sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceEqualityComparer Instance = new();

            private ReferenceEqualityComparer()
            {
            }

            public new bool Equals(object? x, object? y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
