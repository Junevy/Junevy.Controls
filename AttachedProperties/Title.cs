using System.Windows;

namespace Junevy.Controls.AttachedProperties
{
    public class Title
    {
        public static Visibility GetVisibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(VisibilityProperty);
        }

        public static void SetVisibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(VisibilityProperty, value);
        }
        public static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.RegisterAttached("Visibility", typeof(Visibility), typeof(Icon), new PropertyMetadata(Visibility.Visible));

        public static object GetTitle(DependencyObject obj)
        {
            return (object)obj.GetValue(TitleProperty);
        }

        public static void SetTitle(DependencyObject obj, object value)
        {
            obj.SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(object), typeof(Title), new PropertyMetadata(""));



        public static int GetFontSize(DependencyObject obj)
        {
            return (int)obj.GetValue(FontSizeProperty);
        }

        public static void SetFontSize(DependencyObject obj, int value)
        {
            obj.SetValue(FontSizeProperty, value);
        }
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.RegisterAttached("FontSize", typeof(int), typeof(Title), new PropertyMetadata(12));


    }
}
