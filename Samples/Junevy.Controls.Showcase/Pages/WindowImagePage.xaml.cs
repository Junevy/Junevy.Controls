using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Junevy.Controls.Controls.Dialog;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>窗口与图像展示页：ImageViewer / DialogWindow。</summary>
    public partial class WindowImagePage : UserControl
    {
        public WindowImagePage()
        {
            InitializeComponent();
            Viewer.Loaded += (_, _) => Viewer.FitToWindow();
        }

        private void OnFitToWindowClick(object sender, RoutedEventArgs e)
        {
            Viewer.FitToWindow();
        }

        private void OnActualSizeClick(object sender, RoutedEventArgs e)
        {
            Viewer.ActualSize();
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

            var okButton = new Junevy.Controls.Controls.Button.Button { Content = "确定", IsDefault = true, IsTextScaled = false };
            var cancelButton = new Junevy.Controls.Controls.Button.Button { Content = "取消", IsCancel = true, IsTextScaled = false, Margin = new Thickness(12, 0, 0, 0) };
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
    }
}
