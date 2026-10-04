using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Junevy.Controls.AttachedProperties;

namespace Junevy.Controls.Controls.Paging;

/// <summary>
/// 分页栏控件：首页 / 上一页 / 滑窗页码 / 下一页 / 末页 + 每页条数选择器 + 总数文本。
/// <para>
/// 两种使用方式：① 放入已启用 <c>atc:PagingAssist.PageSize</c> 的条目控件内部
/// （ListBox / ListView / DataGrid 的模板页脚已内置），控件自动绑定最近的
/// <see cref="ItemsControl"/> 祖先的分页附加属性；② 独立摆放时在实例上显式绑定
/// <see cref="CurrentPage"/> / <see cref="PageCount"/> / <see cref="TotalCount"/> /
/// <see cref="PageSize"/>（如经 ElementName 绑到条目控件的附加属性）。
/// </para>
/// <para>
/// 导航按钮经路由命令驱动（与 WPF Toolkit DataPager 一致）：
/// <see cref="MoveToFirstPageCommand"/> / <see cref="MoveToPreviousPageCommand"/> /
/// <see cref="MoveToNextPageCommand"/> / <see cref="MoveToLastPageCommand"/> /
/// <see cref="MoveToPageCommand"/>（参数为目标页码）。CanExecute 自动驱动按钮
/// IsEnabled（不落本地值，宿主样式 setter 不被顶掉），自定义模板时按钮绑定
/// 对应命令即可接入。
/// </para>
/// </summary>
[TemplatePart(Name = PartPagesHost, Type = typeof(ItemsControl))]
[TemplatePart(Name = PartPageSizeEditor, Type = typeof(ComboBox))]
public class DataPager : Control
{
    /// <summary>跳转到首页。</summary>
    public static readonly RoutedCommand MoveToFirstPageCommand = new(nameof(MoveToFirstPageCommand), typeof(DataPager));

    /// <summary>跳转到上一页。</summary>
    public static readonly RoutedCommand MoveToPreviousPageCommand = new(nameof(MoveToPreviousPageCommand), typeof(DataPager));

    /// <summary>跳转到下一页。</summary>
    public static readonly RoutedCommand MoveToNextPageCommand = new(nameof(MoveToNextPageCommand), typeof(DataPager));

    /// <summary>跳转到末页。</summary>
    public static readonly RoutedCommand MoveToLastPageCommand = new(nameof(MoveToLastPageCommand), typeof(DataPager));

    /// <summary>跳转到指定页（<see cref="RoutedCommand"/> 参数为目标页码，越界钳制）。</summary>
    public static readonly RoutedCommand MoveToPageCommand = new(nameof(MoveToPageCommand), typeof(DataPager));

    internal const string PartPagesHost = "PART_PagesHost";
    internal const string PartPageSizeEditor = "PART_PageSizeEditor";

