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

        public ShapeMode DisplayMode
        {
            get { return (ShapeMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(ShapeMode), typeof(ToggleButton), new PropertyMetadata(ShapeMode.Rectangular));
    }

    public enum ShapeMode
    {
        Rectangular,
        Circular
    }
}
