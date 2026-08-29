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

    public ToolboxItem()
    {
        IsEnabledChanged += OnIsEnabledChanged;
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

    internal Toolbox? Owner { get; private set; }

    internal bool IsPointerOverTrigger { get; private set; }

    internal bool IsPointerOverPopup { get; private set; }

    internal bool IsPointerOverEitherRegion => IsPointerOverTrigger || IsPointerOverPopup;

    internal void AttachOwner(Toolbox owner)
    {
        if (ReferenceEquals(Owner, owner))
        {
            return;
        }

        Owner?.NotifyItemInvalidated(this);
        Owner = owner;
        SetIsOpen(false);
    }

    internal void DetachOwner(Toolbox owner)
    {
        if (!ReferenceEquals(Owner, owner))
        {
            return;
        }

        SetIsOpen(false);
        IsPointerOverTrigger = false;
        IsPointerOverPopup = false;
        Owner = null;
    }

    internal void SetPointerOverTrigger(bool value)
    {
        IsPointerOverTrigger = value;
    }

    internal void SetPointerOverPopup(bool value)
    {
        IsPointerOverPopup = value;
    }

    internal void SetIsOpen(bool value)
    {
        SetValue(IsOpenPropertyKey, value);
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ToolItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ToolItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is not ToolItem container)
        {
            return;
        }

        container.AttachOwner(this);
        if (!ReferenceEquals(container, item))
        {
            container.SetGeneratedDragData(item);
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is ToolItem container)
        {
            container.ClearGeneratedDragData(item);
            container.DetachOwner(this);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        if (!HasItems)
        {
            Owner?.NotifyItemInvalidated(this);
        }
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!(bool)e.NewValue)
        {
            Owner?.NotifyItemInvalidated(this);
        }
    }
}
