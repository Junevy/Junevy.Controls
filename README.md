# Junevy.Controls

Junevy.Controls 是一个面向 WPF 桌面应用的自定义控件库，提供统一的浅色/深色主题、图标字体、焦点及禁用状态，以及常用的按钮、输入、导航、数据展示和图像查看控件。

## 环境与依赖

| 项目 | 说明 |
| --- | --- |
| 目标框架 | `.NET 8 WPF (net8.0-windows)`、`.NET Framework 4.8 WPF (net48)` |
| 平台 | Windows / WPF |
| NuGet 依赖 | 控件库本身没有第三方包依赖 |
| WPF 程序集 | `PresentationFramework`、`PresentationCore`、`WindowsBase`；`net48` 还引用 `System.Xaml` |
| 主题入口 | `/Junevy.Controls;component/Themes/Generic.xaml` |
| 统一 XAML 命名空间 | `github.com.junevy` |

在项目中引用 `Junevy.Controls` 后，推荐在 `App.xaml` 合并完整主题资源：

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/Junevy.Controls;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

窗口或页面中使用以下命名空间：

```xml
xmlns:jv="github.com.junevy"
xmlns:atc="clr-namespace:Junevy.Controls.AttachedProperties;assembly=Junevy.Controls"
```

`jv` 包含控件库的全部公开控件。`atc` 用于 `Icon`、`AttachFuc` 和 `ExpanderBehavior` 附加属性。

## 主题

`Themes/Generic.xaml` 会加载默认浅色主题、所有控件样式、滚动条、焦点样式和内置图标字体。主题相关颜色应使用 `DynamicResource`，这样运行时切换主题后现有控件可以同步刷新。

| 资源键 | 用途 |
| --- | --- |
| `Theme.Brush.Background.App` | 应用或页面背景 |
| `Theme.Brush.Surface.Base` | 控件、面板的基础表面 |
| `Theme.Brush.Surface.Raised` | 抬高的表面 |
| `Theme.Brush.Surface.Sunken` | 输入焦点等内陷表面 |
| `Theme.Brush.Surface.Hover` | 鼠标悬停 |
| `Theme.Brush.Surface.Pressed` | 按下状态 |
| `Theme.Brush.Surface.Selected` | 选中状态 |
| `Theme.Brush.Text.Primary` | 主要文字 |
| `Theme.Brush.Text.Secondary` | 次要文字 |
| `Theme.Brush.Border.Default` | 默认边框 |
| `Theme.Brush.Border.Focus` | 焦点边框 |
| `Theme.Brush.Accent.Primary` | 主强调色 |
| `Theme.Brush.Status.Success` | 成功状态 |
| `Theme.Brush.Status.Warning` | 警告状态 |
| `Theme.Brush.Status.Danger` | 错误/危险状态 |
| `Theme.ControlCornerRadius` | 默认控件圆角 |
| `Theme.ControlPadding` | 默认控件内边距 |

运行时切换主题：

```csharp
using Junevy.Controls.Themes;

ThemeManager.ApplyTheme(AppTheme.Dark);
ThemeManager.ApplyTheme(AppTheme.Light);
ThemeManager.ToggleTheme();
```

`ThemeManager` 会替换现有主题字典，不要同时手动合并浅色和深色字典。

## 附加属性

### Icon

`Junevy.Controls.AttachedProperties.Icon` 为多个控件提供统一图标数据。

| 附加属性 | 默认值 | 实际效果 |
| --- | --- | --- |
| `atc:Icon.Icon` | `null` | 设置图标内容。可以是图标字体字符，也可以是 `Image`、`Path` 或其他对象。模板支持的控件会在值为空时折叠图标本身；周围布局是否收缩由具体控件决定。 |
| `atc:Icon.FontFamily` | 内置 `iconfont` | 设置图标字体。用于 `Button`、`CardButton`、`TextBox`、`Label`、`AppBar`、`SideMenu`、`TreeMenu`、`TabMenu` 等控件。 |
| `atc:Icon.IconSize` | `14` | 设置图标尺寸。`Button`、`AppBar`、`SideMenu` 和 `TreeMenu` 的模板会读取该值。 |
| `atc:Icon.IconForeground` | `Gray` | 设置图标颜色。`ToolboxItem` 和 `ToolItem` 的默认模板会读取该值；其他控件是否支持取决于其模板。 |

`ToolboxItem` 和 `ToolItem` 的图标字体字符跟随 `IconForeground`，标题跟随 `Foreground`；其他控件的图标字体字符通常跟随 `Foreground`。`Image` 或带固定 `Fill` 的 `Path` 不会自动重新着色。

```xml
<jv:Button
    atc:Icon.FontFamily="{DynamicResource IconFont}"
    atc:Icon.Icon="&#xE60F;"
    atc:Icon.IconSize="18"
    Content="Settings"
    Foreground="{DynamicResource Theme.Brush.Text.Primary}" />
```

### AttachFuc

| 附加属性 | 默认值 | 实际效果 |
| --- | --- | --- |
| `atc:AttachFuc.IsClosable` | `false` | `TextBox` 中控制清空按钮；`TabMenu` 中控制每个页签的关闭按钮。两个控件的默认样式会将它设为 `true`。 |
| `atc:AttachFuc.DisplayMode` | `Normal` | 已注册，当前默认模板没有读取它。 |
| `atc:AttachFuc.DispalyMode` | `Normal` | `DisplayMode` 的历史拼写兼容属性，当前默认模板没有读取它。新代码不要使用。 |

```xml
<jv:TextBox Width="220" atc:AttachFuc.IsClosable="True" Tag="Search..." />
```

### ExpanderBehavior

`atc:ExpanderBehavior.Enable` 用于 `TreeViewItem`。启用后，双击非叶节点会展开或折叠；双击叶节点会调用最近的 `TreeMenu.NavigateCommand`，命令参数是对应的 `TreeMenuItem`。

`TreeMenu` 的默认容器样式已经自动启用该行为，通常不需要手动设置。

### Border.CornerRadius

多个模板通过 WPF 的 `Border.CornerRadius` 依赖属性读取控件圆角，例如 `Button`、`TextBox`、`ComboBox`、`ToggleButton` 和 `ListView`：

```xml
<jv:Button Border.CornerRadius="8" Content="Run" />
```

这不是 Junevy 自定义附加属性，而是 WPF `Border` 的依赖属性附加写法。

## 按钮控件

### Button

`jv:Button` 继承 WPF `Button`，保留 `Command`、`Click`、`ContentTemplate`、键盘焦点和访问键等标准行为。默认模板同时支持文字和 `atc:Icon` 图标；没有图标时不会保留前置空白。

依赖：WPF `Button`、主题资源、`DefaultControlFocusVisualStyle`；使用图标时依赖 `Icon.Icon`、`Icon.FontFamily` 和 `Icon.IconSize`。

