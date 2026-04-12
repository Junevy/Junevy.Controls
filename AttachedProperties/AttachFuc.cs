using System.Windows;

namespace Junevy.Controls.AttachedProperties
{
    public class AttachFuc
    {
        public static readonly DependencyProperty DispalyModeProperty =
            DependencyProperty.RegisterAttached("DispalyMode", typeof(DisplayMode), typeof(AttachFuc), new PropertyMetadata(DisplayMode.Normal));

        public static readonly DependencyProperty IsClosableProperty =
            DependencyProperty.RegisterAttached("IsClosable", typeof(bool), typeof(AttachFuc), new PropertyMetadata(false));



        public static DisplayMode GetDispalyMode(DependencyObject obj)
        {
            return (DisplayMode)obj.GetValue(DispalyModeProperty);
        }

        public static void SetDispalyMode(DependencyObject obj, DisplayMode value)
        {
            obj.SetValue(DispalyModeProperty, value);
        }


        public static bool GetIsClosable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsClosableProperty);
        }

        public static void SetIsClosable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsClosableProperty, value);
        }
    }

    public enum DisplayMode
    {
        Normal,
        Icon,
    }
}
