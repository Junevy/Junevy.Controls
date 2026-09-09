# Hover Toolbox Control Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 为 Junevy.Controls 实现一套独立的一级悬浮工具箱控件，支持延迟展开、300 DIP 六列换行、智能定位、命令执行及拖到 Canvas。

**Architecture:** `Toolbox` 负责一级容器和唯一的悬浮状态协调，`ToolboxItem` 负责分组触发器及 Popup，`ToolItem` 负责内部命令与拖放。Popup 内容使用 `ItemsPresenter + UniformGrid`；位置候选和工作区换算分别由可测试的内部辅助类完成。

**Tech Stack:** C# 12、WPF、XAML、.NET 8 Windows、.NET Framework 4.8、NUnit 4、Microsoft.NET.Test.Sdk。

**Spec:** `docs/superpowers/specs/2026-08-29-hover-toolbox-control-design.md`

## Global Constraints

- 产品项目继续同时支持 `net8.0-windows` 和 `net48`。
- 不引入第三方 UI、停靠或拖放框架。
- 默认打开延迟为 150 ms，默认关闭延迟为 300 ms。
- Popup 默认总宽度为 300 DIP，每行默认固定 6 项。
- 任意时刻最多展开一个 `ToolboxItem`。
- Popup 使用当前显示器工作区，不能使用仅代表主屏幕的 `SystemParameters.WorkArea`。
- 拖放效果固定为 Copy，载荷必须是工具数据而不是 UIElement。
- 保留标准 `ItemsSource`、`ItemTemplate`、`ItemContainerStyle` 和 Button 命令管线。
- 新样式使用现有 `Theme.*` 动态资源，不修改现有 ToolBar 样式。
- 产品代码及公开标识符使用 ASCII；用户界面示例标题可使用中文。

---

## File Map

### 产品代码

- Create: `Common/ToolboxDisplayMode.cs` — 定义 `IconOnly`/`IconAndTitle`。
- Create: `Common/ToolboxPopupPlacement.cs` — 定义 Auto 和四个方向。
- Create: `Controls/Toolbox/Toolbox.cs` — 根容器、依赖属性、计时器、活动项和窗口生命周期。
- Create: `Controls/Toolbox/ToolboxItem.cs` — 分组容器、模板部件、内部 ToolItem 容器生成及 Hover 意图。
- Create: `Controls/Toolbox/ToolItem.cs` — 命令按钮、拖动阈值、DataObject 和 Click 抑制。
- Create: `Controls/Toolbox/ToolboxPopupPlacementCalculator.cs` — 纯候选位置计算。
- Create: `Controls/Toolbox/MonitorWorkAreaProvider.cs` — 当前显示器工作区及物理像素到 DIP 转换。
- Create: `Controls/Toolbox/Toolbox.xaml` — 三个控件的默认样式和模板。
- Modify: `Themes/Generic.xaml` — 合并 Toolbox 资源字典。
- Modify: `AssemblyInfo.cs` — 映射 `Junevy.Controls.Controls.Toolbox` 到公共 XAML 命名空间，并开放 internals 给测试程序集。
- Modify: `README.md` — 增加 API、XAML、拖放和边界说明。

### 自动化测试

- Create: `Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj` — net8.0-windows NUnit 测试项目。
- Create: `Tests/Junevy.Controls.Tests/Toolbox/ToolboxDefaultsTests.cs` — DP 默认值和验证。
- Create: `Tests/Junevy.Controls.Tests/Toolbox/ToolboxContainerTests.cs` — 容器生成和单一活动项。
- Create: `Tests/Junevy.Controls.Tests/Toolbox/PopupPlacementCalculatorTests.cs` — 方向顺序和候选坐标。
- Create: `Tests/Junevy.Controls.Tests/Toolbox/ToolItemDragTests.cs` — 阈值、载荷和 Click 抑制。
- Modify: `Junevy.Controls.sln` — 加入测试项目。

### 人工验收

