using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;
using JvButton = Junevy.Controls.Controls.Button.Button;

namespace Junevy.Controls.Controls.Dialog
{
    /// <summary>
    /// 操作前的确认对话框：消息正文 + 页脚按钮行（由 <see cref="ConfirmDialogButtons"/> 决定显示「确定」「取消」）。
    /// 继承 <see cref="DialogWindow"/>，窗框（圆角、阴影、标题栏、拖拽、<c>Esc</c> 关闭、圆角裁剪）全部复用，
    /// 只多占一个模板页脚插槽 <c>PART_FooterHost</c>——窗框模板不复制。
    ///
    /// <para>推荐经 <see cref="ConfirmDialogService"/> 的 <c>ShowAsync</c> 系列方法弹出（自动处理 Owner 与线程）；
    /// 确需自己摆窗口时才用 <see cref="ShowAsync"/>。</para>
    ///
    /// <para>关闭语义：点「确定」返回 <see cref="ConfirmDialogResult.Confirm"/>；点「取消」、标题栏 ✕、
    /// <c>Esc</c>、<c>Alt+F4</c> 一律返回 <see cref="ConfirmDialogResult.Cancel"/>（默认值即 Cancel），
    /// 不存在第三种结果。取消按钮刻意<b>不</b>设 <c>IsCancel</c>：基类已在 <c>PreviewKeyDown</c> 统一处理
    /// <c>Esc</c>，两处都处理会让一次按键走两条关闭路径。</para>
    /// </summary>
    [TemplatePart(Name = DialogWindow.PartFooterHost, Type = typeof(ContentControl))]
    public class ConfirmDialogWindow : DialogWindow
    {
        private const string FooterTemplateKey = "ConfirmDialogFooterTemplate";

        private readonly TaskCompletionSource<ConfirmDialogResult> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private ContentControl? _footerHost;
        private JvButton? _confirmButton;
        private JvButton? _cancelButton;
        private bool _shown;
        private bool _shownModally;

