using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Box
{
    /// <summary>
    /// 横向列表的鼠标滚轮支持：把鼠标滚轮折算成水平滚动。
    /// </summary>
    /// <remarks>
    /// WPF 的 <see cref="ScrollViewer.OnMouseWheel"/> 只调用 <c>MouseWheelUp</c>/<c>MouseWheelDown</c>，
    /// 并且无论是否真的滚动过都会把事件标记为已处理；竖向滚不动时不会自动退化为水平滚动。
    /// 因此横向列表需要自己完成这一步折算。折算量与 WPF 竖向滚轮保持一致：
    /// 逻辑滚动（<see cref="ScrollViewer.CanContentScroll"/> 为 <c>true</c>）按
    /// <see cref="SystemParameters.WheelScrollLines"/> 条项目滚动，像素滚动按一个文本行高折算。
    /// </remarks>
    internal static class HorizontalWheelScrolling
    {
        /// <summary>一个滚轮刻度对应的角度增量。</summary>
        private const double WheelNotch = 120.0;

        /// <summary>像素滚动模式下一个文本行的估算高度，与 WPF 竖向滚轮的换算基准一致。</summary>
        private const double PixelLineHeight = 16.0;

        /// <summary>
        /// 模板中承载滚动的 <see cref="ScrollViewer"/> 部件名，与本库的 ListBox/ListView 模板约定一致。
        /// </summary>
        private const string ScrollViewerPartName = "PART_ScrollViewer";

        /// <summary>
        /// 尝试把滚轮事件折算成水平滚动，成功接管时返回 <c>true</c>。
        /// </summary>
        /// <remarks>
        /// 只有同时满足以下条件才接管，其余情况一律交回 WPF 原生行为：
        /// 事件尚未被处理；控件处于横向模式；鼠标位于本控件自己的滚动区域内；
        /// 竖向已经滚不动（竖向优先是 WPF 的既有语义）而横向还可以滚动。
        /// </remarks>
        /// <param name="owner">发起滚轮处理的控件。</param>
        /// <param name="orientation"><paramref name="owner"/> 当前的排列方向。</param>
        /// <param name="e">鼠标滚轮事件参数。</param>
        public static bool TryScroll(Control owner, Orientation orientation, MouseWheelEventArgs e)
        {
            if (owner == null || e == null || e.Handled || orientation != Orientation.Horizontal)
            {
                return false;
            }

            ScrollViewer viewer = FindOwnScrollViewer(owner, e.OriginalSource as DependencyObject);
            if (viewer == null || viewer.ScrollableWidth <= 0 || viewer.ScrollableHeight > 0)
            {
                return false;
            }

            double notches = e.Delta / WheelNotch;
            double lines = SystemParameters.WheelScrollLines;
            double step = viewer.CanContentScroll
                ? (lines > 0 ? lines : Math.Max(1.0, viewer.ViewportWidth))
                : (lines > 0 ? lines : 1.0) * PixelLineHeight;

            double target = viewer.HorizontalOffset - (notches * step);
            viewer.ScrollToHorizontalOffset(Math.Max(0.0, Math.Min(viewer.ScrollableWidth, target)));
            return true;
        }

        /// <summary>
        /// 找到「鼠标下方且属于本控件」的 <see cref="ScrollViewer"/>。
        /// </summary>
        /// <remarks>
        /// 从事件源向上遇到的第一个 <see cref="ScrollViewer"/> 就是鼠标位置处最内层的滚动宿主；
        /// 只有当它就是模板里那个滚动宿主时才接管，避免抢走条目模板内部自带滚动控件的滚轮。
        /// 模板被整体替换且未命名该部件时返回 <c>null</c>，此时保持 WPF 原生行为。
        /// </remarks>
        private static ScrollViewer FindOwnScrollViewer(Control owner, DependencyObject source)
        {
            var own = owner.Template?.FindName(ScrollViewerPartName, owner) as ScrollViewer;
            if (own == null)
            {
                return null;
            }

            if (source == null)
            {
                return own;
            }

            for (DependencyObject current = source; current != null; current = VisualTreeHelper.GetParent(current))
            {
                if (current is ScrollViewer found)
                {
                    return ReferenceEquals(found, own) ? own : null;
                }
            }

            return null;
        }
    }
}
