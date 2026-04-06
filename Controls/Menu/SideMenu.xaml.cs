using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Junevy.Controls.Controls.Menu
{
    //[TemplatePart(Name = "PART_SIDEMENU", Type = typeof(ListBox))]
    public class SideMenu : ListBox
    {

        public enum Mode : byte
        {
            Horizontal = 0x01,
            Vertical = 0x01 << 1
        }

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


        public Mode DisplayMode
        {
            get { return (Mode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(Mode), typeof(SideMenu), new PropertyMetadata(Mode.Horizontal));


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
