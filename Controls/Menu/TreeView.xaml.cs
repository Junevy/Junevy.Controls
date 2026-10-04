using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Junevy.Controls.AttachedProperties;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// 树形视图，继承 WPF <see cref="System.Windows.Controls.TreeView"/>。条目为 <see cref="TreeMenuItem"/> 数据模型，
    /// 由库内唯一的 <see cref="System.Windows.HierarchicalDataTemplate"/> 渲染；
    /// 展开/选中交互见 <see cref="ExpanderBehavior"/>（默认容器样式已启用）。
    /// <see cref="NavigateCommand"/> 为**选中驱动**：选中项变化（单击、键盘 ↑↓、程序化 <c>IsSelected=true</c>）即执行，
    /// 参数为新选中的数据项——MVVM 下直接绑定即可导航；<see cref="ItemDoubleClick"/> 为**双击驱动**的冒泡路由事件，
    /// 携带数据项与容器，表达「打开/编辑」类意图。两原语正交（详见行为矩阵）。
    /// 合并 Themes/Generic.xaml 后，官方 <see cref="System.Windows.Controls.TreeView"/> 自动继承同一默认样式，
    /// 其扩展能力经 <see cref="TreeViewAssist"/> 附加属性提供（本类的实例依赖属性与其为同一 DP；
    /// 官方实例的命令导航维持叶激活旧语义，见 <see cref="ExpanderBehavior"/>）。
    /// </summary>
    public class TreeView : System.Windows.Controls.TreeView
    {
        static TreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(typeof(TreeView)));
        }

        /// <summary>行首观感（见 <see cref="TreeViewDisplayMode"/>）。</summary>
        public static readonly DependencyProperty DisplayModeProperty =
            TreeViewAssist.DisplayModeProperty.AddOwner(typeof(TreeView));

        public TreeViewDisplayMode DisplayMode
        {
            get { return (TreeViewDisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        /// <summary>
        /// 选中驱动导航命令：选中项变化（单击、键盘 ↑↓、程序化 <c>IsSelected=true</c>）时执行，
        /// 参数为新选中的数据项（模型对象，非容器）。清除选中、<see cref="CollapseAll"/>/<see cref="ExpandAll"/>
        /// 引起的选中上提与还原不触发；<see cref="ICommand.CanExecute"/> 为 false 时静默跳过。
        /// 命令处理器内同步改选中不会再次触发（重入守卫），需导航重定向请派发到 Dispatcher 队列。
        /// </summary>
        public static readonly DependencyProperty NavigateCommandProperty =
            TreeViewAssist.NavigateCommandProperty.AddOwner(typeof(TreeView));

        public ICommand? NavigateCommand
        {
            get { return (ICommand?)GetValue(NavigateCommandProperty); }
            set { SetValue(NavigateCommandProperty, value); }
        }

        /// <summary>
        /// 双击驱动导航事件（Bubble）：左键双击命中某 <see cref="TreeViewItem"/>（第 2 次按下）时抛出，
        /// 参数携带数据项与容器；双击展开箭头、滚动条、空白不触发，分组/叶子照发由消费方自滤。
        /// 与继承的 <see cref="Control.MouseDoubleClick"/>（Direct、不携带数据项）互补，推荐用本事件。
        /// </summary>
        public static readonly RoutedEvent ItemDoubleClickEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ItemDoubleClick),
                RoutingStrategy.Bubble,
                typeof(TreeItemEventHandler),
                typeof(TreeView));

        public event TreeItemEventHandler ItemDoubleClick
        {
            add { AddHandler(ItemDoubleClickEvent, value); }
            remove { RemoveHandler(ItemDoubleClickEvent, value); }
        }

        /// <summary>节点悬停背景画刷。默认 null，由样式注入 Surface.Hover；显式设 null 时模板回退触发器仍取该令牌值。</summary>
        public static readonly DependencyProperty ItemHoverBackgroundProperty =
            TreeViewAssist.ItemHoverBackgroundProperty.AddOwner(typeof(TreeView));

        public Brush? ItemHoverBackground
        {
            get { return (Brush?)GetValue(ItemHoverBackgroundProperty); }
            set { SetValue(ItemHoverBackgroundProperty, value); }
        }

        /// <summary>选中节点背景画刷。默认 null，由样式注入 Surface.Sunken；显式设 null 时模板回退触发器仍取该令牌值。</summary>
        public static readonly DependencyProperty SelectedItemBackgroundProperty =
            TreeViewAssist.SelectedItemBackgroundProperty.AddOwner(typeof(TreeView));

        public Brush? SelectedItemBackground
        {
            get { return (Brush?)GetValue(SelectedItemBackgroundProperty); }
            set { SetValue(SelectedItemBackgroundProperty, value); }
        }

        /// <summary>子级相对本级的缩进宽度（DIP），默认 10。</summary>
        public static readonly DependencyProperty IndentSizeProperty =
            TreeViewAssist.IndentSizeProperty.AddOwner(typeof(TreeView));

        public double IndentSize
        {
            get { return (double)GetValue(IndentSizeProperty); }
            set { SetValue(IndentSizeProperty, value); }
        }

        /// <summary>选中节点时是否自动展开其祖先，默认 true。</summary>
        public static readonly DependencyProperty AutoExpandAncestorsProperty =
            TreeViewAssist.AutoExpandAncestorsProperty.AddOwner(typeof(TreeView));

        public bool AutoExpandAncestors
        {
            get { return (bool)GetValue(AutoExpandAncestorsProperty); }
            set { SetValue(AutoExpandAncestorsProperty, value); }
        }

        private object? _selectionToRestore;
        private bool _isNavigating;
        private bool _suppressNavigation;

        /// <summary>
        /// 递归展开 <see cref="Items"/> 中每个 <see cref="TreeMenuItem"/>（写数据模型，虚拟化下尚未生成的容器同样被覆盖），
        /// 并把上一次 <see cref="CollapseAll"/> 期间被 WPF 原生语义「上提到分支」的选中还原回原来的深层节点。
        /// 条目不是 <see cref="TreeMenuItem"/> 时展开不生效——自定义模型需在自己的数据类上实现等价的递归展开。
        /// 期间由收起/还原引起的选中变化不触发 <see cref="NavigateCommand"/>。
        /// </summary>
        public void ExpandAll()
        {
            _suppressNavigation = true;
            try
            {
                foreach (var item in Items)
                {
                    if (item is TreeMenuItem node)
                    {
                        node.ExpandAll();
                    }
                }

                var target = _selectionToRestore;
                _selectionToRestore = null;
                if (target is TreeMenuItem selectedNode && !ReferenceEquals(SelectedItem, selectedNode))
                {
                    // 走模型而非直接设 SelectedItem：展开后容器要等一趟布局才生成，
                    // 而容器样式上的 IsSelected 双向绑定在生成时即把选中显形，WPF 同时取消分支的选中。
                    selectedNode.IsSelected = true;
                }
            }
            finally
            {
                _suppressNavigation = false;
            }
        }

        /// <summary>
        /// 递归收起 <see cref="Items"/> 中每个 <see cref="TreeMenuItem"/>。
        /// WPF 原生语义：收起内含选中子孙的分支会把选中上提到该分支（本库不改动这一行为），
        /// 因此这里记住上提前的节点，供 <see cref="ExpandAll"/> 还原；期间任何手工改变选中都会作废该还原。
        /// 期间由收起引起的选中变化不触发 <see cref="NavigateCommand"/>。
        /// </summary>
        public void CollapseAll()
        {
            _suppressNavigation = true;
            try
            {
                var selected = SelectedItem;
                foreach (var item in Items)
                {
                    if (item is TreeMenuItem node)
                    {
                        node.CollapseAll();
                    }
                }

                _selectionToRestore = selected != null && !ReferenceEquals(selected, SelectedItem) ? selected : null;
            }
            finally
            {
                _suppressNavigation = false;
            }
        }

        /// <inheritdoc />
        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);

            // CollapseAll 期间的「选中上提」不会作废还原目标：那里是先取局部、循环结束后才赋值，
            // 本回调必然发生在赋值之前。这里无需识别来源。
            _selectionToRestore = null;

            // 选中驱动导航：清除选中、整树收起/展开的选中变化均不触发；
            // _isNavigating 吞掉命令处理器内同步改选中的重入（防 ping-pong，重定向不二次导航）。
            if (_suppressNavigation || _isNavigating || e.NewValue is null)
            {
                return;
            }

            if (NavigateCommand?.CanExecute(e.NewValue) != true)
            {
                return;
            }

            _isNavigating = true;
            try
            {
                NavigateCommand.Execute(e.NewValue);
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <inheritdoc />
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            // 双击检测必须在隧道层：ExpanderBehavior 在条目上以 PreviewMouseLeftButtonDown 接管分支双击并置
            // Handled，冒泡阶段的 MouseLeftButtonDown 会被抑制——隧道先于条目处理器运行，才能两不误。
            if (e.ClickCount != 2)
            {
                return;
            }

            // 命中容器才发：滚动条、空白处不触发。
            if (FindAncestor<TreeViewItem>(e.OriginalSource as DependencyObject) is not { } container)
            {
                return;
            }

            // 只认本树的容器：嵌套 TreeView（内容区里的子树）的双击由内层树负责。
            if (!ReferenceEquals(FindAncestor<System.Windows.Controls.TreeView>(container), this))
            {
                return;
            }

            // 展开箭头双击 = 两次切换（净效果为零），不发「打开」事件；与 ExpanderBehavior 同守卫。
            if (FindAncestor<ToggleButton>(e.OriginalSource as DependencyObject) != null)
            {
                return;
            }

            RaiseEvent(new TreeItemEventArgs(ItemDoubleClickEvent, this, container, ResolveItem(container)));
        }

        /// <summary>把容器还原为数据项：<c>ItemsSource</c> 模式经生成器取回模型；直接 <c>Items</c> 模式回退容器 DataContext。</summary>
        private object? ResolveItem(TreeViewItem container)
        {
            var item = ItemContainerGenerator.ItemFromContainer(container);
            return item == DependencyProperty.UnsetValue ? container.DataContext : item;
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match)
                {
                    return match;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
