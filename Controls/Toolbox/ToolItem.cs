using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Toolbox;

public sealed class ToolItem : System.Windows.Controls.Button
{
    private sealed class DragDataDefaultMarker
    {
    }

    private static readonly DragDataDefaultMarker DragDataDefaultValue = new();

    private object? _generatedDragData;
    private bool _ownsGeneratedDragData;

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
            new PropertyMetadata(DragDataDefaultValue, null, CoerceDragData));

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

    public ToolItem()
    {
        CoerceValue(DragDataProperty);
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

    internal ToolboxItem? Owner { get; private set; }

    internal string? EffectiveDragDataFormat
    {
        get
        {
            ValueSource source = DependencyPropertyHelper.GetValueSource(this, DragDataFormatProperty);
            return IsUntouchedDefault(source)
                ? Owner?.Owner?.DragDataFormat
                : DragDataFormat;
        }
    }

    internal void AttachOwner(ToolboxItem owner)
    {
        Owner = owner;
    }

    internal void DetachOwner(ToolboxItem owner)
    {
        if (ReferenceEquals(Owner, owner))
        {
            Owner = null;
        }
    }

    internal void SetGeneratedDragData(object item)
    {
        ValueSource source = DependencyPropertyHelper.GetValueSource(this, DragDataProperty);
        if (!IsUntouchedDefault(source))
        {
            return;
        }

        _generatedDragData = item;
        _ownsGeneratedDragData = true;
        CoerceValue(DragDataProperty);
    }

    internal void ClearGeneratedDragData(object item)
    {
        if (!_ownsGeneratedDragData || !ReferenceEquals(_generatedDragData, item))
        {
            return;
        }

        _generatedDragData = null;
        _ownsGeneratedDragData = false;
        CoerceValue(DragDataProperty);
    }

    private static object? CoerceDragData(DependencyObject dependencyObject, object? baseValue)
    {
        var toolItem = (ToolItem)dependencyObject;
        if (baseValue is not DragDataDefaultMarker)
        {
            return baseValue;
        }

        return toolItem._ownsGeneratedDragData ? toolItem._generatedDragData : null;
    }

    private static bool IsUntouchedDefault(ValueSource source)
    {
        return source.BaseValueSource == BaseValueSource.Default
            && !source.IsAnimated
            && !source.IsCurrent
            && !source.IsExpression;
    }
}
