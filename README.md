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

`jv` 包含控件库的全部公开控件。`atc` 用于 `Icon`、`TitleAssist`、`PlaceholderAssist`、`DataGridAssist`、`DatePickerAssist` 和 `ExpanderBehavior` 附加属性。

### 官方同名控件自动生效

合并 `Themes/Generic.xaml` 后，以下控件**无需 `jv:` 前缀**即可直接以官方写法使用并自动获得本库样式（通过 App 作用域隐式样式接管）：

| 官方写法 | 生效范围 | 说明 |
| --- | --- | --- |
| `<Button>` | 外观 + 交互 | 完整等效 |
| `<CheckBox>` | 外观 + 交互 | 完整等效 |
| `<TextBox>` | 外观 + 交互 | 占位符通过附加属性 `atc:PlaceholderAssist.Placeholder` 提供（不再使用 `Tag`）；清空按钮的 Click 处理在 `jv:TextBox` 中，原生实例上仅外观 |
| `<ComboBox>` | 外观 + 交互 | 占位符通过附加属性 `atc:PlaceholderAssist.Placeholder` 提供（`jv:ComboBox` 默认显示 "Select an item..."），主体单击切换折叠逻辑在 `jv:ComboBox` 中，原生实例走 WPF 原生行为 |
| `<ListBox>` / `<ListView>` | 外观 + 交互 | 外观完整等效；水平滑动（`Orientation`）为本库 `jv:` 实例专有属性，原生实例沿用 WPF 原生排列 |
| `<DataGrid>` | 外观 + 交互 | 完整等效（含专属模板）；空态提示通过附加属性 `atc:DataGridAssist.EmptyText` 提供，原生与 `jv:` 实例均可用 |
| `<DatePicker>` | 外观 + 交互 | 完整等效（含日历全套模板）；占位符通过附加属性 `atc:DatePickerAssist.PlaceHolder` 提供，原生实例可用 |
| `<Slider>` | 外观 + 交互 | 完整等效（轨道、滑块、分页、刻度、选择区段）；数值框（`ShowValueBox`/`ValueBoxSide`/`ValueFormatString`）为本库 `jv:Slider` 专有，原生实例上自动折叠 |
| `<ToolTip>` | 外观 | 完整等效，任意元素的 `ToolTip` 属性自动获得主题样式 |

以下控件因依赖自有依赖属性（样式触发器直接引用），**必须使用 `jv:` 前缀**：`RadioButton`、`ToggleButton`（`DisplayMode`/`SwitchSize`）、`Label`（`DisplayMode`）、`TextBlock`（`Text`/`TextAlignment`/`TextWrapping`）、`ProgressBar`（`ProgressText` 等）、`Slider`（`ShowValueBox`/`ValueBoxSide`/`ValueFormatString`，原生实例仅有外观）。

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

### ExpanderBehavior

`atc:ExpanderBehavior.Enable` 用于 `TreeViewItem`。启用后，双击非叶节点会展开或折叠；双击叶节点会调用最近的 `TreeMenu.NavigateCommand`，命令参数是对应的 `TreeMenuItem`。

`TreeMenu` 的默认容器样式已经自动启用该行为，通常不需要手动设置。

### TitleAssist

`atc:TitleAssist` 为 `TextBox` 和 `ComboBox` 在输入框外侧显示一个标题，提示该输入框的用途。标题内容为任意对象（`object`），可以直接设为 iconfont 字形文本。`TitleWidth` 可为标题区域指定固定宽度，用于表单式布局中输入框整列对齐。

| 附加属性 | 默认值 | 实际效果 |
| --- | --- | --- |
| `atc:TitleAssist.Title` | `null` | 标题内容；为 `null` 时不显示标题，也不占用布局空间 |
| `atc:TitleAssist.TitlePlacement` | `Top` | 标题位置：`Top` / `Bottom` / `Left` / `Right`，标题与输入框间距固定 4 DIP |
| `atc:TitleAssist.TitleWidth` | `NaN` | 标题区域固定宽度（DIP），四个方位统一生效；`NaN` 时自适应标题内容。表单式布局中统一设置后，不同长度的标题保持一致的标题—输入框间距，输入框整列对齐（`Left` 方位标题自动右对齐贴合输入框） |
| `atc:TitleAssist.TitleFontFamily` | `null` | 标题字体族；为 `null` 时继承控件自身字体。标题为 iconfont 字形时需设置为 iconfont |
| `atc:TitleAssist.TitleFontSize` | `NaN` | 标题字号；`NaN` 时继承控件自身字号 |
| `atc:TitleAssist.TitleForeground` | `null` | 标题颜色；为 `null` 时由默认样式提供主题次级文本色 |
| `atc:TitleAssist.TitleFontWeight` | `Normal` | 标题字重 |

标题同时支持 `jv:TextBox` / `jv:ComboBox` 与原生 `<TextBox>` / `<ComboBox>` 借用默认外观的场景（附加属性经模板绑定生效）。

```xml
<jv:TextBox
    Width="220"
    atc:TitleAssist.Title="Server IP"
    atc:TitleAssist.TitlePlacement="Left"
    atc:PlaceholderAssist.Placeholder="192.168.1.100" />

<!-- iconfont 标题 -->
<jv:ComboBox
    Width="220"
    atc:TitleAssist.Title="&#xE60F; Settings"
    atc:TitleAssist.TitleFontFamily="{DynamicResource IconFont}"
    atc:TitleAssist.TitleFontSize="16"
    atc:TitleAssist.TitleFontWeight="Bold"
    atc:TitleAssist.TitleForeground="OrangeRed" />

<!-- 固定宽度表单对齐：标题长短不一致时输入框仍整列对齐 -->
<jv:TextBox
    Width="260"
    atc:TitleAssist.Title="用户名"
    atc:TitleAssist.TitlePlacement="Left"
    atc:TitleAssist.TitleWidth="110" />
<jv:ComboBox
    Width="260"
    atc:TitleAssist.Title="电子邮箱地址"
    atc:TitleAssist.TitlePlacement="Left"
    atc:TitleAssist.TitleWidth="110" />
```

### PlaceholderAssist

`atc:PlaceholderAssist.Placeholder` 为 `TextBox` 和 `ComboBox` 在输入框内部显示占位内容，提示用户应输入或选择什么。内容为任意对象（`object`），可以直接设为 iconfont 字形文本（字体族跟随 `atc:Icon.FontFamily`）。

| 附加属性 | 默认值 | 实际效果 |
| --- | --- | --- |
| `atc:PlaceholderAssist.Placeholder` | `null` | 占位内容；仅在控件无值时显示——`TextBox` 为文本为空且未聚焦，`ComboBox` 为未选中项，有值后自动隐藏 |

占位符同时支持 `jv:TextBox` / `jv:ComboBox` 与原生 `<TextBox>` / `<ComboBox>` 借用默认外观的场景（附加属性经模板绑定生效）。`jv:ComboBox` 的隐式样式保留历史默认占位文案 "Select an item..."；`TextBox` 的占位符改由本附加属性提供，`Tag` 不再被模板消费、恢复普通用途。

