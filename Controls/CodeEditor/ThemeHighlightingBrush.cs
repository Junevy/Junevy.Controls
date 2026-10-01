using System.Windows.Media;

using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace Junevy.Controls.Controls.CodeEditor
{
    /// <summary>
    /// 主题化高亮画刷：不持有固定颜色，渲染期从当前 TextView 起向上查找主题资源键。
    /// <para>
    /// ThemeManager 切换主题时替换的是顶层合并字典，资源查找走视觉树即时命中新画刷，
    /// 因此语法着色随深浅主题切换即时生效，无需控件重设颜色。
    /// </para>
    /// </summary>
    internal sealed class ThemeHighlightingBrush : HighlightingBrush
    {
        private readonly string resourceKey;

        public ThemeHighlightingBrush(string resourceKey)
        {
            this.resourceKey = resourceKey;
        }

        /// <summary>
        /// 渲染期解析主题画刷；主题未应用（资源缺失）时返回 <see langword="null"/>，
        /// AvalonEdit 视为「未着色」并回落到默认前景色，属预期降级行为。
        /// </summary>
        public override Brush GetBrush(ITextRunConstructionContext context)
        {
            Brush? brush = context?.TextView?.TryFindResource(resourceKey) as Brush;
            return brush!;
        }

        public override string ToString()
        {
            return resourceKey;
        }
    }
}
