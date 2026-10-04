using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Junevy.Controls.Controls.CodeEditor;
using Junevy.Controls.Themes;

namespace DebugProbe;

/// <summary>
/// CodeEditor 冒烟探针（无窗口、离屏渲染）：
///   1. 模板应用与 PART_Editor 部件；
///   2. Text 依赖属性与内部文档的双向同步；
///   3. 全部语言的语法高亮定义加载与主题画刷映射；
///   4. 明暗两主题离屏渲染 PNG + 像素断言（表面画刷确实解析到主题资源）。
///   注意：切主题后重建实例再渲染——离屏元素树不接收 Application 级资源变更通知
///   （真实应用中元素挂在窗口树上，ThemeManager 顶层字典替换对全部控件即时生效）。
/// </summary>
internal static class CodeEditorProbe
{
    private const double HostWidth = 680;
    private const double HostHeight = 420;

    private const string SampleCode =
        """
        using System;
        using System.Collections.Generic;

        namespace Demo
        {
            /// <summary>
            /// 示例代码：验证注释、字符串、数字着色。
            /// </summary>
            public class Sample
            {
                private const double Pi = 3.14159;
                private readonly List<string> items = new();
                private static int counter = 42;

                // 普通注释：计算圆面积
                public double Area(double radius)
                {
                    if (radius <= 0) throw new ArgumentException("半径必须为正数", nameof(radius));
                    return Pi * radius * radius;
                }
            }
        }
        """;

    public static int Run()
    {
        var app = Application.Current ?? new Application();
        ThemeManager.ApplyTheme(AppTheme.Light);

        int failures = 0;
        void Check(bool ok, string name)
        {
            Console.WriteLine((ok ? "PASS" : "FAIL") + "  " + name);
            if (!ok)
            {
                failures++;
            }
        }

        // —— 行为断言（浅色主题实例）——
        CodeEditor editor = BuildEditor();
        Check(editor.InnerEditor != null, "模板应用：PART_Editor 就位");
        Check(editor.InnerEditor!.Text == SampleCode, "Text 依赖属性 → 内部文档同步");

        editor.InnerEditor.AppendText("\n// 由内部编辑区追加");
        Check(editor.Text.EndsWith("// 由内部编辑区追加", StringComparison.Ordinal), "内部编辑 → Text 依赖属性同步");

        Check(editor.InnerEditor.SyntaxHighlighting != null, "C# 高亮定义已加载");
        var highlighting = editor.InnerEditor.SyntaxHighlighting!;
        Check(highlighting.GetNamedColor("Keywords")?.Foreground?.ToString() == "Theme.Brush.Accent.Primary", "关键字 → Accent.Primary");
        Check(highlighting.GetNamedColor("Comment")?.Foreground?.ToString() == "Theme.Brush.Text.Tertiary", "注释 → Text.Tertiary");
        Check(highlighting.GetNamedColor("String")?.Foreground?.ToString() == "Theme.Brush.Status.Success", "字符串 → Status.Success");
        Check(highlighting.GetNamedColor("NumberLiteral")?.Foreground?.ToString() == "Theme.Brush.Status.Danger", "数字 → Status.Danger");
        // 文档注释颜色（XmlDoc/DocComment）在定义构建期经自定义解析器解析为私有 XmlDoc 副本的主题化颜色对象：
        // GetNamedColor 只查本定义字典（AvalonEdit 行为），故限定名不可直接查询；解析失败会在加载时抛异常，
        // 上面「C# 高亮定义已加载」即证明 import 解析链路通过。着色效果见渲染 PNG。

        // 全语言定义加载（同时验证 AvalonEdit 内嵌资源名在发行包中真实存在）
        bool allLanguagesLoaded = true;
        foreach (CodeLanguage language in Enum.GetValues<CodeLanguage>())
        {
            if (language == CodeLanguage.None)
            {
                continue;
            }

            editor.SyntaxLanguage = language;
            if (editor.InnerEditor.SyntaxHighlighting == null)
            {
                Console.WriteLine("      未加载：" + language);
                allLanguagesLoaded = false;
            }
        }

        Check(allLanguagesLoaded, "全部 CodeLanguage 高亮定义加载");

        // —— 明暗主题渲染（各自重建实例，验证资源解析与配色正确性）——
        string outputDirectory = AppContext.BaseDirectory;

        ThemeManager.ApplyTheme(AppTheme.Light);
        CodeEditor lightEditor = BuildEditor();
        string lightPath = Path.Combine(outputDirectory, "codeeditor-light.png");
        SaveRender(lightEditor, lightPath);
        Color lightPixel = GetPixel(lightPath, 640, 360);
        Check(lightPixel.R > 240 && lightPixel.G > 240 && lightPixel.B > 240, $"浅色表面为纸白（角点 {lightPixel}）");

        ThemeManager.ApplyTheme(AppTheme.Dark);
        CodeEditor darkEditor = BuildEditor();
        string darkPath = Path.Combine(outputDirectory, "codeeditor-dark.png");
        SaveRender(darkEditor, darkPath);
        Color darkPixel = GetPixel(darkPath, 640, 360);
        Check(darkPixel.R < 80 && darkPixel.G < 80 && darkPixel.B < 80, $"深色表面为暗底（角点 {darkPixel}）");

        Console.WriteLine("RENDER  " + lightPath);
        Console.WriteLine("RENDER  " + darkPath);

        return failures == 0 ? 0 : 1;
    }

    private static CodeEditor BuildEditor()
    {
        var editor = new CodeEditor
        {
            Width = HostWidth,
            Height = HostHeight,
            SyntaxLanguage = CodeLanguage.CSharp,
        };
        editor.Text = SampleCode;

        var host = new Border { Width = HostWidth, Height = HostHeight, Child = editor };
        host.Measure(new Size(HostWidth, HostHeight));
        host.Arrange(new Rect(0, 0, HostWidth, HostHeight));
        host.UpdateLayout();
        return editor;
    }

    private static void SaveRender(Visual visual, string path)
    {
        var bitmap = new RenderTargetBitmap((int)HostWidth, (int)HostHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using FileStream stream = File.Create(path);
        encoder.Save(stream);
    }

    private static Color GetPixel(string path, int x, int y)
    {
        var decoder = new PngBitmapDecoder(new Uri(path), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        byte[] pixels = new byte[4];
        decoder.Frames[0].CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);
        return Color.FromRgb(pixels[0], pixels[1], pixels[2]);
    }
}
