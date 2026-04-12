using Junevy.Controls.AttachedProperties;
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


        public DisplayMode DisplayMode
        {
            get { return (DisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(DisplayMode), typeof(TreeMenu), new PropertyMetadata(DisplayMode.Icon));



    }
}