```xml
<StackPanel Orientation="Horizontal">
    <jv:Button Content="Save" Command="{Binding SaveCommand}" />
    <jv:Button
        atc:Icon.Icon="&#xE611;"
        atc:Icon.IconSize="18"
        Content="Refresh"
        Command="{Binding RefreshCommand}" />
</StackPanel>
```

`NoBorderButtonStyle` 是可直接使用的无边框样式，适合标题栏等紧凑操作区。自定义普通按钮样式时，优先基于隐式类型样式 `{StaticResource {x:Type jv:Button}}`，避免与其他控件字典中的同名内部资源冲突。

### CardButton

`jv:CardButton` 继承 `jv:Button`，用于指标卡、快捷入口或带主数值的可点击卡片。

| 属性 | 效果 |
| --- | --- |
| `Title` | 卡片左上方标题，类型为 `object` |
| `Content` | 卡片主要内容或数值 |
| `MainColor` | 主要内容和图标颜色 |
| `atc:Icon.Icon` | 卡片右侧图标 |
| `atc:Icon.FontFamily` | 图标字体 |

```xml
<jv:CardButton
    Width="240"
    Title="Online Cameras"
    Content="12"
    MainColor="{DynamicResource Theme.Brush.Status.Success}"
    atc:Icon.Icon="&#xE66B;"
    Command="{Binding OpenCamerasCommand}" />
```

### ToggleButton

`jv:ToggleButton` 继承 WPF `ToggleButton`，提供矩形和圆形开关模板。默认隐式样式使用圆形模板。

| 属性 | 效果 |
| --- | --- |
| `IsChecked` | 标准可空选中状态 |
| `DisplayMode` | 形状状态属性；当前只由 `ExpanderButton` 样式的触发器读取 |
| `SwitchWidth` | 开关轨道宽度，默认 `40` |
| `SwitchHeight` | 开关轨道高度，默认 `20` |

```xml
<jv:ToggleButton
    Content="Auto exposure"
    IsChecked="{Binding AutoExposure, Mode=TwoWay}"
    SwitchWidth="44"
    SwitchHeight="22"
    Template="{StaticResource SwitchToggleButton_Radius}" />
```

普通开关需要显式选择 `SwitchToggleButton_Radius` 或 `SwitchToggleButton_Rect` 模板；仅设置 `DisplayMode` 不会替换普通开关的模板。`ExpanderButton` 是库内公开的箭头开关样式，`TreeMenu` 使用它显示展开按钮。

### RadioButton

`jv:RadioButton` 继承 WPF `RadioButton`，支持标准分组、命令和双向选中绑定。

| 属性 | 效果 |
| --- | --- |
| `DisplayMode="Circular"` | 圆形单选标记，默认模式 |
| `DisplayMode="Rectangular"` | 方形单选标记 |

```xml
<StackPanel>
    <jv:RadioButton GroupName="Mode" Content="Automatic" IsChecked="True" />
    <jv:RadioButton GroupName="Mode" Content="Manual" DisplayMode="Rectangular" />
</StackPanel>
```

## 输入与选择控件

### CheckBox

`jv:CheckBox` 继承 WPF `CheckBox`，保留 `IsChecked`、三态和命令行为，使用内置图标字体绘制勾选标记。

依赖：主题资源、焦点样式、`atc:Icon.FontFamily`。该附加属性只影响勾选符号字体。

```xml
<jv:CheckBox Content="Enable inspection" IsChecked="{Binding InspectionEnabled, Mode=TwoWay}" />
```

### TextBox

`jv:TextBox` 继承 WPF `TextBox`，提供占位文本、前置图标和清空按钮。点击清空按钮会调用 `Clear()` 并重新聚焦输入框。

| 属性/附加属性 | 效果 |
| --- | --- |
| `Tag` | 默认模板把 `Tag` 当作占位文本；文本为空且未聚焦时显示 |
| `atc:Icon.Icon` | 前置图标；为空时图标区域折叠 |
| `atc:Icon.FontFamily` | 图标和清空按钮字体 |
| `atc:AttachFuc.IsClosable` | 是否显示清空按钮，默认样式为 `true` |

```xml
<jv:TextBox
    Width="260"
    atc:Icon.Icon="&#xE60C;"
    atc:AttachFuc.IsClosable="True"
    Tag="Camera name"
    Text="{Binding CameraName, UpdateSourceTrigger=PropertyChanged}" />
```

### ComboBox 与 ComboBoxItem

`jv:ComboBox` 继承 WPF `ComboBox`，支持标准 `ItemsSource`、`ItemTemplate`、可编辑模式、键盘操作和选择绑定。`jv:ComboBoxItem` 是对应的公开容器类型；绑定数据时通常不需要手动创建它。

额外属性 `PlaceHolder` 会在没有选中项时显示占位文本。

```xml
<jv:ComboBox
    Width="220"
    DisplayMemberPath="Name"
    ItemsSource="{Binding Cameras}"
    PlaceHolder="Select a camera..."
    SelectedItem="{Binding SelectedCamera, Mode=TwoWay}" />
```

也可以直接声明项目：

```xml
<jv:ComboBox PlaceHolder="Select mode...">
    <jv:ComboBoxItem Content="Continuous" />
    <jv:ComboBoxItem Content="Trigger" />
</jv:ComboBox>
```

## 集合与数据控件

### ListBox

`jv:ListBox` 继承 WPF `ListBox`，提供统一的悬停、选中、焦点和禁用状态，并默认启用 UI 虚拟化和回收模式。

依赖：标准 `ItemsSource`、`ItemTemplate` 和 `ListBoxItem` 容器，无额外附加属性。

```xml
<jv:ListBox ItemsSource="{Binding Devices}" SelectedItem="{Binding SelectedDevice, Mode=TwoWay}">
    <jv:ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}" />
        </DataTemplate>
    </jv:ListBox.ItemTemplate>
</jv:ListBox>
```

### ListView

`jv:ListView` 继承 WPF `ListView`，同时支持普通列表和标准 `GridView`。控件保留 WPF 的 `View` 管线，可以正常使用 `GridViewColumn.DisplayMemberBinding`、单元格模板和自定义 `ItemTemplate`。

```xml
<jv:ListView ItemsSource="{Binding Devices}">
    <jv:ListView.View>
        <GridView>
            <GridViewColumn Header="Name" DisplayMemberBinding="{Binding Name}" />
            <GridViewColumn Header="Status" DisplayMemberBinding="{Binding Status}" />
        </GridView>
    </jv:ListView.View>
</jv:ListView>
```

依赖：WPF `ListView`/`GridView`、虚拟化面板和主题滚动条，无额外附加属性。

### DataGrid

`jv:DataGrid` 继承 WPF `DataGrid`，统一列标题、单元格、行悬停和选中样式。默认启用行列虚拟化，关闭新增行、删除行和行高调整，并使用整行单选。

