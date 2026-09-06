using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;

namespace Junevy.Controls.Controls.Dialog
{
    /// <summary>
    /// Junevy 风格的无边框对话框窗口：圆角、阴影、主题化标题栏，颜色全部跟随
    /// <c>Theme.Brush.*</c> 动态资源，支持运行时浅色/深色切换。
    ///
    /// 窗口内容与 DataContext 由宿主注入（例如 Prism DialogService 会设置
    /// <c>Content</c> 和 <c>DataContext</c>）；标题自动读取 DataContext 上的
    /// <c>Title</c> 属性（如实现 <c>IDialogAware</c> 的 ViewModel），并监听
    /// <c>INotifyPropertyChanged</c> 同步刷新。
    ///
    /// 宿主项目接入 Prism 时，无需本库引用 Prism，只需派生并补一个 Result 属性：
    /// <code>
    /// public class PrismDialogWindow : DialogWindow, IDialogWindow
    /// {
    ///     public IDialogResult Result { get; set; }
    /// }
    /// containerRegistry.RegisterDialogWindow&lt;PrismDialogWindow&gt;();
    /// </code>
    /// </summary>
    public class DialogWindow : Window
    {
        private const string PartClipGrid = "PART_ClipGrid";

        private WindowChrome? _chrome;
        private FrameworkElement? _clipElement;
        private INotifyPropertyChanged? _trackedViewModel;

