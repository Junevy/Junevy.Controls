using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Junevy.Controls.Controls.Bar
{
    [ContentProperty("Items")]
    public class AppBar : ContentControl
    {
        static AppBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBar), new FrameworkPropertyMetadata(typeof(AppBar)));
        }

        //public AppBar()
        //{
        //    this.Items = new();
        //}

        public IEnumerable<MenuItem> Items
        {
            get { return (IEnumerable<MenuItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable<MenuItem>), typeof(AppBar));


        //public string WindowTitle
        //{
        //    get { return (string)GetValue(WindowTitleProperty); }
        //    set { SetValue(WindowTitleProperty, value); }
        //}
        //public static readonly DependencyProperty WindowTitleProperty =
        //    DependencyProperty.Register("WindowTitle", typeof(string), typeof(AppBar), new PropertyMetadata("WindowTitle"));

    }
}
