using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// TabControl 的页签容器，继承 WPF <see cref="TabItem"/>。
    /// 提供图标、关闭按钮（经 <see cref="TabControl.CloseTabCommand"/> 关闭；<see cref="CanClose"/> 为 false 时是固定页签）、
    /// 重命名（双击标题或按 F2，经 <see cref="CanRename"/>；空白标题不提交）与页签图标 <see cref="Icon"/>。
    /// </summary>
    [TemplatePart(Name = PART_EditHeaderTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_CloseButton, Type = typeof(System.Windows.Controls.Button))]
    [TemplatePart(Name = PART_HeaderPresenter, Type = typeof(ContentPresenter))]
    public class TabControlItem : TabItem
    {
        private const string PART_EditHeaderTextBox = "PART_EditHeaderTextBox";
        private const string PART_CloseButton = "PART_CloseButton";
        private const string PART_HeaderPresenter = "PART_HeaderPresenter";

        private TextBox? headerTextBox;
        private System.Windows.Controls.Button? closeButton;

        /// <summary>进入编辑态时的原标题：空白标题提交时据此还原（见 <see cref="TextBox_LostFocus"/>）。</summary>
        private string? headerBeforeEdit;

        static TabControlItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TabControlItem),
                new FrameworkPropertyMetadata(typeof(TabControlItem)));
        }

        public override void OnApplyTemplate()
        {
            if (headerTextBox != null)
            {
                headerTextBox.LostFocus -= TextBox_LostFocus;
                headerTextBox.PreviewKeyDown -= HeaderTextBox_PreviewKeyDown;
            }

            base.OnApplyTemplate();

            headerTextBox = GetTemplateChild(PART_EditHeaderTextBox) as TextBox;
            closeButton = GetTemplateChild(PART_CloseButton) as System.Windows.Controls.Button;
            if (headerTextBox == null)
            {
                return;
            }

            headerTextBox.LostFocus += TextBox_LostFocus;
            headerTextBox.PreviewKeyDown += HeaderTextBox_PreviewKeyDown;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            // 双击必须确由本页签自身的可视子树（页签头区域）发起才进入重命名：
            // MouseLeftButtonDown / MouseDoubleClick 都会投递到本容器，内容区、嵌套控件等
            // 其他子树的双击也可能到达本容器，来源校验避免「内容双击误触更名」。
            // 关闭按钮在本页签子树内，需单独排除。
            // 注意 MouseDoubleClick 是 RoutingStrategy.Direct 的路由事件，不沿可视树路由，
            // 置 Handled 拦不住任何东西——承重的守卫是下面两条子树校验。
            if (!e.Handled
                && TabControl.IsWithinSubtree(e.OriginalSource as DependencyObject, this)
                && !TabControl.IsWithinSubtree(e.OriginalSource as DependencyObject, closeButton)
                && TryBeginEdit())
            {
                // 无需处理 Handled：Direct 事件不路由到祖先容器
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // F2 是编辑类控件的通用重命名入口，给纯鼠标的双击补一条键盘路径
            if (!e.Handled && e.Key == Key.F2 && TryBeginEdit())
            {
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        /// 进入页签标题编辑态：先过全部闸门（条目级 / 控件级开关、未在编辑、标题是字符串、编辑框已就位），
        /// 再延迟取焦并全选。双击与 F2 共用本方法，两条入口的闸门因此不会走样。
        /// </summary>
        private bool TryBeginEdit()
        {
            if (!CanRename
                || !HostAllowsRename()
                || IsEditing
                || Header is not string
                || headerTextBox is not TextBox editBox)
            {
                return false;
            }

            SetValue(IsEditingPropertyKey, true);
            headerBeforeEdit = Header as string;

            // 捕获本次的编辑框实例：延迟派发期间模板可能重建并把字段置空
            Dispatcher.BeginInvoke(new Action(() =>
            {
                editBox.Focus();
                editBox.SelectAll();
            }), DispatcherPriority.Input);

            return true;
        }

        /// <summary>控件级重命名开关查询：所在 TabControl 的 CanRename；不在 TabControl 内时视为允许。</summary>
        private bool HostAllowsRename()
        {
            return (ItemsControl.ItemsControlFromItemContainer(this) as TabControl)?.CanRename ?? true;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SetValue(IsEditingPropertyKey, false);

            // 空白标题不予提交：本次编辑视为放弃，标题回落进入编辑态前的原值。
            // 还原写在 LostFocus 里而不是 LostKeyboardFocus：编辑框的绑定（UpdateSourceTrigger=LostFocus）
            // 先于本处理程序把文本写回 Header，此处必须显式写回一次，不能依赖两个焦点事件的先后顺序。
            TextBox textBox = (TextBox)sender;
            if (headerBeforeEdit is not null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                SetCurrentValue(HeaderProperty, headerBeforeEdit);
            }

            headerBeforeEdit = null;
        }

        private void HeaderTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Enter:
                    // 提交编辑：移出焦点，经LostFocus绑定把文本写回Header
                    e.Handled = true;
                    Keyboard.Focus(this);
                    break;
                case Key.Escape:
                    // 取消编辑：绑定在LostFocus才写回，先恢复原文本再移出焦点即可还原
                    e.Handled = true;
                    textBox.Text = Header as string ?? string.Empty;
                    Keyboard.Focus(this);
                    break;
            }
        }

        private static readonly DependencyPropertyKey IsEditingPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsEditing),
                typeof(bool),
                typeof(TabControlItem),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        public bool IsEditing => (bool)GetValue(IsEditingProperty);

        /// <summary>
        /// 是否允许双击页签标题进入重命名（条目级开关，默认允许）。设为 false 后双击不再进入编辑态；
        /// 若在编辑过程中被禁用，将立即退出编辑并保留当前文本。
        /// 还需所在 <c>TabControl</c> 的控件级 <c>CanRename</c> 同为 true 才能进入重命名。
        /// </summary>
        public static readonly DependencyProperty CanRenameProperty =
            DependencyProperty.Register(nameof(CanRename), typeof(bool), typeof(TabControlItem), new PropertyMetadata(true, OnCanRenameChanged));

        public bool CanRename
        {
            get { return (bool)GetValue(CanRenameProperty); }
            set { SetValue(CanRenameProperty, value); }
        }

        private static void OnCanRenameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabControlItem item && !(bool)e.NewValue && item.IsEditing)
            {
                item.SetValue(IsEditingPropertyKey, false);
            }
        }

        /// <summary>
        /// 是否允许关闭本页签（条目级开关，默认允许）。用于「固定页签」——首页、固定监控页这类不允许被关掉的页签。
        /// 设为 <c>false</c> 后：关闭按钮不再显示，<see cref="TabControl.CloseTab"/> 静默忽略，
        /// <see cref="TabControl.CloseTabCommand"/> 的 CanExecute 为 <c>false</c>（关闭按钮随之禁用）。
        /// 与 <see cref="TabControl.CanCloseLastTab"/> 正交：后者管数量下限（最后一个页签能不能关），本属性管单个页签。
        /// </summary>
        public static readonly DependencyProperty CanCloseProperty =
            DependencyProperty.Register(nameof(CanClose), typeof(bool), typeof(TabControlItem), new PropertyMetadata(true));

        public bool CanClose
        {
            get { return (bool)GetValue(CanCloseProperty); }
            set { SetValue(CanCloseProperty, value); }
        }

        /// <summary>页签图标；通常为图标字体字形字符串，字体族取所在 <c>TabControl</c> 的 <c>atc:Icon.FontFamily</c>。</summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(object), typeof(TabControlItem));

        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
    }
}
