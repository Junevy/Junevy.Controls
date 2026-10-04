using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Junevy.Controls.AttachedProperties;

namespace DebugProbe;

/// <summary>
/// Label 冒烟探针（离屏 Measure/Arrange，真实走模板应用管线）：
///   1. 各 DisplayMode 样式触发器注入默认图标、局部值可覆盖/置空；
///   2. 无图标时图标区折叠；
///   3. 水平留白实测：有图标（Success）与无图标（Neutral）内容块均左右对称（图标折叠后不留残余间距）；
///   4. 固定宽度下 HorizontalContentAlignment=Center 落实（垂直方向作对照）；
///   5. 图标字号/颜色经 atc:Icon.IconSize / atc:Icon.IconForeground 注入（样式默认 + 局部覆盖）；
///   6. Neutral 默认中性灰底（Theme.Brush.Status.Neutral），局部 Background 覆盖后保持（红/黄色块用法），
///      Error/Success/Warning 状态底不受影响。
/// 离屏元素树不接收窗口布局，模板展开用 ApplyTemplate，尺寸用显式 Measure/Arrange（同 CodeEditorProbe）；
/// 触发器/绑定异步生效，测量多轮收敛到稳定尺寸；主题刷子依赖 CodeEditorProbe 先行加载的浅色主题。
/// </summary>
internal static class LabelProbe
{
    public static int Run()
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

        void Report(double left, double right, string name)
        {
            Console.WriteLine($"INFO  {name}: 左留白 {left:F1}px, 右留白 {right:F1}px");
        }

        // CodeEditorProbe 已被改为复用现有实例，本探针最先跑并自建 Application；
        // OnExplicitShutdown 保证后续探针关窗不会连带关闭应用（pack URI 与资源查找随之失效且无法重建）
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        Junevy.Controls.Themes.ThemeManager.ApplyTheme(Junevy.Controls.Themes.AppTheme.Light);
        Check(Application.Current.Resources.MergedDictionaries.Count > 0, "浅色主题资源已加载（刷子断言前提）");

        // 离屏窗口只为提供资源查找与视觉树链路，尺寸一律走显式 Measure/Arrange
        var root = new StackPanel();
        var window = new Window
        {
            Left = -30000,
            Top = 0,
            Width = 500,
            Height = 300,
            ShowInTaskbar = false,
            ShowActivated = false,
            Content = root
        };

        Brush? ThemeBrush(string key) => Application.Current?.TryFindResource(key) as Brush;
        bool SameBrush(Brush? a, Brush? b) => a is SolidColorBrush ca && b is SolidColorBrush cb
            ? ca.Color.Equals(cb.Color)
            : ReferenceEquals(a, b);

        window.Show();
        Pump(200);

        // —— 1. 样式触发器注入默认图标 / 局部值覆盖 / 局部置空 / 状态底回归 ——
        var error = new Junevy.Controls.Controls.Text.Label { Content = "Error" };
        root.Children.Add(error);
        Layout(error);
        var errorIcon = error.Template?.FindName("IconPresenter", error) as ContentControl;
        Check(errorIcon != null, "Error 模式 IconPresenter 就位");
        Check(errorIcon != null && Equals(errorIcon.Content, "\ue61a"), "Error 模式触发器注入默认图标（\\ue61a）");
        Check(SameBrush(error.Background, ThemeBrush("Theme.Brush.Status.Danger")), "Error 状态底不受本次改动影响（Status.Danger）");

        if (errorIcon != null)
        {
            Icon.SetIcon(error, "\ue651");
            Check(Equals(errorIcon.Content, "\ue651"), "局部值覆盖触发器图标（\\ue651）");
            Icon.SetIcon(error, string.Empty);
            Check(errorIcon.Visibility == Visibility.Collapsed, "局部置空图标后图标区折叠");
        }

        // —— 2. Neutral 默认无图标、默认中性灰底；局部 Background 覆盖优先于触发器 ——
        var neutral = new Junevy.Controls.Controls.Text.Label { Content = "Neutral", DisplayMode = Junevy.Controls.Controls.Text.LabelDisplayMode.Neutral };
        root.Children.Add(neutral);
        Layout(neutral);
        var neutralIcon = neutral.Template?.FindName("IconPresenter", neutral) as ContentControl;
        Check(neutralIcon != null && neutralIcon.Visibility == Visibility.Collapsed, "Neutral 默认无图标时图标区折叠");
        Check(SameBrush(neutral.Background, ThemeBrush("Theme.Brush.Status.Neutral")), "Neutral 默认中性灰底（Status.Neutral）");