```xml
<jv:DataGrid AutoGenerateColumns="False" ItemsSource="{Binding InspectionResults}">
    <jv:DataGrid.Columns>
        <DataGridTextColumn Header="Time" Binding="{Binding Time}" />
        <DataGridTextColumn Header="Result" Binding="{Binding Result}" />
    </jv:DataGrid.Columns>
</jv:DataGrid>
```

依赖：WPF `DataGrid` 的标准列类型、排序、编辑和绑定机制，无额外附加属性。

## 文本与状态控件

### Label

`jv:Label` 继承 WPF `Label`，用于状态标签和带图标的提示文本。

| `DisplayMode` | 效果 |
| --- | --- |
| `0` | 错误色块标签 |
| `1` | 成功色块标签 |
| `-1` | 警告色块标签 |
| `10` | 无边框 Error 提示 |
| `-11` | 无边框 Warning 提示 |
| `11` | 无边框 Notice 提示 |
| `100` | 默认强调色标签 |

`DisplayMode=0`、`1`、`-1` 使用模板内置的固定状态图标；`10`、`-11`、`11` 使用各自固定的无边框提示图标。`DisplayMode=100` 的默认模板才读取 `atc:Icon.Icon` 和 `atc:Icon.FontFamily`，图标为空时会折叠图标区域。

```xml
<StackPanel Orientation="Horizontal">
    <jv:Label Content="Connected" DisplayMode="1" />
    <jv:Label Content="Low exposure" DisplayMode="-1" />
    <jv:Label atc:Icon.Icon="&#xE651;" Content="Notice" DisplayMode="100" />
</StackPanel>
```

### TextTitle

`jv:TextTitle` 继承 WPF `ContentControl`，左侧显示 `Content`，右侧显示 `Title`，适合图标或图片加标题的组合。

```xml
<jv:TextTitle Title="Inspection Station" FontSize="20">
    <Image Width="32" Height="32" Source="/Resources;component/PNG/inspector.png" />
</jv:TextTitle>
```

依赖：标准 `Content`/`ContentTemplate` 和 `Title` 依赖属性，无专用附加属性。

## 菜单与导航控件

### ContextMenu 与 ContextMenuItem

`jv:ContextMenu` 继承 WPF `ContextMenu`，保留命令、键盘导航、复选状态、快捷键文本、分隔线和多级子菜单。一级和所有子菜单使用相同主题，高亮不会回退到系统蓝色。当前模板固定保留一列 24px 的图标/勾选对齐区域；没有图标时图标内容会折叠，但该对齐列仍存在。

`jv:ContextMenuItem` 继承 WPF `MenuItem`，使用标准的 `Header`、`Icon`、`Command`、`CommandParameter`、`InputGestureText`、`IsCheckable` 和子项集合。

```xml
<jv:Button Content="Actions">
    <jv:Button.ContextMenu>
        <jv:ContextMenu>
            <jv:ContextMenuItem Header="Open" Icon="&#xE60F;" Command="{Binding OpenCommand}" InputGestureText="Ctrl+O" />
            <Separator />
            <jv:ContextMenuItem Header="Export">
                <jv:ContextMenuItem Header="PNG" Command="{Binding ExportPngCommand}" />
                <jv:ContextMenuItem Header="JPEG" Command="{Binding ExportJpegCommand}" />
            </jv:ContextMenuItem>
        </jv:ContextMenu>
    </jv:Button.ContextMenu>
</jv:Button>
```

也可以在 `jv:ContextMenu` 内使用原生 `<MenuItem>`；默认样式会统一应用到子菜单。不要在上下文菜单中使用 `<jv:MenuItem>`，因为它是 `SideMenu`/`TreeMenu` 的导航数据控件，不是 WPF 菜单项。

`ItemsSource` 仍按 WPF 标准使用。绑定普通数据时通过 `ItemContainerStyle` 设置 `Header`、`Icon` 和 `Command`：

```xml
<jv:ContextMenu ItemsSource="{Binding Actions}">
    <jv:ContextMenu.ItemContainerStyle>
        <Style BasedOn="{StaticResource JunevyContextMenuItemStyle}" TargetType="{x:Type MenuItem}">
            <Setter Property="Header" Value="{Binding Title}" />
            <Setter Property="Icon" Value="{Binding Icon}" />
            <Setter Property="Command" Value="{Binding Command}" />
        </Style>
    </jv:ContextMenu.ItemContainerStyle>
</jv:ContextMenu>
```

### MenuItem

`jv:MenuItem` 是导航数据控件，继承 `ContentControl`，供 `SideMenu` 和 `TreeMenuItem` 使用。它与 WPF `MenuItem` 没有继承关系。

| 属性 | 效果 |
| --- | --- |
| `Title` | 导航标题 |
| `Icon` | 图标字体字符或任意内容 |
| `Orientation` | 图标与标题的排列方向 |
| `TargetType` | 可由应用保存目标页面或视图类型；控件库不会自动创建该类型 |
| `Id` | 每个实例自动生成的只读 `Guid` |

直接作为 `SideMenu` 的导航数据使用：

```xml
<jv:SideMenu>
    <jv:MenuItem Title="Home" Icon="&#xE65D;" />
    <jv:MenuItem Title="Settings" Icon="&#xE60F;" />
</jv:SideMenu>
```

### SideMenu

`jv:SideMenu` 继承 `jv:ListBox`，适合应用侧边导航。它使用选择机制而不是按钮命令，通常绑定 `SelectedItem` 后由 ViewModel 完成导航。

| 属性 | 效果 |
| --- | --- |
| `Orientation` | 菜单项面板方向，默认 `Vertical` |
| `DisplayMode="Horizontal"` | 图标与标题横向排列 |
| `DisplayMode="Vertical"` | 紧凑图标模式，默认宽度调整为 `60` |
| `ItemHeight` | 固定项目高度；默认 `NaN`，使用内容自然高度 |
| `atc:Icon.FontFamily` | 所有菜单项图标字体 |
| `atc:Icon.IconSize` | 所有菜单项图标尺寸 |

```xml
<jv:SideMenu
    Width="180"
    atc:Icon.IconSize="20"
    DisplayMode="Horizontal"
    ItemHeight="44"
    ItemsSource="{Binding NavigationItems}"
    SelectedItem="{Binding SelectedNavigationItem, Mode=TwoWay}" />
```

`NavigationItems` 可以是包含 `Title` 和 `Icon` 属性的普通 ViewModel 集合，也可以是 `jv:MenuItem` 集合。

### TreeMenu 与 TreeMenuItem

`jv:TreeMenu` 继承 WPF `TreeView`，`jv:TreeMenuItem` 继承导航 `jv:MenuItem` 并增加子节点集合。

