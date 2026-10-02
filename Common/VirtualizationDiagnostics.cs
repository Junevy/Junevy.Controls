using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Common;

/// <summary>
/// 虚拟化失效诊断（仅 DEBUG 构建生效，Release 下调用点整体编译剔除）。
/// <para>
/// 列表/表格控件的条目虚拟化依赖"测量约束在滚动方向上有界"：当约束为无穷
/// （典型于无定高嵌套在 <see cref="StackPanel"/> 中，或被未定高的外层
/// <see cref="ScrollViewer"/> 直接承载）时，所有条目会被一次性实例化，
/// 滚动性能与内存占用都会显著劣化。此诊断在测量阶段捕获该状态并输出一次性警告：
/// 不抛异常、不打断布局——"合法但低效"的用法由开发者按提示自行修正。
/// </para>
/// </summary>
internal static class VirtualizationDiagnostics
{
    private sealed class WarnState
    {
        /// <summary>当前是否已处于"已告警"状态；恢复有界后复位，允许下次再次告警。</summary>
        public bool Warned;
    }

    private static readonly ConditionalWeakTable<FrameworkElement, WarnState> WarnStates = new();

    /// <summary>
    /// 检查本次测量的滚动方向约束是否为无穷，为无穷时输出一次诊断警告；
    /// 恢复有界后重新武装，下次再次进入无穷约束会再度告警。
    /// </summary>
    /// <param name="owner">被测量的控件，仅用于输出类型名。</param>
    /// <param name="scrollOrientation">滚动方向：竖向列表检查高度约束，横向列表检查宽度约束。</param>
    /// <param name="availableSize">MeasureOverride 收到的可用尺寸。</param>
    [Conditional("DEBUG")]
    internal static void ReportIfUnbounded(FrameworkElement owner, Orientation scrollOrientation, Size availableSize)
    {
        bool vertical = scrollOrientation == Orientation.Vertical;
        bool unbounded = vertical ? double.IsInfinity(availableSize.Height) : double.IsInfinity(availableSize.Width);

        WarnState state = WarnStates.GetOrCreateValue(owner);
        if (!unbounded)
        {
            state.Warned = false;
            return;
        }

        if (state.Warned)
        {
            return;
        }

        state.Warned = true;
        Debug.WriteLine(
            $"Junevy.Controls：{owner.GetType().Name} 在{(vertical ? "高度" : "宽度")}方向收到无约束（∞）测量，" +
            "条目虚拟化已失效（全部条目将被一次性实例化，滚动变卡、内存升高）。" +
            "请为控件设置 Height/MaxHeight，或避免将其无定高地嵌套在 StackPanel / 外层 ScrollViewer 中。",
            "Junevy.Controls.Virtualization");
    }
}
