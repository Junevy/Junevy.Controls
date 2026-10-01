using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Junevy.Controls.Controls.CodeEditor;

namespace Junevy.Controls.Showcase
{
    /// <summary>
    /// 演示用关键词补全提供者：仅依赖核心库的 ICodeCompletionProvider 抽象（零 Roslyn 依赖），
    /// 挂到 CodeEditor.CompletionProvider 后输入标识符字符 / 点号 / Ctrl+Space 即弹出；
    /// 完整的系统类 IntelliSense 见伴生包 Junevy.Controls.CodeCompletion。
    /// </summary>
    internal sealed class SimpleKeywordCompletionProvider : ICodeCompletionProvider
    {
        private static readonly string[] Keywords =
        {
            "class", "public", "private", "protected", "internal", "static", "readonly", "const",
            "void", "int", "string", "double", "bool", "object", "var", "new", "return", "if",
            "else", "switch", "case", "foreach", "for", "while", "do", "using", "namespace",
            "override", "virtual", "abstract", "async", "await", "this", "base", "null", "true",
            "false", "try", "catch", "finally", "throw", "break", "continue", "sealed", "enum",
        };

        public Task<CodeCompletionResult?> GetCompletionsAsync(CodeCompletionContext context, CancellationToken cancellationToken)
        {
            string prefix = GetWordBeforeCaret(context);
            if (prefix.Length == 0)
            {
                return Task.FromResult<CodeCompletionResult?>(null);
            }

            List<CodeCompletionItem> items = Keywords
                .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(k => new CodeCompletionItem(k, k, "C# 关键词"))
                .ToList();

            return Task.FromResult<CodeCompletionResult?>(items.Count == 0 ? null : new CodeCompletionResult(items));
        }

        private static string GetWordBeforeCaret(CodeCompletionContext context)
        {
            string text = context.Text;
            int start = Math.Min(context.CaretIndex, text.Length);
            while (start > 0 && (char.IsLetterOrDigit(text[start - 1]) || text[start - 1] == '_'))
            {
                start--;
            }

            return text[start..context.CaretIndex];
        }
    }
}