- Create: `Samples/Junevy.Controls.ToolboxDemo/Junevy.Controls.ToolboxDemo.csproj` — 最小 WPF 演示应用。
- Create: `Samples/Junevy.Controls.ToolboxDemo/App.xaml`
- Create: `Samples/Junevy.Controls.ToolboxDemo/App.xaml.cs`
- Create: `Samples/Junevy.Controls.ToolboxDemo/MainWindow.xaml`
- Create: `Samples/Junevy.Controls.ToolboxDemo/MainWindow.xaml.cs`
- Create: `Samples/Junevy.Controls.ToolboxDemo/ToolDefinition.cs`
- Modify: `Junevy.Controls.sln` — 加入演示项目。

---

### Task 1: Establish the Public Contracts and Test Harness

**Files:**
- Create: `Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj`
- Create: `Tests/Junevy.Controls.Tests/Toolbox/ToolboxDefaultsTests.cs`
- Create: `Common/ToolboxDisplayMode.cs`
- Create: `Common/ToolboxPopupPlacement.cs`
- Create: `Controls/Toolbox/Toolbox.cs`
- Create: `Controls/Toolbox/ToolboxItem.cs`
- Create: `Controls/Toolbox/ToolItem.cs`
- Modify: `AssemblyInfo.cs`
- Modify: `Junevy.Controls.sln`

**Interfaces:**
- Produces: `Toolbox`, `ToolboxItem`, `ToolItem`, `ToolboxDisplayMode`, `ToolboxPopupPlacement` and their public dependency properties from the spec.
- Consumes: WPF `ItemsControl`, `HeaderedItemsControl`, `Button`, dependency properties and the existing `github.com.junevy` XAML namespace.

- [ ] **Step 1: Add the NUnit test project and solution entry**

Create a `net8.0-windows` project with `UseWPF=true`, `IsTestProject=true`, NUnit 4, NUnit3TestAdapter and Microsoft.NET.Test.Sdk, referencing `../../../Junevy.Controls.csproj`. Add it using:

```powershell
dotnet sln Junevy.Controls.sln add Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj
```

- [ ] **Step 2: Write failing default-contract tests**

Use `[Apartment(ApartmentState.STA)]` and assert these exact defaults:

```csharp
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
});
```

Also assert `ToolboxItem.DisplayMode == IconOnly`, `ToolItem.DisplayMode == IconAndTitle`, and `ToolItem.IsDragEnabled == true`.

- [ ] **Step 3: Run the focused tests and verify the expected compile failure**

