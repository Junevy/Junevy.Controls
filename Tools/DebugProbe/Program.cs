namespace DebugProbe;

internal static class Program
{
    // STAThread 对 async Main 无效（编译器警告 STATTHREAD 忽略），保持同步入口；
    // Roslyn 探针在 provider 内部全为 ConfigureAwait(false)，阻塞等待不会死锁
    [STAThread]
    private static int Main(string[] args)
    {
        // LabelProbe 最先跑：自建 Application + OnExplicitShutdown（后续探针关窗不会连带关闭应用），
        // 主题刷子断言依赖可用的 Application 资源查找；CodeEditorProbe 复用该实例。
        // 任何探针先关光窗口都会令 OnLastWindowClose 关闭应用，pack URI 随之失效且实例无法重建。
        int failures = LabelProbe.Run();
        failures += CodeEditorProbe.Run();
        failures += RoslynCompletionProbe.RunAsync().GetAwaiter().GetResult();
        failures += SmoothScrollProbe.Run();
        failures += PasswordBoxProbe.Run();
        failures += ExpanderPanelProbe.Run();
        return failures == 0 ? 0 : 1;
    }
}
