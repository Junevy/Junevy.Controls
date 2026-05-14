using System.Windows;

namespace Junevy.Controls.Controls.DataGrid
{
    public class DataGrid : System.Windows.Controls.DataGrid
    {
        static DataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DataGrid),
                new FrameworkPropertyMetadata(typeof(DataGrid)));
        }

    }
}
