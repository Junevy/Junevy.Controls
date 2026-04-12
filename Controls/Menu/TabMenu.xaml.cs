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


        //public ICommand CloseCommand
        //{
        //    get { return (ICommand)GetValue(CloseCommandProperty); }
        //    set { SetValue(CloseCommandProperty, value); }
        //}
        //public static readonly DependencyProperty CloseCommandProperty =
        //    DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(TabMenu));



        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(TabMenu), new PropertyMetadata(Orientation.Horizontal));
    }
}
