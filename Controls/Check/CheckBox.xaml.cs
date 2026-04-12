using System.Windows;

namespace Junevy.Controls.Controls.Check
{
    public class CheckBox : System.Windows.Controls.CheckBox
    {
        static CheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CheckBox),
                new FrameworkPropertyMetadata(typeof(CheckBox)));
        }
    }
}