        static DialogWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DialogWindow),
                new FrameworkPropertyMetadata(typeof(DialogWindow)));
        }

        public DialogWindow()
        {
            // 窗框关键属性在构造函数中设置，避免被子类样式覆盖后失效。
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            ShowActivated = true;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;

            _chrome = new WindowChrome
            {
                CaptionHeight = TitleBarHeight,
                ResizeBorderThickness = new Thickness(ShadowMargin.Left),
                GlassFrameThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                UseAeroCaptionButtons = false
            };
            WindowChrome.SetWindowChrome(this, _chrome);

            CommandBindings.Add(new CommandBinding(
                SystemCommands.MinimizeWindowCommand, (_, _) => SystemCommands.MinimizeWindow(this)));
            CommandBindings.Add(new CommandBinding(
                SystemCommands.MaximizeWindowCommand, (_, _) => SystemCommands.MaximizeWindow(this)));
            CommandBindings.Add(new CommandBinding(
                SystemCommands.RestoreWindowCommand, (_, _) => SystemCommands.RestoreWindow(this)));
            CommandBindings.Add(new CommandBinding(
                SystemCommands.CloseWindowCommand, (_, _) => Close()));

            // Esc 关闭与 ProgressBarWindow 保持一致；焦点在文本输入框内时不关闭，
            // 宿主（如 Prism）仍可通过 Closing 事件按 CanCloseDialog 拦截。
            PreviewKeyDown += (_, e) =>
            {
                if (e.Key != Key.Escape || !ShowCloseButton)
                {
                    return;
                }

                if (Keyboard.FocusedElement is TextBoxBase or PasswordBox)
                {
                    return;
                }

                Close();
            };

            DataContextChanged += OnDataContextChanged;
            StateChanged += OnWindowStateChanged;
        }

        #region 可配置项

        /// <summary>标题栏高度，同时作为 WindowChrome 的拖拽区高度。</summary>
        public static readonly DependencyProperty TitleBarHeightProperty = DependencyProperty.Register(
            nameof(TitleBarHeight), typeof(double), typeof(DialogWindow),
            new PropertyMetadata(40d, OnTitleBarHeightChanged));

        public double TitleBarHeight
        {
            get => (double)GetValue(TitleBarHeightProperty);
            set => SetValue(TitleBarHeightProperty, value);
        }

        /// <summary>
        /// 窗口四周为阴影保留的透明外边距。拖拽（标题栏）之外的可视边缘
        /// 通过 WindowChrome 的 ResizeBorderThickness 与该值对齐。
        /// </summary>
        public static readonly DependencyProperty ShadowMarginProperty = DependencyProperty.Register(
            nameof(ShadowMargin), typeof(Thickness), typeof(DialogWindow),
            new PropertyMetadata(new Thickness(16d), OnShadowMarginChanged));

        public Thickness ShadowMargin
        {
            get => (Thickness)GetValue(ShadowMarginProperty);
            set => SetValue(ShadowMarginProperty, value);
        }

        /// <summary>窗口圆角（最大化时自动归零）。</summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(DialogWindow),
            new PropertyMetadata(new CornerRadius(6d), OnCornerRadiusChanged));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty ShowMinimizeButtonProperty = DependencyProperty.Register(
            nameof(ShowMinimizeButton), typeof(bool), typeof(DialogWindow), new PropertyMetadata(false));

        public bool ShowMinimizeButton
        {
            get => (bool)GetValue(ShowMinimizeButtonProperty);
            set => SetValue(ShowMinimizeButtonProperty, value);
        }

        public static readonly DependencyProperty ShowMaximizeButtonProperty = DependencyProperty.Register(
            nameof(ShowMaximizeButton), typeof(bool), typeof(DialogWindow), new PropertyMetadata(false));

        public bool ShowMaximizeButton
        {
            get => (bool)GetValue(ShowMaximizeButtonProperty);
            set => SetValue(ShowMaximizeButtonProperty, value);
        }

        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register(
            nameof(ShowCloseButton), typeof(bool), typeof(DialogWindow), new PropertyMetadata(true));

        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_clipElement != null)
            {
                _clipElement.SizeChanged -= OnClipElementSizeChanged;
            }

            _clipElement = GetTemplateChild(PartClipGrid) as FrameworkElement;
            if (_clipElement != null)
            {
                _clipElement.SizeChanged += OnClipElementSizeChanged;
            }

            UpdateContentClip();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_trackedViewModel != null)
            {
                _trackedViewModel.PropertyChanged -= OnTrackedViewModelPropertyChanged;
                _trackedViewModel = null;
            }
        }

        private static void OnTitleBarHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogWindow window && window._chrome != null)
            {
                window._chrome.CaptionHeight = (double)e.NewValue;
            }
        }

        private static void OnShadowMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogWindow window && e.NewValue is Thickness margin)
            {
                if (window._chrome != null)
                {
                    window._chrome.ResizeBorderThickness = new Thickness(margin.Left);
                }
                window.SyncMaximizedPadding();
            }
        }

        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogWindow window)
            {
                window.UpdateContentClip();
            }
        }

        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            SyncMaximizedPadding();
            UpdateContentClip();
        }

        private void OnClipElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateContentClip();
        }

        /// <summary>
        /// 最大化时无边框窗口会超出屏幕一个 ResizeBorderThickness，
        /// 这里用 Padding 抵消（因此该窗口的 Padding 由内部管理）。
        /// </summary>
        private void SyncMaximizedPadding()
        {
            Padding = WindowState == WindowState.Maximized
                ? new Thickness(ShadowMargin.Left)
                : default(Thickness);
        }

        /// <summary>按圆角裁剪内容区，避免内容自带背景顶破窗口圆角。</summary>
        private void UpdateContentClip()
        {
            if (_clipElement == null)
            {
                return;
            }

            double radius = WindowState == WindowState.Maximized ? 0d : CornerRadius.TopLeft;
            if (radius <= 0d)
            {
                _clipElement.Clip = null;
                return;
            }

            var bounds = new Rect(0d, 0d, _clipElement.ActualWidth, _clipElement.ActualHeight);
            _clipElement.Clip = new RectangleGeometry(bounds, radius, radius);
        }

        #region 标题同步（读取 DataContext.Title，不依赖 Prism）

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_trackedViewModel != null)
            {
                _trackedViewModel.PropertyChanged -= OnTrackedViewModelPropertyChanged;
                _trackedViewModel = null;
            }

            if (e.NewValue is INotifyPropertyChanged notifyPropertyChanged)
            {
                _trackedViewModel = notifyPropertyChanged;
                _trackedViewModel.PropertyChanged += OnTrackedViewModelPropertyChanged;
            }

            SyncTitleFromDataContext();
        }

        private void OnTrackedViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Title) || string.IsNullOrEmpty(e.PropertyName))
            {
                SyncTitleFromDataContext();
            }
        }

        private void SyncTitleFromDataContext()
        {
            object? dataContext = DataContext;
            if (dataContext == null)
            {
                return;
            }

            PropertyInfo? titleProperty = dataContext.GetType().GetProperty(
                nameof(Title), BindingFlags.Public | BindingFlags.Instance);
            if (titleProperty == null || titleProperty.PropertyType != typeof(string))
            {
                return;
            }

            if (titleProperty.GetValue(dataContext) is string title && title.Length > 0)
            {
                Title = title;
            }
        }

        #endregion
    }
}
