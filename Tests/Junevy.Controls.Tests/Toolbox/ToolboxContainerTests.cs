using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using Junevy.Controls.Controls.Toolbox;
using NUnit.Framework;
using IconProperties = Junevy.Controls.AttachedProperties.Icon;
using ToolboxControl = Junevy.Controls.Controls.Toolbox.Toolbox;

namespace Junevy.Controls.Tests.Toolbox;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ToolboxContainerTests
{
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
        AssertGeneratedPayload(container, first);

        InvokeClear(group, container, first);
        Assert.That(container.DragData, Is.Null);

        InvokePrepare(group, container, second);
        AssertGeneratedPayload(container, second);

        InvokeClear(group, container, second);
        Assert.That(container.DragData, Is.Null);
    }

    [Test]
    public void GeneratedContainer_RecordsGeneratedPayloadAsCurrentValue()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();

        InvokePrepare(group, container, payload);
        AssertGeneratedPayload(container, payload);

        Assert.Multiple(() =>
        {
            Assert.That(GetPrivateField<object>(container, "_generatedDragData"), Is.SameAs(payload));
            Assert.That(
                DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty).IsCurrent,
                Is.True);
        });

        InvokeClear(group, container, payload);
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
    public void GeneratedContainer_DoesNotOverwriteLocalNull()
    {
        var group = new ToolboxItem();
        var container = new ToolItem();
        container.SetValue(ToolItem.DragDataProperty, null);

        InvokePrepare(group, container, new object());

        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.Null);
            Assert.That(container.ReadLocalValue(ToolItem.DragDataProperty), Is.Null);
            Assert.That(DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty).BaseValueSource,
                Is.EqualTo(BaseValueSource.Local));
        });
    }

    [Test]
    public void GeneratedContainer_DoesNotOverwriteBindingToNull()
    {
        var group = new ToolboxItem();
        var source = new TextBlock { Tag = null };
        var container = new ToolItem();
        BindingOperations.SetBinding(
            container,
            ToolItem.DragDataProperty,
            new Binding(nameof(FrameworkElement.Tag)) { Source = source });

        InvokePrepare(group, container, new object());

        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.Null);
            Assert.That(BindingOperations.GetBindingExpression(container, ToolItem.DragDataProperty), Is.Not.Null);
            Assert.That(DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty).IsExpression,
                Is.True);
        });
    }

    [Test]
    public void GeneratedContainer_DoesNotOverwriteStyleNull()
    {
        var group = new ToolboxItem();
        var container = new ToolItem();
        var style = new Style(typeof(ToolItem));
        style.Setters.Add(new Setter(ToolItem.DragDataProperty, null));
        container.Style = style;

        InvokePrepare(group, container, new object());

        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.Null);
            Assert.That(DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty).BaseValueSource,
                Is.EqualTo(BaseValueSource.Style));
        });
    }

    [Test]
    public void GeneratedContainer_DoesNotOverwriteActiveStyleTrigger()
    {
        var group = new ToolboxItem();
        var container = new ToolItem { Tag = "active" };
        var triggerPayload = new object();
        var style = new Style(typeof(ToolItem));
        var trigger = new Trigger
        {
            Property = FrameworkElement.TagProperty,
            Value = "active"
        };
        trigger.Setters.Add(new Setter(ToolItem.DragDataProperty, triggerPayload));
        style.Triggers.Add(trigger);
        container.Style = style;

        InvokePrepare(group, container, new object());

        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(triggerPayload));
            Assert.That(DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty).BaseValueSource,
                Is.EqualTo(BaseValueSource.StyleTrigger));
        });
    }

    [Test]
    public void BaseTypedCurrentNullAtMetadataDefaultAllowsGeneratedFallback()
    {
        var group = new ToolboxItem();
        var generatedPayload = new object();
        var container = new ToolItem();
        DependencyObject baseTypedContainer = container;
        baseTypedContainer.SetCurrentValue(ToolItem.DragDataProperty, null);

        ValueSource beforePrepare = DependencyPropertyHelper.GetValueSource(
            container,
            ToolItem.DragDataProperty);

        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.Null);
            Assert.That(beforePrepare.BaseValueSource, Is.EqualTo(BaseValueSource.Default));
            Assert.That(beforePrepare.IsCurrent, Is.False);
            Assert.That(beforePrepare.IsCoerced, Is.False);
        });

        InvokePrepare(group, container, generatedPayload);

        ValueSource afterPrepare = DependencyPropertyHelper.GetValueSource(
            container,
            ToolItem.DragDataProperty);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(generatedPayload));
            Assert.That(afterPrepare.BaseValueSource, Is.EqualTo(BaseValueSource.Default));
            Assert.That(afterPrepare.IsCurrent, Is.True);
            Assert.That(GetPrivateField<object>(container, "_generatedDragData"), Is.SameAs(generatedPayload));
        });
    }

    [Test]
    public void GeneratedContainer_UsesOnlyPublicDragDataLocalState()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();

        InvokePrepare(group, container, payload);

        LocalValueEnumerator localValues = container.GetLocalValueEnumerator();
        bool foundDragData = false;
        while (localValues.MoveNext())
        {
            if (localValues.Current.Property == ToolItem.DragDataProperty)
            {
                foundDragData = true;
                Assert.That(localValues.Current.Value, Is.SameAs(payload));
            }
        }

        Assert.That(foundDragData, Is.True);
        AssertGeneratedPayload(container, payload);

        InvokeClear(group, container, payload);
        AssertGeneratedStateReleased(container);
    }

    [Test]
    public void GeneratedContainer_ClearPreservesLaterBindingWithTheSamePayload()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, payload);
        AssertGeneratedPayload(container, payload);
        var source = new TextBlock { Tag = payload };
        BindingOperations.SetBinding(
            container,
            ToolItem.DragDataProperty,
            new Binding(nameof(FrameworkElement.Tag)) { Source = source });
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
        AssertGeneratedPayload(container, payload);
        container.DragData = payload;
        InvokeClear(group, container, payload);

        Assert.That(container.DragData, Is.SameAs(payload));
        Assert.That(container.ReadLocalValue(ToolItem.DragDataProperty), Is.SameAs(payload));
    }

    [Test]
    public void GeneratedContainer_ClearPreservesLaterDistinctCurrentValue()
    {
        var group = new ToolboxItem();
        var generatedPayload = new object();
        var currentPayload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, generatedPayload);
        AssertGeneratedPayload(container, generatedPayload);
        container.SetCurrentValue(ToolItem.DragDataProperty, currentPayload);

        InvokeClear(group, container, generatedPayload);

        ValueSource source = DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(currentPayload));
            Assert.That(source.IsCurrent, Is.True);
            Assert.That(GetPrivateField<object?>(container, "_generatedDragData"), Is.Null);
        });
    }

    [Test]
    public void GeneratedContainer_ClearRevealsLaterStyleValue()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, payload);
        AssertGeneratedPayload(container, payload);
        var style = new Style(typeof(ToolItem));
        style.Setters.Add(new Setter(ToolItem.DragDataProperty, payload));

        container.Style = style;
        Assert.That(container.DragData, Is.SameAs(payload));

        InvokeClear(group, container, payload);
        ValueSource afterClear = DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty);
        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(payload));
            Assert.That(afterClear.BaseValueSource, Is.EqualTo(BaseValueSource.Style));
            Assert.That(afterClear.IsCoerced, Is.False);
        });
    }

    [Test]
    public void GeneratedContainer_LaterLocalNullTakesEffectAndSurvivesClear()
    {
        var group = new ToolboxItem();
        var payload = new object();
        var container = new ToolItem();
        InvokePrepare(group, container, payload);
        AssertGeneratedPayload(container, payload);

        container.DragData = null;
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
        AssertGeneratedPayload(container, payload);
        var source = new TextBlock { Tag = null };

        BindingOperations.SetBinding(
            container,
            ToolItem.DragDataProperty,
            new Binding(nameof(FrameworkElement.Tag)) { Source = source });
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
        AssertGeneratedPayload(container, payload);
        var style = new Style(typeof(ToolItem));
        style.Setters.Add(new Setter(ToolItem.DragDataProperty, null));

        container.Style = style;
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

    [Test]
    public void RealTwoLevelGenerators_CreateToolboxAndToolItemContainers()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero };
        var groupData = new ToolboxGroupData("Shapes", "S");
        var firstTool = new ToolboxToolData("Line", "L");
        var secondTool = new ToolboxToolData("Circle", "C");
        groupData.Tools.Add(firstTool);
        groupData.Tools.Add(secondTool);
        toolbox.Items.Add(groupData);
        toolbox.ItemContainerStyle = CreateGroupContainerStyle();
        Window? window = null;

        try
        {
            window = WpfTestHost.Show(toolbox);
            WpfTestHost.PumpUntil(
                window.Dispatcher,
                () => toolbox.ItemContainerGenerator.ContainerFromItem(groupData) is ToolboxItem,
                TimeSpan.FromSeconds(1));
            var group = (ToolboxItem)toolbox.ItemContainerGenerator.ContainerFromItem(groupData);

            group.SetPointerOverTrigger(true);
            toolbox.RequestOpen(group);
            WpfTestHost.PumpUntil(
                window.Dispatcher,
                () => group.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated,
                TimeSpan.FromSeconds(1));

            var firstContainer = (ToolItem)group.ItemContainerGenerator.ContainerFromItem(firstTool);
            var secondContainer = (ToolItem)group.ItemContainerGenerator.ContainerFromItem(secondTool);
            Assert.Multiple(() =>
            {
                Assert.That(firstContainer, Is.Not.Null);
                Assert.That(secondContainer, Is.Not.Null);
                Assert.That(firstContainer.DragData, Is.SameAs(firstTool));
                Assert.That(secondContainer.DragData, Is.SameAs(secondTool));
                Assert.That(firstContainer.Owner, Is.SameAs(group));
                Assert.That(firstContainer.Title, Is.EqualTo(firstTool.Title));
                Assert.That(firstContainer.Icon, Is.EqualTo(firstTool.Icon));
            });
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void ToolboxItemTemplate_ExposesRequiredPartsAndPopupContent()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero };
        var tool = new ToolItem { Icon = "I", Title = "Inspect", Foreground = Brushes.Red };
        var group = new ToolboxItem { Icon = "G", Title = "Geometry", Foreground = Brushes.Red };
        group.Items.Add(tool);
        toolbox.Items.Add(group);
        Window? window = null;

        try
        {
            window = WpfTestHost.Show(toolbox);
            IconProperties.SetIconForeground(tool, Brushes.Yellow);
            IconProperties.SetIconForeground(group, Brushes.Yellow);
            group.SetPointerOverTrigger(true);
            toolbox.RequestOpen(group);
            WpfTestHost.PumpUntil(window.Dispatcher, () => group.IsOpen, TimeSpan.FromSeconds(1));

            var trigger = group.Template.FindName("PART_TriggerButton", group);
            var popup = group.Template.FindName("PART_Popup", group) as Popup;
            var popupRoot = popup?.Child is null
                ? null
                : FindVisualChildByName<FrameworkElement>(popup.Child, "PART_PopupRoot");
            var triggerTitle = trigger is DependencyObject triggerRoot
                ? FindVisualChildByName<TextBlock>(triggerRoot, "TriggerTitle")
                : null;
            var toolTitle = popupRoot is null
                ? null
                : FindVisualChildByName<TextBlock>(popupRoot, "ToolTitle");
            var triggerIcon = trigger is DependencyObject triggerIconRoot
                ? FindVisualChild<ContentControl>(triggerIconRoot, control => Equals(control.Content, group.Icon))
                : null;
            var toolIcon = popupRoot is null
                ? null
                : FindVisualChild<ContentControl>(popupRoot, control => Equals(control.Content, tool.Icon));

            Assert.Multiple(() =>
            {
                Assert.That(trigger, Is.AssignableTo<ButtonBase>());
                Assert.That(popup, Is.Not.Null);
                Assert.That(popup!.IsOpen, Is.True);
                Assert.That(popupRoot, Is.Not.Null);
                Assert.That(group.PopupRoot, Is.SameAs(popupRoot));
                Assert.That(FindVisualChild<TextBlock>(popupRoot!, text => text.Text == tool.Title), Is.Not.Null);
                Assert.That(FindVisualChild<ContentPresenter>(
                    popupRoot!,
                    presenter => Equals(presenter.Content, tool.Icon)), Is.Not.Null);
                Assert.That(triggerTitle?.Margin, Is.EqualTo(new Thickness(0d, 5d, 0d, 0d)));
                Assert.That(toolTitle?.Margin, Is.EqualTo(new Thickness(0d, 5d, 0d, 0d)));
                Assert.That(triggerIcon?.Foreground, Is.EqualTo(Brushes.Yellow));
                Assert.That(toolIcon?.Foreground, Is.EqualTo(Brushes.Yellow));
                Assert.That(triggerTitle?.Foreground, Is.EqualTo(Brushes.Red));
                Assert.That(toolTitle?.Foreground, Is.EqualTo(Brushes.Red));
            });
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void ToolItem_DefaultStyleUsesCrossCursorForDragEnabledItems()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero };
        var tool = new ToolItem { Icon = "I", Title = "Inspect" };
        var group = new ToolboxItem { Title = "Geometry" };
        group.Items.Add(tool);
        toolbox.Items.Add(group);
        Window? window = null;

        try
        {
            window = WpfTestHost.Show(toolbox);
            group.SetPointerOverTrigger(true);
            toolbox.RequestOpen(group);
            WpfTestHost.PumpUntil(window.Dispatcher, () => group.IsOpen, TimeSpan.FromSeconds(1));

            Assert.That(tool.Cursor, Is.EqualTo(Cursors.Cross));
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void OpenPopup_UsesDefaultWidthAndFourColumnWrapping()
    {
        var toolbox = new ToolboxControl
        {
            OpenDelay = TimeSpan.Zero
        };
        var group = new ToolboxItem { Title = "Shapes" };
        for (int index = 1; index <= 5; index++)
        {
            group.Items.Add(new ToolItem { Icon = index.ToString(), Title = "Tool " + index });
        }

        toolbox.Items.Add(group);
        Window? window = null;

        try
        {
            window = WpfTestHost.Show(toolbox);
            group.SetPointerOverTrigger(true);
            toolbox.RequestOpen(group);
            WpfTestHost.PumpUntil(window.Dispatcher, () => group.IsOpen, TimeSpan.FromSeconds(1));

            var popup = (Popup)group.Template.FindName("PART_Popup", group);
            var popupRoot = FindVisualChildByName<FrameworkElement>(popup.Child, "PART_PopupRoot");
            var panel = FindVisualChild<UniformGrid>(popupRoot!);
            Assert.Multiple(() =>
            {
                Assert.That(popupRoot, Is.Not.Null);
                Assert.That(popupRoot!.ActualWidth, Is.EqualTo(300d).Within(0.1d));
                Assert.That(panel, Is.Not.Null);
                Assert.That(panel!.Columns, Is.EqualTo(4));
                Assert.That(panel.Children.Count, Is.EqualTo(5));
            });

            var first = (FrameworkElement)panel!.Children[0];
            var fifth = (FrameworkElement)panel.Children[4];
            double firstY = first.TransformToAncestor(panel).Transform(new Point()).Y;
            double fifthY = fifth.TransformToAncestor(panel).Transform(new Point()).Y;
            Assert.That(fifthY, Is.GreaterThan(firstY));
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void TemplateTriggerHover_TransfersToPopupWithoutClosingUntilBothRegionsLeave()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero, CloseDelay = TimeSpan.Zero };
        var group = new ToolboxItem { Title = "Shapes" };
        group.Items.Add(new ToolItem { Title = "Line" });
        toolbox.Items.Add(group);
        Window? window = null;
        try
        {
            window = WpfTestHost.Show(toolbox);
            var trigger = (Button)group.Template.FindName("PART_TriggerButton", group)!;
            var popupRoot = (FrameworkElement)group.Template.FindName("PART_PopupRoot", group)!;
            RaiseMouse(trigger, Mouse.MouseEnterEvent);
            WpfTestHost.Drain(window.Dispatcher);
            Assert.That(group.IsOpen, Is.True);

            RaiseMouse(trigger, Mouse.MouseLeaveEvent);
            RaiseMouse(popupRoot, Mouse.MouseEnterEvent);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(20));
            Assert.That(group.IsOpen, Is.True);

            RaiseMouse(popupRoot, Mouse.MouseLeaveEvent);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(30));
            Assert.That(group.IsOpen, Is.False);
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void TemplateTriggerHover_RespectsNonZeroOpenAndCloseDelays()
    {
        var toolbox = new ToolboxControl
        {
            OpenDelay = TimeSpan.FromMilliseconds(40),
            CloseDelay = TimeSpan.FromMilliseconds(50)
        };
        var group = new ToolboxItem { Title = "Shapes" };
        group.Items.Add(new ToolItem { Title = "Line" });
        toolbox.Items.Add(group);
        Window? window = null;
        try
        {
            window = WpfTestHost.Show(toolbox);
            var trigger = (Button)group.Template.FindName("PART_TriggerButton", group)!;
            RaiseMouse(trigger, Mouse.MouseEnterEvent);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(10));
            Assert.That(group.IsOpen, Is.False, "OpenDelay must suppress a premature open.");

            WpfTestHost.PumpUntil(window.Dispatcher, () => group.IsOpen, TimeSpan.FromMilliseconds(150));
            RaiseMouse(trigger, Mouse.MouseLeaveEvent);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(15));
            Assert.That(group.IsOpen, Is.True, "CloseDelay must keep the popup open before expiry.");

            WpfTestHost.PumpUntil(window.Dispatcher, () => !group.IsOpen, TimeSpan.FromMilliseconds(150));
            Assert.That(toolbox.ActiveItem, Is.Null);
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void TemplateHover_EnteringSecondTriggerCancelsPendingFirstOpen()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.FromMilliseconds(30) };
        var first = new ToolboxItem { Title = "A" };
        var second = new ToolboxItem { Title = "B" };
        first.Items.Add(new ToolItem());
        second.Items.Add(new ToolItem());
        toolbox.Items.Add(first);
        toolbox.Items.Add(second);
        Window? window = null;
        try
        {
            window = WpfTestHost.Show(toolbox);
            var triggerA = (Button)first.Template.FindName("PART_TriggerButton", first)!;
            var triggerB = (Button)second.Template.FindName("PART_TriggerButton", second)!;
            RaiseMouse(triggerA, Mouse.MouseEnterEvent);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(10));
            RaiseMouse(triggerB, Mouse.MouseEnterEvent);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(50));
            Assert.Multiple(() =>
            {
                Assert.That(first.IsOpen, Is.False);
                Assert.That(second.IsOpen, Is.True);
            });
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void ActiveTriggerClick_ClosesAndCancelsPendingOpen()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero };
        var group = new ToolboxItem { Title = "A" };
        group.Items.Add(new ToolItem());
        toolbox.Items.Add(group);
        Window? window = null;
        try
        {
            window = WpfTestHost.Show(toolbox);
            var trigger = (Button)group.Template.FindName("PART_TriggerButton", group)!;
            RaiseMouse(trigger, Mouse.MouseEnterEvent);
            WpfTestHost.Drain(window.Dispatcher);
            Assert.That(group.IsOpen, Is.True);
            RaiseMouse(trigger, Mouse.MouseLeaveEvent);
            trigger.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            WpfTestHost.Drain(window.Dispatcher);
            Assert.That(toolbox.ActiveItem, Is.Null);
            Assert.That(group.IsOpen, Is.False);
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void TriggerClick_OpensPendingHoverImmediatelyAndCancelsTimer()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.FromMilliseconds(40) };
        var group = new ToolboxItem { Title = "A" };
        group.Items.Add(new ToolItem());
        toolbox.Items.Add(group);
        Window? window = null;
        try
        {
            window = WpfTestHost.Show(toolbox);
            var trigger = (Button)group.Template.FindName("PART_TriggerButton", group)!;
            RaiseMouse(trigger, Mouse.MouseEnterEvent);
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(5));
            trigger.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.Multiple(() =>
            {
                Assert.That(group.IsOpen, Is.True);
                Assert.That(toolbox.ActiveItem, Is.SameAs(group));
            });
            WpfTestHost.PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(70));
            Assert.Multiple(() =>
            {
                Assert.That(group.IsOpen, Is.True);
                Assert.That(toolbox.ActiveItem, Is.SameAs(group));
            });
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void EscapeFromTrigger_ClosesAndRestoresTriggerFocus()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero };
        var group = new ToolboxItem { Title = "A" };
        group.Items.Add(new ToolItem());
        toolbox.Items.Add(group);
        Window? window = null;
        try
        {
            window = WpfTestHost.Show(toolbox);
            var trigger = (Button)group.Template.FindName("PART_TriggerButton", group)!;
            RaiseMouse(trigger, Mouse.MouseEnterEvent);
            WpfTestHost.Drain(window.Dispatcher);
            trigger.Focus();
            var key = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(trigger), Environment.TickCount, Key.Escape)
            { RoutedEvent = Keyboard.PreviewKeyDownEvent };
            trigger.RaiseEvent(key);
            Assert.Multiple(() =>
            {
                Assert.That(group.IsOpen, Is.False);
                Assert.That(trigger.IsKeyboardFocusWithin, Is.True);
            });
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void EscapeFromPopup_ClosesAndReturnsFocusToTrigger()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.Zero };
        var group = new ToolboxItem { Title = "A" };
        group.Items.Add(new ToolItem { Title = "Line" });
        toolbox.Items.Add(group);
        Window? window = null;
        try
        {
            window = WpfTestHost.Show(toolbox);
            var trigger = (Button)group.Template.FindName("PART_TriggerButton", group)!;
            RaiseMouse(trigger, Mouse.MouseEnterEvent);
            WpfTestHost.Drain(window.Dispatcher);
            var tool = (ToolItem)group.ItemContainerGenerator.ContainerFromIndex(0);
            tool.Focus();
            var key = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(tool), Environment.TickCount, Key.Escape)
            { RoutedEvent = Keyboard.PreviewKeyDownEvent };
            tool.RaiseEvent(key);
            Assert.That(group.IsOpen, Is.False);
            Assert.That(trigger.IsKeyboardFocusWithin, Is.True);
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void KeyboardClick_UsesButtonClickAndFocusesFirstEnabledTool()
    {
        var toolbox = new ToolboxControl { OpenDelay = TimeSpan.FromMilliseconds(1) };
        var group = new ToolboxItem { Title = "A" };
        group.Items.Add(new ToolItem { IsEnabled = false, Title = "Disabled" });
        group.Items.Add(new ToolItem { IsEnabled = true, Title = "Enabled" });
        toolbox.Items.Add(group);
        Window? window = null;
        try
        {
            window = WpfTestHost.Show(toolbox);
            var trigger = (Button)group.Template.FindName("PART_TriggerButton", group)!;
            trigger.Focus();
            var keyDown = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(trigger), Environment.TickCount, Key.Enter)
            { RoutedEvent = Keyboard.PreviewKeyDownEvent };
            trigger.RaiseEvent(keyDown);
            trigger.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            WpfTestHost.PumpUntil(window.Dispatcher, () => group.IsOpen, TimeSpan.FromSeconds(1));
            WpfTestHost.Drain(window.Dispatcher);
            var enabled = (ToolItem)group.ItemContainerGenerator.ContainerFromIndex(1);
            Assert.That(enabled.IsKeyboardFocusWithin, Is.True);
        }
        finally
        {
            WpfTestHost.CloseAndDrain(window);
        }
    }

    private static void RaiseMouse(UIElement target, RoutedEvent routedEvent)
    {
        var args = new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount) { RoutedEvent = routedEvent };
        target.RaiseEvent(args);
    }

    private static ToolboxItem CreateGroup()
    {
        var group = new ToolboxItem();
        group.Items.Add(new ToolItem());
        return group;
    }

    private static Style CreateGroupContainerStyle()
    {
        var toolStyle = new Style(typeof(ToolItem));
        toolStyle.Setters.Add(new Setter(ToolItem.TitleProperty, new Binding(nameof(ToolboxToolData.Title))));
        toolStyle.Setters.Add(new Setter(ToolItem.IconProperty, new Binding(nameof(ToolboxToolData.Icon))));

        var groupStyle = new Style(typeof(ToolboxItem));
        groupStyle.Setters.Add(new Setter(ToolboxItem.TitleProperty, new Binding(nameof(ToolboxGroupData.Title))));
        groupStyle.Setters.Add(new Setter(ToolboxItem.IconProperty, new Binding(nameof(ToolboxGroupData.Icon))));
        groupStyle.Setters.Add(new Setter(ItemsControl.ItemsSourceProperty, new Binding(nameof(ToolboxGroupData.Tools))));
        groupStyle.Setters.Add(new Setter(ItemsControl.ItemContainerStyleProperty, toolStyle));
        return groupStyle;
    }

    private static T? FindVisualChild<T>(DependencyObject root, Func<T, bool>? predicate = null)
        where T : DependencyObject
    {
        if (root is T self && (predicate is null || predicate(self)))
        {
            return self;
        }

        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T match && (predicate is null || predicate(match)))
            {
                return match;
            }

            T? descendant = FindVisualChild(child, predicate);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static T? FindVisualChildByName<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        return FindVisualChild<T>(root, element => element.Name == name);
    }

    private sealed class ToolboxGroupData
    {
        internal ToolboxGroupData(string title, object icon)
        {
            Title = title;
            Icon = icon;
        }

        public string Title { get; }

        public object Icon { get; }

        public List<ToolboxToolData> Tools { get; } = new();
    }

    private sealed class ToolboxToolData
    {
        internal ToolboxToolData(string title, object icon)
        {
            Title = title;
            Icon = icon;
        }

        public string Title { get; }

        public object Icon { get; }
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

    private static void AssertGeneratedPayload(ToolItem container, object payload)
    {
        ValueSource source = DependencyPropertyHelper.GetValueSource(container, ToolItem.DragDataProperty);

        Assert.Multiple(() =>
        {
            Assert.That(container.DragData, Is.SameAs(payload));
            Assert.That(container.GetValue(ToolItem.DragDataProperty), Is.SameAs(payload));
            Assert.That(source.BaseValueSource, Is.EqualTo(BaseValueSource.Default));
            Assert.That(source.IsCurrent, Is.True);
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
            Assert.That(GetPrivateField<object?>(container, "_generatedDragData"), Is.Null);
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
