using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Junevy.Controls.Controls.Toolbox;
using NUnit.Framework;
using ToolboxControl = Junevy.Controls.Controls.Toolbox.Toolbox;

namespace Junevy.Controls.Tests.Toolbox;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ToolboxContainerTests
{
    private enum TriggerPayloadKind
    {
        Null,
        Distinct,
        SameAsGenerated
    }

    [Test]
    public void ToolItem_FreshDragDataPreservesThePublicDependencyPropertyContract()
    {
        var item = new ToolItem();
        PropertyMetadata metadata = ToolItem.DragDataProperty.GetMetadata(typeof(ToolItem));
        ValueSource source = DependencyPropertyHelper.GetValueSource(item, ToolItem.DragDataProperty);

        Assert.Multiple(() =>
        {
            Assert.That(metadata.DefaultValue, Is.Null);
            Assert.That(item.DragData, Is.Null);
            Assert.That(item.GetValue(ToolItem.DragDataProperty), Is.Null);
            Assert.That(item.ReadLocalValue(ToolItem.DragDataProperty), Is.SameAs(DependencyProperty.UnsetValue));
            Assert.That(source.BaseValueSource, Is.EqualTo(BaseValueSource.Default));
            Assert.That(source.IsCoerced, Is.False);
        });
    }

    [Test]
    public void ExplicitContainers_AttachAndDetachTheirOwners()
    {
        var toolbox = new ToolboxControl();
        var group = CreateGroup();
        var tool = new ToolItem { DragData = new object() };

        InvokePrepare(toolbox, group, group);
        InvokePrepare(group, tool, tool);

        Assert.Multiple(() =>
        {
            Assert.That(group.Owner, Is.SameAs(toolbox));
            Assert.That(tool.Owner, Is.SameAs(group));
            Assert.That(tool.EffectiveDragDataFormat, Is.EqualTo(toolbox.DragDataFormat));
        });

        InvokeClear(group, tool, tool);
        InvokeClear(toolbox, group, group);

        Assert.Multiple(() =>
        {
            Assert.That(group.Owner, Is.Null);
            Assert.That(tool.Owner, Is.Null);
            Assert.That(tool.DragData, Is.Not.Null, "Explicit payloads must survive container cleanup.");
        });
    }

    [Test]
    public void DragDataFormat_TracksRootChangesUnlessTheToolHasAnExplicitValue()
    {
        var toolbox = new ToolboxControl();
        var group = CreateGroup();
        var inherited = new ToolItem();
        var overridden = new ToolItem { DragDataFormat = "Application.CustomTool" };

        InvokePrepare(toolbox, group, group);
        InvokePrepare(group, inherited, inherited);
        InvokePrepare(group, overridden, overridden);

        Assert.Multiple(() =>
        {
            Assert.That(inherited.EffectiveDragDataFormat, Is.EqualTo("Junevy.Controls.Tool"));
            Assert.That(overridden.EffectiveDragDataFormat, Is.EqualTo("Application.CustomTool"));
        });

        toolbox.DragDataFormat = "Application.RuntimeTool";

        Assert.Multiple(() =>
        {
            Assert.That(inherited.EffectiveDragDataFormat, Is.EqualTo("Application.RuntimeTool"));
            Assert.That(overridden.EffectiveDragDataFormat, Is.EqualTo("Application.CustomTool"));
        });
    }

    [Test]
    public void CurrentDragDataFormat_TakesPrecedenceOverTheRootFormat()
    {
        var toolbox = new ToolboxControl();
        var group = CreateGroup();
        var tool = new ToolItem();
        tool.SetCurrentValue(ToolItem.DragDataFormatProperty, "Application.CurrentTool");

        InvokePrepare(toolbox, group, group);
        InvokePrepare(group, tool, tool);
        toolbox.DragDataFormat = "Application.RuntimeTool";

        Assert.That(tool.EffectiveDragDataFormat, Is.EqualTo("Application.CurrentTool"));
    }

    [Test]
    public void GeneratedContainer_AssignsAndClearsOwnedDragPayloadAcrossRecycling()
    {
        var group = new ToolboxItem();
        var container = new ToolItem();
        var first = new object();
        var second = new object();

        InvokePrepare(group, container, first);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, first);

