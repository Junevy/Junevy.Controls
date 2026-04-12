using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Junevy.Controls.Controls.Menu
{
    public class TabMenuItem : TabItem
    {
        private TextBox textBox;

        public Guid Id { get; }

        public TabMenuItem()
        {
            this.Loaded += OnTabMenuItemLoaded;
            Id = new Guid();
        }

        private void OnTabMenuItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;
            textBox.IsHitTestVisible = true;  // 编辑时恢复命中测试
            textBox.IsReadOnly = false;
        }

        private void OnTabMenuItemLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TabMenuItem item)
            {
                if (item.Template?.FindName("PART_EditHeaderTextBox", item) is System.Windows.Controls.TextBox textBox)
                {
                    this.textBox = textBox;

                    textBox.IsReadOnly = true;
                    textBox.IsHitTestVisible = false; // 初始状态穿透事件

                    textBox.MouseDoubleClick += TextBox_DoubleClick;
                    textBox.LostFocus += TextBox_LostFocus;
                }

                item.MouseDoubleClick += OnTabMenuItemDoubleClick;
            }
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
                textBox.IsHitTestVisible = false; // 失去焦点后恢复穿透
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

