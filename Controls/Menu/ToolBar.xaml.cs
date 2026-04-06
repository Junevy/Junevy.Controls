using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Menu
{
    public class ToolBar : SideMenu
    {
        static ToolBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ToolBar),
                new FrameworkPropertyMetadata(typeof(ToolBar)));
        }

        public new Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly new DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ToolBar), new PropertyMetadata(Orientation.Horizontal));
    }
}
