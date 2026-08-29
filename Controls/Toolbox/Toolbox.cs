using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Toolbox;

public sealed class Toolbox : ItemsControl
{
    private static readonly DependencyPropertyKey ActiveItemPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ActiveItem),
            typeof(ToolboxItem),
            typeof(Toolbox),
            new PropertyMetadata(null));

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(Toolbox),
            new PropertyMetadata(Orientation.Vertical));

    public static readonly DependencyProperty OpenDelayProperty =
        DependencyProperty.Register(
            nameof(OpenDelay),
            typeof(TimeSpan),
            typeof(Toolbox),
            new PropertyMetadata(TimeSpan.FromMilliseconds(150)),
            IsNonNegativeDelay);

    public static readonly DependencyProperty CloseDelayProperty =
        DependencyProperty.Register(
            nameof(CloseDelay),
            typeof(TimeSpan),
            typeof(Toolbox),
            new PropertyMetadata(TimeSpan.FromMilliseconds(300)),
            IsNonNegativeDelay);

    public static readonly DependencyProperty PopupWidthProperty =
        DependencyProperty.Register(
            nameof(PopupWidth),
            typeof(double),
            typeof(Toolbox),
            new PropertyMetadata(300d),
            IsPositiveFiniteDouble);

    public static readonly DependencyProperty ColumnCountProperty =
        DependencyProperty.Register(
            nameof(ColumnCount),
            typeof(int),
            typeof(Toolbox),
            new PropertyMetadata(6),
            IsPositiveColumnCount);

    public static readonly DependencyProperty PopupMaxHeightProperty =
        DependencyProperty.Register(
            nameof(PopupMaxHeight),
            typeof(double),
            typeof(Toolbox),
            new PropertyMetadata(480d),
            IsPositiveFiniteDouble);

    public static readonly DependencyProperty PopupPlacementProperty =
        DependencyProperty.Register(
            nameof(PopupPlacement),
            typeof(ToolboxPopupPlacement),
            typeof(Toolbox),
            new PropertyMetadata(ToolboxPopupPlacement.Auto));

    public static readonly DependencyProperty DragDataFormatProperty =
        DependencyProperty.Register(
            nameof(DragDataFormat),
            typeof(string),
            typeof(Toolbox),
            new PropertyMetadata("Junevy.Controls.Tool"),
            IsNonEmptyFormat);

    public static readonly DependencyProperty ActiveItemProperty = ActiveItemPropertyKey.DependencyProperty;

    static Toolbox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Toolbox),
            new FrameworkPropertyMetadata(typeof(Toolbox)));
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public TimeSpan OpenDelay
    {
        get => (TimeSpan)GetValue(OpenDelayProperty);
        set => SetValue(OpenDelayProperty, value);
    }

    public TimeSpan CloseDelay
    {
        get => (TimeSpan)GetValue(CloseDelayProperty);
        set => SetValue(CloseDelayProperty, value);
    }

    public double PopupWidth
    {
        get => (double)GetValue(PopupWidthProperty);
        set => SetValue(PopupWidthProperty, value);
    }

    public int ColumnCount
    {
        get => (int)GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    public double PopupMaxHeight
    {
        get => (double)GetValue(PopupMaxHeightProperty);
        set => SetValue(PopupMaxHeightProperty, value);
    }

    public ToolboxPopupPlacement PopupPlacement
    {
        get => (ToolboxPopupPlacement)GetValue(PopupPlacementProperty);
        set => SetValue(PopupPlacementProperty, value);
    }

    public string DragDataFormat
    {
        get => (string)GetValue(DragDataFormatProperty);
        set => SetValue(DragDataFormatProperty, value);
    }

    public ToolboxItem? ActiveItem => (ToolboxItem?)GetValue(ActiveItemProperty);

    public void ClosePopup()
    {
        SetValue(ActiveItemPropertyKey, null);
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ToolboxItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ToolboxItem;
    }

    private static bool IsNonNegativeDelay(object value)
    {
        return (TimeSpan)value >= TimeSpan.Zero;
    }

    private static bool IsPositiveFiniteDouble(object value)
    {
        double number = (double)value;
        return number > 0d && !double.IsNaN(number) && !double.IsInfinity(number);
    }

    private static bool IsPositiveColumnCount(object value)
    {
        return (int)value >= 1;
    }

    private static bool IsNonEmptyFormat(object value)
    {
        return value is string text && !string.IsNullOrWhiteSpace(text);
    }
}
