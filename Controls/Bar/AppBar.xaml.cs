using Junevy.Controls.Controls.Menu;
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

    }
}
