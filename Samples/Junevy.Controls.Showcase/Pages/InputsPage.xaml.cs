using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>输入与选择展示页：CheckBox / TextBox / ComboBox / DatePicker / Slider 与附加属性演示。</summary>
    public partial class InputsPage : UserControl
    {
        public InputsPage()
        {
            InitializeComponent();
            DataContext = SampleData.Instance;
        }
    }
}
