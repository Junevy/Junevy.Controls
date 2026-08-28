using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Junevy.Controls.Controls.Menu
{
    [TemplatePart(Name = PART_EditHeaderTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_CloseButton, Type = typeof(System.Windows.Controls.Button))]
    public class TabMenuItem : TabItem
    {
        private const string PART_EditHeaderTextBox = "PART_EditHeaderTextBox";
        private const string PART_CloseButton = "PART_CloseButton";
        private TextBox? headerTextBox;
        private System.Windows.Controls.Button? closeButton;

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
                headerTextBox.LostFocus -= TextBox_LostFocus;
            }

            if (closeButton != null)
            {
                closeButton.MouseDoubleClick -= CloseButton_MouseDoubleClick;
            }

            base.OnApplyTemplate();

            headerTextBox = GetTemplateChild(PART_EditHeaderTextBox) as TextBox;
            closeButton = GetTemplateChild(PART_CloseButton) as System.Windows.Controls.Button;
            if (closeButton != null)
            {
                closeButton.MouseDoubleClick += CloseButton_MouseDoubleClick;
            }

            if (headerTextBox == null)
            {
                return;
            }

            headerTextBox.LostFocus += TextBox_LostFocus;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (!e.Handled
                && !IsEditing
                && Header is string
                && headerTextBox != null
                && !IsDescendantOfCloseButton(e.OriginalSource as DependencyObject))
            {
                e.Handled = true;
                SetValue(IsEditingPropertyKey, true);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    headerTextBox.Focus();
                    headerTextBox.SelectAll();
                }), DispatcherPriority.Input);
            }

            base.OnMouseDoubleClick(e);
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

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SetValue(IsEditingPropertyKey, false);
        }

        private static readonly DependencyPropertyKey IsEditingPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsEditing),
                typeof(bool),
                typeof(TabMenuItem),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        public bool IsEditing => (bool)GetValue(IsEditingProperty);

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