```xml
<jv:TextBox
    Width="220"
    atc:PlaceholderAssist.Placeholder="Server IP" />

<!-- iconfont 占位符 -->
<jv:ComboBox
    Width="220"
    atc:PlaceholderAssist.Placeholder="&#xE60F; Device" />
```

### Border.CornerRadius

多个模板通过 WPF 的 `Border.CornerRadius` 依赖属性读取控件圆角，例如 `Button`、`TextBox`、`ComboBox`、`ToggleButton` 和 `ListView`：

```xml
<!--  经样式 Setter 设置（推荐）  -->
<Style x:Key="RoundButton" BasedOn="{StaticResource {x:Type jv:Button}}" TargetType="{x:Type jv:Button}">
    <Setter Property="Border.CornerRadius" Value="8" />
</Style>
<jv:Button Content="Run" Style="{StaticResource RoundButton}" />
```

这不是 Junevy 自定义附加属性，而是 WPF `Border` 的依赖属性附加写法。注意：逐实例 attribute 写法 `<jv:Button Border.CornerRadius="8" Content="Run" />` 会被 WPF 标记编译器拒绝（MC3015——`Border` 未提供附加属性的 Get/Set 访问器），请使用样式 Setter 或代码 `SetValue` 设置。

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
| `SwitchSize` | 开关整体高度，轨道宽度按 2:1 比例自动推导，默认 `20`，建议不小于 `12` |

开关采用「轨道 + 滑块」结构：滑块直径 = `SwitchSize` − 4（扣除左右边框与内边距），与轨道内壁严丝合缝；胶囊模板圆角 = `SwitchSize` / 2，矩形模板使用 `Theme.SmallCornerRadius`，滑块圆角恒比轨道圆角小 2 DIP（内缩量），任意尺寸下内外圆角都保持视觉吻合。切换时滑块以缓动动画滑动到对侧。

```xml
<jv:ToggleButton
    Content="Auto exposure"
    IsChecked="{Binding AutoExposure, Mode=TwoWay}"
    SwitchSize="22"
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

`jv:TextBox` 继承 WPF `TextBox`，提供占位文本、前置图标、清空按钮和内部命令按钮。点击清空按钮会调用 `Clear()` 并重新聚焦输入框；命令按钮用于在输入框内直接触发命令（如提交、检索）。

| 属性/附加属性 | 效果 |
| --- | --- |
| `atc:PlaceholderAssist.Placeholder` | 占位文本（支持 iconfont 字形），文本为空且未聚焦时显示；`Tag` 不再被模板消费 |
| `atc:Icon.Icon` | 前置图标；为空时图标区域折叠 |
| `atc:Icon.FontFamily` | 图标、清空按钮和占位符字体 |
| `ShowClear`（依赖属性） | 是否显示清空按钮，默认样式为 `true`。复用 TextBox 外观又不需要清空按钮的场景（如 `Slider` 数值框）会显式设为 `false`；原生 `<TextBox>` 借用默认外观时，可在样式内以 `txt:TextBox.ShowClear` 限定形式设置 |
| `ShowCommandButton`（依赖属性） | 是否显示内部命令按钮，默认 `false`。**与清空按钮互斥**：`ShowClear="True"`（清空按钮显示）时命令按钮强制隐藏，需组合 `ShowClear="False"` + `ShowCommandButton="True"` 使用 |
| `CommandButtonCommand`（依赖属性） | 命令按钮点击时执行的命令（ICommand），可绑定 ViewModel 命令；按钮可用性随命令 `CanExecute` 自动启停 |
| `CommandButtonCommandParameter`（依赖属性） | 传递给 `CommandButtonCommand` 的命令参数 |
| `CommandButtonContent`（依赖属性） | 命令按钮内容（文本或 iconfont 字形），字体族跟随 `atc:Icon.FontFamily`，字号/颜色继承控件自身取值；为 `null` 时显示空白占位，建议显式设置 |
| `atc:TitleAssist.Title` 系列（含 `TitleWidth` 固定宽度） | 在输入框外侧显示用途标题，位置可选 `Top`/`Bottom`/`Left`/`Right`，支持 iconfont 与自定义字体样式，详见 [TitleAssist](#titleassist) |

```xml
<jv:TextBox
    Width="260"
    atc:Icon.Icon="&#xE60C;"
    ShowClear="True"
    atc:PlaceholderAssist.Placeholder="Camera name"
    Text="{Binding CameraName, UpdateSourceTrigger=PropertyChanged}" />
```

命令按钮示例（与清空按钮互斥，需 `ShowClear="False"`）：

```xml
<jv:TextBox
    Width="260"
    ShowClear="False"
    ShowCommandButton="True"
    CommandButtonContent="&#xE611;"
    CommandButtonCommand="{Binding SearchCommand}"
    CommandButtonCommandParameter="{Binding SearchKeyword}"
    atc:PlaceholderAssist.Placeholder="Search camera" />
```

### ComboBox 与 ComboBoxItem

`jv:ComboBox` 继承 WPF `ComboBox`，支持标准 `ItemsSource`、`ItemTemplate`、可编辑模式、键盘操作和选择绑定。不可编辑时，单击主体区域与单击箭头按钮等效：展开未打开的下拉，再次单击则折叠。`jv:ComboBoxItem` 是对应的公开容器类型；绑定数据时通常不需要手动创建它。

占位符统一由附加属性 `atc:PlaceholderAssist.Placeholder` 提供（未选中项时显示；`jv:ComboBox` 未设置时默认显示 "Select an item..."），详见 [PlaceholderAssist](#placeholderassist)。`atc:TitleAssist.Title` 系列附加属性可在下拉框外侧显示用途标题，详见 [TitleAssist](#titleassist)。

```xml
<jv:ComboBox
    Width="220"
    DisplayMemberPath="Name"
    ItemsSource="{Binding Cameras}"
    atc:PlaceholderAssist.Placeholder="Select a camera..."
    SelectedItem="{Binding SelectedCamera, Mode=TwoWay}" />
```

也可以直接声明项目：

```xml
<jv:ComboBox atc:PlaceholderAssist.Placeholder="Select mode...">
    <jv:ComboBoxItem Content="Continuous" />
    <jv:ComboBoxItem Content="Trigger" />
</jv:ComboBox>
```

### GroupBox

`jv:GroupBox` 继承 WPF `GroupBox`，以卡片形式呈现标题与内容。**单击标题区域即可折叠/展开**：折叠后内容区域完全隐藏（`Collapsed`，不占布局空间），标题左侧箭头同步旋转指示状态。

| 属性 | 效果 |
| --- | --- |
| `IsCollapsible` | 是否允许单击标题折叠，默认 `true`；设为 `false` 后标题仅作展示，悬停无高亮 |
| `IsCollapsed` | 内容是否已折叠，默认支持双向绑定，可从代码或绑定控制展开/收起 |

标题可以是任意对象（`HeaderTemplate`/`HeaderTemplateSelector` 照常可用）；标题内的按钮、复选框等交互元素不受点击折叠影响，照常响应。

```xml
<jv:GroupBox Header="采集设置" IsCollapsed="{Binding IsAdvancedCollapsed}">
    <StackPanel>
        <jv:TextBox Width="200" />
        <jv:ComboBox Width="200" atc:PlaceholderAssist.Placeholder="Select mode...">
            <jv:ComboBoxItem Content="Continuous" />
        </jv:ComboBox>
    </StackPanel>
