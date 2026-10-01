using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Junevy.Controls.Controls.CodeEditor
{
    /// <summary>
    /// CodeEditor 的补全管线（部分类）：把 <see cref="ICodeCompletionProvider"/> 的建议
    /// 接到 AvalonEdit 内置 <see cref="CompletionWindow"/> 弹窗上——触发、过期判断、过滤与提交
    /// 全部走 AvalonEdit 内建机制，本类不关心补全引擎。
    /// <para>
    /// 弹窗外观由 CodeEditor.xaml 中重写的 CompletionList 模板与条目样式主题化；
    /// Roslyn（系统类 IntelliSense）实现见伴生包 Junevy.Controls.CodeCompletion。
    /// </para>
    /// </summary>
    public partial class CodeEditor
    {
        /// <summary>
        /// 标识 <see cref="CompletionProvider"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty CompletionProviderProperty =
            DependencyProperty.Register(
                nameof(CompletionProvider),
                typeof(ICodeCompletionProvider),
                typeof(CodeEditor),
                new PropertyMetadata(null, OnCompletionProviderChanged));

        /// <summary>
        /// 补全提供者：非 <see langword="null"/> 时启用补全（输入标识符字符 / 点号 / Ctrl+Space 触发），
        /// <see langword="null"/> 时关闭。默认 <see langword="null"/>（零行为变化）。
        /// </summary>
        public ICodeCompletionProvider? CompletionProvider
        {
            get { return (ICodeCompletionProvider?)GetValue(CompletionProviderProperty); }
            set { SetValue(CompletionProviderProperty, value); }
        }

        private CompletionWindow? completionWindow;
        private CancellationTokenSource? completionCts;

        private static void OnCompletionProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 提供者被移除/替换时收起旧弹窗，避免旧引擎的建议残留在新引擎上
            ((CodeEditor)d).CloseCompletionWindow();
        }

        /// <summary>挂接编辑区事件（由 <see cref="OnApplyTemplate"/> 在模板部件就位后调用）。</summary>
        private void AttachCompletionPipeline(TextArea textArea)
        {
            textArea.TextEntered += CompletionTextEntered;
            textArea.TextEntering += CompletionTextEntering;
            textArea.KeyDown += CompletionKeyDown;
        }

        /// <summary>解除事件挂接并收起弹窗（模板重应用或控件释放时调用）。</summary>
        private void DetachCompletionPipeline(TextArea textArea)
        {
            textArea.TextEntered -= CompletionTextEntered;
            textArea.TextEntering -= CompletionTextEntering;
            textArea.KeyDown -= CompletionKeyDown;
            CloseCompletionWindow();
        }

        private void CompletionKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space
                && Keyboard.Modifiers == ModifierKeys.Control
                && CompletionProvider != null)
            {
                e.Handled = true;
                _ = ShowCompletionWindowAsync();
            }
        }

        private void CompletionTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (CompletionProvider == null || e.Text.Length == 0)
            {
                return;
            }

            char entered = e.Text[0];
            if (char.IsLetterOrDigit(entered) || entered == '_' || entered == '.')
            {
                _ = ShowCompletionWindowAsync();
            }
        }

        private void CompletionTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (completionWindow == null || e.Text.Length == 0)
            {
                return;
            }

            char entering = e.Text[0];
            if (!char.IsLetterOrDigit(entering) && entering != '_')
            {
                // 输入非标识符字符（空格、括号、运算符…）时直接收起且不提交——
                // 提交手势（Enter/Tab/点击）由 CompletionList 自行处理；点号收起后
                // 由 TextEntered 重新请求成员补全
                CloseCompletionWindow();
            }
        }

        private async Task ShowCompletionWindowAsync()
        {
            if (CompletionProvider == null || editor == null)
            {
                return;
            }

            completionCts?.Cancel();
            var cts = new CancellationTokenSource();
            completionCts = cts;
            CancellationToken token = cts.Token;

            string text = editor.Document.Text;
            int caret = editor.CaretOffset;

            CodeCompletionResult? result;
            try
            {
                result = await CompletionProvider.GetCompletionsAsync(new CodeCompletionContext(text, caret), token).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception)
            {
                // 补全是辅助能力：提供者内部的异常不应打断宿主输入流，降级为无建议
                return;
            }

            if (token.IsCancellationRequested
                || editor == null
                || result == null
                || result.Items.Count == 0)
            {
                return;
            }

            // 请求期间文本或光标已变化 → 结果过期
            if (editor.Document.Text != text || editor.CaretOffset != caret)
            {
                return;
            }

            CloseCompletionWindow();

            var window = new CompletionWindow(editor.TextArea);
            IList<ICompletionData> target = window.CompletionList.CompletionData;
            foreach (CodeCompletionItem item in result.Items)
            {
                target.Add(new CompletionDataItem(item));
            }
            window.Closed += OnCompletionWindowClosed;
            completionWindow = window;
            window.Show();
        }

        private void OnCompletionWindowClosed(object? sender, EventArgs e)
        {
            if (sender is CompletionWindow window && completionWindow == window)
            {
                window.Closed -= OnCompletionWindowClosed;
                completionWindow = null;
            }
        }

        private void CloseCompletionWindow()
        {
            if (completionWindow != null)
            {
                completionWindow.Close();
                completionWindow = null;
            }
        }

        /// <summary>
        /// 补全条目适配器：把引擎无关的 <see cref="CodeCompletionItem"/> 接到 AvalonEdit 弹窗
        /// （默认插入语义 = 用 <see cref="CodeCompletionItem.InsertText"/> 替换光标前的输入前缀段）。
        /// </summary>
        private sealed class CompletionDataItem : ICompletionData
        {
            private readonly CodeCompletionItem item;

            public CompletionDataItem(CodeCompletionItem item)
            {
                this.item = item;
            }

            public ImageSource Image => null!;

            public string Text => item.DisplayText;

            public object Content => item.DisplayText;

            public object Description => item.Description ?? string.Empty;

            public double Priority => 0;

            public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, item.InsertText);
            }
        }
    }
}
