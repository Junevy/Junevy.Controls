# Task 8 Report: Toolbox Documentation and Release Regression

## Outcome

Updated `README.md` with the public `Toolbox`, `ToolboxItem`, and `ToolItem` API, including every public property and default value, validation constraints, standard `ItemsSource` and explicit-container rules, default drag format and business-data payload semantics, Canvas Drop example, popup placement and multi-monitor behavior, and the limitation that Canvas node creation belongs to the consuming application. Added all three controls to the control index and dependency table. No product or demo source files were changed.

## Verification

- `dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --configuration Release`: passed, 92/92 on `net8.0-windows` and 92/92 on `net48`, zero failures and zero skipped tests.
- `dotnet build Junevy.Controls.csproj -c Release -f net8.0-windows`: succeeded, 0 warnings, 0 errors.
- `dotnet build Junevy.Controls.csproj -c Release -f net48`: succeeded, 0 warnings, 0 errors.
- `dotnet build Samples/Junevy.Controls.ToolboxDemo/Junevy.Controls.ToolboxDemo.csproj -c Release`: succeeded, 0 warnings, 0 errors.
- `dotnet build Junevy.Controls.sln -c Release`: succeeded, 0 warnings, 0 errors; existing product, test, demo, and legacy application projects all built.
- `git diff --check`: passed.

The SDK emitted informational `NETSDK1057` messages because the installed .NET SDK is a preview build. An initial parallel build/test batch also hit a transient shared `obj/Release/net48` file lock; all required commands were rerun serially and passed.

Interactive WPF visual and multi-monitor checks remain manual acceptance activities, as noted in the Task 7 report; no new concern was found in the documented API or release build matrix.
