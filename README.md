# Junevy.Controls

Junevy.Controls is a WPF custom control library for desktop applications. It provides a unified visual language, light and dark theme resources, icon font integration, and a set of commonly used controls for navigation, input, buttons, data display, and image viewing.

The library targets:

- .NET 8 WPF: `net8.0-windows`
- .NET Framework WPF: `net48`

## Features

- Unified theme tokens for colors, corner radius, padding, borders, focus states, disabled states, and shadows.
- Light and dark palettes in `Themes/AppColors.Light.xaml` and `Themes/AppColors.Dark.xaml`.
- Keyboard-friendly focus visuals with rounded corners aligned to each control.
- Reusable styles loaded through `Themes/Generic.xaml`.
- Icon font and image resources are embedded in `Junevy.Controls`.

## Quick Start

Add a reference to `Junevy.Controls`, then merge the generic resource dictionary in your WPF application:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/Junevy.Controls;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Use the controls with the namespace below:

```xml
xmlns:jv="clr-namespace:Junevy.Controls.Controls.Menu;assembly=Junevy.Controls"
```

For controls from other namespaces, reference the matching namespace, for example:

```xml
xmlns:jvText="clr-namespace:Junevy.Controls.Controls.Text;assembly=Junevy.Controls"
xmlns:jvButton="clr-namespace:Junevy.Controls.Controls.Button;assembly=Junevy.Controls"
xmlns:jvBox="clr-namespace:Junevy.Controls.Controls.Box;assembly=Junevy.Controls"
```

## Themes

Theme resources follow semantic design-token naming. Prefer the new keys below for new controls and applications:

- `Theme.Brush.Background.App`: application and page background.
- `Theme.Brush.Surface.Base`: default panel, card, and control background.
- `Theme.Brush.Surface.Raised`: elevated containers.
- `Theme.Brush.Surface.Sunken`: inset areas such as input focus backgrounds.
- `Theme.Brush.Surface.Hover`: hover state.
- `Theme.Brush.Surface.Pressed`: pressed state.
- `Theme.Brush.Surface.Selected`: selected state.
- `Theme.Brush.Text.Primary`: primary text.
- `Theme.Brush.Text.Secondary`: secondary text.
- `Theme.Brush.Text.Tertiary`: low-emphasis text.
- `Theme.Brush.Border.Default`: standard border.
- `Theme.Brush.Border.Strong`: emphasized border.
- `Theme.Brush.Border.Focus`: keyboard and focus indication.
- `Theme.Brush.Accent.Primary`: brand and primary action color.
- `Theme.Brush.Status.Success`: success state.
- `Theme.Brush.Status.Warning`: warning state.
- `Theme.Brush.Status.Danger`: danger or error state.
- `Theme.Brush.ScrollBar.Thumb`: scrollbar thumb.
- `Theme.Brush.ScrollBar.ThumbHover`: scrollbar thumb on hover.
- `Theme.Brush.Overlay.Backdrop`: modal/dialog backdrop.
- `Theme.ControlCornerRadius`
- `Theme.ControlPadding`

All resource keys use the semantic `Theme.Brush.*` / `Theme.Color.*` convention. No legacy aliases — every key has a single, semantic name.

`Themes/AppColors.xaml` loads the default (Light) palette. Use `ThemeManager.ApplyTheme(AppTheme.Dark)` for runtime theme switching. The manager replaces the active theme dictionary; never merge both themes at the same time.

## Controls

### Navigation

- `SideMenu`: Side navigation menu with horizontal and vertical layout support.
- `TreeMenu`: Hierarchical navigation menu.
- `TabMenu`: Tab navigation with editable tab headers.
- `ToolBar`: Toolbar and toolbar item styles.
- `ContextMenu`: Themed context menu with optional icons and nested submenus.

Example:

```xml
<jv:ContextMenu>
    <jv:ContextMenuItem Header="Open" Icon="&#xE001;" />
    <jv:ContextMenuItem Header="Export">
        <jv:ContextMenuItem Header="PNG" />
        <jv:ContextMenuItem Header="JPEG" />
    </jv:ContextMenuItem>
</jv:ContextMenu>
```

`ContextMenuItem` uses WPF's native `Header`, `Icon`, `Command`, `InputGestureText`, and `Items` properties. Empty icons collapse their host column, so text starts at the same left edge whether an icon is supplied or not.

### Buttons

- `Button`: Standard and icon button styles.
- `CardButton`: Card-like clickable container.
- `ToggleButton`: Switch and expander toggle styles.
- `RadioButton`: Rectangular and circular radio button styles.

### Input

- `TextBox`: Text input with placeholder, icon support, clear button, and unified focus style.
- `ComboBox`: Themed dropdown selector with placeholder support.
- `CheckBox`: Themed checkbox with icon font check mark.

### Display

- `Label`: Semantic label styles for normal, success, warning, and danger states.
- `TextTitle`: Title text control.
- `ListBox`: Theme-aware selectable list with virtualized scrolling.
- `ListView`: Theme-aware list view with standard content and `GridView` layouts.
- `DataGrid`: Themed data grid styles.
- `ImageViewer`: Image display and viewing control.
- `AppBar`: Application title bar style.

## Notes

- Prefer `DynamicResource` for theme-dependent values so runtime theme switching can update existing controls.
- Keep focus visuals and control templates bound to shared theme radius resources to maintain consistent rounded corners.
- Avoid merging both `AppColors.Light.xaml` and `AppColors.Dark.xaml` into the same dictionary at the same time, because duplicate keys will make the last merged dictionary override the previous one.
