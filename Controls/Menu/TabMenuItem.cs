using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Junevy.Controls.Controls.Menu
{
    public class TabMenuItem : TabItem
    {
        private TextBox? headerTextBox;

        public Guid Id { get; } = Guid.NewGuid();

        static TabMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TabMenuItem),
                new FrameworkPropertyMetadata(typeof(TabMenuItem)));
        }

        public override void OnApplyTemplate()
        {
            if (headerTextBox != null)
            {
                headerTextBox.MouseDoubleClick -= TextBox_DoubleClick;
                headerTextBox.LostFocus -= TextBox_LostFocus;
            }

            base.OnApplyTemplate();

            headerTextBox = GetTemplateChild("PART_EditHeaderTextBox") as TextBox;
            if (headerTextBox == null)
            {
                return;
            }

            headerTextBox.IsReadOnly = true;
            headerTextBox.IsHitTestVisible = false;
            headerTextBox.MouseDoubleClick += TextBox_DoubleClick;
            headerTextBox.LostFocus += TextBox_LostFocus;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (!e.Handled && headerTextBox != null)
            {
                headerTextBox.IsHitTestVisible = true;
                headerTextBox.IsReadOnly = false;
                headerTextBox.Focus();
                headerTextBox.SelectAll();
            }

            base.OnMouseDoubleClick(e);
        }

        private void TextBox_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.IsReadOnly = true;
                textBox.IsHitTestVisible = false;
            }
        }

        /// <summary>
        /// MenuItem内元素的布局方向
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(TabMenuItem), new PropertyMetadata(Orientation.Horizontal));


        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(TabMenuItem));



        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }


    }
}

