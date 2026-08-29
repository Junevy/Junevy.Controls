using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Toolbox;

public sealed class ToolItem : System.Windows.Controls.Button
{
    private object? _generatedDragData;
    private Point? _dragStart;
    private bool _suppressClick;
    private bool _mouseGestureActive;
    private bool _isCompletingMouseGesture;

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

    internal ToolboxItem? Owner { get; private set; }

    internal Func<DependencyObject, DataObject, DragDropEffects, DragDropEffects> DragExecutor { get; set; } =
        static (source, data, allowedEffects) => DragDrop.DoDragDrop(source, data, allowedEffects);

    internal bool SuppressClick => _suppressClick;

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

    internal DataObject? CreateDragDataObject()
    {
        string? format = EffectiveDragDataFormat;
        if (!IsDragEnabled || DragData is null || string.IsNullOrWhiteSpace(format))
        {
            return null;
        }

        var data = new DataObject();
        data.SetData(format, DragData, false);
        return data;
    }

    internal static bool ExceedsDragThreshold(
        Point start,
        Point current,
        double minHorizontal,
        double minVertical)
    {
        return Math.Abs(current.X - start.X) > minHorizontal
            || Math.Abs(current.Y - start.Y) > minVertical;
    }

    internal void BeginDragGesture(Point start)
    {
        _suppressClick = false;
        _mouseGestureActive = true;
        _dragStart = start;
    }

    internal DragDropEffects? ContinueDragGesture(Point current, MouseButtonState leftButton)
    {
        if (leftButton != MouseButtonState.Pressed || _dragStart is not Point start)
        {
            _dragStart = null;
            return null;
        }

        if (!ExceedsDragThreshold(
                start,
                current,
                SystemParameters.MinimumHorizontalDragDistance,
                SystemParameters.MinimumVerticalDragDistance))
        {
            return null;
        }

        DataObject? data = CreateDragDataObject();
        if (data is null)
        {
            _dragStart = null;
            return null;
        }

        _dragStart = null;
        return ExecuteDrag(data);
    }

    internal DragDropEffects ExecuteDrag(DataObject data)
    {
        ToolboxItem? initiatingOwner = Owner;
        Toolbox? initiatingCoordinator = initiatingOwner?.Owner;
        _suppressClick = _mouseGestureActive;
        if (initiatingOwner is not null)
        {
            initiatingCoordinator?.NotifyDragStarted(initiatingOwner);
        }

        try
        {
            return DragExecutor(this, data, DragDropEffects.Copy);
        }
        finally
        {
            if (initiatingOwner is not null)
            {
                initiatingCoordinator?.NotifyDragCompleted(initiatingOwner);
            }
        }
    }

    internal void CompleteMouseGesture(Action completeMouseUp)
    {
        _isCompletingMouseGesture = _mouseGestureActive && _suppressClick;
        try
        {
            completeMouseUp();
        }
        finally
        {
            _dragStart = null;
            _mouseGestureActive = false;
            _suppressClick = false;
            _isCompletingMouseGesture = false;
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

        SetCurrentValue(DragDataProperty, item);
        _generatedDragData = item;
    }

    internal void ClearGeneratedDragData(object item)
    {
        if (!ReferenceEquals(_generatedDragData, item))
        {
            return;
        }

        try
        {
            ValueSource source = DependencyPropertyHelper.GetValueSource(this, DragDataProperty);
            if (source.BaseValueSource == BaseValueSource.Default
                && source.IsCurrent
                && ReferenceEquals(DragData, _generatedDragData))
            {
                ClearValue(DragDataProperty);
            }
        }
        finally
        {
            _generatedDragData = null;
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        BeginDragGesture(e.GetPosition(this));
        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        ContinueDragGesture(e.GetPosition(this), e.LeftButton);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        CompleteMouseGesture(() => InvokeBaseMouseLeftButtonUp(e));
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        _dragStart = null;
        base.OnLostMouseCapture(e);
    }

    protected override void OnClick()
    {
        if (_isCompletingMouseGesture && _suppressClick)
        {
            _suppressClick = false;
            return;
        }

        _suppressClick = false;
        _mouseGestureActive = false;
        base.OnClick();
    }

    private void InvokeBaseMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
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
