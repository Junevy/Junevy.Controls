using System.Windows;

namespace Junevy.Controls.Controls.Button
{
    public class ToggleButton : System.Windows.Controls.Primitives.ToggleButton
    {
        static ToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Junevy.Controls.Controls.Button.ToggleButton),
                new FrameworkPropertyMetadata(typeof(Junevy.Controls.Controls.Button.ToggleButton)));
        }

    }
}
