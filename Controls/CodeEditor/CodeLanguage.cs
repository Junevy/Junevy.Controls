namespace Junevy.Controls.Controls.CodeEditor
{
    /// <summary>
    /// 代码编辑器支持的语法高亮语言。
    /// <para>
    /// 高亮规则取自 AvalonEdit 内置 XSHD 定义的私有副本（不改动全局共享定义），
    /// 着色统一映射到 <c>Theme.Brush.*</c> 主题画刷，深浅主题切换即时生效。
    /// </para>
    /// </summary>
    public enum CodeLanguage
    {
        /// <summary>纯文本，不启用语法高亮。</summary>
        None = 0,

        /// <summary>C#。</summary>
        CSharp,

        /// <summary>Visual Basic。</summary>
        VisualBasic,

        /// <summary>C/C++。</summary>
        Cpp,

        /// <summary>Java。</summary>
        Java,

        /// <summary>JavaScript。</summary>
        JavaScript,

        /// <summary>HTML。</summary>
        Html,

        /// <summary>CSS。</summary>
        Css,

        /// <summary>XML / XAML / 配置文件。</summary>
        Xml,

        /// <summary>JSON。</summary>
        Json,

        /// <summary>T-SQL。</summary>
        Sql,

        /// <summary>Python。</summary>
        Python,

        /// <summary>Markdown。</summary>
        Markdown,

        /// <summary>PowerShell。</summary>
        PowerShell,
    }
}
