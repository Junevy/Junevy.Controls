using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace Junevy.Controls.AttachedProperties
{
    public class Icon
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(object), typeof(Icon), new PropertyMetadata(null));

        // private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        // {
        //     if (d is FrameworkElement element)
        //     {
        //         bool isEmpty = IsIconValueEmpty(e.NewValue);
        //         ApplyIconHostVisibility(element, isEmpty);
        //     }
        // }

        // private static bool IsIconValueEmpty(object value)
        // {
        //     return value == null || (value is string s && string.IsNullOrEmpty(s));
        // }

        // private static void ApplyIconHostVisibility(FrameworkElement element, bool collapse)
        // {
        //     var visibility = collapse ? Visibility.Collapsed : Visibility.Visible;

        //     if (element.IsLoaded)
        //     {
        //         SetIconHostVisibility(element, visibility);
        //     }
        //     else
        //     {
        //         RoutedEventHandler handler = null;
        //         handler = (s, _) =>
        //         {
        //             if (s is FrameworkElement fe)
        //             {
        //                 fe.Loaded -= handler;
        //                 bool isEmpty = IsIconValueEmpty(fe.GetValue(IconProperty));
        //                 SetIconHostVisibility(fe, isEmpty ? Visibility.Collapsed : Visibility.Visible);
        //             }
        //         };
        //         element.Loaded += handler;
        //     }
        // }

        // private static void SetIconHostVisibility(FrameworkElement element, Visibility visibility)
        // {
        //     if (element.Template?.FindName("PART_IconHost", element) is FrameworkElement iconHost)
        //     {
        //         // element.temp
        //         iconHost.Visibility = visibility;
        //     }
        // }

        // Resolve the embedded font relative to its XAML resource so the default
        // value has the same pack URI base as the IconFont static resource.
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.RegisterAttached("FontFamily", typeof(FontFamily), typeof(Icon),
                new PropertyMetadata(
                    new FontFamily(
                        new Uri(
                            "pack://application:,,,/Junevy.Controls;component/Resources/Font/IconFont.xaml",
                            UriKind.Absolute),
                        "./iconfont.ttf#iconfont")));

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.RegisterAttached("IconSize", typeof(int), typeof(Icon), new PropertyMetadata(14));

        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.RegisterAttached("IconForeground", typeof(Brush), typeof(Icon), new PropertyMetadata(Brushes.Gray));

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
