# SDD ledger — plan: docs/superpowers/plans/2026-08-29-hover-toolbox-control.md

## Setup

- Branch: `fix_control`
- Start commit: `4c8597cdc474e7a29e968fd82395d8d481edbf61`
- Spec: `docs/superpowers/specs/2026-08-29-hover-toolbox-control-design.md`
- Baseline: `dotnet build Junevy.Controls.csproj -f net8.0-windows` and `-f net48` both succeeded with 0 warnings and 0 errors.
- Ruling: execute in the current feature branch because the approved plan/spec are untracked in this workspace and the user explicitly requested immediate multi-agent execution — isolation is provided by strict per-agent file ownership — cost if wrong: changes are not separated into a linked worktree and must be reverted by commit if rejected.
- Ruling: create SDD artifacts manually because the bundled Bash scripts contain CRLF shebangs and fail under WSL — preserve the required directory, brief, report, and ledger contracts — cost if wrong: artifact generation is not automated, so task extraction needs manual consistency checks.

## Preflight Dependency Scan

| Producer | Consumer | Shared files/interfaces | Result |
| --- | --- | --- | --- |
| Task 1 | Task 2 | `Toolbox`, `ToolboxItem`, `ToolItem`, enums, test project | Consistent; Task 2 builds on Task 1 contracts. |
| Task 2 | Task 3 | owner wiring and read-only state consumed by templates | Consistent; template part code must not duplicate coordination. |
| Task 3 | Task 4 | `ToolboxItem.cs`, `ToolboxContainerTests.cs`, template parts | Consistent; Task 4 extends event behavior after parts exist. |
| Task 4 | Task 5 | active popup and window subscriptions | Consistent; Task 5 reuses the single active item lifecycle. |
| Task 5 | Task 6 | `ToolboxItem.cs`, `Toolbox.cs` | Consistent; placement is independent of drag locking. |
| Task 6 | Task 7 | public drag format/data contract consumed by demo | Consistent. |
| Task 7 | Task 8 | demo and documented public API | Consistent. |
| Task 1 | Task 1 | test project path/reference and initial source shells | Defect: plan says test project reference is `../../../Junevy.Controls.csproj`; from `Tests/Junevy.Controls.Tests` the correct path is `../../Junevy.Controls.csproj`. |
| Task 2 | Task 2 | `SetCurrentValue` plus explicit Style semantics | Defect: `ReadLocalValue` cannot detect a Style setter. Generated payload assignment must occur during preparation without overwriting local or bound values; Style-supplied payload should be allowed to win after style application. |
| Task 3 | Task 3 | `TemplateBinding IsOpen` to `Popup.IsOpen` | Consistent; read-only DP is bindable. |
| Task 4 | Task 4 | dynamic collection close behavior | Requires implementation via `OnItemsChanged` rather than external collection subscription; avoids leaks and satisfies spec. |
| Task 5 | Task 5 | monitor work area fallback | Defect: falling back to Window bounds may clip popup and conflicts with full work-area requirement; use Win32 nearest-monitor data whenever an HWND exists, and only use target bounds before HWND creation. |
| Task 6 | Task 6 | suppressing Button Click after drag | Requires careful mouse-up lifecycle; clearing suppression before `OnClick` would re-enable the click. |
| Task 7 | Task 7 | sample project path/reference | Same relative path defect as Task 1: use `../../Junevy.Controls.csproj` from `Samples/Junevy.Controls.ToolboxDemo`. |
| Task 8 | Task 8 | full solution build | Existing `Junevy.Controls.Test` has an external `SimpleNavigation.dll` reference; full-solution verification may fail independently. Product, unit-test, and new demo builds remain authoritative; record any existing demo failure separately. |

## Preflight Rulings

