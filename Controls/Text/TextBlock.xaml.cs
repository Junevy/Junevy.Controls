using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Text
{
    /// <summary>
    /// 标题文本控件：左侧显示图标，右侧显示 <see cref="Text"/>，适合"图标 + 标题"的组合。
    /// 图标统一由 <see cref="AttachedProperties.Icon"/> 附加属性提供（<c>atc:Icon.Icon</c> 可为图标字体字符、
    /// <c>Image</c>、<c>Path</c> 等任意内容，为空时图标区域整体折叠），本控件不再承载内容。
    /// 作为纯显示控件，默认不可聚焦、不参与 Tab 导航。
    /// </summary>
    public class TextBlock : Control
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
