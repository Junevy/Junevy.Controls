using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Toolbox;

public sealed class Toolbox : ItemsControl
{
    private static readonly DependencyPropertyKey ActiveItemPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ActiveItem),
            typeof(ToolboxItem),
            typeof(Toolbox),
            new PropertyMetadata(null));

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(Toolbox),
            new PropertyMetadata(Orientation.Vertical));

    public static readonly DependencyProperty OpenDelayProperty =
        DependencyProperty.Register(
            nameof(OpenDelay),
            typeof(TimeSpan),
            typeof(Toolbox),
            new PropertyMetadata(TimeSpan.FromMilliseconds(150)),
            IsNonNegativeDelay);

    public static readonly DependencyProperty CloseDelayProperty =
        DependencyProperty.Register(
            nameof(CloseDelay),
            typeof(TimeSpan),
            typeof(Toolbox),
            new PropertyMetadata(TimeSpan.FromMilliseconds(300)),
            IsNonNegativeDelay);

    public static readonly DependencyProperty PopupWidthProperty =
        DependencyProperty.Register(
            nameof(PopupWidth),
            typeof(double),
            typeof(Toolbox),
            new PropertyMetadata(300d),
            IsPositiveFiniteDouble);

    public static readonly DependencyProperty ColumnCountProperty =
        DependencyProperty.Register(
            nameof(ColumnCount),
            typeof(int),
            typeof(Toolbox),
            new PropertyMetadata(6),
            IsPositiveColumnCount);

    public static readonly DependencyProperty PopupMaxHeightProperty =
        DependencyProperty.Register(
            nameof(PopupMaxHeight),
            typeof(double),
            typeof(Toolbox),
            new PropertyMetadata(480d),
            IsPositiveFiniteDouble);

    public static readonly DependencyProperty PopupPlacementProperty =
        DependencyProperty.Register(
            nameof(PopupPlacement),
            typeof(ToolboxPopupPlacement),
            typeof(Toolbox),
            new PropertyMetadata(ToolboxPopupPlacement.Auto));

    public static readonly DependencyProperty DragDataFormatProperty =
        DependencyProperty.Register(
            nameof(DragDataFormat),
            typeof(string),
            typeof(Toolbox),
            new PropertyMetadata("Junevy.Controls.Tool"),
            IsNonEmptyFormat);

    public static readonly DependencyProperty ActiveItemProperty = ActiveItemPropertyKey.DependencyProperty;

    private readonly DispatcherTimer _openTimer;
    private readonly DispatcherTimer _closeTimer;
    private ToolboxItem? _pendingItem;
    private ToolboxItem? _activeItem;
    private Window? _hostWindow;
    private ToolboxItem? _dragOwner;

    static Toolbox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Toolbox),
            new FrameworkPropertyMetadata(typeof(Toolbox)));
    }

    public Toolbox()
    {
        _openTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
        _openTimer.Tick += OnOpenTimerTick;
        _closeTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
        _closeTimer.Tick += OnCloseTimerTick;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public TimeSpan OpenDelay
    {
        get => (TimeSpan)GetValue(OpenDelayProperty);
        set => SetValue(OpenDelayProperty, value);
    }

    public TimeSpan CloseDelay
    {
        get => (TimeSpan)GetValue(CloseDelayProperty);
        set => SetValue(CloseDelayProperty, value);
    }

    public double PopupWidth
    {
        get => (double)GetValue(PopupWidthProperty);
        set => SetValue(PopupWidthProperty, value);
    }

    public int ColumnCount
    {
        get => (int)GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    public double PopupMaxHeight
    {
        get => (double)GetValue(PopupMaxHeightProperty);
        set => SetValue(PopupMaxHeightProperty, value);
    }

    public ToolboxPopupPlacement PopupPlacement
    {
        get => (ToolboxPopupPlacement)GetValue(PopupPlacementProperty);
        set => SetValue(PopupPlacementProperty, value);
    }

    public string DragDataFormat
    {
        get => (string)GetValue(DragDataFormatProperty);
        set => SetValue(DragDataFormatProperty, value);
    }

    public ToolboxItem? ActiveItem => (ToolboxItem?)GetValue(ActiveItemProperty);

    public void ClosePopup()
    {
        StopOpenTimer();
        _closeTimer.Stop();
        _dragOwner = null;

        if (_activeItem is not null)
        {
            _activeItem.SetIsOpen(false);
            _activeItem = null;
        }

        SetValue(ActiveItemPropertyKey, null);
    }

    internal Action? RepositionRequested { get; set; }

    internal void RequestOpen(ToolboxItem item)
    {
        if (!IsEligibleRequest(item))
        {
            return;
        }

        _closeTimer.Stop();
        if (ReferenceEquals(_activeItem, item))
        {
            StopOpenTimer();
            return;
        }

        _openTimer.Stop();
        _pendingItem = item;
        _openTimer.Interval = OpenDelay;
        _openTimer.Start();
    }

    internal void RequestClose(ToolboxItem item)
    {
        if (!IsEligibleRequest(item))
        {
            return;
        }

        bool cancelledPendingOpen = ReferenceEquals(_pendingItem, item);
        if (cancelledPendingOpen)
        {
            StopOpenTimer();
        }

        if (ReferenceEquals(_activeItem, item) && item.IsPointerOverEitherRegion)
        {
            _closeTimer.Stop();
            return;
        }

        if (ReferenceEquals(_activeItem, item) || cancelledPendingOpen)
        {
            ScheduleActiveCloseIfNeeded();
        }
    }

    internal void NotifyDragStarted(ToolboxItem item)
    {
        if (!ReferenceEquals(_activeItem, item) || !IsEligibleRequest(item))
        {
            return;
        }

        if (_dragOwner is not null && !ReferenceEquals(_dragOwner, item))
        {
            return;
        }

        _dragOwner = item;
        _closeTimer.Stop();
    }

    internal void NotifyDragCompleted(ToolboxItem item)
    {
        if (!ReferenceEquals(_dragOwner, item))
        {
            return;
        }

        _dragOwner = null;
        if (_activeItem is not null && !_activeItem.IsPointerOverEitherRegion)
        {
            ClosePopup();
        }
    }

    internal void NotifyItemInvalidated(ToolboxItem item)
    {
        if (!ReferenceEquals(item.Owner, this))
        {
            return;
        }

        if (ReferenceEquals(_pendingItem, item))
        {
            StopOpenTimer();
            ScheduleActiveCloseIfNeeded();
        }

        if (ReferenceEquals(_activeItem, item))
        {
            ClosePopup();
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ToolboxItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ToolboxItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is ToolboxItem container)
        {
            container.AttachOwner(this);
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is ToolboxItem container)
        {
            bool affectsCoordinator = ReferenceEquals(_pendingItem, container)
                || ReferenceEquals(_activeItem, container);
            if (affectsCoordinator)
            {
                ClosePopup();
            }
            else if (ReferenceEquals(_dragOwner, container))
            {
                _dragOwner = null;
                ScheduleActiveCloseIfNeeded();
            }

            container.DetachOwner(this);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    private void OnOpenTimerTick(object? sender, EventArgs e)
    {
        _openTimer.Stop();
        ToolboxItem? item = _pendingItem;
        _pendingItem = null;

        if (item is null
            || !ReferenceEquals(item.Owner, this)
            || !item.IsLoaded
            || !item.IsEnabled
            || !item.HasItems
            || !item.IsPointerOverTrigger)
        {
            ScheduleActiveCloseIfNeeded();
            return;
        }

        SetActiveItem(item);
    }

    private void OnCloseTimerTick(object? sender, EventArgs e)
    {
        _closeTimer.Stop();
        if (_dragOwner is not null || _activeItem is null || _activeItem.IsPointerOverEitherRegion)
        {
            return;
        }

        ClosePopup();
    }

    private void SetActiveItem(ToolboxItem item)
    {
        _closeTimer.Stop();
        if (ReferenceEquals(_activeItem, item))
        {
            return;
        }

        if (_activeItem is not null)
        {
            _activeItem.SetIsOpen(false);
        }

        _activeItem = null;
        SetValue(ActiveItemPropertyKey, null);

        item.SetIsOpen(true);
        _activeItem = item;
        SetValue(ActiveItemPropertyKey, item);
    }

    private bool IsEligibleRequest(ToolboxItem? item)
    {
        return item is not null
            && ReferenceEquals(item.Owner, this)
            && item.IsEnabled
            && item.HasItems;
    }

    private void StopOpenTimer()
    {
        _openTimer.Stop();
        _pendingItem = null;
    }

    private void ScheduleActiveCloseIfNeeded()
    {
        _closeTimer.Stop();
        if (_dragOwner is not null
            || _activeItem is null
            || _activeItem.IsPointerOverEitherRegion)
        {
            return;
        }

        _closeTimer.Interval = CloseDelay;
        _closeTimer.Start();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AttachHostWindow(Window.GetWindow(this));
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        DetachHostWindow();
        ClosePopup();
    }

    private void AttachHostWindow(Window? window)
    {
        if (ReferenceEquals(_hostWindow, window))
        {
            return;
        }

        DetachHostWindow();
        _hostWindow = window;
        if (_hostWindow is null)
        {
            return;
        }

        _hostWindow.Deactivated += OnHostDeactivated;
        _hostWindow.StateChanged += OnHostStateChanged;
        _hostWindow.LocationChanged += OnHostLocationChanged;
        _hostWindow.SizeChanged += OnHostSizeChanged;
    }

    private void DetachHostWindow()
    {
        if (_hostWindow is null)
        {
            return;
        }

        _hostWindow.Deactivated -= OnHostDeactivated;
        _hostWindow.StateChanged -= OnHostStateChanged;
        _hostWindow.LocationChanged -= OnHostLocationChanged;
        _hostWindow.SizeChanged -= OnHostSizeChanged;
        _hostWindow = null;
    }

    private void OnHostDeactivated(object? sender, EventArgs e)
    {
        ClosePopup();
    }

    private void OnHostStateChanged(object? sender, EventArgs e)
    {
        if (_hostWindow?.WindowState == WindowState.Minimized)
        {
            ClosePopup();
        }
    }

    private void OnHostLocationChanged(object? sender, EventArgs e)
    {
        RequestRepositionIfOpen();
    }

    private void OnHostSizeChanged(object sender, SizeChangedEventArgs e)
    {
        RequestRepositionIfOpen();
    }

    private void RequestRepositionIfOpen()
    {
        if (_activeItem is not null)
        {
            RepositionRequested?.Invoke();
        }
    }

    private static bool IsNonNegativeDelay(object value)
    {
        return (TimeSpan)value >= TimeSpan.Zero;
    }

    private static bool IsPositiveFiniteDouble(object value)
    {
        double number = (double)value;
        return number > 0d && !double.IsNaN(number) && !double.IsInfinity(number);
    }

    private static bool IsPositiveColumnCount(object value)
    {
        return (int)value >= 1;
    }

    private static bool IsNonEmptyFormat(object value)
    {
        return value is string text && !string.IsNullOrWhiteSpace(text);
    }
}
