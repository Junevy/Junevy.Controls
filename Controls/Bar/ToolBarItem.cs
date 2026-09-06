using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Menu
{
    public class ToolBarItem : System.Windows.Controls.Button
    {
        static ToolBarItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ToolBarItem),
                new FrameworkPropertyMetadata(typeof(ToolBarItem)));
        }

        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(ToolBarItem));


        public Orientation DisplayOrientation
        {
            get { return (Orientation)GetValue(DisplayOrientationProperty); }
            set { SetValue(DisplayOrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayOrientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayOrientationProperty =
            DependencyProperty.Register("DisplayOrientation", typeof(Orientation), typeof(ToolBarItem), new PropertyMetadata(Orientation.Horizontal));







    }
}