</jv:GroupBox>

<!-- 折叠状态由代码控制 -->
<jv:GroupBox Header="诊断日志" IsCollapsible="True" IsCollapsed="True">
    <TextBlock Text="已折叠的内容默认不可见" />
</jv:GroupBox>
```

依赖：标准 `Header`/`Content` 管线、主题资源，模板通过 `Border.CornerRadius` 读取圆角。

### DatePicker

`DatePicker` 为 WPF 官方控件的完整主题接管：输入框、下拉日历图标与 `Calendar` 弹层全部按官方模板部件契约实现（`PART_Root`/`PART_TextBox`/`PART_Button`/`PART_Popup`）。日历弹层由 `DatePicker` 内部创建的 `Calendar` 承载——其 `Style` 被官方代码绑定到 `DatePicker.CalendarStyle` 属性（绑定属显式赋值，会绕过隐式样式查找），因此弹层主题经由 `DatePicker` 样式中的 `CalendarStyle` Setter 注入，`Calendar` 内部再显式下发 `CalendarItemStyle`/`CalendarDayButtonStyle`/`CalendarButtonStyle`（弹层子树内隐式样式同样不生效）。视觉与库内一致：卡片输入框（悬停/聚焦/展开高亮、禁用态）、日历卡片带阴影、今日高亮与选中色、月/年视图导航。合并 `Themes/Generic.xaml` 后，原生写法 `<DatePicker>` 与 `jv:DatePicker`（库内派生类）均直接生效，外观一致。

```xml
<DatePicker SelectedDate="{Binding BeginDate}"
            atc:DatePickerAssist.PlaceHolder="选择开始日期" />
```

**占位符**：`atc:DatePickerAssist.PlaceHolder` 为附加属性（未选日期且文本为空时显示），官方原生实例同样支持，不设置则无占位文案。

依赖：WPF `DatePicker`/`Calendar` 标准行为（`SelectedDateFormat`、`FirstDayOfWeek`、`BlackoutDates` 等）、主题滚动条与阴影令牌；附加属性 `atc:DatePickerAssist.PlaceHolder`（占位符）。

### Slider

`jv:Slider` 继承 WPF `Slider`，完整保留官方的拖拽、轨道分页、方向键与 `Home`/`End`、刻度、选择区段行为，另在滑块的上/下/左/右任意一侧附加一个可手动键入数值的数值框（模板部件 `PART_ValueBox`，外观复用库内 `jv:TextBox`）。合并 `Themes/Generic.xaml` 后，原生写法 `<Slider>` 与 `jv:Slider` 外观完全一致，数值框属 `jv:Slider` 专有——原生实例上自动折叠且不占布局。视觉上滑块为 16px 直角方形握手，轨道与选择区段均为直角矩形（无圆角），悬停描边高亮、拖拽填充主色。

| 属性 | 默认值 | 效果 |
| --- | --- | --- |
| `ShowValueBox` | `true` | 是否显示数值框；`false` 时整体 `Collapsed`，轨道立即占满腾出的空间，运行时切换即时生效 |
| `ValueBoxSide` | `Right` | 数值框停靠侧：`Left`/`Top`/`Right`/`Bottom` 四选一，横竖滑块均可任选。主轴侧限宽 `120`、交叉轴侧限宽 `160` 限高 `28`，避免数值框把轨道挤扁 |
| `ValueFormatString` | `null` | 数值框的显示格式（如 `F1`、`0.00`、`p0`），仅影响显示；`null` 时按当前区域性直接输出数值 |

**键入与提交**：输入过程中不改值，回车或数值框失焦时提交。按当前区域性解析（`NumberStyles.Float \| AllowThousands`，千分位按分组解析而非小数点），越界自动夹取到 `Minimum`/`Maximum`，非法文本（空串、非数字、`NaN`、无穷）不改值并把显示还原为当前值。写回使用 `SetCurrentValue`，双向绑定不受影响；回车不置 `Handled`，宿主的默认按钮等行为保持不变。

**方向与官方部件**：纵向滑块沿用 WPF 官方默认方向——最小值在下、向上增大，`Slider.IsDirectionReversed` 仍可由使用方设置（模板不写 `PART_Track` 的任何属性，否则会顶掉 `Track` 自身对方向/范围/值的自动绑定）。`TickPlacement` 在纵向时 `TopLeft` 为左侧刻度、`BottomRight` 为右侧刻度，与官方主题一致；`IsSelectionRangeEnabled` 配合 `SelectionStart`/`SelectionEnd` 的选择区段画在轨道之上，宿主层 `IsHitTestVisible=False`，点击照常落到轨道分页。

```xml
<jv:Slider Minimum="0"
           Maximum="100"
           Value="{Binding Volume}"
           ShowValueBox="True"
           ValueBoxSide="Right"
           ValueFormatString="F0"
           TickPlacement="BottomRight"
           TickFrequency="10" />
```

依赖：WPF `Slider`/`Track`/`Thumb`/`RepeatButton`/`TickBar` 标准部件契约、`DefaultTextBoxStyle`（数值框）、主题令牌（`Theme.Brush.Accent.Primary`、`Theme.Brush.Accent.Secondary`、`Theme.Brush.Surface.Sunken`、`Theme.Brush.Border.Default`、`Theme.Brush.State.DisabledSurface`）与 `DefaultControlFocusVisualStyle`。

## 集合与数据控件

### ListBox

`jv:ListBox` 继承 WPF `ListBox`，提供统一的悬停、选中、焦点和禁用状态，并默认启用 UI 虚拟化和回收模式。项目既可按默认的竖向列表排列，也可通过 `Orientation` 切换为横向带状列表并沿水平方向滑动。

| 属性 | 默认值 | 效果 |
| --- | --- | --- |
| `Orientation` | `Vertical` | `Vertical`：项目自上而下排列，垂直滚动条按需显示、水平滚动条关闭；`Horizontal`：项目自左向右排列，水平滚动条按需显示、垂直滚动条关闭（水平滑动，鼠标滚轮同样横向滚动）。运行时修改立即生效，无需重建控件 |

依赖：标准 `ItemsSource`、`ItemTemplate` 和 `ListBoxItem` 容器；排列与滚动方向由 `Orientation` 驱动，无额外附加属性。

竖向列表（默认）：

```xml
<jv:ListBox ItemsSource="{Binding Devices}" SelectedItem="{Binding SelectedDevice, Mode=TwoWay}">
    <jv:ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}" />
        </DataTemplate>
    </jv:ListBox.ItemTemplate>
