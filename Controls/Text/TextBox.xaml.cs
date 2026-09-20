using System.Windows;
namespace Junevy.Controls.Controls.Text
{
    [TemplatePart(Name = PART_ClearButton, Type = typeof(System.Windows.Controls.Button))]
    public class TextBox : System.Windows.Controls.TextBox
    {
        private const string PART_ClearButton = "PART_ClearButton";
        private System.Windows.Controls.Button? clearButton;

        static TextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TextBox),
                new FrameworkPropertyMetadata(typeof(TextBox)));
        }

        /// <summary>
        /// 标识 <see cref="ShowClear"/> 的依赖属性。
        /// <para>
        /// 值可设置在任何 TextBox 实例上（含原生 TextBox，借由 SetValue 写入属性存储），
        /// 因此样式与模板中需使用 <c>local:TextBox.ShowClear</c> 限定形式引用。
        /// </para>
        /// </summary>
        public static readonly DependencyProperty ShowClearProperty =
            DependencyProperty.Register(nameof(ShowClear), typeof(bool), typeof(TextBox), new PropertyMetadata(false));

        /// <summary>
        /// 是否显示清空按钮：按钮显示在文本框内右侧，点击后清空文本并回收焦点。
        /// <para>
        /// 默认样式 <c>DefaultTextBoxStyle</c> 会将其设为 <see langword="true"/>；
        /// 复用 TextBox 外观的场景（如 Slider 数值框）可显式设为 <see langword="false"/> 关闭。
        /// </para>
        /// </summary>
        public bool ShowClear
        {
            get { return (bool)GetValue(ShowClearProperty); }
            set { SetValue(ShowClearProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            if (clearButton != null)
            {
                clearButton.Click -= ClearTextBoxText;
            }

            base.OnApplyTemplate();

            clearButton = GetTemplateChild(PART_ClearButton) as System.Windows.Controls.Button;
            if (clearButton != null)
            {
                clearButton.Click += ClearTextBoxText;
            }
        }

        private void ClearTextBoxText(object sender, RoutedEventArgs e)
        {
            Clear();
            Focus();
        }
    }
}
