using System.Windows.Controls;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>文本与状态展示页：Label / TextBlock / ToolTip / CodeEditor。</summary>
    public partial class TextStatePage : UserControl
    {
        private const string DemoCode =
            """
            using System;
            using System.Collections.Generic;

            namespace Demo
            {
                /// <summary>
                /// 示例：语法着色映射主题画刷，随浅色/深色主题即时切换。
                /// </summary>
                public class Sample
                {
                    private const double Pi = 3.14159;
                    private readonly List<string> items = new();

                    // 普通注释：计算圆面积
                    public double Area(double radius)
                    {
                        if (radius <= 0) throw new ArgumentException("半径必须为正数", nameof(radius));
                        return Pi * radius * radius;
                    }
                }
            }
            """;

        public TextStatePage()
        {
            InitializeComponent();
            CodeEditorInstance.Text = DemoCode;
            // Roslyn 系统类 IntelliSense（伴生包 Junevy.Controls.CodeCompletion）：
            // 输入标识符（如 str、Conso）弹类型/关键词补全，输入 Console. 弹成员
            CodeEditorInstance.CompletionProvider = new Junevy.Controls.CodeCompletion.RoslynCodeCompletionProvider();
        }
    }
}
