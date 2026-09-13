using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// DatePicker 附加属性集。
    /// <para>
    /// 以附加属性形式增强 DatePicker，官方原生实例与派生实例均可直接设置，
    /// 无需派生新类型。占位文案经由内部 <c>DatePickerTextBox.Watermark</c>
    /// 属性生效（该依赖属性为 internal，外部模板绑定会被官方 OnApplyTemplate
    /// 的重绑定覆盖，因此此处通过反射写入），官方与派生实例行为一致。
    /// </para>
    /// </summary>
    public class DatePickerAssist
    {
        /// <summary>
        /// 官方 DatePickerTextBox 内部水印依赖属性（反射解析，解析失败则占位功能静默降级）。
        /// </summary>
        private static readonly DependencyProperty WatermarkProperty = ResolveWatermarkProperty();

        /// <summary>
        /// 占位提示文本：未选择日期且文本为空时显示在输入区；为空（null/空白）时不显示。
        /// </summary>
        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.RegisterAttached("PlaceHolder", typeof(string), typeof(DatePickerAssist), new PropertyMetadata(null, OnPlaceHolderChanged));

        /// <summary>
        /// 读取指定 DatePicker 上的占位提示文本。
        /// </summary>
        public static string GetPlaceHolder(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceHolderProperty);
        }

        /// <summary>
        /// 在指定 DatePicker 上设置占位提示文本。
        /// </summary>
        public static void SetPlaceHolder(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceHolderProperty, value);
        }

        /// <summary>
        /// 占位文本变化时同步到内部 Watermark（模板未就绪时延迟到 Loaded 再写入）。
        /// </summary>
        private static void OnPlaceHolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DatePicker datePicker && WatermarkProperty != null)
            {
                RoutedEventHandler loadedHandler = null;
                loadedHandler = (sender, args) =>
                {
                    datePicker.Loaded -= loadedHandler;
                    ApplyPlaceHolder(datePicker, (string)datePicker.GetValue(PlaceHolderProperty));
                };
                datePicker.Loaded += loadedHandler;

                // 模板已就绪则立即生效（Loaded 兜底保证模板晚应用时也能写入）
                ApplyPlaceHolder(datePicker, (string)e.NewValue);
            }
        }

        /// <summary>
        /// 将占位文本写入 DatePicker 内部 PART_TextBox 的 Watermark 依赖属性。
        /// </summary>
        private static void ApplyPlaceHolder(DatePicker datePicker, string placeHolder)
        {
            if (datePicker?.Template == null || WatermarkProperty == null)
            {
                return;
            }

            if (datePicker.Template.FindName("PART_TextBox", datePicker) is DatePickerTextBox textBox)
            {
                textBox.SetValue(WatermarkProperty, placeHolder);
            }
        }

        /// <summary>
        /// 反射解析 DatePickerTextBox.WatermarkProperty（internal static readonly）。
        /// </summary>
        private static DependencyProperty ResolveWatermarkProperty()
        {
            try
            {
                var field = typeof(DatePickerTextBox).GetField(
                    "WatermarkProperty",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                return field?.GetValue(null) as DependencyProperty;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