</jv:ListBox>
```

横向滑动（例如缩略图、相机列表等横向带状内容）：

```xml
<jv:ListBox Height="110" Orientation="Horizontal" ItemsSource="{Binding Thumbnails}">
    <jv:ListBox.ItemTemplate>
        <DataTemplate>
            <Image Width="120" Height="80" Source="{Binding Preview}" Stretch="Uniform" />
        </DataTemplate>
    </jv:ListBox.ItemTemplate>
</jv:ListBox>
```

横向模式实际把项目面板替换为横向 `VirtualizingStackPanel`，虚拟化与回收模式保持启用，条目数量很多时不会一次性实例化全部容器。项目高度默认撑满控件（容器 `VerticalAlignment` 为 `Stretch`），需要固定尺寸时在 `ItemContainerStyle` 中设置 `Height`、`Width` 或对齐方式。官方 `<ListBox>` 实例沿用 WPF 原生排列，仅外观被本库接管；使用水平滑动请使用 `jv:ListBox`。

横向模式下鼠标滚轮同样左右滚动。WPF 的 `ScrollViewer` 只把滚轮用于竖直滚动，竖向滚不动时不会自动退化为水平滚动，`jv:ListBox` 在竖向不可滚动时补上这一步折算，折算量与 WPF 竖向滚轮保持一致（逻辑滚动按 `SystemParameters.WheelScrollLines` 条项目滚动，像素滚动按一个行高折算）；条目模板内部自带滚动控件时（例如条目里还有 `ScrollViewer`）滚轮仍归内层控件。竖向列表、`Orientation` 行为以及原生 `<ListBox>` 完全沿用 WPF 原生滚轮行为。

自定义模板时请保留名为 `PART_ScrollViewer` 的 `ScrollViewer`（本库两个模板均如此命名），滚轮折算依赖该部件定位滚动宿主；缺少该部件时不会报错，只是退回 WPF 原生滚轮行为。

导航控件 `jv:SideMenu` 也有一个同名属性，但它继承的是 WPF `ListBox` 而非 `jv:ListBox`，其 `Orientation` 表示菜单项面板的排列方向，与本属性无关。

### ListView

`jv:ListView` 继承 WPF `ListView`，同时支持普通列表和标准 `GridView`。控件保留 WPF 的 `View` 管线，可以正常使用 `GridViewColumn.DisplayMemberBinding`、单元格模板和自定义 `ItemTemplate`。普通列表同样支持横向带状排列与水平滑动。

| 属性 | 默认值 | 效果 |
| --- | --- | --- |
| `Orientation` | `Vertical` | 排列与滚动方向：`Vertical` 竖向列表；`Horizontal` 项目自左向右排列并水平滑动。仅在未设置 `View` 的普通列表上生效 |

`Orientation="Horizontal"` 需要配合“没有 `View`”这一条件：列表一旦使用 `GridView`（或自定义视图），排列方向维持竖向，`Orientation` 不参与布局，以免破坏列布局与表头。因此同一条 `jv:ListView` 可以安全地在两种模式间复用。

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

```xml
<!-- 普通列表 + 水平滑动 -->
<jv:ListView Height="110" Orientation="Horizontal" ItemsSource="{Binding Devices}">
    <jv:ListView.ItemTemplate>
        <DataTemplate>
            <Border Width="140" Padding="8">
                <TextBlock Text="{Binding Name}" />
            </Border>
        </DataTemplate>
    </jv:ListView.ItemTemplate>
</jv:ListView>
```

依赖：WPF `ListView`/`GridView`、虚拟化面板和主题滚动条，无额外附加属性。

横向模式同样支持鼠标滚轮左右滚动，实现与 `jv:ListBox` 一致（横向且竖向不可滚动时按 `SystemParameters.WheelScrollLines` 折算）；使用 `GridView` 时列表维持竖向，滚轮行为与 WPF 原生一致。

### DataGrid

`jv:DataGrid` 继承 WPF `DataGrid`，提供专属控件模板（完整实现官方模板部件契约 `PART_ColumnHeadersPresenter` / `PART_RowsPresenter` / `PART_ScrollContentPresenter`）与库内统一的卡片式视觉：Sunken 列标题（悬停高亮、排序方向箭头）、透明单元格（行悬停与选中色直接透出、键盘焦点时底边切换为焦点色）、行悬停/选中高亮。默认启用行列虚拟化，关闭新增行、删除行和行高调整，并使用整行单选。合并 `Themes/Generic.xaml` 后，原生写法 `<DataGrid>` 直接生效，无需 `jv:` 前缀。

```xml
<jv:DataGrid AutoGenerateColumns="False"
             ItemsSource="{Binding InspectionResults}"
             atc:DataGridAssist.EmptyText="暂无检测结果">
    <jv:DataGrid.Columns>
        <DataGridTextColumn Header="Time" Binding="{Binding Time}" />
        <DataGridTextColumn Header="Result" Binding="{Binding Result}" />
    </jv:DataGrid.Columns>
</jv:DataGrid>
```

**空态提示**：`atc:DataGridAssist.EmptyText` 为附加属性，Items 为空且该文本非空时显示在内容区中央；官方原生实例同样支持。不设置则无空态提示。

依赖：WPF `DataGrid` 的标准列类型、排序、编辑和绑定机制，虚拟化面板与主题滚动条；附加属性 `atc:DataGridAssist.EmptyText`（空态提示）。

## 文本与状态控件

### Label

`jv:Label` 继承 WPF `Label`，用于状态标签和带图标的提示文本。`DisplayMode` 为枚举 `LabelDisplayMode`（历史魔数取值已映射为枚举成员，数值保持兼容）。

| `DisplayMode`（`LabelDisplayMode`） | 效果 |
| --- | --- |
| `Error`（`0`，默认） | 错误色块标签 |
| `Success`（`1`） | 成功色块标签 |
| `Warning`（`-1`） | 警告色块标签 |
| `BorderlessError`（`10`） | 无边框 Error 提示 |
| `BorderlessWarning`（`-11`） | 无边框 Warning 提示 |
| `BorderlessNotice`（`11`） | 无边框 Notice 提示 |
| `Neutral`（`100`） | 中性标签，背景跟随 `Background` |

标签内容始终由 `Content` 提供，样式不会改写。各模式通过样式触发器注入默认图标（`atc:Icon.Icon`），可用局部值覆盖；图标为空时折叠图标区域。所有模式均读取 `atc:Icon.Icon` 和 `atc:Icon.FontFamily`。

```xml
<StackPanel Orientation="Horizontal">
    <jv:Label Content="Connected" DisplayMode="Success" />
    <jv:Label Content="Low exposure" DisplayMode="Warning" />
    <jv:Label atc:Icon.Icon="&#xE651;" Content="Notice" DisplayMode="Neutral" />
