using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        /// 菜单项悬停背景画刷(默认经样式取 State.HoverScrim 状态层纱色,随主题自适应)。
        /// </summary>
        public Brush ItemHoverBackground
        {
            get { return (Brush)GetValue(ItemHoverBackgroundProperty); }
            set { SetValue(ItemHoverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ItemHoverBackgroundProperty =
            DependencyProperty.Register("ItemHoverBackground", typeof(Brush), typeof(SideMenu), new PropertyMetadata(null));

        /// <summary>
        /// 选中项背景画刷(默认经样式取 State.PressedScrim 状态层纱色——中性灰,不再是强调蓝;
        /// 需要强调色时设为 Theme.Brush.Surface.Selected 或任意画刷)。
        /// </summary>
        public Brush SelectedItemBackground
        {
            get { return (Brush)GetValue(SelectedItemBackgroundProperty); }
            set { SetValue(SelectedItemBackgroundProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemBackgroundProperty =
            DependencyProperty.Register("SelectedItemBackground", typeof(Brush), typeof(SideMenu), new PropertyMetadata(null));

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