Run:

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter ToolboxDefaultsTests
```

Expected: compilation fails because the new control types do not exist.

- [ ] **Step 4: Implement the enums, control shells and dependency properties**

Implement the exact public signatures specified in the design document. Use `ValidateValueCallback` for the following rules:

```csharp
private static bool IsNonNegativeDelay(object value) => (TimeSpan)value >= TimeSpan.Zero;
private static bool IsPositiveDouble(object value) => (double)value > 0 && !double.IsNaN((double)value);
private static bool IsPositiveColumnCount(object value) => (int)value >= 1;
private static bool IsNonEmptyFormat(object value) => value is string text && !string.IsNullOrWhiteSpace(text);
```

Register read-only `ActiveItemProperty` and `IsOpenProperty` using `DependencyProperty.RegisterReadOnly`. Override `DefaultStyleKeyProperty` for all three controls. `Toolbox.GetContainerForItemOverride()` must return `ToolboxItem`; `ToolboxItem.GetContainerForItemOverride()` must return `ToolItem`. Explicit containers must remain their own containers.

- [ ] **Step 5: Add XAML namespace and test access declarations**

Add to `AssemblyInfo.cs`:

```csharp
[assembly: XmlnsDefinition("github.com.junevy", "Junevy.Controls.Controls.Toolbox")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Junevy.Controls.Tests")]
```

- [ ] **Step 6: Run contract tests and both product builds**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter ToolboxDefaultsTests
dotnet build Junevy.Controls.csproj -f net8.0-windows
dotnet build Junevy.Controls.csproj -f net48
```

Expected: all commands succeed.

- [ ] **Step 7: Commit the contract slice**

```powershell
git add Common Controls/Toolbox AssemblyInfo.cs Tests/Junevy.Controls.Tests Junevy.Controls.sln
git commit -m "feat: add toolbox control contracts"
```

---

### Task 2: Implement Container Preparation and Single-Owner State

**Files:**
- Modify: `Controls/Toolbox/Toolbox.cs`
- Modify: `Controls/Toolbox/ToolboxItem.cs`
- Modify: `Controls/Toolbox/ToolItem.cs`
- Create: `Tests/Junevy.Controls.Tests/Toolbox/ToolboxContainerTests.cs`

**Interfaces:**
- Consumes: Task 1 control types and read-only state properties.
- Produces: `Toolbox.RequestOpen`, `RequestClose`, `NotifyDragStarted`, `NotifyDragCompleted` internal coordination and automatic data-item drag payload assignment.

- [ ] **Step 1: Write STA tests for standard WPF container behavior**

Host a `Toolbox` in a temporary `Window`, call `Show()` and `UpdateLayout()`, then assert:

```csharp
ToolboxItem groupContainer = (ToolboxItem)toolbox.ItemContainerGenerator.ContainerFromItem(groupData);
ToolItem toolContainer = (ToolItem)groupContainer.ItemContainerGenerator.ContainerFromItem(toolData);
Assert.That(toolContainer.DragData, Is.SameAs(toolData));
```

Add a second test proving explicitly supplied `ToolboxItem` and `ToolItem` instances remain their own containers. Close the temporary Window in `finally`.

- [ ] **Step 2: Write a failing single-active-item test**

Set `OpenDelay=TimeSpan.Zero`, request item A then item B through internal methods, pump the Dispatcher at `DispatcherPriority.Background`, and assert:

```csharp
Assert.Multiple(() =>
{
    Assert.That(itemA.IsOpen, Is.False);
    Assert.That(itemB.IsOpen, Is.True);
    Assert.That(toolbox.ActiveItem, Is.SameAs(itemB));
});
```

- [ ] **Step 3: Run the tests and verify failure**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter ToolboxContainerTests
```

Expected: tests fail because preparation and coordination are not implemented.

- [ ] **Step 4: Implement owner wiring and cleanup**

Override `PrepareContainerForItemOverride` and `ClearContainerForItemOverride` in both item controls. Cache owners through internal `AttachOwner`/`DetachOwner` methods. For generated `ToolItem` containers, use `SetCurrentValue(DragDataProperty, item)` only when `ReadLocalValue(DragDataProperty) == DependencyProperty.UnsetValue`; do not overwrite an explicit Style or local value.

- [ ] **Step 5: Implement the two-timer coordinator**

Use one `DispatcherTimer` for opening and one for closing. Track `_pendingItem`, `_activeItem`, and `_isDragging`. Every open tick must recheck `IsLoaded`, `IsEnabled`, `HasItems` and `IsPointerOverTrigger`. `SetActiveItem` must close the old item before opening the new one. `ClosePopup()` cancels both timers, closes the active item and clears the read-only `ActiveItem`.

- [ ] **Step 6: Add lifecycle cleanup**

On `Loaded`, subscribe to the owning Window's `Deactivated`, `StateChanged`, `LocationChanged` and `SizeChanged`. On `Unloaded`, unsubscribe all Window handlers, stop timers and close the Popup. Window deactivation or minimization calls `ClosePopup()` immediately.

- [ ] **Step 7: Run tests and build both targets**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter "ToolboxContainerTests|ToolboxDefaultsTests"
dotnet build Junevy.Controls.csproj -f net8.0-windows
dotnet build Junevy.Controls.csproj -f net48
```

- [ ] **Step 8: Commit the coordination slice**

```powershell
git add Controls/Toolbox Tests/Junevy.Controls.Tests/Toolbox
git commit -m "feat: coordinate toolbox popup state"
```

---

### Task 3: Build the Six-Column Popup Template

**Files:**
- Create: `Controls/Toolbox/Toolbox.xaml`
- Modify: `Controls/Toolbox/ToolboxItem.cs`
- Modify: `Themes/Generic.xaml`
- Modify: `Tests/Junevy.Controls.Tests/Toolbox/ToolboxContainerTests.cs`

**Interfaces:**
- Consumes: `ToolboxItem.IsOpen`, root layout properties and existing Theme resources.
- Produces: `PART_TriggerButton`, `PART_Popup`, `DefaultToolboxStyle`, `DefaultToolboxItemStyle`, `DefaultToolItemStyle`.

- [ ] **Step 1: Write a failing template-parts and layout test**

After applying the theme in an STA Window, assert `ToolboxItem.Template.FindName("PART_TriggerButton", item)` is a `ButtonBase`, `PART_Popup` is a `Popup`, and the generated items panel is a `UniformGrid` whose `Columns` is 6. Add 7 child tools and assert the seventh child's arranged Y coordinate is greater than the first child's arranged Y coordinate.

- [ ] **Step 2: Run the focused test and verify failure**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter "ToolboxContainerTests"
```

- [ ] **Step 3: Implement the root Toolbox template**

Use an `ItemsPresenter` inside the root Border. The default ItemsPanel is a `VirtualizingStackPanel` whose orientation binds to `Toolbox.Orientation`. Do not add a ScrollViewer to the root control unless the consumer requests one through an outer container.

- [ ] **Step 4: Implement the ToolboxItem template**

The trigger uses a `Button` named `PART_TriggerButton`. Bind icon and title vertically, collapse the title for `IconOnly`, and set both `ToolTip` and `AutomationProperties.Name` from `Title`. The Popup must use:

```xml
<Popup x:Name="PART_Popup"
       AllowsTransparency="True"
       Focusable="False"
       IsOpen="{TemplateBinding IsOpen}"
       Placement="Custom"
       PlacementTarget="{Binding ElementName=PART_TriggerButton}"
       PopupAnimation="Fade"
       StaysOpen="True">
```

Inside it, use a themed Border with total Width bound to `PopupWidth`, followed by a ScrollViewer with horizontal scrolling disabled and vertical scrolling Auto. The ItemsPanel is `UniformGrid` with `Columns` bound to `ColumnCount`.

- [ ] **Step 5: Implement the ToolItem visual contract**

Use fixed default Height 68, transparent background, themed hover/pressed/disabled states, centered icon, and one-line centered title with `TextTrimming=CharacterEllipsis`. Bind `AutomationProperties.Name` and ToolTip to Title. Do not hardcode item Width.

- [ ] **Step 6: Attach and detach template events safely**

In `ToolboxItem.OnApplyTemplate()`, detach handlers from old parts before `base.OnApplyTemplate()`, then attach `MouseEnter`, `MouseLeave`, `Click`, `PreviewKeyDown`, Popup `MouseEnter` and Popup `MouseLeave` to new parts. Reapplying a theme must not duplicate handlers.

- [ ] **Step 7: Merge the new resource dictionary**

Add this once to `Themes/Generic.xaml` near the other menu/control resources:

```xml
<ResourceDictionary Source="/Junevy.Controls;component/Controls/Toolbox/Toolbox.xaml" />
```

- [ ] **Step 8: Verify layout and cross-target XAML compilation**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter ToolboxContainerTests
dotnet build Junevy.Controls.csproj -f net8.0-windows
dotnet build Junevy.Controls.csproj -f net48
```

- [ ] **Step 9: Commit the visual slice**

```powershell
git add Controls/Toolbox/Toolbox.xaml Controls/Toolbox/ToolboxItem.cs Themes/Generic.xaml Tests/Junevy.Controls.Tests
git commit -m "feat: add six-column toolbox popup template"
```

---

### Task 4: Add Hover Timing, Pointer Transfer and Keyboard Behavior

**Files:**
- Modify: `Controls/Toolbox/Toolbox.cs`
- Modify: `Controls/Toolbox/ToolboxItem.cs`
- Modify: `Tests/Junevy.Controls.Tests/Toolbox/ToolboxContainerTests.cs`

**Interfaces:**
- Consumes: Task 2 coordinator and Task 3 template parts.
- Produces: stable trigger-to-Popup transfer, click toggle, Enter/Space, Escape and focus return.

- [ ] **Step 1: Write failing interaction tests with zero delays**

Cover these transitions independently: trigger enter opens; trigger leave plus Popup enter stays open; both leave closes; entering B cancels pending A; clicking active trigger closes; Escape closes and focuses the trigger. Use zero delays and pump the Dispatcher so tests remain deterministic.

- [ ] **Step 2: Run the focused tests and verify failure**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter ToolboxContainerTests
```

- [ ] **Step 3: Implement pointer flags and intent forwarding**

Maintain `IsPointerOverTrigger` and `IsPointerOverPopup` inside `ToolboxItem`. Trigger enter calls `owner.RequestOpen(this)`; leaving either region calls `owner.RequestClose(this)` only when both flags are false. Entering either region cancels a pending close.

- [ ] **Step 4: Implement click and keyboard behavior**

Click, Enter and Space toggle the current item through the owner. Escape invokes `owner.ClosePopup()` and returns focus to `PART_TriggerButton`. When opening from keyboard, move focus to the first enabled `ToolItem` using `MoveFocus(new TraversalRequest(FocusNavigationDirection.First))`.

- [ ] **Step 5: Add empty/disabled/dynamic collection guards**

An item with `HasItems=false` or `IsEnabled=false` never opens. Observe collection changes through WPF item notifications; if the active group becomes empty or disabled, close it immediately. Do not hold strong event subscriptions to a replaced external collection after the container is cleared.

- [ ] **Step 6: Run tests and both builds**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter ToolboxContainerTests
dotnet build Junevy.Controls.csproj -f net8.0-windows
dotnet build Junevy.Controls.csproj -f net48
```

- [ ] **Step 7: Commit the interaction slice**

```powershell
git add Controls/Toolbox Tests/Junevy.Controls.Tests/Toolbox
git commit -m "feat: add delayed toolbox hover behavior"
```

---

### Task 5: Implement Smart Popup Placement and Monitor Bounds

**Files:**
- Create: `Controls/Toolbox/ToolboxPopupPlacementCalculator.cs`
- Create: `Controls/Toolbox/MonitorWorkAreaProvider.cs`
- Modify: `Controls/Toolbox/ToolboxItem.cs`
- Create: `Tests/Junevy.Controls.Tests/Toolbox/PopupPlacementCalculatorTests.cs`

**Interfaces:**
- Consumes: `Orientation`, `ToolboxPopupPlacement`, target/popup `Size` and current Window.
- Produces: `CustomPopupPlacement[] GetPlacements(Size popupSize, Size targetSize, Point offset, Orientation orientation, ToolboxPopupPlacement preference)` and `Rect GetWorkAreaDip(Visual target)`.

- [ ] **Step 1: Write failing placement-order tests**

Assert exact Auto priorities:

```csharp
Assert.That(vertical.Select(x => x.Direction), Is.EqualTo(new[]
{
    ToolboxPopupPlacement.Right,
    ToolboxPopupPlacement.Left,
    ToolboxPopupPlacement.Bottom,
    ToolboxPopupPlacement.Top
}));

Assert.That(horizontal.Select(x => x.Direction), Is.EqualTo(new[]
{
    ToolboxPopupPlacement.Bottom,
    ToolboxPopupPlacement.Top,
    ToolboxPopupPlacement.Right,
    ToolboxPopupPlacement.Left
}));
```

For target `40x40` and popup `300x200`, assert Right starts at `(40,0)`, Left at `(-300,0)`, Bottom at `(0,40)`, and Top at `(0,-200)`. Explicit Left must be first but still contain the other three fallbacks once each.

- [ ] **Step 2: Run tests and verify failure**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter PopupPlacementCalculatorTests
```

- [ ] **Step 3: Implement the pure placement calculator**

Return four deterministic candidates and map them to WPF `CustomPopupPlacement` values. Keep all arithmetic in DIP. Do not query screens or Window state in this class.

- [ ] **Step 4: Implement monitor-aware work area conversion**

Use `WindowInteropHelper`, `MonitorFromWindow(MONITOR_DEFAULTTONEAREST)` and `GetMonitorInfo`. Convert the physical work-area rectangle with `PresentationSource.FromVisual(target).CompositionTarget.TransformFromDevice`. Fall back to the target Window bounds when a presentation source is temporarily unavailable; never fall back to the primary screen work area.

- [ ] **Step 5: Wire Popup callbacks and effective max height**

Assign `CustomPopupPlacementCallback` in `OnApplyTemplate`. Before opening, set the ScrollViewer/Border maximum height to `Math.Min(PopupMaxHeight, workArea.Height - 16d)`. Guard the result with a minimum of one ToolItem row plus popup chrome.

- [ ] **Step 6: Reposition on host changes**

On Window `LocationChanged` and `SizeChanged`, request reposition only for the active item. Implement reposition by changing `HorizontalOffset` by `0.01` DIP and restoring it in the same Dispatcher turn; do not close/reopen the Popup and do not move focus.

- [ ] **Step 7: Run placement tests and both builds**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter PopupPlacementCalculatorTests
dotnet build Junevy.Controls.csproj -f net8.0-windows
dotnet build Junevy.Controls.csproj -f net48
```

- [ ] **Step 8: Commit the positioning slice**

```powershell
git add Controls/Toolbox Tests/Junevy.Controls.Tests/Toolbox
git commit -m "feat: position toolbox popup within current monitor"
```

---

### Task 6: Implement Drag-to-Canvas Semantics

**Files:**
- Modify: `Controls/Toolbox/ToolItem.cs`
- Modify: `Controls/Toolbox/ToolboxItem.cs`
- Modify: `Controls/Toolbox/Toolbox.cs`
- Create: `Tests/Junevy.Controls.Tests/Toolbox/ToolItemDragTests.cs`

**Interfaces:**
- Consumes: `ToolItem.DragData`, `DragDataFormat`, root owner coordination and WPF DragDrop.
- Produces: thresholded Copy drag, Popup retention during drag and Click suppression for the same mouse gesture.

- [ ] **Step 1: Write failing threshold tests**

Expose this internal pure helper for tests:

```csharp
internal static bool ExceedsDragThreshold(Point start, Point current, double minHorizontal, double minVertical)
```

Assert movements below both thresholds are false, and exceeding either threshold is true.

- [ ] **Step 2: Write failing DataObject tests**

Extract an internal `CreateDragDataObject()` method and assert it contains the exact configured format and the same `DragData` reference. Assert it returns null if drag is disabled, data is null, or the format is empty.

- [ ] **Step 3: Run focused tests and verify failure**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter ToolItemDragTests
```

- [ ] **Step 4: Implement pointer tracking and drag initiation**

Record the start point on left-button down. On mouse move with the left button pressed, compare against WPF system thresholds. Before calling `DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy)`, set `_suppressClick=true` and notify the owning group/root that dragging started. Always notify drag completion in `finally`.

- [ ] **Step 5: Suppress only the drag gesture's Click**

Override `OnClick()`. If `_suppressClick` is true, clear it and return without calling `base.OnClick()`; otherwise preserve standard Button behavior. Clear stale gesture state on lost mouse capture and on a normal mouse-up.

- [ ] **Step 6: Keep Popup open while DragDrop owns the mouse**

`NotifyDragStarted` stops the close timer. `NotifyDragCompleted` clears the drag lock and closes only when the pointer is outside both trigger and Popup. A canceled drag follows the same cleanup path.

- [ ] **Step 7: Run tests and both builds**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter ToolItemDragTests
dotnet build Junevy.Controls.csproj -f net8.0-windows
dotnet build Junevy.Controls.csproj -f net48
```

