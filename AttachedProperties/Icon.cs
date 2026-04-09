using System.Windows;
using System.Windows.Media;

namespace Junevy.Controls.AttachedProperties
{
    public class Icon
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(object), typeof(Icon), new PropertyMetadata(null, OnIconChanged));

        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.RegisterAttached("FontFamily", typeof(FontFamily), typeof(Icon),
                new PropertyMetadata(
                    new FontFamily(
                        new Uri("pack://application:,,,/"),
                            "/Junevy.Controls.Resources;component/Font/#iconfont")));


        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                bool b = e.NewValue != null || (e.NewValue is string s && string.IsNullOrEmpty(s));
                element.Visibility = b ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static object GetIcon(DependencyObject obj)
        {
            return (object)obj.GetValue(IconProperty);
        }

        public static void SetIcon(DependencyObject obj, object value)
        {
            obj.SetValue(IconProperty, value);
        }


        public static FontFamily GetFontFamily(DependencyObject obj)
        {
            return (FontFamily)obj.GetValue(FontFamilyProperty);
        }

        public static void SetFontFamily(DependencyObject obj, FontFamily value)
        {
            obj.SetValue(FontFamilyProperty, value);
        }
    }
}
