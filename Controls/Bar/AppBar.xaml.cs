using System.Windows;

namespace Junevy.Controls.Controls.Bar
{
    //[ContentProperty("Items")]
    //[TemplatePart(Name = "PART_TOOLBAR", Type = typeof(ToolBar))]

    public class AppBar : System.Windows.Controls.ContentControl
    {
        static AppBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBar), new FrameworkPropertyMetadata(typeof(AppBar)));
        }

        public ToolBar? ToolBar
        {
            get => (ToolBar?)GetValue(ToolBarProperty);
            set => SetValue(ToolBarProperty, value);
        }

        public static readonly DependencyProperty ToolBarProperty =
            DependencyProperty.Register(
                nameof(ToolBar),
                typeof(ToolBar),
                typeof(AppBar));

        // The type stays fully qualified: the sibling namespace
        // Junevy.Controls.Controls.Menu shadows System.Windows.Controls.Menu
        // inside Junevy.Controls.Controls.Bar.
        public System.Windows.Controls.Menu? Menu
        {
            get => (System.Windows.Controls.Menu?)GetValue(MenuProperty);
            set => SetValue(MenuProperty, value);
        }

        public static readonly DependencyProperty MenuProperty =
            DependencyProperty.Register(
                nameof(Menu),
                typeof(System.Windows.Controls.Menu),
                typeof(AppBar),
                new PropertyMetadata(null));
    }
}
