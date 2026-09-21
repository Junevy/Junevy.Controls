using System.Collections.ObjectModel;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Bar;

namespace Junevy.Controls.Showcase
{
    /// <summary>设备条目虚拟数据（ListBox / ListView 演示）。</summary>
    public sealed class DeviceItem
    {
        public string Name { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string IP { get; set; } = string.Empty;
    }

    /// <summary>检测结果虚拟数据（DataGrid 演示）。</summary>
    public sealed class ResultItem
    {
        public string Time { get; set; } = string.Empty;

        public string Device { get; set; } = string.Empty;

        public string Result { get; set; } = string.Empty;
    }

    /// <summary>横向缩略图虚拟数据（ListBox 横向滑动演示）。</summary>
    public sealed class TileItem
    {
        public int Index { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    /// <summary>展示程序共享的虚拟数据源，各展示页通过 <c>DataContext</c> 绑定。</summary>
    public sealed class SampleData
    {
        public static SampleData Instance { get; } = new();

        public ObservableCollection<DeviceItem> Devices { get; }

        public ObservableCollection<ResultItem> Results { get; }

        public ObservableCollection<TileItem> Tiles { get; }

        public ObservableCollection<string> Cameras { get; }

        /// <summary>ExpanderPanel ToggleCommand 演示：切换时经 MessageBarService 弹出通知。</summary>
        public RelayCommand PanelToggleCommand { get; }

        /// <summary>TextBox 命令按钮（CommandButton）演示：点击时经 MessageBarService 弹出通知。</summary>
        public RelayCommand TextBoxCommandButtonCommand { get; }

        private SampleData()
        {
            PanelToggleCommand = new RelayCommand(() =>
                MessageBarService.Show("ExpanderPanel", "ToggleCommand 已执行"));

            TextBoxCommandButtonCommand = new RelayCommand(() =>
                MessageBarService.Show("TextBox", "命令按钮已点击"));

            Devices =
            [
                new DeviceItem { Name = "Camera-01", Status = "在线", IP = "192.168.1.101" },
                new DeviceItem { Name = "Camera-02", Status = "在线", IP = "192.168.1.102" },
                new DeviceItem { Name = "Camera-03", Status = "离线", IP = "192.168.1.103" },
                new DeviceItem { Name = "PLC-01", Status = "在线", IP = "192.168.1.201" },
                new DeviceItem { Name = "Robot-01", Status = "告警", IP = "192.168.1.202" }
            ];

            Results =
            [
                new ResultItem { Time = "10:21:05", Device = "Camera-01", Result = "OK" },
                new ResultItem { Time = "10:21:06", Device = "Camera-02", Result = "NG" },
                new ResultItem { Time = "10:21:08", Device = "Camera-01", Result = "OK" },
                new ResultItem { Time = "10:21:11", Device = "Robot-01", Result = "NG" },
                new ResultItem { Time = "10:21:14", Device = "Camera-02", Result = "OK" },
                new ResultItem { Time = "10:21:17", Device = "PLC-01", Result = "OK" },
                new ResultItem { Time = "10:21:20", Device = "Camera-03", Result = "超时" },
                new ResultItem { Time = "10:21:23", Device = "Camera-01", Result = "OK" }
            ];

            Tiles =
            [
                new TileItem { Index = 1, Name = "Frame-01" },
                new TileItem { Index = 2, Name = "Frame-02" },
                new TileItem { Index = 3, Name = "Frame-03" },
                new TileItem { Index = 4, Name = "Frame-04" },
                new TileItem { Index = 5, Name = "Frame-05" },
                new TileItem { Index = 6, Name = "Frame-06" },
                new TileItem { Index = 7, Name = "Frame-07" },
                new TileItem { Index = 8, Name = "Frame-08" }
            ];

            Cameras =
            [
                "Camera-01 (Basler acA1300)",
                "Camera-02 (Hik MV-CA013)",
                "Camera-03 (Daheng MER-125)"
            ];
        }
    }
}
