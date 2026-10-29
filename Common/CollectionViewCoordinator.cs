using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace Junevy.Controls.Common;

/// <summary>
/// 单个 <see cref="ICollectionView"/> 的过滤器所有权协调器。
/// <para>
/// <see cref="ICollectionView.Filter"/> 是单槽位，而默认视图按数据源单例：多个库内行为
/// 各自快照并覆写它会互相抹掉对方的过滤条件。协调器独占该槽位，按固定顺序组合
/// 「宿主过滤器 → 分页窗口」，并把宿主过滤器的识别从「一次性快照」改为「每轮重读」。
/// </para>
/// <para>
/// 职责边界：只管 Filter 槽位。数据源身份守卫、ItemsSource 订阅、Loaded/Unloaded 生命周期、
/// 源内容脏标记都留在各行为自身——它们与控件生命周期绑定，不属于视图。
/// </para>
/// <para>
/// 按视图 keyed 存于 <see cref="ConditionalWeakTable{K,V}"/>：视图不是 DependencyObject，
/// 无法挂附加 DP。CWT 的 ephemeron 语义保证「协调器 → 参与者 → 控件 → ItemsSource → 视图」
/// 的间接回指不会阻止回收（R8 / R8-ctrl 钉住）。但参与者的 <c>CollectionChanged</c> 订阅是 CWT 切不断的强根
/// （rows.CollectionChanged → PagingBehavior → 控件）：源存活时未先设 <c>PageSize=0</c> 就丢弃的控件不会回收、页窗口留在共享视图上（限制 ②）。
/// </para>
/// </summary>
internal sealed class CollectionViewCoordinator
{
    private static readonly ConditionalWeakTable<ICollectionView, CollectionViewCoordinator> Instances = new();

    private readonly ICollectionView _view;
    private readonly Predicate<object> _hostOnlyFilter;
    private readonly Predicate<object> _composedFilter;
    private readonly List<IPagingFilterStage> _stages = new();

    private Predicate<object>? _hostFilter;
    private bool _updating;
    private bool _viewHooked;

    private CollectionViewCoordinator(ICollectionView view)
    {
        _view = view;
        _hostOnlyFilter = ApplyHostFilterOnly;
        _composedFilter = ApplyComposedFilter;
    }

    /// <summary>取得（或创建）该视图的协调器。同一视图恒返回同一实例。</summary>
    internal static CollectionViewCoordinator For(ICollectionView view)
    {
        return Instances.GetValue(view, v => new CollectionViewCoordinator(v));
    }

    /// <summary>被协调的视图。参与者枚举它即得「宿主过滤 + 排序」后的 population。</summary>
    internal ICollectionView View => _view;

    /// <summary>注册分页阶段。首个注册者触发接管：快照宿主过滤器并订阅视图变更。</summary>
    internal void Register(IPagingFilterStage stage)
    {
        if (_stages.Contains(stage))
        {
            return;
        }

        if (_stages.Count == 0)
        {
            // 首次接管：此刻槽位里必然是宿主的过滤器（上一轮 Unregister 已交还）
            _hostFilter = _view.Filter;
            _view.CollectionChanged += OnViewCollectionChanged;
            _viewHooked = true;
        }

        _stages.Add(stage);
    }

    /// <summary>注销分页阶段。最后一个注销者把过滤器交还宿主并解除订阅。</summary>
    internal void Unregister(IPagingFilterStage stage)
    {
        if (!_stages.Remove(stage) || _stages.Count > 0)
        {
            return;
        }

        // 先摘变更回调再交还过滤条件：给 Filter 赋值会触发 Refresh，
        // 回调若还挂着会立刻把组合过滤重新算回去
        if (_viewHooked)
        {
            _view.CollectionChanged -= OnViewCollectionChanged;
            _viewHooked = false;
        }

        _view.Filter = _hostFilter;
        _hostFilter = null;
    }

    /// <summary>完整重算：重读宿主过滤器 → 只留宿主过滤让参与者算窗口 → 启用组合过滤。</summary>
    internal void Refresh()
    {
        if (_updating)
        {
            return;
        }

        _updating = true;
        try
        {
            // 宿主可能在上一次重算之后换了 view.Filter：不是本协调器的两个谓词就重新快照。
            // 只在接管时快照一次，会让旧快照把宿主的新条件覆写回去（探针 P1/P8）。
            // 两个谓词都要认：宿主过滤器在第一趟抛异常时槽位会停在 _hostOnlyFilter 上，
            // 只认组合谓词会把自己的第一趟谓词快照成宿主的，下一轮 ApplyHostFilterOnly
            // 便无界自调用（不可捕获的 StackOverflow，探针 T4-throw）
            var currentFilter = _view.Filter;
            if (!ReferenceEquals(currentFilter, _composedFilter) && !ReferenceEquals(currentFilter, _hostOnlyFilter))
            {
                _hostFilter = currentFilter;
            }

            // 第一趟：仅保留宿主过滤（排序已生效），参与者据此统计总数并算页窗口。
            // 用索引遍历而非 foreach：参与者可能在 Prepare 途中注销（宿主换 ItemsSource），
            // _stages 会在遍历期间被修改（设计意图，R6 未实测该交错）
            _view.Filter = _hostOnlyFilter;
            for (int i = 0; i < _stages.Count; i++)
            {
                _stages[i].Prepare();
            }

            // 参与者可能在 Prepare 途中换掉数据源并注销：此时已不再持有该视图，
            // 不得把组合过滤器写回去（探针 R6 钉住本早退守卫）
            if (_stages.Count == 0)
            {
                return;
            }

            // 第二趟：启用组合过滤
            _view.Filter = _composedFilter;
        }
        finally
        {
            _updating = false;
        }
    }

    private void OnViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 自身重算引发的 Refresh 由 Refresh 顶部的 _updating 守卫吸收
        Refresh();
    }

    private bool ApplyHostFilterOnly(object item)
    {
        return _hostFilter?.Invoke(item) ?? true;
    }

    private bool ApplyComposedFilter(object item)
    {
        if (!ApplyHostFilterOnly(item))
        {
            return false;
        }

        // 同样用索引遍历：组合过滤生效时引发的 Refresh 里，宿主处理器也可能换源并注销参与者（设计意图）
        for (int i = 0; i < _stages.Count; i++)
        {
            if (!_stages[i].Accept(item))
            {
                return false;
            }
        }

        return true;
    }
}
