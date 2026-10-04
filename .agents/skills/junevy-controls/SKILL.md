---
name: junevy-controls
description: Junevy.Controls WPF 自定义控件库的使用指南（NuGet 包 Junevy.Controls / 本仓库）。凡是在 XAML 或 C# 中引用 Junevy.Controls、出现 jv: 或 atc: 前缀、使用 jv:Button / jv:TreeView / jv:TabControl 等控件、做 WPF 主题切换、图标字体（iconfont）或本库附加属性，或在本仓库（含 Samples/Junevy.Controls.Showcase）内工作——即使用户没有点名控件库，也应加载本 skill。
---

# Junevy.Controls 使用指南

Junevy.Controls 是面向 WPF（net8.0-windows / net48）的自定义控件库：统一浅色/深色主题、内置图标字体、40+ 控件（按钮、输入、集合、导航、布局、通知、窗口、图像）。核心第三方依赖只有 AvalonEdit（仅 `jv:CodeEditor` 使用）；可选伴生包 `Junevy.Controls.CodeCompletion` 提供 Roslyn 智能感知。

本 skill 是决策层指南：接入方式、选型规则、高频陷阱。**每个控件的完整属性表和示例在 README.md 对应章节**（见文末阅读地图）——写非平凡 XAML 前先查对应章节，不要凭 WPF 经验猜本库行为。

## 1. 接入三步（缺任何一步都会「样式不生效」）

1. 引用 NuGet 包 `Junevy.Controls`（或项目引用本仓库）。
2. `App.xaml` 合并主题入口——**只合并这一个字典**：

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/Junevy.Controls;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

3. 窗口/页面声明命名空间：

```xml
xmlns:jv="github.com.junevy"
xmlns:atc="clr-namespace:Junevy.Controls.AttachedProperties;assembly=Junevy.Controls"
```

**宿主窗口的底色与文字颜色不在库管辖内**，必须自设，否则深色主题下仍是白底黑字：

```xml
<Window Background="{DynamicResource Theme.Brush.Background.App}"
        TextElement.Foreground="{DynamicResource Theme.Brush.Text.Primary}">
```

运行时切换主题（`ThemeManager` 会替换主题字典，不要再手动合并 `AppColors.Light/Dark.xaml`，两者同时合并会互相覆盖）：

```csharp
using Junevy.Controls.Themes;
ThemeManager.ApplyTheme(AppTheme.Dark);
ThemeManager.ToggleTheme();
```

## 2. 选型规则：官方写法还是 `jv:` 前缀

这是用本库最重要的决策。合并 Generic.xaml 后分三类：

**A. 官方写法自动生效**（无需前缀，外观+交互都被库样式接管）：
`Button`、`CheckBox`、`TextBox`、`ComboBox`、`ListBox`、`ListView`、`DataGrid`、`DatePicker`、`Slider`、`TreeView`、`TabControl`、`ToolTip`（任意元素的 ToolTip 属性自动主题化）。

**B. 必须 `jv:` 前缀**（样式触发器直接引用库自有依赖属性，官方实例拿不到）：
`RadioButton`、`ToggleButton`、`Label`、`TextBlock`、`ProgressBar`、`PasswordBox`。注意 `jv:TextBlock` 不是 WPF TextBlock——它继承 ContentControl（左侧 `Content`、右侧 `Text`），WPF 原生 TextBlock 不会被重新着色。

**C. 专有能力只在 `jv:` 实例上有**：即使外观被接管，以下能力官方实例没有——
`jv:ListBox`/`jv:ListView` 的 `Orientation="Horizontal"` 横向滑动；`jv:Slider` 的数值框（`ShowValueBox`/`ValueBoxSide`/`ValueFormatString`）；`jv:TabControl` 的关闭/重命名/`TabClosing` 事件；`jv:TreeView` 的 `ExpandAll()`/`CollapseAll()`；`jv:PasswordBox`（原生 PasswordBox 是密封类，无接管样式，而它 `Password` 依赖属性可双向绑定）。

扩展能力（占位符、标题、空态提示、分页、平滑滚动等）以**附加属性**形式提供，官方实例同样可用——见第 4 节。

## 3. 主题令牌：只绑 `Theme.Brush.*`

调色板是三层令牌模型，**控件与宿主只允许绑第三层角色画刷** `Theme.Brush.*`（如 `Theme.Brush.Text.Primary`、`Theme.Brush.Accent.Primary`、`Theme.Brush.Surface.Hover`）。不要用遗留镜像层 `Theme.Color.*`，不要写死色值，不要取 `Palette.*` 原语。