- [ ] **Step 8: Commit the drag slice**

```powershell
git add Controls/Toolbox Tests/Junevy.Controls.Tests/Toolbox
git commit -m "feat: drag toolbox data to design surfaces"
```

---

### Task 7: Add a Tracked Demo and Perform Visual Acceptance

**Files:**
- Create: `Samples/Junevy.Controls.ToolboxDemo/Junevy.Controls.ToolboxDemo.csproj`
- Create: `Samples/Junevy.Controls.ToolboxDemo/App.xaml`
- Create: `Samples/Junevy.Controls.ToolboxDemo/App.xaml.cs`
- Create: `Samples/Junevy.Controls.ToolboxDemo/MainWindow.xaml`
- Create: `Samples/Junevy.Controls.ToolboxDemo/MainWindow.xaml.cs`
- Create: `Samples/Junevy.Controls.ToolboxDemo/ToolDefinition.cs`
- Modify: `Junevy.Controls.sln`

**Interfaces:**
- Consumes: completed Toolbox public API and standard WPF Canvas Drop events.
- Produces: reproducible visual/manual acceptance surface with 2 groups and at least 14 tools.

- [ ] **Step 1: Create the minimal WPF demo project**

Target `net8.0-windows`, enable WPF, reference `../../../Junevy.Controls.csproj`, and add it to the solution. Do not add DI, MVVM packages or navigation frameworks.

