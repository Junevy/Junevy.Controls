using System;
using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Menu
{
    public class MenuItem : ContentControl
    {
        static MenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MenuItem),
                new FrameworkPropertyMetadata(typeof(MenuItem)));
        }

        public Guid Id { get; } = Guid.NewGuid();

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MenuItem), new PropertyMetadata(""));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(MenuItem));

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(MenuItem), new PropertyMetadata(Orientation.Horizontal));

        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register("TargetType", typeof(Type), typeof(MenuItem));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public Type TargetType
        {
            get { return (Type)GetValue(TargetTypeProperty); }
            set { SetValue(TargetTypeProperty, value); }
        }
    }
}
