using System.Windows;
namespace Junevy.Controls.Controls.Text
{
    [TemplatePart(Name = PART_CloseButton, Type = typeof(System.Windows.Controls.Button))]
    public class TextBox : System.Windows.Controls.TextBox
    {
        private const string PART_CloseButton = "PART_CloseButton";
        private System.Windows.Controls.Button? closeButton;

        static TextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TextBox),
                new FrameworkPropertyMetadata(typeof(TextBox)));
        }

        public override void OnApplyTemplate()
        {
            if (closeButton != null)
            {
                closeButton.Click -= ClearTextBoxText;
            }

            base.OnApplyTemplate();

            closeButton = GetTemplateChild(PART_CloseButton) as System.Windows.Controls.Button;
            if (closeButton != null)
            {
                closeButton.Click += ClearTextBoxText;
            }
        }

        private void ClearTextBoxText(object sender, RoutedEventArgs e)
        {
            Clear();
            Focus();
        }
    }
}
