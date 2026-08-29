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
    public void GeneratedContainer_AssignsAndClearsOwnedDragPayloadAcrossRecycling()
    {
        var group = new ToolboxItem();
        var container = new ToolItem();
        var first = new object();
        var second = new object();

        InvokePrepare(group, container, first);
        Assert.That(container.DragData, Is.SameAs(first));

        InvokeClear(group, container, first);
        Assert.That(container.DragData, Is.Null);

        InvokePrepare(group, container, second);
        Assert.That(container.DragData, Is.SameAs(second));

        InvokeClear(group, container, second);
        Assert.That(container.DragData, Is.Null);
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