        var redNeutral = new Junevy.Controls.Controls.Text.Label
        {
            Content = "Red",
            DisplayMode = Junevy.Controls.Controls.Text.LabelDisplayMode.Neutral,
            Background = Brushes.Red
        };
        root.Children.Add(redNeutral);
        Layout(redNeutral);
        Check(SameBrush(redNeutral.Background, Brushes.Red), "Neutral 局部 Background（红色）覆盖默认灰底");
        Check(SameBrush(redNeutral.Foreground, ThemeBrush("Theme.Brush.Text.OnAccent")), "局部改底色时前景保持 OnAccent（既有用法不受影响）");

        // —— 3. 水平留白实测（自动宽度，内容即实际占位） ——
        var success = new Junevy.Controls.Controls.Text.Label { Content = "Success", DisplayMode = Junevy.Controls.Controls.Text.LabelDisplayMode.Success };
        root.Children.Add(success);
        Layout(success);
        var successText = FindText(success, "Success");
        if (successText != null)
        {
            DumpParts(success, successText);
            var successIcon = success.Template?.FindName("IconPresenter", success) as ContentControl;
            var (left, right) = BlockInset(success, successText, successIcon);
            Report(left, right, "Success（有图标）内容块");
            Check(Math.Abs(left - right) <= 1.0, "有图标时内容块左右留白对称");
            Check(successIcon != null && Math.Abs(successIcon.FontSize - 8) < 0.1, "Boxed 默认图标字号取 atc:Icon.IconSize=8");
        }
        else
        {
            Check(false, "Success 文本节点定位");
        }

        var neutralText = FindText(neutral, "Neutral");
        if (neutralText != null)
        {
            DumpParts(neutral, neutralText);
            var (left, right) = BlockInset(neutral, neutralText, neutralIcon);
            Report(left, right, "Neutral（无图标）内容块");
            Check(Math.Abs(left - right) <= 1.0, "无图标时内容块左右留白对称");
        }
        else
        {
            Check(false, "Neutral 文本节点定位");
        }

        // —— 4. 固定宽度下 HorizontalContentAlignment=Center 落实（垂直方向对照） ——
        var wide = new Junevy.Controls.Controls.Text.Label
        {
            Content = "Centered?",
            DisplayMode = Junevy.Controls.Controls.Text.LabelDisplayMode.Neutral,
            Width = 400,
            Height = 60
        };
        root.Children.Add(wide);
        Layout(wide);
        var wideText = FindText(wide, "Centered?");
        if (wideText != null)
        {
            var (left, right) = BlockInset(wide, wideText, null);
            Report(left, right, "Neutral 固定 400px 水平");
            Check(Math.Abs(left - right) <= 1.0, "HorizontalContentAlignment=Center 被模板落实（水平居中）");

            var rect = wideText.TransformToVisual(wide).TransformBounds(new Rect(wideText.RenderSize));
            var top = rect.Top;
            var bottom = wide.ActualHeight - rect.Bottom;
            Report(top, bottom, "Neutral 固定 60px 垂直");
            Check(Math.Abs(top - bottom) <= 1.0, "VerticalContentAlignment=Center 被模板落实（垂直居中）");
        }
        else
        {
            Check(false, "Centered? 文本节点定位");
        }

        // —— 5. IconSize / IconForeground 局部覆盖 + Borderless 默认值 ——
        var sized = new Junevy.Controls.Controls.Text.Label { Content = "Sized", DisplayMode = Junevy.Controls.Controls.Text.LabelDisplayMode.Neutral };
        Icon.SetIcon(sized, "\ue651");
        Icon.SetIconSize(sized, 20);
        Icon.SetIconForeground(sized, Brushes.Orange);
        root.Children.Add(sized);
        Layout(sized);
        var sizedIcon = sized.Template?.FindName("IconPresenter", sized) as ContentControl;
        Check(sizedIcon != null && Math.Abs(sizedIcon.FontSize - 20) < 0.1, "局部 atc:Icon.IconSize=20 覆盖图标字号");
        Check(sizedIcon != null && SameBrush(sizedIcon.Foreground, Brushes.Orange), "局部 atc:Icon.IconForeground 覆盖图标颜色");

