using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Menu
{
    public class ToolBar : ItemsControl
    {
        static ToolBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ToolBar),
                new FrameworkPropertyMetadata(typeof(ToolBar)));
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ToolBar), new PropertyMetadata(Orientation.Horizontal));
    }
}
