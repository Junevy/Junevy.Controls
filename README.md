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
- Icon font resources provided by `Junevy.Controls.Resources`.

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

Theme resources are defined as reusable keys such as:

- `Theme.BackgroundBrush`
- `Theme.SurfaceBrush`
- `Theme.PrimaryBrush`
- `Theme.BorderBrush`
- `Theme.FocusBrush`
- `Theme.ControlCornerRadius`
- `Theme.ControlPadding`

`Themes/AppColors.xaml` should load only the default palette. Runtime theme switching should replace the active theme dictionary instead of merging light and dark dictionaries at the same time.

## Controls

### Navigation

- `SideMenu`: Side navigation menu with horizontal and vertical layout support.
- `TreeMenu`: Hierarchical navigation menu.
- `TabMenu`: Tab navigation with editable tab headers.
- `ToolBar`: Toolbar and toolbar item styles.

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
- `DataGrid`: Themed data grid styles.
- `ImageViewer`: Image display and viewing control.
- `AppBar`: Application title bar style.

## Notes

- Prefer `DynamicResource` for theme-dependent values so runtime theme switching can update existing controls.
- Keep focus visuals and control templates bound to shared theme radius resources to maintain consistent rounded corners.
- Avoid merging both `AppColors.Light.xaml` and `AppColors.Dark.xaml` into the same dictionary at the same time, because duplicate keys will make the last merged dictionary override the previous one.
