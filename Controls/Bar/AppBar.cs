using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Bar
{
    /// <summary>
    /// 应用标题栏：提供应用图标、标题以及最小化、最大化/还原、关闭按钮。
    /// 布局经 <see cref="Mode"/>（<see cref="AppBarMode"/>）切换：
    /// <see cref="AppBarMode.Default"/>（图标 + 标题 / 分隔线 / 工具栏）、
    /// <see cref="AppBarMode.MenuBar"/>（图标 + 应用名 + 菜单栏 + 系统按钮）、
    /// <see cref="AppBarMode.Expandable"/>（抽屉开关 + 居中标题 + 系统按钮，
    /// 配合 <see cref="Drawer"/> 在标题栏下方展开悬浮抽屉）。
    /// </summary>
    [TemplatePart(Name = PartDrawerToggle, Type = typeof(ButtonBase))]
    [TemplatePart(Name = PartDrawerHost, Type = typeof(Border))]
    public class AppBar : System.Windows.Controls.ContentControl
    {
        private const string PartDrawerToggle = "PART_DrawerToggle";
        private const string PartDrawerHost = "PART_DrawerHost";

        // StaysOpen=False 的 Popup 收起时会放行落点上的鼠标消息：在开关按钮上收起抽屉后，
        // 开关仍会收到一次完整 Click。记录收起时刻，短窗口内的「再次打开」一律视为余波忽略。
        private const int DrawerReopenGuardMillis = 250;

        private ButtonBase? _drawerToggle;
        private Border? _drawerHost;
        private long? _drawerClosedAtTickCount;
        private bool _windowCommandsRequested;

        static AppBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBar), new FrameworkPropertyMetadata(typeof(AppBar)));
        }

        public AppBar()
        {
            Loaded += (_, _) => EnsureWindowCommands();
        }

        public override void OnApplyTemplate()
        {
            if (_drawerToggle != null)
            {
                _drawerToggle.Click -= OnDrawerToggleClick;
                _drawerToggle = null;
            }

            base.OnApplyTemplate();

            _drawerToggle = GetTemplateChild(PartDrawerToggle) as ButtonBase;
            if (_drawerToggle != null)
            {
                _drawerToggle.Click += OnDrawerToggleClick;
            }

            _drawerHost = GetTemplateChild(PartDrawerHost) as Border;

            EnsureWindowCommands();
        }

        // The caption buttons in the templates are bound to SystemCommands, which WPF never
        // implements, so the host window has to carry the bindings for them to stay enabled.
        private void EnsureWindowCommands()
        {
            if (_windowCommandsRequested)
            {
                return;
            }

            var window = Window.GetWindow(this);
            if (window == null)
            {
                return;
            }

            _windowCommandsRequested = true;
            WindowSystemCommands.EnsureRegistered(window);
        }

        /// <summary>布局模式，切换模板（默认 <see cref="AppBarMode.Default"/>）。</summary>
        public AppBarMode Mode
        {
            get => (AppBarMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(
                nameof(Mode),
                typeof(AppBarMode),
                typeof(AppBar),
                new PropertyMetadata(AppBarMode.Default));

        public ToolBar? ToolBar
        {
            get => (ToolBar?)GetValue(ToolBarProperty);
            set => SetValue(ToolBarProperty, value);
        }

        public static readonly DependencyProperty ToolBarProperty =
            DependencyProperty.Register(
                nameof(ToolBar),
                typeof(ToolBar),
                typeof(AppBar));

        // The type stays fully qualified: the sibling namespace
        // Junevy.Controls.Controls.Menu shadows System.Windows.Controls.Menu
        // inside Junevy.Controls.Controls.Bar.
        public System.Windows.Controls.Menu? Menu
        {
            get => (System.Windows.Controls.Menu?)GetValue(MenuProperty);
            set => SetValue(MenuProperty, value);
        }

        public static readonly DependencyProperty MenuProperty =
            DependencyProperty.Register(
                nameof(Menu),
                typeof(System.Windows.Controls.Menu),
                typeof(AppBar),
                new PropertyMetadata(null));

        /// <summary>
        /// 抽屉内容（任意 object，尺寸由内容决定），仅 <see cref="AppBarMode.Expandable"/> 呈现；
        /// 为 <see langword="null"/> 时抽屉开关自动隐藏。
        /// </summary>
        public object? Drawer
        {
            get => GetValue(DrawerProperty);
            set => SetValue(DrawerProperty, value);
        }

        public static readonly DependencyProperty DrawerProperty =
            DependencyProperty.Register(
                nameof(Drawer),
                typeof(object),
                typeof(AppBar),
                new PropertyMetadata(null));

        /// <summary>
        /// 抽屉是否展开。模板内的 <see cref="Popup"/> 双向绑定该值：
        /// 点抽屉外部收起时（StaysOpen=False 自动关闭）会同步写回 <see langword="false"/>。
        /// 仅 <see cref="AppBarMode.Expandable"/> 呈现。
        /// </summary>
        public bool IsDrawerOpen
        {
            get => (bool)GetValue(IsDrawerOpenProperty);
            set => SetValue(IsDrawerOpenProperty, value);
        }

        public static readonly DependencyProperty IsDrawerOpenProperty =
            DependencyProperty.Register(
                nameof(IsDrawerOpen),
                typeof(bool),
                typeof(AppBar),
                new PropertyMetadata(false, OnIsDrawerOpenChanged));

        /// <summary>
        /// 为 <see langword="true"/> 时，点击抽屉内容中未被处理的左键（如列表项）自动收起抽屉；
        /// 按钮等自己处理鼠标按下的控件不会触发收起。
        /// </summary>
        public bool AutoCloseOnDrawerClick
        {
            get => (bool)GetValue(AutoCloseOnDrawerClickProperty);
            set => SetValue(AutoCloseOnDrawerClickProperty, value);
        }

        public static readonly DependencyProperty AutoCloseOnDrawerClickProperty =
            DependencyProperty.Register(
                nameof(AutoCloseOnDrawerClick),
                typeof(bool),
                typeof(AppBar),
                new PropertyMetadata(false));

        /// <summary>抽屉开关按钮的 iconfont 字形（默认为菜单图标）。</summary>
        public string? DrawerToggleIcon
        {
            get => (string?)GetValue(DrawerToggleIconProperty);
            set => SetValue(DrawerToggleIconProperty, value);
        }

        public static readonly DependencyProperty DrawerToggleIconProperty =
            DependencyProperty.Register(
                nameof(DrawerToggleIcon),
                typeof(string),
                typeof(AppBar),
                new PropertyMetadata("\uE65D"));

        private static void OnIsDrawerOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                ((AppBar)d)._drawerClosedAtTickCount = Environment.TickCount;
            }
        }

        private void OnDrawerToggleClick(object sender, RoutedEventArgs e)
        {
            // 开关按钮上收起抽屉后，放行的鼠标消息仍会让按钮产生一次 Click——
            // 收起后短窗口内的「再次打开」是同一次交互的余波，忽略（防重复展开）。
            // 时间差用 long 运算（Tick 会回绕，不可用任何 Min/Max 哨兵值做减法）。
            var closedAgo = _drawerClosedAtTickCount.HasValue
                ? (long)Environment.TickCount - _drawerClosedAtTickCount.Value
                : (long?)null;
            if (!IsDrawerOpen && closedAgo.HasValue && closedAgo.Value < DrawerReopenGuardMillis)
            {
                return;
            }

            SetCurrentValue(IsDrawerOpenProperty, !IsDrawerOpen);
        }
    }
}
