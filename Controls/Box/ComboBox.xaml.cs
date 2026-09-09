using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Box
{
    public class ComboBox : System.Windows.Controls.ComboBox
    {
        private const string PartPopup = "PART_Popup";

        private Popup? popup;

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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            popup = GetTemplateChild(PartPopup) as Popup;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (e.Handled || !IsEnabled || IsEditable)
            {
                return;
            }

            // 弹出列表内部的点击（ComboBoxItem 等）交给选择逻辑处理，不要在这里切换。
            if (IsInDropDownClick(e.OriginalSource as DependencyObject))
            {
                return;
            }

            // Let the arrow toggle handle its own click.
            if (FindVisualParent<ButtonBase>(e.OriginalSource as DependencyObject) is not null)
            {
                return;
            }

            Focus();

            // 主体区域单击：未展开则展开，已展开则折叠（与箭头按钮的切换行为一致）。
            IsDropDownOpen = !IsDropDownOpen;
            e.Handled = true;
        }

        private bool IsInDropDownClick(DependencyObject? source)
        {
            while (source is not null)
            {
                if (popup is not null && ReferenceEquals(source, popup))
                {
                    return true;
                }

                source = VisualTreeHelper.GetParent(source);
            }

            return false;
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
