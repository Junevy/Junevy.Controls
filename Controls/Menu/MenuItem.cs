using System.Windows;
using System.Windows.Controls;


namespace Junevy.Controls.Controls.Menu
{
    public class MenuItem : System.Windows.Controls.MenuItem
    {
        /// <summary>
        /// Guid，唯一标识
        /// </summary>
        public Guid Id { get; }

        public MenuItem()
        {
            Id = new Guid();
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MenuItem), new PropertyMetadata(""));

        /// <summary>
        /// MenuItem内元素的布局方向
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(MenuItem), new PropertyMetadata(Orientation.Horizontal));


        /// <summary>
        /// 导航目标的类型，必须是一个Page类型，导航时会创建这个类型的实例并显示在目标区域
        /// </summary>
        public Type TargetType
        {
            get { return (Type)GetValue(TargetTypeProperty); }
            set { SetValue(TargetTypeProperty, value); }
        }
        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register("TargetType", typeof(Type), typeof(MenuItem));


    }
}
