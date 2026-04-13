using System.Windows;

namespace Junevy.Controls.Controls.Box
{
    public class ComboBox : System.Windows.Controls.ComboBox
    {
        static ComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ComboBox),
                new FrameworkPropertyMetadata(typeof(ComboBox)));
        }
    }
}