- [ ] **Step 2: Define immutable demo data**

```csharp
public sealed record ToolDefinition(string Title, string Icon, string Kind);
```

Create two observable group collections. The first contains 14 tools so rows 1, 2 and 3 are visible; include one long title to verify ellipsis.

- [ ] **Step 3: Build the demo layout**

Use a left-side vertical `Toolbox` and a large `Canvas AllowDrop=true`. Bind group icons/titles with `Toolbox.ItemContainerStyle`; bind inner tool icons/titles with each group's `ItemContainerStyle`. Set `PopupWidth=300`, `ColumnCount=6`, `OpenDelay=0:0:0.15`, and `CloseDelay=0:0:0.30`.

- [ ] **Step 4: Implement Canvas drag feedback and Drop**

On `DragOver`, accept only `Junevy.Controls.Tool` and set Copy. On Drop, create a small themed Border at `e.GetPosition(canvas)` showing the `ToolDefinition.Title`. This demo behavior verifies the payload but remains outside the control library.

- [ ] **Step 5: Run the demo and complete the acceptance matrix**

```powershell
dotnet run --project Samples/Junevy.Controls.ToolboxDemo/Junevy.Controls.ToolboxDemo.csproj
```

Verify manually:

- Hover opens after approximately 150 ms; crossing to Popup does not close it.
- 14 tools render as 6 + 6 + 2 without horizontal scrolling.
- Long title is ellipsized and its ToolTip shows the full title.
- Rapidly moving between groups never leaves two Popups open.
- Clicking and keyboard navigation work; Escape closes and restores focus.
- Dragging creates exactly one Canvas node and does not execute Click.
- Moving the Window near all four edges changes placement as needed.
- Moving between monitors at different scaling values keeps the Popup adjacent and on-screen.
- Switching light/dark resources keeps text, hover and borders readable.

