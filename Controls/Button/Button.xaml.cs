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
        /// 内容是否随控件尺寸等比缩放（默认 true）。启用后模板经 ShrinkBox 缩放宿主渲染内容：
        /// 空间充足（按钮尺寸不小于内容自然尺寸）时保持原始字号，与官方 Button 一致；仅当按钮被
        /// 挤压（显式尺寸或布局约束小于内容自然尺寸）时，文字与图标作为整体等比缩小并保持居中，
        /// 缩放系数以 1 为上限（只缩小不放大，需要大字内容请直接设置 FontSize）。
        /// 设为 false 恢复完全固定字号行为（被挤压时也不缩小）。
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

        /// <summary>
        /// 是否显示常态浮起阴影（默认 true），阴影取自主题令牌 <c>Theme.ButtonShadow</c>，
        /// 只作用于模板的背景层，不影响文字 ClearType。与 ComboBox / TextBox 等没有浮起感的同级控件
        /// 并排时（表单行、工具条、对话框底栏）可按实例或样式设为 false，避免同排控件一半浮起一半贴平；
        /// 按压与禁用态本就不显示阴影，与此设置无关。
        /// </summary>
        public bool ShowShadow
        {
            get { return (bool)GetValue(ShowShadowProperty); }
            set { SetValue(ShowShadowProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="ShowShadow"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty ShowShadowProperty =
            DependencyProperty.Register(nameof(ShowShadow), typeof(bool), typeof(Button), new PropertyMetadata(true));
    }
}
