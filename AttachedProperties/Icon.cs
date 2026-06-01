using System.Windows;
using System.Windows.Controls;
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

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.RegisterAttached("IconSize", typeof(int), typeof(Icon), new PropertyMetadata(14));

        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.RegisterAttached("IconForeground", typeof(Brush), typeof(Icon), new PropertyMetadata(Brushes.Gray));


        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                bool b = e.NewValue == null || (e.NewValue is string s && string.IsNullOrEmpty(s));
                element.Visibility = b ? Visibility.Collapsed : Visibility.Visible;
                //element.child
            }

            //if (d is Control c)
            //{
            //    if (c.Template == null) return;
            //    var el = c.Template.FindName("PART_ICON", c);

            //    if (el == null) return;
            //    var cc = el as Control;
            //    if (cc == null) return;
            //    bool b = e.NewValue == null || (e.NewValue is string s && string.IsNullOrEmpty(s));
            //    cc.Visibility = b ? Visibility.Visible : Visibility.Visible;
            //}
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



        public static int GetIconSize(DependencyObject obj)
        {
            return (int)obj.GetValue(IconSizeProperty);
        }

        public static void SetIconSize(DependencyObject obj, int value)
        {
            obj.SetValue(IconSizeProperty, value);
        }


        public static Brush GetIconForeground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(IconForegroundProperty);
        }

        public static void SetIconForeground(DependencyObject obj, Brush value)
        {
            obj.SetValue(IconForegroundProperty, value);
        }
    }
}
