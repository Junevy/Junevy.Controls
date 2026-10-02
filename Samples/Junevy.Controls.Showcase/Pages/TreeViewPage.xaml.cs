using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Junevy.Controls.AttachedProperties;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Bar;
using Junevy.Controls.Controls.Menu;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>树形视图展示页：jv:TreeView 数据模型驱动 / Icon 模式 / 官方 TreeView 自动继承样式 / 代码控制展开与选中。</summary>
    public partial class TreeViewPage : UserControl
    {
        /// <summary>NavigateCommand 收到的叶节点激活反馈（参数为叶节点的数据对象）。</summary>
        private readonly ParameterRelayCommand navigateLeafCommand = new(
            p => MessageBarService.Show("TreeView", $"激活叶节点：{(p as TreeMenuItem)?.Title ?? p}"));

        public TreeViewPage()
        {
            InitializeComponent();

            MainTree.ItemsSource = new[] { BuildStationTree() };
            MainTree.NavigateCommand = navigateLeafCommand;

            IconModeTree.ItemsSource = new[] { BuildStationTree() };

            // 官方 <TreeView>：扩展能力全部经 atc:TreeViewAssist 附加属性提供
            NativeTree.ItemsSource = new[] { BuildStationTree() };
            TreeViewAssist.SetNavigateCommand(NativeTree, navigateLeafCommand);

            ControlledTree.ItemsSource = new[] { BuildStationTree() };
            ControlledTree.NavigateCommand = navigateLeafCommand;
        }

        /// <summary>构建演示树：检测工作站 → 图像采集 / 运动控制 / 数据报表，四级层级、图标与初始展开状态齐全。</summary>
        private static TreeMenuItem BuildStationTree()
        {
            return new TreeMenuItem
            {
                Title = "检测工作站",
                Icon = "\uE6BC",
                IsExpanded = true,
                Children =
                {
                    new TreeMenuItem
                    {
                        Title = "图像采集",
                        Icon = "\uE60C",
                        IsExpanded = true,
                        Children =
                        {
                            new TreeMenuItem { Title = "相机 A（1 号工位）", IsSelected = true },
                            new TreeMenuItem { Title = "相机 B（2 号工位）" },
                            new TreeMenuItem
                            {
                                Title = "标定",
                                Icon = "\uE602",
                                Children =
                                {
                                    new TreeMenuItem { Title = "九点标定" },
                                    new TreeMenuItem { Title = "手眼标定" }
                                }
                            }
                        }
                    },
                    new TreeMenuItem
                    {
                        Title = "运动控制",
                        Icon = "\uE603",
                        IsExpanded = true,
                        Children =
                        {
                            new TreeMenuItem { Title = "X 轴" },
                            new TreeMenuItem { Title = "Y 轴" },
                            new TreeMenuItem { Title = "Z 轴" }
                        }
                    },
                    new TreeMenuItem
                    {
                        Title = "数据报表",
                        Icon = "\uE646",
                        Children =
                        {
                            new TreeMenuItem { Title = "检测报告" },
                            new TreeMenuItem { Title = "趋势分析" }
                        }
                    },
                    new TreeMenuItem { Title = "系统日志", Icon = "\uE651" }
                }
            };
        }

        private void OnExpandAllClick(object sender, RoutedEventArgs e)
        {
            foreach (var root in ControlledTree.Items)
            {
                if (root is TreeMenuItem node)
                {
                    SetExpanded(node, true);
                }
            }
        }

        private void OnCollapseAllClick(object sender, RoutedEventArgs e)
        {
            foreach (var root in ControlledTree.Items)
            {
                if (root is TreeMenuItem node)
                {
                    SetExpanded(node, false);
                }
            }
        }

        private void OnSelectCalibrationClick(object sender, RoutedEventArgs e)
        {
            foreach (var root in ControlledTree.Items)
            {
                if (root is not TreeMenuItem node)
                {
                    continue;
                }

                var path = new List<TreeMenuItem>();
                if (TryFindNode(node, "九点标定", path))
                {
                    // 展开祖先使目标可见，最后一级 IsSelected 双向绑定回写容器选中态
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        path[i].IsExpanded = true;
                    }
                    path[^1].IsSelected = true;
                    MessageBarService.Show("TreeView", "已选中「九点标定」（IsSelected 写入数据模型）");
                    return;
                }
            }
        }

        private static void SetExpanded(TreeMenuItem node, bool value)
        {
            node.IsExpanded = value;
            foreach (var child in node.Children)
            {
                SetExpanded(child, value);
            }
        }

        private static bool TryFindNode(TreeMenuItem node, string title, List<TreeMenuItem> path)
        {
            path.Add(node);
            if (node.Title == title)
            {
                return true;
            }

            foreach (var child in node.Children)
            {
                if (TryFindNode(child, title, path))
                {
                    return true;
                }
            }

            path.RemoveAt(path.Count - 1);
            return false;
        }

        /// <summary>携带参数的轻量 ICommand（RelayCommand 丢弃参数，导航反馈需要节点对象）。</summary>
        private sealed class ParameterRelayCommand(Action<object?> execute) : ICommand
        {
            public event EventHandler? CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                execute(parameter);
            }
        }
    }
}