- 主题相关值一律 `DynamicResource`——用 `StaticResource` 会导致切主题后不刷新。固定不变的资源（如无）才用 `StaticResource`。
- 覆盖令牌只能**追加 MergedDictionary**。`Application.Current.Resources["键"] = 值` 这类本层条目会永久遮蔽主题字典，`ApplyTheme` 之后仍返回旧值。
- 自定义控件样式基于隐式类型样式派生：`BasedOn="{StaticResource {x:Type jv:Button}}"`。常用 keyed 样式：`NoBorderButtonStyle`（无边框按钮）、`CompactToolboxStyle`、`JunevyContextMenuItemStyle`、`DefaultToolBarItemStyle` 等。
- 两个颜色陷阱：`Theme.Brush.Text.OnAccent` 在深色主题下是**墨色不是白色**，状态色块上的文字走这个键、不要写死 `#FFFFFF`；浅色主题的 `Status.Warning` 是真黄（白卡上对比度仅 1.7:1），黄底上不要放白字或深灰小字，用「`Status.WarningSubtle` 淡底 + `Text.Primary` 深色前景」。
- 布局令牌：`Theme.ControlCornerRadius`（控件圆角）、`Theme.ControlPadding`、`Theme.PopupShadow`、`Theme.ButtonShadow`。

## 4. 附加属性速查（`atc:` 命名空间）

| 附加属性 | 用途 | 关键点 |
| --- | --- | --- |
| `atc:Icon.Icon` / `.FontFamily` / `.IconSize` / `.IconForeground` | 控件统一图标 | `Icon` 可为图标字体字符（如 `&#xE60F;`）、`Image`、`Path` 等任意对象；为空时图标区自动折叠。字体默认就是内置 iconfont |
| `atc:PlaceholderAssist.Placeholder` | TextBox / ComboBox 占位符 | 支持官方实例；`Tag` 不再被模板消费。`jv:ComboBox` 不设时默认 "Select an item..." |
| `atc:TitleAssist.Title` 系列 | 输入框外侧标题 | `TitlePlacement`（Top/Bottom/Left/Right）、`TitleWidth`（表单对齐）、`IsRequired`（必填标识） |
| `atc:DataGridAssist.EmptyText` | DataGrid 空态提示 | 官方实例可用 |
| `atc:DatePickerAssist.PlaceHolder` | DatePicker 占位符 | 官方实例可用 |
| `atc:TreeViewAssist.*` | TreeView 扩展 | `DisplayMode`（`Chevron`/`Indicator`）、`IndentSize`、`AutoExpandAncestors`、`NavigateCommand`——`jv:TreeView` 有同名实例属性，附加属性写法对官方 `<TreeView>` 同样生效 |
| `atc:ExpanderBehavior.Enable` | TreeViewItem 双击展开 / Enter 激活 | 默认容器样式已启用，通常无需手动设 |
| `atc:SmoothScrolling.IsEnabled` / `.Step` | 平滑滚轮 | 库内 ListBox/ListView/DataGrid/ComboBox/SideMenu 默认已开启；可设到任意 ScrollViewer |
| `atc:PagingAssist.PageSize` / `.CurrentPage` / `.Placement` | 客户端分页 | 设 `PageSize` 后 ListBox/ListView/DataGrid 模板内自动出现页码栏（`jv:DataPager`）。仅支持 `ItemsSource`；两个启用分页的控件不可共享同一数据源 |

**图标字体**：`{DynamicResource IconFont}`（线性，默认）/ `{DynamicResource IconFontFilled}`（面性），码点完全一致（67 个图标），同一字符换 FontFamily 即整套切换风格。图标字符着色走 `Foreground`；`Image` 和固定 `Fill` 的 `Path` 不会自动重新着色。

## 5. 控件清单与独有能力

