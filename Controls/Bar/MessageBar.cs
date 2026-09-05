using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Bar
{
    /// <summary>
    /// 应用内通知条。参考 WPF-UI 的 <c>Snackbar</c>：
    /// 通过 <see cref="IsShown"/> 控制显示/隐藏，带滑入滑出动画，
    /// 可按 <see cref="Timeout"/> 自动关闭，并触发 Opening/Opened/Closing/Closed 路由事件。
    /// </summary>
    public class MessageBar : ContentControl
    {
        /// <summary>Informational 状态的默认图标字体字符。</summary>
        public const string InformationalIconGlyph = "\uE651";

        /// <summary>Success 状态的默认图标字体字符。</summary>
        public const string SuccessIconGlyph = "\uE613";

        /// <summary>Warning 状态的默认图标字体字符。</summary>
        public const string WarningIconGlyph = "\uE932";

        /// <summary>Danger 状态的默认图标字体字符。</summary>
        public const string DangerIconGlyph = "\uE61A";

        private const double SlideOffset = 16;
        private static readonly Duration AnimationDuration = new(TimeSpan.FromMilliseconds(180));

        private readonly TranslateTransform _slideTransform = new(0, SlideOffset);
        private DispatcherTimer? _closeTimer;
        private bool _suppressIsShownChanged;

        static MessageBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MessageBar),
                new FrameworkPropertyMetadata(typeof(MessageBar)));
        }

        public MessageBar()
        {
            RenderTransform = _slideTransform;
            Visibility = Visibility.Collapsed;
            Opacity = 0;
            CloseCommand = new ActionCommand(Hide);
            UpdateDefaultIcon();
            Loaded += (_, _) =>
            {
                if (IsShown)
                {
                    RestartCloseTimer();
                }
            };
            Unloaded += (_, _) => StopCloseTimer();
        }

        #region Routed events

        /// <summary>开始显示之前触发，可取消。</summary>
        public static readonly RoutedEvent OpeningEvent = EventManager.RegisterRoutedEvent(
            nameof(Opening), RoutingStrategy.Bubble, typeof(MessageBarCancelEventHandler), typeof(MessageBar));

        /// <summary>显示动画完成后触发。</summary>
        public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent(
            nameof(Opened), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MessageBar));

        /// <summary>开始隐藏之前触发（自动关闭同样触发），可取消。</summary>
        public static readonly RoutedEvent ClosingEvent = EventManager.RegisterRoutedEvent(
            nameof(Closing), RoutingStrategy.Bubble, typeof(MessageBarCancelEventHandler), typeof(MessageBar));

        /// <summary>隐藏动画完成后触发。</summary>
        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
            nameof(Closed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MessageBar));

        public event MessageBarCancelEventHandler Opening
        {
            add => AddHandler(OpeningEvent, value);
            remove => RemoveHandler(OpeningEvent, value);
        }

        public event RoutedEventHandler Opened
        {
            add => AddHandler(OpenedEvent, value);
            remove => RemoveHandler(OpenedEvent, value);
        }

        public event MessageBarCancelEventHandler Closing
        {
            add => AddHandler(ClosingEvent, value);
            remove => RemoveHandler(ClosingEvent, value);
        }

        public event RoutedEventHandler Closed
        {
            add => AddHandler(ClosedEvent, value);
            remove => RemoveHandler(ClosedEvent, value);
        }

        #endregion

        #region Dependency properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(object), typeof(MessageBar), new PropertyMetadata(null));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(object), typeof(MessageBar), new PropertyMetadata(null));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(object), typeof(MessageBar), new PropertyMetadata(null));

        public static readonly DependencyProperty AppearanceProperty =
            DependencyProperty.Register(
                nameof(Appearance),
                typeof(MessageBarAppearance),
                typeof(MessageBar),
                new PropertyMetadata(MessageBarAppearance.Informational, OnAppearanceChanged));

        public static readonly DependencyProperty IsShownProperty =
            DependencyProperty.Register(nameof(IsShown), typeof(bool), typeof(MessageBar), new PropertyMetadata(false, OnIsShownChanged));

        public static readonly DependencyProperty TimeoutProperty =
            DependencyProperty.Register(
                nameof(Timeout),
                typeof(TimeSpan),
                typeof(MessageBar),
                new PropertyMetadata(TimeSpan.FromSeconds(2), OnTimeoutChanged),
                value => (TimeSpan)value >= TimeSpan.Zero);

        public static readonly DependencyProperty CloseButtonEnabledProperty =
            DependencyProperty.Register(nameof(CloseButtonEnabled), typeof(bool), typeof(MessageBar), new PropertyMetadata(true));

        /// <summary>标题行，为 <c>null</c> 时不显示。可绑定任意对象。</summary>
        public object? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>正文内容，可绑定任意对象。</summary>
        public object? Message
        {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        /// <summary>
        /// 图标内容，通常为图标字体字符。更改 <see cref="Appearance"/> 时，
        /// 若当前值尚未被用户显式指定（或仍是内置默认字形），会自动切换为对应状态的默认图标。
        /// </summary>
        public object? Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>严重程度外观，决定左侧色条与图标颜色。</summary>
        public MessageBarAppearance Appearance
        {
            get => (MessageBarAppearance)GetValue(AppearanceProperty);
            set => SetValue(AppearanceProperty, value);
        }

        /// <summary>是否显示。设置后触发滑入/滑出动画与 Opening/Closing 等事件。</summary>
        public bool IsShown
        {
            get => (bool)GetValue(IsShownProperty);
            set => SetValue(IsShownProperty, value);
        }

        /// <summary>自动关闭的等待时间；小于或等于 <see cref="TimeSpan.Zero"/> 时禁用自动关闭。默认 2 秒。</summary>
        public TimeSpan Timeout
        {
            get => (TimeSpan)GetValue(TimeoutProperty);
            set => SetValue(TimeoutProperty, value);
        }

        /// <summary>是否显示关闭按钮，默认 <c>true</c>。</summary>
        public bool CloseButtonEnabled
        {
            get => (bool)GetValue(CloseButtonEnabledProperty);
            set => SetValue(CloseButtonEnabledProperty, value);
        }

        #endregion

        /// <summary>模板内关闭按钮使用的命令。</summary>
        public ICommand CloseCommand { get; }

        /// <summary>显示通知（等同设置 <see cref="IsShown"/> 为 <c>true</c>）。</summary>
        public void Show()
        {
            if (IsShown)
            {
                return;
            }

            SetCurrentValue(IsShownProperty, true);
        }

        /// <summary>隐藏通知（等同设置 <see cref="IsShown"/> 为 <c>false</c>）。</summary>
        public void Hide()
        {
            if (!IsShown)
            {
                return;
            }

            SetCurrentValue(IsShownProperty, false);
        }

        /// <summary>返回指定外观的默认图标字形。</summary>
        public static string GetDefaultGlyph(MessageBarAppearance appearance) => appearance switch
        {
            MessageBarAppearance.Success => SuccessIconGlyph,
            MessageBarAppearance.Warning => WarningIconGlyph,
            MessageBarAppearance.Danger => DangerIconGlyph,
            _ => InformationalIconGlyph
        };

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageBar bar)
            {
                bar.UpdateDefaultIcon();
            }
        }

        private static void OnTimeoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageBar bar && bar.IsShown)
            {
                bar.RestartCloseTimer();
            }
        }

        private static void OnIsShownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MessageBar)d).HandleIsShownChanged((bool)e.NewValue);
        }

        private void HandleIsShownChanged(bool shown)
        {
            if (_suppressIsShownChanged)
            {
                return;
            }

            var cancelArgs = new MessageBarCancelEventArgs(shown ? OpeningEvent : ClosingEvent, this);
            RaiseEvent(cancelArgs);
            if (cancelArgs.Cancel)
            {
                RevertIsShown(!shown);
                return;
            }

            StopCloseTimer();

            if (shown)
            {
                Visibility = Visibility.Visible;
                BeginShowAnimation();
                RestartCloseTimer();
            }
            else
            {
                BeginHideAnimation();
            }
        }

        private void RevertIsShown(bool shown)
        {
            _suppressIsShownChanged = true;
            try
            {
                SetCurrentValue(IsShownProperty, shown);
            }
            finally
            {
                _suppressIsShownChanged = false;
            }
        }

        private void UpdateDefaultIcon()
        {
            // 仅在用户未显式设置图标（或仍是内置字形）时跟随外观变化。
            if (Icon is null || IsBuiltInGlyph(Icon as string))
            {
                SetCurrentValue(IconProperty, GetDefaultGlyph(Appearance));
            }
        }

        private bool IsBuiltInGlyph(string? glyph) => glyph is InformationalIconGlyph
            or SuccessIconGlyph
            or WarningIconGlyph
            or DangerIconGlyph;

        private void BeginShowAnimation()
        {
            Opacity = 0;
            _slideTransform.Y = SlideOffset;

            var opacity = new DoubleAnimation(1, AnimationDuration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            var slide = new DoubleAnimation(0, AnimationDuration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            slide.Completed += (_, _) => RaiseEvent(new RoutedEventArgs(OpenedEvent, this));

            BeginAnimation(OpacityProperty, opacity);
            _slideTransform.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        private void BeginHideAnimation()
        {
            var opacity = new DoubleAnimation(0, AnimationDuration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
            var slide = new DoubleAnimation(SlideOffset, AnimationDuration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
            opacity.Completed += (_, _) =>
            {
                Visibility = Visibility.Collapsed;
                RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
            };

            BeginAnimation(OpacityProperty, opacity);
            _slideTransform.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        private void RestartCloseTimer()
        {
            StopCloseTimer();

            if (Timeout <= TimeSpan.Zero)
            {
                return;
            }

            _closeTimer = new DispatcherTimer(Timeout, DispatcherPriority.Background, (_, _) => Hide(), Dispatcher);
        }

        private void StopCloseTimer()
        {
            _closeTimer?.Stop();
            _closeTimer = null;
        }

        private sealed class ActionCommand : ICommand
        {
            private readonly Action _execute;

            public ActionCommand(Action execute)
            {
                _execute = execute;
            }

            public event EventHandler? CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object? parameter) => true;

            public void Execute(object? parameter) => _execute();
        }
    }

    /// <summary>可取消的 MessageBar 事件参数。</summary>
    public class MessageBarCancelEventArgs : RoutedEventArgs
    {
        public MessageBarCancelEventArgs(RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
        }

        /// <summary>设为 <c>true</c> 可阻止显示或关闭。</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>处理可取消 MessageBar 事件的方法。</summary>
    public delegate void MessageBarCancelEventHandler(object sender, MessageBarCancelEventArgs e);
}
