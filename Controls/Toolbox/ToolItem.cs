using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Toolbox;

public sealed class ToolItem : System.Windows.Controls.Button
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(ToolItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ToolItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(
            nameof(DisplayMode),
            typeof(ToolboxDisplayMode),
            typeof(ToolItem),
            new PropertyMetadata(ToolboxDisplayMode.IconAndTitle));

    public static readonly DependencyProperty IsDragEnabledProperty =
        DependencyProperty.Register(
            nameof(IsDragEnabled),
            typeof(bool),
            typeof(ToolItem),
            new PropertyMetadata(true));

    public static readonly DependencyProperty DragDataProperty =
        DependencyProperty.Register(
            nameof(DragData),
            typeof(object),
            typeof(ToolItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DragDataFormatProperty =
        DependencyProperty.Register(
            nameof(DragDataFormat),
            typeof(string),
            typeof(ToolItem),
            new PropertyMetadata(null));

    static ToolItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolItem),
            new FrameworkPropertyMetadata(typeof(ToolItem)));
    }

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ToolboxDisplayMode DisplayMode
    {
        get => (ToolboxDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public bool IsDragEnabled
    {
        get => (bool)GetValue(IsDragEnabledProperty);
        set => SetValue(IsDragEnabledProperty, value);
    }

    public object? DragData
    {
        get => GetValue(DragDataProperty);
        set => SetValue(DragDataProperty, value);
    }

    public string? DragDataFormat
    {
        get => (string?)GetValue(DragDataFormatProperty);
        set => SetValue(DragDataFormatProperty, value);
    }
}
