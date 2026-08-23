using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Junevy.Controls.Controls.Menu
{
    public class TabMenuItem : TabItem
    {
        private TextBox? headerTextBox;
        private System.Windows.Controls.Button? closeButton;
        private bool _isEditing;

        public bool IsEditing => _isEditing;

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

            if (closeButton != null)
            {
                closeButton.MouseDoubleClick -= CloseButton_MouseDoubleClick;
            }

            base.OnApplyTemplate();

            headerTextBox = GetTemplateChild("PART_EditHeaderTextBox") as TextBox;
            closeButton = GetTemplateChild("PART_CloseButton") as System.Windows.Controls.Button;
            if (closeButton != null)
            {
                closeButton.MouseDoubleClick += CloseButton_MouseDoubleClick;
            }

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
                e.Handled = true;
                _isEditing = true;
                headerTextBox.IsHitTestVisible = true;
                headerTextBox.IsReadOnly = false;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    headerTextBox.Focus();
                    headerTextBox.SelectAll();
                }), DispatcherPriority.Input);
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // The editable header TextBox and close button are inside the
            // TabItem template. Explicitly select on a header click so the
            // custom template cannot prevent the normal TabItem behavior.
            if (!IsDescendantOfCloseButton(e.OriginalSource as DependencyObject))
            {
                IsSelected = true;
            }

            base.OnMouseLeftButtonDown(e);
        }

        private bool IsDescendantOfCloseButton(DependencyObject? source)
        {
            while (source is not null)
            {
                if (ReferenceEquals(source, closeButton))
                {
                    return true;
                }

                source = System.Windows.Media.VisualTreeHelper.GetParent(source)
                    ?? System.Windows.LogicalTreeHelper.GetParent(source);
            }

            return false;
        }

        private void CloseButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
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
                _isEditing = false;
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

