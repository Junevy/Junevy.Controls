using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>布局展示页：ExpanderPanel / SidePanel / Border.CornerRadius。</summary>
    public partial class LayoutPage : UserControl
    {
        public LayoutPage()
        {
            InitializeComponent();
        }

        private void OnLeftPanelClick(object sender, RoutedEventArgs e)
        {
            LeftPanel.IsOpen = true;
        }

        private void OnRightPanelClick(object sender, RoutedEventArgs e)
        {
            RightPanel.IsOpen = true;
        }

        private void OnClosePanelClick(object sender, RoutedEventArgs e)
        {
            LeftPanel.IsOpen = false;
            RightPanel.IsOpen = false;
        }
    }
}
