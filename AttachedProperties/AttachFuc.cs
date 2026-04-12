using Junevy.Controls.Controls.Text;
using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.AttachedProperties
{
    public class AttachFuc
    {
        public static readonly DependencyProperty DispalyModeProperty =
            DependencyProperty.RegisterAttached("DispalyMode", typeof(DisplayMode), typeof(AttachFuc), new PropertyMetadata(DisplayMode.Normal));

        public static readonly DependencyProperty IsClosableProperty =
            DependencyProperty.RegisterAttached("IsClosable", typeof(bool), typeof(AttachFuc), new PropertyMetadata(false));

        //private static void OnCloseButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is Controls.Text.TextBox tb)
        //    {
        //        if ((bool)e.NewValue == true)
        //        {
        //            tb.Loaded += OnTextBoxLoaded;
        //        }
        //    }
        //}

        //private static void OnTextBoxLoaded(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Controls.Text.TextBox textBox)
        //    {
        //        var closeButton = textBox.Template.FindName("PART_CloseButton", textBox) as Button;
        //        closeButton.Click += ClickTextBox;
        //    }


        //}

        //private static void ClickTextBox(object sender, RoutedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

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
