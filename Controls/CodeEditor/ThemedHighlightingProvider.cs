using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Junevy.Controls.Controls.CodeEditor
{
    /// <summary>
    /// 主题化语法高亮提供者：从 AvalonEdit 程序集内嵌的 XSHD 资源加载<strong>私有副本</strong>，
    /// 再把副本里的命名颜色逐个替换为跟随主题的画刷。
    /// <para>
    /// 不直接改用 <see cref="HighlightingManager.Instance"/> 的共享定义——那是全程序单例，
    /// 改它的颜色会波及宿主应用里其他 AvalonEdit 编辑器；私有副本按「语言 → 注册名」缓存，
    /// 副本内颜色经 <see cref="ThemeHighlightingBrush"/> 资源引用式取色，定义实例本身可跨编辑器安全共享。
    /// </para>
    /// </summary>
    internal static class ThemedHighlightingProvider
    {
        /// <summary>AvalonEdit 内嵌 XSHD 的资源名前缀（与其 Highlighting.Resources.Resources.OpenStream 的拼接规则一致）。</summary>
        private const string ResourcePrefix = "ICSharpCode.AvalonEdit.Highlighting.Resources.";

        /// <summary>
        /// 内置高亮注册名 → XSHD 资源名。完整收录（而非只列 <see cref="CodeLanguage"/> 子集），
        /// 因为 XSHD 的 import 以注册名引用（如 C# 规则里的 <c>XmlDoc/DocComment</c>），须能递归解析全量内置定义。
        /// </summary>
        private static readonly Dictionary<string, string> BuiltInResources = new(StringComparer.Ordinal)
        {
            ["XmlDoc"] = "XmlDoc.xshd",
            ["C#"] = "CSharp-Mode.xshd",
            ["JavaScript"] = "JavaScript-Mode.xshd",
            ["HTML"] = "HTML-Mode.xshd",
            ["ASP/XHTML"] = "ASPX.xshd",
            ["Boo"] = "Boo.xshd",
            ["Coco"] = "Coco-Mode.xshd",
            ["CSS"] = "CSS-Mode.xshd",
            ["C++"] = "CPP-Mode.xshd",
            ["Java"] = "Java-Mode.xshd",
            ["Patch"] = "Patch-Mode.xshd",
            ["PowerShell"] = "PowerShell.xshd",
            ["PHP"] = "PHP-Mode.xshd",
            ["Python"] = "Python-Mode.xshd",
            ["TeX"] = "Tex-Mode.xshd",
            ["TSQL"] = "TSQL-Mode.xshd",
            ["VB"] = "VB-Mode.xshd",
            ["XML"] = "XML-Mode.xshd",
            ["MarkDown"] = "MarkDown-Mode.xshd",
            ["MarkDownWithFontSize"] = "MarkDownWithFontSize-Mode.xshd",
            ["Json"] = "Json.xshd",
        };

        /// <summary>支持的语言 → 内置注册名。</summary>
        private static readonly Dictionary<CodeLanguage, string> LanguageNames = new()
        {
            [CodeLanguage.CSharp] = "C#",
            [CodeLanguage.VisualBasic] = "VB",
            [CodeLanguage.Cpp] = "C++",
            [CodeLanguage.Java] = "Java",
            [CodeLanguage.JavaScript] = "JavaScript",
            [CodeLanguage.Html] = "HTML",
            [CodeLanguage.Css] = "CSS",
            [CodeLanguage.Xml] = "XML",
            [CodeLanguage.Json] = "Json",
            [CodeLanguage.Sql] = "TSQL",
            [CodeLanguage.Python] = "Python",
            [CodeLanguage.Markdown] = "MarkDown",
            [CodeLanguage.PowerShell] = "PowerShell",
        };

        /// <summary>已主题化的私有定义缓存（键为注册名）。</summary>
        private static readonly Dictionary<string, IHighlightingDefinition> definitionCache = new(StringComparer.Ordinal);

        /// <summary>加载中标记：XSHD import 递归解析时防环；环内退回共享定义，宁可不着色也不栈溢出。</summary>
        private static readonly HashSet<string> loadingNames = new(StringComparer.Ordinal);

        /// <summary>
        /// 取指定语言的语法高亮定义（私有副本、颜色已映射主题画刷）；
        /// <see cref="CodeLanguage.None"/> 返回 <see langword="null"/>（不启用高亮）。
        /// </summary>
        public static IHighlightingDefinition? GetDefinition(CodeLanguage language)
        {
            if (language == CodeLanguage.None || !LanguageNames.TryGetValue(language, out string? name))
            {
                return null;
            }

            return GetByName(name);
        }

        /// <summary>按注册名取私有定义，未收录的注册名返回 <see langword="null"/>。</summary>
        private static IHighlightingDefinition? GetByName(string name)
        {
            if (definitionCache.TryGetValue(name, out IHighlightingDefinition? cached))
            {
                return cached;
            }

            if (!BuiltInResources.ContainsKey(name) || loadingNames.Contains(name))
            {
                return null;
            }

            loadingNames.Add(name);
            try
            {
                IHighlightingDefinition definition = ApplyThemeColors(Load(name));
                definitionCache[name] = definition;
                return definition;
            }
            finally
            {
                loadingNames.Remove(name);
            }
        }

        private static IHighlightingDefinition Load(string name)
        {
            string resourceName = BuiltInResources[name];
            Assembly avalonEditAssembly = typeof(HighlightingManager).Assembly;
            using Stream stream = avalonEditAssembly.GetManifestResourceStream(ResourcePrefix + resourceName)
                ?? throw new InvalidOperationException($"AvalonEdit 内置高亮资源缺失：{resourceName}");
            using XmlReader reader = XmlReader.Create(stream);

            // 解析器传本类自身的延迟解析器：import（如 C# 引用 XmlDoc/DocCommentSet）
            // 同样解析到主题化私有副本，保证注释里的文档标记颜色与主题一致
            return HighlightingLoader.Load(HighlightingLoader.LoadXshd(reader), new BuiltInResolver());
        }

        /// <summary>把定义里全部命名颜色替换为主题画刷；未归类颜色保持 null（渲染为默认前景色）。</summary>
        private static IHighlightingDefinition ApplyThemeColors(IHighlightingDefinition definition)
        {
            foreach (HighlightingColor color in definition.NamedHighlightingColors)
            {
                CodeTokenCategory? category = Categorize(color.Name);
                color.Foreground = category == null ? null : new ThemeHighlightingBrush(GetBrushKey(category.Value));
            }

            return definition;
        }

        private static string GetBrushKey(CodeTokenCategory category)
        {
            return category switch
            {
                CodeTokenCategory.Keyword => "Theme.Brush.Accent.Primary",
                CodeTokenCategory.Type => "Theme.Brush.Accent.Secondary",
                CodeTokenCategory.String => "Theme.Brush.Status.Success",
                CodeTokenCategory.Number => "Theme.Brush.Status.Danger",
                CodeTokenCategory.Comment => "Theme.Brush.Text.Tertiary",
                CodeTokenCategory.Preprocessor => "Theme.Brush.Text.Secondary",
                _ => "Theme.Brush.Text.Primary",
            };
        }

        /// <summary>
        /// 命名颜色 → 语义类别。先查精确表（覆盖受支持语言 XSHD 里的全部命名颜色），
        /// 再按名称特征兜底（保险未收录的新增语言）；两者都不中则不着色。
        /// </summary>
        private static CodeTokenCategory? Categorize(string? name)
        {
            if (name == null || name.Length == 0)
            {
                return null;
            }

            if (ExactCategories.TryGetValue(name, out CodeTokenCategory category))
            {
                return category;
            }

            if (Contains(name, "Comment") || Contains(name, "Doc"))
            {
                return CodeTokenCategory.Comment;
            }

            if (Contains(name, "String") || Contains(name, "Character"))
            {
                return CodeTokenCategory.String;
            }

            if (Contains(name, "Number") || Contains(name, "Digits"))
            {
                return CodeTokenCategory.Number;
            }

            if (Contains(name, "Preprocessor"))
            {
                return CodeTokenCategory.Preprocessor;
            }

            if (Contains(name, "Keyword") || Contains(name, "Tag"))
            {
                return CodeTokenCategory.Keyword;
            }

            if (Contains(name, "Type"))
            {
                return CodeTokenCategory.Type;
            }

            return null;
        }

        /// <summary>net48 没有 string.Contains(string, StringComparison) 重载，统一走 IndexOf。</summary>
        private static bool Contains(string name, string value)
        {
            return name.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// 命名颜色的精确归类。映射口径：
        /// 关键字/语句/标签名 → 主色（Cobalt）；类型词与属性名/选择器 → 次色（Coating）；
        /// 字符串与字符 → 成功绿；数字 → 危险红；注释 → 弱化灰；预处理指令 → 次级灰。
        /// </summary>
        private static readonly Dictionary<string, CodeTokenCategory> ExactCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            // 注释与文档注释（含 /// 的 XML 文档标记）
            ["Comment"] = CodeTokenCategory.Comment,
            ["CommentTags"] = CodeTokenCategory.Comment,
            ["DocComment"] = CodeTokenCategory.Comment,
            ["XmlDoc/DocComment"] = CodeTokenCategory.Comment,
            ["JavaDocTags"] = CodeTokenCategory.Comment,
            ["KnownDocTags"] = CodeTokenCategory.Comment,
            ["BlockQuote"] = CodeTokenCategory.Comment,

            // 字符串 / 字符 / 字面引文
            ["String"] = CodeTokenCategory.String,
            ["StringInterpolation"] = CodeTokenCategory.String,
            ["XmlString"] = CodeTokenCategory.String,
            ["AttributeValue"] = CodeTokenCategory.String,
            ["Char"] = CodeTokenCategory.String,
            ["Character"] = CodeTokenCategory.String,
            ["DateLiteral"] = CodeTokenCategory.String,
            ["CData"] = CodeTokenCategory.String,
            ["Regex"] = CodeTokenCategory.String,
            ["Entities"] = CodeTokenCategory.String,
            ["Entity"] = CodeTokenCategory.String,
            ["EntityReference"] = CodeTokenCategory.String,
            ["BrokenEntity"] = CodeTokenCategory.String,
            ["Code"] = CodeTokenCategory.String,

            // 数字
            ["Number"] = CodeTokenCategory.Number,
            ["NumberLiteral"] = CodeTokenCategory.Number,
            ["Digits"] = CodeTokenCategory.Number,

            // 预处理指令
            ["Preprocessor"] = CodeTokenCategory.Preprocessor,

            // 类型词 / 属性名 / 选择器（次色，与关键字区分）
            ["TypeKeywords"] = CodeTokenCategory.Keyword,
            ["ValueTypeKeywords"] = CodeTokenCategory.Keyword,
            ["ReferenceTypeKeywords"] = CodeTokenCategory.Keyword,
            ["ValueTypes"] = CodeTokenCategory.Keyword,
            ["ReferenceTypes"] = CodeTokenCategory.Keyword,
            ["DataTypes"] = CodeTokenCategory.Keyword,
            ["AttributeName"] = CodeTokenCategory.Type,
            ["Selector"] = CodeTokenCategory.Type,
            ["Attributes"] = CodeTokenCategory.Type,
            ["Class"] = CodeTokenCategory.Type,
            ["Link"] = CodeTokenCategory.Type,
            ["Image"] = CodeTokenCategory.Type,

            // 关键字 / 语句 / 标签 / 文档类型声明 / 标题
            ["Keywords"] = CodeTokenCategory.Keyword,
            ["AccessKeywords"] = CodeTokenCategory.Keyword,
            ["AccessModifiers"] = CodeTokenCategory.Keyword,
            ["CheckedKeyword"] = CodeTokenCategory.Keyword,
            ["CompoundKeywords"] = CodeTokenCategory.Keyword,
            ["ContextKeywords"] = CodeTokenCategory.Keyword,
            ["ControlFlow"] = CodeTokenCategory.Keyword,
            ["ExceptionHandling"] = CodeTokenCategory.Keyword,
            ["ExceptionHandlingStatements"] = CodeTokenCategory.Keyword,
            ["ExceptionKeywords"] = CodeTokenCategory.Keyword,
            ["FunctionKeywords"] = CodeTokenCategory.Keyword,
            ["GetSetAddRemove"] = CodeTokenCategory.Keyword,
            ["IterationStatements"] = CodeTokenCategory.Keyword,
            ["JavaScriptKeyWords"] = CodeTokenCategory.Keyword,
            ["JavaScriptGlobalFunctions"] = CodeTokenCategory.Keyword,
            ["JavaScriptIntrinsics"] = CodeTokenCategory.Keyword,
            ["JavaScriptLiterals"] = CodeTokenCategory.Keyword,
            ["JumpKeywords"] = CodeTokenCategory.Keyword,
            ["JumpStatements"] = CodeTokenCategory.Keyword,
            ["LoopKeywords"] = CodeTokenCategory.Keyword,
            ["Modifiers"] = CodeTokenCategory.Keyword,
            ["NamespaceKeywords"] = CodeTokenCategory.Keyword,
            ["NullOrValueKeywords"] = CodeTokenCategory.Keyword,
            ["OperatorKeywords"] = CodeTokenCategory.Keyword,
            ["Package"] = CodeTokenCategory.Keyword,
            ["ParameterModifiers"] = CodeTokenCategory.Keyword,
            ["SelectionStatements"] = CodeTokenCategory.Keyword,
            ["SemanticKeywords"] = CodeTokenCategory.Keyword,
            ["ThisOrBaseReference"] = CodeTokenCategory.Keyword,
            ["TrueFalse"] = CodeTokenCategory.Keyword,
            ["UnsafeKeywords"] = CodeTokenCategory.Keyword,
            ["Visibility"] = CodeTokenCategory.Keyword,
            ["Bool"] = CodeTokenCategory.Keyword,
            ["BooleanConstants"] = CodeTokenCategory.Keyword,
            ["Constants"] = CodeTokenCategory.Keyword,
            ["Null"] = CodeTokenCategory.Keyword,
            ["This"] = CodeTokenCategory.Keyword,
            ["Void"] = CodeTokenCategory.Keyword,
            ["Friend"] = CodeTokenCategory.Keyword,
            ["Command"] = CodeTokenCategory.Keyword,
            ["Heading"] = CodeTokenCategory.Keyword,
            ["Tags"] = CodeTokenCategory.Keyword,
            ["HtmlTag"] = CodeTokenCategory.Keyword,
            ["XmlTag"] = CodeTokenCategory.Keyword,
            ["ScriptTag"] = CodeTokenCategory.Keyword,
            ["JScriptTag"] = CodeTokenCategory.Keyword,
            ["JavaScriptTag"] = CodeTokenCategory.Keyword,
            ["VBScriptTag"] = CodeTokenCategory.Keyword,
            ["UnknownScriptTag"] = CodeTokenCategory.Keyword,
            ["XmlDeclaration"] = CodeTokenCategory.Keyword,
            ["DocType"] = CodeTokenCategory.Keyword,
        };

        /// <summary>
        /// XSHD 引用解析器：import 与「定义名/颜色名」引用（如 C# 的 <c>XmlDoc/DocComment</c>）
        /// 解析到主题化私有副本；未收录的注册名退回共享定义（只读使用，不会被修改）。
        /// </summary>
        private sealed class BuiltInResolver : IHighlightingDefinitionReferenceResolver
        {
            public IHighlightingDefinition? GetDefinition(string name)
            {
                return GetByName(name) ?? HighlightingManager.Instance.GetDefinition(name);
            }
        }

        private enum CodeTokenCategory
        {
            Keyword,
            Type,
            String,
            Number,
            Comment,
            Preprocessor,
        }
    }
}
