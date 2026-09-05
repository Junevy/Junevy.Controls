using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Bar
{
    /// <summary>
    /// <see cref="MessageBar"/> 的宿主容器。放在窗口布局中（通常覆盖在底部），
    /// 由 <see cref="MessageBarService"/> 或调用方在其中显示通知条。
    /// </summary>
    public class MessageBarPresenter : ContentControl
    {
        static MessageBarPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MessageBarPresenter),
                new FrameworkPropertyMetadata(typeof(MessageBarPresenter)));
        }

        /// <summary>当前承载的通知条；没有则为 <c>null</c>。</summary>
        public MessageBar? ActiveMessageBar => Content as MessageBar;

        /// <summary>替换内容并显示指定通知条。</summary>
        public void Show(MessageBar messageBar)
        {
            if (messageBar is null)
            {
                throw new ArgumentNullException(nameof(messageBar));
            }

            Content = messageBar;
            messageBar.Show();
        }

        /// <summary>隐藏当前通知条（带动画）。</summary>
        public void Dismiss()
        {
            ActiveMessageBar?.Hide();
        }
    }
}
