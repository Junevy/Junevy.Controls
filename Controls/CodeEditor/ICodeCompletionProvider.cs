using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Junevy.Controls.Controls.CodeEditor
{
    /// <summary>
    /// 补全请求上下文：编辑器全文与光标位置（偏移量与 AvalonEdit 文档坐标一致，0 起算）。
    /// </summary>
    public sealed class CodeCompletionContext
    {
        public CodeCompletionContext(string text, int caretIndex)
        {
            Text = text ?? string.Empty;
            CaretIndex = caretIndex;
        }

        /// <summary>触发补全时的编辑器全文快照。</summary>
        public string Text { get; }

        /// <summary>触发补全时的光标偏移。</summary>
        public int CaretIndex { get; }
    }

    /// <summary>
    /// 单条补全建议（纯数据，与具体补全引擎解耦）。
    /// </summary>
    public sealed class CodeCompletionItem
    {
        public CodeCompletionItem(string displayText, string? insertText = null, string? description = null)
        {
            DisplayText = displayText ?? string.Empty;
            InsertText = string.IsNullOrEmpty(insertText) ? DisplayText : insertText!;
            Description = description;
        }

        /// <summary>弹窗中显示的文本，同时作为输入过滤的目标。</summary>
        public string DisplayText { get; }

        /// <summary>选中后实际插入的文本；与显示文本分离（如 Roslyn 返回的是替换整个单词的完整文本）。</summary>
        public string InsertText { get; }

        /// <summary>说明文字（显示于弹窗下方的描述区）；null 时说明区留空。</summary>
        public string? Description { get; }
    }

    /// <summary>
    /// 一次补全请求的结果。
    /// </summary>
    public sealed class CodeCompletionResult
    {
        public CodeCompletionResult(IReadOnlyList<CodeCompletionItem> items)
        {
            Items = items ?? Array.Empty<CodeCompletionItem>();
        }

        /// <summary>补全建议列表（弹窗内按输入前缀二次过滤）。</summary>
        public IReadOnlyList<CodeCompletionItem> Items { get; }
    }

    /// <summary>
    /// 代码补全提供者——核心库的挂载点抽象（零第三方依赖）：
    /// 由宿主实现并挂到 <c>CodeEditor.CompletionProvider</c>，编辑器负责触发、弹窗、过滤与提交。
    /// <para>
    /// 伴生包 <c>Junevy.Controls.CodeCompletion</c> 基于 Roslyn 提供"系统类 IntelliSense"实现；
    /// 宿主也可以自行实现本接口接入任意引擎（关键词表、LSP 等）。
    /// </para>
    /// </summary>
    public interface ICodeCompletionProvider
    {
        /// <summary>
        /// 异步获取补全建议（由 UI 线程发起，实现应在调用线程以外执行重计算）；
        /// 返回 <see langword="null"/> 表示无建议。实现抛出的异常会被编辑器吞掉并降级为无建议。
        /// </summary>
        Task<CodeCompletionResult?> GetCompletionsAsync(CodeCompletionContext context, CancellationToken cancellationToken);
    }
}
