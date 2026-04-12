using System.Windows;

namespace Junevy.Controls.Controls.Text
{
    public class TextBox : System.Windows.Controls.TextBox
    {
        static TextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TextBox),
                new FrameworkPropertyMetadata(typeof(TextBox)));
        }


        public TextBox()
        {
            this.Loaded += TextBox_Loaded;
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Template?.FindName("PART_CloseButton", textBox) is System.Windows.Controls.Button closeButton)
                    closeButton.Click += ClearTextBoxText;
            }
        }

        private void ClearTextBoxText(object sender, RoutedEventArgs e)
        {
            this.Clear();
            this.Focus();
        }
    }
}
