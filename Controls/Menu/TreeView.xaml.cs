using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Junevy.Controls.AttachedProperties;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// 树形视图，继承 WPF <see cref="System.Windows.Controls.TreeView"/>。条目为 <see cref="TreeMenuItem"/> 数据模型，
    /// 由库内唯一的 <see cref="System.Windows.HierarchicalDataTemplate"/> 渲染；
    /// 叶节点的激活（双击或按 Enter）经 <see cref="NavigateCommand"/> 交由宿主处理，
    /// 展开/选中交互见 <see cref="ExpanderBehavior"/>（默认容器样式已启用）。
    /// 合并 Themes/Generic.xaml 后，官方 <see cref="System.Windows.Controls.TreeView"/> 自动继承同一默认样式，
    /// 其扩展能力经 <see cref="TreeViewAssist"/> 附加属性提供（本类的实例依赖属性与其为同一 DP）。
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

        /// <summary>激活叶节点时执行的命令（双击或按 Enter，见 <see cref="ExpanderBehavior"/>），参数为该节点的数据对象。默认 null。</summary>
        public static readonly DependencyProperty NavigateCommandProperty =
            TreeViewAssist.NavigateCommandProperty.AddOwner(typeof(TreeView));

        public ICommand? NavigateCommand
        {
            get { return (ICommand?)GetValue(NavigateCommandProperty); }
            set { SetValue(NavigateCommandProperty, value); }
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

        /// <summary>
        /// 递归展开 <see cref="Items"/> 中每个 <see cref="TreeMenuItem"/>（写数据模型，虚拟化下尚未生成的容器同样被覆盖），
        /// 并把上一次 <see cref="CollapseAll"/> 期间被 WPF 原生语义「上提到分支」的选中还原回原来的深层节点。
        /// 条目不是 <see cref="TreeMenuItem"/> 时展开不生效——自定义模型需在自己的数据类上实现等价的递归展开。
        /// </summary>
        public void ExpandAll()
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

        /// <summary>
        /// 递归收起 <see cref="Items"/> 中每个 <see cref="TreeMenuItem"/>。
        /// WPF 原生语义：收起内含选中子孙的分支会把选中上提到该分支（本库不改动这一行为），
        /// 因此这里记住上提前的节点，供 <see cref="ExpandAll"/> 还原；期间任何手工改变选中都会作废该还原。
        /// </summary>
        public void CollapseAll()
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

        /// <inheritdoc />
        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);

            // CollapseAll 期间的「选中上提」不会作废还原目标：那里是先取局部、循环结束后才赋值，
            // 本回调必然发生在赋值之前。这里无需识别来源。
            _selectionToRestore = null;
        }
    }
}