| 分类 | 控件 → 独有能力 / 注意点 |
| --- | --- |
| 按钮 | `jv:Button`（`ShowShadow` 浮起阴影）；`jv:CardButton`（指标卡：`Title`/`MainColor`）；`jv:ToggleButton`（开关：`SwitchSize`，形状由模板 `SwitchToggleButton_Radius`（胶囊）/`SwitchToggleButton_Rect`（圆角矩形）决定，**没有** DisplayMode 形状开关）；`jv:RadioButton`（`DisplayMode` Circular/Rectangular） |
| 输入 | `jv:TextBox`（`ShowClear` 清空按钮；`ShowCommandButton`+`CommandButtonCommand` 内嵌命令按钮，与清空按钮互斥）；`jv:PasswordBox`（`Password` 可绑定、`RevealMode`、`IsError` 泛红）；`jv:ComboBox`（单击主体切换下拉）；`jv:GroupBox`（点标题折叠：`IsCollapsible`/`IsCollapsed`）；`DatePicker`（官方写法即可）；`jv:Slider`（数值框三属性） |
| 集合 | `jv:ListBox` / `jv:ListView`（`Orientation="Horizontal"` 横向带状滑动；ListView 设了 `View`/GridView 时该属性失效）；`DataGrid`（官方写法即可；默认关闭新增/删除行、整行单选）；`jv:DataPager`（独立分页栏，可与 PagingAssist 显式绑定） |
| 文本/状态 | `jv:Label`（状态标签：`DisplayMode` 枚举 Error/Success/Warning/Borderless*/Neutral）；`jv:TextBlock`（左 `Content` 右 `Text` 的标题组合，非 WPF TextBlock） |
| 菜单/导航 | `jv:ContextMenu` + `jv:ContextMenuItem`（上下文菜单用这两个）；**`jv:MenuItem` 是 SideMenu 的导航数据控件，不是 WPF MenuItem——不要放进 ContextMenu**；`jv:SideMenu`（侧边导航，继承 WPF ListBox，绑 `SelectedItem`）；`jv:TreeView` + `jv:TreeMenuItem`（见下）；`jv:TabControl`（页签多时横向滚动不换行、不支持 `TabStripPlacement` 四方向；圆角是附加属性 `jv:TabControl.HeaderCornerRadius`/`.ContentCornerRadius`，官方 TabControl 也能用；关闭流程走 `TabClosing` 路由事件，可 `e.Cancel=true`）；`jv:ToolBar`/`jv:ToolBarItem`；`jv:Toolbox`/`jv:ToolboxItem`/`jv:ToolItem`（悬浮工具箱 + 拖放，数据格式 `"Junevy.Controls.Tool"`） |
| 布局 | `jv:ExpanderPanel`（`DisplayMode` Classic/Card、`ExpandDirection` 四方向、`HeaderExtra` 扩展槽）；`jv:SidePanel`（浮层侧滑面板：`IsOpen`/`Side`，放入 Grid 不指定 Row/Column 自动跨满） |
| 通知 | `jv:Badge`（角标：`Count`/`MaxCount`/`IsDot`/`Corner`）；`jv:MessageBar` + `MessageBarService`（注册一次 `SetPresenter` 后任意处 `Show`，非 UI 线程可调；用 `IsShown` 控制显隐，不要直接设 Visibility/Opacity）；`ToolTip`（官方写法自动主题化） |
| 窗口/图像 | `jv:DialogWindow`（无边框对话框宿主，无默认宽高按内容收缩，Prism 场景派生补 `IDialogWindow`）；`jv:ImageViewer`（滚轮缩放、拖动平移、`FitToWindow()`/`ActualSize()`、右键保存）；`jv:AppBar`（无边框标题栏：`Mode` Default/MenuBar/Expandable，配合宿主 WindowChrome，系统按钮命令自动补齐）；`jv:InfoBar`（用户信息条 + 弹出菜单，`MenuContent` 可放任意面板） |

**TreeView 专项**（最容易用错）：
- 数据模型是 `jv:TreeMenuItem`（普通 POCO，`Title`/`Icon`/`Children`/`IsExpanded`/`IsSelected`，非控件）。整树展开/收起用实例方法 `NavTree.ExpandAll()` / `CollapseAll()`。
- 换自定义数据类型时，必须在 `ItemContainerStyle` 里把容器的 `IsExpanded`/`IsSelected` 双向绑定到模型，否则展开/选中状态读不回来。
- 「按路径选中」一个祖先从未展开过的深层节点：容器尚未生成，必须宿主先沿路径置 `IsExpanded = true` 再写 `IsSelected`（`AutoExpandAncestors` 只对容器已生成的节点生效）。
- 叶节点激活（双击/Enter）走 `NavigateCommand`，参数是叶节点数据对象；未挂命令时事件不会被吞。

## 6. ItemsSource 约定（WPF 标准规则，违反即嵌套容器）