</StackPanel>
```

### TextBlock

`jv:TextBlock`（原 `TextTitle`）继承 WPF `ContentControl`，左侧显示 `Content`，右侧显示 `Text`，适合图标或图片加标题的组合。作为纯显示控件，默认 `Focusable=False`、`IsTabStop=False`。

```xml
<jv:TextBlock Text="Inspection Station" FontSize="20">
    <Image Width="32" Height="32" Source="/Resources;component/PNG/inspector.png" />
</jv:TextBlock>
```

依赖：标准 `Content`/`ContentTemplate` 管线和 `Text`、`TextAlignment`、`TextWrapping` 依赖属性，无专用附加属性。标题文本超宽时以省略号截断（`TextTrimming`），可通过对齐/内边距属性覆盖模板默认值。

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

`jv:SideMenu` 继承 WPF `ListBox`（**不是** `jv:ListBox`：它只复用 WPF 的列表选择机制，模板与样式完全独立），适合应用侧边导航。它使用选择机制而不是按钮命令，通常绑定 `SelectedItem` 后由 ViewModel 完成导航。

| 属性 | 效果 |
| --- | --- |
| `Orientation` | 菜单项面板排列方向，默认 `Vertical`。这是 `jv:SideMenu` 自有的属性，与 `jv:ListBox` 的 `Orientation`（列表排列和滚动方向）含义不同、互不影响 |
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
| `IsClosable` | 是否显示关闭按钮，默认 `true` |
| `TabClosing` | 关闭前事件；设置 `TabCloseEventArgs.Cancel=true` 可取消 |
| `TabClosed` | 成功关闭后的事件 |
| `CloseTab(TabMenuItem)` | 通过代码关闭指定页签 |
| `TabMenuItem.Icon` | 页签图标 |
| `TabMenuItem.IsEditing` | 双击文字标题进入编辑时的只读状态 |
| `TabMenuItem.CanRename` | 是否允许双击标题重命名，默认 `true`；设为 `false` 后双击不再进入编辑态，编辑中被禁用会立即退出编辑并保留当前文本 |

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

从 Popup 内发起拖放的那一刻（移动超出系统拖拽阈值），弹出窗口立即收起：内容先置为 `Collapsed`（不渲染、不参与命中测试，落点不会被残留弹层拦截）再关闭弹层，拖拽全程保持隐藏；`ActiveItem` 同步清空，拖拽期间悬停触发器或点击不会重新展开，松开落点后可正常再次悬停/点击展开。

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

`jv:AppBar` 继承 WPF `ContentControl`，提供应用图标、标题以及最小化、最大化/还原、关闭按钮。系统按钮通过 WPF `SystemCommands` 操作所在窗口。库内提供两套模板，`ToolBar` 与 `Menu` 二选一：

| 模板资源键 | 布局 | 使用的内容属性 |
| --- | --- | --- |
| `DefaultAppBar`（默认） | 图标 + 标题 / 分隔线 / 工具栏，右侧系统按钮 | `ToolBar` |
| `MenuBarAppBar` | 单行：图标 + 应用名 + 菜单栏 + 弹性空白 + 系统按钮 | `Menu` |

| 属性/附加属性 | 效果 |
| --- | --- |
| `Content` | 应用标题或任意标题内容 |
| `ToolBar` | `jv:ToolBar` 实例；仅 `DefaultAppBar` 呈现 |
| `Menu` | WPF `Menu` 实例；仅 `MenuBarAppBar` 呈现 |
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

#### 菜单栏模板 `MenuBarAppBar`

菜单使用 WPF 原生 `Menu` / `MenuItem`，业务侧照常写菜单，外观由库内 `JunevyMenuBarStyle`（`Menu`）与 `JunevyMenuBarItemStyle`（顶层项）接管：顶层项横向排列、子菜单向下弹出（`Placement="Bottom"`、不画右箭头），二级及更深层沿用 `JunevyContextMenuItemStyle` 的右向弹出外观，与 `jv:ContextMenu` 完全一致。两个样式只作为 keyed 资源注册，并在模板作用域内注入，不会改变应用中其他原生 `Menu` 的外观。

```xml
<jv:AppBar
    Height="36"
    atc:Icon.Icon="&#xE60F;"
    atc:Icon.IconSize="16"
    Content="Junevy Controls"
    FontSize="13"
    Foreground="{DynamicResource Theme.Brush.Text.Primary}"
    Template="{StaticResource MenuBarAppBar}">
    <jv:AppBar.Menu>
        <Menu>
            <MenuItem Header="文件(_F)">
                <MenuItem Command="ApplicationCommands.New" Header="新建" InputGestureText="Ctrl+N" />
                <MenuItem Command="{Binding OpenCommand}" Header="打开…" InputGestureText="Ctrl+O" />
                <Separator />
                <MenuItem Header="导出">
                    <MenuItem Command="{Binding ExportPngCommand}" Header="导出为 PNG" />
                </MenuItem>
            </MenuItem>
            <MenuItem Header="编辑(_E)" ItemsSource="{Binding EditMenuItems}" />
            <MenuItem Header="视图(_V)">
                <MenuItem IsCheckable="True" Header="显示网格" />
            </MenuItem>
            <MenuItem Command="{Binding HelpCommand}" Header="帮助" />
        </Menu>
    </jv:AppBar.Menu>
</jv:AppBar>
```

`Content` 与顶层菜单头都启用了 `RecognizesAccessKey`，`文件(_F)` 这类写法可用 `Alt+F` 导航；无子菜单的顶层项（如「帮助」）直接执行自身 `Command`/`Click`。最大化/还原按钮的字形与命令由 `WindowState` 触发器切换，`Content`、`Command`、`ToolTip` 只能通过样式设置——在按钮上写本地值会压过触发器，使按钮固定在「最大化」。

#### 与 `WindowChrome` 搭配

模板本身不含窗口 chrome，无边框窗口由宿主 `Window` 配置。`CaptionHeight` 应与 `AppBar` 高度一致，标题栏内需要交互的区域（菜单栏与三个系统按钮）已在模板内标记 `WindowChrome.IsHitTestVisibleInChrome="True"`，其余区域（图标、应用名、弹性空白）保持可拖动，双击最大化由 `WindowChrome` 自动处理：

```xml
<Window
    Title="Junevy Controls"
    WindowStyle="None"
    UseLayoutRounding="True">
    <WindowChrome.WindowChrome>
        <WindowChrome
            CaptionHeight="36"
            CornerRadius="0"
            GlassFrameThickness="0"
            ResizeBorderThickness="6"
            UseAeroCaptionButtons="False" />
    </WindowChrome.WindowChrome>

    <!--  最大化时窗口边界会比工作区大出一圈，用 Margin 内缩补偿，避免标题栏被裁切。  -->
    <Border>
        <Border.Style>
            <Style TargetType="{x:Type Border}">
                <Setter Property="Margin" Value="0" />
                <Style.Triggers>
                    <DataTrigger Binding="{Binding WindowState, RelativeSource={RelativeSource AncestorType=Window}}" Value="Maximized">
                        <Setter Property="Margin" Value="7" />
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </Border.Style>
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="auto" />
                <RowDefinition Height="*" />
            </Grid.RowDefinitions>
            <!--  jv:AppBar 放在第 0 行  -->
        </Grid>
    </Border>
