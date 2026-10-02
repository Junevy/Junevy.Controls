using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Junevy.Controls.Controls.Box;

namespace DebugProbe;

/// <summary>
/// PasswordBox 冒烟探针（隐藏窗口承载，真实走模板应用管线）：
///   1. 依赖属性默认值（RevealMode=Click / IsError=false / IsRevealed=false / Password=""）；
///   2. 模板部件就位（PART_HostPasswordBox / PART_RevealTextBox / PART_RevealButton / PART_Placeholder）；
///   3. 模板应用晚于设值：已录入密码在 OnApplyTemplate 推给掩码宿主；
///   4. Password 单向同步：程序设值 → 掩码宿主、明文宿主（绑定）同步刷新；
///   5. 明文框编辑 → 写回 Password（可编辑明文），PasswordChanged 恰好触发一次（防重入）；
///   6. 掩码宿主输入 → 写回 Password，值等价时不回写宿主（防光标重置）；
///   7. 点击切换（Click 模式按钮逻辑）；
///   8. 长按显示（PressAndHold：按下即显示、抬起即隐藏，Click 事件不切换）；
///   9. 切换 RevealMode 时收起明文；
///  10. IsError 点亮/熄灭泛红层；
///  11. 占位符：空 + 未持焦 + 未显示明文时可见，录入/聚焦/明文态隐藏；
///  12. MaxLength 传导到两个宿主；Clear() 清空。
/// 焦点搬移链路（掩码↔明文焦点跟随）依赖活动窗口的真实键盘焦点，离屏窗口不激活，
/// 无法稳定断言，实机冒烟覆盖。
/// </summary>
internal static class PasswordBoxProbe
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
            Width = 420,
            Height = 200,
            ShowInTaskbar = false,
            ShowActivated = false,
            Content = root
        };
        window.Show();
        Pump(200);

        var box = new Junevy.Controls.Controls.Box.PasswordBox { Width = 240 };
        int passwordChangedCount = 0;
        box.PasswordChanged += (_, _) => passwordChangedCount++;
        root.Children.Add(box);

        // —— 1. 默认值 ——
        Check(box.RevealMode == PasswordRevealMode.Click, "默认 RevealMode=Click");
        Check(!box.IsError, "默认 IsError=false");
        Check(!box.IsRevealed, "默认 IsRevealed=false");
        Check(box.Password == string.Empty, "默认 Password=空串");
        Check(box.PasswordChar == '●', "默认 PasswordChar=●");

        // 模板应用前先录密码，验证 OnApplyTemplate 推送
        box.Password = "Early";
        Pump(300);

        // —— 2. 模板部件就位 ——
        box.ApplyTemplate();
        Pump(100);
        var hostBox = box.Template?.FindName("PART_HostPasswordBox", box) as System.Windows.Controls.PasswordBox;
        var revealText = box.Template?.FindName("PART_RevealTextBox", box) as System.Windows.Controls.TextBox;
        var revealButton = box.Template?.FindName("PART_RevealButton", box) as System.Windows.Controls.Button;
        var placeholder = box.Template?.FindName("PART_Placeholder", box) as UIElement;
        Check(hostBox != null, "PART_HostPasswordBox 就位");
        Check(revealText != null, "PART_RevealTextBox 就位");
        Check(revealButton != null, "PART_RevealButton 就位");
        Check(placeholder != null, "PART_Placeholder 就位");
        if (hostBox == null || revealText == null || revealButton == null || placeholder == null)
        {
            window.Close();
            return failures;
        }

        // —— 3. 模板应用晚于设值：密码推给掩码宿主 ——
        Check(hostBox.Password == "Early", "模板应用后掩码宿主收到已录入密码（Early）");
        Check(revealText.Text == "Early", "明文宿主经绑定收到已录入密码（Early）");

        // —— 4. Password 单向同步 + 宿主互斥 ——
        passwordChangedCount = 0;
        box.Password = "Junevy";
        Check(hostBox.Password == "Junevy", "程序设值同步到掩码宿主（Junevy）");
        Check(revealText.Text == "Junevy", "程序设值经绑定同步到明文宿主（Junevy）");
        Check(passwordChangedCount == 1, "PasswordChanged 恰好触发一次");
        box.IsRevealed = true;
        Pump(100);
        Check(hostBox.Visibility == Visibility.Collapsed, "明文态掩码宿主折叠");
        Check(revealText.Visibility == Visibility.Visible, "明文态明文宿主可见");

        // —— 5. 明文框编辑写回（事件不重复触发） ——
        passwordChangedCount = 0;
        revealText.Text = "NewPass";
        Check(box.Password == "NewPass", "明文框编辑写回 Password（NewPass）");
        Check(hostBox.Password == "NewPass", "掩码宿主同步到明文框输入（NewPass）");
        Check(passwordChangedCount == 1, "明文框编辑 PasswordChanged 恰好触发一次（无回环）");

        // —— 6. 掩码宿主输入写回 ——
        passwordChangedCount = 0;
        hostBox.Password = "FromHost";
        Check(box.Password == "FromHost", "掩码宿主输入写回 Password（FromHost）");
        Check(revealText.Text == "FromHost", "明文宿主跟随掩码输入（FromHost）");
        Check(passwordChangedCount == 1, "掩码输入 PasswordChanged 恰好触发一次");

        // —— 7. Click 模式：点击切换（先切到 PressAndHold 再切回，触发重置链路） ——
        box.RevealMode = PasswordRevealMode.PressAndHold;
        box.RevealMode = PasswordRevealMode.Click;
        Pump(50);
        Check(!box.IsRevealed, "切回 Click 模式已收起明文");
        revealButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        Check(box.IsRevealed, "Click 模式单击显示明文");
        revealButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        Check(!box.IsRevealed, "Click 模式再次单击隐藏明文");

        // —— 8. PressAndHold：按下显示、抬起隐藏 ——
        box.RevealMode = PasswordRevealMode.PressAndHold;
        Pump(50);
        Check(!box.IsRevealed, "切换 PressAndHold 已收起明文");
        if (Mouse.PrimaryDevice != null)
        {
            var pressArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent
            };
            revealButton.RaiseEvent(pressArgs);
            Check(box.IsRevealed, "长按按下即显示明文");
            var releaseArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonUpEvent
            };
            revealButton.RaiseEvent(releaseArgs);
            Check(!box.IsRevealed, "长按松开即隐藏明文");
        }
        else
        {
            Console.WriteLine("SKIP  输入设备不可用，长按用例跳过");
        }

        // PressAndHold 模式下 Click 事件不应切换（行为分派隔离）
        revealButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        Check(!box.IsRevealed, "PressAndHold 模式忽略 Click 切换");

        // —— 10. IsError 泛红层 ——
        var errorTint = box.Template?.FindName("ErrorTint", box) as UIElement;
        Check(errorTint != null, "ErrorTint 就位");
        if (errorTint != null)
        {
            box.IsError = true;
            Pump(50);
            Check(errorTint.Visibility == Visibility.Visible, "IsError=true 泛红层点亮");
            box.IsError = false;
            Pump(50);
            Check(errorTint.Visibility == Visibility.Collapsed, "IsError=false 泛红层熄灭");
        }

        // —— 11. 占位符显隐 ——
        box.Password = string.Empty;
        Pump(50);
        Check(placeholder.Visibility == Visibility.Visible, "空密码 + 未持焦显示占位符");
        box.Password = "abc";
        Pump(50);
        Check(placeholder.Visibility == Visibility.Collapsed, "录入后占位符隐藏");
        box.Password = string.Empty;
        box.IsRevealed = true;
        Pump(50);
        Check(placeholder.Visibility == Visibility.Collapsed, "明文态占位符不显示");
        box.IsRevealed = false;
        Pump(50);
        Check(placeholder.Visibility == Visibility.Visible, "收回明文后占位符恢复");

        // —— 12. MaxLength 传导 + Clear ——
        box.MaxLength = 5;
        Check(hostBox.MaxLength == 5, "MaxLength 传导到掩码宿主");
        Check(revealText.MaxLength == 5, "MaxLength 传导到明文宿主");
        box.Password = "LeftOver";
        box.Clear();
        Check(box.Password == string.Empty && hostBox.Password == string.Empty && revealText.Text == string.Empty,
            "Clear() 三处同步清空");

        window.Close();
        Console.WriteLine($"PasswordBoxProbe 完成：{(failures == 0 ? "全部 PASS" : failures + " 项 FAIL")}");
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
