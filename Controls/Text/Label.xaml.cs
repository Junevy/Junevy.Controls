using System.Windows;

namespace Junevy.Controls.Controls.Text
{
    public class Label : System.Windows.Controls.Label
    {
        static Label()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Label),
                new FrameworkPropertyMetadata(typeof(Label)));
        }


        public int DisplayMode
        {
            get { return (int)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(int), typeof(Label), new PropertyMetadata(0));



    }
}