1. 绑普通数据 → 容器由控件生成，用 `ItemTemplate` 控制内容、`ItemContainerStyle` 设容器属性。
2. 集合元素已是容器类型（`ToolboxItem`/`ToolItem`/`ToolBarItem`/`TabControlItem`/`MenuItem`）→ WPF 直接用该实例，可能忽略 `ItemTemplate`。
3. **不要**在这些控件的 `ItemTemplate` 里再创建同名容器类型；要绑容器属性（如 `Icon`、`Command`）用 `ItemContainerStyle`（基于 `DefaultToolItemStyle` 等 keyed 样式派生）。
4. `ContextMenu` 内用 `jv:ContextMenuItem` 或原生 `MenuItem`，永远不要用 `jv:MenuItem`。

## 7. 高频陷阱清单

- `Border.CornerRadius` 是借用 WPF Border 的附加写法：**逐实例 attribute 写法 `<jv:Button Border.CornerRadius="8">` 编译报错 MC3015**，必须用样式 Setter 或代码 `SetValue`。且开关（ToggleButton）不读它，形状由模板决定。
- `jv:CodeEditor` 的语法语言属性叫 `SyntaxLanguage`（不叫 `Language`，刻意避开 `FrameworkElement.Language`）。
- 深色主题下窗口仍是白底黑字 → 忘了给 Window 设 `Background`/`TextElement.Foreground`（见第 1 节）。
- 切主题后颜色不变 → 某处用了 `StaticResource` 绑主题令牌。
- 覆盖令牌后被「永久记住」→ 用了 `Resources[key] = value` 而不是追加字典。
- 图标不显示 → `atc:Icon.Icon` 为空时图标区整体折叠（这是设计行为）；或字符用了错误字体（需 `atc:Icon.FontFamily` 或默认 iconfont）。
- 分页不生效 → 数据是 XAML 直接声明子条目而非 `ItemsSource`；或两个控件共享了同一数据源。
- MessageBar 直接设 `Visibility`/`Opacity`/`RenderTransform` 失效 → 这三个属性由控件随 `IsShown` 管理。
- `TabControl` 写了 `DisplayMode`/`TabStripPlacement` 报错或无效 → 前者已在 3.2.0 删除（写 XAML 会编译失败），后者不支持四方向。
- 自定义模板时必须保留 WPF 标准部件名（`PART_ContentHost`、`PART_ScrollViewer`、`ItemsPresenter` 等）——本库多处行为（平滑滚动、滚轮折算）依赖 `PART_ScrollViewer` 命名。

## 8. 深入阅读地图（写代码前按需查阅）

README.md（仓库根目录；下游项目见 GitHub 仓库同名文件）约 2000 行，含每个控件的完整属性表 + 可运行示例，**用哪个控件先读它的章节**：

| 需要什么 | 看 README 哪一节 |
| --- | --- |
| 主题令牌全表（浅/深色值 + 用途） | 「主题」→「角色令牌全表」 |
| 某个附加属性的完整参数表 | 「附加属性」下同名小节（Icon / PagingAssist / ExpanderBehavior / SmoothScrolling / TitleAssist / PlaceholderAssist） |
| 某个控件的属性表与示例 | 「控件索引」列出的分类章节（如「按钮控件」「菜单与导航控件」） |
| TreeView 数据模型与行为边界 | 「TreeView 与 TreeMenuItem」+「ExpanderBehavior」 |
| TabControl 关闭/重命名/绑定写法 | 「TabControl 与 TabControlItem」 |
| 无边框窗口 + AppBar 搭配 | 「AppBar」→「与 WindowChrome 搭配」 |
| Toolbox 拖放消费端写法 | 「Toolbox、ToolboxItem 与 ToolItem」 |
| ItemsSource / 容器规则 | 「ItemsSource 使用约定」 |

其他入口：
- Showcase 示例程序（每个控件都有可运行的演示页 + XAML 源码块）：`dotnet run --project Samples/Junevy.Controls.Showcase`；页面源码在 `Samples/Junevy.Controls.Showcase/`，XAML 片段集中在 `ShowcaseSnippets.cs`。
- 版本行为变更查 `CHANGELOG.md`（每条含更新时间与影响面）；破坏性更名（如 `TreeMenu`→`TreeView`、`TabMenu`→`TabControl`）都有迁移说明。
- 在本仓库内开发控件（而非使用）：遵循根目录 `AGENTS.md` 的流程要求（更新 CHANGELOG/README、调色板经 `Tools/palette/` 工具链、图标经 `Tools/iconfont/` 重建，不手写 XAML 色值、不改 TTF）。
