using Junevy.Controls.CodeCompletion;
using Junevy.Controls.Controls.CodeEditor;

namespace DebugProbe;

/// <summary>
/// Roslyn 伴生包端到端探针：经公共接口 ICodeCompletionProvider 验证
/// RoslynCodeCompletionProvider 的系统类补全（类型名 + 成员 + 插入文本语义）。
/// </summary>
internal static class RoslynCompletionProbe
{
    public static async Task<int> RunAsync()
    {
        int failures = 0;
        void Check(bool ok, string name)
        {
            Console.WriteLine((ok ? "PASS" : "FAIL") + "  " + name);
            if (!ok)
            {
                failures++;
            }
        }

        var provider = new RoslynCodeCompletionProvider();

        const string typeText = "using System;\nusing System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        Conso\n    }\n}";
        CodeCompletionResult? typeResult = await provider.GetCompletionsAsync(
            new CodeCompletionContext(typeText, typeText.IndexOf("Conso") + "Conso".Length),
            CancellationToken.None);
        Check(typeResult != null && typeResult.Items.Count > 0, $"类型名补全返回 {typeResult?.Items.Count ?? 0} 条");
        CodeCompletionItem? console = typeResult?.Items.FirstOrDefault(i => i.DisplayText == "Console");
        Check(console != null, "含系统类 Console");
        Check(console?.InsertText == "Console", "Console 插入文本 = 完整替换词");

        const string memberText = "using System;\n\nclass C\n{\n    void M()\n    {\n        Console.";
        CodeCompletionResult? memberResult = await new RoslynCodeCompletionProvider().GetCompletionsAsync(
            new CodeCompletionContext(memberText, memberText.Length),
            CancellationToken.None);
        Check(memberResult != null && memberResult.Items.Count > 0, $"成员补全返回 {memberResult?.Items.Count ?? 0} 条");
        Check(memberResult?.Items.Any(i => i.DisplayText == "WriteLine") == true, "含成员 WriteLine");

        // 同一 provider 第二次请求（多编辑器/连续输入的真实场景）
        CodeCompletionResult? secondCallResult = await provider.GetCompletionsAsync(
            new CodeCompletionContext(memberText, memberText.Length),
            CancellationToken.None);
        Check(secondCallResult != null && secondCallResult.Items.Count > 0, $"同一 provider 第二次请求返回 {secondCallResult?.Items.Count ?? 0} 条");

        return failures == 0 ? 0 : 1;
    }
}
