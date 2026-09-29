using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Bar;

namespace Junevy.Controls.Showcase.Controls
{
    /// <summary>
    /// 演示区块：标题 + 描述 + 演示内容 + 可折叠的 XAML 源码块（语法高亮 + 一键复制）。
    /// 隐式样式与模板定义在 App.xaml 合并的 Controls/DemoSection.xaml 中；
    /// 源码着色基于 <see cref="XamlHighlighter"/>，画刷经 DynamicResource 绑定主题令牌，随明暗主题自动切换。
    /// </summary>
    public class DemoSection : ContentControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(DemoSection), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(DemoSection), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty XamlCodeProperty =
            DependencyProperty.Register(nameof(XamlCode), typeof(string), typeof(DemoSection), new PropertyMetadata(string.Empty, OnXamlCodeChanged));

        public static readonly DependencyProperty IsCodeExpandedProperty =
            DependencyProperty.Register(nameof(IsCodeExpanded), typeof(bool), typeof(DemoSection), new PropertyMetadata(true));

        private TextBlock? _codeTextBlock;
        private ScrollViewer? _codeScroll;

        public DemoSection()
        {
            CopyCodeCommand = new RelayCommand(CopyCode);
            ToggleCodeCommand = new RelayCommand(() => IsCodeExpanded = !IsCodeExpanded);
        }

        /// <summary>控件名（区块标题）。</summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>用法说明（标题下方、演示内容上方）。</summary>
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        /// <summary>展示在演示内容下方的 XAML 源码片段。</summary>
        public string XamlCode
        {
            get => (string)GetValue(XamlCodeProperty);
            set => SetValue(XamlCodeProperty, value);
        }

        /// <summary>代码块是否展开（默认展开，可经「收起代码」切换）。</summary>
        public bool IsCodeExpanded
        {
            get => (bool)GetValue(IsCodeExpandedProperty);
            set => SetValue(IsCodeExpandedProperty, value);
        }

        /// <summary>复制源码到剪贴板（模板「复制代码」按钮绑定）。</summary>
        public ICommand CopyCodeCommand { get; }

        /// <summary>切换代码块显隐（模板「收起/展开代码」按钮绑定）。</summary>
        public ICommand ToggleCodeCommand { get; }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _codeTextBlock = GetTemplateChild("PART_CodeText") as TextBlock;
            RenderCode();

            if (_codeScroll != null)
            {
                _codeScroll.PreviewMouseWheel -= OnCodeScrollPreviewMouseWheel;
            }

            _codeScroll = GetTemplateChild("PART_CodeScroll") as ScrollViewer;
            if (_codeScroll != null)
            {
                _codeScroll.PreviewMouseWheel += OnCodeScrollPreviewMouseWheel;
            }
        }

        private static void OnXamlCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DemoSection section)
            {
                section.RenderCode();
            }
        }

        private void RenderCode()
        {
            if (_codeTextBlock == null)
            {
                return;
            }

            _codeTextBlock.Inlines.Clear();
            foreach (var token in XamlHighlighter.Tokenize(XamlCode))
            {
                var run = new Run(token.Text);
                run.SetResourceReference(TextElement.ForegroundProperty, ResourceKey(token.Kind));
                if (token.Kind == XamlTokenKind.Comment)
                {
                    run.FontStyle = FontStyles.Italic;
                }

                _codeTextBlock.Inlines.Add(run);
            }
        }

        private void CopyCode()
        {
            try
            {
                Clipboard.SetText(XamlCode);
                MessageBarService.Show("演示", $"「{Title}」的 XAML 代码已复制到剪贴板。");
            }
            catch (Exception)
            {
                MessageBarService.Show(MessageBarAppearance.Warning, "演示", "复制失败：剪贴板被其他进程占用，请稍后重试。");
            }
        }

        /// <summary>
        /// 代码块内层 ScrollViewer 纵向滚动是禁用的（只横向滚动），
        /// 但它会把滚轮事件吞掉，导致鼠标悬停在代码块上时外层页面无法滚动。
        /// 这里在内层自身无法纵向消费滚轮时，把事件转发给外层可滚动的 ScrollViewer。
        /// </summary>
        private void OnCodeScrollPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta == 0 || sender is not ScrollViewer inner)
            {
                return;
            }

            var innerCanScroll = e.Delta < 0
                ? inner.VerticalOffset < inner.ScrollableHeight
                : inner.VerticalOffset > 0;
            if (innerCanScroll)
            {
                return;
            }

            var outer = FindOuterScrollViewer(inner);
            if (outer == null)
            {
                return;
            }

            e.Handled = true;
            var forwarded = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = inner
            };
            outer.RaiseEvent(forwarded);
        }

        private static ScrollViewer? FindOuterScrollViewer(DependencyObject current)
        {
            var parent = VisualTreeHelper.GetParent(current);
            while (parent != null)
            {
                if (parent is ScrollViewer scrollViewer)
                {
                    return scrollViewer;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        private static string ResourceKey(XamlTokenKind kind) => kind switch
        {
            XamlTokenKind.Element => "Theme.Brush.Accent.Primary",
            XamlTokenKind.Attribute => "Theme.Brush.Status.Danger",
            XamlTokenKind.Value => "Theme.Brush.Status.Success",
            XamlTokenKind.Markup => "Theme.Brush.Status.Warning",
            XamlTokenKind.Comment or XamlTokenKind.Punct => "Theme.Brush.Text.Secondary",
            _ => "Theme.Brush.Text.Primary",
        };
    }
}
