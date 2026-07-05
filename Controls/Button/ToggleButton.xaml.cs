using Junevy.Controls.Common;
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

        public double SwitchWidth
        {
            get { return (double)GetValue(SwitchWidthProperty); }
            set { SetValue(SwitchWidthProperty, value); }
        }
        public static readonly DependencyProperty SwitchWidthProperty =
            DependencyProperty.Register(nameof(SwitchWidth), typeof(double), typeof(ToggleButton), new PropertyMetadata(40.0));

        public double SwitchHeight
        {
            get { return (double)GetValue(SwitchHeightProperty); }
            set { SetValue(SwitchHeightProperty, value); }
        }
        public static readonly DependencyProperty SwitchHeightProperty =
            DependencyProperty.Register(nameof(SwitchHeight), typeof(double), typeof(ToggleButton), new PropertyMetadata(20.0));
    }


}
