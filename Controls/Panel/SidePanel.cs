using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Junevy.Controls.Controls.Panel;

/// <summary>
/// 侧滑面板控件：作为浮层放置在 <see cref="Grid"/> 等布局容器中，
/// 通过 <see cref="IsOpen"/> 绑定布尔值控制滑出/收回，通过 <see cref="Side"/> 配置滑出方向
/// （左/右/上/下）。收起时面板完全滑出可视区域、不占用布局空间；
/// 展开时以浮层形式叠加显示在兄弟内容上方，可选半透明遮罩与阴影过渡。
/// 展开期间点击面板本体以外的区域、宿主窗口失焦或最小化时自动收回（<see cref="CloseOnOutsideClick"/>）。
/// </summary>
/// <remarks>
/// 直接放入 <see cref="Grid"/>（不指定 Row/Column）时，控件自动跨满父 Grid 的所有列/行，
/// 形成覆盖整个 Grid 的浮层；滑出面板自身的宽度与高度由其 <c>Content</c> 决定，
/// 可通过设置内容的 <c>Width</c>/<c>Height</c> 指定。
/// </remarks>
[TemplatePart(Name = PartBackdrop, Type = typeof(Border))]
[TemplatePart(Name = PartContent, Type = typeof(Border))]
[TemplatePart(Name = PartTranslate, Type = typeof(TranslateTransform))]
public class SidePanel : ContentControl
{
    private const string PartBackdrop = "PART_Backdrop";
    private const string PartContent = "PART_Content";
    private const string PartTranslate = "PART_Translate";

