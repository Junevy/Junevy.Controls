using System;
using System.Windows.Controls;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Bar;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>通知展示页：Badge / MessageBar / MessageBarService。</summary>
    public partial class NotifyPage : UserControl
    {
        public NotifyPage()
        {
            InitializeComponent();
        }

        private void OnInlineShowClick(object sender, System.Windows.RoutedEventArgs e)
        {
            InlineBar.Show();
        }

        private void OnInlineHideClick(object sender, System.Windows.RoutedEventArgs e)
        {
            InlineBar.Hide();
        }

        private void OnServiceInfoClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBarService.Show("Informational 通知：无标题、默认 2 秒自动关闭。");
        }

        private void OnServiceSuccessClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBarService.Show(MessageBarAppearance.Success, "已保存", "曝光设置已写入设备。");
        }

        private void OnServiceWarningClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBarService.Show(MessageBarAppearance.Warning, "参数越界", "增益高于推荐上限，已按上限执行。");
        }

        private void OnServiceDangerClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBarService.Show(MessageBarAppearance.Danger, "设备失联", "Camera-02 连接超时。", TimeSpan.FromSeconds(5));
        }

        private void OnServiceClearClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBarService.Clear();
        }
    }
}