| 属性 | 效果 |
| --- | --- |
| `TreeMenu.DisplayMode` | `Normal` 显示展开箭头；`Icon` 使用左侧层级指示器 |
| `TreeMenu.NavigateCommand` | 激活叶节点时执行（双击或按 `Enter`），参数为叶节点 `TreeMenuItem` |
| `TreeMenuItem.Childrens` | 子节点集合，构造时自动初始化 |
| `TreeMenuItem.IsLeaf` | 根据 `Childrens` 是否为空计算的只读状态 |
| `atc:Icon.FontFamily` | 节点图标字体 |
| `atc:Icon.IconSize` | `Icon` 模式的节点图标大小 |
| `atc:ExpanderBehavior.Enable` | 默认容器样式已启用；控制双击/`Enter` 展开与激活导航 |

```csharp
public ObservableCollection<TreeMenuItem> NavigationTree { get; } =
[
    new TreeMenuItem
    {
        Title = "Camera",
        Icon = "\uE66B",
        Childrens =
        {
            new TreeMenuItem { Title = "Live View", TargetType = typeof(LiveView) },
            new TreeMenuItem { Title = "Settings", TargetType = typeof(CameraSettings) }
        }
    }
];
```

```xml
<jv:TreeMenu
    atc:Icon.IconSize="18"
    DisplayMode="Normal"
    ItemsSource="{Binding NavigationTree}"
    NavigateCommand="{Binding NavigateCommand}" />
```

当前默认树模板和双击行为依赖 `TreeMenuItem`，因此树数据应使用该类型。

交互约定：

- 单击选中节点；双击文件夹节点切换展开/收起，双击叶节点触发 `NavigateCommand`。
- 键盘方向键沿用 WPF `TreeView` 原生行为：`↑`/`↓` 移动选择，`→`/`←` 展开/收起；`Enter` 激活叶节点或切换文件夹展开。
- 悬停、选中、禁用三种视觉状态使用主题色区分，并作用于整行；长列表自动显示垂直滚动条。

### TabMenu 与 TabMenuItem

`jv:TabMenu` 继承 WPF `TabControl`，`jv:TabMenuItem` 继承 WPF `TabItem`。它遵循标准的 `ItemsSource`、`ItemTemplate`、`ContentTemplate` 和容器生成规则；点击页签标题会切换对应内容。

| 属性/事件 | 效果 |
| --- | --- |
| `CanCloseLastTab` | 是否允许关闭最后一个页签，默认 `true` |
| `HeaderCornerRadius` | 页签头圆角 |
| `ContentCornerRadius` | 内容区域圆角 |
| `atc:AttachFuc.IsClosable` | 是否显示关闭按钮，默认样式为 `true` |
| `TabClosing` | 关闭前事件；设置 `TabCloseEventArgs.Cancel=true` 可取消 |
| `TabClosed` | 成功关闭后的事件 |
| `CloseTab(TabMenuItem)` | 通过代码关闭指定页签 |
| `TabMenuItem.Icon` | 页签图标 |
| `TabMenuItem.IsEditing` | 双击文字标题进入编辑时的只读状态 |

直接声明页签：

```xml
<jv:TabMenu CanCloseLastTab="False">
    <jv:TabMenuItem Header="Camera 1" Icon="&#xE66B;">
        <local:CameraView />
    </jv:TabMenuItem>
    <jv:TabMenuItem Header="Logs">
        <local:LogView />
    </jv:TabMenuItem>
</jv:TabMenu>
```

绑定普通数据集合时，标准 `ItemTemplate` 控制页签标题，`ContentTemplate` 控制选中项内容：

```xml
<jv:TabMenu ItemsSource="{Binding Editors}">
    <jv:TabMenu.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Title}" />
        </DataTemplate>
    </jv:TabMenu.ItemTemplate>
    <jv:TabMenu.ContentTemplate>
        <DataTemplate>
            <ContentPresenter Content="{Binding View}" />
        </DataTemplate>
    </jv:TabMenu.ContentTemplate>
</jv:TabMenu>
```

只有需要把 `Icon` 等容器属性绑定到 ViewModel 时，才派生默认容器样式：

```xml
<jv:TabMenu.ItemContainerStyle>
    <Style BasedOn="{StaticResource DefaultTabMenuItemStyle}" TargetType="{x:Type jv:TabMenuItem}">
        <Setter Property="Icon" Value="{Binding Icon}" />
    </Style>
</jv:TabMenu.ItemContainerStyle>
```

### ToolBar 与 ToolBarItem

`jv:ToolBar` 继承 WPF `ItemsControl`；`jv:ToolBarItem` 继承 WPF `Button`，因此保留 `Command`、`Click`、焦点、按下和禁用行为。工具栏既支持直接声明项目，也支持 `ItemsSource`。

| 属性 | 效果 |
| --- | --- |
| `ToolBar.Orientation` | 项目排列方向，默认 `Horizontal` |
| `ToolBarItem.Icon` | 按钮图标 |
| `ToolBarItem.DisplayOrientation` | 图标和 `Content` 的排列方向 |
| `ToolBar.Foreground` | 默认继承到所有未显式设置颜色的项目 |

直接声明：

```xml
<jv:ToolBar FontFamily="{DynamicResource IconFont}" FontSize="22" Foreground="Yellow">
    <jv:ToolBarItem Icon="&#xE60F;" Command="{Binding SettingsCommand}" ToolTip="Settings" />
    <jv:ToolBarItem Icon="&#xE611;" Command="{Binding SaveCommand}" ToolTip="Save" />
</jv:ToolBar>
```

绑定普通 ViewModel 集合时，`ToolBar` 自动生成 `ToolBarItem` 容器。使用 `ItemContainerStyle` 绑定按钮属性：

```xml
<jv:ToolBar
    FontFamily="{DynamicResource IconFont}"
    FontSize="22"
    Foreground="Yellow"
    ItemsSource="{Binding AppBarMenuItems}">
    <jv:ToolBar.ItemContainerStyle>
        <Style BasedOn="{StaticResource DefaultToolBarItemStyle}" TargetType="{x:Type jv:ToolBarItem}">
            <Setter Property="Command" Value="{Binding Command}" />
            <Setter Property="Icon" Value="{Binding Icon}" />
            <Setter Property="ToolTip" Value="{Binding Tooltip}" />
        </Style>
    </jv:ToolBar.ItemContainerStyle>
</jv:ToolBar>
```

`ItemTemplate` 只用于显示按钮的 `Content`，不要在其中再次创建 `ToolBarItem`。如果 `ItemsSource` 本身存放 `ToolBarItem`，WPF 会把它们视为现成容器并忽略 `ItemTemplate`，这是标准 `ItemsControl` 行为。

### Toolbox、ToolboxItem 与 ToolItem

`jv:Toolbox` 是一级悬浮工具箱，继承 WPF `ItemsControl`；`jv:ToolboxItem` 是分组触发器和 Popup 容器，继承 `HeaderedItemsControl`；`jv:ToolItem` 继承 WPF `Button`，保留标准 `Command`、`CommandParameter`、`Click`、焦点和禁用行为。任意时刻最多展开一个分组。

`Toolbox` 的公开属性和方法：

