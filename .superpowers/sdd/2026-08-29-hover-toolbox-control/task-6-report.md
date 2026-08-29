# Task 6 Report: Drag-to-Canvas Semantics

## Outcome

Implemented thresholded `ToolItem` drag initiation with a business-object payload, fixed Copy semantics, root popup drag locking, cancellation/exception cleanup, and suppression of only the click associated with the drag gesture.

## Product Changes

- `ToolItem.ExceedsDragThreshold` is a pure internal helper using strict greater-than comparison on either axis.
- `ToolItem.CreateDragDataObject` returns null unless dragging is enabled, payload data exists, and the effective format is nonblank. The created object contains the exact format and original payload reference without automatic conversion formats.
- Left-button press records the local start point. Mouse movement starts one drag only after a WPF system threshold is exceeded while the left button remains pressed.
- `DragExecutor` is an internal replaceable seam. Production defaults to `DragDrop.DoDragDrop`; tests avoid the OLE loop. Allowed effects are always `DragDropEffects.Copy`.
- Drag start sets click suppression and notifies the owning group/root before execution. Completion notification is guaranteed by `finally`, including cancellation and exceptions.
- `Toolbox` retains the popup while a drag is active. Completion closes it only when the pointer is outside both trigger and popup.
- `OnClick` consumes and clears only the drag gesture suppression; subsequent clicks keep normal `Button` command behavior. Mouse-up calls the base implementation before final state cleanup so suppression is not cleared before `OnClick`. Lost capture clears the stale drag start without prematurely clearing click suppression.

## Tests

Added `ToolItemDragTests.cs` with coverage for:

- below/equal threshold versus either-axis threshold exceedance;
- exact DataObject format and reference identity;
- disabled, null-data, null-format, and whitespace-format rejection;
- recorded gesture threshold and one-shot initiation;
- released-left-button stale-start cleanup;
- Copy effect and root drag lock during the executor call;
- canceled and exceptional drag cleanup;
- popup retention while the pointer remains over the popup;
- single-gesture click suppression followed by normal command execution.

## Verification

- `ToolItemDragTests`: 17 passed on `net8.0-windows`; 17 passed on `net48`.
- `ToolboxContainerTests|ToolboxDefaultsTests|PopupPlacementCalculatorTests`: 71 passed on `net8.0-windows`; 71 passed on `net48`.
- Product builds and `git diff --check` are recorded in the final task handoff.

## Self-Review

- The default payload remains `DragData`, never the `ToolItem` or another UI element.
- Drag notification ordering is start-before-executor and completion-in-finally.
- Popup close behavior remains centralized in the root coordinator.
- No Task 7 or later files were changed.

## Independent Review Follow-up

The two Important findings from the independent review were addressed:

- Click suppression is now bound to the initiating mouse gesture. A new left-button gesture clears stale suppression, and suppression is honored only while that same gesture's mouse-up synchronously invokes the Button base click path. If OLE drag completion produces no source mouse-up/click, later mouse, keyboard, access-key, automation, or direct activation executes normally.
- Drag execution captures both the initiating ToolboxItem and its root coordinator before entering the replaceable executor. The finally block notifies that exact coordinator even if the tool is removed or reparented during the nested drag loop.

Additional STA coverage exercises routed mouse down/up cleanup, the synchronous same-gesture click window, independent activation after a drag with no source mouse-up, a new mouse gesture after stale state, and reentrant tool reparenting while the original group remains valid.

Follow-up verification:

- ToolItemDragTests: 21 passed on net8.0-windows; 21 passed on net48.
- ToolboxContainerTests, ToolboxDefaultsTests, and PopupPlacementCalculatorTests: 71 passed on net8.0-windows; 71 passed on net48.
