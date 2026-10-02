using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Junevy.Controls.Showcase.Pages
{
    public partial class IconsPage : UserControl
    {
        /// <summary>67 个图标的码点表，与 Resources/Font/iconfont*.ttf 的 cmap 保持一致。</summary>
        private static readonly (string Name, int Cp)[] AllIcons =
        [
            new("a-ModbusRtujieru", 0xE600),
            new("a-ModbusTCPjieru", 0xE601),
            new("zuobiaobiaoding", 0xE602),
            new("zuobiaoxi", 0xE603),
            new("peifang", 0xE604),
            new("zhuye-zuobiaoxi-chuangjianzuobiaoxi", 0xE606),
            new("qushi1", 0xE608),
            new("shezhi2", 0xE60A),
            new("Camera", 0xE60C),
            new("status_min", 0xE60E),
            new("wenjianjia1", 0xE60F),
            new("a-yunhangyunhangzhongzhunbeizhong", 0xE610),
            new("icon-", 0xE611),
            new("gouxuankuang-yigouxuan", 0xE612),
            new("ok", 0xE613),
            new("tianjiaguangyuan", 0xE617),
            new("tianjia_huaban", 0xE619),
            new("fail", 0xE61A),
            new("yunhang1", 0xE61B),
            new("lingcunwei1", 0xE627),
            new("shezhi1", 0xE628),
            new("wangluoxitong", 0xE62E),
            new("status_close", 0xE639),
            new("baocun", 0xE63F),
            new("yunhang3", 0xE640),
            new("baogao", 0xE646),
            new("xinjian", 0xE64F),
            new("zhankai", 0xE650),
            new("icon", 0xE651),
            new("baocun2", 0xE65C),
            new("home1", 0xE65D),
            new("icon-test", 0xE661),
            new("chuangjiantubiao", 0xE662),
            new("mendianshezhi", 0xE666),
            new("mobancaidan", 0xE66A),
            new("shezhi", 0xE66B),
            new("baocun1", 0xE67C),
            new("tingzhi", 0xE67D),
            new("Settings", 0xE67E),
            new("xiangji1", 0xE67F),
            new("lingcunwei", 0xE685),
            new("total", 0xE68D),
            new("status_max", 0xE691),
            new("lujing", 0xE692),
            new("zhijiaozuobiaoxi", 0xE69E),
            new("a-shezhi-shucaidanshezhi", 0xE6A5),
            new("workflow", 0xE6BC),
            new("yunhang2", 0xE6C1),
            new("shexiangji", 0xE6CA),
            new("yuzhifenge", 0xE6CB),
            new("tishi", 0xE6D1),
            new("LDshangshengyanchudian", 0xE6F2),
            new("xiangji", 0xE72C),
            new("jinguangdengguan", 0xE73D),
            new("cj", 0xE761),
            new("huanyuanchuangkoubili", 0xE772),
            new("user1", 0xE7B2),
            new("run-solid", 0xE7DC),
            new("home2", 0xE7FE),
            new("yunhang", 0xE809),
            new("wenjianjia", 0xE80C),
            new("qushi", 0xE87B),
            new("warning1", 0xE932),
            new("radio-button-kuai", 0xE981),
            new("modbus", 0xE9AE),
            new("24gf-stop", 0xEA89),
            new("24gf-folderStar", 0xEAC5),
        ];

        private sealed class IconItem
        {
            public string Name { get; }
            public string Cp { get; }
            public string Glyph { get; }
            public FontFamily Font { get; set; }
            public double Size { get; set; }

            public IconItem(string name, int cp, FontFamily font, double size)
            {
                Name = name;
                Cp = $"U+{cp:X4}";
                Glyph = char.ToString((char)cp);
                Font = font;
                Size = size;
            }
        }

        private readonly List<IconItem> _items = [];
        private FontFamily _linearFont;
        private FontFamily _filledFont;

        public IconsPage()
        {
            InitializeComponent();
            _linearFont = (FontFamily)FindResource("IconFont");
            _filledFont = (FontFamily)FindResource("IconFontFilled");
            foreach (var (name, cp) in AllIcons)
            {
                _items.Add(new IconItem(name, cp, _linearFont, SizeSlider.Value));
            }
            ApplyFilter();
        }

        private void OnFilterChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void OnStyleChanged(object sender, RoutedEventArgs e) => ApplyFilter();

        private void OnSizeChanged(object sender, RoutedEventArgs e)
        {
            if (IconHost == null)
            {
                return;
            }
            foreach (var item in _items)
            {
                item.Size = SizeSlider.Value;
            }
            IconHost.Items.Refresh();
        }

        private void ApplyFilter()
        {
            if (IconHost == null)
            {
                return;
            }
            var font = FilledRadio is { IsChecked: true } ? _filledFont : _linearFont;
            var filter = FilterBox.Text.Trim();
            IEnumerable<IconItem> query = _items;
            if (filter.Length > 0)
            {
                query = query.Where(i =>
                    i.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || i.Cp.Contains(filter, StringComparison.OrdinalIgnoreCase));
            }
            foreach (var item in _items)
            {
                item.Font = font;
            }
            IconHost.ItemsSource = query.ToList();
        }
    }
}