| 属性/方法 | 默认值 | 效果 |
| --- | --- | --- |
| `Orientation` | `Vertical` | 一级分组排列方向；也决定 `PopupPlacement=Auto` 的优先方向 |
| `OpenDelay` | `150 ms` | 指针停留在有效分组触发器上后打开 Popup 的延迟；不能为负值 |
| `CloseDelay` | `300 ms` | 指针同时离开触发器和 Popup 后关闭的延迟；不能为负值 |
| `PopupWidth` | `300` DIP | Popup 边框总宽度；必须为有限且大于 `0` 的值 |
| `ColumnCount` | `4` | Popup 中 `UniformGrid` 的固定列数；至少为 `1` |
| `PopupMaxHeight` | `480` DIP | Popup 请求的最大高度；必须为有限且大于 `0` 的值 |
| `PopupPlacement` | `Auto` | 位置偏好：`Auto`、`Right`、`Left`、`Bottom` 或 `Top` |
| `DragDataFormat` | `Junevy.Controls.Tool` | 子工具未单独指定格式时使用的 WPF 拖放数据格式；不能为空或空白 |
| `ActiveItem` | `null` | 当前打开的 `ToolboxItem`，只读 |
| `ClosePopup()` | - | 立即取消待处理的打开/关闭并关闭当前 Popup |

`ToolboxItem` 的公开属性：

| 属性 | 默认值 | 效果 |
| --- | --- | --- |
| `Icon` | `null` | 分组图标，可使用图标字体字符或任意对象 |
| `Title` | `null` | 分组标题，同时用于默认 ToolTip 和自动化名称 |
| `DisplayMode` | `IconOnly` | `IconOnly` 只显示图标；`IconAndTitle` 同时显示标题 |
| `IsOpen` | `false` | Popup 是否打开，只读 |

`ToolItem` 的公开属性：

| 属性 | 默认值 | 效果 |
| --- | --- | --- |
| `Icon` | `null` | 工具图标，可使用图标字体字符或任意对象 |
| `Title` | `null` | 工具标题；默认单行省略，并作为 ToolTip 和自动化名称 |
| `DisplayMode` | `IconAndTitle` | `IconOnly` 只显示图标；`IconAndTitle` 同时显示标题 |
| `IsDragEnabled` | `true` | 是否允许超过 WPF 系统拖动阈值后启动 Copy 拖放；启用且控件可用时默认鼠标指针为十字形 `Cross` |
| `DragData` | `null` | 拖放载荷；应为工具定义等业务数据，不要使用 `ToolItem` 或其他 UI 对象 |
| `DragDataFormat` | `null` | 单项拖放格式；未设置时继承所属 `Toolbox.DragDataFormat` |

`ToolboxItem` 和 `ToolItem` 的默认模板在图标与标题之间保留 5 DIP 间距。可在各自的 `ItemContainerStyle` 中使用 `atc:Icon.IconSize` 调整图标大小、使用 `atc:Icon.IconForeground` 设置图标颜色、使用 `Foreground` 设置标题颜色，并使用 `FontSize` 调整标题字号。两条颜色通道彼此独立。

库内提供两组紧凑样式，可将一级分组触发器由默认 48 DIP 等比缩小到约 40 DIP（图标、标题与间距同步缩小；Popup 内的 `ToolItem` 保持默认尺寸不变）：

- `CompactToolboxItemStyle`：紧凑的 `ToolboxItem` 项样式。
- `CompactToolboxStyle`：紧凑的 `Toolbox` 容器样式，减小内边距并默认使用紧凑项样式。

```xml
<jv:Toolbox Style="{StaticResource CompactToolboxStyle}" ... />
<!-- 或保持默认容器样式，仅替换分组项样式 -->
<jv:Toolbox ItemContainerStyle="{StaticResource CompactToolboxItemStyle}" ... />
```


一级 `ToolboxItem` 的 Hover 背景覆盖完整触发区域，并使用 `Theme.SmallCornerRadius`（默认 4 DIP）裁切圆角；该背景不属于图标内容，也不会改变图标大小或布局。

Popup 内的 `ToolItem` 在 `IsDragEnabled="True"` 且 `IsEnabled="True"` 时显示十字形鼠标指针，提示该项可以拖动到设计画布；关闭拖动或禁用工具项后会恢复系统默认指针。一级 `ToolboxItem` 仍使用默认指针，因为它负责展开和切换分组。

绑定普通数据集合时，`Toolbox` 自动为外层数据生成 `ToolboxItem`，`ToolboxItem` 自动为内层数据生成 `ToolItem`。使用两级 `ItemContainerStyle` 绑定分组和工具属性；普通内层数据对象还会成为所生成 `ToolItem` 的默认 `DragData`。显式提供 `ToolboxItem` 或 `ToolItem` 时，WPF 会直接使用该实例，调用方应自行设置其属性和 `DragData`，不要在 `ItemTemplate` 中再创建同类型容器。

```xml
<Window.Resources>
    <Style x:Key="ToolItemStyle"
           BasedOn="{StaticResource DefaultToolItemStyle}"
           TargetType="{x:Type jv:ToolItem}">
        <Setter Property="Icon" Value="{Binding Icon}" />
        <Setter Property="Title" Value="{Binding Title}" />
        <Setter Property="Command" Value="{Binding DataContext.PlaceToolCommand, RelativeSource={RelativeSource AncestorType=Window}}" />
        <Setter Property="CommandParameter" Value="{Binding}" />
    </Style>

    <Style x:Key="ToolboxItemStyle"
           BasedOn="{StaticResource DefaultToolboxItemStyle}"
           TargetType="{x:Type jv:ToolboxItem}">
        <Setter Property="Icon" Value="{Binding Icon}" />
        <Setter Property="Title" Value="{Binding Title}" />
        <Setter Property="ItemsSource" Value="{Binding Tools}" />
        <Setter Property="ItemContainerStyle" Value="{StaticResource ToolItemStyle}" />
    </Style>
</Window.Resources>

<jv:Toolbox
    ItemContainerStyle="{StaticResource ToolboxItemStyle}"
    ItemsSource="{Binding ToolGroups}" />
```

Popup 默认使用 300 DIP 总宽度和四列网格，水平滚动关闭，超出有效高度时垂直滚动。垂直工具箱的 `Auto` 定位顺序为右、左、下、上；水平工具箱为下、上、右、左。显式位置仍保留其余方向作为空间不足时的回退。控件按目标窗口所在显示器取得工作区并将物理像素转换为 DIP，有效最大高度为 `min(PopupMaxHeight, 当前显示器工作区高度 - 16 DIP)`；窗口移动、调整大小或跨越不同缩放比例的显示器时，已打开的 Popup 会重新定位。窗口失活、最小化或控件卸载时 Popup 会立即关闭。

