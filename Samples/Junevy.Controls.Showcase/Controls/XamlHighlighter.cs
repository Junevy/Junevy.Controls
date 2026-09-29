namespace Junevy.Controls.Showcase.Controls
{
    /// <summary>XAML 词法单元类别。</summary>
    public enum XamlTokenKind
    {
        /// <summary>标签之间的文本内容。</summary>
        Plain,

        /// <summary>元素名（jv:Button 等）。</summary>
        Element,

        /// <summary>属性名。</summary>
        Attribute,

        /// <summary>普通字符串属性值。</summary>
        Value,

        /// <summary>标记扩展属性值（{Binding} / {StaticResource} 等）。</summary>
        Markup,

        /// <summary>注释。</summary>
        Comment,

        /// <summary>标点与括号（&lt;、&gt;、=、/）。</summary>
        Punct,
    }

    /// <summary>一个词法单元：文本片段 + 类别。</summary>
    public readonly record struct XamlToken(string Text, XamlTokenKind Kind);

    /// <summary>
    /// 轻量 XAML 词法扫描器：把源码切成带类别的片段，供演示页的代码块着色。
    /// 只做展示级着色，不做完整 XML 解析（演示片段均为受控内容）。
    /// </summary>
    public static class XamlHighlighter
    {
        public static IReadOnlyList<XamlToken> Tokenize(string? xaml)
        {
            var tokens = new List<XamlToken>();
            if (string.IsNullOrEmpty(xaml))
            {
                return tokens;
            }

            var i = 0;
            var inTag = false;
            while (i < xaml.Length)
            {
                var c = xaml[i];

                // 注释：整段吞掉，不区分内部符号。
                if (xaml.AsSpan(i).StartsWith("<!--", StringComparison.Ordinal))
                {
                    var end = xaml.IndexOf("-->", i, StringComparison.Ordinal);
                    end = end < 0 ? xaml.Length : end + 3;
                    tokens.Add(new XamlToken(xaml[i..end], XamlTokenKind.Comment));
                    i = end;
                    continue;
                }

                if (c == '<')
                {
                    inTag = true;
                    if (i + 1 < xaml.Length && xaml[i + 1] == '/')
                    {
                        tokens.Add(new XamlToken("</", XamlTokenKind.Punct));
                        i += 2;
                    }
                    else
                    {
                        tokens.Add(new XamlToken("<", XamlTokenKind.Punct));
                        i++;
                    }

                    // 元素名：字母/数字/下划线/冒号/点。
                    var j = i;
                    while (j < xaml.Length && (char.IsLetterOrDigit(xaml[j]) || xaml[j] is '_' or ':' or '.'))
                    {
                        j++;
                    }

                    if (j > i)
                    {
                        tokens.Add(new XamlToken(xaml[i..j], XamlTokenKind.Element));
                        i = j;
                    }

                    continue;
                }

                if (c == '>')
                {
                    inTag = false;
                    tokens.Add(new XamlToken(">", XamlTokenKind.Punct));
                    i++;
                    continue;
                }

                if (c == '"')
                {
                    var end = xaml.IndexOf('"', i + 1);
                    end = end < 0 ? xaml.Length : end + 1;
                    var text = xaml[i..end];
                    var kind = text.Length > 1 && text[1] == '{' ? XamlTokenKind.Markup : XamlTokenKind.Value;
                    tokens.Add(new XamlToken(text, kind));
                    i = end;
                    continue;
                }

                if (c == '=')
                {
                    tokens.Add(new XamlToken("=", XamlTokenKind.Punct));
                    i++;
                    continue;
                }

                if (c == '/' && inTag)
                {
                    tokens.Add(new XamlToken("/", XamlTokenKind.Punct));
                    i++;
                    continue;
                }

                // 一段连续的普通字符：标签内视为属性名，标签间视为文本内容。
                var k = i;
                while (k < xaml.Length)
                {
                    var ch = xaml[k];
                    if (ch is '<' or '>' or '"' or '=')
                    {
                        break;
                    }

                    if (inTag && ch == '/')
                    {
                        break;
                    }

                    k++;
                }

                if (k > i)
                {
                    tokens.Add(new XamlToken(xaml[i..k], inTag ? XamlTokenKind.Attribute : XamlTokenKind.Plain));
                    i = k;
                }
                else
                {
                    i++;
                }
            }

            return tokens;
        }
    }
}
