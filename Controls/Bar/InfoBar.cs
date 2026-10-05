using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Bar
{
    /// <summary>
    /// 用户信息条：左侧头像（图片，或名称首字符的圆形字标），右侧为名称与可选的设置按钮
    /// （<see cref="InfoBarDisplayMode.Text"/>，默认）；或仅头像、悬停经 ToolTip 显示名称
    /// （<see cref="InfoBarDisplayMode.AvatarOnly"/>）。
    /// <para>
    /// 点击弹出菜单（与控件等宽的浮层）：菜单项可在 XAML 中直接编写（作为控件的 Items），
    /// 也可经 <c>ItemsSource</c> 绑定一个列表（项模板用 <c>ItemTemplate</c> 定制）。
    /// 菜单默认向上展开（适合侧栏 / 标题栏底部，<see cref="MenuPlacement"/> 可换），
    /// 点菜单外部或再次点击收起；<see cref="AutoCloseOnMenuClick"/>=true 时点击菜单内的按钮项自动收起。
    /// 设置按钮点击冒泡 <see cref="SettingsClick"/> 路由事件。
    /// </para>
    /// </summary>
    [TemplatePart(Name = PartRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PartSettingsButton, Type = typeof(ButtonBase))]
    [TemplatePart(Name = PartMenuPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PartMenuHost, Type = typeof(FrameworkElement))]
    public class InfoBar : ItemsControl
    {
        private const string PartRoot = "PART_Root";
        private const string PartSettingsButton = "PART_SettingsButton";
        private const string PartMenuPopup = "PART_MenuPopup";
        private const string PartMenuHost = "PART_MenuHost";

        // StaysOpen=False 的 Popup 收起时会放行落点上的鼠标消息：菜单在本控件上收起后，
        // 本控件仍会收到一次完整 MouseDown。记录收起时刻，短窗口内的「再次打开」视为余波忽略。
        // 时间差用 long 运算（Tick 会回绕，不可用任何 Min/Max 哨兵值做减法）。
        private const int MenuReopenGuardMillis = 250;

        private FrameworkElement? _root;
        private FrameworkElement? _menuHost;
        private ButtonBase? _settingsButton;
        private long? _menuClosedAtTickCount;

        /// <summary>标识 <see cref="SettingsClick"/> 的路由事件。</summary>
        public static readonly RoutedEvent SettingsClickEvent =
            EventManager.RegisterRoutedEvent(
                nameof(SettingsClick),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(InfoBar));

        /// <summary>点击设置按钮时触发。</summary>
        public event RoutedEventHandler SettingsClick
        {
            add => AddHandler(SettingsClickEvent, value);
            remove => RemoveHandler(SettingsClickEvent, value);
        }

        static InfoBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoBar), new FrameworkPropertyMetadata(typeof(InfoBar)));
        }

        /// <summary>控件尺寸变化时同步等宽弹层的宽度。</summary>
        public InfoBar()
        {
            SizeChanged += (_, _) => UpdateMenuSize();
        }

        /// <summary>用户名称：Text 布局显示在头像右侧；AvatarOnly 布局经 ToolTip 显示；同时决定字标首字符。
        /// （不叫 Name——与 FrameworkElement.Name 冲突，XAML 里会被当成元素名。）</summary>
        public string? UserName
        {
            get => (string?)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }

        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register(
                nameof(UserName),
                typeof(string),
                typeof(InfoBar),
                new PropertyMetadata(null, OnUserNameChanged));

        /// <summary>头像图片（圆形裁切）；为 <see langword="null"/> 时显示名称首字符的圆形字标。</summary>
        public ImageSource? AvatarSource
        {
            get => (ImageSource?)GetValue(AvatarSourceProperty);
            set => SetValue(AvatarSourceProperty, value);
        }

        public static readonly DependencyProperty AvatarSourceProperty =
            DependencyProperty.Register(
                nameof(AvatarSource),
                typeof(ImageSource),
                typeof(InfoBar),
                new PropertyMetadata(null));

        /// <summary>显示布局（默认 <see cref="InfoBarDisplayMode.Text"/>）。</summary>
        public InfoBarDisplayMode DisplayMode
        {
            get => (InfoBarDisplayMode)GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(
                nameof(DisplayMode),
                typeof(InfoBarDisplayMode),
                typeof(InfoBar),
                new PropertyMetadata(InfoBarDisplayMode.Text));

        /// <summary>是否显示设置按钮（Text 布局；点击触发 <see cref="SettingsClick"/>）。</summary>
        public bool ShowSettingsButton
        {
            get => (bool)GetValue(ShowSettingsButtonProperty);
            set => SetValue(ShowSettingsButtonProperty, value);
        }

        public static readonly DependencyProperty ShowSettingsButtonProperty =
            DependencyProperty.Register(
                nameof(ShowSettingsButton),
                typeof(bool),
                typeof(InfoBar),
                new PropertyMetadata(true));

        /// <summary>
        /// 菜单是否展开。模板内的 <see cref="Popup"/> 双向绑定该值：
        /// 点菜单外部收起时（StaysOpen=False 自动关闭）会同步写回 <see langword="false"/>。
        /// </summary>
        public bool IsMenuOpen
        {
            get => (bool)GetValue(IsMenuOpenProperty);
            set => SetValue(IsMenuOpenProperty, value);
        }

        public static readonly DependencyProperty IsMenuOpenProperty =
            DependencyProperty.Register(
                nameof(IsMenuOpen),
                typeof(bool),
                typeof(InfoBar),
                new PropertyMetadata(false, OnIsMenuOpenChanged));

        /// <summary>菜单相对控件的弹出方位（默认 <see cref="PlacementMode.Top"/>，适合侧栏底部）。</summary>
        public PlacementMode MenuPlacement
        {
            get => (PlacementMode)GetValue(MenuPlacementProperty);
            set => SetValue(MenuPlacementProperty, value);
        }

        public static readonly DependencyProperty MenuPlacementProperty =
            DependencyProperty.Register(
                nameof(MenuPlacement),
                typeof(PlacementMode),
                typeof(InfoBar),
                new PropertyMetadata(PlacementMode.Top));

        /// <summary>
        /// 为 <see langword="true"/>（默认）时，点击菜单内的按钮项自动收起菜单
        /// （按钮的 Click 是冒泡路由事件，经菜单宿主统一监听）；列表项等非按钮点击不收起。
        /// </summary>
        public bool AutoCloseOnMenuClick
        {
            get => (bool)GetValue(AutoCloseOnMenuClickProperty);
            set => SetValue(AutoCloseOnMenuClickProperty, value);
        }

        public static readonly DependencyProperty AutoCloseOnMenuClickProperty =
            DependencyProperty.Register(
                nameof(AutoCloseOnMenuClick),
                typeof(bool),
                typeof(InfoBar),
                new PropertyMetadata(true));

        /// <summary>
        /// 弹出面板的任意内容（<see cref="ListBox"/>、<see cref="UserControl"/>、复杂布局等），
        /// 设置后弹层以它替代菜单项列表呈现，宽度随内容自适应（Popup 窗口语义，
        /// 不受父容器与控件自身尺寸约束），需要定宽时显式设置 <see cref="MenuWidth"/>。
        /// 为 <see langword="null"/> 时弹层呈现 <c>Items</c> / <c>ItemsSource</c> 的菜单项。
        /// </summary>
        public object? MenuContent
        {
            get => GetValue(MenuContentProperty);
            set => SetValue(MenuContentProperty, value);
        }

        public static readonly DependencyProperty MenuContentProperty =
            DependencyProperty.Register(
                nameof(MenuContent),
                typeof(object),
                typeof(InfoBar),
                new PropertyMetadata(null, OnMenuSizeChanged));

        /// <summary>
        /// 弹层宽度（DIP）；<see cref="double.NaN"/>（默认）时，菜单项模式与控件等宽，
        /// <see cref="MenuContent"/> 面板模式随内容自适应。
        /// </summary>
        public double MenuWidth
        {
            get => (double)GetValue(MenuWidthProperty);
            set => SetValue(MenuWidthProperty, value);
        }

        public static readonly DependencyProperty MenuWidthProperty =
            DependencyProperty.Register(
                nameof(MenuWidth),
                typeof(double),
                typeof(InfoBar),
                new PropertyMetadata(double.NaN, OnMenuSizeChanged));

        /// <summary>
        /// 弹层最大高度（DIP）；<see cref="double.NaN"/>（默认）时不限制。
        /// 列表项较多时建议设置，避免弹层超出屏幕。
        /// </summary>
        public double MenuMaxHeight
        {
            get => (double)GetValue(MenuMaxHeightProperty);
            set => SetValue(MenuMaxHeightProperty, value);
        }

        public static readonly DependencyProperty MenuMaxHeightProperty =
            DependencyProperty.Register(
                nameof(MenuMaxHeight),
                typeof(double),
                typeof(InfoBar),
                new PropertyMetadata(double.NaN, OnMenuSizeChanged));

        /// <summary>头像圆形字标（名称首字符大写），由 <see cref="UserName"/> 派生，模板内部使用。</summary>
        public string AvatarLetter
        {
            get => (string)GetValue(AvatarLetterProperty);
            set => SetValue(AvatarLetterProperty, value);
        }

        public static readonly DependencyProperty AvatarLetterProperty =
            DependencyProperty.Register(
                nameof(AvatarLetter),
                typeof(string),
                typeof(InfoBar),
                new PropertyMetadata("?"));

        public override void OnApplyTemplate()
        {
            if (_root != null)
            {
                _root.MouseLeftButtonUp -= OnRootMouseLeftButtonUp;
                _root = null;
            }

            if (_menuHost != null)
            {
                _menuHost.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnMenuItemClick));
                _menuHost = null;
            }

            base.OnApplyTemplate();

            _root = GetTemplateChild(PartRoot) as FrameworkElement;
            if (_root != null)
            {
                _root.MouseLeftButtonUp += OnRootMouseLeftButtonUp;
            }

            _menuHost = GetTemplateChild(PartMenuHost) as FrameworkElement;
            if (_menuHost != null)
            {
                _menuHost.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnMenuItemClick));
                UpdateMenuSize();
            }

            _settingsButton = GetTemplateChild(PartSettingsButton) as ButtonBase;
            if (_settingsButton != null)
            {
                _settingsButton.Click += OnSettingsButtonClick;
            }
        }

        /// <summary>
        /// 数据项（字符串 / 模型）自动包装为整行按钮容器（可点击；点击冒泡
        /// <see cref="ButtonBase.Click"/>，经菜单宿主触发自动收起，
        /// 宿主也可在 InfoBar 上监听 <see cref="ButtonBase.Click"/> 并经
        /// <c>OriginalSource.DataContext</c> 区分菜单项）；XAML 中直接编写的元素
        /// （jv:Button / 分隔线等 UIElement）按原样使用，不再包装。
        /// </summary>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is UIElement;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new System.Windows.Controls.Button();
        }

        private static void OnMenuSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBar)d).UpdateMenuSize();
        }

        /// <summary>
        /// 按 MenuWidth / MenuMaxHeight 同步弹层宿主尺寸：
        /// MenuWidth 显式设置时定宽；NaN 时菜单项模式回退为与控件等宽，
        /// MenuContent 面板模式保持 NaN（随内容自适应，Popup 窗口语义，
        /// 弹层经 PlacementTarget 定位、尺寸不受父容器约束）。
        /// </summary>
        private void UpdateMenuSize()
        {
            if (_menuHost == null)
            {
                return;
            }

            _menuHost.Width = !double.IsNaN(MenuWidth)
                ? MenuWidth
                : MenuContent != null ? double.NaN : ActualWidth;
            _menuHost.MaxHeight = double.IsNaN(MenuMaxHeight) ? double.PositiveInfinity : MenuMaxHeight;
        }

        private static void OnUserNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var text = e.NewValue as string;
            var infoBar = (InfoBar)d;
            infoBar.AvatarLetter = string.IsNullOrWhiteSpace(text)
                ? "?"
                : char.ToUpperInvariant(text!.Trim()[0]).ToString();
        }

        private static void OnIsMenuOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                ((InfoBar)d)._menuClosedAtTickCount = Environment.TickCount;
            }
        }

        private void OnRootMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Items.Count == 0 && ItemsSource == null && MenuContent == null)
            {
                return;
            }

            // 菜单在鼠标抬起时弹出（若在按下阶段开 Popup，StaysOpen=False 的捕获
            // 会与进行中的点击手势冲突——菜单随松键立即关闭）。
            // 本控件上收起菜单后，放行的鼠标消息仍会产生一次 MouseUp——
            // 收起后短窗口内的「再次打开」是同一次交互的余波，忽略（防重复弹出）。
            // 时间差用 long 运算（Tick 会回绕，不可用任何 Min/Max 哨兵值做减法）。
            var closedAgo = _menuClosedAtTickCount.HasValue
                ? (long)Environment.TickCount - _menuClosedAtTickCount.Value
                : (long?)null;
            if (!IsMenuOpen && closedAgo.HasValue && closedAgo.Value < MenuReopenGuardMillis)
            {
                return;
            }

            SetCurrentValue(IsMenuOpenProperty, !IsMenuOpen);
            e.Handled = true;
        }

        private void OnMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (AutoCloseOnMenuClick)
            {
                SetCurrentValue(IsMenuOpenProperty, false);
            }
        }

        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SettingsClickEvent, this));
        }
    }
}