        InvokeClear(group, container, first);
        Assert.That(container.DragData, Is.Null);

        InvokePrepare(group, container, second);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, second);

        InvokeClear(group, container, second);
        Assert.That(container.DragData, Is.Null);
    }

    [Test]
    public void GeneratedContainer_ExternalCoercionKeepsEffectivePayloadAndOwnershipConsistent()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();

        InvokePrepare(group, container, payload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, payload);

        Assert.Multiple(() =>
        {
            Assert.That(GetPrivateField<bool>(container, "_ownsGeneratedDragData"), Is.True);
            Assert.That(GetPrivateField<object>(container, "_generatedDragData"), Is.SameAs(payload));
        });

        InvokeClear(group, container, payload);
        AssertGeneratedStateReleased(container);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void GeneratedContainer_OperationFailureReleasesMarkerPayloadAndOperation(int operationValue)
    {
        var failureOperation = (ToolItem.GeneratedDragDataOperation)operationValue;
        var payload = new object();
        var nextPayload = new object();
        var container = new ToolItem();

        if (failureOperation != ToolItem.GeneratedDragDataOperation.ApplyState)
        {
            container.SetGeneratedDragData(payload);
        }

        container.GeneratedDragDataOperationCompletedForTest = operation =>
        {
            if (operation == failureOperation)
            {
                throw new InvalidOperationException("Injected generated DragData operation failure.");
            }
        };

        if (failureOperation == ToolItem.GeneratedDragDataOperation.ApplyState)
        {
            Assert.Throws<InvalidOperationException>(() => container.SetGeneratedDragData(payload));
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => container.ClearGeneratedDragData(payload));
        }

        container.GeneratedDragDataOperationCompletedForTest = null;
        AssertGeneratedStateReleased(container);

        container.SetGeneratedDragData(nextPayload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, nextPayload);
        container.ClearGeneratedDragData(nextPayload);
        AssertGeneratedStateReleased(container);
    }

    [Test]
    public void GeneratedContainer_DoesNotOverwriteStylePayload()
    {
        var group = new ToolboxItem();
        var container = new ToolItem();
        var stylePayload = new object();
        var style = new Style(typeof(ToolItem));
        style.Setters.Add(new Setter(ToolItem.DragDataProperty, stylePayload));
        container.Style = style;

        InvokePrepare(group, container, new object());

        Assert.That(container.DragData, Is.SameAs(stylePayload));
        InvokeClear(group, container, new object());
    }

    [Test]
    public void GeneratedContainer_DoesNotOverwriteLocalPayload()
    {
        var group = new ToolboxItem();
        var localPayload = new object();
        var container = new ToolItem { DragData = localPayload };

        InvokePrepare(group, container, new object());

        Assert.That(container.DragData, Is.SameAs(localPayload));
        InvokeClear(group, container, new object());
        Assert.That(container.DragData, Is.SameAs(localPayload));
    }

    [Test]
    public void GeneratedContainer_DoesNotOverwriteBinding()
    {
        var group = new ToolboxItem();
        var source = new TextBlock { Tag = new object() };
        var container = new ToolItem();
        BindingOperations.SetBinding(
            container,
            ToolItem.DragDataProperty,
            new Binding(nameof(FrameworkElement.Tag)) { Source = source });
        container.CoerceValue(ToolItem.DragDataProperty);

        InvokePrepare(group, container, new object());

        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(source.Tag));
            Assert.That(BindingOperations.GetBindingExpression(container, ToolItem.DragDataProperty), Is.Not.Null);
        });
        InvokeClear(group, container, new object());
    }

    [Test]
    public void GeneratedContainer_DoesNotOverwritePreexistingCurrentPayload()
    {
        var group = new ToolboxItem();
        var currentPayload = new object();
        var container = new ToolItem();
        container.SetCurrentValue(ToolItem.DragDataProperty, currentPayload);

        InvokePrepare(group, container, new object());

        Assert.That(container.DragData, Is.SameAs(currentPayload));
    }

    [Test]
    public void GeneratedContainer_DoesNotOverwritePreexistingCurrentNull()
    {
        var group = new ToolboxItem();
        var container = new ToolItem();
        container.SetCurrentValue(ToolItem.DragDataProperty, null);

        InvokePrepare(group, container, new object());

        ValueSource source = DependencyPropertyHelper.GetValueSource(
            container,
            ToolItem.DragDataProperty);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.Null);
            Assert.That(source.IsCurrent, Is.True);
            Assert.That(GetPrivateField<bool>(container, "_ownsGeneratedDragData"), Is.False);
            Assert.That(GetPrivateField<object?>(container, "_generatedDragData"), Is.Null);
            Assert.That(GetPrivateField<object?>(container, "_generatedStateMarker"), Is.Null);
        });
    }

    [Test]
    public void GeneratedContainer_ClearPreservesLaterBindingWithTheSamePayload()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, payload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, payload);
        var source = new TextBlock { Tag = payload };
        BindingOperations.SetBinding(
            container,
            ToolItem.DragDataProperty,
            new Binding(nameof(FrameworkElement.Tag)) { Source = source });
        container.CoerceValue(ToolItem.DragDataProperty);

        InvokeClear(group, container, payload);

        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(payload));
            Assert.That(BindingOperations.GetBindingExpression(container, ToolItem.DragDataProperty), Is.Not.Null);
        });
    }

    [Test]
    public void GeneratedContainer_ClearPreservesLaterLocalValueWithTheSamePayload()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, payload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, payload);
        container.DragData = payload;
        container.CoerceValue(ToolItem.DragDataProperty);

        InvokeClear(group, container, payload);

        Assert.That(container.DragData, Is.SameAs(payload));
        Assert.That(container.ReadLocalValue(ToolItem.DragDataProperty), Is.SameAs(payload));
    }

    [Test]
    public void GeneratedContainer_ClearPreservesLaterCurrentValueWithTheSamePayload()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, payload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, payload);
        container.SetCurrentValue(ToolItem.DragDataProperty, payload);
        container.CoerceValue(ToolItem.DragDataProperty);

        InvokeClear(group, container, payload);

        ValueSource source = DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(payload));
            Assert.That(source.IsCurrent, Is.True);
        });
    }

    [Test]
    public void GeneratedContainer_ClearPreservesLaterStyleWithTheSamePayloadAfterExternalCoercion()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, payload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, payload);
        var style = new Style(typeof(ToolItem));
        style.Setters.Add(new Setter(ToolItem.DragDataProperty, payload));

        container.Style = style;
        container.CoerceValue(ToolItem.DragDataProperty);

        ValueSource beforeClear = DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(payload));
            Assert.That(beforeClear.BaseValueSource, Is.EqualTo(BaseValueSource.Style));
            Assert.That(beforeClear.IsCoerced, Is.False);
        });

        InvokeClear(group, container, payload);
        ValueSource afterClear = DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(payload));
            Assert.That(afterClear.BaseValueSource, Is.EqualTo(BaseValueSource.Style));
            Assert.That(afterClear.IsCoerced, Is.False);
        });
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void GeneratedContainer_InitiallyInactiveStyleTriggerTakesPrecedenceWhenActivated(
        int payloadKindValue)
    {
        var payloadKind = (TriggerPayloadKind)payloadKindValue;
        var group = new ToolboxItem();
        var generatedPayload = new object();
        object? triggerPayload = CreateTriggerPayload(payloadKind, generatedPayload);
        var container = new ToolItem
        {
            Style = CreateDragDataTriggerStyle(triggerPayload)
        };

        InvokePrepare(group, container, generatedPayload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, generatedPayload);

        ActivateDragDataTrigger(container);

        AssertTriggerPayloadOwnsDragData(container, triggerPayload);
        container.CoerceValue(ToolItem.DragDataProperty);
        AssertTriggerPayloadOwnsDragData(container, triggerPayload);

        InvokeClear(group, container, generatedPayload);
        AssertTriggerPayloadOwnsDragData(container, triggerPayload);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void GeneratedContainer_LaterInactiveStyleTriggerTakesPrecedenceWhenActivated(
        int payloadKindValue)
    {
        var payloadKind = (TriggerPayloadKind)payloadKindValue;
        var group = new ToolboxItem();
        var generatedPayload = new object();
        object? triggerPayload = CreateTriggerPayload(payloadKind, generatedPayload);
        var container = new ToolItem();

        InvokePrepare(group, container, generatedPayload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, generatedPayload);
        container.Style = CreateDragDataTriggerStyle(triggerPayload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, generatedPayload);

        ActivateDragDataTrigger(container);

        AssertTriggerPayloadOwnsDragData(container, triggerPayload);
        container.CoerceValue(ToolItem.DragDataProperty);
        AssertTriggerPayloadOwnsDragData(container, triggerPayload);

        InvokeClear(group, container, generatedPayload);
        AssertTriggerPayloadOwnsDragData(container, triggerPayload);
    }

    [Test]
    public void GeneratedContainer_LaterLocalNullTakesEffectAndSurvivesClear()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, payload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, payload);

        container.DragData = null;
        container.CoerceValue(ToolItem.DragDataProperty);

        Assert.That(container.DragData, Is.Null, "A later local null must take effect before recycling.");
        InvokeClear(group, container, payload);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.Null);
            Assert.That(container.ReadLocalValue(ToolItem.DragDataProperty), Is.Null);
            Assert.That(DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty).BaseValueSource,
                Is.EqualTo(BaseValueSource.Local));
        });
    }

    [Test]
    public void GeneratedContainer_LaterBindingToNullTakesEffectAndSurvivesClear()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, payload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, payload);
        var source = new TextBlock { Tag = null };

        BindingOperations.SetBinding(
            container,
            ToolItem.DragDataProperty,
            new Binding(nameof(FrameworkElement.Tag)) { Source = source });
        container.CoerceValue(ToolItem.DragDataProperty);

        Assert.That(container.DragData, Is.Null, "A later Binding-to-null must take effect before recycling.");
        InvokeClear(group, container, payload);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.Null);
            Assert.That(BindingOperations.GetBindingExpression(container, ToolItem.DragDataProperty), Is.Not.Null);
            Assert.That(DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty).IsExpression,
                Is.True);
        });
    }

    [Test]
    public void GeneratedContainer_LaterStyleNullTakesEffectAndSurvivesClear()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, payload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, payload);
        var style = new Style(typeof(ToolItem));
        style.Setters.Add(new Setter(ToolItem.DragDataProperty, null));

        container.Style = style;
        container.CoerceValue(ToolItem.DragDataProperty);

        Assert.That(container.DragData, Is.Null, "A later Style null must take effect before recycling.");
        InvokeClear(group, container, payload);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.Null);
            Assert.That(DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty).BaseValueSource,
                Is.EqualTo(BaseValueSource.Style));
        });
    }

    [Test]
    public void GeneratedContainer_LaterCurrentNullTakesEffectAndSurvivesClear()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, payload);
        AssertGeneratedPayloadSurvivesExternalCoercion(container, payload);

        container.SetCurrentValue(ToolItem.DragDataProperty, null);
        container.CoerceValue(ToolItem.DragDataProperty);

        ValueSource beforeClear = DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.Null, "A later current null must take effect before recycling.");
            Assert.That(beforeClear.IsCurrent, Is.True);
            Assert.That(GetPrivateField<bool>(container, "_ownsGeneratedDragData"), Is.False);
            Assert.That(GetPrivateField<object?>(container, "_generatedDragData"), Is.Null);
            Assert.That(GetPrivateField<object?>(container, "_generatedStateMarker"), Is.Null);
        });

        InvokeClear(group, container, payload);
        ValueSource afterClear = DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.Null);
            Assert.That(afterClear.IsCurrent, Is.True);
        });
    }

    [Test]
    public void ZeroDelayOpening_AThenB_LeavesOnlyBActive()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero };
        ToolboxItem first = CreateGroup();
        ToolboxItem second = CreateGroup();
        Window? window = null;

        try
        {
            InvokePrepare(toolbox, first, first);
            InvokePrepare(toolbox, second, second);
            first.SetPointerOverTrigger(true);
            second.SetPointerOverTrigger(true);
            window = WpfTestHost.Show(toolbox, first, second);

            toolbox.RequestOpen(first);
            WpfTestHost.Drain(window.Dispatcher);
            toolbox.RequestOpen(second);
            WpfTestHost.Drain(window.Dispatcher);

            Assert.Multiple(() =>
            {
                Assert.That(first.IsOpen, Is.False);
                Assert.That(second.IsOpen, Is.True);
                Assert.That(toolbox.ActiveItem, Is.SameAs(second));
            });
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void LeavingPendingGroup_RestoresCloseScheduleForTheActiveGroup()
    {
        var toolbox = new ToolboxControl
        {
            OpenDelay = TimeSpan.Zero,
            CloseDelay = TimeSpan.FromMilliseconds(40)
        };
        ToolboxItem active = CreateGroup();
        ToolboxItem pending = CreateGroup();
        Window? window = null;

        try
        {
            InvokePrepare(toolbox, active, active);
            InvokePrepare(toolbox, pending, pending);
            active.SetPointerOverTrigger(true);
            pending.SetPointerOverTrigger(false);
            window = WpfTestHost.Show(toolbox, active, pending);
            Open(toolbox, active, window);

            active.SetPointerOverTrigger(false);
            toolbox.RequestClose(active);

            toolbox.OpenDelay = TimeSpan.FromMilliseconds(200);
            pending.SetPointerOverTrigger(true);
            toolbox.RequestOpen(pending);
            pending.SetPointerOverTrigger(false);
            toolbox.RequestClose(pending);

            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(100));

            Assert.Multiple(() =>
            {
                Assert.That(active.IsOpen, Is.False);
                Assert.That(pending.IsOpen, Is.False);
                Assert.That(toolbox.ActiveItem, Is.Null);
            });
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void PendingOpen_RevalidatesStaleDetachedDisabledEmptyAndPointerLeftTargets()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.FromMilliseconds(10) };
        ToolboxItem stale = CreateGroup();
        ToolboxItem replacement = CreateGroup();
        ToolboxItem detached = CreateGroup();
        ToolboxItem disabled = CreateGroup();
        var empty = new ToolboxItem();
        ToolboxItem pointerLeft = CreateGroup();
        Window? window = null;

        try
        {
            foreach (ToolboxItem item in new[] { stale, replacement, detached, disabled, empty, pointerLeft })
            {
                InvokePrepare(toolbox, item, item);
                item.SetPointerOverTrigger(true);
            }

            window = WpfTestHost.Show(toolbox, stale, replacement, detached, disabled, empty, pointerLeft);

            toolbox.RequestOpen(stale);
            toolbox.RequestOpen(replacement);
            replacement.SetPointerOverTrigger(false);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(30));
            Assert.That(stale.IsOpen, Is.False, "A replaced pending target must not become active.");

            toolbox.RequestOpen(detached);
            InvokeClear(toolbox, detached, detached);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(30));

            disabled.IsEnabled = false;
            toolbox.RequestOpen(disabled);
            toolbox.RequestOpen(empty);
            pointerLeft.SetPointerOverTrigger(false);
            toolbox.RequestOpen(pointerLeft);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(30));

            Assert.Multiple(() =>
            {
                Assert.That(detached.IsOpen, Is.False);
                Assert.That(disabled.IsOpen, Is.False);
                Assert.That(empty.IsOpen, Is.False);
                Assert.That(pointerLeft.IsOpen, Is.False);
                Assert.That(toolbox.ActiveItem, Is.Null);
            });
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void PendingOpen_IsCancelledWhenTheTargetBecomesDisabledOrEmpty()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.FromMilliseconds(20) };
        ToolboxItem disabled = CreateGroup();
        ToolboxItem emptied = CreateGroup();
        Window? window = null;

        try
        {
            InvokePrepare(toolbox, disabled, disabled);
            InvokePrepare(toolbox, emptied, emptied);
            disabled.SetPointerOverTrigger(true);
            emptied.SetPointerOverTrigger(true);
            window = WpfTestHost.Show(toolbox, disabled, emptied);

            toolbox.RequestOpen(disabled);
            disabled.IsEnabled = false;
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(40));

            toolbox.RequestOpen(emptied);
            emptied.Items.Clear();
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(40));

            Assert.Multiple(() =>
            {
                Assert.That(disabled.IsOpen, Is.False);
                Assert.That(emptied.IsOpen, Is.False);
                Assert.That(toolbox.ActiveItem, Is.Null);
            });
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void ClosePopupAndContainerClear_CancelPendingAndActiveStateSynchronously()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero };
        ToolboxItem active = CreateGroup();
        ToolboxItem pending = CreateGroup();
        Window? window = null;

        try
        {
            InvokePrepare(toolbox, active, active);
            InvokePrepare(toolbox, pending, pending);
            active.SetPointerOverTrigger(true);
            pending.SetPointerOverTrigger(true);
            window = WpfTestHost.Show(toolbox, active, pending);

            toolbox.RequestOpen(active);
            WpfTestHost.Drain(window.Dispatcher);
            toolbox.OpenDelay = TimeSpan.FromMilliseconds(10);
            toolbox.RequestOpen(pending);
            toolbox.ClosePopup();
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(30));

            Assert.Multiple(() =>
            {
                Assert.That(active.IsOpen, Is.False);
                Assert.That(pending.IsOpen, Is.False);
                Assert.That(toolbox.ActiveItem, Is.Null);
            });

            toolbox.OpenDelay = TimeSpan.Zero;
            toolbox.RequestOpen(active);
            WpfTestHost.Drain(window.Dispatcher);
            InvokeClear(toolbox, active, active);

            Assert.Multiple(() =>
            {
                Assert.That(active.IsOpen, Is.False);
                Assert.That(active.Owner, Is.Null);
                Assert.That(toolbox.ActiveItem, Is.Null);
            });
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void UnloadDeactivationAndMinimization_CloseActiveState()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero };
        ToolboxItem group = CreateGroup();
        Window? window = null;

        try
        {
            InvokePrepare(toolbox, group, group);
            group.SetPointerOverTrigger(true);
            window = WpfTestHost.Show(toolbox, group);
            var panel = (Panel)window.Content;

            Open(toolbox, group, window);
            InvokeNonPublic(toolbox, "OnHostDeactivated", window, EventArgs.Empty);
            AssertClosed(toolbox, group);

            Open(toolbox, group, window);
            window.WindowState = WindowState.Minimized;
            WpfTestHost.Drain(window.Dispatcher);
            AssertClosed(toolbox, group);
            window.WindowState = WindowState.Normal;
            WpfTestHost.Drain(window.Dispatcher);

            Open(toolbox, group, window);
            panel.Children.Remove(toolbox);
            WpfTestHost.Drain(window.Dispatcher);
            AssertClosed(toolbox, group);
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void DisablingOrEmptyingTheActiveGroup_ClosesItSynchronously()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero };
        ToolboxItem group = CreateGroup();
        Window? window = null;

        try
        {
            InvokePrepare(toolbox, group, group);
            group.SetPointerOverTrigger(true);
            window = WpfTestHost.Show(toolbox, group);

            Open(toolbox, group, window);
            group.IsEnabled = false;
            AssertClosed(toolbox, group);

            group.IsEnabled = true;
            Open(toolbox, group, window);
            group.Items.Clear();
            AssertClosed(toolbox, group);
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void DragLock_PreventsTimedCloseAndCompletionUsesBothPointerRegions()
    {
        var toolbox = new ToolboxControl
        {
            OpenDelay = TimeSpan.Zero,
            CloseDelay = TimeSpan.FromMilliseconds(10)
        };
        ToolboxItem group = CreateGroup();
        Window? window = null;

        try
        {
            InvokePrepare(toolbox, group, group);
            group.SetPointerOverTrigger(true);
            window = WpfTestHost.Show(toolbox, group);
            Open(toolbox, group, window);

            group.SetPointerOverTrigger(false);
            toolbox.RequestClose(group);
            toolbox.NotifyDragStarted(group);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(30));
            Assert.That(group.IsOpen, Is.True);

            group.SetPointerOverPopup(true);
            toolbox.NotifyDragCompleted(group);
            Assert.That(group.IsOpen, Is.True, "Pointer over the popup keeps it open after drag completion.");

            toolbox.NotifyDragStarted(group);
            group.SetPointerOverPopup(false);
            toolbox.NotifyDragCompleted(group);
            AssertClosed(toolbox, group);
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void DragCompletionForPreviousActiveGroup_ReleasesOnlyItsOwnLock()
    {
        var toolbox = new ToolboxControl
        {
            OpenDelay = TimeSpan.Zero,
            CloseDelay = TimeSpan.FromMilliseconds(10)
        };
        ToolboxItem first = CreateGroup();
        ToolboxItem second = CreateGroup();
        ToolboxItem foreign = CreateGroup();
        var foreignOwner = new ToolboxControl();
        Window? window = null;

        try
        {
            InvokePrepare(toolbox, first, first);
            InvokePrepare(toolbox, second, second);
            InvokePrepare(foreignOwner, foreign, foreign);
            first.SetPointerOverTrigger(true);
            second.SetPointerOverTrigger(true);
            window = WpfTestHost.Show(toolbox, first, second);
            Open(toolbox, first, window);

            toolbox.NotifyDragStarted(first);
            toolbox.NotifyDragCompleted(foreign);
            toolbox.RequestOpen(second);
            WpfTestHost.Drain(window.Dispatcher);
            Assert.That(toolbox.ActiveItem, Is.SameAs(second));

            second.SetPointerOverTrigger(false);
            toolbox.RequestClose(second);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(30));
            Assert.That(second.IsOpen, Is.True, "A foreign completion must not release A's drag lock.");

            toolbox.NotifyDragCompleted(first);
            toolbox.RequestClose(second);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(30));
            AssertClosed(toolbox, second);
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void ForeignAndDetachedCoordinationCalls_DoNotMutateActiveStateOrDragLock()
    {
        var toolbox = new ToolboxControl
        {
            OpenDelay = TimeSpan.Zero,
            CloseDelay = TimeSpan.FromMilliseconds(10)
        };
        ToolboxItem active = CreateGroup();
        ToolboxItem foreign = CreateGroup();
        ToolboxItem detached = CreateGroup();
        var foreignOwner = new ToolboxControl();
        Window? window = null;

        try
        {
            InvokePrepare(toolbox, active, active);
            InvokePrepare(foreignOwner, foreign, foreign);
            active.SetPointerOverTrigger(true);
            window = WpfTestHost.Show(toolbox, active);
            Open(toolbox, active, window);

            active.SetPointerOverTrigger(false);
            toolbox.RequestClose(foreign);
            toolbox.RequestClose(detached);
            toolbox.NotifyDragStarted(foreign);
            toolbox.NotifyDragStarted(detached);
            toolbox.NotifyDragCompleted(foreign);
            toolbox.NotifyDragCompleted(detached);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(30));
            Assert.That(active.IsOpen, Is.True, "Foreign or detached calls must not close the active item.");

            toolbox.RequestClose(active);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(30));
            AssertClosed(toolbox, active);
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void RepeatedLoadUnload_DoesNotDuplicateWindowLifecycleSubscriptions()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero };
        ToolboxItem group = CreateGroup();
        Window? window = null;
        int repositionRequests = 0;
        toolbox.RepositionRequested = () => repositionRequests++;

        try
        {
            InvokePrepare(toolbox, group, group);
            group.SetPointerOverTrigger(true);
            window = WpfTestHost.Show(toolbox, group);
            var panel = (Panel)window.Content;

            for (int cycle = 1; cycle <= 2; cycle++)
            {
                Open(toolbox, group, window);
                window.Left += 1d;
                WpfTestHost.Drain(window.Dispatcher);
                Assert.That(repositionRequests, Is.EqualTo(cycle));

                panel.Children.Remove(toolbox);
                WpfTestHost.Drain(window.Dispatcher);
                panel.Children.Insert(0, toolbox);
                WpfTestHost.PumpUntil(
                    window.Dispatcher,
                    () => toolbox.IsLoaded,
                    TimeSpan.FromSeconds(1));
            }
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void TestHost_SetupFailureClosesAndDrainsTheShownWindow()
    {
        Window? observedWindow = null;
        int initialOpenWindowCount = WpfTestHost.OpenWindowCount;

        try
        {
            Assert.Throws<AssertionException>(() =>
                WpfTestHost.Show(
                    window =>
                    {
                        observedWindow = window;
                        return false;
                    },
                    TimeSpan.FromMilliseconds(10),
                    new Border()));

            Assert.That(observedWindow, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(observedWindow!.IsVisible, Is.False);
                Assert.That(observedWindow.IsLoaded, Is.False);
                Assert.That(WpfTestHost.OpenWindowCount, Is.EqualTo(initialOpenWindowCount));
            });
        }
        finally
        {
            WpfTestHost.CloseAndDrain(observedWindow);
        }
    }

    private static ToolboxItem CreateGroup()
    {
        var group = new ToolboxItem();
        group.Items.Add(new ToolItem());
        return group;
    }

    private static void Open(ToolboxControl toolbox, ToolboxItem group, Window window)
    {
        toolbox.RequestOpen(group);
        WpfTestHost.Drain(window.Dispatcher);
        Assert.That(toolbox.ActiveItem, Is.SameAs(group));
    }

    private static void AssertClosed(ToolboxControl toolbox, ToolboxItem group)
    {
        Assert.Multiple(() =>
        {
            Assert.That(group.IsOpen, Is.False);
            Assert.That(toolbox.ActiveItem, Is.Null);
        });
    }

    private static void AssertGeneratedPayloadSurvivesExternalCoercion(ToolItem container, object payload)
    {
        container.CoerceValue(ToolItem.DragDataProperty);
        ValueSource source = DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty);

        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(payload));
            Assert.That(container.GetValue(ToolItem.DragDataProperty), Is.SameAs(payload));
            Assert.That(source.IsCoerced, Is.True);
        });
    }

    private static object? CreateTriggerPayload(TriggerPayloadKind payloadKind, object generatedPayload)
    {
        return payloadKind switch
        {
            TriggerPayloadKind.Null => null,
            TriggerPayloadKind.Distinct => new object(),
            TriggerPayloadKind.SameAsGenerated => generatedPayload,
            _ => throw new ArgumentOutOfRangeException(nameof(payloadKind))
        };
    }

    private static Style CreateDragDataTriggerStyle(object? payload)
    {
        var style = new Style(typeof(ToolItem));
        var trigger = new Trigger
        {
            Property = FrameworkElement.TagProperty,
            Value = "active"
        };
        trigger.Setters.Add(new Setter(ToolItem.DragDataProperty, payload));
        style.Triggers.Add(trigger);
        return style;
    }

    private static void ActivateDragDataTrigger(ToolItem container)
    {
        container.Tag = "active";
    }

    private static void AssertTriggerPayloadOwnsDragData(ToolItem container, object? payload)
    {
        ValueSource source = DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(payload));
            Assert.That(container.GetValue(ToolItem.DragDataProperty), Is.SameAs(payload));
            Assert.That(source.BaseValueSource, Is.EqualTo(BaseValueSource.StyleTrigger));
            Assert.That(GetPrivateField<bool>(container, "_ownsGeneratedDragData"), Is.False);
            Assert.That(GetPrivateField<object?>(container, "_generatedDragData"), Is.Null);
            Assert.That(GetPrivateField<object?>(container, "_generatedStateMarker"), Is.Null);
        });
    }

    private static void AssertGeneratedStateReleased(ToolItem container)
    {
        ValueSource source = DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.Null);
            Assert.That(container.GetValue(ToolItem.DragDataProperty), Is.Null);
            Assert.That(source.IsCoerced, Is.False);
            Assert.That(source.IsCurrent, Is.False);
            Assert.That(container.ReadLocalValue(ToolItem.DragDataProperty), Is.SameAs(DependencyProperty.UnsetValue));
            Assert.That(GetPrivateField<bool>(container, "_ownsGeneratedDragData"), Is.False);
            Assert.That(GetPrivateField<object?>(container, "_generatedDragData"), Is.Null);
            Assert.That(GetPrivateField<object?>(container, "_generatedStateMarker"), Is.Null);
            Assert.That(GetPrivateField<object>(container, "_generatedDragDataOperation").ToString(), Is.EqualTo("None"));
        });
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (T)field.GetValue(target)!;
    }

    private static void InvokePrepare(ItemsControl itemsControl, DependencyObject container, object item)
    {
        InvokeContainerMethod(itemsControl, "PrepareContainerForItemOverride", container, item);
    }

    private static void InvokeClear(ItemsControl itemsControl, DependencyObject container, object item)
    {
        InvokeContainerMethod(itemsControl, "ClearContainerForItemOverride", container, item);
    }

    private static void InvokeContainerMethod(
        ItemsControl itemsControl,
        string methodName,
        DependencyObject container,
        object item)
    {
        MethodInfo method = itemsControl.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        method.Invoke(itemsControl, new[] { container, item });
    }

    private static void InvokeNonPublic(object target, string methodName, params object[] arguments)
    {
        MethodInfo method = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        method.Invoke(target, arguments);
    }
}
