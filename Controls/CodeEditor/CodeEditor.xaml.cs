using System;
using System.Windows;
using System.Windows.Controls;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;

namespace Junevy.Controls.Controls.CodeEditor
{
    /// <summary>
    /// 代码编辑器控件：完全封装 AvalonEdit 的 <see cref="TextEditor"/>，公共 API 只暴露 Junevy 类型，
    /// 高级场景（折叠管理、自定义 Renderer、补全窗等）经 <see cref="InnerEditor"/> 逃生口取内部实例。
    /// <para>
    /// 外观口径与 TextBox 一致（Surface.Base 表面 + Border.Default 发丝线 + 控件圆角/内边距令牌），
    /// 选区、行号与语法着色全部引用 <c>Theme.Brush.*</c> 主题画刷，深浅主题切换即时生效；
    /// 语法高亮按 <see cref="Language"/> 加载 AvalonEdit 内置规则的私有副本，不会改动全局共享定义。
    /// </para>
    /// </summary>
    [TemplatePart(Name = PART_Editor, Type = typeof(TextEditor))]
    public partial class CodeEditor : Control
    {
        private const string PART_Editor = "PART_Editor";

        private const string SurfaceSelectedBrushKey = "Theme.Brush.Surface.Selected";
        private const string TextPrimaryBrushKey = "Theme.Brush.Text.Primary";

        private TextEditor? editor;
        private bool syncingText;

        static CodeEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CodeEditor),
                new FrameworkPropertyMetadata(typeof(CodeEditor)));
        }

        /// <summary>
        /// 标识 <see cref="Text"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(CodeEditor),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextChanged));

        /// <summary>
        /// 获取或设置编辑器文本。
        /// <para>
        /// 编辑器内输入时本属性按 PropertyChanged 节奏同步；从外部设置时整体替换文档文本，
        /// 与 AvalonEdit 语义一致——光标回到起点、撤销栈清空。
        /// </para>
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="SyntaxLanguage"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty SyntaxLanguageProperty =
            DependencyProperty.Register(
                nameof(SyntaxLanguage),
                typeof(CodeLanguage),
                typeof(CodeEditor),
                new PropertyMetadata(CodeLanguage.None, OnSyntaxLanguageChanged));

        /// <summary>
        /// 获取或设置语法高亮语言，运行时切换即时生效；
        /// <see cref="CodeLanguage.None"/> 表示纯文本、不着色。
        /// <para>命名说明：不用 <c>Language</c> 是为避免遮蔽 <see cref="FrameworkElement.Language"/>（xml:lang）。</para>
        /// </summary>
        public CodeLanguage SyntaxLanguage
        {
            get { return (CodeLanguage)GetValue(SyntaxLanguageProperty); }
            set { SetValue(SyntaxLanguageProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="ShowLineNumbers"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty ShowLineNumbersProperty =
            DependencyProperty.Register(
                nameof(ShowLineNumbers),
                typeof(bool),
                typeof(CodeEditor),
                new PropertyMetadata(true));

        /// <summary>
        /// 是否显示行号（行号颜色跟随主题 <c>Theme.Brush.Text.Tertiary</c>）。默认 <see langword="true"/>。
        /// </summary>
        public bool ShowLineNumbers
        {
            get { return (bool)GetValue(ShowLineNumbersProperty); }
            set { SetValue(ShowLineNumbersProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="IsReadOnly"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(CodeEditor),
                new PropertyMetadata(false));

        /// <summary>
        /// 是否只读：只读时内容不可编辑，表面下沉为 <c>Theme.Brush.Surface.Sunken</c>（与 TextBox 只读态一致）。
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="WordWrap"/> 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty WordWrapProperty =
            DependencyProperty.Register(
                nameof(WordWrap),
                typeof(bool),
                typeof(CodeEditor),
                new PropertyMetadata(false));

        /// <summary>
        /// 是否自动换行；换行时隐藏水平滚动条。
        /// </summary>
        public bool WordWrap
        {
            get { return (bool)GetValue(WordWrapProperty); }
            set { SetValue(WordWrapProperty, value); }
        }

        /// <summary>
        /// 内部 AvalonEdit 编辑器实例（逃生口）：折叠管理、自定义背景渲染器、补全窗等
        /// 未封装能力经此访问；模板应用前为 <see langword="null"/>。
        /// </summary>
        public TextEditor? InnerEditor => editor;

        public override void OnApplyTemplate()
        {
            if (editor != null)
            {
                editor.TextChanged -= OnEditorTextChanged;
                DetachCompletionPipeline(editor.TextArea);
            }

            base.OnApplyTemplate();

            editor = GetTemplateChild(PART_Editor) as TextEditor;
            if (editor == null)
            {
                return;
            }

            editor.TextChanged += OnEditorTextChanged;
            AttachCompletionPipeline(editor.TextArea);
            ApplySelectionBrushes(editor);
            ApplyLanguage();
            PushTextIntoEditor();
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CodeEditor)d;
            if (!control.syncingText)
            {
                control.PushTextIntoEditor();
            }
        }

        private static void OnSyntaxLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CodeEditor)d).ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            if (editor != null)
            {
                editor.SyntaxHighlighting = ThemedHighlightingProvider.GetDefinition(SyntaxLanguage);
            }
        }

        private void PushTextIntoEditor()
        {
            if (editor == null)
            {
                return;
            }

            string text = Text;
            if (!string.Equals(editor.Text, text, StringComparison.Ordinal))
            {
                // 整体替换语义（光标回起点、撤销栈清空）仅在文本实际变化时触发，
                // 避免绑定回推把光标打回文档开头
                editor.Text = text;
            }
        }

        private void OnEditorTextChanged(object? sender, EventArgs e)
        {
            if (editor == null)
            {
                return;
            }

            syncingText = true;
            try
            {
                SetCurrentValue(TextProperty, editor.Text);
            }
            finally
            {
                syncingText = false;
            }
        }

        /// <summary>
        /// 选区颜色用资源引用而非固定画刷：ThemeManager 换主题字典后即时生效，
        /// 且优先级高于 AvalonEdit 默认样式的 SystemColors 选区色。
        /// </summary>
        private static void ApplySelectionBrushes(TextEditor target)
        {
            TextArea textArea = target.TextArea;
            textArea.SetResourceReference(TextArea.SelectionBrushProperty, SurfaceSelectedBrushKey);
            textArea.SetResourceReference(TextArea.SelectionForegroundProperty, TextPrimaryBrushKey);
        }
    }
}