拖放固定使用 `DragDropEffects.Copy`。默认格式是 `Junevy.Controls.Tool`，载荷是 `DragData`；启动拖动的鼠标手势不会再执行按钮 Click。Canvas 必须设置 `AllowDrop="True"`，并由消费方验证格式、读取业务数据和创建节点：

```csharp
private void Canvas_OnDrop(object sender, DragEventArgs e)
{
    const string format = "Junevy.Controls.Tool";
    if (sender is not Canvas canvas || !e.Data.GetDataPresent(format))
    {
        e.Effects = DragDropEffects.None;
        e.Handled = true;
        return;
    }

    object toolDefinition = e.Data.GetData(format);
    Point position = e.GetPosition(canvas);
    viewModel.AddTool(toolDefinition, position);
    e.Effects = DragDropEffects.Copy;
    e.Handled = true;
}
```

控件库只负责工具的展示、命令和拖放数据传递，不包含 Canvas 节点工厂、节点创建、连线、撤销重做或序列化；这些能力和具体坐标语义属于消费应用。

### AppBar

`jv:AppBar` 继承 WPF `ContentControl`，提供应用图标、标题、工具栏以及最小化、最大化/还原、关闭按钮。系统按钮通过 WPF `SystemCommands` 操作所在窗口。

| 属性/附加属性 | 效果 |
| --- | --- |
| `Content` | 应用标题或任意标题内容 |
| `ToolBar` | `jv:ToolBar` 实例 |
| `atc:Icon.Icon` | 左侧应用图标，可使用图标字体或 `Image` |
| `atc:Icon.FontFamily` | 应用图标和标题栏系统按钮字体 |
| `atc:Icon.IconSize` | 左侧应用图标区域大小 |
| `Foreground` | 标题和应用图标颜色；图片本身不受影响 |

```xml
<jv:AppBar
    Height="75"
    atc:Icon.FontFamily="{DynamicResource IconFont}"
    atc:Icon.IconSize="50"
    Content="Machine Automation System"
    Foreground="{DynamicResource Theme.Brush.Text.Secondary}">
    <atc:Icon.Icon>
        <Image Width="40" Height="40" Source="/Resources;component/PNG/inspector.png" />
    </atc:Icon.Icon>
    <jv:AppBar.ToolBar>
        <jv:ToolBar
            BorderBrush="Transparent"
            Foreground="{Binding Foreground, RelativeSource={RelativeSource AncestorType={x:Type jv:AppBar}}}">
            <jv:ToolBarItem Icon="&#xE60F;" Command="{Binding SettingsCommand}" />
        </jv:ToolBar>
    </jv:AppBar.ToolBar>
</jv:AppBar>
```

依赖：所在 `Window`、WPF `SystemCommands`、内置图标字体、`ToolBar` 和 `Button` 样式。自定义无边框窗口时仍需由应用配置 `WindowChrome`、`WindowStyle` 和拖动区域。

## 布局控件

### ExpanderPanel

`jv:ExpanderPanel` 继承 WPF `HeaderedContentControl`，提供可折叠的头部与内容区，支持平滑的展开/折叠过渡动画，并可通过模板重写、命令绑定与依赖属性绑定无缝集成到现有项目。

| 属性/事件/方法 | 默认值 | 效果 |
| --- | --- | --- |
| `Header` | `null` | 头部内容；点击头部切换展开/折叠 |
| `Content` | `null` | 内容区 |
| `IsExpanded` | `true` | 是否展开；支持双向绑定 |
| `ExpandDirection` | `Down` | 展开方向，与 WPF `Expander` 语义一致：`Down` 头部在上、内容向下展开；`Up` 头部在下、内容向上展开；`Left` 头部在右、内容向左展开；`Right` 头部在左、内容向右展开 |
| `AnimationDuration` | `200 ms` | 展开/折叠过渡动画时长；`Automatic` 或 `Forever` 视为无效，`0` 表示无过渡动画直接切换 |
| `ToggleCommand` | `null` | 状态切换时执行的命令；`CanExecute` 返回 `false` 时不会执行 |
| `CommandParameter` | `null` | 传给 `ToggleCommand` 的参数 |
| `Toggle()` | - | 切换展开/折叠状态 |
| `Expanded` | - | 展开时触发的冒泡路由事件；随状态切换立即触发，不等待动画结束 |
| `Collapsed` | - | 折叠时触发的冒泡路由事件；随状态切换立即触发，不等待动画结束 |

```xml
<jv:ExpanderPanel
    Header="Camera"
    ExpandDirection="Down"
    IsExpanded="{Binding CameraExpanded, Mode=TwoWay}"
    AnimationDuration="0:0:0.25">
    <Grid>
        <TextBlock Text="Camera settings" />
    </Grid>
</jv:ExpanderPanel>
```

键盘与无障碍：头部使用 `ToggleButton`，按 `Space` 切换，自动获得按钮角色、可访问名称与焦点视觉样式；控件自身通过 `ExpanderPanelAutomationPeer` 暴露 UIA `ExpandCollapse` 模式，辅助工具可以读取并切换展开状态。

展开/折叠动画基于 `LayoutTransform` 缩放：动画期间周围布局同步收缩，折叠完成后不留占位空间，也不会出现布局跳变。

## 通知控件

### MessageBar、MessageBarPresenter 与 MessageBarService

`jv:MessageBar` 是参考 WPF-UI `Snackbar` 的应用内通知条：以卡片形式滑入显示标题、正文和状态图标，支持超时自动关闭、手动关闭和显示/隐藏生命周期事件。`jv:MessageBarPresenter` 是通知条的宿主容器；`MessageBarService` 是静态服务，注册宿主后可在任意位置弹出通知。

| 属性/方法/事件 | 效果 |
| --- | --- |
| `Title` | 标题行，类型为 `object`；为 `null` 时不显示 |
| `Message` | 正文内容，类型为 `object` |
| `Appearance` | `Informational`、`Success`、`Warning`、`Danger`，对应主题状态色，决定左侧色条与图标颜色 |
| `Icon` | 图标内容；未显式设置时跟随 `Appearance` 使用内置图标字体字形，显式设置后不再被外观切换覆盖 |
| `IsShown` | 是否显示；设置后播放滑入/滑出动画，完全折叠时占位消失 |
| `Timeout` | 自动关闭时间，默认 2 秒；零或负值禁用自动关闭 |
| `CloseButtonEnabled` | 是否显示右上角关闭按钮，默认 `true` |
| `Show()` / `Hide()` | 显示或隐藏，等同设置 `IsShown` |
| `Opening` / `Closing` | 显示或隐藏之前触发；设置 `MessageBarCancelEventArgs.Cancel=true` 可取消 |
| `Opened` / `Closed` | 显示或隐藏动画完成后触发 |

`MessageBar` 的 `Visibility`、`Opacity` 和 `RenderTransform` 由控件自身随 `IsShown` 管理，请通过 `IsShown` 控制显示状态，不要直接设置这三个属性。

服务方法（`MessageBarService`）：