        static ConfirmDialogWindow()
        {
            // 窗框模板直接复用 DialogWindow 的隐式样式：DefaultStyleKey 指到基类类型即可命中
            // DialogWindow.xaml 里 TargetType=DialogWindow 的那条样式，无需复制模板。
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ConfirmDialogWindow),
                new FrameworkPropertyMetadata(typeof(DialogWindow)));
        }

        public ConfirmDialogWindow()
        {
            // 消息过长时按 MaxWidth 折行而不是把窗口拉成一整行屏宽；短消息仍按 SizeToContent 自适应
            MinWidth = 360;
            MaxWidth = 520;
        }

        #region 依赖属性

        /// <summary>对话框正文。字符串会自动包成自动换行的 <see cref="TextBlock"/>；传 UI 元素则原样呈现。</summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message), typeof(object), typeof(ConfirmDialogWindow),
                new PropertyMetadata(null, OnMessageChanged));

        public object? Message
        {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        /// <summary>按钮组合，默认 <see cref="ConfirmDialogButtons.OkCancel"/>。</summary>
        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register(
                nameof(Buttons), typeof(ConfirmDialogButtons), typeof(ConfirmDialogWindow),
                new PropertyMetadata(ConfirmDialogButtons.OkCancel, OnButtonsChanged));

        public ConfirmDialogButtons Buttons
        {
            get => (ConfirmDialogButtons)GetValue(ButtonsProperty);
            set => SetValue(ButtonsProperty, value);
        }

        /// <summary>
        /// 响应 <c>Enter</c> 的默认按钮，默认 <see cref="ConfirmDialogDefaultButton.Confirm"/>。
        /// 仅 <see cref="ConfirmDialogButtons.OkCancel"/> 有意义；单按钮组合下唯一那颗按钮天然是默认按钮。
        /// </summary>
        public static readonly DependencyProperty DefaultButtonProperty =
            DependencyProperty.Register(
                nameof(DefaultButton), typeof(ConfirmDialogDefaultButton), typeof(ConfirmDialogWindow),
                new PropertyMetadata(ConfirmDialogDefaultButton.Confirm, OnButtonsChanged));

        public ConfirmDialogDefaultButton DefaultButton
        {
            get => (ConfirmDialogDefaultButton)GetValue(DefaultButtonProperty);
            set => SetValue(DefaultButtonProperty, value);
        }

        private static readonly DependencyPropertyKey ResultPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(Result), typeof(ConfirmDialogResult), typeof(ConfirmDialogWindow),
                new PropertyMetadata(ConfirmDialogResult.Cancel));

        /// <summary>操作结果，默认 <see cref="ConfirmDialogResult.Cancel"/>；关闭时写入，供事件处理程序读取。</summary>
        public static readonly DependencyProperty ResultProperty = ResultPropertyKey.DependencyProperty;

        /// <inheritdoc cref="ResultProperty"/>
        public ConfirmDialogResult Result => (ConfirmDialogResult)GetValue(ResultProperty);

        #endregion

        /// <summary>
        /// 模态显示对话框并返回操作结果。必须在 UI 线程调用（跨线程请走 <see cref="ConfirmDialogService"/>）；
        /// 同一实例只能显示一次，每次确认请新建实例。
        /// </summary>
        public Task<ConfirmDialogResult> ShowAsync()
        {
            if (_shown)
            {
                throw new InvalidOperationException(
                    "同一个 ConfirmDialogWindow 实例只能显示一次。请为每次确认新建实例——ConfirmDialogService 会自动新建。");
            }

            if (!Dispatcher.CheckAccess())
            {
                throw new InvalidOperationException(
                    "ShowAsync 必须在 UI 线程调用；跨线程请使用 ConfirmDialogService.ShowAsync（内部会切回 UI 线程）。");
            }

            _shown = true;
            _shownModally = true;

            // 模态阻塞到窗口关闭；OnClosed 里完成任务，此处返回的已经是完成态 Task
            ShowDialog();
            return _completion.Task;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DetachFooter();
            _footerHost = GetTemplateChild(DialogWindow.PartFooterHost) as ContentControl;
            if (_footerHost is null)
            {
                return;
            }

            if (Application.Current?.TryFindResource(FooterTemplateKey) is ControlTemplate footerTemplate)
            {
                _footerHost.Template = footerTemplate;

                // 必须先 ApplyTemplate：给部件赋 Template 只是挂上模板，实例化要等到布局 passes。
                // 不强制应用就 FindName，按钮引用会拿到 null——表现为「点确定没反应、模态窗口关不掉」。
                _footerHost.ApplyTemplate();
            }

            _confirmButton = _footerHost.Template?.FindName("PART_ConfirmButton", _footerHost) as JvButton;
            _cancelButton = _footerHost.Template?.FindName("PART_CancelButton", _footerHost) as JvButton;
            if (_confirmButton is null || _cancelButton is null)
            {
                _confirmButton = null;
                _cancelButton = null;
                return;
            }

            _confirmButton.Click += OnConfirmButtonClick;
            _cancelButton.Click += OnCancelButtonClick;
            SyncFooter();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // DialogResult 只能在 ShowDialog 显示的窗口上设置，非模态 Show() 时设置会抛
            // InvalidOperationException。因此仅在本控件的模态入口 ShowAsync 走过的窗口上写它——
            // 宿主若直接调 ShowDialog()，Result 依然正确，只是 DialogResult 保持 null。
            if (!e.Cancel && _shownModally)
            {
                DialogResult = Result == ConfirmDialogResult.Confirm;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _completion.TrySetResult(Result);
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // 初始焦点落在默认按钮：键盘用户打开即可直接回车，不必先 Tab 一轮
            JvButton? defaultButton = GetDefaultButton();
            if (defaultButton is not null && !defaultButton.IsKeyboardFocusWithin)
            {
                defaultButton.Focus();
            }
        }

        private JvButton? GetDefaultButton()
        {
            if (_confirmButton is null || _cancelButton is null)
            {
                return null;
            }

            bool confirmIsDefault = _confirmButton.Visibility == Visibility.Visible
                && DefaultButton == ConfirmDialogDefaultButton.Confirm;
            return confirmIsDefault ? _confirmButton : _cancelButton;
        }

        private void DetachFooter()
        {
            if (_confirmButton is not null)
            {
                _confirmButton.Click -= OnConfirmButtonClick;
                _confirmButton = null;
            }

            if (_cancelButton is not null)
            {
                _cancelButton.Click -= OnCancelButtonClick;
                _cancelButton = null;
            }

            if (_footerHost is not null)
            {
                _footerHost.Template = null;
                _footerHost = null;
            }
        }

        /// <summary>按 <see cref="Buttons"/> / <see cref="DefaultButton"/> 同步按钮的显隐与默认按钮归属。</summary>
        private void SyncFooter()
        {
            if (_confirmButton is null || _cancelButton is null)
            {
                return;
            }

            _confirmButton.Visibility = Buttons == ConfirmDialogButtons.Cancel ? Visibility.Collapsed : Visibility.Visible;
            _cancelButton.Visibility = Buttons == ConfirmDialogButtons.Ok ? Visibility.Collapsed : Visibility.Visible;

            JvButton? defaultButton = GetDefaultButton();
            _confirmButton.IsDefault = ReferenceEquals(defaultButton, _confirmButton);
            _cancelButton.IsDefault = ReferenceEquals(defaultButton, _cancelButton);
        }

        private void OnConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            SetValue(ResultPropertyKey, ConfirmDialogResult.Confirm);
            Close();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            SetValue(ResultPropertyKey, ConfirmDialogResult.Cancel);
            Close();
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConfirmDialogWindow window)
            {
                window.SyncBody();
            }
        }

        private static void OnButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConfirmDialogWindow window)
            {
                window.SyncFooter();
            }
        }

        /// <summary>
        /// 把 <see cref="Message"/> 落成正文内容。宿主已显式设过 <see cref="System.Windows.Controls.ContentControl.Content"/>
        /// 时以宿主内容为准，不覆盖。
        /// </summary>
        private void SyncBody()
        {
            if (Message is null || Content is not null)
            {
                return;
            }

            Content = Message is UIElement element
                ? element
                : new TextBlock
                {
                    Text = Message as string ?? Message.ToString(),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(24, 20, 24, 4)
                };
        }
    }
}