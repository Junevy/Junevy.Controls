using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Junevy.Controls.Controls.Box
{
    /// <summary>
    /// Theme-aware ListBox that retains the standard WPF selection and item
    /// generation behavior while supplying Junevy's default visual style.
    /// 另提供 <see cref="Orientation"/> 依赖属性：设为
    /// <see cref="System.Windows.Controls.Orientation.Horizontal"/> 时项目横向排列
    /// 并沿水平方向滚动，默认 <see cref="System.Windows.Controls.Orientation.Vertical"/>。
    /// </summary>
    public class ListBox : System.Windows.Controls.ListBox
    {
        static ListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ListBox),
                new FrameworkPropertyMetadata(typeof(ListBox)));
        }

        /// <summary>
        /// 项目排列方向，同时决定滚动方向。默认
        /// <see cref="System.Windows.Controls.Orientation.Vertical"/>：项目自上而下排列，
        /// 垂直滚动条按需显示、水平滚动条关闭。设为
        /// <see cref="System.Windows.Controls.Orientation.Horizontal"/> 后项目自左向右排列，
        /// 水平滚动条按需显示、垂直滚动条关闭。
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="Orientation"/> 的依赖属性。默认样式经由该属性的触发器切换
        /// <see cref="ItemsControl.ItemsPanel"/> 与两个方向的滚动条可见性，
        /// 因此运行时改变取值即可立即改变排列与滚动方向。
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(ListBox),
                new PropertyMetadata(Orientation.Vertical));

        /// <summary>
        /// 横向模式下把鼠标滚轮折算成水平滚动。
        /// </summary>
        /// <remarks>
        /// WPF 的 <see cref="ScrollViewer.OnMouseWheel"/> 只做竖直滚动并不会退化为水平滚动
        /// （详见 <see cref="HorizontalWheelScrolling"/>），这里补上该退化路径。
        /// 竖向模式以及不该由本控件接管的滚轮仍完全沿用 WPF 原生行为。
        /// </remarks>
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (HorizontalWheelScrolling.TryScroll(this, this.Orientation, e))
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewMouseWheel(e);
        }
    }
}
