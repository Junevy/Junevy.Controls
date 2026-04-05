using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Menu
{
    [TemplatePart(Name = "PART_SIDEMENU", Type = typeof(ListBox))]
    public class SideMenu : ListBox
    {
        static SideMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SideMenu),
                new FrameworkPropertyMetadata(typeof(SideMenu)));
        }

        public SideMenu()
        {
            this.SelectionMode = SelectionMode.Single;
            this.SelectedIndex = 0;
        }

        /// <summary>
        /// The orientation of the side menu (Vertical or Horizontal)
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(MenuBase), new PropertyMetadata(Orientation.Vertical));




        public int MyProperty
        {
            get { return (int)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("MyProperty", typeof(int), typeof(ownerclass), new PropertyMetadata(0));




        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer sv)
            {
                sv.ScrollToHorizontalOffset(sv.HorizontalOffset - e.Delta / 3.0);
                e.Handled = true;
            }
        }
    }
}