    private readonly ObservableCollection<object> _pageNumbers = [];

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(DataPager),
            new PropertyMetadata(Orientation.Horizontal, OnVisualStateInvalidated));

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(
            nameof(CurrentPage),
            typeof(int),
            typeof(DataPager),
            new PropertyMetadata(1, OnPageStateChanged));

    public static readonly DependencyProperty PageCountProperty =
        DependencyProperty.Register(
            nameof(PageCount),
            typeof(int),
            typeof(DataPager),
            new PropertyMetadata(1, OnPageStateChanged));

    public static readonly DependencyProperty TotalCountProperty =
        DependencyProperty.Register(
            nameof(TotalCount),
            typeof(int),
            typeof(DataPager),
            new PropertyMetadata(0, OnPageStateChanged));

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(
            nameof(PageSize),
            typeof(int),
            typeof(DataPager),
            new PropertyMetadata(0, OnPageSizeChanged),
            value => (int)value >= 0);

    public static readonly DependencyProperty PageSizeOptionsProperty =
        DependencyProperty.Register(
            nameof(PageSizeOptions),
            typeof(string),
            typeof(DataPager),
            new PropertyMetadata("10,20,50,100", OnPageSizeOptionsChanged));

    /// <summary>页码滑窗宽度（窗口内同时可见的页码数，最小 1）。</summary>
    public static readonly DependencyProperty ButtonCountProperty =
        DependencyProperty.Register(
            nameof(ButtonCount),
            typeof(int),
            typeof(DataPager),
            new PropertyMetadata(5, OnPageStateChanged),
            value => (int)value >= 1);

    static DataPager()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DataPager),
            new FrameworkPropertyMetadata(typeof(DataPager)));
    }

    public DataPager()
    {
        CommandBindings.Add(new CommandBinding(MoveToFirstPageCommand, OnMoveToFirstPage, CanMoveToFirstPage));
        CommandBindings.Add(new CommandBinding(MoveToPreviousPageCommand, OnMoveToPreviousPage, CanMoveToPreviousPage));
        CommandBindings.Add(new CommandBinding(MoveToNextPageCommand, OnMoveToNextPage, CanMoveToNextPage));
        CommandBindings.Add(new CommandBinding(MoveToLastPageCommand, OnMoveToLastPage, CanMoveToLastPage));
        CommandBindings.Add(new CommandBinding(MoveToPageCommand, OnMoveToPage, CanMoveToPage));
        Loaded += OnLoaded;
        IsVisibleChanged += OnSelfIsVisibleChanged;
    }

    /// <summary>页码栏布局方向；在条目控件 Left/Right 方位页脚中自动切换为 Vertical。</summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>当前页码（从 1 起）。</summary>
    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    /// <summary>总页数。</summary>
    public int PageCount
    {
        get => (int)GetValue(PageCountProperty);
        set => SetValue(PageCountProperty, value);
    }

    /// <summary>条目总数。</summary>
    public int TotalCount
    {
        get => (int)GetValue(TotalCountProperty);
        set => SetValue(TotalCountProperty, value);
    }

    /// <summary>每页条数（页脚选择器写入后经绑定同步回条目控件的 PagingAssist.PageSize）。</summary>
    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    /// <summary>每页条数候选值，逗号分隔（如 "10,20,50,100"）。</summary>
    public string PageSizeOptions
    {
        get => (string)GetValue(PageSizeOptionsProperty);
        set => SetValue(PageSizeOptionsProperty, value);
    }

    /// <summary>滑窗内同时可见的页码数（最小 1）。</summary>
    public int ButtonCount
    {
        get => (int)GetValue(ButtonCountProperty);
        set => SetValue(ButtonCountProperty, value);
    }

    /// <summary>页码序列（模板内部产物，<see cref="PageNumberItem"/> 与 <see cref="PageEllipsisItem"/>；由 <c>PART_PagesHost</c> 渲染）。</summary>
    internal ObservableCollection<object> PageNumbers => _pageNumbers;

    private ItemsControl? _owner;
    private ObservableCollection<int>? _pageSizeOptions;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BindToOwner();
        UpdateVisualState();
    }

    private void OnSelfIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // 模板页脚初始折叠（分页未启用）：Loaded 时 BindToOwner 会因 owner.PageSize=0
        // 提前返回。运行时把 PageSize 从 0 调到 >0 后页脚才变为可见——此时 Loaded 已
        // 错过，借可见性恢复补建与宿主的绑定。
        if ((bool)e.NewValue)
        {
            BindToOwner();
        }
    }

    /// <summary>
    /// 自动模式：绑定最近的 ItemsControl 祖先的分页附加属性（仅当自身依赖属性
    /// 保持默认、未被宿主显式赋值时生效——独立摆放并显式绑定的实例不受影响）。
    /// </summary>
    private void BindToOwner()
    {
        if (_owner is not null)
        {
            return;
        }

        var owner = FindAncestor<ItemsControl>(this);
        if (owner is null || PagingAssist.GetPageSize(owner) <= 0)
        {
            return;
        }

        _owner = owner;
        TryBind(CurrentPageProperty, PagingAssist.CurrentPageProperty, BindingMode.TwoWay);
        TryBind(PageCountProperty, PagingAssist.PageCountProperty, BindingMode.OneWay);
        TryBind(TotalCountProperty, PagingAssist.TotalCountProperty, BindingMode.OneWay);
        TryBind(PageSizeProperty, PagingAssist.PageSizeProperty, BindingMode.TwoWay);
    }

    private void TryBind(DependencyProperty pagerProperty, DependencyProperty ownerProperty, BindingMode mode)
    {
        ValueSource source = DependencyPropertyHelper.GetValueSource(this, pagerProperty);
        if (source.BaseValueSource != BaseValueSource.Default || source.IsCurrent)
        {
            return;
        }

        var binding = new Binding
        {
            Path = new PropertyPath("(0)", ownerProperty),
            Source = _owner,
            Mode = mode,
        };
        SetBinding(pagerProperty, binding);
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = current is Visual || current is System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(current)
                : null;
        }

        return null;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild(PartPagesHost) is ItemsControl pagesHost)
        {
            pagesHost.ItemsSource = _pageNumbers;
        }

        if (GetTemplateChild(PartPageSizeEditor) is ComboBox pageSizeEditor)
        {
            _pageSizeOptions = BuildPageSizeOptions(PageSizeOptions, PageSize);
            pageSizeEditor.ItemsSource = _pageSizeOptions;
            pageSizeEditor.SelectedItem = PageSize > 0 ? PageSize : null;
            pageSizeEditor.SelectionChanged += OnPageSizeEditorSelectionChanged;
        }

        UpdateVisualState();
    }

    private void OnPageSizeEditorSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GetTemplateChild(PartPageSizeEditor) is ComboBox editor && editor.SelectedItem is int pageSize)
        {
            // SetCurrentValue：镜像绑定（若有）不被本地值顶掉，TwoWay 继续把新值推送回 PagingAssist
            SetCurrentValue(PageSizeProperty, pageSize);
        }
    }

    private void OnPageStateChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateVisualState();

        // 页码状态变化后命令按钮的 CanExecute 立即重查（否则要等下一次焦点/空闲调度）
        CommandManager.InvalidateRequerySuggested();
    }

    private static void OnPageStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DataPager)d).OnPageStateChanged(e);
    }

    private void OnPageSizeOptionsChanged(DependencyPropertyChangedEventArgs e)
    {
        if (GetTemplateChild(PartPageSizeEditor) is ComboBox editor)
        {
            _pageSizeOptions = BuildPageSizeOptions(PageSizeOptions, PageSize);
            editor.ItemsSource = _pageSizeOptions;
        }
    }

    private static void OnPageSizeOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DataPager)d).OnPageSizeOptionsChanged(e);
    }

    private static void OnVisualStateInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DataPager)d).UpdateVisualState();
    }

    private void OnPageSizeChanged(DependencyPropertyChangedEventArgs e)
    {
        // PageSize 被外部（宿主绑定 / PagingAssist 同步）改变：选择器候选与选中值随之校正
        if (GetTemplateChild(PartPageSizeEditor) is ComboBox editor)
        {
            _pageSizeOptions = BuildPageSizeOptions(PageSizeOptions, PageSize);
            editor.ItemsSource = _pageSizeOptions;
            editor.SelectedItem = PageSize > 0 ? PageSize : null;
        }
    }

    private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DataPager)d).OnPageSizeChanged(e);
    }

    /// <summary>解析候选值并并入当前 PageSize（保证选择器能显示当前值）。</summary>
    private ObservableCollection<int> BuildPageSizeOptions(string options, int current)
    {
        var values = ParsePageSizeOptionValues(options).ToHashSet();
        if (current > 0)
        {
            values.Add(current);
        }

        return new ObservableCollection<int>(values.OrderBy(value => value));
    }

    private static IEnumerable<int> ParsePageSizeOptionValues(string options)
    {
        return options
            .Split(',')
            .Select(text => text.Trim())
            .Where(text => text.Length > 0)
            .Select(text => int.TryParse(text, out int value) ? value : -1)
            .Where(value => value > 0)
            .Distinct();
    }

    private void UpdateVisualState()
    {
        RegeneratePageNumbers();

        if (GetTemplateChild(PartPageSizeEditor) is ComboBox editor
            && PageSize > 0
            && _pageSizeOptions is not null
            && !_pageSizeOptions.Contains(PageSize))
        {
            // PageSize 被外部改为候选之外的值：并入候选并选中
            _pageSizeOptions.Add(PageSize);
            editor.SelectedItem = PageSize;
        }
    }

    private void CanMoveToFirstPage(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = PageCount > 0 && CurrentPage > 1;
    }

    private void CanMoveToPreviousPage(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = PageCount > 0 && CurrentPage > 1;
    }

    private void CanMoveToNextPage(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = CurrentPage < PageCount;
    }

    private void CanMoveToLastPage(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = CurrentPage < PageCount;
    }

    private void CanMoveToPage(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = e.Parameter is int page && page >= 1 && page <= PageCount;
    }

    private void OnMoveToFirstPage(object sender, ExecutedRoutedEventArgs e)
    {
        SetCurrentValue(CurrentPageProperty, 1);
    }

    private void OnMoveToPreviousPage(object sender, ExecutedRoutedEventArgs e)
    {
        SetCurrentValue(CurrentPageProperty, Math.Max(1, CurrentPage - 1));
    }

    private void OnMoveToNextPage(object sender, ExecutedRoutedEventArgs e)
    {
        SetCurrentValue(CurrentPageProperty, Math.Min(Math.Max(1, PageCount), CurrentPage + 1));
    }

    private void OnMoveToLastPage(object sender, ExecutedRoutedEventArgs e)
    {
        SetCurrentValue(CurrentPageProperty, Math.Max(1, PageCount));
    }

    private void OnMoveToPage(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is int page)
        {
            SetCurrentValue(CurrentPageProperty, Math.Max(1, Math.Min(page, Math.Max(1, PageCount))));
        }
    }

    private void RegeneratePageNumbers()
    {
        var numbers = _pageNumbers;
        numbers.Clear();
        int pageCount = Math.Max(0, PageCount);
        if (pageCount == 0)
        {
            return;
        }

        int window = Math.Max(1, ButtonCount);
        if (pageCount <= window)
        {
            for (int page = 1; page <= pageCount; page++)
            {
                numbers.Add(new PageNumberItem(page, page == CurrentPage));
            }

            return;
        }

        int half = window / 2;
        int start = Math.Max(2, CurrentPage - half);
        int end = Math.Min(pageCount - 1, start + window - 1);
        start = Math.Max(2, end - window + 1);

        numbers.Add(new PageNumberItem(1, CurrentPage == 1));
        if (start > 2)
        {
            numbers.Add(PageEllipsisItem.Instance);
        }

        for (int page = start; page <= end; page++)
        {
            numbers.Add(new PageNumberItem(page, page == CurrentPage));
        }

        if (end < pageCount - 1)
        {
            numbers.Add(PageEllipsisItem.Instance);
        }

        numbers.Add(new PageNumberItem(pageCount, CurrentPage == pageCount));
    }
}

/// <summary>
/// 省略号条目：独立类型而非裸字符串，宿主字符串数据不会与本库的省略号渲染规则相撞。
/// </summary>
public sealed class PageEllipsisItem
{
    internal static readonly PageEllipsisItem Instance = new();

    private PageEllipsisItem()
    {
    }
}

/// <summary>页码条目：页码 + 是否当前页（模板据此高亮当前页按钮）。属性须为 public 供绑定引擎读取。</summary>
public sealed class PageNumberItem
{
    internal PageNumberItem(int page, bool isCurrent)
    {
        Page = page;
        IsCurrent = isCurrent;
    }

    public int Page { get; }

    public bool IsCurrent { get; }
}
