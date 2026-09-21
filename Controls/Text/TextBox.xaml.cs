using System.Windows;
using System.Windows.Input;
namespace Junevy.Controls.Controls.Text
{
    [TemplatePart(Name = PART_ClearButton, Type = typeof(System.Windows.Controls.Button))]
    [TemplatePart(Name = PART_CommandButton, Type = typeof(System.Windows.Controls.Button))]
    public class TextBox : System.Windows.Controls.TextBox
    {
        private const string PART_ClearButton = "PART_ClearButton";
        private const string PART_CommandButton = "PART_CommandButton";
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

        /// <summary>
        /// 标识 <see cref="ShowCommandButton"/> 的依赖属性。
        /// <para>
        /// 值可设置在任何 TextBox 实例上（含原生 TextBox，借由 SetValue 写入属性存储），
        /// 因此样式与模板中需使用 <c>local:TextBox.ShowCommandButton</c> 限定形式引用。
        /// </para>
        /// </summary>
        public static readonly DependencyProperty ShowCommandButtonProperty =
            DependencyProperty.Register(nameof(ShowCommandButton), typeof(bool), typeof(TextBox), new PropertyMetadata(false));

        /// <summary>
        /// 是否显示内部命令按钮：按钮显示在文本框内右侧（与清空按钮同位），点击时执行
        /// <see cref="CommandButtonCommand"/>，命令可用性（CanExecute）自动控制按钮启停。
        /// <para>
        /// 与清空按钮互斥：<see cref="ShowClear"/> 为 <see langword="true"/>（清空按钮显示）时
        /// 本按钮强制隐藏；需要显示命令按钮时应将 <see cref="ShowClear"/> 设为 <see langword="false"/>。
        /// </para>
        /// </summary>
        public bool ShowCommandButton
        {
            get { return (bool)GetValue(ShowCommandButtonProperty); }
            set { SetValue(ShowCommandButtonProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="CommandButtonCommand"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty CommandButtonCommandProperty =
            DependencyProperty.Register(nameof(CommandButtonCommand), typeof(ICommand), typeof(TextBox), new PropertyMetadata(null));

        /// <summary>
        /// 命令按钮点击时执行的命令，可绑定到 ViewModel 命令；为 <see langword="null"/> 时按钮仍显示但不执行操作。
        /// </summary>
        public ICommand? CommandButtonCommand
        {
            get { return (ICommand?)GetValue(CommandButtonCommandProperty); }
            set { SetValue(CommandButtonCommandProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="CommandButtonCommandParameter"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty CommandButtonCommandParameterProperty =
            DependencyProperty.Register(nameof(CommandButtonCommandParameter), typeof(object), typeof(TextBox), new PropertyMetadata(null));

        /// <summary>
        /// 传递给 <see cref="CommandButtonCommand"/> 的命令参数。
        /// </summary>
        public object? CommandButtonCommandParameter
        {
            get { return GetValue(CommandButtonCommandParameterProperty); }
            set { SetValue(CommandButtonCommandParameterProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="CommandButtonContent"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty CommandButtonContentProperty =
            DependencyProperty.Register(nameof(CommandButtonContent), typeof(object), typeof(TextBox), new PropertyMetadata(null));

        /// <summary>
        /// 命令按钮的内容（文本或 iconfont 字形）；字体族跟随 <c>atc:Icon.FontFamily</c>，
        /// 字号/颜色继承控件自身取值。为 <see langword="null"/> 时按钮显示为空白占位，建议显式设置。
        /// </summary>
        public object? CommandButtonContent
        {
            get { return GetValue(CommandButtonContentProperty); }
            set { SetValue(CommandButtonContentProperty, value); }
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
