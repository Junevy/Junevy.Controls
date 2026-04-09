using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Menu
{
    public class TabMenu : TabControl
    {
        static TabMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TabMenu),
                new FrameworkPropertyMetadata(typeof(TabMenu)));
        }


        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(TabMenu), new PropertyMetadata(Orientation.Horizontal));
    }
}
