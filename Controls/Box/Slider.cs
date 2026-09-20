using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Junevy.Controls.Controls.Box
{
    /// <summary>
    /// 滑块控件：在官方 <see cref="System.Windows.Controls.Slider"/> 的拖拽、分页与刻度行为基础上，
    /// 增加一个可停靠在上/下/左/右任意一侧的数值框（<c>PART_ValueBox</c>）。数值框支持手动键入：
    /// 回车或失焦提交，越界自动夹取到 <see cref="RangeBase.Minimum"/> 与 <see cref="RangeBase.Maximum"/> 之间，
    /// 非法输入还原为当前值；隐藏时不占据布局空间。视觉风格与控件库主题令牌保持一致。
    /// </summary>
    [TemplatePart(Name = PartValueBox, Type = typeof(TextBox))]
    public class Slider : System.Windows.Controls.Slider
    {
        private const string PartValueBox = "PART_ValueBox";

        private TextBox? valueBox;

        static Slider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Slider),
                new FrameworkPropertyMetadata(typeof(Slider)));
        }

        /// <summary>
        /// 是否显示数值框（默认显示）。设为 false 时数值框整体收起，不占据布局空间。
        /// </summary>
        public bool ShowValueBox
        {
            get { return (bool)GetValue(ShowValueBoxProperty); }
            set { SetValue(ShowValueBoxProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="ShowValueBox"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty ShowValueBoxProperty =
            DependencyProperty.Register(
                nameof(ShowValueBox),
                typeof(bool),
                typeof(Slider),
                new PropertyMetadata(true));

        /// <summary>
        /// 数值框相对滑块本体的停靠侧（默认 <see cref="SliderValueBoxSide.Right"/>）。
        /// 切换停靠侧会按主轴/交叉轴交换数值框的宽高上限。
        /// </summary>
        public SliderValueBoxSide ValueBoxSide
        {
            get { return (SliderValueBoxSide)GetValue(ValueBoxSideProperty); }
            set { SetValue(ValueBoxSideProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="ValueBoxSide"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty ValueBoxSideProperty =
            DependencyProperty.Register(
                nameof(ValueBoxSide),
                typeof(SliderValueBoxSide),
                typeof(Slider),
                new PropertyMetadata(SliderValueBoxSide.Right));

        /// <summary>
        /// 数值框的显示格式，例如 "F1"、"0.00"、"p0"（默认 null，按当前区域性直接输出数值）。
        /// 仅影响显示：键入时仍按数值解析，不受该格式串限制。
        /// </summary>
        public string? ValueFormatString
        {
            get { return (string?)GetValue(ValueFormatStringProperty); }
            set { SetValue(ValueFormatStringProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="ValueFormatString"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty ValueFormatStringProperty =
            DependencyProperty.Register(
                nameof(ValueFormatString),
                typeof(string),
                typeof(Slider),
                new PropertyMetadata(null, OnValueFormatStringChanged));

        /// <summary>
        /// 模板就位后接管数值框：订阅提交事件并回填当前值。
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (valueBox != null)
            {
                valueBox.LostFocus -= OnValueBoxLostFocus;
                valueBox.PreviewKeyDown -= OnValueBoxPreviewKeyDown;
            }

            valueBox = GetTemplateChild(PartValueBox) as TextBox;

            if (valueBox != null)
            {
                valueBox.LostFocus += OnValueBoxLostFocus;
                valueBox.PreviewKeyDown += OnValueBoxPreviewKeyDown;
            }

            UpdateValueBoxText();
        }

        /// <summary>
        /// 值变化时同步数值框显示（基类负责公开事件与轨道刷新）。
        /// </summary>
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            UpdateValueBoxText();
        }

        /// <summary>
        /// 上限变化时刷新数值框显示，使显示的数值始终落在当前区间内。
        /// </summary>
        protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            base.OnMaximumChanged(oldMaximum, newMaximum);
            UpdateValueBoxText();
        }

        /// <summary>
        /// 下限变化时刷新数值框显示，使显示的数值始终落在当前区间内。
        /// </summary>
        protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            base.OnMinimumChanged(oldMinimum, newMinimum);
            UpdateValueBoxText();
        }

        private static void OnValueFormatStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Slider)d).UpdateValueBoxText();
        }

        private void OnValueBoxLostFocus(object sender, RoutedEventArgs e)
        {
            CommitValueBoxText();
        }

        private void OnValueBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
            {
                return;
            }

            // 不置 Handled：回车继续冒泡，宿主的默认按钮等行为不受影响
            CommitValueBoxText();
        }

        /// <summary>
        /// 按当前区域性解析文本：合法则提交（越界由 RangeBase 的值强制夹取），非法或为空则还原显示。
        /// </summary>
        private void CommitValueBoxText()
        {
            if (valueBox is null)
            {
                return;
            }

            var text = valueBox.Text?.Trim();

            // NaN / 无穷不会被 RangeBase 的强制回调夹取，必须在这里挡住
            if (!string.IsNullOrEmpty(text)
                && double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var parsed)
                && !double.IsNaN(parsed)
                && !double.IsInfinity(parsed))
            {
                SetCurrentValue(ValueProperty, parsed);
            }

            UpdateValueBoxText();
        }

        private void UpdateValueBoxText()
        {
            if (valueBox is null)
            {
                return;
            }

            var text = FormatValue(Value);

            if (!string.Equals(valueBox.Text, text, StringComparison.Ordinal))
            {
                valueBox.Text = text;
            }
        }

        private string FormatValue(double value)
        {
            var culture = CultureInfo.CurrentCulture;

            if (string.IsNullOrEmpty(ValueFormatString))
            {
                return value.ToString(culture);
            }

            try
            {
                return value.ToString(ValueFormatString, culture);
            }
            catch (FormatException)
            {
                return value.ToString(culture);
            }
        }
    }
}
