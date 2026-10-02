using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// 平滑滚动附加属性集：把鼠标滚轮的离散跳变改为连续的补间滚动。
    /// <para>
    /// WPF 的滚轮滚动没有任何补间，一个刻度就是一次瞬移；列表类控件在按项滚动下
    /// 还会把偏移吸附到条目边界，观感上就是"从第 1 项直接跳到第 2 项"。本附加属性
    /// 接管 <see cref="ScrollViewer"/> 的滚轮输入，把目标偏移交给逐帧渲染回调以
    /// 指数趋近的方式收敛过去（与浏览器平滑滚动同款手感：连续滚动时目标累加、
    /// 速度连续，不存在逐格重启动画的脉冲感）；步长默认跟随系统"滚轮滚动行数"
    /// 折算，也可经 <see cref="Step"/> 指定每刻度像素数。补间期间新的滚轮输入会从
    /// 当前目标继续累计；拖动滑块、点击箭头/轨道、按下鼠标、键盘滚动等用户直接
    /// 操作会立即停止补间、交还控制权。
    /// </para>
    /// <para>
    /// 接管遵循"就近滚动宿主"原则：响应前沿可视树从事件源向上检查，途中若存在
    /// 更内层的 <see cref="ScrollViewer"/>（外层 ScrollViewer 内嵌 DataGrid/ListView、
    /// 条目模板自带滚动区等），则放手交给内层——任意嵌套下永远只有离鼠标最近的
    /// 滚动宿主响应滚轮；内层滚到边界时不接力外层，均与 WPF 原生语义一致。
    /// </para>
    /// <para>
    /// 连续补间要求像素级偏移：列表类控件应同时设置
    /// <c>VirtualizingPanel.ScrollUnit="Pixel"</c> 与
    /// <c>VirtualizingPanel.CacheLength="1,1"</c>（本库 ListBox/ListView/DataGrid/
    /// SideMenu 默认样式已内置）——前者避免边界吸附，后者把容器生成提前到滚动
    /// 到达之前，消除补间中的实体化顿挫；按项滚动时本行为自动退回 WPF 原生滚轮。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 用法：设在任何可滚动控件或 <see cref="ScrollViewer"/> 上
    /// （<c>atc:SmoothScrolling.IsEnabled="True"</c>）。设在控件上时，其模板须按库内约定
    /// 命名滚动宿主部件 <c>PART_ScrollViewer</c>；模板缺失或未按约定命名时保持 WPF
    /// 原生行为（与 <see cref="Junevy.Controls.Controls.Box.HorizontalWheelScrolling"/>
    /// 的部件约定一致）。本库 ListBox / ListView / DataGrid / ComboBox / SideMenu
    /// 经默认样式启用，实例上设为 <c>False</c> 可单独关闭。
    /// </remarks>
    public class SmoothScrolling
    {
        /// <summary>
        /// 模板中承载滚动的 <see cref="ScrollViewer"/> 部件名，与本库各控件模板约定一致。
        /// </summary>
        private const string ScrollViewerPartName = "PART_ScrollViewer";

        /// <summary>一个滚轮刻度对应的角度增量（Win32 的 WHEEL_DELTA）。</summary>
        internal const double WheelNotch = 120.0;

        /// <summary>像素滚动模式下一个文本行的折算高度，与 WPF 像素滚轮的换算基准一致。</summary>
        internal const double PixelLineHeight = 16.0;

        /// <summary>偏移比较容差。</summary>
        private const double OffsetEpsilon = 0.01;

        /// <summary>补间收敛阈值：距目标不足半像素时直接落定并停止逐帧驱动。</summary>
        private const double SnapEpsilon = 0.5;

        /// <summary>
        /// 指数趋近时间常数（毫秒）：每帧向目标收敛"剩余距离 × (1 - e^(-dt/τ))"。
        /// 取 70ms 时单格约 200ms 内到位，连续滚动无脉冲感；值越小越跟手、越大越绵。
        /// </summary>
        private const double SmoothingTauMilliseconds = 70.0;

        /// <summary>标识 <see cref="GetIsEnabled"/> / <see cref="SetIsEnabled"/> 附加属性。</summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(SmoothScrolling),
                new PropertyMetadata(false, OnIsEnabledChanged));

        /// <summary>
        /// 标识 <see cref="GetStep"/> / <see cref="SetStep"/> 附加属性。
        /// </summary>
        public static readonly DependencyProperty StepProperty =
            DependencyProperty.RegisterAttached(
                "Step",
                typeof(double),
                typeof(SmoothScrolling),
                new PropertyMetadata(double.NaN));

        /// <summary>
        /// 每个被挂接 <see cref="ScrollViewer"/> 一份的补间运行状态。
        /// </summary>
        private sealed class WheelState
        {
            /// <summary>承载 IsEnabled 的宿主（控件或 ScrollViewer 自身），<see cref="Step"/> 的读取源。</summary>
            public DependencyObject Host = null!;

            /// <summary>逐帧渲染回调是否已挂接（即补间进行中）。</summary>
            public bool IsAnimating;

            /// <summary>补间目标偏移；连续滚轮在新目标上继续累加。</summary>
            public double TargetVertical;
            public double TargetHorizontal;

            /// <summary>挂接到 <see cref="CompositionTarget.Rendering"/> 的回调（卸载用）。</summary>
            public EventHandler? RenderingHandler;

            /// <summary>上一帧的渲染时间戳：用于帧间 dt 计算与同帧重复回调去重。</summary>
            public TimeSpan? LastFrameTime;

            // —— DEBUG 遥测状态（Release 不写入）——

            /// <summary>上一帧命令的目标/偏移：用于识别"目标未变而偏移被外部移动"（回弹/抢滚轮信号）。</summary>
            public double LastTargetVertical;
            public double LastCommandedVertical;
            public double LastTargetHorizontal;
            public double LastCommandedHorizontal;
        }

        /// <summary>标识 <see cref="WheelState"/> 的挂接标记（内部）。</summary>
        private static readonly DependencyProperty WheelStateProperty =
            DependencyProperty.RegisterAttached(
                "WheelState",
                typeof(WheelState),
                typeof(SmoothScrolling),
                new PropertyMetadata(null));

        /// <summary>
        /// 读取指定对象上是否启用平滑滚动。
        /// </summary>
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        /// <summary>
        /// 在指定对象上启用/关闭平滑滚动。
        /// </summary>
        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        /// <summary>
        /// 读取每个滚轮刻度的滚动像素数；未设置时为 NaN，表示跟随系统设置折算。
        /// </summary>
        public static double GetStep(DependencyObject obj)
        {
            return (double)obj.GetValue(StepProperty);
        }

        /// <summary>
        /// 设置每个滚轮刻度的滚动像素数；设回 NaN 恢复跟随系统。
        /// </summary>
        public static void SetStep(DependencyObject obj, double value)
        {
            obj.SetValue(StepProperty, value);
        }

        /// <summary>
        /// IsEnabled 变更：设在 <see cref="ScrollViewer"/> 上直接挂接自身；
        /// 设在控件上则解析模板中的滚动宿主部件。
        /// </summary>
        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer viewer)
            {
                if ((bool)e.NewValue)
                {
                    HookViewer(viewer, viewer);
                }
                else
                {
                    UnhookViewer(viewer);
                }

                return;
            }

            if (d is FrameworkElement host)
            {
                if ((bool)e.NewValue)
                {
                    AttachToHost(host);
                }
                else
                {
                    DetachFromHost(host);
                }
            }
        }

        /// <summary>
        /// 在宿主模板中解析滚动宿主部件并挂接；模板尚未应用（样式 setter 的生效时机
        /// 早于模板应用）时延迟到 <see cref="FrameworkElement.Loaded"/> 再解析一次。
        /// </summary>
        /// <remarks>
        /// 只有 <see cref="Control"/> 携带模板；其他 FrameworkElement 宿主上设置本属性
        /// 不会生效（应在 <see cref="ScrollViewer"/> 上直接设置）。
        /// </remarks>
        private static void AttachToHost(FrameworkElement host)
        {
            //  Template 是 Control 的属性：非 Control 宿主（不携带模板）视同模板缺失，保持原生滚动
            if (host is Control templatedHost && templatedHost.Template?.FindName(ScrollViewerPartName, host) is ScrollViewer viewer)
            {
                HookViewer(viewer, host);
                return;
            }

            if (!host.IsLoaded)
            {
                host.Loaded += OnHostLoaded;
            }
        }

        /// <summary>宿主就绪后的延迟挂接入口。</summary>
        private static void OnHostLoaded(object sender, RoutedEventArgs e)
        {
            var host = (FrameworkElement)sender;
            host.Loaded -= OnHostLoaded;
            if (GetIsEnabled(host))
            {
                AttachToHost(host);
            }
        }

        /// <summary>从宿主模板解析滚动宿主部件并解除挂接。</summary>
        private static void DetachFromHost(FrameworkElement host)
        {
            if (host is Control templatedHost && templatedHost.Template?.FindName(ScrollViewerPartName, host) is ScrollViewer viewer)
            {
                UnhookViewer(viewer);
            }
        }

        /// <summary>
        /// 挂接到滚动宿主。重复挂接安全，并遵循宿主优先级：控件经模板解析的挂接
        /// （宿主 ≠ 滚动宿主自身）可刷新宿主引用；自挂接（如应用级隐式样式直接命中
        /// 控件模板内部的 ScrollViewer）不得覆盖控件已建立的宿主——
        /// <see cref="UsesPixelScrolling"/> 与 <see cref="Step"/> 都按宿主解析，
        /// 宿主被自挂接夺走会让滚动单位判定失效、平滑退化为原生。
        /// </summary>
        private static void HookViewer(ScrollViewer viewer, DependencyObject host)
        {
            if (viewer.GetValue(WheelStateProperty) is WheelState existing)
            {
                if (!ReferenceEquals(host, viewer))
                {
                    existing.Host = host;
                }

                return;
            }

            var state = new WheelState { Host = host };
            viewer.SetValue(WheelStateProperty, state);
            viewer.PreviewMouseWheel += OnViewerPreviewMouseWheel;
            viewer.AddHandler(ScrollBar.ScrollEvent, new ScrollEventHandler(OnScrollBarScroll), handledEventsToo: true);
            viewer.PreviewKeyDown += OnViewerPreviewKeyDown;
            viewer.PreviewMouseLeftButtonDown += OnViewerPreviewMouseLeftButtonDown;
        }

        /// <summary>解除挂接并复位补间状态。</summary>
        private static void UnhookViewer(ScrollViewer viewer)
        {
            if (viewer.GetValue(WheelStateProperty) is not WheelState state)
            {
                return;
            }

            CancelSmoothing(viewer, state);
            viewer.PreviewMouseWheel -= OnViewerPreviewMouseWheel;
            viewer.RemoveHandler(ScrollBar.ScrollEvent, new ScrollEventHandler(OnScrollBarScroll));
            viewer.PreviewKeyDown -= OnViewerPreviewKeyDown;
            viewer.PreviewMouseLeftButtonDown -= OnViewerPreviewMouseLeftButtonDown;
            viewer.SetValue(WheelStateProperty, null);
        }

        /// <summary>
        /// 滚轮接管入口（预览阶段）：就近滚动宿主检查通过后，把刻度累加进补间目标。
        /// </summary>
        /// <remarks>
        /// 用预览阶段的原因：ScrollViewer 自身的 <c>OnMouseWheel</c> 类处理器在冒泡阶段
        /// 先于实例处理器执行并瞬时滚动，只有预览接管才能抢在它前面；接管后标记事件
        /// 已处理，避免原生路径叠加一次瞬时滚动。不可滚动时不动手也不吞事件，交回
        /// WPF 原生行为，条目内部依赖 PreviewMouseWheel 的自定义处理不受影响。
        /// </remarks>
        private static void OnViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            var viewer = (ScrollViewer)sender;
            if (!IsNearestScrollOwner(viewer, e.OriginalSource as DependencyObject))
            {
                return;
            }

            if (viewer.GetValue(WheelStateProperty) is not WheelState state)
            {
                return;
            }

            double notches = e.Delta / WheelNotch;
            bool pixelScrolling = UsesPixelScrolling(viewer, state);
            if (viewer.ScrollableHeight > 0.0 && pixelScrolling)
            {
                AccumulateTarget(viewer, state, true, notches);
                e.Handled = true;
            }
        }

        /// <summary>
        /// 判断滚动宿主当前是否按像素滚动：未启用逻辑滚动（CanContentScroll=false）时
        /// 天然是像素滚动；启用逻辑滚动时由虚拟化面板的 ScrollUnit 决定偏移单位。
        /// 补间要求像素级偏移，按项滚动（ScrollUnit=Item）下偏移被吸附到条目边界、
        /// 步长折算单位也不符，此时不接管，交回 WPF 原生行为（本库列表样式默认已配 Pixel）。
        /// </summary>
        private static bool UsesPixelScrolling(ScrollViewer viewer, WheelState state)
        {
            if (!viewer.CanContentScroll)
            {
                return true;
            }

            return state.Host is ItemsControl itemsControl
                && VirtualizingPanel.GetScrollUnit(itemsControl) == ScrollUnit.Pixel;
        }

        /// <summary>
        /// 判断 <paramref name="viewer"/> 是否为事件源处"离鼠标最近的滚动宿主"：
        /// 沿可视树从事件源向上走到本 <paramref name="viewer"/>，途中遇到任何其他
        /// <see cref="ScrollViewer"/> 即说明鼠标下方有更内层的滚动宿主，本层应放手。
        /// </summary>
        /// <remarks>
        /// 事件源不在本 <paramref name="viewer"/> 子树内（如独立 Popup 内容）或为
        /// 非可视元素（如 Hyperlink）时返回 false，交回 WPF 原生处理。
        /// </remarks>
        private static bool IsNearestScrollOwner(ScrollViewer viewer, DependencyObject? source)
        {
            if (source == null)
            {
                // 与 HorizontalWheelScrolling 的约定一致：源未知时视为本层
                return true;
            }

            for (DependencyObject? current = source; current != null; current = GetVisualParent(current))
            {
                if (ReferenceEquals(current, viewer))
                {
                    return true;
                }

                if (current is ScrollViewer)
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>可视树父级；非可视元素（ContentElement 等）不可经可视树向上遍历，返回 null。</summary>
        private static DependencyObject? GetVisualParent(DependencyObject node)
        {
            return node is Visual || node is System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(node)
                : null;
        }

        /// <summary>
        /// 把滚轮刻度折算成目标偏移增量并累加进补间目标，随后确保逐帧驱动已挂接。
        /// 目标从"当前补间目标"（补间中）或"当前实际偏移"（静止）起算，连续滚轮自然累加。
        /// </summary>
        private static void AccumulateTarget(ScrollViewer viewer, WheelState state, bool vertical, double notches)
        {
            double step = ResolveStep(viewer, state, vertical);
            double current = vertical ? viewer.VerticalOffset : viewer.HorizontalOffset;
            double baseTarget = state.IsAnimating ? (vertical ? state.TargetVertical : state.TargetHorizontal) : current;
            double max = vertical ? viewer.ScrollableHeight : viewer.ScrollableWidth;
            double target = Math.Max(0.0, Math.Min(max, baseTarget - (notches * step)));

            if (Math.Abs(target - (vertical ? state.TargetVertical : state.TargetHorizontal)) < OffsetEpsilon
                && state.IsAnimating)
            {
                // 目标无变化（已在边界）：保持接管语义（事件已吞掉），无需重启驱动
                return;
            }

            if (vertical)
            {
                state.TargetVertical = target;
            }
            else
            {
                state.TargetHorizontal = target;
            }

            EnsureRenderingAttached(viewer, state);
        }

        /// <summary>
        /// 解析每刻度步长：显式 <see cref="Step"/> 优先；否则跟随系统"滚轮滚动行数"。
        /// </summary>
        private static double ResolveStep(ScrollViewer viewer, WheelState state, bool vertical)
        {
            double configured = GetStep(state.Host);
            if (!double.IsNaN(configured) && configured > 0.0)
            {
                return configured;
            }

            return ResolveAutoStep(viewer, vertical);
        }

        /// <summary>
        /// 自动步长：跟随系统"滚轮滚动行数"（N 行 × 文本行高，与 WPF 像素滚轮一致），
        /// 系统设为"一次滚动一页"（行数 ≤ 0）时按一个视口计。
        /// </summary>
        private static double ResolveAutoStep(ScrollViewer viewer, bool vertical)
        {
            double lines = SystemParameters.WheelScrollLines;
            if (lines > 0)
            {
                return lines * PixelLineHeight;
            }

            return vertical ? viewer.ViewportHeight : viewer.ViewportWidth;
        }

        /// <summary>
        /// 以滚轮增量驱动竖向滚动。供"内层宿主无法纵向消费滚轮、需要交还外层"的场景复用
        /// （如代码块、标尺条等纯横向宿主把竖向滚轮转交给页面）。
        /// </summary>
        /// <remarks>
        /// 与真实滚轮共用同一条路径：宿主已挂接（行为启用）时按平滑步长累加补间目标
        /// （连续转发与真实滚轮混用都不会互相拉扯），否则按同一折算瞬时滚动。
        /// </remarks>
        /// <param name="viewer">接收滚轮的外层滚动宿主。</param>
        /// <param name="delta">原始滚轮增量（<see cref="MouseWheelEventArgs.Delta"/>，向下为负）。</param>
        public static void ScrollByWheel(ScrollViewer viewer, double delta)
        {
            if (viewer.GetValue(WheelStateProperty) is not WheelState state)
            {
                double autoStep = ResolveAutoStep(viewer, true);
                double target = viewer.VerticalOffset - (delta / WheelNotch * autoStep);
                viewer.ScrollToVerticalOffset(Math.Max(0.0, Math.Min(viewer.ScrollableHeight, target)));
                return;
            }

            AccumulateTarget(viewer, state, true, delta / WheelNotch);
        }

        /// <summary>
        /// 确保逐帧渲染回调已挂接；挂接时初始化帧时间戳。
        /// </summary>
        private static void EnsureRenderingAttached(ScrollViewer viewer, WheelState state)
        {
            if (state.IsAnimating)
            {
                return;
            }

            state.IsAnimating = true;
            state.LastFrameTime = null;
            EventHandler handler = (sender, args) => Tick(viewer, state, args);
            state.RenderingHandler = handler;
            CompositionTarget.Rendering += handler;
        }

        /// <summary>
        /// 逐帧驱动：当前偏移按"剩余距离 × (1 - e^(-dt/τ))"向目标指数趋近。
        /// 指数趋近的速度曲线连续（不存在逐格重启动画的速度重置），连续滚动无脉冲感；
        /// 两轴均收敛到阈值内后落定并自行摘除回调，不占用空闲帧。
        /// </summary>
        private static void Tick(ScrollViewer viewer, WheelState state, EventArgs args)
        {
            // 同一帧可能收到重复回调（特定渲染配置下 Rendering 每帧触发多次），按时间戳去重
            TimeSpan frameTime = args is RenderingEventArgs rendering ? rendering.RenderingTime : default;
            double frameMilliseconds = 16.7;
            if (state.LastFrameTime is TimeSpan last)
            {
                double delta = (frameTime - last).TotalMilliseconds;
                if (delta <= 0)
                {
                    return;
                }

                frameMilliseconds = Math.Min(100.0, Math.Max(1.0, delta));
            }

            state.LastFrameTime = frameTime;

            bool verticalSettled = TickAxis(viewer, state, true, frameMilliseconds);
            bool horizontalSettled = TickAxis(viewer, state, false, frameMilliseconds);
            if (verticalSettled && horizontalSettled)
            {
                DetachRendering(viewer, state);
            }
        }

        /// <summary>单轴趋近；返回该轴是否已收敛（收敛时把偏移精确落到目标上）。</summary>
        private static bool TickAxis(ScrollViewer viewer, WheelState state, bool vertical, double frameMilliseconds)
        {
            double current = vertical ? viewer.VerticalOffset : viewer.HorizontalOffset;
            double target = vertical ? state.TargetVertical : state.TargetHorizontal;
            TraceExternalMove(state, vertical, target, current);
            double remaining = target - current;
            if (Math.Abs(remaining) < SnapEpsilon)
            {
                if (Math.Abs(remaining) >= OffsetEpsilon)
                {
                    SetOffset(viewer, vertical, target);
                }

                TraceCommanded(state, vertical, target, Math.Abs(remaining) >= OffsetEpsilon ? target : current);
                return true;
            }

            // 指数趋近：步长与剩余距离成正比、与帧时长相关，帧率波动下收敛速度一致
            double factor = 1.0 - Math.Exp(-frameMilliseconds / SmoothingTauMilliseconds);
            double next = current + (remaining * factor);
            double max = vertical ? viewer.ScrollableHeight : viewer.ScrollableWidth;
            next = Math.Max(0.0, Math.Min(max, next));
            SetOffset(viewer, vertical, next);
            TraceCommanded(state, vertical, target, next);
            return false;
        }

        /// <summary>
        /// DEBUG 诊断：补间中目标未变而实际偏移朝远离目标方向移动（超过 1px），说明存在
        /// 外部滚动源（BringIntoView、程序滚动、第三方滚轮处理等）在补间窗口内改写偏移——
        /// 这是"回弹/抢滚轮"类问题的直接信号，输出细节辅助定位。
        /// </summary>
        [Conditional("DEBUG")]
        private static void TraceExternalMove(WheelState state, bool vertical, double target, double current)
        {
            double lastTarget = vertical ? state.LastTargetVertical : state.LastTargetHorizontal;
            double lastCommanded = vertical ? state.LastCommandedVertical : state.LastCommandedHorizontal;
            if (Math.Abs(target - lastTarget) < OffsetEpsilon
                && Math.Abs(target - current) > Math.Abs(target - lastCommanded) + 1.0)
            {
                Debug.WriteLine(
                    $"Junevy.Controls：补间期间偏移被外部移动（轴={(vertical ? "竖向" : "横向")}，" +
                    $"目标={target:F1}，上一帧命令={lastCommanded:F1}，当前实际={current:F1}）。" +
                    "可能来源：BringIntoView（选中项/焦点变化）、程序滚动、第三方滚轮处理。",
                    "Junevy.Controls.SmoothScrolling");
            }
        }

        /// <summary>DEBUG 诊断：记录每帧命令的目标/偏移，供 <see cref="TraceExternalMove"/> 比对。</summary>
        [Conditional("DEBUG")]
        private static void TraceCommanded(WheelState state, bool vertical, double target, double commanded)
        {
            if (vertical)
            {
                state.LastTargetVertical = target;
                state.LastCommandedVertical = commanded;
            }
            else
            {
                state.LastTargetHorizontal = target;
                state.LastCommandedHorizontal = commanded;
            }
        }

        /// <summary>命令真实偏移。</summary>
        private static void SetOffset(ScrollViewer viewer, bool vertical, double value)
        {
            if (vertical)
            {
                viewer.ScrollToVerticalOffset(value);
            }
            else
            {
                viewer.ScrollToHorizontalOffset(value);
            }
        }

        /// <summary>
        /// 横向滚动的统一出口：宿主已挂接（行为启用）时把目标偏移交给逐帧补间，
        /// 否则瞬时定位。供 <see cref="Junevy.Controls.Controls.Box.HorizontalWheelScrolling"/>
        /// 复用，保证横向退化路径与竖向滚轮的手感一致。
        /// </summary>
        internal static void ScrollToHorizontalOffset(ScrollViewer viewer, double target)
        {
            double clamped = Math.Max(0.0, Math.Min(viewer.ScrollableWidth, target));
            if (viewer.GetValue(WheelStateProperty) is not WheelState state)
            {
                viewer.ScrollToHorizontalOffset(clamped);
                return;
            }

            if (Math.Abs(clamped - state.TargetHorizontal) < OffsetEpsilon && state.IsAnimating)
            {
                return;
            }

            state.TargetHorizontal = clamped;
            EnsureRenderingAttached(viewer, state);
        }

        /// <summary>
        /// 用户直接操作滚动条（拖动滑块、点击箭头/轨道、滚动宿主上按方向键）时立即交还控制权。
        /// </summary>
        /// <remarks>
        /// 以 handledEventsToo 注册：ScrollViewer 自身对 Scroll 事件的处理不会拦截本监听。
        /// 本类自身的补间只经 <see cref="ScrollViewer.ScrollToVerticalOffset"/> 命令偏移，
        /// 不会引发 <see cref="ScrollBar.ScrollEvent"/>，因此不存在误打断。
        /// </remarks>
        private static void OnScrollBarScroll(object sender, ScrollEventArgs e)
        {
            var viewer = (ScrollViewer)sender;
            if (viewer.GetValue(WheelStateProperty) is WheelState state && state.IsAnimating)
            {
                TraceCancel("ScrollBar.Scroll（拖动滑块/箭头/轨道/滚动宿主键盘）");
                CancelSmoothing(viewer, state);
            }
        }

        /// <summary>补间期间按键（方向键/翻页移动选中项等）时立即交还控制权。</summary>
        private static void OnViewerPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var viewer = (ScrollViewer)sender;
            if (viewer.GetValue(WheelStateProperty) is WheelState state && state.IsAnimating)
            {
                TraceCancel("按键（键盘导航/BringIntoView 前兆）");
                CancelSmoothing(viewer, state);
            }
        }

        /// <summary>
        /// 补间期间按下鼠标（点击条目触发 BringIntoView、抓取滑块等）时立即交还控制权。
        /// </summary>
        private static void OnViewerPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var viewer = (ScrollViewer)sender;
            if (viewer.GetValue(WheelStateProperty) is WheelState state && state.IsAnimating)
            {
                TraceCancel("按下鼠标（点击条目/抓取滑块）");
                CancelSmoothing(viewer, state);
            }
        }

        /// <summary>DEBUG 诊断：记录补间被何种用户操作打断。</summary>
        [Conditional("DEBUG")]
        private static void TraceCancel(string source)
        {
            Debug.WriteLine($"Junevy.Controls：平滑滚动被用户操作打断（{source}）。", "Junevy.Controls.SmoothScrolling");
        }

        /// <summary>
        /// 停止补间：摘除逐帧回调即可——偏移本身不再被任何动画时钟持有，
        /// 天然冻结在当前实际位置，不存在回跳或跳到作废目标的问题。
        /// </summary>
        private static void CancelSmoothing(ScrollViewer viewer, WheelState state)
        {
            DetachRendering(viewer, state);
        }

        /// <summary>摘除逐帧渲染回调并复位状态。</summary>
        private static void DetachRendering(ScrollViewer viewer, WheelState state)
        {
            if (state.RenderingHandler != null)
            {
                CompositionTarget.Rendering -= state.RenderingHandler;
                state.RenderingHandler = null;
            }

            state.IsAnimating = false;
            state.LastFrameTime = null;
        }
    }
}
