using Junevy.Controls.Common;
using System.Windows;

namespace Junevy.Controls.Controls.Button
{
    public class RadioButton : System.Windows.Controls.RadioButton
    {
        static RadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RadioButton),
                new FrameworkPropertyMetadata(typeof(RadioButton)));
        }

        public ShapeMode DisplayMode
        {
            get { return (ShapeMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(ShapeMode), typeof(RadioButton), new PropertyMetadata(ShapeMode.Circular));


    }
}
