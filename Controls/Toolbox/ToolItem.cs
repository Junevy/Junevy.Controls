using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Toolbox;

public sealed class ToolItem : System.Windows.Controls.Button
{
    private static readonly object CurrentNullDragDataMarker = new();

    internal enum GeneratedDragDataOperation
    {
        None,
        ApplyState,
        ProbeState,
        ClearState
    }

    private object? _generatedDragData;
    private object? _generatedStateMarker;
    private bool _ownsGeneratedDragData;
    private bool _generatedStateObserved;
    private bool _isApplyingCurrentNullMarker;
    private GeneratedDragDataOperation _generatedDragDataOperation;

    private static readonly DependencyProperty GeneratedDragDataStateProperty =
        DependencyProperty.Register(
            "GeneratedDragDataState",
            typeof(object),
            typeof(ToolItem),
            new PropertyMetadata(null, OnGeneratedDragDataStateChanged));

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

    public new void SetCurrentValue(DependencyProperty dependencyProperty, object? value)
    {
        if (dependencyProperty == DragDataProperty && value is null)
        {
            // WPF drops a current null when coercion returns the metadata default. Preserve
            // the current source with a non-public base marker and expose null via coercion.
            _isApplyingCurrentNullMarker = true;
            try
            {
                SetValue(dependencyProperty, CurrentNullDragDataMarker);
                base.SetCurrentValue(dependencyProperty, null);
            }
            finally
            {
                _isApplyingCurrentNullMarker = false;
            }

            if (_ownsGeneratedDragData)
            {
                ReleaseGeneratedDragDataOwnership(recoerceDragData: true);
            }

            return;
        }

        base.SetCurrentValue(dependencyProperty, value);
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

        var stateMarker = new object();
        _generatedDragData = item;
        _generatedStateMarker = stateMarker;
        _ownsGeneratedDragData = true;
        bool stateApplied = false;
        try
        {
            ExecuteGeneratedDragDataOperation(
                GeneratedDragDataOperation.ApplyState,
                () =>
                {
                    SetValue(GeneratedDragDataStateProperty, stateMarker);
                    stateApplied = true;
                });
        }
        catch
        {
            ReleaseGeneratedDragDataAfterFailure(stateApplied);
            throw;
        }
    }

    internal void ClearGeneratedDragData(object item)
    {
        if (!_ownsGeneratedDragData || !ReferenceEquals(_generatedDragData, item))
        {
            return;
        }

        bool stateCleared = false;
        try
        {
            _generatedStateObserved = false;
            ExecuteGeneratedDragDataOperation(
                GeneratedDragDataOperation.ProbeState,
                () => CoerceValue(DragDataProperty));

            if (_generatedStateObserved)
            {
                ExecuteGeneratedDragDataOperation(
                GeneratedDragDataOperation.ClearState,
                () =>
                {
                    ClearValue(GeneratedDragDataStateProperty);
                    CoerceValue(DragDataProperty);
                    stateCleared = true;
                });
            }
        }
        catch
        {
            ReleaseGeneratedDragDataAfterFailure(_generatedStateObserved && !stateCleared);
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
        if (ReferenceEquals(baseValue, CurrentNullDragDataMarker))
        {
            return toolItem._isApplyingCurrentNullMarker
                ? toolItem._generatedDragData ?? CurrentNullDragDataMarker
                : null;
        }

        if (!toolItem.HasGeneratedDragDataState())
        {
            return baseValue;
        }

        ValueSource source = DependencyPropertyHelper.GetValueSource(toolItem, DragDataProperty);
        if (IsGeneratedDefaultSource(source))
        {
            if (toolItem._generatedDragDataOperation == GeneratedDragDataOperation.ProbeState)
            {
                toolItem._generatedStateObserved = true;
            }

            return toolItem._generatedDragData;
        }

        if (toolItem._generatedDragDataOperation == GeneratedDragDataOperation.None)
        {
            toolItem.ReleaseGeneratedDragDataOwnership(recoerceDragData: false);
        }

        return baseValue;
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (HasGeneratedDragDataState()
            && e.Property != DragDataProperty
            && e.Property != GeneratedDragDataStateProperty)
        {
            CoerceValue(DragDataProperty);
        }
    }

    private static void OnGeneratedDragDataStateChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        var toolItem = (ToolItem)dependencyObject;
        if (toolItem._generatedDragDataOperation != GeneratedDragDataOperation.ClearState)
        {
            toolItem.CoerceValue(DragDataProperty);
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

    private void ReleaseGeneratedDragDataAfterFailure(bool clearState)
    {
        try
        {
            if (clearState)
            {
                _generatedDragDataOperation = GeneratedDragDataOperation.ClearState;
                ClearValue(GeneratedDragDataStateProperty);
                CoerceValue(DragDataProperty);
            }
        }
        finally
        {
            ReleaseGeneratedDragDataState();
        }
    }

    private void ReleaseGeneratedDragDataOwnership(bool recoerceDragData)
    {
        try
        {
            if (HasGeneratedDragDataState())
            {
                ExecuteGeneratedDragDataOperation(
                    GeneratedDragDataOperation.ClearState,
                    () =>
                    {
                        ClearValue(GeneratedDragDataStateProperty);
                        if (recoerceDragData)
                        {
                            CoerceValue(DragDataProperty);
                        }
                    });
            }
        }
        finally
        {
            _generatedDragDataOperation = GeneratedDragDataOperation.None;
            _generatedStateObserved = false;
            _generatedStateMarker = null;
            _generatedDragData = null;
            _ownsGeneratedDragData = false;
        }
    }

    private void ReleaseGeneratedDragDataState()
    {
        _generatedDragDataOperation = GeneratedDragDataOperation.None;
        _generatedStateObserved = false;
        _generatedStateMarker = null;
        _generatedDragData = null;
        _ownsGeneratedDragData = false;
    }

    private bool HasGeneratedDragDataState()
    {
        return _ownsGeneratedDragData
            && _generatedStateMarker is not null
            && ReferenceEquals(GetValue(GeneratedDragDataStateProperty), _generatedStateMarker);
    }

    private static bool IsGeneratedDefaultSource(ValueSource source)
    {
        return source.BaseValueSource == BaseValueSource.Default
            && !source.IsAnimated
            && !source.IsCurrent
            && !source.IsExpression;
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
