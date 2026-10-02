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
            if (entered == '.')
            {
                // 点号切换到成员补全上下文：词内窗口收起且不提交（点号已照常插入文档），
                // 以点号后的位置为新段起点重新请求
                CloseCompletionWindow();
                _ = ShowCompletionWindowAsync();
            }
            else if ((char.IsLetterOrDigit(entered) || entered == '_') && completionWindow == null)
            {
                // 仅在词首字符时开窗并请求一次；窗口存续期间的后续按键由 CompletionWindow
                // 内建的段追踪 + 子串过滤（IsFiltering，默认开启）接管——逐键关闭重开窗口
                // 会让替换段归零（选中项只插入不替换前缀，"str" 变 "strstring"）且弹窗闪烁
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
            // 窗口存续期间不再重复请求：段内输入由 CompletionWindow 内建过滤接管
            if (CompletionProvider == null || editor == null || completionWindow != null)
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

            // 请求期间文本或光标已变化：
            //  · 仍在同一标识符内续打（快速输入的常态）→ 以最新上下文重发请求
            //    （弹窗尚未打开、无重入；items 匹配短前缀 + CompletionList 按当前段子串过滤，语义仍正确）
            //  · 否则上下文已变（回退、换行、进入字符串等）→ 结果作废
            if (editor.Document.Text != text || editor.CaretOffset != caret)
            {
                if (IsIdentifierContinuation(editor.Document.Text, caret, editor.CaretOffset))
                {
                    _ = ShowCompletionWindowAsync();
                }

                return;
            }

            CloseCompletionWindow();

            var window = new CompletionWindow(editor.TextArea);
            // 窗口在词首字符敲入后才打开（TextEntered 是插入后事件），CompletionWindow 的段锚点
            // 默认从当前光标起算、且文档变更时 StartOffset 用 BeforeInsertion 锚点不会向前扩展——
            // 把段起点回扩到词首，让已输入的整个前缀纳入替换段与过滤文本
            //（否则 Tab 补全只替换第二个字符起的部分："consol" + Tab → "cConsole"）。
            // 点号场景词首即光标（前一个字符是 '.'），不受影响。
            int wordStart = editor.CaretOffset;
            while (wordStart > 0 && (char.IsLetterOrDigit(text[wordStart - 1]) || text[wordStart - 1] == '_'))
            {
                wordStart--;
            }

            window.StartOffset = wordStart;
            IList<ICompletionData> target = window.CompletionList.CompletionData;
            foreach (CodeCompletionItem item in result.Items)
            {
                target.Add(new CompletionDataItem(item));
            }
            window.Closed += OnCompletionWindowClosed;
            completionWindow = window;
            window.Show();
        }

        /// <summary>
        /// 判断从请求时光标到当前光标之间是否仍是同一标识符的连续输入
        /// （词首到当前光标之间只出现标识符字符），用于快速输入时安全重发请求。
        /// </summary>
        private static bool IsIdentifierContinuation(string text, int queryCaret, int currentCaret)
        {
            if (currentCaret < queryCaret)
            {
                return false;
            }

            int start = queryCaret;
            while (start > 0 && (char.IsLetterOrDigit(text[start - 1]) || text[start - 1] == '_'))
            {
                start--;
            }

            for (int i = start; i < currentCaret; i++)
            {
                if (!char.IsLetterOrDigit(text[i]) && text[i] != '_')
                {
                    return false;
                }
            }

            return true;
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
