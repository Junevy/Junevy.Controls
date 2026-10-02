namespace DebugProbe;

internal static class Program
{
    // STAThread 对 async Main 无效（编译器警告 STATTHREAD 忽略），保持同步入口；
    // Roslyn 探针在 provider 内部全为 ConfigureAwait(false)，阻塞等待不会死锁
    [STAThread]
    private static int Main(string[] args)
    {
        int failures = CodeEditorProbe.Run();
        failures += RoslynCompletionProbe.RunAsync().GetAwaiter().GetResult();
        failures += SmoothScrollProbe.Run();
        failures += PasswordBoxProbe.Run();
        failures += ExpanderPanelProbe.Run();
        return failures == 0 ? 0 : 1;
    }
}
