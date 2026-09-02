using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Box
{
    public class ComboBox : System.Windows.Controls.ComboBox
    {
        static ComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ComboBox),
                new FrameworkPropertyMetadata(typeof(ComboBox)));
        }

        public string PlaceHolder
        {
            get { return (string)GetValue(PlaceHolderProperty); }
            set { SetValue(PlaceHolderProperty, value); }
        }
        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.Register(nameof(PlaceHolder), typeof(string), typeof(ComboBox), new PropertyMetadata("Select an item..."));

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (e.Handled || !IsEnabled || IsEditable || IsDropDownOpen)
            {
                return;
            }

            // Let the arrow toggle handle its own click; all other clicks open the list.
            if (FindVisualParent<ButtonBase>(e.OriginalSource as DependencyObject) is not null)
            {
                return;
            }

            Focus();
            IsDropDownOpen = true;
            e.Handled = true;
        }

        private static T? FindVisualParent<T>(DependencyObject? child)
            where T : DependencyObject
        {
            while (child is not null)
            {
                if (child is T match)
                {
                    return match;
                }

                child = VisualTreeHelper.GetParent(child);
            }

            return null;
        }


    }
}