        var borderless = new Junevy.Controls.Controls.Text.Label { Content = "Borderless", DisplayMode = Junevy.Controls.Controls.Text.LabelDisplayMode.BorderlessError };
        root.Children.Add(borderless);
        Layout(borderless);
        var borderlessIcon = borderless.Template?.FindName("IconPresenter", borderless) as ContentControl;
        Check(borderlessIcon != null && Math.Abs(borderlessIcon.FontSize - 14) < 0.1, "Borderless 默认图标字号取 atc:Icon.IconSize=14");
        Check(SameBrush(borderlessIcon?.Foreground, ThemeBrush("Theme.Brush.Status.Danger")), "Borderless 图标颜色取 atc:Icon.IconForeground（Status.Danger）");

        window.Close();
        Console.WriteLine($"LabelProbe 完成：{(failures == 0 ? "全部 PASS" : failures + " 项 FAIL")}");
        return failures;
    }

    // 样式触发器注入图标、TemplatedParent 绑定解析都是异步到达的，测量必须多轮收敛到稳定尺寸
    private static void Layout(FrameworkElement element)
    {
        element.ApplyTemplate();
        var last = Size.Empty;
        for (int i = 0; i < 4; i++)
        {
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));
            element.UpdateLayout();
            Pump(50);
            if (element.DesiredSize == last)
            {
                break;
            }

            last = element.DesiredSize;
        }
    }

    // 内容块 = 可见图标 + 文本的整体占位（图标折叠时即文本本身），左右留白相对 Label 边缘
    private static (double left, double right) BlockInset(
        System.Windows.Controls.Label label, FrameworkElement text, FrameworkElement? icon)
    {
        var textRect = text.TransformToVisual(label).TransformBounds(new Rect(text.RenderSize));
        var left = textRect.Left;
        var rightEdge = textRect.Right;
        if (icon is { Visibility: Visibility.Visible })
        {
            var iconRect = icon.TransformToVisual(label).TransformBounds(new Rect(icon.RenderSize));
            left = Math.Min(left, iconRect.Left);
            rightEdge = Math.Max(rightEdge, iconRect.Right);
        }

        return (left, label.ActualWidth - rightEdge);
    }

    private static void DumpParts(System.Windows.Controls.Label label, FrameworkElement text)
    {
        Console.WriteLine($"DUMP  {label.Content} Desired={label.DesiredSize} Actual={label.ActualWidth:F1}x{label.ActualHeight:F1} Padding={label.Padding}");
        int count = VisualTreeHelper.GetChildrenCount(label);
        for (int i = 0; i < count; i++)
        {
            DumpRect(VisualTreeHelper.GetChild(label, i), label, 1);
        }
    }

    private static void DumpRect(DependencyObject node, System.Windows.Controls.Label label, int depth)
    {
        if (node is FrameworkElement fe)
        {
            var rect = fe.TransformToVisual(label).TransformBounds(new Rect(fe.RenderSize));
            Console.WriteLine($"DUMP  {new string(' ', depth * 2)}{fe.GetType().Name} Margin={fe.Margin} Rect={rect}");
        }

        int count = VisualTreeHelper.GetChildrenCount(node);
        for (int i = 0; i < count; i++)
        {
            DumpRect(VisualTreeHelper.GetChild(node, i), label, depth + 1);
        }
    }

    // ContentPresenter（RecognizesAccessKey=True）对字符串内容生成 AccessText（直接继承 FrameworkElement，非 TextBlock）
    private static FrameworkElement? FindText(DependencyObject root, string text)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            var matched = (child as TextBlock)?.Text == text || (child as AccessText)?.Text == text
                ? child as FrameworkElement
                : null;
            if (matched != null)
            {
                return matched;
            }

            var deep = FindText(child, text);
            if (deep != null)
            {
                return deep;
            }
        }

        return null;
    }

    private static void Pump(int milliseconds)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < milliseconds)
        {
            DoEvents();
            Thread.Sleep(15);
        }
    }

    private static void DoEvents()
    {
        var frame = new System.Windows.Threading.DispatcherFrame();
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Background,
            new Action(() => frame.Continue = false));
        System.Windows.Threading.Dispatcher.PushFrame(frame);
    }
}
