using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Junevy.Controls.AttachedProperties;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// 树形视图，继承 WPF <see cref="System.Windows.Controls.TreeView"/>。条目为 <see cref="TreeMenuItem"/> 数据模型，
    /// 由默认 <see cref="System.Windows.HierarchicalDataTemplate"/> 渲染；
    /// 叶节点的激活（双击或 Enter）经 <see cref="NavigateCommand"/> 交由宿主处理。
    /// 合并 Themes/Generic.xaml 后，官方 <see cref="System.Windows.Controls.TreeView"/> 自动继承同一默认样式，
    /// 其扩展能力经 <see cref="TreeViewAssist"/> 附加属性提供（本类的实例依赖属性与其同一 DP）。
    /// </summary>
    public class TreeView : System.Windows.Controls.TreeView
    {

        static TreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TreeView),
                new FrameworkPropertyMetadata(typeof(TreeView)));
        }


        /// <summary>激活叶节点时执行的命令（双击或按 Enter，见 <see cref="ExpanderBehavior"/>），参数为该节点的数据对象。</summary>
        public ICommand NavigateCommand
        {
            get { return (ICommand)GetValue(NavigateCommandProperty); }
            set { SetValue(NavigateCommandProperty, value); }
        }
        /// <summary><see cref="NavigateCommand"/> 的依赖属性标识符（与 <see cref="TreeViewAssist.NavigateCommandProperty"/> 同一 DP）。</summary>
        public static readonly DependencyProperty NavigateCommandProperty =
            TreeViewAssist.NavigateCommandProperty.AddOwner(typeof(TreeView));




        /// <summary>显示模式：<see cref="DisplayMode.Normal"/> 显示展开箭头，<see cref="DisplayMode.Icon"/> 使用左侧层级指示器。</summary>
        public DisplayMode DisplayMode
        {
            get { return (DisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }
        /// <summary><see cref="DisplayMode"/> 的依赖属性标识符（与 <see cref="TreeViewAssist.DisplayModeProperty"/> 同一 DP）。</summary>
        public static readonly DependencyProperty DisplayModeProperty =
            TreeViewAssist.DisplayModeProperty.AddOwner(typeof(TreeView));

        /// <summary>节点悬停背景画刷(默认 Surface.Hover;可自定义,模板触发器经 AncestorType 绑定)。</summary>
        public Brush ItemHoverBackground
        {
            get { return (Brush)GetValue(ItemHoverBackgroundProperty); }
            set { SetValue(ItemHoverBackgroundProperty, value); }
        }
        /// <summary><see cref="ItemHoverBackground"/> 的依赖属性标识符（与 <see cref="TreeViewAssist.ItemHoverBackgroundProperty"/> 同一 DP）。</summary>
        public static readonly DependencyProperty ItemHoverBackgroundProperty =
            TreeViewAssist.ItemHoverBackgroundProperty.AddOwner(typeof(TreeView));

        /// <summary>选中节点背景画刷(默认 Surface.Sunken 中性灰;可自定义)。</summary>
        public Brush SelectedItemBackground
        {
            get { return (Brush)GetValue(SelectedItemBackgroundProperty); }
            set { SetValue(SelectedItemBackgroundProperty, value); }
        }
        /// <summary><see cref="SelectedItemBackground"/> 的依赖属性标识符（与 <see cref="TreeViewAssist.SelectedItemBackgroundProperty"/> 同一 DP）。</summary>
        public static readonly DependencyProperty SelectedItemBackgroundProperty =
            TreeViewAssist.SelectedItemBackgroundProperty.AddOwner(typeof(TreeView));



    }
}
