using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Junevy.Controls.Converters
{
    /// <summary>
    /// CornerRadius筛选转换器：只保留指定方位的圆角，其余置0。
    /// 可通过 Mode 属性或 ConverterParameter 指定方位（Top/Bottom/Left/Right/All），
    /// 用于页签顶部圆角、内容区底部圆角等场景。
    /// </summary>
    public class CornerRadiusFilterConverter : IValueConverter
    {
        /// <summary>
        /// 保留的圆角方位，默认保留全部。
        /// </summary>
        public string Mode { get; set; } = "All";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not CornerRadius radius)
            {
                return new CornerRadius(0d);
            }

            string mode = parameter as string ?? Mode;
            return mode switch
            {
                "Top" => new CornerRadius(radius.TopLeft, radius.TopRight, 0d, 0d),
                "Bottom" => new CornerRadius(0d, 0d, radius.BottomRight, radius.BottomLeft),
                "Left" => new CornerRadius(radius.TopLeft, 0d, 0d, radius.BottomLeft),
                "Right" => new CornerRadius(0d, radius.TopRight, radius.BottomRight, 0d),
                _ => radius
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