- Ruling: use `../../Junevy.Controls.csproj` in both new nested projects because it is the correct relative path — cost if wrong: project restore fails immediately.
- Ruling: use `OnItemsChanged` for active-group empty detection instead of subscribing to external collections — cost if wrong: item changes delivered through unusual custom views may need an additional weak listener.
- Ruling: test generated `DragData` defaults separately from Style overrides; never overwrite local values or bindings — cost if wrong: a Style-only `DragData` setter could require deferred preparation after style application.
- Ruling: treat product/test/demo project builds as the release gate if the legacy demo's unrelated external reference fails — cost if wrong: an integration break visible only through the old demo solution could be missed.
- Ruling: move the two-level `ItemContainerGenerator` integration test from Task 2 to Task 3 because custom controls have no usable `ItemsPresenter` before `Toolbox.xaml` is merged — Task 2 tests owner/state behavior with explicit containers — cost if wrong: generated-container behavior is validated one task later.
- Ruling: multi-target `Junevy.Controls.Tests` to `net8.0-windows;net48` so defaults, dependency properties, theme smoke, and pure placement logic run on both supported runtimes — cost if wrong: NUnit adapter/framework compatibility may require narrowing individual UI tests by target.
- Ruling: add a shared nonparallel STA WPF test host with bounded `DispatcherFrame` pumping; do not use `Thread.Sleep` or unbounded frames — cost if wrong: timing-sensitive tests may still need platform-specific tolerances.
- Ruling: theme tests must resolve implicit styles through `Themes/Generic.xaml`, and Popup layout tests must open the real Popup, wait for `Opened` and generated containers, then inspect `popup.Child` — cost if wrong: tests are slower and require an interactive Windows session.
- Ruling: listen for popup hover on a named visual root (`PART_PopupRoot`), not on the `Popup` object — cost if wrong: custom consumer templates must preserve the new required part for hover transfer.
- Ruling: introduce a replaceable internal drag executor or gesture state seam so tests cover click suppression, drag locking, cancel, and exception cleanup without entering the real OLE drag loop — cost if wrong: one extra internal abstraction is retained solely for deterministic behavior testing.
- Ruling: make the pure placement calculator return an internal `PlacementCandidate` containing direction, point, and primary axis, then map candidates to WPF `CustomPopupPlacement` in the Popup callback — cost if wrong: the internal placement API differs from the original plan text but remains outside the public contract.
- Ruling: propagate root layout and drag-format settings through owner access/inherited dependency-property bindings, with explicit local `ToolboxItem`/`ToolItem` values taking precedence and runtime root changes reflected in open/generated containers — cost if wrong: consumers depending on Style precedence may need an explicit value-source test adjustment.
- Ruling: rely on the trigger Button's `Click` for Enter/Space activation rather than independently toggling in `PreviewKeyDown`; use key handling only for Escape/navigation — cost if wrong: a custom trigger template that is not a Button must implement equivalent activation itself.
- Ruling: after keyboard-open, wait for Popup `Opened` and generated containers, then explicitly focus the first enabled `ToolItem`; configure directional keyboard navigation on `PART_PopupRoot` — cost if wrong: focus transfer occurs one Dispatcher turn later than immediate toggle.
- Ruling: clearing a pending or active `ToolboxItem` container must atomically cancel its pending request, close its Popup, clear `ActiveItem`, and detach owner references — cost if wrong: removal may perform additional synchronous UI cleanup during collection mutation.
- Ruling: clicking/toggling an item cancels its pending open request so an expired timer cannot reopen it — cost if wrong: rapid hover-click interaction favors the explicit click over the original hover timer.
- Ruling: reject the review claim that `Icon.FontFamily` and `Icon.IconSize` do not exist; `AttachedProperties/Icon.cs` defines both and `AssemblyInfo.cs` maps the namespace — cost if wrong: none; this is verified repository evidence.
- Ruling: the root product SDK project must exclude nested `Tests/**` and planned `Samples/**` from `DefaultItemExcludes` and explicitly remove their C#/WPF inputs (`Compile`, `Page`, and `ApplicationDefinition`) — test and sample projects retain normal SDK compilation and assembly metadata generation without per-file guards — cost if wrong: a future nonstandard SDK item type under those trees may require an additional explicit removal.

## Task Status

- Task 1: complete; independent re-review PASS on `97ebadd` + `f5e10e9`, no Critical or Important findings.
- Task 2: sixth fix round complete; awaiting re-review (`d213d60` + `f58a711` + `c1dd37e` + `f0dad8b` + `bb43d2c` + sixth-fix commit pending).
