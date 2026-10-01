using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

using Junevy.Controls.Controls.CodeEditor;

namespace Junevy.Controls.CodeCompletion
{
    /// <summary>
    /// 基于 Roslyn 的代码补全提供者：接入 <see cref="CompletionService"/> 官方完成引擎，
    /// 为 <c>jv:CodeEditor</c> 提供系统类 IntelliSense（类型名、成员、关键词等）。
    /// <para>
    /// 程序集引用来源：.NET 运行时目录中的基础类库（零额外 NuGet 包）；宿主自有程序集或
    /// 第三方库经 <see cref="AdditionalReferences"/> 追加（须在首次补全前加入）。补全引擎的
    /// provider 位于 Microsoft.CodeAnalysis.Features / CSharp.Features 程序集，须全部喂给 MEF
    /// 宿主——漏装任何一个都会静默返回空结果；单文件发布等重排部署形态无法自动发现这些文件，不受支持。
    /// </para>
    /// <para>
    /// 每个实例持有独立的 <see cref="AdhocWorkspace"/>（Roslyn 的编译缓存按工作区生效）；
    /// 实例不承诺线程安全，请在创建它的 UI 线程上使用。
    /// </para>
    /// </summary>
    public sealed class RoslynCodeCompletionProvider : ICodeCompletionProvider
    {
        private const string MefAssemblySearchPattern = "Microsoft.CodeAnalysis*.dll";

        private readonly Lazy<AdhocWorkspace> workspace;
        private readonly Lazy<Document> document;
        private readonly List<MetadataReference> additionalReferences = new();

        /// <summary>
        /// 追加的元数据引用（宿主自有程序集、第三方库、独立的 BCL 引用包等）；
        /// 须在首次补全前加入（工作区按需创建并快照此列表）。
        /// </summary>
        public IList<MetadataReference> AdditionalReferences => additionalReferences;

        public RoslynCodeCompletionProvider()
        {
            workspace = new Lazy<AdhocWorkspace>(() => new AdhocWorkspace(MefHostServices.Create(LoadMefAssemblies())));
            document = new Lazy<Document>(() =>
            {
                Project project = workspace.Value.AddProject(CreateProjectInfo());
                return workspace.Value.AddDocument(project.Id, "code.cs", SourceText.From(string.Empty));
            });
        }

        /// <inheritdoc/>
        public async Task<CodeCompletionResult?> GetCompletionsAsync(CodeCompletionContext context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(context.Text) || context.CaretIndex < 0 || context.CaretIndex > context.Text.Length)
            {
                return null;
            }

            Document document = this.document.Value;
            // 不把文本提交回工作区（避免 fork/应用的状态机问题）：
            // 每次从基准文档 fork 出带全文的文档直接查询，补全服务按传入文档分析
            Document forked = document.WithText(SourceText.From(context.Text));
            CompletionService service = CompletionService.GetService(forked)!;
            CompletionList? list = await service
                .GetCompletionsAsync(forked, context.CaretIndex, CompletionTrigger.Invoke, null, null, cancellationToken)
                .ConfigureAwait(false);
            if (list == null || list.ItemsList.Count == 0)
            {
                return null;
            }

            var items = new List<CodeCompletionItem>(list.ItemsList.Count);
            foreach (CompletionItem roslynItem in list.ItemsList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                items.Add(new CodeCompletionItem(
                    roslynItem.DisplayText,
                    await GetInsertTextAsync(service, document, roslynItem, roslynItem.DisplayText, cancellationToken).ConfigureAwait(false),
                    roslynItem.InlineDescription));
            }

            return new CodeCompletionResult(items);
        }

        /// <summary>
        /// 取选中后的插入文本：Roslyn 给出的是"替换光标前单词段"的完整文本，
        /// 与 AvalonEdit CompletionWindow 的前缀段替换语义一致；个别条目取不到时回落显示文本。
        /// </summary>
        private static async Task<string> GetInsertTextAsync(
            CompletionService service,
            Document document,
            CompletionItem item,
            string fallback,
            CancellationToken cancellationToken)
        {
            try
            {
                CompletionChange change = await service.GetChangeAsync(document, item, null, cancellationToken).ConfigureAwait(false);
                string newText = change.TextChange.NewText;
                return string.IsNullOrEmpty(newText) ? fallback : newText;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return fallback;
            }
        }

        private ProjectInfo CreateProjectInfo()
        {
            var references = new List<MetadataReference>(additionalReferences);
            references.AddRange(EnumerateRuntimeReferences());

            return ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                "JunevyCodeCompletion",
                "JunevyCodeCompletion",
                LanguageNames.CSharp,
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                metadataReferences: references);
        }

        /// <summary>
        /// MEF 宿主程序集：部署目录里全部 Roslyn 程序集——
        /// Features（补全 provider）与 Workspaces（核心服务）的导出缺一不可。
        /// </summary>
        private static IEnumerable<Assembly> LoadMefAssemblies()
        {
            string[] files = Directory.GetFiles(AppContext.BaseDirectory, MefAssemblySearchPattern);
            if (files.Length == 0)
            {
                throw new InvalidOperationException(
                    $"部署目录中未找到 Roslyn 程序集（{MefAssemblySearchPattern}）。"
                    + "补全引擎的 MEF 装配依赖这些程序集文件；单文件发布等重排形态不受支持，请使用常规部署。");
            }

            return files.Select(Assembly.LoadFrom).Distinct().ToArray();
        }

        /// <summary>
        /// 枚举 .NET 运行时目录中的基础类库引用：仅取 System./Microsoft./mscorlib/netstandard 托管程序集，
        /// 跳过资源卫星文件与 Roslyn 自身；非托管 DLL（BadImageFormatException）逐个忽略。
        /// </summary>
        private static IEnumerable<MetadataReference> EnumerateRuntimeReferences()
        {
            string runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
            foreach (string file in Directory.GetFiles(runtimeDirectory, "*.dll"))
            {
                string name = Path.GetFileName(file);
                if (name.EndsWith(".resources.dll", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("Microsoft.CodeAnalysis", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                bool isBaseLibrary =
                    name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("netstandard.dll", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase);
                if (!isBaseLibrary)
                {
                    continue;
                }

                MetadataReference? reference = null;
                try
                {
                    reference = MetadataReference.CreateFromFile(file);
                }
                catch (Exception ex) when (ex is BadImageFormatException or FileNotFoundException)
                {
                    // 非托管或缺失符号的文件：跳过即可
                }

                if (reference != null)
                {
                    yield return reference;
                }
            }
        }
    }
}
