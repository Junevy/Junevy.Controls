using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

using Junevy.Controls.AttachedProperties;
using Junevy.Controls.Controls.Box;
using Junevy.Controls.Themes;
using ListBox = Junevy.Controls.Controls.Box.ListBox;

namespace DebugProbe;

/// <summary>
/// 平滑滚动冒烟探针（隐藏窗口承载：动画时钟需要渲染管线推进，离屏元素不计时）：
///   1. 默认样式启用 + PART_ScrollViewer 挂接 + ScrollUnit=Pixel 生效；
///   2. 滚轮接管：事件标记已处理、偏移不瞬跳、补间产生中间帧、终值落在一个步长上；
///   3. 连续滚轮从当前动画位置累计，不重置不叠加；
///   4. 拖动滑块（ScrollBar Scroll 事件）与程序滚动（ScrollChanged 偏差）都能打断补间；
///   5. 嵌套就近接管：滚轮作用于内层列表时外层不动，直接作用于外层时外层正常滚动；
///   6. 行为关闭时预览阶段不接管，原生路径照常瞬时滚动；
///   7. Step 附加属性按显式像素步长生效；
///   8. 横向列表滚轮折算 + 补间（ScrollUnit=Pixel 下按像素步长）；
///   9. 宿主优先级：隐式样式命中模板内 ScrollViewer 的自挂接不覆盖控件宿主；
///  10. 快速连续滚轮（爆发式/匀速/悬停目标中途切换）：偏移单调不减、无回弹；
///  11. 纯横向宿主（代码块同构）：竖向滚轮被 WPF 原生吞掉、页面不滚——原生行为与平滑无关；
///      ScrollByWheel 转发出口与真实滚轮同路径（步长折算 + 目标累加 + 补间）；
///  12. DEBUG 虚拟化失效诊断：无界测量告警一次，恢复有界后重新武装。
/// </summary>
internal static class SmoothScrollProbe
{
    public static int Run()
    {
        // CodeEditorProbe 已创建 Application；这里只把主题切回浅色，保证模板资源可解析
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

        if (Mouse.PrimaryDevice == null)
        {
            Console.WriteLine("FAIL  输入设备不可用（Mouse.PrimaryDevice == null），滚轮事件无法合成");
            return 1;
        }

        var root = new StackPanel();
        var window = new Window
        {
            Left = -30000,
            Top = 0,
            Width = 520,
            Height = 500,
            ShowInTaskbar = false,
            ShowActivated = false,
            Content = root
        };
        window.Show();
        Pump(250);

        // —— 1. 竖向列表：接管 + 补间 ——
        ListBox list = BuildList(200, 420, 320);
        root.Children.Add(list);
        Pump(300);

        ScrollViewer? viewer = list.Template?.FindName("PART_ScrollViewer", list) as ScrollViewer;
        Check(viewer != null, "PART_ScrollViewer 就位");
        if (viewer == null)
        {
            window.Close();
            return failures;
        }

        Check(VirtualizingPanel.GetScrollUnit(list) == ScrollUnit.Pixel, "ScrollUnit=Pixel 已随默认样式生效");
        Check(viewer.ScrollableHeight > 3000, $"按像素滚动（ScrollableHeight={viewer.ScrollableHeight:F0}px，按项应约为 200）");

        double lines = SystemParameters.WheelScrollLines;

        // 与库内 SmoothScrolling.PixelLineHeight（internal）保持一致：像素滚轮一步 = N × 16px
        double expectedStep = lines > 0 ? lines * 16.0 : viewer.ViewportHeight;

        bool handled = RaisePreviewWheel(viewer, -120);
        Check(handled, "滚轮接管：事件标记已处理");
        Check(viewer.VerticalOffset < 4.0, $"无瞬时跳变（事件后偏移 {viewer.VerticalOffset:F1}px）");

        var samples = new List<double>();
        for (int i = 0; i < 5; i++)
        {
            Pump(35);
            samples.Add(viewer.VerticalOffset);
        }

        Check(samples.Count(value => value > 0.5) >= 3 && samples.Distinct().Count() >= 3,
            $"补间产生多个中间帧（{string.Join(", ", samples.Select(value => value.ToString("F1")))}）");

        Pump(250);
        Check(Math.Abs(viewer.VerticalOffset - expectedStep) < 6,
            $"补间终值 ≈ 一个步长（实际 {viewer.VerticalOffset:F1} / 期望 {expectedStep:F0}）");

        // —— 2. 连续滚轮：目标精确累加（从上一目标继续 + 一格，无欠冲）——
        double startOffset = viewer.VerticalOffset;
        RaisePreviewWheel(viewer, -120);
        Pump(90);
        double midFlight = viewer.VerticalOffset;
        RaisePreviewWheel(viewer, -120);
        Pump(400);
        Check(midFlight > startOffset && Math.Abs(viewer.VerticalOffset - (startOffset + (2 * expectedStep))) < 8,
            $"连续滚轮目标精确累加（起点 {startOffset:F1}，中途 {midFlight:F1} → 终值 {viewer.VerticalOffset:F1} = 起点 + 两格）");

        // —— 3. 拖动滑块打断 ——
        RaisePreviewWheel(viewer, -120);
        Pump(60);
        double interruptedAt = viewer.VerticalOffset;
        bool probeSawBarScroll = false;
        viewer.AddHandler(ScrollBar.ScrollEvent, new ScrollEventHandler((s, args) => probeSawBarScroll = true), handledEventsToo: true);
        var thumbArgs = new ScrollEventArgs(ScrollEventType.ThumbTrack, viewer.VerticalOffset)
        {
            RoutedEvent = ScrollBar.ScrollEvent
        };
        viewer.RaiseEvent(thumbArgs);
        Pump(320);
        Console.WriteLine($"      （Spy：Scroll 事件触发={probeSawBarScroll}）");
        Check(Math.Abs(viewer.VerticalOffset - interruptedAt) < 1.5,
            $"拖动滑块打断补间（打断时 {interruptedAt:F1} → 泵送后 {viewer.VerticalOffset:F1}）");

        // —— 4. 鼠标按下 / 按键打断 ——
        RaisePreviewWheel(viewer, -120);
        Pump(40);
        double mouseDownAt = viewer.VerticalOffset;
        var mouseArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
        {
            RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent
        };
        viewer.RaiseEvent(mouseArgs);
        Pump(320);
        Check(Math.Abs(viewer.VerticalOffset - mouseDownAt) < 1.5,
            $"按下鼠标打断补间（打断时 {mouseDownAt:F1} → 泵送后 {viewer.VerticalOffset:F1}）");

        RaisePreviewWheel(viewer, -120);
        Pump(40);
        double keyDownAt = viewer.VerticalOffset;
        var keyArgs = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), Environment.TickCount, Key.Down)
        {
            RoutedEvent = UIElement.PreviewKeyDownEvent
        };
        viewer.RaiseEvent(keyArgs);
        Pump(320);
        Check(Math.Abs(viewer.VerticalOffset - keyDownAt) < 1.5,
            $"按键打断补间（打断时 {keyDownAt:F1} → 泵送后 {viewer.VerticalOffset:F1}）");

        // —— 5. 打断后恢复接管 ——
        bool recovered = RaisePreviewWheel(viewer, -120);
        Pump(400);
        Check(recovered && viewer.VerticalOffset > keyDownAt + 4, "打断后滚轮恢复接管并继续滚动");

        // —— 6. 嵌套就近接管 ——
        ListBox inner = BuildList(50, 360, 100);
        ListBox outer = BuildList(2, 420, 130);
        outer.ItemsSource = new object[] { new Border { Child = inner, Padding = new Thickness(8) }, "Tail item" };
        root.Children.Add(outer);
        Pump(400);

        ScrollViewer? innerViewer = inner.Template?.FindName("PART_ScrollViewer", inner) as ScrollViewer;
        ScrollViewer? outerViewer = outer.Template?.FindName("PART_ScrollViewer", outer) as ScrollViewer;
        if (innerViewer == null || outerViewer == null)
        {
            Check(false, "嵌套用例模板就位");
            window.Close();
            return failures;
        }

        RaisePreviewWheel(innerViewer, -120);
        Pump(400);
        Check(innerViewer.VerticalOffset > 4.0 && outerViewer.VerticalOffset < 0.5,
            $"滚轮作用于内层列表：内层滚动（{innerViewer.VerticalOffset:F1}）外层不动（{outerViewer.VerticalOffset:F1}）");

        bool outerHandled = RaisePreviewWheel(outerViewer, -120);
        Pump(400);
        Check(outerHandled && outerViewer.VerticalOffset > 4.0, "滚轮直接作用于外层列表时正常接管滚动");

        // —— 7. 行为关闭：走原生瞬时滚动 ——
        ListBox nativeList = BuildList(200, 420, 320);
        root.Children.Add(nativeList);
        Pump(300);
        SmoothScrolling.SetIsEnabled(nativeList, false);
        ScrollViewer? nativeViewer = nativeList.Template?.FindName("PART_ScrollViewer", nativeList) as ScrollViewer;
        if (nativeViewer == null)
        {
            Check(false, "原生用例模板就位");
            window.Close();
            return failures;
        }

        bool previewHandled = RaisePreviewWheel(nativeViewer, -120);
        Check(!previewHandled, "行为关闭：预览阶段不接管");
        double beforeNative = nativeViewer.VerticalOffset;
        var bubblingArgs = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, -120)
        {
            RoutedEvent = UIElement.MouseWheelEvent
        };
        nativeViewer.RaiseEvent(bubblingArgs);
        Pump(80);
        Console.WriteLine($"      （原生路径：Handled={bubblingArgs.Handled}，ScrollableHeight={nativeViewer.ScrollableHeight:F0}，偏移 {beforeNative:F1} → {nativeViewer.VerticalOffset:F1}）");
        Check(bubblingArgs.Handled && nativeViewer.VerticalOffset > beforeNative + 1, "行为关闭：原生路径照常瞬时滚动");

        // —— 8. Step 附加属性 ——
        double beforeStep = viewer.VerticalOffset;
        SmoothScrolling.SetStep(list, 120);
        RaisePreviewWheel(viewer, -120);
        Pump(400);
        Check(Math.Abs(viewer.VerticalOffset - (beforeStep + 120)) < 8,
            $"Step=120 生效（{beforeStep:F1} → {viewer.VerticalOffset:F1}）");
        SmoothScrolling.SetStep(list, double.NaN);

        // —— 9. 横向列表：折算 + 补间 ——
        ListBox horizontal = BuildList(200, 420, 120, horizontal: true);
        root.Children.Add(horizontal);
        Pump(300);
        ScrollViewer? horizontalViewer = horizontal.Template?.FindName("PART_ScrollViewer", horizontal) as ScrollViewer;
        if (horizontalViewer == null)
        {
            Check(false, "横向用例模板就位");
            window.Close();
            return failures;
        }

        bool horizontalHandled = RaisePreviewWheel(horizontalViewer, -120);
        Check(horizontalHandled && horizontalViewer.HorizontalOffset < 4.0, "横向列表滚轮接管且无瞬跳");
        Pump(400);
        Check(Math.Abs(horizontalViewer.HorizontalOffset - expectedStep) < 8,
            $"横向补间按像素步长到位（实际 {horizontalViewer.HorizontalOffset:F1} / 期望 {expectedStep:F0}）");

        // —— 10. 宿主优先级：自挂接不得覆盖控件宿主 ——
        // 模拟应用级隐式样式直接命中控件模板内 ScrollViewer（自挂接）：宿主若被自挂接夺走
        // （Host=ScrollViewer 自身），UsesPixelScrolling 拿不到 ItemsControl 宿主，
        // 像素单位判定失效，滚轮会退回原生（事件不被接管）
        ListBox precedenceList = BuildList(50, 360, 200);
        root.Children.Add(precedenceList);
        Pump(300);
        ScrollViewer? precedenceViewer = precedenceList.Template?.FindName("PART_ScrollViewer", precedenceList) as ScrollViewer;
        if (precedenceViewer == null)
        {
            Check(false, "宿主优先级用例模板就位");
            window.Close();
            return failures;
        }

        SmoothScrolling.SetIsEnabled(precedenceViewer, true);
        bool precedenceHandled = RaisePreviewWheel(precedenceViewer, -120);
        Pump(400);
        Check(precedenceHandled, "宿主优先级：自挂接不覆盖控件宿主，平滑滚动保持接管");
        SmoothScrolling.SetIsEnabled(precedenceViewer, false);
        SmoothScrolling.SetIsEnabled(precedenceList, false);
        SmoothScrolling.SetIsEnabled(precedenceList, true);

        // —— 11. 快速连续滚轮：偏移单调、无回弹 ——
        ListBox fastList = BuildList(400, 360, 200);
        root.Children.Add(fastList);
        Pump(300);
        ScrollViewer? fastViewer = fastList.Template?.FindName("PART_ScrollViewer", fastList) as ScrollViewer;
        if (fastViewer == null)
        {
            Check(false, "快速滚轮用例模板就位");
            window.Close();
            return failures;
        }

        // 爆发式：12 格在同一调度帧内连发（事件间不产生渲染帧）
        for (int i = 0; i < 12; i++)
        {
            RaisePreviewWheel(fastViewer, -120);
        }

        var burstSamples = new List<double>();
        for (int i = 0; i < 22; i++)
        {
            Pump(40);
            burstSamples.Add(fastViewer.VerticalOffset);
        }

        Check(IsMonotonicNonDecreasing(burstSamples),
            $"爆发式 12 格连滚：偏移单调不减（{FormatSamples(burstSamples)}）");
        Check(Math.Abs(burstSamples[^1] - (12 * 48)) < 8,
            $"爆发式连滚终值 = 12 格步长（实际 {burstSamples[^1]:F1} / 期望 {12 * 48}）");

        // 匀速快滚：每格间隔约 35ms（接近真实快滚的帧间节奏）
        var pacedSamples = new List<double>();
        for (int i = 0; i < 10; i++)
        {
            RaisePreviewWheel(fastViewer, -120);
            Pump(35);
            pacedSamples.Add(fastViewer.VerticalOffset);
        }

        for (int i = 0; i < 14; i++)
        {
            Pump(40);
            pacedSamples.Add(fastViewer.VerticalOffset);
        }

        Check(IsMonotonicNonDecreasing(pacedSamples), "匀速快滚：偏移单调不减（无回弹）");

        // 悬停目标中途切换：页面宿主滚 2 格后（不泵帧）剩余滚轮落到内层列表
        // （与真实页面同构：ScrollViewer 自挂接 + 内含列表，双补间并存时互不回拉）
        var pageHost = new ScrollViewer { Width = 360, Height = 200 };
        ListBox innerInPage = BuildList(100, 320, 400);
        var pageContent = new StackPanel();
        pageContent.Children.Add(innerInPage);
        pageContent.Children.Add(new Border { Height = 300 });
        pageHost.Content = pageContent;
        root.Children.Add(pageHost);
        Pump(300);
        SmoothScrolling.SetIsEnabled(pageHost, true);

        RaisePreviewWheel(pageHost, -120);
        RaisePreviewWheel(pageHost, -120);
        ScrollViewer? innerInPageViewer = innerInPage.Template?.FindName("PART_ScrollViewer", innerInPage) as ScrollViewer;
        if (innerInPageViewer == null)
        {
            Check(false, "悬停切换用例模板就位");
            window.Close();
            return failures;
        }

        for (int i = 0; i < 3; i++)
        {
            RaisePreviewWheel(innerInPageViewer, -120);
        }

        var pageSamples = new List<double>();
        var innerSamples = new List<double>();
        for (int i = 0; i < 18; i++)
        {
            Pump(40);
            pageSamples.Add(pageHost.VerticalOffset);
            innerSamples.Add(innerInPageViewer.VerticalOffset);
        }

        Check(IsMonotonicNonDecreasing(pageSamples) && Math.Abs(pageSamples[^1] - (2 * 48)) < 8,
            $"悬停切换后页面宿主收敛到自身 2 格（终值 {pageSamples[^1]:F1}，采样 {FormatSamples(pageSamples)}）");
        Check(IsMonotonicNonDecreasing(innerSamples) && Math.Abs(innerSamples[^1] - (3 * 48)) < 8,
            $"悬停切换后内层列表收敛到 3 格（终值 {innerSamples[^1]:F1}，采样 {FormatSamples(innerSamples)}）");

        // —— 12. 纯横向宿主（如代码块）：竖向滚轮被 WPF 原生吞掉，页面不滚 ——
        // 复现 Showcase 展开的 XAML 代码块（VerticalScrollBarVisibility=Disabled + Horizontal=Auto）
        // 场景：代码块完全不做任何挂接，证明"吞滚轮"是 ScrollViewer.OnMouseWheel 的原生行为
        //（无条件标记已处理），与平滑滚动无关
        var pageHost2 = new ScrollViewer { Width = 360, Height = 200 };
        var codeBlock = new ScrollViewer
        {
            Width = 340,
            Height = 60,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        codeBlock.Content = new TextBlock
        {
            Text = new string('x', 400),
            TextWrapping = TextWrapping.NoWrap,
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 12
        };
        var pageContent2 = new StackPanel();
        pageContent2.Children.Add(new Border { Height = 150 });
        pageContent2.Children.Add(codeBlock);
        pageContent2.Children.Add(new Border { Height = 400 });
        pageHost2.Content = pageContent2;
        root.Children.Add(pageHost2);
        Pump(300);
        SmoothScrolling.SetIsEnabled(pageHost2, true);

        bool ourTaken = RaisePreviewWheel(codeBlock, -120);
        Check(!ourTaken, "纯横向宿主：本库不接管竖向滚轮（ScrollableHeight=0 不满足竖向接管）");

        double pageBefore2 = pageHost2.VerticalOffset;
        var nativeSwallowArgs = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, -120)
        {
            RoutedEvent = UIElement.MouseWheelEvent
        };
        codeBlock.RaiseEvent(nativeSwallowArgs);
        Pump(200);
        Check(nativeSwallowArgs.Handled && Math.Abs(pageHost2.VerticalOffset - pageBefore2) < 0.5,
            $"纯横向宿主吞掉竖向滚轮（WPF 原生行为，代码块无任何挂接）：页面不滚（{pageBefore2:F1} → {pageHost2.VerticalOffset:F1}）");

        // 转发走平滑出口：ScrollByWheel 与真实滚轮同路径（步长折算 + 目标累加 + 补间）
        double beforeForward = pageHost2.VerticalOffset;
        SmoothScrolling.ScrollByWheel(pageHost2, -120);
        Pump(300);
        Check(Math.Abs(pageHost2.VerticalOffset - (beforeForward + expectedStep)) < 8,
            $"ScrollByWheel 转发走平滑补间（{beforeForward:F1} → {pageHost2.VerticalOffset:F1} ≈ 一格步长）");

        SmoothScrolling.ScrollByWheel(pageHost2, -120);
        SmoothScrolling.ScrollByWheel(pageHost2, -60);
        Pump(400);
        Check(Math.Abs(pageHost2.VerticalOffset - (beforeForward + (2.5 * expectedStep))) < 8,
            $"连续转发目标精确累加（{beforeForward:F1} → {pageHost2.VerticalOffset:F1} = 起点 + 2.5 格）");

        // —— 13. DEBUG 虚拟化失效诊断 ——
        var listener = new CapturingListener();
        Trace.Listeners.Add(listener);
        try
        {
            ListBox unbounded = BuildList(3, 400, double.NaN);
            unbounded.Measure(new Size(400, double.PositiveInfinity));
            Check(listener.Messages.Any(message => message.Contains("虚拟化已失效")), "无界测量触发 DEBUG 诊断警告");
            int warningsBefore = listener.Messages.Count;

            unbounded.Measure(new Size(400, 240));
            unbounded.Measure(new Size(400, double.PositiveInfinity));
            Check(listener.Messages.Count > warningsBefore, "恢复有界后重新武装，再次无界会再度告警");
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }

        window.Close();
        return failures;
    }

    private static ListBox BuildList(int count, double width, double height, bool horizontal = false)
    {
        var list = new ListBox
        {
            Width = width,
            Height = height,
            Orientation = horizontal ? Orientation.Horizontal : Orientation.Vertical
        };
        list.ItemsSource = Enumerable.Range(0, count).Select(index => "Item " + index).ToList();
        return list;
    }

    /// <summary>采样序列是否单调不减（允许 0.5px 的浮点抖动）。</summary>
    private static bool IsMonotonicNonDecreasing(List<double> samples)
    {
        for (int i = 1; i < samples.Count; i++)
        {
            if (samples[i] < samples[i - 1] - 0.5)
            {
                return false;
            }
        }

        return true;
    }

    private static string FormatSamples(List<double> samples)
    {
        return string.Join(", ", samples.Select(value => value.ToString("F1")));
    }

    /// <summary>合成预览滚轮事件（隧穿路由），返回事件是否被标记已处理。</summary>
    private static bool RaisePreviewWheel(UIElement target, int delta)
    {
        var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta)
        {
            RoutedEvent = UIElement.PreviewMouseWheelEvent
        };
        target.RaiseEvent(args);
        return args.Handled;
    }

    /// <summary>推进渲染/布局/动画时钟若干毫秒（隐藏窗口的渲染管线负责动画计时）。</summary>
    private static void Pump(int milliseconds)
    {
        var watch = Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < milliseconds)
        {
            DoEvents();
            Thread.Sleep(15);
        }
    }

    private static void DoEvents()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    /// <summary>捕获 Debug/Trace 输出的监听器（诊断告警断言用）。</summary>
    private sealed class CapturingListener : TraceListener
    {
        public List<string> Messages { get; } = new();

        public override void Write(string? message)
        {
        }

        public override void WriteLine(string? message)
        {
            if (message != null)
            {
                Messages.Add(message);
            }
        }

        public override void Write(object? o, string? category)
        {
        }

        public override void WriteLine(object? o, string? category)
        {
            if (o != null)
            {
                Messages.Add(o.ToString() ?? string.Empty);
            }
        }
    }
}
