using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Bar;
using Junevy.Controls.Controls.Progress;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>进度条展示页：jv:ProgressBar（线性 / 环形）与 ProgressBarWindow 进度对话框。</summary>
    public partial class ProgressPage : UserControl
    {
        public ProgressPage()
        {
            InitializeComponent();
        }

        private async void OnDeployWindowClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ProgressBarWindow
            {
                Title = "固件部署",
                Message = "正在部署固件…",
                Detail = "准备中…",
                Owner = Window.GetWindow(this)
            };
            dialog.Show();

            // 用户点对话框关闭按钮 = 取消等待：经 Cancelled 事件终止模拟任务。
            using var cancellation = new CancellationTokenSource();
            dialog.Cancelled += (_, _) => cancellation.Cancel();

            try
            {
                for (int percent = 0; percent <= 100; percent += 2)
                {
                    dialog.Report(percent);
                    dialog.UpdateDetail($"已完成 {percent}%");
                    await Task.Delay(60, cancellation.Token);
                }

                dialog.UpdateDetail("部署完成");
                dialog.RequestClose();
                MessageBarService.Show(MessageBarAppearance.Success, "固件部署", "部署完成。");
            }
            catch (OperationCanceledException)
            {
                MessageBarService.Show(MessageBarAppearance.Warning, "固件部署", "部署已取消。");
            }
        }

        private async void OnWaitingWindowClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ProgressBarWindow
            {
                Title = "连接设备",
                Message = "正在连接设备…",
                Detail = "Camera-02 · 192.168.1.32",
                IsIndeterminate = true,
                CloseButtonEnabled = false,
                Owner = Window.GetWindow(this)
            };
            dialog.Show();

            await Task.Delay(3000);
            dialog.UpdateDetail("已连接");
            dialog.RequestClose();
            MessageBarService.Show(MessageBarAppearance.Success, "连接设备", "Camera-02 已上线。");
        }
    }
}
