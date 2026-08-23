using System.Windows;

namespace Junevy.Controls.Controls.Box
{
    /// <summary>
    /// Theme-aware ListBox that retains the standard WPF selection and item
    /// generation behavior while supplying Junevy's default visual style.
    /// </summary>
    public class ListBox : System.Windows.Controls.ListBox
    {
        static ListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ListBox),
                new FrameworkPropertyMetadata(typeof(ListBox)));
        }
    }
}
