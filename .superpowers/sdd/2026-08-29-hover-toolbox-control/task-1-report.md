# Task 1 Implementation Report

## Status

Completed and committed as `97ebadd` (`feat: add toolbox control contracts`).

## Files Changed

- `Common/ToolboxDisplayMode.cs`
- `Common/ToolboxPopupPlacement.cs`
- `Controls/Toolbox/Toolbox.cs`
- `Controls/Toolbox/ToolboxItem.cs`
- `Controls/Toolbox/ToolItem.cs`
- `AssemblyInfo.cs`
- `Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj`
- `Tests/Junevy.Controls.Tests/Toolbox/ToolboxDefaultsTests.cs`
- `Junevy.Controls.sln`
- Approved design: `docs/superpowers/specs/2026-08-29-hover-toolbox-control-design.md`
- Approved plan: `docs/superpowers/plans/2026-08-29-hover-toolbox-control.md`

No `Toolbox.xaml`, `Themes/Generic.xaml`, README, sample, existing ToolBar, popup behavior, placement behavior, timer behavior, or drag mechanics were changed.

## TDD Evidence

### Red

Command:

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter ToolboxDefaultsTests
```

Observed expected compile failure before production types were added:

```text
error CS0234: Junevy.Controls.Controls does not contain namespace Toolbox
```

The repository SDK glob initially also compiled nested test sources into the product project. The test project now defines `JUNEVY_CONTROLS_TESTS`, and the test source is conditionally compiled under that symbol so product builds do not acquire NUnit dependencies. Assembly attribute generation is disabled in the nested test project to avoid generated `Tests/**/obj` sources colliding with the parent SDK project glob.

### Green

Command:

```powershell
dotnet test Tests/Junevy.Controls.Tests/Junevy.Controls.Tests.csproj --filter ToolboxDefaultsTests --no-restore
```

Result:

```text
net8.0-windows: 20 passed, 0 failed, 0 skipped
net48:           20 passed, 0 failed, 0 skipped
```

Commands:

```powershell
dotnet build Junevy.Controls.csproj -f net8.0-windows --no-restore
dotnet build Junevy.Controls.csproj -f net48 --no-restore
```

Result: both product builds succeeded with 0 warnings and 0 errors. The SDK emitted only the informational `NETSDK1057` preview-SDK message.

## Plan Correction Applied

Per test pre-review, the test project targets both `net8.0-windows` and `net48` instead of only `net8.0-windows`. The WPF fixture uses both `[Apartment(ApartmentState.STA)]` and `[NonParallelizable]`. Contract and dependency-property validation tests execute on both frameworks.

## Self-Review

- Public enum values and ordering match the approved design.
- Exact control defaults are covered on both target frameworks.
- Negative delays, non-positive/NaN/infinite dimensions, column counts below one, and blank root drag formats are rejected.
- `ActiveItem` and `IsOpen` use read-only dependency properties.
- All three controls override their default style key.
- Generated containers are `ToolboxItem`/`ToolItem`, while explicit containers are preserved.
- `ClosePopup()` is currently an intentionally minimal no-op state reset because Task 1 contains no opening behavior.
- The public XAML namespace and test internals access are declared.
- `git diff --cached --check` passed before commit.

## Concerns

- The environment uses a .NET 10 preview SDK, which emits `NETSDK1057`; this is informational and did not affect either target framework.
- No template exists yet by design, so the new controls are public contract shells until later tasks add styles and behavior.

## Fix Round: Project Isolation

Independent review of `97ebadd` found that the root SDK project still evaluated nested test sources and generated `obj` sources as product compiler inputs. The temporary `JUNEVY_CONTROLS_TESTS` source guard and disabled assembly metadata generation were removed.

The root `Junevy.Controls.csproj` now excludes `Tests/**` and planned `Samples/**` trees through `DefaultItemExcludes`, plus explicit `Compile`, `Page`, and `ApplicationDefinition` removals. Evaluated MSBuild `Compile`/`Page`/`ApplicationDefinition` inputs contain zero paths under either nested tree.

The product and test projects were cleaned for both targets, then the four verified generated directories were removed: product `bin`/`obj` and test-project `bin`/`obj`. Verification from the clean state produced:

```text
ToolboxDefaultsTests net8.0-windows: 20 passed, 0 failed, 0 skipped
ToolboxDefaultsTests net48:           20 passed, 0 failed, 0 skipped
Product net8.0-windows build:         succeeded, 0 warnings, 0 errors
Product net48 build:                  succeeded, 0 warnings, 0 errors
```

Standard test assembly metadata is restored:

```text
net8.0-windows: AssemblyVersion=1.0.0.0; TargetFramework=.NETCoreApp,Version=v8.0
net48:           AssemblyVersion=1.0.0.0; TargetFramework=.NETFramework,Version=v4.8
```

Fix commit message: `fix: isolate toolbox test project`.