</Window>
```

标题栏右键系统菜单不属于控件库职责，需要宿主窗口自行处理，例如在 `AppBar` 区域的 `MouseRightButtonUp` 中调用 `SystemCommands.ShowSystemMenu(this, point)`。

**系统按钮的命令处理由 `AppBar` 自动补齐。** WPF 只声明了 `SystemCommands` 的 `Minimize/Maximize/Restore/Close` 四个路由命令，**不提供任何处理器**：命令找不到绑定时，`CommandSource` 会把按钮自动置灰。因此 `AppBar` 在套用模板时会检查所在 `Window`，为其中尚未注册的那几个命令补上标准绑定（执行时调用 `SystemCommands.MaximizeWindow(window)` 等；可用性按 `ResizeMode`/`WindowState` 判定，`ResizeMode=NoResize` 时最大化/还原按钮自动禁用，与系统语义一致）。规则：

- 宿主窗口已自行 `CommandBindings.Add(...)` 的那几个命令，`AppBar` 一律不覆盖，宿主可继续自定义或禁用对应按钮。
- 同一窗口内多个 `AppBar`、切换模板都只会注册一组绑定。
- 若希望完全自行接管，直接在窗口上注册四个绑定即可（官方 WindowChrome 示例写法）。

依赖：所在 `Window`、WPF `SystemCommands`（按钮命令绑定由 `AppBar` 自动补齐）、内置图标字体、`Button` 样式；`DefaultAppBar` 另依赖 `ToolBar`，`MenuBarAppBar` 另依赖 WPF `Menu`/`MenuItem`、`Separator` 与 `ContextMenu` 系列样式。自定义无边框窗口时仍需由应用配置 `WindowChrome`、`WindowStyle` 和拖动区域。

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

### SidePanel

`jv:SidePanel` 继承 WPF `ContentControl`，是作为浮层使用的侧滑面板：把 `IsOpen` 绑定到一个布尔值，值为 `true` 时面板从 `Side` 指定的边缘（左/右/上/下）以滑动 + 淡入动画滑出，叠加显示在兄弟内容上方；值为 `false` 时完全滑出可视区域，不占用任何布局空间。展开期间点击面板以外的区域、切换宿主窗口失焦或最小化都会自动收回（可用 `CloseOnOutsideClick` 关闭）。

推荐直接放入 `Grid`（不指定 `Row`/`Column`）：控件会自动跨满父 Grid 的所有列/行（不会覆盖使用者显式设置的 `Grid.ColumnSpan`/`Grid.RowSpan`），面板宽度与高度由其 `Content` 决定，可通过设置内容的 `Width`/`Height` 指定。

| 属性/事件/方法 | 默认值 | 效果 |
| --- | --- | --- |
| `IsOpen` | `false` | 是否滑出；支持双向绑定 |
| `Side` | `Left` | 滑出方向：`Left`/`Right` 垂直填满、水平停靠对应边缘；`Top`/`Bottom` 水平填满、垂直停靠对应边缘；运行时切换立即生效 |
| `AnimationDuration` | `250 ms` | 滑出/收回过渡动画时长；`Automatic` 或 `Forever` 视为无效，`0` 表示无过渡动画直接切换（事件仍会触发） |
| `IsBackdropEnabled` | `true` | 是否启用遮罩层；展开时在面板背后显示半透明遮罩 |
| `BackdropBrush` | 主题 `Theme.Brush.Overlay.Backdrop` | 遮罩层画刷 |
| `CloseOnOutsideClick` | `true` | 展开时是否启用"点击外部即收回"：面板本体以外的点击（在鼠标抬起时判定，与宿主的切换按钮命令不冲突）、宿主窗口失焦、宿主窗口最小化都会收回面板；置为 `false` 后收回完全由宿主通过 `IsOpen`/`Toggle()` 控制 |
| `Toggle()` | - | 切换滑出/收回状态 |
| `Opened` / `Closed` | - | 滑出/收回动画完成后触发的冒泡路由事件；动画时长为 `0` 时随状态切换立即触发 |

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>

    <!--  两列各自的业务内容  -->
    <ContentControl Grid.Column="0" Content="{Binding LeftView}" />
    <ContentControl Grid.Column="1" Content="{Binding RightView}" />

    <!--  SidePanel 不指定 Column：自动跨满整个 Grid，从左侧滑出  -->
    <jv:SidePanel IsOpen="{Binding IsPanelOpen, Mode=TwoWay}" Side="Left">
        <StackPanel Width="300" Margin="8">
            <TextBlock Text="侧滑面板内容" />
        </StackPanel>
    </jv:SidePanel>
</Grid>
```

实现说明：收起时面板内容通过 `TranslateTransform` 完全平移出父容器并被裁剪，同时以透明度 0 兜底，根 Grid 的 `Background` 为空保证鼠标命中测试穿透到下层内容；展开时遮罩层淡入，命中测试由遮罩与面板正常接管。面板表面使用主题 `Surface.Raised`、`Theme.PopupShadow` 阴影与主题圆角。滑出/收回动画采用 `SineEase`（`EaseInOut`）曲线：`250 ms` 内约 15 帧的采样下，`CubicEase` 的峰值速度达平均速度的 1.875 倍，中段单帧位移接近 68 DIP（约 85 物理像素）而首尾两帧几乎不动，观感上就是"起步一顿、中间一跳"；`SineEase` 的峰值/均值比为 π/2 ≈ 1.57，同帧数下峰值位移降到约 38 DIP 且首帧即有位移，实测帧间隔与硬件渲染档位（`RenderCapability.Tier=2`）均无变化，属于曲线分布而非性能问题。

自动收回（`CloseOnOutsideClick=true`）在宿主窗口上分两阶段完成：`AddHandler(Mouse.PreviewMouseDownEvent, …, handledEventsToo: true)` 只记录"本次按下起于面板本体之外、且当时已展开"，`AddHandler(Mouse.MouseUpEvent, …, handledEventsToo: true)` 再执行收回。抬起阶段按钮的 `Click` 已由 `ButtonBase` 触发完毕（它早于事件冒泡到窗口），因此「按钮 + `IsOpen` 双向绑定 + 命令把布尔值取反」这一最常见写法不会互相打架：命令已把面板收回时控件不再重复写值，按下时还是收起态（本次点击负责展开）时也不会刚展开就被同一次抬起收回去。接管已处理事件（`handledEventsToo`）保证兄弟控件把鼠标事件标记为 `Handled` 也不漏判，处理过程自身不设置 `Handled`，面板以外的控件照常响应。点击是否落在面板本体上按"祖先链命中 `PART_Content` 或按下点位于 `PART_Content` 范围内"判定，因此遮罩本身的点击属于"外部"，多个抽屉叠放时也不会把压在别人遮罩下的本体误判为外部点击；内/外结论以按下位置为准，按下后拖出或拖入本体不改变本次判定。面板内的 `ComboBox`、`ContextMenu` 等弹层内容位于独立的 `PopupRoot` 顶层窗口，宿主窗口级处理收不到其中的点击，故不会误收回。宿主窗口 `Deactivated` 与 `StateChanged`（最小化）同样触发收回，语义与 `Toolbox` 一致；收回通过 `SetCurrentValue` 写回 `IsOpen`，双向绑定时数据源同步更新。面板卸载或属性置为 `false` 时，宿主窗口上的两个鼠标句柄与失焦/状态句柄会被完整摘除。

