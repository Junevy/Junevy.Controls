using System;
using System.Windows;
using System.Windows.Controls;

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

        /// <summary>
        /// The orientation of the side menu (Vertical or Horizontal)
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SideMenu), new PropertyMetadata(Orientation.Vertical));


        public Mode DisplayMode
        {
            get { return (Mode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(Mode), typeof(SideMenu), new PropertyMetadata(Mode.Horizontal));

        /// <summary>
        /// Optional fixed height for each menu item.  NaN keeps the natural
        /// content height, while a value makes vertical menus predictable.
        /// </summary>
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(
                nameof(ItemHeight),
                typeof(double),
                typeof(SideMenu),
                new PropertyMetadata(double.NaN),
                IsValidItemHeight);

        private static bool IsValidItemHeight(object value)
        {
            double height = (double)value;
            return double.IsNaN(height) || (height >= 0 && !double.IsInfinity(height));
        }
    }
}
