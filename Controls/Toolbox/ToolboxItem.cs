using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Toolbox;

[TemplatePart(Name = PartTriggerButton, Type = typeof(ButtonBase))]
[TemplatePart(Name = PartPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartPopupRoot, Type = typeof(FrameworkElement))]
public sealed class ToolboxItem : HeaderedItemsControl
{
    internal const string PartTriggerButton = "PART_TriggerButton";
    internal const string PartPopup = "PART_Popup";
    internal const string PartPopupRoot = "PART_PopupRoot";

    private static readonly DependencyPropertyKey IsOpenPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsOpen),
            typeof(bool),
            typeof(ToolboxItem),
            new PropertyMetadata(false));

    private ButtonBase? _triggerButton;
    private Popup? _popup;
    private FrameworkElement? _popupRoot;
    private Key? _keyboardActivationKey;
    private bool _focusFirstToolOnOpen;
    private int _focusAttemptCount;

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(ToolboxItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ToolboxItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(
            nameof(DisplayMode),
            typeof(ToolboxDisplayMode),
            typeof(ToolboxItem),
            new PropertyMetadata(ToolboxDisplayMode.IconOnly));

    public static readonly DependencyProperty IsOpenProperty = IsOpenPropertyKey.DependencyProperty;

    static ToolboxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolboxItem),
            new FrameworkPropertyMetadata(typeof(ToolboxItem)));
    }

    public ToolboxItem()
    {
        IsEnabledChanged += OnIsEnabledChanged;
    }

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ToolboxDisplayMode DisplayMode
    {
        get => (ToolboxDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public bool IsOpen => (bool)GetValue(IsOpenProperty);

    internal ButtonBase? TriggerButton => _triggerButton;

    internal Popup? Popup => _popup;

    internal FrameworkElement? PopupRoot => _popupRoot;

    internal Toolbox? Owner { get; private set; }

    internal bool IsPointerOverTrigger { get; private set; }

    internal bool IsPointerOverPopup { get; private set; }

    internal bool IsPointerOverEitherRegion => IsPointerOverTrigger || IsPointerOverPopup;

    internal void AttachOwner(Toolbox owner)
    {
        if (ReferenceEquals(Owner, owner))
        {
            return;
        }

        Owner?.NotifyItemInvalidated(this);
        Owner = owner;
        SetIsOpen(false);
    }

    internal void DetachOwner(Toolbox owner)
    {
        if (!ReferenceEquals(Owner, owner))
        {
            return;
        }

        SetIsOpen(false);
        IsPointerOverTrigger = false;
        IsPointerOverPopup = false;
        Owner = null;
    }

    internal void SetPointerOverTrigger(bool value)
    {
        IsPointerOverTrigger = value;
    }

    internal void SetPointerOverPopup(bool value)
    {
        IsPointerOverPopup = value;
    }

    internal void SetIsOpen(bool value)
    {
        if (value)
        {
            // 拖拽收起时弹出层内容曾被置为 Collapsed，重新展开前恢复可见。
            if (_popupRoot is not null)
            {
                _popupRoot.Visibility = Visibility.Visible;
            }

            ApplyOwnerLayout();
        }

        if (!value)
        {
            _focusFirstToolOnOpen = false;
            _focusAttemptCount = 0;
            ItemContainerGenerator.StatusChanged -= OnItemGeneratorStatusChanged;
        }

        SetValue(IsOpenPropertyKey, value);
    }

    // 拖拽发起时立即收起弹出窗口：内容置为 Collapsed（不渲染、不参与命中测试），
    // 由 Toolbox 随后关闭弹层；再次展开时由 SetIsOpen 恢复可见性。
    internal void HidePopupForDrag()
    {
        if (_popupRoot is not null)
        {
            _popupRoot.Visibility = Visibility.Collapsed;
        }
    }

    internal void RequestFocusFirstEnabledTool()
    {
        _focusFirstToolOnOpen = true;
        _focusAttemptCount = 0;
        ItemContainerGenerator.StatusChanged -= OnItemGeneratorStatusChanged;
        ItemContainerGenerator.StatusChanged += OnItemGeneratorStatusChanged;
    }

    internal void ScheduleFocusFirstEnabledTool()
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(FocusFirstEnabledTool));
    }

    public override void OnApplyTemplate()
    {
        DetachTemplateParts();
        base.OnApplyTemplate();

        _triggerButton = GetTemplateChild(PartTriggerButton) as ButtonBase;
        _popup = GetTemplateChild(PartPopup) as Popup;
        _popupRoot = GetTemplateChild(PartPopupRoot) as FrameworkElement;

        if (_triggerButton is not null)
        {
            _triggerButton.MouseEnter += OnTriggerMouseEnter;
            _triggerButton.MouseLeave += OnTriggerMouseLeave;
            _triggerButton.Click += OnTriggerClick;
            _triggerButton.PreviewKeyDown += OnTriggerPreviewKeyDown;
            _triggerButton.PreviewKeyUp += OnTriggerPreviewKeyUp;
            _triggerButton.LostKeyboardFocus += OnTriggerLostKeyboardFocus;
        }

        if (_popupRoot is not null)
        {
            _popupRoot.MouseEnter += OnPopupMouseEnter;
            _popupRoot.MouseLeave += OnPopupMouseLeave;
            _popupRoot.PreviewKeyDown += OnPopupPreviewKeyDown;
        }

        if (_popup is not null)
        {
            _popup.CustomPopupPlacementCallback = GetCustomPopupPlacements;
            _popup.Opened += OnPopupOpened;
            // Popup 经系统路径自行关闭（如宿主窗口关闭）时，TemplateBinding 单向绑定不会
            // 回写只读 IsOpen，经 Closed 事件同步回条目并通知协调器清空 ActiveItem。
            _popup.Closed += OnPopupClosed;
        }

        ApplyOwnerLayout();
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ToolItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ToolItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is not ToolItem container)
        {
            return;
        }

        container.AttachOwner(this);
        if (!ReferenceEquals(container, item))
        {
            container.SetGeneratedDragData(item);
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is ToolItem container)
        {
            container.ClearGeneratedDragData(item);
            container.DetachOwner(this);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        if (!HasItems)
        {
            Owner?.NotifyItemInvalidated(this);
        }
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!(bool)e.NewValue)
        {
            Owner?.NotifyItemInvalidated(this);
        }
    }

    private void DetachTemplateParts()
    {
        if (_triggerButton is not null)
        {
            _triggerButton.MouseEnter -= OnTriggerMouseEnter;
            _triggerButton.MouseLeave -= OnTriggerMouseLeave;
            _triggerButton.Click -= OnTriggerClick;
            _triggerButton.PreviewKeyDown -= OnTriggerPreviewKeyDown;
            _triggerButton.PreviewKeyUp -= OnTriggerPreviewKeyUp;
            _triggerButton.LostKeyboardFocus -= OnTriggerLostKeyboardFocus;
        }

        if (_popupRoot is not null)
        {
            _popupRoot.MouseEnter -= OnPopupMouseEnter;
            _popupRoot.MouseLeave -= OnPopupMouseLeave;
            _popupRoot.PreviewKeyDown -= OnPopupPreviewKeyDown;
        }

        if (_popup is not null)
        {
            _popup.CustomPopupPlacementCallback = null;
            _popup.Opened -= OnPopupOpened;
            _popup.Closed -= OnPopupClosed;
        }

        _triggerButton = null;
        _popup = null;
        _popupRoot = null;
        _keyboardActivationKey = null;
    }

    private void OnTriggerMouseEnter(object sender, MouseEventArgs e)
    {
        SetPointerOverTrigger(true);
        Owner?.RequestOpen(this);
    }

    private void OnTriggerMouseLeave(object sender, MouseEventArgs e)
    {
        SetPointerOverTrigger(false);
        Owner?.RequestClose(this);
    }

    private void OnTriggerClick(object sender, RoutedEventArgs e)
    {
        if (Owner is null)
        {
            return;
        }

        bool focusFirstTool = _keyboardActivationKey is Key.Enter or Key.Space;
        _keyboardActivationKey = null;
        Owner.Toggle(this, focusFirstTool);
    }

    private void OnTriggerPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CloseAndRestoreTriggerFocus(e);
        }
        else if (e.Key is Key.Enter or Key.Space)
        {
            _keyboardActivationKey = e.Key;
        }
    }

    private void OnTriggerPreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != _keyboardActivationKey)
        {
            return;
        }

        Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            new Action(() => _keyboardActivationKey = null));
    }

    private void OnTriggerLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (!IsOpen)
        {
            _keyboardActivationKey = null;
        }
    }

    private void OnPopupPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CloseAndRestoreTriggerFocus(e);
        }
    }

    private void CloseAndRestoreTriggerFocus(KeyEventArgs e)
    {
        if (Owner is null)
        {
            return;
        }

        Owner.ClosePopup();
        _triggerButton?.Focus();
        e.Handled = true;
    }

    private void OnPopupMouseEnter(object sender, MouseEventArgs e)
    {
        SetPointerOverPopup(true);
        Owner?.RequestOpen(this);
    }

    private void OnPopupMouseLeave(object sender, MouseEventArgs e)
    {
        SetPointerOverPopup(false);
        Owner?.RequestClose(this);
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        ApplyOwnerLayout();
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(ApplyOwnerLayout));
        if (_focusFirstToolOnOpen)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(FocusFirstEnabledTool));
        }
    }

    // Popup 经系统路径自行关闭（宿主窗口关闭、Popup.Close 被外部调用等）时，
    // TemplateBinding 单向绑定不会回写只读 IsOpen——在 Closed 事件里同步回条目，
    // 并交由协调器清空 ActiveItem/计时器（常规关闭路径 IsOpen 已为 false，此处为空操作）。
    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (!IsOpen)
        {
            return;
        }

        SetIsOpen(false);
        Owner?.NotifyItemInvalidated(this);
    }

    private CustomPopupPlacement[] GetCustomPopupPlacements(
        Size popupSize,
        Size targetSize,
        Point offset)
    {
        if (Owner is null)
        {
            return Array.Empty<CustomPopupPlacement>();
        }

        return ToolboxPopupPlacementCalculator.GetPlacements(
                popupSize,
                targetSize,
                offset,
                Owner.Orientation,
                Owner.PopupPlacement)
            .Select(candidate => candidate.Placement)
            .ToArray();
    }

    private void FocusFirstEnabledTool()
    {
        if (!_focusFirstToolOnOpen || !IsOpen)
        {
            return;
        }

        for (int index = 0; index < Items.Count; index++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(index) is ToolItem tool && tool.IsEnabled)
            {
                _focusFirstToolOnOpen = false;
                _focusAttemptCount = 0;
                ItemContainerGenerator.StatusChanged -= OnItemGeneratorStatusChanged;
                tool.Focus();
                return;
            }
        }

        if (++_focusAttemptCount < 20)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(FocusFirstEnabledTool));
        }
        else
        {
            _focusFirstToolOnOpen = false;
            _focusAttemptCount = 0;
            ItemContainerGenerator.StatusChanged -= OnItemGeneratorStatusChanged;
        }
    }

    private void OnItemGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (_focusFirstToolOnOpen)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(FocusFirstEnabledTool));
        }
    }

    internal void ApplyOwnerLayout()
    {
        if (Owner is null || _popupRoot is null)
        {
            return;
        }

        BindingOperations.SetBinding(
            _popupRoot,
            WidthProperty,
            new Binding(nameof(Toolbox.PopupWidth)) { Source = Owner });
        Visual workAreaTarget = _triggerButton is not null ? _triggerButton : this;
        Rect workArea = MonitorWorkAreaProvider.GetWorkAreaDip(workAreaTarget);
        double availableHeight = Math.Max(0d, workArea.Height - 16d);
        _popupRoot.MaxHeight = Math.Min(Owner.PopupMaxHeight, availableHeight);

        UniformGrid? panel = FindPopupItemsPanel(_popupRoot);
        if (panel is not null)
        {
            BindingOperations.SetBinding(
                panel,
                UniformGrid.ColumnsProperty,
                new Binding(nameof(Toolbox.ColumnCount)) { Source = Owner });
        }
    }

    /// <summary>
    /// 弹层网格按「ItemsPresenter 的直接子级」定位（ItemsPanel 模板的根元素）：
    /// 宿主替换 ItemsPanel 为非 UniformGrid 时静默跳过，条目内容中嵌套的
    /// UniformGrid 不会被误绑。
    /// </summary>
    private static UniformGrid? FindPopupItemsPanel(DependencyObject popupRoot)
    {
        ItemsPresenter? presenter = FindVisualChild<ItemsPresenter>(popupRoot);
        return presenter is null ? null : FindDirectVisualChild<UniformGrid>(presenter);
    }

    private static T? FindDirectVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            if (VisualTreeHelper.GetChild(root, index) is T match)
            {
                return match;
            }
        }

        return null;
    }

    internal void RepositionPopup()
    {
        if (_popup is null || !_popup.IsOpen)
        {
            return;
        }

        ApplyOwnerLayout();
        double originalOffset = _popup.HorizontalOffset;
        _popup.HorizontalOffset = originalOffset + 0.01d;
        _popup.HorizontalOffset = originalOffset;
    }

    internal void NotifyDragStarted()
    {
        Owner?.NotifyDragStarted(this);
    }

    internal void NotifyDragCompleted()
    {
        Owner?.NotifyDragCompleted(this);
    }

    private static T? FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                return match;
            }

            T? descendant = FindVisualChild<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }
}
