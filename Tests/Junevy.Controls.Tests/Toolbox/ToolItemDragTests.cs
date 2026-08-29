using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Junevy.Controls.Controls.Toolbox;
using NUnit.Framework;
using ToolboxControl = Junevy.Controls.Controls.Toolbox.Toolbox;

namespace Junevy.Controls.Tests.Toolbox;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ToolItemDragTests
{
    [TestCase(0d, 0d)]
    [TestCase(3.99d, 5.99d)]
    [TestCase(-3.99d, -5.99d)]
    [TestCase(4d, 6d)]
    public void ExceedsDragThreshold_WhenNeitherAxisExceedsThreshold_ReturnsFalse(
        double currentX,
        double currentY)
    {
        Assert.That(
            ToolItem.ExceedsDragThreshold(new Point(0d, 0d), new Point(currentX, currentY), 4d, 6d),
            Is.False);
    }

    [TestCase(4.01d, 0d)]
    [TestCase(-4.01d, 0d)]
    [TestCase(0d, 6.01d)]
    [TestCase(0d, -6.01d)]
    public void ExceedsDragThreshold_WhenEitherAxisExceedsThreshold_ReturnsTrue(
        double currentX,
        double currentY)
    {
        Assert.That(
            ToolItem.ExceedsDragThreshold(new Point(0d, 0d), new Point(currentX, currentY), 4d, 6d),
            Is.True);
    }

    [Test]
    public void CreateDragDataObject_UsesExactEffectiveFormatAndPayloadReference()
    {
        object payload = new();
        var toolbox = new ToolboxControl { DragDataFormat = "Application.ToolDefinition" };
        var group = CreateGroup();
        var tool = new ToolItem { DragData = payload };
        toolbox.Items.Add(group);
        group.Items.Add(tool);
        AttachOwners(toolbox, group, tool);

        DataObject? data = tool.CreateDragDataObject();

        Assert.Multiple(() =>
        {
            Assert.That(data, Is.Not.Null);
            Assert.That(data!.GetDataPresent("Application.ToolDefinition", false), Is.True);
            Assert.That(data.GetFormats(false), Is.EqualTo(new[] { "Application.ToolDefinition" }));
            Assert.That(data.GetData("Application.ToolDefinition", false), Is.SameAs(payload));
            Assert.That(data.GetDataPresent(DataFormats.Serializable, false), Is.False);
        });
    }

    [Test]
    public void CreateDragDataObject_WhenDragCannotStart_ReturnsNull()
    {
        object payload = new();

        Assert.Multiple(() =>
        {
            Assert.That(new ToolItem { IsDragEnabled = false, DragData = payload, DragDataFormat = "Tool" }.CreateDragDataObject(), Is.Null);
            Assert.That(new ToolItem { DragData = null, DragDataFormat = "Tool" }.CreateDragDataObject(), Is.Null);
            Assert.That(new ToolItem { DragData = payload, DragDataFormat = null }.CreateDragDataObject(), Is.Null);
            Assert.That(new ToolItem { DragData = payload, DragDataFormat = "   " }.CreateDragDataObject(), Is.Null);
        });
    }

    [Test]
    public void ContinueDragGesture_BelowThresholdDoesNotStartThenExceededThresholdStartsOnce()
    {
        (ToolboxControl _, ToolboxItem _, ToolItem tool) = CreateOpenOwnedTool(new object());
        int executions = 0;
        tool.DragExecutor = (_, _, effects) =>
        {
            executions++;
            return effects;
        };
        tool.BeginDragGesture(new Point(10d, 20d));

        DragDropEffects? below = tool.ContinueDragGesture(
            new Point(10d + SystemParameters.MinimumHorizontalDragDistance, 20d),
            MouseButtonState.Pressed);
        DragDropEffects? exceeded = tool.ContinueDragGesture(
            new Point(10d + SystemParameters.MinimumHorizontalDragDistance + 0.01d, 20d),
            MouseButtonState.Pressed);
        DragDropEffects? repeated = tool.ContinueDragGesture(
            new Point(100d, 100d),
            MouseButtonState.Pressed);

        Assert.Multiple(() =>
        {
            Assert.That(below, Is.Null);
            Assert.That(exceeded, Is.EqualTo(DragDropEffects.Copy));
            Assert.That(repeated, Is.Null);
            Assert.That(executions, Is.EqualTo(1));
        });
    }

    [Test]
    public void ContinueDragGesture_WhenLeftButtonIsReleased_ClearsRecordedStart()
    {
        (ToolboxControl _, ToolboxItem _, ToolItem tool) = CreateOpenOwnedTool(new object());
        int executions = 0;
        tool.DragExecutor = (_, _, effects) =>
        {
            executions++;
            return effects;
        };
        tool.BeginDragGesture(new Point());

        Assert.That(
            tool.ContinueDragGesture(new Point(100d, 100d), MouseButtonState.Released),
            Is.Null);
        Assert.That(
            tool.ContinueDragGesture(new Point(100d, 100d), MouseButtonState.Pressed),
            Is.Null);
        Assert.That(executions, Is.Zero);
    }