| 方法 | 效果 |
| --- | --- |
| `SetPresenter(presenter)` | 注册宿主容器；重复调用会替换，整个应用通常只需注册一次 |
| `Show(message)` | Informational 外观、无标题 |
| `Show(title, message)` | Informational 外观 |
| `Show(appearance, message)` / `Show(appearance, title, message)` | 指定外观 |
| `Show(appearance, title, message, timeout)` | 指定外观与超时；`timeout` 为零或负值时禁用自动关闭 |
| `Clear()` | 隐藏当前显示的通知 |

服务可在非 UI 线程调用，内部会自动调度到宿主所在的 UI 线程。宿主同一时间只承载一条通知，新通知会替换旧通知，被替换的通知停止自己的自动关闭计时。

在窗口底部放置宿主（覆盖在内容之上，不占用布局空间），并在代码中注册：

```xml
<Grid>
    <local:MainContent />

    <!--  覆盖在内容底部居中  -->
    <jv:MessageBarPresenter x:Name="NotificationPresenter" Margin="16,0,16,24" />
</Grid>
```

```csharp
public MainWindow()
{
    InitializeComponent();
    MessageBarService.SetPresenter(NotificationPresenter);
}

private void OnSaved()
{
    MessageBarService.Show(MessageBarAppearance.Success, "Saved", "Exposure settings saved.");
    MessageBarService.Show(MessageBarAppearance.Danger, "Device lost", "Camera 2 disconnected.", TimeSpan.FromSeconds(5));
}
```

也可以直接在布局中声明通知条，并用 `IsShown` 控制显隐：

```xml
<jv:MessageBar
    Title="Low exposure"
    Message="Scene brightness below target."
    Appearance="Warning"
    IsShown="True"
    Closing="OnMessageBarClosing" />
```

依赖：WPF `ContentControl`、`DispatcherTimer` 动画与计时、`jv:Button`（关闭按钮）、主题资源和内置图标字体；图标跟随 `atc:Icon.FontFamily` 与 `atc:Icon.IconSize`。

## 窗口控件

### DialogWindow

`jv:DialogWindow` 是通用对话框宿主窗口：无边框、圆角、投影、主题化标题栏（图标、标题、最小化/最大化/关闭按钮），颜色全部跟随 `Theme.Brush.*` 动态资源，支持运行时浅色/深色切换。标题栏支持拖动，边缘支持鼠标缩放；Esc 可关闭（宿主可通过 `Closing` 事件拦截）。标题会自动读取 `DataContext` 上的 `Title` 属性（例如实现 `IDialogAware` 的 ViewModel），并监听 `INotifyPropertyChanged` 同步刷新。

| 属性 | 效果 |
| --- | --- |
| `TitleBarHeight` | 标题栏高度，默认 `40`，同时是拖拽区高度 |
| `ShadowMargin` | 四周为投影保留的透明外边距，默认 `16`；缩放热区与它对齐 |
| `CornerRadius` | 窗口圆角，默认跟随 `Theme.ControlCornerRadius`，最大化时自动归零 |
| `ShowMinimizeButton` | 是否显示最小化按钮，默认 `false` |
| `ShowMaximizeButton` | 是否显示最大化按钮，默认 `false` |
| `ShowCloseButton` | 是否显示关闭按钮（同时控制 Esc），默认 `true` |

窗口内容与 `DataContext` 由宿主注入后显示在标题栏下方；内容会按窗口圆角裁剪，避免内容自带背景顶破圆角。`Padding` 由窗口内部在最大化时管理，请勿依赖。

在应用项目中接入 Prism 的 `IDialogService` 时，本库无需引用 Prism，派生一个窗口补上 `Result` 属性即可（Prism 8.x 在 `Prism.Services.Dialogs`，9.x 在 `Prism.Dialogs`）：

```csharp
public class PrismDialogWindow : Junevy.Controls.Controls.Dialog.DialogWindow, IDialogWindow
{
    public IDialogResult Result { get; set; }
}
```

注册并弹出（ViewModel 实现 `IDialogAware`，其 `Title` 会成为窗口标题）：

```csharp
// App.RegisterTypes
containerRegistry.RegisterDialogWindow<PrismDialogWindow>();
containerRegistry.RegisterDialog<DeviceSettingView, DeviceSettingViewModel>();

// 任意位置
_dialogService.ShowDialog(nameof(DeviceSettingView), parameters, result =>
{
    if (result.Result == ButtonResult.OK) { /* ... */ }
});
```

不使用 Prism 时也可以直接实例化：`new DialogWindow { Content = view }.ShowDialog()`。子类如需覆盖默认样式，请再执行一次 `DefaultStyleKeyProperty.OverrideMetadata(typeof(子类), new FrameworkPropertyMetadata(typeof(DialogWindow)))`。

依赖：WPF `Window`、`WindowChrome`、`SystemCommands`、`RectangleGeometry` 圆角裁剪、主题资源；不依赖任何第三方包。

## 图像控件

### ImageViewer

`jv:ImageViewer` 是用于工业图像或普通位图检查的查看器。

| 属性/操作 | 效果 |
| --- | --- |
| `Source` | 要显示的 `ImageSource` |
| `BackgroundImage` | 自定义背景图；为空时使用内置棋盘背景 |
| 鼠标滚轮 | 以鼠标位置为中心缩放，范围约为 `0.05x` 到 `64x` |
| 按住鼠标左键拖动 | 平移图像 |
| `FitToWindow()` | 按查看器尺寸等比适应并居中，要求 `Source` 是 `BitmapSource` |
| `ActualSize()` | 恢复 1:1 变换 |
| 右键菜单 | Fit to Window、Actual Size、保存 PNG、保存 BMP |

```xml
<jv:ImageViewer Width="800" Height="600" Source="{Binding CurrentFrame}" />
```

保存功能依赖 WPF `BitmapSource`、`BitmapEncoder` 和 Windows `SaveFileDialog`，不需要额外 NuGet 包。构造函数会创建默认右键菜单；如果应用重新设置 `ContextMenu`，默认图像命令将被替换。

## ItemsSource 使用约定

Junevy.Controls 遵循 WPF 的项目容器规则：

1. `ItemsSource` 为普通数据对象时，控件负责生成容器；用 `ItemTemplate` 控制内容显示，用 `ItemContainerStyle` 设置容器属性。
2. 集合元素已经是容器类型时，例如 `ToolboxItem`、`ToolItem`、`ToolBarItem`、`TabMenuItem` 或原生 `MenuItem`，WPF 会直接使用该实例，并可能忽略 `ItemTemplate`。
3. 不要在 `Toolbox.ItemTemplate`、`ToolboxItem.ItemTemplate`、`ToolBar.ItemTemplate` 或 `TabMenu.ItemTemplate` 中创建对应的容器类型，否则会形成嵌套容器。
4. `ContextMenu` 使用 WPF `MenuItem`/`jv:ContextMenuItem`；`jv:MenuItem` 仅用于导航控件。

