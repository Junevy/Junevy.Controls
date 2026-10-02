using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Junevy.Controls.Controls.Box
{
    /// <summary>
    /// 密码框（模板复合控件）：WPF 原生 <see cref="System.Windows.Controls.PasswordBox"/> 是密封类，
    /// 无法派生，故本控件继承 <see cref="Control"/>，模板内嵌一个原生 PasswordBox（PART_HostPasswordBox）
    /// 承载真实密码编辑，附带明文 TextBox（PART_RevealTextBox）与显示按钮（PART_RevealButton）。
    /// <para>
    /// 相比原生控件的增强：<see cref="Password"/> 是依赖属性（原生密码不可绑定），
    /// 支持点击/长按显示明文（<see cref="RevealMode"/>）与错误泛红（<see cref="IsError"/>）。
    /// </para>
    /// </summary>
    [TemplatePart(Name = PART_HostPasswordBox, Type = typeof(System.Windows.Controls.PasswordBox))]
    [TemplatePart(Name = PART_RevealTextBox, Type = typeof(System.Windows.Controls.TextBox))]
    [TemplatePart(Name = PART_RevealButton, Type = typeof(System.Windows.Controls.Button))]
    public class PasswordBox : Control
    {
        private const string PART_HostPasswordBox = "PART_HostPasswordBox";
        private const string PART_RevealTextBox = "PART_RevealTextBox";
        private const string PART_RevealButton = "PART_RevealButton";

        private System.Windows.Controls.PasswordBox? hostPasswordBox;
        private System.Windows.Controls.TextBox? revealTextBox;
        private System.Windows.Controls.Button? revealButton;

        static PasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PasswordBox),
                new FrameworkPropertyMetadata(typeof(PasswordBox)));
        }

        /// <summary>
        /// 标识 <see cref="PasswordChangedEvent"/> 的路由事件。
        /// </summary>
        public static readonly RoutedEvent PasswordChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(PasswordChanged), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(PasswordBox));

        /// <summary>
        /// 密码内容变化时触发（掩码框输入、明文框编辑、程序设值均会触发）。
        /// </summary>
        public event RoutedEventHandler PasswordChanged
        {
            add { AddHandler(PasswordChangedEvent, value); }
            remove { RemoveHandler(PasswordChangedEvent, value); }
        }

        /// <summary>
        /// 标识 <see cref="Password"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(nameof(Password), typeof(string), typeof(PasswordBox),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));

        /// <summary>
        /// 密码内容。与原生 PasswordBox 不同，本属性是真正的依赖属性，可双向绑定到 ViewModel。
        /// </summary>
        [System.ComponentModel.DefaultValue("")]
        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = (PasswordBox)d;

            // 掩码宿主与 Password 值等价时不再回写（宿主输入触发的变更即由此拦停，防重入、防光标重置）
            if (box.hostPasswordBox != null && box.hostPasswordBox.Password != box.Password)
            {
                box.hostPasswordBox.Password = box.Password;
            }

            box.RaiseEvent(new RoutedEventArgs(PasswordChangedEvent, box));
        }

        /// <summary>
        /// 标识 <see cref="PasswordChar"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty PasswordCharProperty =
            DependencyProperty.Register(nameof(PasswordChar), typeof(char), typeof(PasswordBox),
                new PropertyMetadata('●'));

        /// <summary>
        /// 掩码字符，默认「●」，经模板绑定传导给内嵌的原生密码框。
        /// </summary>
        public char PasswordChar
        {
            get { return (char)GetValue(PasswordCharProperty); }
            set { SetValue(PasswordCharProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="MaxLength"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(PasswordBox),
                new PropertyMetadata(0));

        /// <summary>
        /// 最大密码长度（0 表示不限制），同时作用于掩码框与明文框。
        /// </summary>
        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="RevealMode"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty RevealModeProperty =
            DependencyProperty.Register(
                nameof(RevealMode), typeof(PasswordRevealMode), typeof(PasswordBox),
                new PropertyMetadata(PasswordRevealMode.Click, OnRevealModeChanged));

        /// <summary>
        /// 显示密码按钮的触发方式：默认 <see cref="PasswordRevealMode.Click"/>（点击切换）；
        /// 设为 <see cref="PasswordRevealMode.PressAndHold"/> 时按下显示、松开（或按住移出按钮）立即隐藏。
        /// 切换取值时收起明文，避免「未按压仍显示明文」的悬置状态。
        /// </summary>
        public PasswordRevealMode RevealMode
        {
            get { return (PasswordRevealMode)GetValue(RevealModeProperty); }
            set { SetValue(RevealModeProperty, value); }
        }

        private static void OnRevealModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PasswordBox)d).IsRevealed = false;
        }

        /// <summary>
        /// 标识 <see cref="IsError"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty IsErrorProperty =
            DependencyProperty.Register(nameof(IsError), typeof(bool), typeof(PasswordBox), new PropertyMetadata(false));

        /// <summary>
        /// 密码错误状态：<see langword="true"/> 时输入区背景叠加一层低透明度的
        /// <c>Theme.Brush.Status.Danger</c>（微微泛红），边框、文字等其余视觉不变；由业务侧验证密码失败时置位。
        /// </summary>
        public bool IsError
        {
            get { return (bool)GetValue(IsErrorProperty); }
            set { SetValue(IsErrorProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="IsRevealed"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty IsRevealedProperty =
            DependencyProperty.Register(nameof(IsRevealed), typeof(bool), typeof(PasswordBox),
                new PropertyMetadata(false, OnIsRevealedChanged));

        /// <summary>
        /// 当前是否以明文显示密码。模板据此切换「掩码内容 / 明文文本」两个宿主的显隐；
        /// 可双向绑定（如把「是否显示密码」挂到 ViewModel）。改变时焦点跟随：
        /// 掩码框与明文框谁持有键盘焦点，切向谁。
        /// </summary>
        public bool IsRevealed
        {
            get { return (bool)GetValue(IsRevealedProperty); }
            set { SetValue(IsRevealedProperty, value); }
        }

        private static void OnIsRevealedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = (PasswordBox)d;

            // 焦点仅在密码框体系内（掩码框或明文框持有键盘焦点）时搬移，不抢外部焦点
            if ((bool)e.NewValue)
            {
                if (box.hostPasswordBox != null && box.hostPasswordBox.IsKeyboardFocused
                    && box.revealTextBox != null)
                {
                    box.revealTextBox.Focus();
                    box.revealTextBox.CaretIndex = box.revealTextBox.Text.Length;
                }
            }
            else if (box.revealTextBox != null && box.revealTextBox.IsKeyboardFocused
                     && box.hostPasswordBox != null)
            {
                box.hostPasswordBox.Focus();
            }
        }

        /// <summary>
        /// 清空密码。
        /// </summary>
        public void Clear()
        {
            Password = string.Empty;
        }

        /// <summary>
        /// 点击控件空白区域（内边距、边框一带）时把键盘焦点交给掩码宿主，
        /// 保证点哪都能进入输入；文字区域的聚焦由内嵌原生框自行处理。
        /// </summary>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (hostPasswordBox != null && !hostPasswordBox.IsKeyboardFocused)
            {
                hostPasswordBox.Focus();
            }
        }

        public override void OnApplyTemplate()
        {
            if (revealButton != null)
            {
                revealButton.Click -= OnRevealButtonClick;
                revealButton.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(OnRevealButtonPress));
                revealButton.RemoveHandler(UIElement.MouseLeftButtonUpEvent,
                    new MouseButtonEventHandler(OnRevealButtonRelease));
                revealButton.MouseLeave -= OnRevealButtonLeave;
            }

            if (hostPasswordBox != null)
            {
                hostPasswordBox.PasswordChanged -= OnHostPasswordChanged;
            }

            base.OnApplyTemplate();

            hostPasswordBox = GetTemplateChild(PART_HostPasswordBox) as System.Windows.Controls.PasswordBox;
            revealTextBox = GetTemplateChild(PART_RevealTextBox) as System.Windows.Controls.TextBox;
            revealButton = GetTemplateChild(PART_RevealButton) as System.Windows.Controls.Button;

            if (hostPasswordBox != null)
            {
                hostPasswordBox.PasswordChanged += OnHostPasswordChanged;
                // 模板可能在设值之后才应用，把已录入的密码推给掩码宿主
                if (hostPasswordBox.Password != Password)
                {
                    hostPasswordBox.Password = Password;
                }
            }

            if (revealButton != null)
            {
                revealButton.Click += OnRevealButtonClick;
                // 预览按下走隧道事件，ButtonBase 不吞；抬起事件会被 ButtonBase 类处理器标记
                // Handled（内部用于 Click 派发），必须以 handledEventsToo=true 挂接才能收到
                revealButton.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(OnRevealButtonPress), true);
                revealButton.AddHandler(UIElement.MouseLeftButtonUpEvent,
                    new MouseButtonEventHandler(OnRevealButtonRelease), true);
                revealButton.MouseLeave += OnRevealButtonLeave;
            }
        }

        private void OnHostPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (hostPasswordBox != null && Password != hostPasswordBox.Password)
            {
                // DP 回调内的值等价检查会拦停回写，这里只写属性源
                Password = hostPasswordBox.Password;
            }
        }

        private void OnRevealButtonClick(object sender, RoutedEventArgs e)
        {
            if (RevealMode == PasswordRevealMode.Click)
            {
                IsRevealed = !IsRevealed;
            }
        }

        private void OnRevealButtonPress(object sender, MouseButtonEventArgs e)
        {
            if (RevealMode == PasswordRevealMode.PressAndHold)
            {
                IsRevealed = true;
            }
        }

        private void OnRevealButtonRelease(object sender, MouseButtonEventArgs e)
        {
            // ButtonBase 按下时会捕获鼠标，即使光标已移出按钮，抬起事件仍送达这里
            if (RevealMode == PasswordRevealMode.PressAndHold)
            {
                IsRevealed = false;
            }
        }

        private void OnRevealButtonLeave(object sender, MouseEventArgs e)
        {
            // 长按中拖出按钮范围即收起，避免「按住不放、光标已离开」时明文悬置
            if (RevealMode == PasswordRevealMode.PressAndHold && IsRevealed
                && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                IsRevealed = false;
            }
        }
    }
}
