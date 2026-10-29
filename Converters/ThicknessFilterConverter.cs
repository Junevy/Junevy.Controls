using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Junevy.Controls.Converters
{
    /// <summary>
    /// Thickness筛选转换器：按 Mode 输出边框厚度。
    /// NoBottom 保留左/上/右三边、底边置0，用于选中页签与内容区衔接（去掉底边但保留宿主设定的其余三边）；
    /// NegativeTop 输出 Thickness(0, -Top, 0, 0)，用于把内容区上移自身上边线厚度，使该线延伸进页签行充当页签间分割线。
    /// 两者都必须由 Mode 决定而不是在模板里写死绝对值：宿主改 BorderThickness 后，写死的 1,1,1,0 与 -1 会反过来覆盖宿主设置。
    /// </summary>
    public class ThicknessFilterConverter : IValueConverter
    {
        /// <summary>
        /// 变换方式，默认原样透传。
        /// </summary>
        public string Mode { get; set; } = "All";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Thickness thickness)
            {
                return new Thickness(0d);
            }

            string mode = parameter as string ?? Mode;
            return mode switch
            {
                "NoBottom" => new Thickness(thickness.Left, thickness.Top, thickness.Right, 0d),
                "NegativeTop" => new Thickness(0d, -thickness.Top, 0d, 0d),
                _ => thickness
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
