using System.Windows.Controls;

namespace Junevy.Controls.Showcase.Pages
{
    /// <summary>图像展示页：ImageViewer / TransparentBackground。</summary>
    public partial class ImagePage : UserControl
    {
        public ImagePage()
        {
            InitializeComponent();
            Viewer.Loaded += (_, _) => Viewer.FitToWindow();
        }

        private void OnFitToWindowClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Viewer.FitToWindow();
        }

        private void OnActualSizeClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Viewer.ActualSize();
        }
    }
}
