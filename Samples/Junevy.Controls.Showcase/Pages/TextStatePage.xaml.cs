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
            CodeEditorInstance.CompletionProvider = new SimpleKeywordCompletionProvider();
        }
    }
}
