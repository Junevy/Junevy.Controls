using System;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Bar
{
    /// <summary>
    /// 参考 WPF-UI <c>SnackbarService</c> 的静态通知服务：
    /// 先通过 <see cref="SetPresenter"/> 注册窗口中的 <see cref="MessageBarPresenter"/>，
    /// 之后可在任意位置调用 <see cref="Show(MessageBarAppearance, string, string)"/> 系列方法弹出通知。
    /// </summary>
    public static class MessageBarService
    {
        private static MessageBarPresenter? _presenter;

        /// <summary>注册用于承载通知条的宿主。重复调用会替换之前的宿主。</summary>
        public static void SetPresenter(MessageBarPresenter presenter)
        {
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        }

        /// <summary>以 Informational 外观显示一条无标题通知。</summary>
        public static void Show(string message) => ShowCore(MessageBarAppearance.Informational, null, message, null);

        /// <summary>以 Informational 外观显示一条通知。</summary>
        public static void Show(string title, string message) => ShowCore(MessageBarAppearance.Informational, title, message, null);

        /// <summary>显示一条无标题通知。</summary>
        public static void Show(MessageBarAppearance appearance, string message) => ShowCore(appearance, null, message, null);

        /// <summary>显示一条通知，使用 <see cref="MessageBar.Timeout"/> 的默认超时。</summary>
        public static void Show(MessageBarAppearance appearance, string title, string message) => ShowCore(appearance, title, message, null);

        /// <summary>显示一条通知并指定自动关闭时间；<paramref name="timeout"/> 为零或负值时禁用自动关闭。</summary>
        public static void Show(MessageBarAppearance appearance, string title, string message, TimeSpan timeout) => ShowCore(appearance, title, message, timeout);

        /// <summary>隐藏当前显示的通知。</summary>
        public static void Clear()
        {
            MessageBarPresenter? presenter = _presenter;
            if (presenter is null)
            {
                return;
            }

            if (!presenter.Dispatcher.CheckAccess())
            {
                presenter.Dispatcher.BeginInvoke(new Action(Clear));
                return;
            }

            presenter.Dismiss();
        }

        private static void ShowCore(MessageBarAppearance appearance, string? title, string message, TimeSpan? timeout)
        {
            MessageBarPresenter? presenter = _presenter;
            if (presenter is null)
            {
                throw new InvalidOperationException(
                    "MessageBarService.SetPresenter 必须先在窗口中注册 MessageBarPresenter 后才能显示通知。");
            }

            if (!presenter.Dispatcher.CheckAccess())
            {
                presenter.Dispatcher.BeginInvoke(() => ShowCore(appearance, title, message, timeout));
                return;
            }

            var messageBar = new MessageBar
            {
                Appearance = appearance,
                Message = message
            };

            if (!string.IsNullOrEmpty(title))
            {
                messageBar.Title = title;
            }

            if (timeout.HasValue)
            {
                messageBar.Timeout = timeout.Value;
            }

            presenter.Show(messageBar);
        }
    }
}
