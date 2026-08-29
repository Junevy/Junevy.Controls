using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NUnit.Framework;

namespace Junevy.Controls.Tests.Toolbox;

internal static class WpfTestHost
{
    private static readonly TimeSpan DrainTimeout = TimeSpan.FromSeconds(1);

    internal static Window Show(params FrameworkElement[] elements)
    {
        var panel = new StackPanel();
        foreach (FrameworkElement element in elements)
        {
            panel.Children.Add(element);
        }

        var window = new Window
        {
            Content = panel,
            ShowActivated = false,
            ShowInTaskbar = false,
            Width = 320d,
            Height = 240d,
            WindowStyle = WindowStyle.ToolWindow
        };

        window.Show();
        window.UpdateLayout();
        PumpUntil(
            window.Dispatcher,
            () => window.IsLoaded && elements.All(element => element.IsLoaded),
            DrainTimeout);

        return window;
    }

    internal static void Drain(Dispatcher dispatcher)
    {
        var frame = new DispatcherFrame();
        DispatcherOperation operation = dispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle,
            new Action(() => frame.Continue = false));
        var timeout = CreateStopTimer(dispatcher, frame, DrainTimeout);

        timeout.Start();
        Dispatcher.PushFrame(frame);
        timeout.Stop();

        Assert.That(operation.Status, Is.Not.EqualTo(DispatcherOperationStatus.Pending),
            "The Dispatcher did not drain before the bounded timeout.");
    }

    internal static void PumpFor(Dispatcher dispatcher, TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = CreateStopTimer(dispatcher, frame, duration);

        timer.Start();
        Dispatcher.PushFrame(frame);
        timer.Stop();
    }

    internal static void PumpUntil(Dispatcher dispatcher, Func<bool> condition, TimeSpan timeout)
    {
        if (condition())
        {
            return;
        }

        var frame = new DispatcherFrame();
        var poll = new DispatcherTimer(DispatcherPriority.ApplicationIdle, dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(1)
        };
        var timeoutTimer = CreateStopTimer(dispatcher, frame, timeout);

        poll.Tick += (_, _) =>
        {
            if (condition())
            {
                frame.Continue = false;
            }
        };

        poll.Start();
        timeoutTimer.Start();
        Dispatcher.PushFrame(frame);
        poll.Stop();
        timeoutTimer.Stop();

        Assert.That(condition(), Is.True, "The WPF condition did not become true before the bounded timeout.");
    }

    internal static void CloseAndDrain(Window? window)
    {
        if (window is null)
        {
            return;
        }

        Dispatcher dispatcher = window.Dispatcher;
        window.Close();
        Drain(dispatcher);
    }

    private static DispatcherTimer CreateStopTimer(
        Dispatcher dispatcher,
        DispatcherFrame frame,
        TimeSpan duration)
    {
        var timer = new DispatcherTimer(DispatcherPriority.Send, dispatcher)
        {
            Interval = duration > TimeSpan.Zero ? duration : TimeSpan.FromMilliseconds(1)
        };
        timer.Tick += (_, _) => frame.Continue = false;
        return timer;
    }
}
