using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// TreeMenu 的树节点数据模型（POCO，非控件）。作为 <see cref="TreeMenu"/> 的
    /// <c>ItemsSource</c> 条目使用，由默认的 <see cref="System.Windows.HierarchicalDataTemplate"/>
    /// 渲染 <see cref="Title"/> 与 <see cref="Icon"/>，层级由 <see cref="Children"/> 提供。
    /// 不继承 DispatcherObject，可在任意线程构建数据。
    /// </summary>
    public class TreeMenuItem : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private object? _icon;
        private Type? _targetType;
        private bool _isExpanded;
        private bool _isSelected;

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>节点标题。</summary>
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>节点图标；通常为图标字体字形字符串，模板按图标字体渲染。</summary>
        public object? Icon
        {
            get => _icon;
            set
            {
                if (!Equals(_icon, value))
                {
                    _icon = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>节点对应的页面类型等业务元数据，由 <c>TreeMenu.NavigateCommand</c> 的处理方自行解释。</summary>
        public Type? TargetType
        {
            get => _targetType;
            set
            {
                if (_targetType != value)
                {
                    _targetType = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 展开/收起状态。默认容器样式将其与 <see cref="System.Windows.Controls.TreeViewItem.IsExpanded"/>
        /// 双向绑定，直接修改即可展开或收起节点；虚拟化/容器回收后状态仍保留在模型上。
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 选中状态。默认容器样式将其与 <see cref="System.Windows.Controls.TreeViewItem.IsSelected"/>
        /// 双向绑定，直接修改即可选中节点。
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>子节点集合。构造时自动初始化，直接 <c>Add</c> 即可更新视图。</summary>
        public ObservableCollection<TreeMenuItem> Children { get; } = [];

        /// <summary>是否为叶节点（<see cref="Children"/> 为空）。</summary>
        public bool IsLeaf => Children.Count == 0;

        /// <inheritdoc />
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
