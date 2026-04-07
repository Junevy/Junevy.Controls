using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Menu
{
    public class TreeMenu : TreeView
    {

        static TreeMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TreeMenu),
                new FrameworkPropertyMetadata(typeof(TreeMenu)));
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(TreeMenu), new PropertyMetadata(Orientation.Horizontal));


    }
}
