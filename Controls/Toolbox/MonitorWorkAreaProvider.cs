using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Toolbox;

internal static class MonitorWorkAreaProvider
{
    private const uint MonitorDefaultToNearest = 0x00000002;

    internal static Rect GetWorkAreaDip(Visual target)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        Window? window = Window.GetWindow(target);
        if (window is null)
        {
            return GetTargetBounds(target);
        }

        IntPtr windowHandle = new WindowInteropHelper(window).Handle;
        if (windowHandle == IntPtr.Zero)
        {
            return GetWindowBounds(window);
        }

        IntPtr monitor = MonitorFromWindow(windowHandle, MonitorDefaultToNearest);
        var monitorInfo = new MonitorInfo { Size = Marshal.SizeOf(typeof(MonitorInfo)) };
        if (monitor == IntPtr.Zero || !GetMonitorInfo(monitor, ref monitorInfo))
        {
            return GetWindowBounds(window);
        }

        PresentationSource? source = PresentationSource.FromVisual(target);
        if (source?.CompositionTarget is null)
        {
            return GetWindowBounds(window);
        }

        Matrix fromDevice = source.CompositionTarget.TransformFromDevice;
        Point topLeft = fromDevice.Transform(
            new Point(monitorInfo.WorkArea.Left, monitorInfo.WorkArea.Top));
        Point bottomRight = fromDevice.Transform(
            new Point(monitorInfo.WorkArea.Right, monitorInfo.WorkArea.Bottom));
        return new Rect(topLeft, bottomRight);
    }

    private static Rect GetWindowBounds(Window window)
    {
        double width = window.ActualWidth > 0d ? window.ActualWidth : window.Width;
        double height = window.ActualHeight > 0d ? window.ActualHeight : window.Height;
        return new Rect(window.Left, window.Top, Math.Max(0d, width), Math.Max(0d, height));
    }

    private static Rect GetTargetBounds(Visual target)
    {
        if (target is FrameworkElement element)
        {
            return new Rect(0d, 0d, element.ActualWidth, element.ActualHeight);
        }

        return Rect.Empty;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr windowHandle, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo monitorInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfo
    {
        internal int Size;
        internal NativeRect Monitor;
        internal NativeRect WorkArea;
        internal uint Flags;
    }
}