    [Test]
    public void ExecuteDrag_UsesCopyAndHoldsRootDragLockUntilCompletion()
    {
        object payload = new();
        (ToolboxControl toolbox, ToolboxItem group, ToolItem tool) = CreateOpenOwnedTool(payload);
        DragDropEffects observedAllowedEffects = DragDropEffects.None;
        object? observedPayload = null;
        bool lockObservedInsideExecutor = false;
        tool.DragExecutor = (source, data, allowedEffects) =>
        {
            observedAllowedEffects = allowedEffects;
            observedPayload = data.GetData(tool.EffectiveDragDataFormat!, false);
            lockObservedInsideExecutor = toolbox.IsDragInProgress;
            group.SetPointerOverTrigger(false);
            group.SetPointerOverPopup(false);
            toolbox.RequestClose(group);
            return DragDropEffects.Copy;
        };

        DragDropEffects result = tool.ExecuteDrag(tool.CreateDragDataObject()!);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(DragDropEffects.Copy));
            Assert.That(observedAllowedEffects, Is.EqualTo(DragDropEffects.Copy));
            Assert.That(observedPayload, Is.SameAs(payload));
            Assert.That(lockObservedInsideExecutor, Is.True);
            Assert.That(toolbox.IsDragInProgress, Is.False);
            Assert.That(group.IsOpen, Is.False, "Completion closes only after both hover regions have left.");
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ExecuteDrag_CancellationOrExceptionAlwaysReleasesLockAndClosesOutsidePopup(bool throwFromExecutor)
    {
        (ToolboxControl toolbox, ToolboxItem group, ToolItem tool) = CreateOpenOwnedTool(new object());
        group.SetPointerOverTrigger(false);
        group.SetPointerOverPopup(false);
        tool.DragExecutor = (_, _, _) => throwFromExecutor
            ? throw new InvalidOperationException("OLE failure")
            : DragDropEffects.None;

        if (throwFromExecutor)
        {
            Assert.Throws<InvalidOperationException>(() => tool.ExecuteDrag(tool.CreateDragDataObject()!));
        }
        else
        {
            Assert.That(tool.ExecuteDrag(tool.CreateDragDataObject()!), Is.EqualTo(DragDropEffects.None));
        }

        Assert.Multiple(() =>
        {
            Assert.That(toolbox.IsDragInProgress, Is.False);
            Assert.That(group.IsOpen, Is.False);
            Assert.That(tool.SuppressClick, Is.True, "The matching mouse gesture still owns click suppression.");
        });
    }

    [Test]
    public void ExecuteDrag_WhenPointerRemainsOverPopup_KeepsPopupOpen()
    {
        (ToolboxControl toolbox, ToolboxItem group, ToolItem tool) = CreateOpenOwnedTool(new object());
        group.SetPointerOverTrigger(false);
        group.SetPointerOverPopup(true);
        tool.DragExecutor = (_, _, _) => DragDropEffects.None;

        tool.ExecuteDrag(tool.CreateDragDataObject()!);

        Assert.Multiple(() =>
        {
            Assert.That(toolbox.IsDragInProgress, Is.False);
            Assert.That(group.IsOpen, Is.True);
        });
    }

    [Test]
    public void OnClick_AfterDragSuppressesOnlyThatGestureThenPreservesButtonCommandBehavior()
    {
        (ToolboxControl _, ToolboxItem _, ToolItem tool) = CreateOpenOwnedTool(new object());
        int executions = 0;
        tool.Command = new TestCommand(() => executions++);
        tool.DragExecutor = (_, _, _) => DragDropEffects.Copy;
        tool.ExecuteDrag(tool.CreateDragDataObject()!);

        InvokeOnClick(tool);
        Assert.Multiple(() =>
        {
            Assert.That(executions, Is.Zero);
            Assert.That(tool.SuppressClick, Is.False);
        });

        InvokeOnClick(tool);
        Assert.That(executions, Is.EqualTo(1));
    }

    private static (ToolboxControl Toolbox, ToolboxItem Group, ToolItem Tool) CreateOpenOwnedTool(object payload)
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero, CloseDelay = TimeSpan.Zero };
        var group = new ToolboxItem();
        var tool = new ToolItem { DragData = payload };
        group.Items.Add(tool);
        toolbox.Items.Add(group);
        AttachOwners(toolbox, group, tool);
        group.SetPointerOverTrigger(true);
        toolbox.Toggle(group, false);
        Assert.That(group.IsOpen, Is.True);
        return (toolbox, group, tool);
    }

    private static ToolboxItem CreateGroup()
    {
        return new ToolboxItem();
    }

    private static void AttachOwners(ToolboxControl toolbox, ToolboxItem group, ToolItem tool)
    {
        group.AttachOwner(toolbox);
        tool.AttachOwner(group);
    }

    private static void InvokeOnClick(ToolItem tool)
    {
        MethodInfo method = typeof(ToolItem).GetMethod(
            "OnClick",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        method.Invoke(tool, null);
    }

    private sealed class TestCommand : ICommand
    {
        private readonly Action _execute;

        internal TestCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute();
    }
}
