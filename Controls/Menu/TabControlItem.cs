using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// TabControl 的页签容器，继承 WPF <see cref="TabItem"/>。
    /// 提供图标、关闭按钮（经 <see cref="TabControl.CloseTabCommand"/> 关闭）与双击标题重命名（<see cref="CanRename"/>）。
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

            if (closeButton != null)
            {
                closeButton.MouseDoubleClick -= CloseButton_MouseDoubleClick;
            }

            base.OnApplyTemplate();

            headerTextBox = GetTemplateChild(PART_EditHeaderTextBox) as TextBox;
            closeButton = GetTemplateChild(PART_CloseButton) as System.Windows.Controls.Button;
            if (closeButton != null)
            {
                closeButton.MouseDoubleClick += CloseButton_MouseDoubleClick;
            }

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
            // MouseLeftButtonDown / MouseDoubleClick 都是冒泡路由，内容区、嵌套控件等
            // 其他子树的双击也可能投递到本容器，来源校验避免「内容双击误触更名」。
            if (!e.Handled
                && CanRename
                && HostAllowsRename()
                && !IsEditing
                && Header is string
                && headerTextBox is TextBox editBox
                && TabControl.IsWithinSubtree(e.OriginalSource as DependencyObject, this)
                && !TabControl.IsWithinSubtree(e.OriginalSource as DependencyObject, closeButton))
            {
                e.Handled = true;
                SetValue(IsEditingPropertyKey, true);

                // 捕获本次的编辑框实例：延迟派发期间模板可能重建并把字段置空
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    editBox.Focus();
                    editBox.SelectAll();
                }), DispatcherPriority.Input);
            }

            base.OnMouseDoubleClick(e);
        }

        /// <summary>控件级重命名开关查询：所在 TabControl 的 CanRename；不在 TabControl 内时视为允许。</summary>
        private bool HostAllowsRename()
        {
            return (ItemsControl.ItemsControlFromItemContainer(this) as TabControl)?.CanRename ?? true;
        }

        private void CloseButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SetValue(IsEditingPropertyKey, false);
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
