using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>携带树节点的路由事件回调。</summary>
    public delegate void TreeItemEventHandler(object sender, TreeItemEventArgs e);

    /// <summary>
    /// <see cref="TreeView.ItemDoubleClick"/> 路由事件的事件参数。
    /// </summary>
    public class TreeItemEventArgs : RoutedEventArgs
    {
        public TreeItemEventArgs(RoutedEvent routedEvent, object source, TreeViewItem container, object? item)
            : base(routedEvent, source)
        {
            Container = container;
            Item = item;
        }

        /// <summary>
        /// 双击命中的数据项：<c>ItemsSource</c> 模式下为模型对象；直接往 <c>Items</c> 放对象时为该条目本身
        /// （可能是容器自身）。双击未携带数据的容器时为 <c>null</c>。
        /// </summary>
        public object? Item { get; }

        /// <summary>双击命中的 <see cref="TreeViewItem"/> 容器。仅事件处理期间有效，不要跨事件缓存（容器不回收但可能随条目增删失效）。</summary>
        public TreeViewItem Container { get; }
    }
}
