using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Box
{
    /// <summary>
    /// Theme-aware ListView with the standard WPF view pipeline, including
    /// GridView columns and user-supplied item templates.
    /// </summary>
    public class ListView : System.Windows.Controls.ListView
    {
        static ListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ListView),
                new FrameworkPropertyMetadata(typeof(ListView)));

            // WPF's base ListView replaces its default style key with the
            // GridView system key whenever View changes. Keep this control's
            // theme active and let its style select the GridView template.
            ViewProperty.OverrideMetadata(
                typeof(ListView),
                new FrameworkPropertyMetadata(null, OnViewChanged));
        }

        private static void OnViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (Equals(d.GetValue(DefaultStyleKeyProperty), GridView.GridViewStyleKey))
            {
                d.ClearValue(DefaultStyleKeyProperty);
            }
        }
    }
}
