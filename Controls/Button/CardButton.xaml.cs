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

        public SolidColorBrush MainColor
        {
            get { return (SolidColorBrush)GetValue(MainColorProperty); }
            set { SetValue(MainColorProperty, value); }
        }
        public static readonly DependencyProperty MainColorProperty =
            DependencyProperty.Register("MainColor", typeof(SolidColorBrush), typeof(CardButton), new PropertyMetadata(Brushes.Purple));




        public object Title
        {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(CardButton), new PropertyMetadata(""));



    }
}
