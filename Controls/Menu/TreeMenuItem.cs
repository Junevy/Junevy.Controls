using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

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

        //public string Title
        //{
        //    get { return (string)GetValue(TitleProperty); }
        //    set { SetValue(TitleProperty, value); }
        //}
        //public static readonly DependencyProperty TitleProperty =
        //    DependencyProperty.Register("Title", typeof(), typeof(TreeMenuItem));


        //public object Icon
        //{
        //    get { return (object)GetValue(IconProperty); }
        //    set { SetValue(IconProperty, value); }
        //}
        //public static readonly DependencyProperty IconProperty =
        //    DependencyProperty.Register("Icon", typeof(object), typeof(TreeMenuItem));



    }
}