## 通知控件

### Badge

`jv:Badge` 继承 WPF `ContentControl`，是类似手机应用右上角未读提醒的角标控件：包裹任意内容（按钮、图标、菜单项等），在内容的指定角落叠加一个小圆点或未读数角标。包裹层不参与焦点与 Tab 导航，角标本体关闭命中测试，点击穿透到被包裹的内容。

| 属性 | 默认值 | 效果 |
| --- | --- | --- |
| `Count` | `0` | 未读数；小于等于 0 时角标隐藏，大于 `MaxCount` 显示 `99+` 形式 |
| `MaxCount` | `99` | 数字显示上限 |
| `IsDot` | `false` | 纯圆点模式（固定 10x10，不显示数字）；显示/隐藏仍由 `Count` 控制 |
| `Corner` | `TopRight` | 停靠角落：`TopLeft` / `TopRight` / `BottomLeft` / `BottomRight`，运行时切换立即生效 |
| `OffsetX` / `OffsetY` | `0` | 在角落停靠位置基础上的像素级微调 |

```xml
<jv:Badge Count="{Binding UnreadCount}" Corner="TopRight">
    <jv:Button atc:Icon.Icon="&#xE60F;" Content="消息" />
</jv:Badge>

<!--  纯圆点："有更新"提醒  -->
<jv:Badge Count="{Binding HasUpdate, Converter={StaticResource BoolToCount}}" IsDot="True">
    <jv:Button Content="更新" />
</jv:Badge>
```

停靠说明：角标以自身中心对准内容（视觉边界）的角落，自动补偿内容自身的 `Margin`；包裹层按内容自然尺寸收紧（默认 Left/Top，可通过 `HorizontalContentAlignment`/`VerticalContentAlignment` 调整），父容器拉伸包裹层不会导致角标脱离内容角落。角标为 16x16 胶囊（`Status.Danger` 背景 + `Text.OnAccent` 文字），需要自定义外观时重写模板即可。

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

### ToolTip

`ToolTip` 为 WPF 官方控件的完整主题接管（无 `jv:` 派生类）：悬浮卡片式提示——`Surface.Overlay` 表面、细边框、主题圆角，系统级阴影由 `HasDropShadow` 提供。合并 `Themes/Generic.xaml` 后，任意元素的 `ToolTip` 属性自动获得该样式，无需 `jv:` 前缀。

```xml
<TextBlock Text="曝光增益"
           ToolTip="取值范围 1.0 - 16.0，调整后立即生效" />
```

依赖：WPF `ToolTip` 标准行为（`Placement`、`InitialShowDelay` 等），主题资源。

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

窗口没有默认宽高：启用 `SizeToContent = WidthAndHeight`，尺寸完全跟随注入的内容（如 UserControl）自动收缩或撑大，不会出现系统默认窗口尺寸留下的大片空白。需要固定尺寸时给内容控件设置显式 `Width`/`Height` 即可；该模式下窗口不可拖拽边缘缩放（WPF 官方契约，符合对话框语义），最大化时自动切换为手动尺寸以正常铺满屏幕。

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
| 输入/选择 | `CheckBox`、`TextBox`、`ComboBox`、`ComboBoxItem`、`DatePicker`、`Slider` |
| 集合/数据 | `ListBox`、`ListView`、`DataGrid` |
| 文本/状态 | `Label`、`TextBlock` |
| 通知 | `Badge`、`MessageBar`、`MessageBarPresenter`、`MessageBarService`、`ToolTip` |
| 窗口 | `DialogWindow` |
| 布局 | `ExpanderPanel`、`SidePanel`、`GroupBox` |
| 菜单/导航 | `ContextMenu`、`ContextMenuItem`、`MenuItem`、`SideMenu`、`TreeMenu`、`TreeMenuItem`、`TabMenu`、`TabMenuItem`、`ToolBar`、`ToolBarItem`、`Toolbox`、`ToolboxItem`、`ToolItem` |
| 图像 | `ImageViewer` |

## 控件依赖速查

这里的“依赖”指控件正常工作时所依赖的 WPF 基类、同库控件、主题资源或附加属性；全部控件都依赖 `Themes/Generic.xaml` 提供的默认主题样式。

