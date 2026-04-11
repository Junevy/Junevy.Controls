using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Junevy.Controls.Controls.Menu
{
    public class TabMenuItem : TabItem
    {
        public Guid Id { get; }

        public TabMenuItem()
        {
            Id = new Guid();
        }

        /// <summary>
        /// MenuItem内元素的布局方向
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(TabMenuItem), new PropertyMetadata(Orientation.Horizontal));


        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(TabMenuItem));


        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register("TargetType", typeof(Type), typeof(TabMenuItem), new PropertyMetadata(null, OnTargetTypeChanged));




        //public Type Test
        //{
        //    get { return (Type)GetValue(TestProperty); }
        //    set { SetValue(TestProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Test.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty TestProperty =
        //    DependencyProperty.Register("Test", typeof(Type), typeof(TabMenuItem), new PropertyMetadata(null));





        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public Type TargetType
        {
            get { return (Type)GetValue(TargetTypeProperty); }
            set { SetValue(TargetTypeProperty, value); }
        }

        private static void OnTargetTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabMenuItem t)
            {
                //var p1 = t.GetUIParentCore();
                //var p =  as TabMenu;

                 var p = t.Parent;
            }
        }

    }
}

