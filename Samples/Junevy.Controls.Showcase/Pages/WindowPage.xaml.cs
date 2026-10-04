using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Bar;
using Junevy.Controls.Controls.Dialog;
using Junevy.Controls.Controls.Progress;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>窗口与对话框展示页：DialogWindow / ProgressBarWindow。</summary>
    public partial class WindowPage : UserControl
    {
        public WindowPage()
        {
            InitializeComponent();
        }

        private void OnOpenDialogClick(object sender, RoutedEventArgs e)
        {
            var title = new TextBlock
            {
                Text = "设备设置",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var nameBox = new Junevy.Controls.Controls.Text.TextBox
            {
                Width = 320,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12)
            };
            Junevy.Controls.AttachedProperties.TitleAssist.SetTitle(nameBox, "设备名称");
            Junevy.Controls.AttachedProperties.PlaceholderAssist.SetPlaceholder(nameBox, "输入设备名称…");

            var modeBox = new Junevy.Controls.Controls.Box.ComboBox
            {
                Width = 320,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12)
            };
            modeBox.Items.Add("Continuous");
            modeBox.Items.Add("Trigger");
            Junevy.Controls.AttachedProperties.TitleAssist.SetTitle(modeBox, "采集模式");

            var autoStart = new Junevy.Controls.Controls.Button.ToggleButton
            {
                Content = "开机自动采集",
                IsChecked = true,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var okButton = new Junevy.Controls.Controls.Button.Button { Content = "确定", IsDefault = true };
            var cancelButton = new Junevy.Controls.Controls.Button.Button { Content = "取消", IsCancel = true, Margin = new Thickness(12, 0, 0, 0) };
            var buttonRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            buttonRow.Children.Add(okButton);
            buttonRow.Children.Add(cancelButton);

            var content = new StackPanel { Margin = new Thickness(28), Width = 380 };
            content.Children.Add(title);
            content.Children.Add(nameBox);
            content.Children.Add(modeBox);
            content.Children.Add(autoStart);
            content.Children.Add(buttonRow);

            DialogWindow dialog = new()
            {
                Title = "设备设置",
                Content = content,
                ShowMaximizeButton = true,
                Owner = Window.GetWindow(this)
            };
            okButton.Click += (_, _) => dialog.Close();
            cancelButton.Click += (_, _) => dialog.Close();
            dialog.ShowDialog();
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
