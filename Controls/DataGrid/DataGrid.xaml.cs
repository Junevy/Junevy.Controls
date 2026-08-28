namespace Junevy.Controls.Controls.DataGrid
{
    public class DataGrid : System.Windows.Controls.DataGrid
    {
        static DataGrid()
        {
            System.Windows.FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DataGrid),
                new System.Windows.FrameworkPropertyMetadata(typeof(DataGrid)));
        }
    }
}
