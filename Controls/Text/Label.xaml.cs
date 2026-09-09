using System.Windows;

namespace Junevy.Controls.Controls.Text
{
    public class Label : System.Windows.Controls.Label
    {
        static Label()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Label),
                new FrameworkPropertyMetadata(typeof(Label)));
        }

        /// <summary>
        /// 标签的显示模式（决定背景与图标外观）。内容始终由 Content 提供，样式不会改写。
        /// </summary>
        public LabelDisplayMode DisplayMode
        {
            get { return (LabelDisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="DisplayMode"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(nameof(DisplayMode), typeof(LabelDisplayMode), typeof(Label), new PropertyMetadata(LabelDisplayMode.Error));
    }
}
