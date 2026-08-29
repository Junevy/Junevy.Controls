using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Toolbox;

public sealed class ToolItem : System.Windows.Controls.Button
{
    internal enum GeneratedDragDataOperation
    {
        None,
        ApplyMarker,
        ProbeMarker,
        ClearMarker
    }

    private object? _generatedDragData;
    private object? _generatedBaseMarker;
    private bool _ownsGeneratedDragData;
    private bool _generatedBaseMarkerObserved;
    private GeneratedDragDataOperation _generatedDragDataOperation;

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
            new PropertyMetadata(null, null, CoerceDragData));

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

    internal ToolboxItem? Owner { get; private set; }

    internal Action<GeneratedDragDataOperation>? GeneratedDragDataOperationCompletedForTest { get; set; }

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

        var marker = new object();
        _generatedDragData = item;
        _generatedBaseMarker = marker;
        _ownsGeneratedDragData = true;
        bool markerApplied = false;
        try
        {
            ExecuteGeneratedDragDataOperation(
                GeneratedDragDataOperation.ApplyMarker,
                () =>
                {
                    SetValue(DragDataProperty, marker);
                    markerApplied = true;
                });
        }
        catch
        {
            ReleaseGeneratedDragDataAfterFailure(markerApplied);
            throw;
        }
    }

    internal void ClearGeneratedDragData(object item)
    {
        if (!_ownsGeneratedDragData || !ReferenceEquals(_generatedDragData, item))
        {
            return;
        }

        bool markerCleared = false;
        try
        {
            _generatedBaseMarkerObserved = false;
            ExecuteGeneratedDragDataOperation(
                GeneratedDragDataOperation.ProbeMarker,
                () => CoerceValue(DragDataProperty));

            if (_generatedBaseMarkerObserved)
            {
                ExecuteGeneratedDragDataOperation(
                    GeneratedDragDataOperation.ClearMarker,
                    () =>
                    {
                        ClearValue(DragDataProperty);
                        markerCleared = true;
                    });
            }
        }
        catch
        {
            ReleaseGeneratedDragDataAfterFailure(_generatedBaseMarkerObserved && !markerCleared);
            throw;
        }
        finally
        {
            ReleaseGeneratedDragDataState();
        }
    }

    private static object? CoerceDragData(DependencyObject dependencyObject, object? baseValue)
    {
        var toolItem = (ToolItem)dependencyObject;
        if (toolItem._ownsGeneratedDragData
            && toolItem._generatedBaseMarker is not null
            && ReferenceEquals(baseValue, toolItem._generatedBaseMarker))
        {
            if (toolItem._generatedDragDataOperation == GeneratedDragDataOperation.ProbeMarker)
            {
                toolItem._generatedBaseMarkerObserved = true;
            }

            return toolItem._generatedDragData;
        }

        if (toolItem._ownsGeneratedDragData
            && toolItem._generatedDragDataOperation == GeneratedDragDataOperation.None)
        {
            toolItem.ReleaseGeneratedDragDataState();
        }

        return baseValue;
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == StyleProperty
            && _ownsGeneratedDragData
            && _generatedBaseMarker is not null
            && ReferenceEquals(ReadLocalValue(DragDataProperty), _generatedBaseMarker))
        {
            ReevaluateGeneratedDragDataAfterStyleChange();
        }
    }

    private void ReevaluateGeneratedDragDataAfterStyleChange()
    {
        object marker = _generatedBaseMarker!;
        try
        {
            ExecuteGeneratedDragDataOperation(
                GeneratedDragDataOperation.ClearMarker,
                () => ClearValue(DragDataProperty));

            ValueSource source = DependencyPropertyHelper.GetValueSource(this, DragDataProperty);
            if (!IsUntouchedDefault(source))
            {
                ReleaseGeneratedDragDataState();
                return;
            }

            ExecuteGeneratedDragDataOperation(
                GeneratedDragDataOperation.ApplyMarker,
                () => SetValue(DragDataProperty, marker));
        }
        catch
        {
            ReleaseGeneratedDragDataAfterFailure(
                ReferenceEquals(ReadLocalValue(DragDataProperty), marker));
            throw;
        }
    }

    private void ExecuteGeneratedDragDataOperation(GeneratedDragDataOperation operation, Action action)
    {
        _generatedDragDataOperation = operation;
        try
        {
            action();
            GeneratedDragDataOperationCompletedForTest?.Invoke(operation);
        }
        finally
        {
            _generatedDragDataOperation = GeneratedDragDataOperation.None;
        }
    }

    private void ReleaseGeneratedDragDataAfterFailure(bool clearMarker)
    {
        try
        {
            if (clearMarker)
            {
                _generatedDragDataOperation = GeneratedDragDataOperation.ClearMarker;
                ClearValue(DragDataProperty);
            }
        }
        finally
        {
            ReleaseGeneratedDragDataState();
        }
    }

    private void ReleaseGeneratedDragDataState()
    {
        _generatedDragDataOperation = GeneratedDragDataOperation.None;
        _generatedBaseMarkerObserved = false;
        _generatedBaseMarker = null;
        _generatedDragData = null;
        _ownsGeneratedDragData = false;
    }

    private static bool IsUntouchedDefault(ValueSource source)
    {
        return source.BaseValueSource == BaseValueSource.Default
            && !source.IsAnimated
            && !source.IsCoerced
            && !source.IsCurrent
            && !source.IsExpression;
    }
}