## 控件索引

| 分类 | 控件 |
| --- | --- |
| 应用栏 | `AppBar` |
| 按钮 | `Button`、`CardButton`、`ToggleButton`、`RadioButton` |
| 输入/选择 | `CheckBox`、`TextBox`、`ComboBox`、`ComboBoxItem` |
| 集合/数据 | `ListBox`、`ListView`、`DataGrid` |
| 文本/状态 | `Label`、`TextTitle` |
| 通知 | `MessageBar`、`MessageBarPresenter`、`MessageBarService` |
| 窗口 | `DialogWindow` |
| 布局 | `ExpanderPanel` |
| 菜单/导航 | `ContextMenu`、`ContextMenuItem`、`MenuItem`、`SideMenu`、`TreeMenu`、`TreeMenuItem`、`TabMenu`、`TabMenuItem`、`ToolBar`、`ToolBarItem`、`Toolbox`、`ToolboxItem`、`ToolItem` |
| 图像 | `ImageViewer` |

## 控件依赖速查

这里的“依赖”指控件正常工作时所依赖的 WPF 基类、同库控件、主题资源或附加属性；全部控件都依赖 `Themes/Generic.xaml` 提供的默认主题样式。

| 控件 | 主要依赖 | 相关附加属性 |
| --- | --- | --- |
| `AppBar` | WPF `ContentControl`、所在 `Window`、`SystemCommands`、`Button`、`ToolBar` | `Icon.Icon`、`Icon.FontFamily`、`Icon.IconSize` |
| `Button` | WPF `Button`、焦点和主题资源 | `Icon.Icon`、`Icon.FontFamily`、`Icon.IconSize`、`Border.CornerRadius` |
| `CardButton` | `jv:Button`、主题资源 | `Icon.Icon`、`Icon.FontFamily`、`Border.CornerRadius` |
| `ToggleButton` | WPF `ToggleButton`、圆形/矩形模板 | `Border.CornerRadius` |
| `RadioButton` | WPF `RadioButton`、`ShapeMode`、焦点资源 | `Icon.FontFamily` 用于选中符号 |
| `CheckBox` | WPF `CheckBox`、焦点资源 | `Icon.FontFamily`、`Border.CornerRadius` |
| `TextBox` | WPF `TextBox`、`jv:Button` 清空按钮 | `Icon.Icon`、`Icon.FontFamily`、`AttachFuc.IsClosable`、`Border.CornerRadius` |
| `ComboBox` | WPF `ComboBox`、`ComboBoxItem`、`jv:ToggleButton` | `Border.CornerRadius` |
| `ComboBoxItem` | WPF `ComboBoxItem`、`DefaultComboBoxItemStyle` | 无 |
| `ListBox` | WPF `ListBox`、`ListBoxItem`、虚拟化和滚动资源 | 无 |
| `ListView` | WPF `ListView`、`GridView`、虚拟化和转换器 | `Border.CornerRadius` |
| `DataGrid` | WPF `DataGrid`、标准列/行/单元格容器、虚拟化 | 无 |
| `Label` | WPF `Label`、状态和图标资源 | `Icon.Icon`、`Icon.FontFamily`（仅相应模板） |
| `TextTitle` | WPF `ContentControl`、标准内容模板管线 | 无 |
| `MessageBar` | WPF `ContentControl`、`DispatcherTimer`、`jv:Button` 关闭按钮、主题资源 | `Icon.FontFamily`、`Icon.IconSize` |
| `MessageBarPresenter` | WPF `ContentControl`、承载 `MessageBar`，配合 `MessageBarService` | 无 |
| `DialogWindow` | WPF `Window`、`WindowChrome`、`SystemCommands`、主题资源（含阴影/圆角令牌） | 无 |
| `ContextMenu` | WPF `ContextMenu`、`MenuItem`、`Separator`、Popup/阴影资源 | 无 |
| `ContextMenuItem` | WPF `MenuItem`、`JunevyContextMenuItemStyle` | 无 |
| `MenuItem` | WPF `ContentControl`；作为 `SideMenu`/`TreeMenuItem` 的导航数据 | 无 |
| `SideMenu` | `jv:ListBox`、`ListBoxItem`、导航数据模板 | `Icon.FontFamily`、`Icon.IconSize` |
| `TreeMenu` | WPF `TreeView`、`TreeMenuItem`、`jv:ToggleButton` | `Icon.FontFamily`、`Icon.IconSize`、`ExpanderBehavior.Enable` |
| `TreeMenuItem` | `jv:MenuItem`、`ObservableCollection<TreeMenuItem>` | 通过所在 `TreeMenu` 使用图标附加属性 |
| `TabMenu` | WPF `TabControl`、`TabMenuItem`、`jv:TextBox`、`jv:Button` | `AttachFuc.IsClosable`、`Icon.FontFamily` |
| `TabMenuItem` | WPF `TabItem`、`DefaultTabMenuItemStyle`、`TabMenu.CloseTabCommand` | 继承所在 `TabMenu` 的相关附加属性 |
| `ToolBar` | WPF `ItemsControl`、`ToolBarItem`、虚拟化面板 | 无；图标由项目自身属性提供 |
| `ToolBarItem` | WPF `Button`、`DefaultToolBarItemStyle` | 无 |
| `Toolbox` | WPF `ItemsControl`、`ToolboxItem`、`Popup`、`UniformGrid`、当前显示器工作区 | `Icon.FontFamily`、`Icon.IconSize`、`Icon.IconForeground` 由分组和工具模板使用 |
| `ToolboxItem` | WPF `HeaderedItemsControl`、`ToolItem`、`DefaultToolboxItemStyle`、所属 `Toolbox` 的交互和布局参数 | `Icon.FontFamily`、`Icon.IconSize`、`Icon.IconForeground` |
| `ToolItem` | WPF `Button` 命令管线、`DefaultToolItemStyle`、WPF `DragDrop` | `Icon.FontFamily`、`Icon.IconSize`、`Icon.IconForeground` |
| `ImageViewer` | WPF `Image`、`MatrixTransform`、`BitmapSource`、`SaveFileDialog` | 无 |
| `ExpanderPanel` | WPF `HeaderedContentControl`、`ToggleButton`、`LayoutTransform` 过渡动画、主题资源 | 无 |

## 开发注意事项

- 主题相关值使用 `DynamicResource`，固定且不会切换的资源才使用 `StaticResource`。
- 自定义控件模板时保留 WPF 标准部件名称和内容管线，例如 `PART_ContentHost`、`PART_EditableTextBox`、`ItemsPresenter`、`ContentTemplate` 和 `ItemContainerStyle`。
- 图标字体字符通过 `Foreground` 着色；位图和固定填充的矢量图不会自动着色。
- 不要同时合并 `AppColors.Light.xaml` 与 `AppColors.Dark.xaml`，否则后合并的重复资源键会覆盖前者。