- [ ] **Step 6: Commit the demo slice**

```powershell
git add Samples/Junevy.Controls.ToolboxDemo Junevy.Controls.sln
git commit -m "test: add toolbox interaction demo"
```

---

### Task 8: Document the Control and Run Final Regression

**Files:**
- Modify: `README.md`
- Review: `docs/superpowers/specs/2026-08-29-hover-toolbox-control-design.md`
- Review: all files created in Tasks 1-7

**Interfaces:**
- Consumes: final public API and demo syntax.
- Produces: user-facing documentation and final verification evidence.

- [ ] **Step 1: Add README API documentation**

Document `Toolbox`, `ToolboxItem`, `ToolItem`, every public property/default, ItemsSource container rules, the default drag format, Canvas Drop example, Popup positioning behavior and the limitation that Canvas node creation belongs to the consuming application. Add all three controls to the control index and dependency table.

- [ ] **Step 2: Run the complete automated test suite**

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --configuration Release
```

Expected: all tests pass with zero skipped tests.

- [ ] **Step 3: Build every target in Release**

```powershell
dotnet build Junevy.Controls.csproj -c Release -f net8.0-windows
dotnet build Junevy.Controls.csproj -c Release -f net48
dotnet build Samples/Junevy.Controls.ToolboxDemo/Junevy.Controls.ToolboxDemo.csproj -c Release
```

Expected: all builds succeed without new warnings.

- [ ] **Step 4: Run regression checks for existing controls**

Build the full solution and open the existing ToolBar/AppBar demo views. Confirm the original `ToolBar` remains single-row, its `ToolBarItem` commands still execute, and no implicit Toolbox style affects `Button`, `ItemsControl` or `HeaderedItemsControl` globally.

```powershell
dotnet build Junevy.Controls.sln -c Release
```

- [ ] **Step 5: Inspect the final diff for scope and generated files**

```powershell
git status --short
git diff --check
git diff --stat
```

Expected: no `bin/`, `obj/`, user settings or unrelated source changes are staged; `git diff --check` reports no whitespace errors.

- [ ] **Step 6: Commit documentation and final verification changes**

```powershell
git add README.md
git commit -m "docs: document hover toolbox control"
```

---

## Execution Gates

Implementation must pause for review at these points:

1. After Task 1: approve public API names and defaults before behavior is added.
2. After Task 3: approve the 300 DIP/six-column visual template before drag and positioning work.
3. After Task 6: approve interaction behavior before adding the demo and documentation.
4. After Task 8: review final diff and verification evidence before release or package version changes.

The implementation does not change the package version, publish a NuGet package, or push commits unless separately authorized.
