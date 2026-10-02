using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

using Junevy.Controls.Controls.Expander;

namespace DebugProbe;

/// <summary>
/// ExpanderPanel 冒烟探针（隐藏窗口承载，真实走模板应用管线）：
///   1. 经典模式默认值（DisplayMode=Classic、Icon/Description/HeaderExtra 为空、IsExpanded=true）；
///   2. 经典模板部件（PART_HeaderButton / PART_ContentHost / PART_ScaleTransform）与 IsChecked 绑定；
///   3. Toggle() 切换 + Expanded/Collapsed 事件各触发一次；
///   4. 卡片模式模板切换：部件就位（PART_HeaderButton / IconHost / DescriptionHost / HeaderExtraHost / Chevron）；
///   5. 卡片头部：Header 进标题槽、Description/Icon 为空折叠、赋值后可见（头部高度自适应）；
///   6. 卡片箭头随展开状态旋转（折叠 90° 朝下、展开 270° 朝上）；
///   7. 头部 IsChecked 双向绑定驱动 IsExpanded（真实点击的底层路径）；
///   8. HeaderExtra 内容送达扩展槽；
///   9. 切回 Classic 后卡片模板部件卸载。
/// 展开动画的中间帧与内容宿主收起时序依赖渲染管线推进，实机冒烟覆盖。
/// </summary>
internal static class ExpanderPanelProbe
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

        var root = new StackPanel();
        var window = new Window
        {
            Left = -30000,
            Top = 0,
            Width = 480,
            Height = 420,
            ShowInTaskbar = false,
            ShowActivated = false,
            Content = root
        };
        window.Show();
        Pump(200);

        var panel = new ExpanderPanel { Width = 420 };
        int expandedCount = 0;
        int collapsedCount = 0;
        panel.Expanded += (_, _) => expandedCount++;
        panel.Collapsed += (_, _) => collapsedCount++;
        root.Children.Add(panel);

        // —— 1. 默认值 ——
        Check(panel.DisplayMode == ExpanderDisplayMode.Classic, "默认 DisplayMode=Classic");
        Check(panel.Icon == null && panel.Description == null && panel.HeaderExtra == null, "默认 Icon/Description/HeaderExtra 为空");
        Check(panel.IsExpanded, "默认 IsExpanded=true");

        panel.ApplyTemplate();
        Pump(200);

        // —— 2. 经典模板 ——
        var classicHeader = panel.Template?.FindName("PART_HeaderButton", panel) as ToggleButton;
        var contentHost = panel.Template?.FindName("PART_ContentHost", panel) as FrameworkElement;
        var scaleTransform = panel.Template?.FindName("PART_ScaleTransform", panel) as ScaleTransform;
        Check(classicHeader != null, "经典模板 PART_HeaderButton 就位");
        Check(contentHost != null, "经典模板 PART_ContentHost 就位");
        Check(scaleTransform != null, "经典模板 PART_ScaleTransform 就位");
        Check(classicHeader != null && classicHeader.IsChecked == true, "头部 IsChecked 镜像 IsExpanded=true");

        // —— 3. Toggle + 事件 ——
        panel.Toggle();
        Check(!panel.IsExpanded && expandedCount == 0 && collapsedCount == 1, "Toggle() 折叠 + Collapsed 事件一次");
        panel.Toggle();
        Check(panel.IsExpanded && expandedCount == 1 && collapsedCount == 1, "Toggle() 展开 + Expanded 事件一次");

        // —— 4. 切卡片模式：模板切换 + 部件就位 ——
        panel.DisplayMode = ExpanderDisplayMode.Card;
        panel.ApplyTemplate();
        Pump(200);

        var cardHeader = panel.Template?.FindName("PART_HeaderButton", panel) as ToggleButton;
        var extraHost = panel.Template?.FindName("HeaderExtraHost", panel) as ContentPresenter;
        // 外层模板应用只建到头部按钮；按钮自身的内层模板在它被测量时才实例化——探针显式触发
        cardHeader?.ApplyTemplate();
        Pump(100);
        // IconHost / DescriptionHost / Chevron 在头部按钮自己的模板名称范围内，经按钮实例查找
        var iconHost = cardHeader?.Template?.FindName("IconHost", cardHeader) as ContentPresenter;
        var descriptionHost = cardHeader?.Template?.FindName("DescriptionHost", cardHeader) as ContentPresenter;
        var chevron = cardHeader?.Template?.FindName("Chevron", cardHeader) as TextBlock;
        Check(cardHeader != null, "卡片模板 PART_HeaderButton 就位");
        Check(iconHost != null && descriptionHost != null && extraHost != null && chevron != null,
            "卡片模板 IconHost/DescriptionHost/HeaderExtraHost/Chevron 就位");
        if (cardHeader == null || iconHost == null || descriptionHost == null || extraHost == null || chevron == null)
        {
            window.Close();
            return failures;
        }

        // —— 5. 标题/描述/图标槽 ——
        Check(Equals(descriptionHost.Visibility, Visibility.Collapsed), "Description 为空时说明槽折叠");
        Check(Equals(iconHost.Visibility, Visibility.Collapsed), "Icon 为空时图标槽折叠");
        panel.Description = "这是补充性说明文字";
        panel.Icon = "\uE60C";
        Pump(100);
        Check(Equals(descriptionHost.Visibility, Visibility.Visible), "Description 赋值后说明槽可见");
        Check(Equals(iconHost.Visibility, Visibility.Visible), "Icon 赋值后图标槽可见");

        // —— 6. 箭头随展开状态旋转（折叠 90° 朝下 / 展开 270° 朝上） ——
        panel.IsExpanded = false;
        Pump(100);
        var collapsedAngle = ((RotateTransform)chevron.RenderTransform).Angle;
        panel.IsExpanded = true;
        Pump(100);
        var expandedAngle = ((RotateTransform)chevron.RenderTransform).Angle;
        Check(collapsedAngle == 90, $"折叠态箭头朝下（{collapsedAngle}°）");
        Check(expandedAngle == 270, $"展开态箭头朝上（{expandedAngle}°）");

        // —— 7. 头部 IsChecked 双向绑定驱动 IsExpanded（真实点击的底层路径） ——
        cardHeader.SetCurrentValue(ToggleButton.IsCheckedProperty, false);
        Check(!panel.IsExpanded, "头部 IsChecked=false → IsExpanded 折叠");
        cardHeader.SetCurrentValue(ToggleButton.IsCheckedProperty, true);
        Check(panel.IsExpanded, "头部 IsChecked=true → IsExpanded 展开");

        // —— 8. HeaderExtra 内容送达扩展槽 ——
        var extraToggle = new ToggleButton { Content = "槽内开关" };
        panel.HeaderExtra = extraToggle;
        Pump(100);
        Check(ReferenceEquals(extraHost.Content, extraToggle), "HeaderExtra 内容送达扩展槽");

        // —— 9. 切回 Classic：卡片部件卸载 ——
        panel.DisplayMode = ExpanderDisplayMode.Classic;
        panel.ApplyTemplate();
        Pump(100);
        Check(panel.Template?.FindName("HeaderExtraHost", panel) == null, "切回 Classic 后卡片模板卸载");
        Check(panel.Template?.FindName("PART_HeaderButton", panel) is ToggleButton, "Classic 模板恢复");

        window.Close();
        Console.WriteLine($"ExpanderPanelProbe 完成：{(failures == 0 ? "全部 PASS" : failures + " 项 FAIL")}");
        return failures;
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
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }
}
