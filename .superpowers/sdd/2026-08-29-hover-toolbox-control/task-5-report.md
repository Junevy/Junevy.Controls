# Task 5 Report: Smart Popup Placement and Monitor Bounds

## Outcome

- Added a pure DIP placement calculator that always returns four deterministic candidates.
- Auto order is Right/Left/Bottom/Top for vertical toolboxes and Bottom/Top/Right/Left for horizontal toolboxes.
- An explicit placement is promoted to the first candidate while every other direction remains available exactly once.
- Added monitor work-area lookup through `WindowInteropHelper`, `MonitorFromWindow(MONITOR_DEFAULTTONEAREST)`, and `GetMonitorInfo`. Physical monitor coordinates are converted to DIP with the target presentation source's `TransformFromDevice`.
- Wired `PART_Popup` to `CustomPopupPlacementCallback`.
- Constrained popup height to `min(PopupMaxHeight, max(0, workArea.Height - 16))`.
- Window location and size changes reposition only the active open popup by changing `HorizontalOffset` by 0.01 DIP and immediately restoring it. The popup remains open and focus is unchanged.

## Height Constraint Decision

The specification asks for at least one tool row while also requiring the popup to remain inside the monitor work area. Those requirements conflict when `workArea.Height - 16` is smaller than one complete row plus popup chrome. The implementation treats the monitor boundary as the hard constraint and does not raise `MaxHeight` above the available height. In that extreme case the popup is clipped/scrollable rather than extending outside the current monitor. On normal work areas, the available height is comfortably larger than one row.

## Verification

- `dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj -f net8.0-windows --filter PopupPlacementCalculatorTests --no-restore`: 4 passed.
- `dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj -f net48 --filter PopupPlacementCalculatorTests --no-restore`: 4 passed.
- `dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj -f net8.0-windows --filter "ToolboxContainerTests|ToolboxDefaultsTests" --no-restore`: 66 passed.
- `dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj -f net48 --filter "ToolboxContainerTests|ToolboxDefaultsTests" --no-restore`: 66 passed.
- `dotnet build Junevy.Controls.csproj -f net8.0-windows --no-restore`: succeeded, 0 warnings, 0 errors.
- `dotnet build Junevy.Controls.csproj -f net48 --no-restore`: succeeded, 0 warnings, 0 errors.
- `git diff --check`: passed.

## Concerns

- Automated tests cover deterministic placement order and coordinates. Actual multi-monitor movement and mixed-DPI transitions still require manual verification on representative hardware.
- The environment uses a preview .NET 10 SDK and emits informational `NETSDK1057` messages before builds/tests; the product builds themselves report zero warnings and zero errors.

## Review Follow-up

- Corrected WPF primary-axis metadata for custom placement candidates: Right and Left use `PopupPrimaryAxis.Vertical`; Bottom and Top use `PopupPrimaryAxis.Horizontal`. Tests now assert the axis for every direction.
- Reconfirmed the height decision: `min(PopupMaxHeight, max(0, workArea.Height - 16))` remains intentional. If the available work area is smaller than one complete row, keeping the popup inside the current monitor takes precedence over the one-row minimum.
