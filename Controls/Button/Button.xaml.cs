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
        /// 是否显示常态浮起阴影（默认 false），阴影取自主题令牌 <c>Theme.ButtonShadow</c>，
        /// 只作用于模板的背景层，不影响文字 ClearType。默认与 ComboBox / TextBox 等没有浮起感的
        /// 同级控件保持一致的贴平观感；需要浮起感强调的按钮（主操作、独立 CTA）可按实例或样式设为 true；
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
            DependencyProperty.Register(nameof(ShowShadow), typeof(bool), typeof(Button), new PropertyMetadata(false));
    }
}
