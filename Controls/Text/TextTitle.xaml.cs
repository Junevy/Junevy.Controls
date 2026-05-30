using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Text
{
    public class TextTitle : ContentControl
    {

        static TextTitle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TextTitle),
                new FrameworkPropertyMetadata(typeof(TextTitle)));
        }



        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TextTitle));

    }
}
