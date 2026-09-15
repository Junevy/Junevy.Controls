using Junevy.Controls.Common;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Junevy.Controls.Controls.Button
{
    public class ToggleButton : System.Windows.Controls.Primitives.ToggleButton
    {
        /// <summary>
        /// 滑块相对轨道边缘的总内缩量：左右边框 1 DIP + 左右内边距 1 DIP，共 4 DIP。
        /// </summary>
        private const double ThumbInset = 4.0;

        /// <summary>
        /// 模板滑块的平移变换，用于执行选中/取消的滑动动画。
        /// </summary>
        private TranslateTransform thumbTranslate;

        static ToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Junevy.Controls.Controls.Button.ToggleButton),
                new FrameworkPropertyMetadata(typeof(Junevy.Controls.Controls.Button.ToggleButton)));
        }

        public ToggleButton()
        {
            // 依赖属性默认值不会触发变更回调，构造时主动推导一次模板几何尺寸
            this.UpdateSwitchGeometry();
        }

        public ShapeMode DisplayMode
        {
            get { return (ShapeMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(ShapeMode), typeof(ToggleButton), new PropertyMetadata(ShapeMode.Rectangular));

        /// <summary>
        /// 开关整体高度（DIP）。轨道宽度固定按 2:1 比例自动推导，任何尺寸下比例恒定、不会变形；建议不小于 12。
        /// </summary>
        public double SwitchSize
        {
            get { return (double)GetValue(SwitchSizeProperty); }
            set { SetValue(SwitchSizeProperty, value); }
        }
        public static readonly DependencyProperty SwitchSizeProperty =
            DependencyProperty.Register(nameof(SwitchSize), typeof(double), typeof(ToggleButton), new PropertyMetadata(20.0, OnSwitchSizeChanged));

        /// <summary>
        /// 开关轨道宽度，由 SwitchSize 按 2:1 推导，仅供模板绑定，外部不应直接设置。
        /// </summary>
        public double TrackWidth
        {
            get { return (double)GetValue(TrackWidthProperty); }
        }
        private static readonly DependencyPropertyKey TrackWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(TrackWidth), typeof(double), typeof(ToggleButton), new PropertyMetadata(40.0));
        public static readonly DependencyProperty TrackWidthProperty = TrackWidthPropertyKey.DependencyProperty;

        /// <summary>
        /// 开关轨道圆角（胶囊圆角 = SwitchSize / 2），仅供模板绑定，外部不应直接设置。
        /// </summary>
        public CornerRadius TrackCornerRadius
        {
            get { return (CornerRadius)GetValue(TrackCornerRadiusProperty); }
        }
        private static readonly DependencyPropertyKey TrackCornerRadiusPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(TrackCornerRadius), typeof(CornerRadius), typeof(ToggleButton), new PropertyMetadata(new CornerRadius(10.0)));
        public static readonly DependencyProperty TrackCornerRadiusProperty = TrackCornerRadiusPropertyKey.DependencyProperty;

        /// <summary>
        /// 滑块边长 = SwitchSize - 4（扣除左右边框与内边距），保证滑块与轨道内壁严丝合缝，仅供模板绑定。
        /// </summary>
        public double ThumbSize
        {
            get { return (double)GetValue(ThumbSizeProperty); }
        }
        private static readonly DependencyPropertyKey ThumbSizePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ThumbSize), typeof(double), typeof(ToggleButton), new PropertyMetadata(16.0));
        public static readonly DependencyProperty ThumbSizeProperty = ThumbSizePropertyKey.DependencyProperty;

        /// <summary>
        /// 滑块圆角 = 滑块边长 / 2，恒比轨道圆角小 2 DIP（与内缩量一致），内外圆角视觉吻合，仅供模板绑定。
        /// </summary>
        public CornerRadius ThumbCornerRadius
        {
            get { return (CornerRadius)GetValue(ThumbCornerRadiusProperty); }
        }
        private static readonly DependencyPropertyKey ThumbCornerRadiusPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ThumbCornerRadius), typeof(CornerRadius), typeof(ToggleButton), new PropertyMetadata(new CornerRadius(8.0)));
        public static readonly DependencyProperty ThumbCornerRadiusProperty = ThumbCornerRadiusPropertyKey.DependencyProperty;

        /// <summary>
        /// 滑块滑动行程 = 轨道内容宽度 - 滑块边长，仅供动画与模板使用。
        /// </summary>
        public double ThumbTravel
        {
            get { return (double)GetValue(ThumbTravelProperty); }
        }
        private static readonly DependencyPropertyKey ThumbTravelPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ThumbTravel), typeof(double), typeof(ToggleButton), new PropertyMetadata(20.0));
        public static readonly DependencyProperty ThumbTravelProperty = ThumbTravelPropertyKey.DependencyProperty;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // 滑块的平移变换挂在 PART_Thumb 的 RenderTransform 上，模板加载后取回用于动画
            if (GetTemplateChild("PART_Thumb") is FrameworkElement thumb)
            {
                this.thumbTranslate = thumb.RenderTransform as TranslateTransform;
            }

            this.UpdateThumbPosition(false);
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            this.UpdateThumbPosition(true);
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);
            this.UpdateThumbPosition(true);
        }

        private static void OnSwitchSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ToggleButton)d;
            control.UpdateSwitchGeometry();
        }

        /// <summary>
        /// 由 SwitchSize 统一推导轨道、滑块、圆角与滑动行程，保证任意尺寸下几何关系正确。
        /// </summary>
        private void UpdateSwitchGeometry()
        {
            double size = Math.Max(this.SwitchSize, 0);

            this.SetValue(TrackWidthPropertyKey, size * 2);
            this.SetValue(TrackCornerRadiusPropertyKey, new CornerRadius(size / 2));

            double thumbSize = Math.Max(size - ThumbInset, 0);
            this.SetValue(ThumbSizePropertyKey, thumbSize);
            this.SetValue(ThumbCornerRadiusPropertyKey, new CornerRadius(thumbSize / 2));

            // 行程 = 轨道内容宽度(2S-4) - 滑块边长(S-4) = S
            this.SetValue(ThumbTravelPropertyKey, Math.Max(size * 2 - ThumbInset - thumbSize, 0));

            this.UpdateThumbPosition(false);
        }

        /// <summary>
        /// 将滑块平移到目标位置；animate 为 true 时执行缓动动画，否则直接定位（初始化、尺寸变更时使用）。
        /// </summary>
        private void UpdateThumbPosition(bool animate)
        {
            if (this.thumbTranslate == null)
            {
                return;
            }

            double target = this.IsChecked == true ? this.ThumbTravel : 0;

            if (animate)
            {
                var animation = new DoubleAnimation(target, TimeSpan.FromMilliseconds(200))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                this.thumbTranslate.BeginAnimation(TranslateTransform.XProperty, animation);
            }
            else
            {
                this.thumbTranslate.BeginAnimation(TranslateTransform.XProperty, null);
                this.thumbTranslate.X = target;
            }
        }
    }
}
