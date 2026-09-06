using Junevy.Controls.Common;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Progress
{
    /// <summary>
    /// 进度对话框窗口。无系统标题栏，内容为居中的进度条与说明文本。
    /// <para>
    /// 通过 <see cref="Window.Show"/>（非模态）或 <see cref="Window.ShowDialog"/>（模态）显示；
    /// <see cref="CloseButtonEnabled"/> 决定用户能否关闭窗口取消等待（含标题栏按钮、Esc 和 Alt+F4），
    /// 设为 <c>false</c> 时窗口只能由代码关闭。
    /// </para>
    /// <para>
    /// 后台线程可通过 <see cref="Report"/>、<see cref="UpdateMessage"/>、<see cref="UpdateDetail"/> 汇报进度
    /// （内部自动调度到 UI 线程），任务完成后调用 <see cref="RequestClose"/> 关闭，
    /// 或用 <see cref="CloseAfter(Task)"/> 在任务结束后自动关闭。
    /// </para>
    /// </summary>
    public class ProgressBarWindow : Window
    {
        private bool _allowClose;

        static ProgressBarWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ProgressBarWindow),
                new FrameworkPropertyMetadata(typeof(ProgressBarWindow)));
        }

        public ProgressBarWindow()
        {
            // Window 在模板加载前不能保持空标题，调用方可在构造后覆盖为具体标题。
            Title = "Progress";
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            ShowActivated = true;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            SizeToContent = SizeToContent.Height;
            Width = 440;

            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (_, _) => TryCloseByUser()));

            MouseLeftButtonDown += (_, _) =>
            {
                try
                {
                    DragMove();
                }
                catch (InvalidOperationException)
                {
                    // 鼠标已在拖动开始后释放，忽略。
                }
            };

            PreviewKeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    TryCloseByUser();
                }
            };
        }

        #region Dependency properties

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(ProgressBarWindow), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(ProgressBarWindow), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(ProgressBarWindow), new PropertyMetadata(100.0));

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(ProgressBarWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty ShapeModeProperty =
            DependencyProperty.Register(nameof(ShapeMode), typeof(ShapeMode), typeof(ProgressBarWindow), new PropertyMetadata(ShapeMode.Circular));

        public static readonly DependencyProperty RingThicknessProperty =
            DependencyProperty.Register(nameof(RingThickness), typeof(double), typeof(ProgressBarWindow), new PropertyMetadata(4.0));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(ProgressBarWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty DetailProperty =
            DependencyProperty.Register(nameof(Detail), typeof(string), typeof(ProgressBarWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty CloseButtonEnabledProperty =
            DependencyProperty.Register(nameof(CloseButtonEnabled), typeof(bool), typeof(ProgressBarWindow), new PropertyMetadata(true));

        /// <summary>当前进度值。</summary>
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        /// <summary>不确定模式（转圈/扫动）。</summary>
        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        /// <summary>进度条形状，默认环形（Circular）。</summary>
        public ShapeMode ShapeMode
        {
            get => (ShapeMode)GetValue(ShapeModeProperty);
            set => SetValue(ShapeModeProperty, value);
        }

        /// <summary>环形进度条线宽。</summary>
        public double RingThickness
        {
            get => (double)GetValue(RingThicknessProperty);
            set => SetValue(RingThicknessProperty, value);
        }

        /// <summary>主说明文本，为 <c>null</c> 时不显示。</summary>
        public string? Message
        {
            get => (string?)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        /// <summary>次要说明文本（例如 "已完成 45%"），为 <c>null</c> 时不显示。</summary>
        public string? Detail
        {
            get => (string?)GetValue(DetailProperty);
            set => SetValue(DetailProperty, value);
        }

        /// <summary>
        /// 是否允许用户关闭窗口（取消等待、放弃操作），默认 <c>true</c>。
        /// 为 <c>false</c> 时隐藏关闭按钮，并阻止 Esc 与 Alt+F4 关闭。
        /// </summary>
        public bool CloseButtonEnabled
        {
            get => (bool)GetValue(CloseButtonEnabledProperty);
            set => SetValue(CloseButtonEnabledProperty, value);
        }

        #endregion

        /// <summary>用户是否主动关闭了窗口（取消等待）。代码调用 <see cref="RequestClose"/> 关闭时为 <c>false</c>。</summary>
        public bool IsCancelled { get; private set; }

        /// <summary>用户取消等待时触发（窗口关闭后）。</summary>
        public event EventHandler? Cancelled;

        /// <summary>汇报进度（线程安全）。</summary>
        public void Report(double value) => RunOnUIThread(() => SetCurrentValue(ValueProperty, value));

        /// <summary>更新主说明文本（线程安全）。</summary>
        public void UpdateMessage(string? message) => RunOnUIThread(() => SetCurrentValue(MessageProperty, message));

        /// <summary>更新次要说明文本（线程安全）。</summary>
        public void UpdateDetail(string? detail) => RunOnUIThread(() => SetCurrentValue(DetailProperty, detail));

        /// <summary>关闭窗口（线程安全）。用于任务完成后由代码取消等待，不会标记为用户取消。</summary>
        public void RequestClose() => RunOnUIThread(() =>
        {
            _allowClose = true;
            Close();
        });

        /// <summary>在任务完成（成功或失败）后自动关闭窗口，线程安全。</summary>
        public void CloseAfter(Task task)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.ContinueWith(
                _ => RequestClose(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        private void TryCloseByUser()
        {
            if (!CloseButtonEnabled)
            {
                return;
            }

            IsCancelled = true;
            _allowClose = true;
            Close();
        }

        private void RunOnUIThread(Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.BeginInvoke(action);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_allowClose)
            {
                return;
            }

            if (!CloseButtonEnabled)
            {
                // 非可关闭窗口：拦截 Alt+F4 等系统关闭途径。
                e.Cancel = true;
                return;
            }

            IsCancelled = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (IsCancelled)
            {
                Cancelled?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
