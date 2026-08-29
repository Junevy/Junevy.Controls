#if JUNEVY_CONTROLS_TESTS
using System.Reflection;
#if NET8_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Toolbox;
using NUnit.Framework;

namespace Junevy.Controls.Tests.Toolbox;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
#if NET8_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public sealed class ToolboxDefaultsTests
{
    [Test]
    public void Enums_ExposeTheDocumentedValuesInOrder()
    {
        Assert.Multiple(() =>
        {
            Assert.That((ToolboxDisplayMode[])Enum.GetValues(typeof(ToolboxDisplayMode)), Is.EqualTo(new[]
            {
                ToolboxDisplayMode.IconOnly,
                ToolboxDisplayMode.IconAndTitle
            }));
            Assert.That((ToolboxPopupPlacement[])Enum.GetValues(typeof(ToolboxPopupPlacement)), Is.EqualTo(new[]
            {
                ToolboxPopupPlacement.Auto,
                ToolboxPopupPlacement.Right,
                ToolboxPopupPlacement.Left,
                ToolboxPopupPlacement.Bottom,
                ToolboxPopupPlacement.Top
            }));
        });
    }

    [Test]
    public void Toolbox_HasTheDocumentedDefaults()
    {
        var toolbox = new Junevy.Controls.Controls.Toolbox.Toolbox();

        Assert.Multiple(() =>
        {
            Assert.That(toolbox.Orientation, Is.EqualTo(Orientation.Vertical));
            Assert.That(toolbox.OpenDelay, Is.EqualTo(TimeSpan.FromMilliseconds(150)));
            Assert.That(toolbox.CloseDelay, Is.EqualTo(TimeSpan.FromMilliseconds(300)));
            Assert.That(toolbox.PopupWidth, Is.EqualTo(300d));
            Assert.That(toolbox.ColumnCount, Is.EqualTo(6));
            Assert.That(toolbox.PopupMaxHeight, Is.EqualTo(480d));
            Assert.That(toolbox.PopupPlacement, Is.EqualTo(ToolboxPopupPlacement.Auto));
            Assert.That(toolbox.DragDataFormat, Is.EqualTo("Junevy.Controls.Tool"));
            Assert.That(toolbox.ActiveItem, Is.Null);
            Assert.That(ReadDefaultStyleKey(toolbox), Is.EqualTo(typeof(Junevy.Controls.Controls.Toolbox.Toolbox)));
            Assert.That(Junevy.Controls.Controls.Toolbox.Toolbox.ActiveItemProperty.ReadOnly, Is.True);
        });
    }

    [Test]
    public void ToolboxItem_HasTheDocumentedDefaults()
    {
        var item = new ToolboxItem();

        Assert.Multiple(() =>
        {
            Assert.That(item.Icon, Is.Null);
            Assert.That(item.Title, Is.Null);
            Assert.That(item.DisplayMode, Is.EqualTo(ToolboxDisplayMode.IconOnly));
            Assert.That(item.IsOpen, Is.False);
            Assert.That(ReadDefaultStyleKey(item), Is.EqualTo(typeof(ToolboxItem)));
            Assert.That(ToolboxItem.IsOpenProperty.ReadOnly, Is.True);
        });
    }

    [Test]
    public void ToolItem_HasTheDocumentedDefaults()
    {
        var item = new ToolItem();

        Assert.Multiple(() =>
        {
            Assert.That(item.Icon, Is.Null);
            Assert.That(item.Title, Is.Null);
            Assert.That(item.DisplayMode, Is.EqualTo(ToolboxDisplayMode.IconAndTitle));
            Assert.That(item.IsDragEnabled, Is.True);
            Assert.That(item.DragData, Is.Null);
            Assert.That(item.DragDataFormat, Is.Null);
            Assert.That(ReadDefaultStyleKey(item), Is.EqualTo(typeof(ToolItem)));
        });
    }

    [TestCase(-10000L)]
    [TestCase(-1L)]
    public void Toolbox_RejectsNegativeDelays(long ticks)
    {
        var toolbox = new Junevy.Controls.Controls.Toolbox.Toolbox();
        var delay = TimeSpan.FromTicks(ticks);

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => toolbox.OpenDelay = delay);
            Assert.Throws<ArgumentException>(() => toolbox.CloseDelay = delay);
        });
    }

    [Test]
    public void Toolbox_AcceptsZeroDelays()
    {
        var toolbox = new Junevy.Controls.Controls.Toolbox.Toolbox
        {
            OpenDelay = TimeSpan.Zero,
            CloseDelay = TimeSpan.Zero
        };

        Assert.Multiple(() =>
        {
            Assert.That(toolbox.OpenDelay, Is.EqualTo(TimeSpan.Zero));
            Assert.That(toolbox.CloseDelay, Is.EqualTo(TimeSpan.Zero));
        });
    }

    [TestCase(0d)]
    [TestCase(-1d)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void Toolbox_RejectsInvalidPositiveDimensions(double value)
    {
        var toolbox = new Junevy.Controls.Controls.Toolbox.Toolbox();

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => toolbox.PopupWidth = value);
            Assert.Throws<ArgumentException>(() => toolbox.PopupMaxHeight = value);
        });
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Toolbox_RejectsColumnCountsBelowOne(int value)
    {
        var toolbox = new Junevy.Controls.Controls.Toolbox.Toolbox();

        Assert.Throws<ArgumentException>(() => toolbox.ColumnCount = value);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   	")]
    public void Toolbox_RejectsBlankDragDataFormats(string? value)
    {
        var toolbox = new Junevy.Controls.Controls.Toolbox.Toolbox();

        Assert.Throws<ArgumentException>(() => toolbox.DragDataFormat = value!);
    }

    [Test]
    public void Toolbox_UsesToolboxItemContainersAndPreservesExplicitContainers()
    {
        var toolbox = new Junevy.Controls.Controls.Toolbox.Toolbox();
        var explicitItem = new ToolboxItem();

        Assert.Multiple(() =>
        {
            Assert.That(InvokeGetContainer(toolbox), Is.TypeOf<ToolboxItem>());
            Assert.That(InvokeIsOwnContainer(toolbox, explicitItem), Is.True);
            Assert.That(InvokeIsOwnContainer(toolbox, new object()), Is.False);
        });
    }

    [Test]
    public void ToolboxItem_UsesToolItemContainersAndPreservesExplicitContainers()
    {
        var toolboxItem = new ToolboxItem();
        var explicitItem = new ToolItem();

        Assert.Multiple(() =>
        {
            Assert.That(InvokeGetContainer(toolboxItem), Is.TypeOf<ToolItem>());
            Assert.That(InvokeIsOwnContainer(toolboxItem, explicitItem), Is.True);
            Assert.That(InvokeIsOwnContainer(toolboxItem, new object()), Is.False);
        });
    }

    [Test]
    public void ClosePopup_IsCallableWhenNoItemIsActive()
    {
        var toolbox = new Junevy.Controls.Controls.Toolbox.Toolbox();

        Assert.DoesNotThrow(toolbox.ClosePopup);
        Assert.That(toolbox.ActiveItem, Is.Null);
    }

    private static DependencyObject InvokeGetContainer(ItemsControl itemsControl)
    {
        MethodInfo method = itemsControl.GetType().GetMethod(
            "GetContainerForItemOverride",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        return (DependencyObject)method.Invoke(itemsControl, null)!;
    }

    private static object? ReadDefaultStyleKey(FrameworkElement element)
    {
        PropertyInfo property = typeof(FrameworkElement).GetProperty(
            "DefaultStyleKey",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        return property.GetValue(element);
    }

    private static bool InvokeIsOwnContainer(ItemsControl itemsControl, object item)
    {
        MethodInfo method = itemsControl.GetType().GetMethod(
            "IsItemItsOwnContainerOverride",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        return (bool)method.Invoke(itemsControl, new[] { item })!;
    }
}
#endif
