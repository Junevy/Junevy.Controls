using System.Collections.ObjectModel;
using System.Windows;

namespace Junevy.Controls.Controls.Menu
{
    public class TreeMenuItem : MenuItem
    {
        public bool IsLeaf => Childrens == null || Childrens.Count == 0;

        public static readonly DependencyProperty ChildrensProperty =
            DependencyProperty.Register("Childrens", typeof(ObservableCollection<TreeMenuItem>), typeof(TreeMenuItem));


        public ObservableCollection<TreeMenuItem> Childrens
        {
            get { return (ObservableCollection<TreeMenuItem>)GetValue(ChildrensProperty); }
            set { SetValue(ChildrensProperty, value); }
        }

    }
}
