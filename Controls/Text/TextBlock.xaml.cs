using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Text
{
    /// <summary>
    /// 文本块控件：左侧显示 <see cref="Content"/>（图标、图片等任意内容），
    /// 右侧显示 <see cref="Text"/>，适合"图标 + 标题"的组合。
    /// 作为纯显示控件，默认不可聚焦、不参与 Tab 导航。
    /// </summary>
    public class TextBlock : ContentControl
    {
        static TextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TextBlock),
                new FrameworkPropertyMetadata(typeof(TextBlock)));

            // 对齐官方 TextBlock 语义：纯显示控件，不可聚焦、不进 Tab 序列。
            FocusableProperty.OverrideMetadata(typeof(TextBlock), new FrameworkPropertyMetadata(false));
            IsTabStopProperty.OverrideMetadata(typeof(TextBlock), new FrameworkPropertyMetadata(false));
        }

        /// <summary>
        /// 右侧显示的标题文本。
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="Text"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextBlock), new PropertyMetadata(string.Empty));

        /// <summary>
        /// 标题文本的对齐方式。
        /// </summary>
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="TextAlignment"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(TextBlock), new PropertyMetadata(TextAlignment.Left));

        /// <summary>
        /// 标题文本的换行方式。
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="TextWrapping"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(TextBlock), new PropertyMetadata(TextWrapping.NoWrap));
    }
}
