using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Progress
{
    /// <summary>
    /// 进度条控件。继承 WPF <see cref="ProgressBar"/>，保留 Value/Minimum/Maximum/IsIndeterminate 标准管线。
    /// <para>
    /// <see cref="ShapeMode.Rectangular"/>：线性进度条；确定模式按值填充，
    /// 不确定模式播放来回扫动动画。
    /// </para>
    /// <para>
    /// <see cref="ShapeMode.Circular"/>：环形进度；确定模式按值绘制圆弧，
    /// 不确定模式显示持续旋转的四分之一圆弧（转圈）。
    /// </para>
    /// </summary>
    public class ProgressBar : System.Windows.Controls.ProgressBar
    {
        private const string PartTrack = "PART_Track";
        private const string PartIndicator = "PART_Indicator";
        private const string PartIndeterminateOverlay = "PART_IndeterminateOverlay";
        private const string PartRingTrack = "PART_RingTrack";
        private const string PartArc = "PART_Arc";
        private const string PartSpinnerArc = "PART_SpinnerArc";

        private FrameworkElement? _track;
        private FrameworkElement? _indicator;
        private FrameworkElement? _overlay;
        private Path? _ringTrack;
        private Path? _arc;
        private Path? _spinnerArc;

        private DoubleAnimation? _overlayAnimation;

        static ProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ProgressBar),
                new FrameworkPropertyMetadata(typeof(ProgressBar)));

            IsIndeterminateProperty.OverrideMetadata(
                typeof(ProgressBar),
                new FrameworkPropertyMetadata(false, OnIsIndeterminateChanged));
        }

        #region Dependency properties

        public static readonly DependencyProperty ShapeModeProperty =
            DependencyProperty.Register(
                nameof(ShapeMode),
                typeof(ShapeMode),
                typeof(ProgressBar),
                new PropertyMetadata(ShapeMode.Rectangular, OnShapeModeChanged));

        public static readonly DependencyProperty RingThicknessProperty =
            DependencyProperty.Register(nameof(RingThickness), typeof(double), typeof(ProgressBar), new PropertyMetadata(4.0));

        public static readonly DependencyProperty ShowProgressTextProperty =
            DependencyProperty.Register(nameof(ShowProgressText), typeof(bool), typeof(ProgressBar), new PropertyMetadata(false));

        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register(nameof(ProgressText), typeof(string), typeof(ProgressBar), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ProgressTextFormatProperty =
            DependencyProperty.Register(
                nameof(ProgressTextFormat),
                typeof(string),
                typeof(ProgressBar),
                new PropertyMetadata(null, OnProgressTextFormatChanged));

        /// <summary>线性（Rectangular）或环形（Circular）显示模式。</summary>
        public ShapeMode ShapeMode
        {
            get => (ShapeMode)GetValue(ShapeModeProperty);
            set => SetValue(ShapeModeProperty, value);
        }

        /// <summary>环形模式的圆弧线宽，默认 4。</summary>
        public double RingThickness
        {
            get => (double)GetValue(RingThicknessProperty);
            set => SetValue(RingThicknessProperty, value);
        }

        /// <summary>是否显示进度文本，默认 <c>false</c>。线性模式显示在右侧，环形模式显示在圆环中心。</summary>
        public bool ShowProgressText
        {
            get => (bool)GetValue(ShowProgressTextProperty);
            set => SetValue(ShowProgressTextProperty, value);
        }

        /// <summary>进度文本，控件根据 Value 自动维护（默认形如 "45%"）。</summary>
        public string ProgressText
        {
            get => (string)GetValue(ProgressTextProperty);
            set => SetValue(ProgressTextProperty, value);
        }

        /// <summary>进度文本格式，<c>{0}</c> 为 0-100 的整数百分比；为 <c>null</c> 时使用 "{0}%"。</summary>
        public string? ProgressTextFormat
        {
            get => (string?)GetValue(ProgressTextFormatProperty);
            set => SetValue(ProgressTextFormatProperty, value);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _track = GetTemplateChild(PartTrack) as FrameworkElement;
            _indicator = GetTemplateChild(PartIndicator) as FrameworkElement;
            _overlay = GetTemplateChild(PartIndeterminateOverlay) as FrameworkElement;
            _ringTrack = GetTemplateChild(PartRingTrack) as Path;
            _arc = GetTemplateChild(PartArc) as Path;
            _spinnerArc = GetTemplateChild(PartSpinnerArc) as Path;

            UpdateRingGeometry();
            UpdateIndeterminateState();
            UpdateProgressText();
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            UpdateRingGeometry();
            UpdateProgressText();
        }

        protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            base.OnMinimumChanged(oldMinimum, newMinimum);
            UpdateRingGeometry();
            UpdateProgressText();
        }

        protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            base.OnMaximumChanged(oldMaximum, newMaximum);
            UpdateRingGeometry();
            UpdateProgressText();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateRingGeometry();
            UpdateIndeterminateState();
        }

        private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressBar bar)
            {
                bar.UpdateIndeterminateState();
            }
        }

        private static void OnShapeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressBar bar)
            {
                bar.UpdateIndeterminateState();
                bar.UpdateRingGeometry();
            }
        }

        private static void OnProgressTextFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressBar bar)
            {
                bar.UpdateProgressText();
            }
        }

        private double GetFraction()
        {
            double range = Maximum - Minimum;
            if (range <= 0)
            {
                return 0;
            }

            return Math.Max(0, Math.Min(1, (Value - Minimum) / range));
        }

        private void UpdateProgressText()
        {
            string format = ProgressTextFormat ?? "{0}%";
            int percent = (int)Math.Round(GetFraction() * 100);
            string text;
            try
            {
                text = string.Format(format, percent);
            }
            catch (FormatException)
            {
                text = string.Format("{0}%", percent);
            }

            SetCurrentValue(ProgressTextProperty, text);
        }

        private void UpdateIndeterminateState()
        {
            bool linearIndeterminate = IsIndeterminate && ShapeMode == ShapeMode.Rectangular;

            if (_indicator != null)
            {
                _indicator.Visibility = linearIndeterminate ? Visibility.Hidden : Visibility.Visible;
            }

            if (_overlay == null || _track == null)
            {
                return;
            }

            if (!linearIndeterminate)
            {
                StopOverlayAnimation();
                return;
            }

            double trackWidth = _track.ActualWidth;
            if (trackWidth <= 0)
            {
                // 尚未完成布局，SizeChanged 时会再次触发。
                return;
            }

            // 模板中声明的 TranslateTransform 未被模板动画使用时会被冻结，
            // 因此这里替换为代码创建的可动画实例。
            if (_overlay.RenderTransform is not TranslateTransform overlayTransform || overlayTransform.IsFrozen)
            {
                overlayTransform = new TranslateTransform();
                _overlay.RenderTransform = overlayTransform;
            }

            double segment = Math.Max(24, trackWidth * 0.3);
            _overlay.Visibility = Visibility.Visible;
            _overlay.Width = segment;
            overlayTransform.X = -segment;

            _overlayAnimation = new DoubleAnimation(-segment, trackWidth, new Duration(TimeSpan.FromMilliseconds(1400)))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            overlayTransform.BeginAnimation(TranslateTransform.XProperty, _overlayAnimation);
        }

        private void StopOverlayAnimation()
        {
            if (_overlay?.RenderTransform is TranslateTransform overlayTransform && !overlayTransform.IsFrozen)
            {
                overlayTransform.BeginAnimation(TranslateTransform.XProperty, null);
            }

            _overlayAnimation = null;

            if (_overlay != null)
            {
                _overlay.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateRingGeometry()
        {
            if (_ringTrack == null || _arc == null || _spinnerArc == null)
            {
                return;
            }

            double size = Math.Min(ActualWidth, ActualHeight);
            if (size <= 0)
            {
                return;
            }

            double thickness = Math.Max(1, RingThickness);
            double radius = Math.Max(1, (size - thickness) / 2);
            var center = new Point(ActualWidth / 2, ActualHeight / 2);

            _ringTrack.Data = new EllipseGeometry(center, radius, radius);
            _spinnerArc.Data = CreateArcGeometry(center, radius, -90, 0);

            double fraction = GetFraction();
            if (fraction <= 0)
            {
                _arc.Data = null;
            }
            else
            {
                double sweep = 360 * Math.Min(fraction, 0.9995);
                _arc.Data = CreateArcGeometry(center, radius, -90, -90 + sweep);
            }
        }

        private static Geometry CreateArcGeometry(Point center, double radius, double startAngle, double endAngle)
        {
            var figure = new PathFigure
            {
                StartPoint = PointAtAngle(center, radius, startAngle),
                IsClosed = false
            };
            figure.Segments.Add(new ArcSegment
            {
                Point = PointAtAngle(center, radius, endAngle),
                Size = new Size(radius, radius),
                IsLargeArc = endAngle - startAngle > 180,
                SweepDirection = SweepDirection.Clockwise
            });
            return new PathGeometry(new[] { figure });
        }

        private static Point PointAtAngle(Point center, double radius, double angle)
        {
            double radians = angle * Math.PI / 180;
            return new Point(
                center.X + radius * Math.Cos(radians),
                center.Y + radius * Math.Sin(radians));
        }
    }
}
