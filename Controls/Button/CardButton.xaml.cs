using System.Windows;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Button
{
    public class CardButton : Button
    {
        static CardButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CardButton),
                new FrameworkPropertyMetadata(typeof(CardButton)));
        }

        public Brush MainColor
        {
            get { return (Brush)GetValue(MainColorProperty); }
            set { SetValue(MainColorProperty, value); }
        }
        public static readonly DependencyProperty MainColorProperty =
            DependencyProperty.Register(nameof(MainColor), typeof(Brush), typeof(CardButton), new PropertyMetadata(Brushes.DodgerBlue));




        public object Title
        {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(object), typeof(CardButton), new PropertyMetadata(""));



    }
}
