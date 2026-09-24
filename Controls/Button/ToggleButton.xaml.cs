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
        /// 轨道描边粗细（DIP），与模板 ToggleHost 的 BorderThickness 保持一致。
        /// </summary>
        private const double TrackBorderThickness = 1.0;

        /// <summary>
        /// 滑块相对轨道外沿的设计内缩（单侧）= 描边 1 DIP + 内边距 1 DIP，实际值会吸附到整数设备像素。
        /// </summary>
        private const double EdgeInset = 2.0;

        /// <summary>
        /// 上次推导几何时所用的设备像素比例，用于在跨屏或系统缩放改变后重新吸附。
        /// </summary>
        private double geometryScale = double.NaN;

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
        /// 实际渲染高度会吸附到整数设备像素，与设定值最多相差半个设备像素，用以换取滑块四周内缩严格相等。
        /// </summary>
        public double SwitchSize
        {
            get { return (double)GetValue(SwitchSizeProperty); }
            set { SetValue(SwitchSizeProperty, value); }
        }
        public static readonly DependencyProperty SwitchSizeProperty =
            DependencyProperty.Register(nameof(SwitchSize), typeof(double), typeof(ToggleButton), new PropertyMetadata(20.0, OnSwitchSizeChanged));

        /// <summary>
        /// 开关轨道高度（DIP）。由 SwitchSize 吸附到整数设备像素后折回，保证轨道边缘落在像素网格上，仅供模板绑定。
        /// </summary>
        public double TrackHeight
        {
            get { return (double)GetValue(TrackHeightProperty); }
        }
        private static readonly DependencyPropertyKey TrackHeightPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(TrackHeight), typeof(double), typeof(ToggleButton), new PropertyMetadata(20.0));
        public static readonly DependencyProperty TrackHeightProperty = TrackHeightPropertyKey.DependencyProperty;

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
        /// 轨道内边距。与描边合计后恰好等于整数设备像素的内缩量，使滑块四周内缩严格相等，仅供模板绑定。
        /// 必须是 Thickness 类型：TemplateBinding 不做类型转换，用 double 绑 Padding 会静默失效。
        /// </summary>
        public Thickness TrackPadding
        {
            get { return (Thickness)GetValue(TrackPaddingProperty); }
        }
        private static readonly DependencyPropertyKey TrackPaddingPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(TrackPadding), typeof(Thickness), typeof(ToggleButton), new PropertyMetadata(new Thickness(1.0)));
        public static readonly DependencyProperty TrackPaddingProperty = TrackPaddingPropertyKey.DependencyProperty;

        /// <summary>
        /// 开关轨道圆角（胶囊圆角 = 轨道高度 / 2），仅供模板绑定，外部不应直接设置。
        /// </summary>
        public CornerRadius TrackCornerRadius
        {
            get { return (CornerRadius)GetValue(TrackCornerRadiusProperty); }
        }
        private static readonly DependencyPropertyKey TrackCornerRadiusPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(TrackCornerRadius), typeof(CornerRadius), typeof(ToggleButton), new PropertyMetadata(new CornerRadius(10.0)));
        public static readonly DependencyProperty TrackCornerRadiusProperty = TrackCornerRadiusPropertyKey.DependencyProperty;

        /// <summary>
        /// 滑块边长 = 轨道高度 - 两侧内缩（均为整数设备像素），保证滑块与轨道内壁严丝合缝，仅供模板绑定。
        /// </summary>
        public double ThumbSize
        {
            get { return (double)GetValue(ThumbSizeProperty); }
        }
        private static readonly DependencyPropertyKey ThumbSizePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ThumbSize), typeof(double), typeof(ToggleButton), new PropertyMetadata(16.0));
        public static readonly DependencyProperty ThumbSizeProperty = ThumbSizePropertyKey.DependencyProperty;

        /// <summary>
        /// 滑块圆角 = 滑块边长 / 2，恒比轨道圆角小一个内缩量，内外圆角视觉吻合，仅供模板绑定。
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

        protected override Size MeasureOverride(Size constraint)
        {
            // 窗口换到不同缩放的屏幕（或系统缩放被改动）后，设备像素比例会变，几何需按新比例重新吸附
            double scale = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            if (!scale.Equals(this.geometryScale))
            {
                this.geometryScale = scale;
                this.UpdateSwitchGeometry();
            }

            return base.MeasureOverride(constraint);
        }

        private static void OnSwitchSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ToggleButton)d;
            control.UpdateSwitchGeometry();
        }

        /// <summary>
        /// 由 SwitchSize 统一推导轨道、滑块、圆角与滑动行程。所有尺寸先换算成整数设备像素再折回 DIP，
        /// 滑块的上下（左右）内缩由同一个 insetPx 决定，因此在任意缩放比例下都严格相等，不会出现一边多 1 像素。
        /// </summary>
        private void UpdateSwitchGeometry()
        {
            double size = Math.Max(this.SwitchSize, 0);
            double scale = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            double trackPx = Math.Round(size * scale);
            double trackWidthPx = Math.Round(size * 2 * scale);
            double insetPx = Math.Max(1, Math.Round(EdgeInset * scale));
            double thumbPx = Math.Max(0, trackPx - insetPx * 2);

            this.SetValue(TrackHeightPropertyKey, trackPx / scale);
            this.SetValue(TrackWidthPropertyKey, trackWidthPx / scale);

            // 描边画笔本身是 1 DIP（缩放到设备上未必是整数），剩下由内边距补足到 insetPx，两者合计恰好落在整数像素上
            double paddingDip = Math.Max(0, (insetPx - TrackBorderThickness * scale) / scale);
            this.SetValue(TrackPaddingPropertyKey, new Thickness(paddingDip));
            this.SetValue(TrackCornerRadiusPropertyKey, new CornerRadius(trackPx / scale / 2));

            this.SetValue(ThumbSizePropertyKey, thumbPx / scale);
            this.SetValue(ThumbCornerRadiusPropertyKey, new CornerRadius(thumbPx / scale / 2));

            this.SetValue(ThumbTravelPropertyKey, Math.Max(0, (trackWidthPx - insetPx * 2 - thumbPx) / scale));

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
