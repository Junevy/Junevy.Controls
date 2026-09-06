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

        // Data items are hosted by the Junevy button container. Explicit
        // ToolBarItem instances remain their own containers, as required by
        // the ItemsControl contract.
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ToolBarItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ToolBarItem;
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
