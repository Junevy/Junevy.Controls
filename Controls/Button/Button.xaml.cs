using System.Windows;

namespace Junevy.Controls.Controls.Button
{
    public class Button : System.Windows.Controls.Button
    {

        static Button()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Button),
                new FrameworkPropertyMetadata(typeof(Button)));
        }

        /// <summary>
        /// 内容是否随控件尺寸等比缩放（默认 true）。启用后模板将内容包在 Viewbox 中，
        /// 文字与图标作为整体随按钮大小缩放并保持居中；按钮按内容自动定位大小时
        /// 缩放系数为 1，外观与未缩放时一致。设为 false 恢复固定字号行为。
        /// </summary>
        public bool IsTextScaled
        {
            get { return (bool)GetValue(IsTextScaledProperty); }
            set { SetValue(IsTextScaledProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="IsTextScaled"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty IsTextScaledProperty =
            DependencyProperty.Register(nameof(IsTextScaled), typeof(bool), typeof(Button), new PropertyMetadata(true));
    }
}
