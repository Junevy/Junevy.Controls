using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

        /// <summary>
        /// 将宿主的最小尺寸约束转发给承载的通知条，确保在 XAML 中为
        /// <see cref="MessageBarPresenter"/> 设置的 <see cref="FrameworkElement.MinWidth"/>
        /// 与 <see cref="FrameworkElement.MinHeight"/> 真正作用于可见的通知条，
        /// 同时保留通知条原有的自适应宽高能力。
        /// </summary>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (oldContent is MessageBar oldBar)
            {
                BindingOperations.ClearBinding(oldBar, MinWidthProperty);
                BindingOperations.ClearBinding(oldBar, MinHeightProperty);
            }

            if (newContent is MessageBar newBar)
            {
                newBar.SetBinding(MinWidthProperty, new Binding(nameof(MinWidth)) { Source = this });
                newBar.SetBinding(MinHeightProperty, new Binding(nameof(MinHeight)) { Source = this });
            }
        }

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
