using System;
using System.Windows;
using System.Windows.Controls;

﻿namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// SideMenu 的导航数据控件（声明于 XAML 或集合中，作为条目数据使用；
    /// 由 SideMenu 的条目 DataTemplate 渲染 Title 与 Icon，自身模板不参与视觉）。
    /// </summary>
    public class MenuItem : ContentControl
    {
        public Guid Id { get; } = Guid.NewGuid();

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MenuItem), new PropertyMetadata(""));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(MenuItem));

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(MenuItem), new PropertyMetadata(Orientation.Horizontal));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
    }
}
