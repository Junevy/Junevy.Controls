using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Box
{
    /// <summary>
    /// 分组框控件：以卡片形式呈现 <see cref="HeaderedContentControl.Header"/> 与
    /// <see cref="HeaderedContentControl.Content"/>，单击标题区域可折叠/展开内容，
    /// 折叠后内容区域完全隐藏。视觉风格与控件库主题令牌保持一致。
    /// </summary>
    [TemplatePart(Name = PartHeader, Type = typeof(FrameworkElement))]
    public class GroupBox : System.Windows.Controls.GroupBox
    {
        private const string PartHeader = "PART_Header";

        private FrameworkElement? headerElement;

        static GroupBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GroupBox),
                new FrameworkPropertyMetadata(typeof(GroupBox)));
        }

        /// <summary>
        /// 是否允许单击标题折叠（默认允许）。设为 false 后标题仅作展示：
        /// 左侧折叠箭头一并隐藏，鼠标悬停不出现高亮，点击不会改变折叠状态。
        /// </summary>
        public bool IsCollapsible
        {
            get { return (bool)GetValue(IsCollapsibleProperty); }
            set { SetValue(IsCollapsibleProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="IsCollapsible"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty IsCollapsibleProperty =
            DependencyProperty.Register(nameof(IsCollapsible), typeof(bool), typeof(GroupBox), new PropertyMetadata(true));

        /// <summary>
        /// 内容是否已折叠。折叠后内容区域完全隐藏（Collapsed，不占布局空间）。
        /// </summary>
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="IsCollapsed"/> 的依赖属性，默认支持双向绑定。
        /// </summary>
        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register(
                nameof(IsCollapsed),
                typeof(bool),
                typeof(GroupBox),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 获取模板中的标题宿主元素（PART_Header），用于限定点击折叠的命中范围。
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            headerElement = GetTemplateChild(PartHeader) as FrameworkElement;
        }

        /// <summary>
        /// 标题区域左键单击时切换折叠状态；标题内的按钮等交互元素照常工作。
        /// </summary>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.Handled
                || !IsCollapsible
                || headerElement is null
                || !IsDescendantOf(e.OriginalSource as DependencyObject, headerElement)
                || IsInsideButtonBase(e.OriginalSource as DependencyObject, headerElement))
            {
                return;
            }

            IsCollapsed = !IsCollapsed;
            e.Handled = true;
        }

        /// <summary>
        /// 判断 <paramref name="node"/> 是否位于 <paramref name="ancestor"/> 的可视子树内。
        /// </summary>
        private static bool IsDescendantOf(DependencyObject? node, DependencyObject ancestor)
        {
            while (node is not null)
            {
                if (ReferenceEquals(node, ancestor))
                {
                    return true;
                }

                node = node is Visual || node is System.Windows.Media.Media3D.Visual3D
                    ? VisualTreeHelper.GetParent(node)
                    : LogicalTreeHelper.GetParent(node);
            }

            return false;
        }

        /// <summary>
        /// 判断 <paramref name="node"/> 到 <paramref name="boundary"/> 的路径上是否存在
        /// <see cref="ButtonBase"/>（如按钮、复选框），存在则不拦截其点击。
        /// </summary>
        private static bool IsInsideButtonBase(DependencyObject? node, DependencyObject boundary)
        {
            while (node is not null)
            {
                if (node is ButtonBase)
                {
                    return true;
                }

                if (ReferenceEquals(node, boundary))
                {
                    return false;
                }

                node = node is Visual || node is System.Windows.Media.Media3D.Visual3D
                    ? VisualTreeHelper.GetParent(node)
                    : LogicalTreeHelper.GetParent(node);
            }

            return false;
        }
    }
}