    private Border? _backdrop;
    private Border? _content;
    private TranslateTransform? _translate;
    private Window? _hostWindow;
    private MouseButtonEventHandler? _gestureDownHandler;
    private MouseButtonEventHandler? _gestureUpHandler;
    private bool _pendingOutsideDismiss;
    private int _stateVersion;
    private bool _pendingStateChangedEvent;

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(SidePanel),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsOpenChanged));

    public static readonly DependencyProperty SideProperty =
        DependencyProperty.Register(
            nameof(Side),
            typeof(SidePanelSide),
            typeof(SidePanel),
            new PropertyMetadata(SidePanelSide.Left, OnSideChanged));

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(
            nameof(AnimationDuration),
            typeof(Duration),
            typeof(SidePanel),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(250))),
            IsValidAnimationDuration);

    public static readonly DependencyProperty IsBackdropEnabledProperty =
        DependencyProperty.Register(
            nameof(IsBackdropEnabled),
            typeof(bool),
            typeof(SidePanel),
            new PropertyMetadata(true, OnBackdropChanged));

    public static readonly DependencyProperty BackdropBrushProperty =
        DependencyProperty.Register(
            nameof(BackdropBrush),
            typeof(Brush),
            typeof(SidePanel),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CloseOnOutsideClickProperty =
        DependencyProperty.Register(
            nameof(CloseOnOutsideClick),
            typeof(bool),
            typeof(SidePanel),
            new PropertyMetadata(true, OnCloseOnOutsideClickChanged));

    public static readonly RoutedEvent OpenedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Opened),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SidePanel));

    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Closed),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SidePanel));

    static SidePanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SidePanel),
            new FrameworkPropertyMetadata(typeof(SidePanel)));
    }

    public SidePanel()
    {
        Loaded += OnPanelLoaded;
        Unloaded += OnPanelUnloaded;
    }

    /// <summary>是否滑出；支持双向绑定，绑定 <c>true</c> 时面板从 <see cref="Side"/> 指定的边缘滑出。</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>滑出方向：Left / Right / Top / Bottom，表示面板从父容器的哪一个边缘滑出。</summary>
    public SidePanelSide Side
    {
        get => (SidePanelSide)GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    /// <summary>滑出/收回过渡动画时长；为 <see cref="Duration.Automatic"/> 或 <see cref="Duration.Forever"/> 时视为无效。</summary>
    public Duration AnimationDuration
    {
        get => (Duration)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>是否启用遮罩层；启用后展开时在面板背后显示 <see cref="BackdropBrush"/> 半透明遮罩。</summary>
    public bool IsBackdropEnabled
    {
        get => (bool)GetValue(IsBackdropEnabledProperty);
        set => SetValue(IsBackdropEnabledProperty, value);
    }

    /// <summary>遮罩层画刷；默认样式使用主题资源 <c>Theme.Brush.Overlay.Backdrop</c>。</summary>
    public Brush? BackdropBrush
    {
        get => (Brush?)GetValue(BackdropBrushProperty);
        set => SetValue(BackdropBrushProperty, value);
    }

    /// <summary>
    /// 展开时点击面板本体以外的区域是否自动收回（在鼠标抬起时判定，此时宿主按钮的命令已执行完毕，
    /// 与「按钮 + <see cref="IsOpen"/> 双向绑定 + 命令取反」的宿主用法不冲突）；
    /// 同时决定宿主窗口失焦、最小化时是否收回。
    /// 置为 <c>false</c> 时收回完全由宿主通过 <see cref="IsOpen"/> 或 <see cref="Toggle"/> 控制。
    /// </summary>
    public bool CloseOnOutsideClick
    {
        get => (bool)GetValue(CloseOnOutsideClickProperty);
        set => SetValue(CloseOnOutsideClickProperty, value);
    }

    /// <summary>滑出动画完成后触发。</summary>
    public event RoutedEventHandler Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    /// <summary>收回动画完成后触发。</summary>
    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        ApplyParentGridSpan();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_content is not null)
        {
            _content.SizeChanged -= OnContentSizeChanged;
        }

        _backdrop = GetTemplateChild(PartBackdrop) as Border;
        _content = GetTemplateChild(PartContent) as Border;
        _translate = GetTemplateChild(PartTranslate) as TranslateTransform;

        if (_content is not null)
        {
            _content.SizeChanged += OnContentSizeChanged;
        }

        ApplyBackdropState(animate: false);
        ApplySlideState(animate: false);

        // 模板加载前发生过状态切换时补发事件，避免事件丢失。
        if (_pendingStateChangedEvent)
        {
            _pendingStateChangedEvent = false;
            RaiseStateChangedEvent(IsOpen);
        }
    }

    /// <summary>切换滑出/收回状态。</summary>
    public void Toggle()
    {
        SetCurrentValue(IsOpenProperty, !IsOpen);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (SidePanel)d;

        if (panel._content is null)
        {
            // 模板尚未加载时无法执行动画，模板应用后会按当前状态就位并补发事件。
            panel._pendingStateChangedEvent = true;
            return;
        }

        panel.ApplySlideState(animate: true);
        panel.ApplyBackdropState(animate: true);

        // 时长为 0 时走快照路径，动画完成回调不会执行，事件在状态切换时立即补发，
        // 与 ExpanderPanel 的"事件跟随状态变化"契约一致。
        if (panel.AnimationDuration.HasTimeSpan && panel.AnimationDuration.TimeSpan <= TimeSpan.Zero)
        {
            panel.RaiseStateChangedEvent(panel.IsOpen);
        }
    }

    private static void OnSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (SidePanel)d;
        panel.ApplySlideState(animate: false);
        panel.ApplyBackdropState(animate: false);
    }

    private static void OnBackdropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (SidePanel)d;
        panel.ApplyBackdropState(animate: false);
    }

    private static void OnCloseOnOutsideClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (SidePanel)d;

        if (panel.CloseOnOutsideClick)
        {
            panel.AttachHostWindow(Window.GetWindow(panel));
        }
        else
        {
            panel.DetachHostWindow();
        }
    }

    #region 点击外部自动收回

    private void OnPanelLoaded(object sender, RoutedEventArgs e)
    {
        if (CloseOnOutsideClick)
        {
            AttachHostWindow(Window.GetWindow(this));
        }
    }

    private void OnPanelUnloaded(object sender, RoutedEventArgs e)
    {
        DetachHostWindow();
    }

    private void AttachHostWindow(Window? window)
    {
        if (window is null || ReferenceEquals(_hostWindow, window))
        {
            return;
        }

        DetachHostWindow();
        _hostWindow = window;
        _gestureDownHandler = OnHostGestureDown;
        _gestureUpHandler = OnHostGestureUp;

        // handledEventsToo：面板所在 Grid 的兄弟内容常把鼠标按下/抬起标记为已处理，
        // 不能因此漏掉"点到面板之外"这件事；接管已处理事件才能全覆盖。
        _hostWindow.AddHandler(Mouse.PreviewMouseDownEvent, _gestureDownHandler, true);
        _hostWindow.AddHandler(Mouse.MouseUpEvent, _gestureUpHandler, true);
        _hostWindow.Deactivated += OnHostDeactivated;
        _hostWindow.StateChanged += OnHostStateChanged;
    }

    private void DetachHostWindow()
    {
        if (_hostWindow is null)
        {
            return;
        }

        if (_gestureDownHandler is not null)
        {
            _hostWindow.RemoveHandler(Mouse.PreviewMouseDownEvent, _gestureDownHandler);
            _gestureDownHandler = null;
        }

        if (_gestureUpHandler is not null)
        {
            _hostWindow.RemoveHandler(Mouse.MouseUpEvent, _gestureUpHandler);
            _gestureUpHandler = null;
        }

        _pendingOutsideDismiss = false;
        _hostWindow.Deactivated -= OnHostDeactivated;
        _hostWindow.StateChanged -= OnHostStateChanged;
        _hostWindow = null;
    }

    /// <summary>
    /// 按下阶段只记录"本次手势起于面板之外"，不立即收回。
    /// 宿主的典型用法是「按钮 + IsOpen 双向绑定 + 命令把值取反」，而按钮的 Click 由
    /// <c>ButtonBase</c> 在抬起阶段触发，早于事件冒泡到宿主窗口：若按下时就把 <see cref="IsOpen"/>
    /// 写成 false，命令读到的已是被收回的 false，取反后又是 true，表现为"折叠后立即再次展开"。
    /// </summary>
    private void OnHostGestureDown(object sender, MouseButtonEventArgs e)
    {
        _pendingOutsideDismiss =
            IsOpen
            && CloseOnOutsideClick
            && _content is not null
            && !IsClickInsidePanel(e);
    }

    /// <summary>
    /// 抬起阶段执行收回：此时按钮命令已同步跑完。宿主若已自行收回面板（本用例）
    /// 或本次点击把面板打开（按下时还是收起态，因而不成立挂起条件），都不会再重复动作。
    /// </summary>
    private void OnHostGestureUp(object sender, MouseButtonEventArgs e)
    {
        if (!_pendingOutsideDismiss)
        {
            return;
        }

        _pendingOutsideDismiss = false;
        CloseFromOutside();
    }

    /// <summary>
    /// 判定点击是否落在滑动本体上。遮罩虽是模板的一部分，语义上却代表"面板以外"，
    /// 所以基准是 <c>PART_Content</c>；另外补一次几何判定，避免本面板被其他浮层遮罩盖住时
    /// 命中的是别人的遮罩而被误判成外部点击。
    /// </summary>
    private bool IsClickInsidePanel(MouseButtonEventArgs e)
    {
        var content = _content;
        if (content is null)
        {
            return true;
        }

        var source = e.OriginalSource as Visual ?? e.Source as Visual;
        if (source is not null && IsWithinContent(source))
        {
            return true;
        }

        var point = e.GetPosition(content);
        return point.X >= 0d && point.Y >= 0d && point.X <= content.ActualWidth && point.Y <= content.ActualHeight;
    }

    private void OnHostDeactivated(object? sender, EventArgs e)
    {
        CloseFromOutside();
    }

    private void OnHostStateChanged(object? sender, EventArgs e)
    {
        if (_hostWindow?.WindowState == WindowState.Minimized)
        {
            CloseFromOutside();
        }
    }

    private void CloseFromOutside()
    {
        if (IsOpen && CloseOnOutsideClick)
        {
            // SetCurrentValue：宿主绑定 IsOpen 时收回要回写绑定，不能盖掉本地值。
            SetCurrentValue(IsOpenProperty, false);
        }
    }

    private bool IsWithinContent(Visual source)
    {
        DependencyObject? current = source;

        while (current is not null)
        {
            if (ReferenceEquals(current, _content))
            {
                return true;
            }

            current = current is Visual visual ? VisualTreeHelper.GetParent(visual) : null;
        }

        return false;
    }

    #endregion

    private static bool IsValidAnimationDuration(object value)
    {
        return value is Duration duration && duration.HasTimeSpan && duration.TimeSpan >= TimeSpan.Zero;
    }

    /// <summary>
    /// 约定：直接放入 Grid（不指定 Row/Column）时自动跨满父 Grid 的所有列/行，形成覆盖全部内容的浮层；
    /// 使用者显式设置的 ColumnSpan/RowSpan 不会被覆盖。
    /// </summary>
    private void ApplyParentGridSpan()
    {
        if (Parent is not Grid grid)
        {
            return;
        }

        int columnCount = grid.ColumnDefinitions.Count;
        if (columnCount > 1 && ReadLocalValue(Grid.ColumnSpanProperty) == DependencyProperty.UnsetValue)
        {
            SetValue(Grid.ColumnSpanProperty, columnCount);
        }

        int rowCount = grid.RowDefinitions.Count;
        if (rowCount > 1 && ReadLocalValue(Grid.RowSpanProperty) == DependencyProperty.UnsetValue)
        {
            SetValue(Grid.RowSpanProperty, rowCount);
        }
    }

    private bool IsHorizontalSide => Side is SidePanelSide.Left or SidePanelSide.Right;

    private DependencyProperty GetSlideAxis()
    {
        return IsHorizontalSide ? TranslateTransform.XProperty : TranslateTransform.YProperty;
    }

    /// <summary>收起状态下内容相对父容器的偏移量（完全滑出可视区域）。</summary>
    private double GetClosedOffset()
    {
        if (_content is null)
        {
            return 0d;
        }

        return Side switch
        {
            SidePanelSide.Left => -_content.ActualWidth,
            SidePanelSide.Right => _content.ActualWidth,
            SidePanelSide.Top => -_content.ActualHeight,
            SidePanelSide.Bottom => _content.ActualHeight,
            _ => 0d,
        };
    }

    private void SetSlideOffset(double offset)
    {
        if (_translate is null)
        {
            return;
        }

        if (IsHorizontalSide)
        {
            _translate.X = offset;
        }
        else
        {
            _translate.Y = offset;
        }
    }

    /// <summary>应用滑出/收回状态；animate 为 true 时执行滑动 + 淡入淡出过渡。</summary>
    private void ApplySlideState(bool animate)
    {
        if (_content is null || _translate is null)
        {
            return;
        }

        DependencyProperty axis = GetSlideAxis();
        double closedOffset = GetClosedOffset();
        _stateVersion++;
        int version = _stateVersion;

        // 停掉两个方向上残留的动画，避免中途反向切换或切换方向时相互干扰。
        _translate.BeginAnimation(TranslateTransform.XProperty, null);
        _translate.BeginAnimation(TranslateTransform.YProperty, null);
        _content.BeginAnimation(OpacityProperty, null);

        TimeSpan duration = AnimationDuration.HasTimeSpan ? AnimationDuration.TimeSpan : TimeSpan.Zero;
        if (!animate || duration <= TimeSpan.Zero)
        {
            SetSlideOffset(IsOpen ? 0d : closedOffset);
            _content.Opacity = IsOpen ? 1d : 0d;
            return;
        }

        bool isOpen = IsOpen;
        var slideAnimation = new DoubleAnimation(isOpen ? closedOffset : 0d, isOpen ? 0d : closedOffset, duration)
        {
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };
        var fadeAnimation = new DoubleAnimation(isOpen ? 0d : 1d, isOpen ? 1d : 0d, duration)
        {
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };

        slideAnimation.Completed += (_, _) =>
        {
            if (version != _stateVersion)
            {
                return;
            }

            _translate.BeginAnimation(axis, null);
            SetSlideOffset(isOpen ? 0d : closedOffset);
            _content.BeginAnimation(OpacityProperty, null);
            _content.Opacity = isOpen ? 1d : 0d;
            RaiseStateChangedEvent(isOpen);
        };

        _translate.BeginAnimation(axis, slideAnimation);
        _content.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    /// <summary>应用遮罩层状态；animate 为 true 时执行透明度过渡，收回完成后折叠遮罩。</summary>
    private void ApplyBackdropState(bool animate)
    {
        if (_backdrop is null)
        {
            return;
        }

        _backdrop.BeginAnimation(OpacityProperty, null);

        if (!IsBackdropEnabled)
        {
            _backdrop.Visibility = Visibility.Collapsed;
            return;
        }

        TimeSpan duration = AnimationDuration.HasTimeSpan ? AnimationDuration.TimeSpan : TimeSpan.Zero;
        if (!animate || duration <= TimeSpan.Zero)
        {
            _backdrop.Opacity = IsOpen ? 1d : 0d;
            _backdrop.Visibility = IsOpen ? Visibility.Visible : Visibility.Collapsed;
            return;
        }

        _backdrop.Visibility = Visibility.Visible;
        var fadeAnimation = new DoubleAnimation(IsOpen ? 1d : 0d, duration)
        {
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };

        if (!IsOpen)
        {
            // 只按 IsOpen 判断：收回途中被再次打开时由新的状态应用流程接管，不做版本校验，
            // 避免校验失效导致遮罩以透明度 0 的形式残留并持续拦截点击。
            fadeAnimation.Completed += (_, _) =>
            {
                if (!IsOpen)
                {
                    _backdrop.Visibility = Visibility.Collapsed;
                }
            };
        }

        _backdrop.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void OnContentSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // 收起状态下内容尺寸变化时同步收回偏移量，保证面板始终完全滑出可视区域。
        if (!IsOpen)
        {
            ApplySlideState(animate: false);
        }
    }

    private void RaiseStateChangedEvent(bool isOpen)
    {
        RaiseEvent(new RoutedEventArgs(isOpen ? OpenedEvent : ClosedEvent, this));
    }
}
