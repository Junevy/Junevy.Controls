using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        /// <summary>
        /// 项目排列方向，同时决定滚动方向。默认
        /// <see cref="System.Windows.Controls.Orientation.Vertical"/>：项目自上而下排列，
        /// 垂直滚动条按需显示。设为
        /// <see cref="System.Windows.Controls.Orientation.Horizontal"/> 后项目自左向右排列，
        /// 水平滚动条按需显示、垂直滚动条关闭；此时列表内容按行横向排列，适用于缩略图、
        /// 卡片等横向带状列表。
        /// </summary>
        /// <remarks>
        /// 横向模式只作用于不使用 <see cref="View"/> 的普通列表。设置了
        /// <see cref="GridView"/>（或其他自定义视图）时列表仍为垂直排列，
        /// 以免破坏 GridView 的列布局与表头。
        /// </remarks>
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
                typeof(ListView),
                new PropertyMetadata(Orientation.Vertical));

        /// <summary>
        /// 横向模式下把鼠标滚轮折算成水平滚动。
        /// </summary>
        /// <remarks>
        /// WPF 的 <see cref="ScrollViewer.OnMouseWheel"/> 只做竖直滚动并不会退化为水平滚动
        /// （详见 <see cref="HorizontalWheelScrolling"/>），这里补上该退化路径。
        /// 竖向模式、使用 <see cref="View"/> 的场景以及不该由本控件接管的滚轮，
        /// 都仍按 WPF 原生行为处理。
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
