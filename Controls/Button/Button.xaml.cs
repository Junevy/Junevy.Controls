using Junevy.Controls.Controls.Menu;
using System.Windows;

namespace Junevy.Controls.Controls.Button
{
    public class Button : System.Windows.Controls.Button
    {

        static Button()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Button),
                new FrameworkPropertyMetadata(typeof(Button)));
        }


        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(Button));


     

        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

    }
}
