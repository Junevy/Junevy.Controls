using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Toolbox;

public sealed class ToolboxItem : HeaderedItemsControl
{
    private static readonly DependencyPropertyKey IsOpenPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsOpen),
            typeof(bool),
            typeof(ToolboxItem),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(ToolboxItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ToolboxItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(
            nameof(DisplayMode),
            typeof(ToolboxDisplayMode),
            typeof(ToolboxItem),
            new PropertyMetadata(ToolboxDisplayMode.IconOnly));

    public static readonly DependencyProperty IsOpenProperty = IsOpenPropertyKey.DependencyProperty;

    static ToolboxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolboxItem),
            new FrameworkPropertyMetadata(typeof(ToolboxItem)));
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

    public bool IsOpen => (bool)GetValue(IsOpenProperty);

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ToolItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ToolItem;
    }
}
