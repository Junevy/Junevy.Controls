using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// 树形导航菜单，继承 WPF <see cref="TreeView"/>。条目为 <see cref="TreeMenuItem"/> 数据模型，
    /// 由默认 <see cref="System.Windows.HierarchicalDataTemplate"/> 渲染；
    /// 叶节点的激活（双击或 Enter）经 <see cref="NavigateCommand"/> 交由宿主处理。
    /// </summary>
    public class TreeMenu : TreeView
    {

        static TreeMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TreeMenu),
                new FrameworkPropertyMetadata(typeof(TreeMenu)));
        }


        /// <summary>激活叶节点时执行的命令（双击或按 Enter，见 <see cref="ExpanderBehavior"/>），参数为该节点的数据对象。</summary>
        public ICommand NavigateCommand
        {
            get { return (ICommand)GetValue(NavigateCommandProperty); }
            set { SetValue(NavigateCommandProperty, value); }
        }
        public static readonly DependencyProperty NavigateCommandProperty =
            DependencyProperty.Register("NavigateCommand", typeof(ICommand), typeof(TreeMenu));




        /// <summary>显示模式：<see cref="DisplayMode.Normal"/> 显示展开箭头，<see cref="DisplayMode.Icon"/> 使用左侧层级指示器。</summary>
        public DisplayMode DisplayMode
        {
            get { return (DisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(DisplayMode), typeof(TreeMenu), new PropertyMetadata(DisplayMode.Normal));

        /// <summary>节点悬停背景画刷(默认 Surface.Hover;可自定义,模板触发器经 AncestorType 绑定)。</summary>
        public Brush ItemHoverBackground
        {
            get { return (Brush)GetValue(ItemHoverBackgroundProperty); }
            set { SetValue(ItemHoverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ItemHoverBackgroundProperty =
            DependencyProperty.Register("ItemHoverBackground", typeof(Brush), typeof(TreeMenu), new PropertyMetadata(null));

        /// <summary>选中节点背景画刷(默认 Surface.Sunken 中性灰;可自定义)。</summary>
        public Brush SelectedItemBackground
        {
            get { return (Brush)GetValue(SelectedItemBackgroundProperty); }
            set { SetValue(SelectedItemBackgroundProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemBackgroundProperty =
            DependencyProperty.Register("SelectedItemBackground", typeof(Brush), typeof(TreeMenu), new PropertyMetadata(null));



    }
}
