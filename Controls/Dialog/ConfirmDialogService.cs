using System.Windows;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Dialog
{
    /// <summary>
    /// 确认对话框的静态入口，参考 WPF-UI 的静态服务写法与库内 <c>MessageBarService</c>：
    /// 在任意位置 <c>await ConfirmDialogService.ShowAsync(...)</c> 即可弹出一个模态确认框并拿到操作结果，
    /// 无需自己 new 窗口、无需设 <c>Owner</c>、也无需关心是否在 UI 线程。
    ///
    /// <para>与 <c>MessageBarService</c> 的差别：那边需要先 <c>SetPresenter</c> 注册一个宿主元素，
    /// 因为通知条必须挂在窗口布局里；对话框是独立顶层窗口，Owner 由本服务按当前活动窗口自动推断，
    /// 因此没有注册步骤。</para>
    ///
    /// <para>Owner 推断口径：优先取当前活动窗口（跳过本身是对话框的窗口），取不到退回
    /// <see cref="Application.MainWindow"/>；两者都没有则不设 Owner（窗口居屏幕显示）。</para>
    /// </summary>
    public static class ConfirmDialogService
    {
        /// <summary>以「确定 + 取消」按钮组合显示一条确认消息。</summary>
        /// <param name="message">消息正文。</param>
        public static Task<ConfirmDialogResult> ShowAsync(string message)
            => ShowAsync(string.Empty, message, ConfirmDialogButtons.OkCancel);

        /// <summary>显示一条带标题的确认消息。</summary>
        /// <param name="title">窗口标题。</param>
        /// <param name="message">消息正文。</param>
        public static Task<ConfirmDialogResult> ShowAsync(string title, string message)
            => ShowAsync(title, message, ConfirmDialogButtons.OkCancel);

        /// <summary>显示一条带标题、指定按钮组合的确认消息。</summary>
        /// <param name="title">窗口标题。</param>
        /// <param name="message">消息正文。</param>
        /// <param name="buttons">按钮组合。</param>
        /// <returns>操作结果：点「确定」为 <see cref="ConfirmDialogResult.Confirm"/>；
        /// 点「取消」或以 ✕ / <c>Esc</c> / <c>Alt+F4</c> 关闭为 <see cref="ConfirmDialogResult.Cancel"/>。</returns>
        /// <exception cref="InvalidOperationException">
        /// 尚未创建 <see cref="Application"/>（在 WPF 应用程序初始化之前调用）。
        /// </exception>
        public static Task<ConfirmDialogResult> ShowAsync(string title, string message, ConfirmDialogButtons buttons)
        {
            Application application = Application.Current
                ?? throw new InvalidOperationException(
                    "ConfirmDialogService 需在 WPF 应用程序初始化之后调用（找不到 Application.Current）。");

            if (application.Dispatcher.CheckAccess())
            {
                return ShowOnUIThread(title, message, buttons);
            }

            // 模态窗口只能在 UI 线程显示：整体切回去，再把结果转发出来
            TaskCompletionSource<ConfirmDialogResult> completion =
                new(TaskCreationOptions.RunContinuationsAsynchronously);

            application.Dispatcher.BeginInvoke(new Action(async () =>
            {
                try
                {
                    ConfirmDialogResult result = await ShowOnUIThread(title, message, buttons);
                    completion.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    completion.TrySetException(ex);
                }
            }));

            return completion.Task;
        }

        private static Task<ConfirmDialogResult> ShowOnUIThread(string title, string message, ConfirmDialogButtons buttons)
        {
            ConfirmDialogWindow dialog = new()
            {
                Title = title,
                Message = message,
                Buttons = buttons,
                Owner = ResolveOwner()
            };

            return dialog.ShowAsync();
        }

        /// <summary>
        /// 取作为 Owner 的窗口：优先当前活动窗口，取不到退回第一个可见的顶层窗口。
        /// 窗口自身的 <c>Owner</c> 为 null 即顶层窗口——对话框的 <c>Owner</c> 指向它的宿主，
        /// 据此把对话框排除掉（模态对话框再套对话框时，不该把对话框当宿主）。
        /// </summary>
        private static Window? ResolveOwner()
        {
            // Application.Windows 只实现非泛型 IEnumerable，得先 Cast 才能用 LINQ
            List<Window> candidates = Application.Current!.Windows
                .Cast<Window>()
                .Where(window => window.IsVisible && window.Owner is null)
                .ToList();

            return candidates.FirstOrDefault(window => window.IsActive) ?? candidates.FirstOrDefault();
        }
    }
}