| 控件 | 主要依赖 | 相关附加属性 |
| --- | --- | --- |
| `AppBar` | WPF `ContentControl`、所在 `Window`、`SystemCommands`（命令绑定由控件自动补齐）、`Button`；`DefaultAppBar` 用 `ToolBar`，`MenuBarAppBar` 用 WPF `Menu`/`MenuItem` 与 `JunevyMenuBarStyle`、`JunevyContextMenuItemStyle` | `Icon.Icon`、`Icon.FontFamily`、`Icon.IconSize` |
| `Button` | WPF `Button`、焦点和主题资源 | `Icon.Icon`、`Icon.FontFamily`、`Icon.IconSize`、`Border.CornerRadius` |
| `CardButton` | `jv:Button`、主题资源 | `Icon.Icon`、`Icon.FontFamily`、`Border.CornerRadius` |
| `ToggleButton` | WPF `ToggleButton`、圆形/矩形模板 | `Border.CornerRadius` |
| `RadioButton` | WPF `RadioButton`、`ShapeMode`、焦点资源 | `Icon.FontFamily` 用于选中符号 |
| `CheckBox` | WPF `CheckBox`、焦点资源 | `Icon.FontFamily`、`Border.CornerRadius` |
| `TextBox` | WPF `TextBox`、`jv:Button` 清空按钮、`jv:Button` 命令按钮 | `Icon.Icon`、`Icon.FontFamily`、`ShowClear`（依赖属性）、`ShowCommandButton`/`CommandButtonCommand`/`CommandButtonCommandParameter`/`CommandButtonContent`（依赖属性）、`PlaceholderAssist.Placeholder`、`TitleAssist.Title` 系列、`Border.CornerRadius` |
| `ComboBox` | WPF `ComboBox`、`ComboBoxItem`、`jv:ToggleButton` | `PlaceholderAssist.Placeholder`、`TitleAssist.Title` 系列、`Border.CornerRadius` |
| `ComboBoxItem` | WPF `ComboBoxItem`、`DefaultComboBoxItemStyle` | 无 |
| `ListBox` | WPF `ListBox`、`ListBoxItem`、虚拟化和滚动资源 | 无 |
| `ListView` | WPF `ListView`、`GridView`、虚拟化和转换器 | `Border.CornerRadius` |
| `DataGrid` | WPF `DataGrid`、标准列/行/单元格容器、虚拟化、主题滚动条 | `atc:DataGridAssist.EmptyText` |
| `DatePicker` | WPF `DatePicker`/`Calendar`、官方模板部件契约、主题阴影令牌 | `atc:DatePickerAssist.PlaceHolder` |
| `Slider` | WPF `Slider`/`Track`/`Thumb`/`RepeatButton`/`TickBar` 部件契约、`DefaultTextBoxStyle`（数值框）、主色与下沉面等主题令牌、`DefaultControlFocusVisualStyle` | `ShowClear`（数值框显式关闭清空按钮） |
| `ToolTip` | WPF `ToolTip`、主题资源 | 无 |
| `Label` | WPF `Label`、状态和图标资源 | `Icon.Icon`、`Icon.FontFamily`（仅相应模板） |
| `TextBlock` | WPF `ContentControl`、`ContentPresenter`、标准内容模板管线 | 无 |
| `Badge` | WPF `ContentControl`、`TranslateTransform` 停靠偏移、主题状态色令牌（`Theme.Brush.Status.Danger`、`Theme.Brush.Text.OnAccent`） | 无 |
| `MessageBar` | WPF `ContentControl`、`DispatcherTimer`、`jv:Button` 关闭按钮、主题资源 | `Icon.FontFamily`、`Icon.IconSize` |
| `MessageBarPresenter` | WPF `ContentControl`、承载 `MessageBar`，配合 `MessageBarService` | 无 |
| `DialogWindow` | WPF `Window`、`WindowChrome`、`SystemCommands`、主题资源（含阴影/圆角令牌） | 无 |
| `ContextMenu` | WPF `ContextMenu`、`MenuItem`、`Separator`、Popup/阴影资源 | 无 |
| `ContextMenuItem` | WPF `MenuItem`、`JunevyContextMenuItemStyle` | 无 |
| `MenuItem` | WPF `ContentControl`；作为 `SideMenu`/`TreeMenuItem` 的导航数据 | 无 |
| `SideMenu` | WPF `ListBox`、`ListBoxItem`、导航数据模板 | `Icon.FontFamily`、`Icon.IconSize` |
| `TreeMenu` | WPF `TreeView`、`TreeMenuItem`、`jv:ToggleButton` | `Icon.FontFamily`、`Icon.IconSize`、`ExpanderBehavior.Enable` |
| `TreeMenuItem` | `jv:MenuItem`、`ObservableCollection<TreeMenuItem>` | 通过所在 `TreeMenu` 使用图标附加属性 |
| `TabMenu` | WPF `TabControl`、`TabMenuItem`、`jv:TextBox`、`jv:Button` | `IsClosable`（控件自身属性）、`Icon.FontFamily` |
| `TabMenuItem` | WPF `TabItem`、`DefaultTabMenuItemStyle`、`TabMenu.CloseTabCommand` | 继承所在 `TabMenu` 的相关附加属性 |
| `ToolBar` | WPF `ItemsControl`、`ToolBarItem`、虚拟化面板 | 无；图标由项目自身属性提供 |
| `ToolBarItem` | WPF `Button`、`DefaultToolBarItemStyle` | 无 |
| `Toolbox` | WPF `ItemsControl`、`ToolboxItem`、`Popup`、`UniformGrid`、当前显示器工作区 | `Icon.FontFamily`、`Icon.IconSize`、`Icon.IconForeground` 由分组和工具模板使用 |
| `ToolboxItem` | WPF `HeaderedItemsControl`、`ToolItem`、`DefaultToolboxItemStyle`、所属 `Toolbox` 的交互和布局参数 | `Icon.FontFamily`、`Icon.IconSize`、`Icon.IconForeground` |
| `ToolItem` | WPF `Button` 命令管线、`DefaultToolItemStyle`、WPF `DragDrop` | `Icon.FontFamily`、`Icon.IconSize`、`Icon.IconForeground` |
| `ImageViewer` | WPF `Image`、`MatrixTransform`、`BitmapSource`、`SaveFileDialog` | 无 |
| `ExpanderPanel` | WPF `HeaderedContentControl`、`ToggleButton`、`LayoutTransform` 过渡动画、主题资源 | 无 |
| `SidePanel` | WPF `ContentControl`、`TranslateTransform` 滑动动画（`SineEase`）、遮罩与主题阴影令牌（`Theme.Brush.Overlay.Backdrop`、`Theme.PopupShadow`）、宿主窗口级点击/失焦/最小化自动收回 | 无 |
| `GroupBox` | WPF `GroupBox`、主题资源（卡片、悬停、状态令牌） | `Border.CornerRadius` |

## 示例程序（Showcase）

`Samples/Junevy.Controls.Showcase` 是使用本控件库开发的分类展示程序，运行：`dotnet run --project Samples/Junevy.Controls.Showcase`（net8.0-windows）。

- **主窗口**：无边框 + `WindowChrome` + `jv:AppBar`（默认模板 DefaultAppBar；一个程序仅一个 AppBar，此处展示默认状态，工具栏按钮演示 `ThemeManager` 主题切换与打开 `jv:SidePanel` 设置抽屉）、`jv:SideMenu` 分类导航、`MessageBarService` 通知宿主。
- **分类页**：按钮 / 输入与选择（PlaceholderAssist、TitleAssist、ShowClear、DatePickerAssist、Slider 数值框）/ 集合与数据（虚拟数据：ListBox 竖向+横向、ListView GridView、DataGrid、EmptyText 空态）/ 文本与状态（Label 全模式、TextBlock、ToolTip）/ 通知（Badge、MessageBar、MessageBarService）/ 布局（GroupBox、ExpanderPanel、Border.CornerRadius）/ 菜单与导航（ContextMenu、TreeMenu、TabMenu、ToolBar、Toolbox）/ 窗口与图像（ImageViewer、DialogWindow）。
- **注意**：`jv:Button` 默认 `IsTextScaled=True`（文字随按钮尺寸等比缩放），内容自适应布局中应设 `IsTextScaled="False"`；展示页经页面隐式样式统一关闭，并保留一个固定尺寸的缩放演示。

## 开发注意事项

- 主题相关值使用 `DynamicResource`，固定且不会切换的资源才使用 `StaticResource`。
- 自定义控件模板时保留 WPF 标准部件名称和内容管线，例如 `PART_ContentHost`、`PART_EditableTextBox`、`ItemsPresenter`、`ContentTemplate` 和 `ItemContainerStyle`。
- 图标字体字符通过 `Foreground` 着色；位图和固定填充的矢量图不会自动着色。
- 不要同时合并 `AppColors.Light.xaml` 与 `AppColors.Dark.xaml`，否则后合并的重复资源键会覆盖前者。
