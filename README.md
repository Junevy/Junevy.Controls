# Junevy.Controls

Junevy.Controls 是一个面向 WPF 桌面应用的自定义控件库，提供统一的浅色/深色主题、图标字体、焦点及禁用状态，以及常用的按钮、输入、导航、数据展示和图像查看控件。

## 环境与依赖

| 项目 | 说明 |
| --- | --- |
| 目标框架 | `.NET 8 WPF (net8.0-windows)`、`.NET Framework 4.8 WPF (net48)` |
| 平台 | Windows / WPF |
| NuGet 依赖 | [`AvalonEdit`](https://www.nuget.org/packages/AvalonEdit)（`jv:CodeEditor` 专用，MIT，**零传递依赖**，net48/net8.0-windows 均覆盖）；其余无第三方包。可选伴生包 [`Junevy.Controls.CodeCompletion`](https://www.nuget.org/packages/Junevy.Controls.CodeCompletion)（CodeEditor 系统类 IntelliSense，按需引入 Roslyn 依赖链） |
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

`jv` 包含控件库的全部公开控件。`atc` 用于 `Icon`、`TitleAssist`、`PlaceholderAssist`、`DataGridAssist`、`DatePickerAssist`、`TreeViewAssist`、`ExpanderBehavior` 和 `SmoothScrolling` 附加属性。

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
| `<TreeView>` | 外观 + 交互 | 完整等效（卡片容器、ExpanderPanel 同款旋转展开图标、悬停/选中态、层级缩进）；`DisplayMode`（`TreeViewDisplayMode`）/ `IndentSize` / `AutoExpandAncestors` / `NavigateCommand` / 悬停与选中画刷经 `atc:TreeViewAssist` 附加属性承载，原生实例同样可用（`jv:TreeView` 另提供同名实例属性）；整树 `ExpandAll()` / `CollapseAll()` 是本库实例方法，WPF 基类未提供，原生实例需逐节点递归；命令导航语义分宿主：`jv:TreeView` 为选中驱动 + `ItemDoubleClick` 事件，官方 `<TreeView>`（经 `atc:TreeViewAssist`）保留叶激活旧语义 |
| `<ToolTip>` | 外观 | 完整等效，任意元素的 `ToolTip` 属性自动获得主题样式 |

以下控件因依赖自有依赖属性（样式触发器直接引用），**必须使用 `jv:` 前缀**：`RadioButton`、`ToggleButton`（`SwitchSize`）、`Label`（`DisplayMode`）、`TextBlock`（`Text`/`TextAlignment`/`TextWrapping`）、`ProgressBar`（`ProgressText` 等）、`Slider`（`ShowValueBox`/`ValueBoxSide`/`ValueFormatString`，原生实例仅有外观）、`PasswordBox`（模板复合控件，原生 `PasswordBox` 为密封类不提供接管样式：`Password` 可绑定、`RevealMode`/`IsError` 等均为 `jv:` 实例属性）。

## 主题

`Themes/Generic.xaml` 会加载默认浅色主题、所有控件样式、滚动条、焦点样式和内置图标字体。主题相关颜色应使用 `DynamicResource`，这样运行时切换主题后现有控件可以同步刷新。

### 三层令牌模型（Aperture）

配色由 `Tools/palette/spec.js` 单一来源生成，改颜色请改 spec 再 `node Tools/palette/emit.js`，不要手写 XAML 里的色值。令牌分三层，**只有第三层可以对控件开放**：

| 层 | 键前缀 | 谁可以用 |
| --- | --- | --- |
| 原语（palette ramp） | `Palette.Chalk.600`、`Palette.Cobalt.500` … | 只在 `AppColors.*.xaml` 内部被引用，控件与宿主都不要直接取 |
| 兼容色值 | `Theme.Color.Text.Primary` … | 旧接口遗留的镜像层，与 `Palette.*` 同值；新代码用 `Theme.Brush.*` |
| 角色画刷 | `Theme.Brush.Text.Primary` … | 控件模板与宿主应用**唯一**该绑的一层 |

`Step` 数字不是权重，而是 CIE **L\***（感知亮度）：同一族里相邻 step 的色差在人类视觉上大致均匀，浅色族 `Chalk` 从 `0`（`#FFFFFF`）走到 `800`（`#24282E`），深色族 `Slate` 方向相反（1.14.0 手调后深色各阶按用途排布：`Slate.30` 承接深色画布，`Slate.50` 是悬停/按下/凹陷的统一台阶，不追求全族单调）。两套主题共用同一条中性线（色相仍恒定在 210°–215°，但彩度封顶 C\*≤7，实测峰值 6.5），只是各自从线的两端往里读——切换主题时画面不会换一个色相说话。**灰就是灰**：蓝只由 `Cobalt` / `Coating` 两个色族承担，纸面与墨面本身不再泛蓝。

设计取向：

- **浅色适合阅读**：阅读面（卡片 `Surface.Base`）是纯白 `#FFFFFF`——1.10.0 曾把卡片封在 `#FBFCFD`、被反馈「整体太灰」，现改为白卡片，正文对比 14.8:1；1.14.0 手调后画布（`Background.App`）同为纯白，「白卡浮于灰画布」的亮度衬托不再存在，层级全交给阴影与描边表达。**侧栏用次级背景**（`Background.Subtle` `#EBEDEF`，比画布深 ΔL\* 2.1）：「次级在左、主背景在右」的区域层级，参考主流工具类应用的侧栏观感；悬停色为 Chalk.50（`#F6F6F6`，比白卡深约 3 个 ΔL\*）。
- **深色不累眼**：正文 L\* 约 89 而非 93+（对深色画布 `#232323` 与卡片 `#1F2226` 均约 11.9:1，读数舒适但不发光）；1.14.0 手调后中性线整体去蓝改纯灰，悬停/按下/凹陷统一落在 `Slate.50`（`#2F2F2F`，比卡片亮约 6 个 ΔL\*），边框被刻意压低。
- **边框分五级，静止态「若有若无」**：`Border.Default` 是发丝线（浅色 1.35:1、深色 1.79:1；浅色 2026-10-02 手调减淡），只勾一层控件轮廓；`Border.Subtle` 同档（浅色 1.35:1、深色 1.15:1）做纯装饰分隔线；`Border.Divider` 是 **Shell 级区域分隔线**——侧栏 / 内容区这种「分区而非控件」的界线（浅色对画布 1.35:1、深色 1.13:1，对次级侧栏底 1.15:1 / 1.04:1）；侧栏铺次级背景后，分隔线对两侧仍保持可辨；`Border.Medium` 是中间强调档（浅色 3.10:1、深色对卡面 2.19:1，2026-10-02 新增），当前用于开关未选中滑块。需要被清楚看见的状态才升到 3:1 以上——输入类控件悬停描边用 `Border.Strong`（浅色 5.07:1、深色 3.19:1），焦点环用 `Border.Focus`；勾选框 / 单选 / 开关的静止描边同日降为 `Border.Default`（原 `Border.Strong` 视觉过重，交互态仍走 accent）。WCAG 1.4.11 的 3:1 由「可交互态」满足，静止发丝线不承诺。
- **两条蓝各司其职**：`Cobalt` 只做主操作（`Accent.Primary`）；`Coating`（镜片镀膜的青）只做信息与次色（`Status.Info` / `Accent.Secondary`）。焦点环 `Border.Focus` 浅色走 `Coating.600`，深色自 2026-10-02 起改用 accent 本色 `Cobalt.450`（用户要求焦点偏蓝而非青；对深色画布 4.85:1）。旧方案里 `Info` 与 `Accent` 是同一个蓝，读不出「这是状态还是这是按钮」。
- **状态色不带复古感**：`Danger` 由砖红 `#A82828`（L\* 37.8）抬亮为朱红 `#BE3E2B`（L\* 45），`Success` 由橄榄 `#157A46` 改为 `#057E42`；`Warning` 独立成 `Palette.Yellow` 一族（Lab 色相角 97–102°），深色 `#E2C600` 压得住深灰墨字约 9.2:1、拆出来当无边框前景对画布也是 9.2:1。
- **浅色警告随真黄放宽（1.14.0 手调）**：1.13.0 时代浅色 `Warning` 停在琥珀 `#A08700`（AA-Large，3.5:1）；手调后两主题统一为真黄 `#E2C600`，白卡上对比度只有约 1.7:1——对比度不再是这一档的目标，`check.js` / `emit.js` 的浅色下限已按实测值锚定（若要恢复 AA-Large，把 spec.js 的 `Status.Warning` 改回 `Yellow.600` 并同步守卫）。因此浅色警告色块上**不要**放白字或深灰小字，优先「淡底 + 深色前景」（`Status.WarningSubtle` + `Text.Primary`）。

### 角色令牌全表

| 角色令牌 | 浅色 | 深色 | 用途 |
| --- | --- | --- | --- |
| `Theme.Brush.Background.App` | `#FFFFFF` | `#232323` | 应用 / 页面画布（1.14.0 手调：浅色与卡片同白、深色由带蓝的 `#16191C` 提亮为纯灰，层级全交给描边与阴影） |
| `Theme.Brush.Background.Subtle` | `#EBEDEF` | `#2F2F2F` | 次级背景：侧栏这类「次要区域」的底色（浅色比画布深 ΔL* 2.1，深色比画布亮约 ΔL* 6，保持「侧栏 / 内容」两区域可辨） |
| `Theme.Brush.Background.Second` | `#F6F6F6` | `#1D1D1D` | 再深一档画布 |
| `Theme.Brush.Background.Third` | `#DADEE2` | `#454A51` | 最深一档画布 |
| `Theme.Brush.Surface.Base` | `#FFFFFF` | `#1F2226` | 卡片、面板基础表面（浅色为纯白阅读面） |
| `Theme.Brush.Surface.Raised` | `#FFFFFF` | `#2C2C2C` | 抬升表面（弹层、浮起卡片） |
| `Theme.Brush.Surface.Sunken` | `#E9EAEA` | `#2F2F2F` | 内陷表面（只读输入区、代码块、进度槽） |
| `Theme.Brush.Surface.Focused` | `#FFFFFF` | `#232323` | 输入框获得焦点时的底色：两主题都与卡片同阶（焦点态由 accent 边框表达，不比静止态更灰） |
| `Theme.Brush.Surface.Overlay` | `#FFFFFF` | `#2F2F2F` | 覆盖层（对话框、下拉；浅色与卡片同白靠阴影分层，深色比卡片亮一档） |
| `Theme.Brush.Surface.Hover` | `#F6F6F6` | `#2F2F2F` | 悬停（浅色比白卡深约 ΔL* 3，深色比卡片亮约 ΔL* 6） |
| `Theme.Brush.Surface.Pressed` | `#E9EAEA` | `#2F2F2F` | 按下（深色与悬停同阶） |
| `Theme.Brush.Surface.Selected` | `#DCEBFD` | `#16304D` | 选中 |
| `Theme.Brush.Text.Primary` | `#24282E` | `#DDE0E5` | 正文 |
| `Theme.Brush.Text.Secondary` | `#494F57` | `#AAB0B8` | 次级文字 |
| `Theme.Brush.Text.Tertiary` | `#60666E` | `#969BA4` | 三级文字（说明、占位） |
| `Theme.Brush.Text.Disabled` | `#B0B7BE` | `#454A51` | 禁用文字 |
| `Theme.Brush.Text.Inverse` | `#FFFFFF` | `#232323` | 反色表面上的文字 |
| `Theme.Brush.Text.OnAccent` | `#FFFFFF` | `#232323` | 主色 / 状态色块上的文字 |
| `Theme.Brush.Border.Default` | `#DADEE2` | `#454A51` | 控件静止边框：发丝线（浅色 1.35:1 / 深色 1.79:1；浅色 2026-10-02 手调减淡），只要勾出轮廓 |
| `Theme.Brush.Border.Subtle` | `#DADEE2` | `#2C2C2C` | 分隔线（装饰性，刻意压得很淡） |
| `Theme.Brush.Border.Divider` | `#DADEE2` | `#2C2C2C` | Shell 级区域分隔线（侧栏 / 内容区等分区界线；侧栏铺次级背景后，对两侧 1.15:1 / 1.04:1） |
| `Theme.Brush.Border.Medium` | `#8C939C` | `#51575E` | Default 与 Strong 的中间强调档（浅色 3.10:1 / 深色对卡面 2.19:1，2026-10-02 新增）：开关未选中滑块 |
| `Theme.Brush.Border.Strong` | `#686F79` | `#697078` | 强调边框：输入类控件悬停描边（≥3:1，满足非文本对比） |
| `Theme.Brush.Border.Focus` | `#00768D` | `#4E90E8` | 焦点环（深色取 accent 本色 `Cobalt.450`） |
| `Theme.Brush.Accent.Primary` | `#1F5FC4` | `#4E90E8` | 主操作 |
| `Theme.Brush.Accent.PrimaryHover` | `#174CA4` | `#639BE9` | 主操作悬停 |
| `Theme.Brush.Accent.PrimaryPressed` | `#123C82` | `#3E82D6` | 主操作按下 |
| `Theme.Brush.Accent.PrimarySubtle` | `#EFF6FF` | `#16304D` | 主色淡底（标签、选中行） |
| `Theme.Brush.Accent.Secondary` | `#00768D` | `#74CBDC` | 次强调 |
| `Theme.Brush.Accent.SecondarySubtle` | `#E6F5F9` | `#0C3440` | 次强调淡底 |
| `Theme.Brush.Status.Info` | `#00768D` | `#74CBDC` | 信息 |
| `Theme.Brush.Status.InfoSubtle` | `#E6F5F9` | `#0C3440` | 信息淡底 |
| `Theme.Brush.Status.Success` | `#057E42` | `#58C278` | 成功 |
| `Theme.Brush.Status.SuccessHover` | `#126A39` | `#6DD38C` | 成功悬停 |
| `Theme.Brush.Status.SuccessSubtle` | `#E7F8EB` | `#173823` | 成功淡底 |
| `Theme.Brush.Status.Warning` | `#E2C600` | `#E2C600` | 警告（1.14.0 手调：两主题统一真黄） |
| `Theme.Brush.Status.WarningHover` | `#EDD600` | `#EDD600` | 警告悬停 |
| `Theme.Brush.Status.WarningSubtle` | `#FFF6C4` | `#3E3503` | 警告淡底 |
| `Theme.Brush.Status.Danger` | `#BE3E2B` | `#E77465` | 危险 |
| `Theme.Brush.Status.DangerHover` | `#99352C` | `#F88E81` | 危险悬停 |
| `Theme.Brush.Status.DangerSubtle` | `#FFF0EE` | `#48201D` | 危险淡底 |
| `Theme.Brush.State.DisabledSurface` | `#E9EAEA` | `#1F2226` | 禁用底 |
| `Theme.Brush.State.DisabledBorder` | `#DADEE2` | `#2C2C2C` | 禁用边框 |
| `Theme.Brush.State.DisabledForeground` | `#B0B7BE` | `#454A51` | 禁用前景 |
| `Theme.Brush.State.DisabledVeil` | `#A0FFFFFF` | `#A0111215` | 禁用蒙层（半透明，盖在日历 / 日期弹层上：浅色洗淡、深色压暗） |
| `Theme.Brush.State.HoverScrim` | `#14070B10` | `#14FFFFFF` | 状态层纱色：悬停反馈（8% 墨 / 8% 白，叠在任意底色上，色相不变） |
| `Theme.Brush.State.PressedScrim` | `#26070B10` | `#26FFFFFF` | 状态层纱色：按压反馈（15% 墨 / 15% 白） |
| `Theme.Brush.ScrollBar.Thumb` | `#B0B7BE` | `#454A51` | 滚动条滑块 |
| `Theme.Brush.ScrollBar.ThumbHover` | `#8C939C` | `#697078` | 滚动条滑块悬停 |
| `Theme.Brush.TransparentBackground.Base` | `#FFFFFF` | `#1F2226` | 棋盘格浅格 |
| `Theme.Brush.TransparentBackground.Alt` | `#DADEE2` | `#1D1D1D` | 棋盘格深格 |
| `Theme.Brush.Status.Disable` | `#B0B7BE` | `#697078` | 停用态前景（旧库仅浅色有，两主题已补齐） |
| `Theme.Brush.Effect.Shadow` | `#26070B10` | `#B3070B10` | 投影载体色（ARGB） |
| `Theme.Brush.Overlay.Backdrop` | `#99070B10` | `#B3070B10` | 遮罩 / 半透明背板 |

以下四项容易被误用，特别注意：

- **Text.OnAccent 在深色下是墨色不是白色**：深色强调色本身已经够亮，白字压上去只有 3.24:1（危险色上更低），换成深灰墨色 `#232323`（1.14.0 手调值）后对主色 4.85:1、危险色块 5.30:1、信息 / 成功 / 警告色块 7.0–9.2:1。`Status.*` 色块上的文字同样走这个键，不要写死 `#FFFFFF`。
- **Text.Inverse** 指「反色表面」上的文字（整体取反的选中条、徽标底等），浅色为白、深色为深灰墨色。当前与 `Text.OnAccent` 同值，但语义不同，不要互换。
- **Status.* 有双重职责**：既当色块底（配 `Text.OnAccent`），也当无边框模式（`Label.DisplayMode`、AppBar 徽标等）的前景。取值必须同时满足「字压得住底」和「底/字在纸上够分量」两端——`Tools/palette/check.js` 对此有硬断言，不满足时 `emit.js` 直接拒绝生成 XAML。1.14.0 手调后浅色 `Status.Warning` 与深色同为真黄 `#E2C600`（白卡上约 1.7:1，守卫已按实测锚定），所以浅色警告徽章 / 无边框前景**不要**配白字或小号深灰字，优先「淡底 + 深色前景」（`Status.WarningSubtle` + `Text.Primary`）；其余状态照守 4.5:1。
- **Surface.Sunken ≠ Surface.Focused**：`Sunken` 是静态内陷（只读输入框、代码块、进度槽、DataGrid 底色），台阶很大（对卡片 ΔL\* 7.7 / 7.9）；输入框**获得焦点**时的底色走 `Surface.Focused`，只比卡片挪半步（ΔL\* 4.2 / 4.3）。写模板时按语义选，不要因为「都是浅一点的底」而互换。

### 官方 WPF 控件的继承范围

把 `Generic.xaml` 合并进 `Application.Resources`（`Samples/Junevy.Controls.Showcase/App.xaml` 即此写法）后，库里的隐式样式会自动作用于这些**官方**控件，无需换成 `jv:` 版本：`Button`、`TextBox`、`ToolTip`、`ScrollBar`、`ListView`、`ListBox`、`DatePicker`、`DatePickerTextBox`、`DataGrid`、`ComboBox`、`CheckBox`、`Slider`、`TreeView`、`TabControl`，以及日历家族的 `Calendar` / `CalendarItem` / `CalendarDayButton` / `CalendarButton`。

以下官方类型**不会**被重新着色（库里的同名隐式样式写在模板内部，只服务 `jv:` 控件自己的部件，不外溢）：`TextBlock`、`Label`、`RadioButton`、`ToggleButton`、`Menu`、`MenuItem`、`Separator`、`ListBoxItem`、`Border`、`GroupBox`、`ProgressBar`、`Expander`、`ContextMenu`、`Window`。用 `jv:` 对应控件，或自行绑 `Theme.Brush.*`：

```xml
<Window
    Background="{DynamicResource Theme.Brush.Background.App}"
    TextElement.Foreground="{DynamicResource Theme.Brush.Text.Primary}">
```

只引用程序集而不合并字典时，上述官方控件一个都不会继承（实测 0 个带隐式样式）；宿主窗口的 `Background` 与文字颜色不在库的管辖内，必须自己设，否则深色主题下仍是白底黑字。

`TreeView` 相关的一个更名提示：合并 `Generic.xaml` 后行首观感用的枚举自 `3.2.0` 起叫 `TreeViewDisplayMode`（值 `Chevron` / `Indicator`，原 `DisplayMode` 的 `Normal` / `Icon`），附加属性写法 `atc:TreeViewAssist.DisplayMode` 不变，`<TreeView>` 与 `jv:TreeView` 通用（详见 [TreeView](#treeview-与-treemenuitem)）。

布局与效果令牌：

| 资源键 | 用途 |
| --- | --- |
| `Theme.ControlCornerRadius` | 默认控件圆角 |
| `Theme.ToolboxCornerRadius` | `Toolbox` 容器专用圆角，默认 `0`（直角） |
| `Theme.ControlPadding` | 默认控件内边距 |
| `Theme.PopupShadow` | 阴影令牌（`DropShadowEffect`），用于弹层、悬浮卡片 |
| `Theme.ButtonShadow` | `jv:Button` 专用的向下浮起阴影令牌（与 `Theme.PopupShadow` 同族，按控件尺寸收紧） |
| `TransparentBackground` | 透明图像背景的棋盘格（中档，8px 方格） |
| `TransparentBackground.Small` / `.Large` | 4px / 16px 方格的棋盘格 |
| `TransparentBackground.Geometry` | 棋盘格单个 16×16 单元内的两块 8×8 方格（纯几何，无颜色） |

阴影只换了载体色相（浅色 `#26070B10`、深色 `#B3070B10`，取代原来的 `#1A0D1520` 与纯黑 `#99000000`），`BlurRadius`、`ShadowDepth`、`Opacity` 三个参数逐条保持原样。WPF 的 `DropShadowEffect` 并不读取 `Color` 的 alpha 通道，浓淡只由 `Opacity` 决定，所以换色相不会改变阴影浓度——离屏实测六个令牌改版前后压暗量漂移均为 `0.00%`。

改配色请先读 `Tools/palette/README.md`：`node Tools/palette/check.js` 跑 121 条对比度 / 亮度台阶断言，`node Tools/palette/emit.js` 重新生成两份 `AppColors.*.xaml` 并回读校验（镜像层漂移、悬空引用、资源计数都会被拦下）。

运行时切换主题：

```csharp
using Junevy.Controls.Themes;

ThemeManager.ApplyTheme(AppTheme.Dark);
ThemeManager.ApplyTheme(AppTheme.Light);
ThemeManager.ToggleTheme();
```

`ThemeManager` 会替换现有主题字典，不要同时手动合并浅色和深色字典。

覆盖主题令牌只能**追加字典**（`Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = … })`），不要用 `Application.Current.Resources["Theme.Brush.Surface.Sunken"] = …` 直接写：这类本层条目优先级高于 `MergedDictionaries`，会**永久遮蔽**主题字典，`ApplyTheme` 之后该令牌仍返回旧主题的画刷（深色主题下选中行退回浅色底、浅色字因此不可读，探针实测）。测试里用完也要 `Remove` 而不是「写回原值」——写回同样留下本层条目。

### 透明背景棋盘格 `TransparentBackground`

用于表示「图像/颜色本身是透明的」那种棋盘格底纹，常见于图片查看器、颜色选择器、图层面板。三个键都是 `DrawingBrush`（矢量绘制、`TileMode=Tile`），方格尺寸由资源自身决定，**不会随控件尺寸拉伸**；配色取自当前主题，切换主题后自动换色。

```xml
<!-- 必须用 DynamicResource，否则切换主题时不会刷新 -->
<Border Width="320" Height="200" Background="{DynamicResource TransparentBackground}" />

<!-- 三档方格尺寸：4px / 8px / 16px -->
<Border Background="{DynamicResource TransparentBackground.Small}" />
<Border Background="{DynamicResource TransparentBackground.Large}" />

<!-- 只需要几何体时（自绘、蒙版、非棋盘格配色）：一个 16x16 单元内的两块 8x8 方格 -->
<Path Data="{DynamicResource TransparentBackground.Geometry}" Fill="Gray" />
```

`ImageViewer` 的棋盘格背景（`CheckerboardBrush`）默认就是 `TransparentBackground`（中档），可覆盖为其他档位。`Resources/Pictures/Image.xaml` 里的 `whiteCheckBoard` 是历史遗留位图，不会随主题变色且会随尺寸拉伸，新代码请改用上述资源。

## 附加属性

### Icon

`Junevy.Controls.AttachedProperties.Icon` 为多个控件提供统一图标数据。

| 附加属性 | 默认值 | 实际效果 |
| --- | --- | --- |
| `atc:Icon.Icon` | `null` | 设置图标内容。可以是图标字体字符，也可以是 `Image`、`Path` 或其他对象。模板支持的控件会在值为空时折叠图标本身；周围布局是否收缩由具体控件决定。 |
| `atc:Icon.FontFamily` | 内置 `iconfont` | 设置图标字体。用于 `Button`、`CardButton`、`TextBox`、`Label`、`AppBar`、`SideMenu`、`TreeView`、`TabControl` 等控件。 |
| `atc:Icon.IconSize` | `14` | 设置图标尺寸。`Button`、`AppBar`、`SideMenu` 和 `TreeView` 的模板会读取该值；`Label` 也读取（Boxed 模式样式默认 `8`、Borderless 模式默认 `14`，`3.2.0` 起支持）；`TreeView` 的节点图标字号在 `Chevron` 与 `Indicator` 两种行首观感下都生效（`3.2.0` 修复：此前只有 `Indicator` 读取，`Chevron` 完全忽略该值）。 |
| `atc:Icon.IconForeground` | `Gray` | 设置图标颜色。`ToolboxItem` 和 `ToolItem` 的默认模板会读取该值；`Label` 也读取（样式按 `DisplayMode` 注入默认值，`3.2.0` 起支持局部覆盖）；其他控件是否支持取决于其模板。 |

`ToolboxItem` 和 `ToolItem` 的图标字体字符跟随 `IconForeground`，标题跟随 `Foreground`；其他控件的图标字体字符通常跟随 `Foreground`。`Image` 或带固定 `Fill` 的 `Path` 不会自动重新着色。

```xml
<jv:Button
    atc:Icon.FontFamily="{DynamicResource IconFont}"
    atc:Icon.Icon="&#xE60F;"
    atc:Icon.IconSize="18"
    Content="Settings"
    Foreground="{DynamicResource Theme.Brush.Text.Primary}" />
```

#### 图标字体资源

库内置两套风格统一、码点完全一致的图标字体（67 个图标，设计规格与重建脚本见 `Tools/iconfont/`）：

| 资源键 | 字体文件 | 风格 |
| --- | --- | --- |
| `{DynamicResource IconFont}` | `Resources/Font/iconfont.ttf` | 线性/描边（默认） |
| `{DynamicResource IconFontFilled}` | `Resources/Font/iconfont-filled.ttf` | 面性/填充 |

两套字体码点、字形名完全一致，同一个图标字符切换 `FontFamily` 即可在两种视觉间整套切换：

```xml
<TextBlock FontFamily="{DynamicResource IconFont}" FontSize="24" Text="&#xE63F;" />
<TextBlock FontFamily="{DynamicResource IconFontFilled}" FontSize="24" Text="&#xE63F;" />
```

Showcase 的「图标字体」页提供全部 67 个图标的总览，支持线性/面性切换、尺寸预览与名称/码点过滤。新增或修改图标请修改 `Tools/iconfont/spec.py` 后执行 `python Tools/iconfont/emit.py` 重建，不要直接手改 TTF。

### PagingAssist

`atc:PagingAssist` 为任意 `ItemsControl`（含 ListBox / ListView / DataGrid 及其官方原生实例）提供**客户端分页**：设置 `PageSize` 后按页过滤条目，并在 ListBox / ListView / DataGrid 的模板内自动出现页码栏（`jv:DataPager`）。

| 附加属性 | 默认值 | 实际效果 |
| --- | --- | --- |
| `atc:PagingAssist.PageSize` | `0` | 每页条数；大于 0 启用分页并自动出现页码栏，置 0 恢复不分页（并还原视图过滤） |
| `atc:PagingAssist.CurrentPage` | `1` | 当前页码（可读写，外部程序化翻页；越界自动钳制） |
| `atc:PagingAssist.PageCount` | `1` | 总页数（只读，供页码控件绑定） |
| `atc:PagingAssist.TotalCount` | `0` | 条目总数（只读，宿主过滤后的总数） |
| `atc:PagingAssist.Placement` | `Bottom` | 页码栏方位：`Bottom` / `Top` / `Left` / `Right`（左/右为竖排页码栏） |

```xml
<jv:DataGrid
    ItemsSource="{Binding Rows}"
    atc:PagingAssist.PageSize="10"
    atc:PagingAssist.Placement="Bottom" />
```

机制与约定：

- **客户端分页**：数据需整体在内存中；实现为数据源默认视图上的组合过滤器（保留宿主已有过滤 + 当前页成员），排序发生在分页之前——DataGrid 列头点击排序为「全局排序后再分页」的正确语义。
- **两个启用分页的控件不可共享同一数据源**：默认视图按数据源单例，共享会争抢同一过滤器；需要同源多控件时用 `CollectionViewSource` 为其一建立独立视图，或各持一份集合。
- **仅支持经 `ItemsSource` 提供的数据**：在 XAML 里以子条目方式直接声明内容的 `ItemsControl`（未设 `ItemsSource`）不会分页（页码栏也不出现）；此类场景请改为绑定集合。`PageSize` 接受大于等于 0 的整数，负数会被拒绝。
- 翻页/数据变化触发视图刷新：滚动位置复位；数据增删时页码自动钳制。
- 页码栏（`jv:DataPager`）含首页/上一页/滑窗页码/下一页/末页、每页条数选择器与总数文本；候选值经 `jv:DataPager.PageSizeOptions`（默认 "10,20,50,100"）配置，当前 `PageSize` 不在候选中时自动并入。

### ExpanderBehavior

`atc:ExpanderBehavior.Enable` 用于 `TreeViewItem`，`TreeView` 的默认容器样式已经自动启用该行为，通常不需要手动设置。启用后：

- **双击行**：分支节点切换展开/收起。导航职责已移出本类：`jv:TreeView` 的 `NavigateCommand` 为**选中驱动**（选中变化即执行，参数=数据项，见 [TreeView](#treeview-与-treemenuitem)），双击改用 `jv:TreeView.ItemDoubleClick` 路由事件（携带数据项）；官方 `<TreeView>` 保留**叶激活**旧语义——双击/Enter 叶节点时执行 `atc:TreeViewAssist.NavigateCommand`。
- **`Enter`**：分支切换展开/收起；叶节点在 `jv:TreeView` 上不执行命令（键盘 ↑↓ 已承担导航），官方 `<TreeView>` 保留叶激活。
- **来源守卫（`3.2.0` 新增）**：`Enter` 只在容器自身持有焦点（事件源即该 `TreeViewItem`）时接管；双击若由行内的可聚焦子控件发起——编辑框、密码框、下拉、列表类控件，或行首的展开箭头——行为不接管，双击选词、开合下拉等原生交互不再被吞掉；双击落在子孙行而非本行时同样不接管（祖先容器不替子孙做决定）。
- **不抢未被消费的输入**：事件没有被接管（如叶节点未满足接管条件）时不标记已处理，宿主自己的键鼠处理程序仍能看到。
- **选中自动展开祖先**：节点被选中时按**所属树**的 `atc:TreeViewAssist.AutoExpandAncestors`（默认 `true`）沿容器父链展开其所有祖先，选中行不会藏在收起的分支里；置 `false` 后由宿主自行展开。**生效边界（探针 C5 实测）**：这一步靠容器自己冒上来的 `Selected` 事件驱动，所以只对「容器已经生成」的节点起作用——祖先从未展开过时，子级容器还不存在，只把 `IsSelected` 写进数据模型并不会让祖先自动展开；这种「按路径选中」的场景必须由宿主沿刚查到的路径先置 `IsExpanded = true` 再写选中。整树展开用 `jv:TreeView.ExpandAll()` 不受此限制（它直接写数据模型）。该属性注册时未启用值继承，设在各容器上不会自动下发，库一律按「最近的 `TreeView`」读取（`jv:TreeView` 实例属性与附加属性是同一 DP）。嵌套树互不串味：内层树条目被选中只展开内层祖先，外层容器不接管。

**叶/枝的判定按容器 `HasItems`，与数据类型无关**；但**展开/选中状态能否保留取决于条目是否暴露 `IsExpanded` / `IsSelected`**：默认容器样式把这两个属性与 `TreeMenuItem.IsExpanded` / `IsSelected` 双向绑定。换用自定义数据类型时必须在 `ItemContainerStyle` 里自行绑定这两条，否则首次展开子级时容器取默认值（收起 / 未选中），模型里的状态读不回来。

### SmoothScrolling

`atc:SmoothScrolling` 把鼠标滚轮的离散跳变改为连续的补间滚动。WPF 的滚轮滚动没有任何动画，一个刻度就是一次瞬移；按项滚动下偏移还会被吸附到条目边界，观感上就是"从第 1 项直接跳到第 2 项"。启用后，滚轮目标偏移由逐帧渲染回调以指数趋近方式收敛过去（与浏览器平滑滚动同款手感：连续滚动时目标累加、速度连续，不存在逐格重启动画的脉冲感）；拖动滑块、点击箭头/轨道、按下鼠标、键盘滚动等用户操作会立即停止补间、交还控制权。

| 附加属性 | 默认值 | 实际效果 |
| --- | --- | --- |
| `atc:SmoothScrolling.IsEnabled` | `false`（库内列表控件经默认样式为 `true`） | 是否启用平滑滚轮。可设在任意 `ScrollViewer` 或可滚动控件上；设在控件上时，其模板须按库内约定命名 `PART_ScrollViewer` 部件，缺失时保持 WPF 原生行为 |
| `atc:SmoothScrolling.Step` | `NaN` | 每个滚轮刻度的滚动像素数；`NaN` 时跟随系统"滚轮滚动行数"折算（N 行 × 16px；系统设为"一次滚动一页"时按一个视口计） |

本库 `ListBox` / `ListView` / `DataGrid` / `ComboBox` / `SideMenu` 的默认样式已启用平滑滚轮，实例上设为 `False` 可单独关闭：

```xml
<!-- 关闭平滑滚轮 / 自定义每刻度步长 -->
<jv:ListBox ItemsSource="{Binding Devices}"
            atc:SmoothScrolling.IsEnabled="False" />

<jv:ListBox ItemsSource="{Binding Devices}"
            atc:SmoothScrolling.Step="120" />
```

页面级的普通 `ScrollViewer` 默认不在启用范围内。要让应用内**所有**滚动宿主（页面、模板内部）统一获得同款手感，在 `App.xaml` 加一条隐式样式即可；嵌套时滚轮仍归最内层宿主，列表控件经控件挂接持有宿主引用、读取自身 `Step`，不受该样式影响：

```xml
<Style TargetType="ScrollViewer">
    <Setter Property="atc:SmoothScrolling.IsEnabled" Value="True" />
    <Setter Property="atc:SmoothScrolling.Step" Value="100" />
</Style>
```

**注意事项（WPF 原生行为，与平滑滚动无关）**：原生 `ScrollViewer` 的 `OnMouseWheel` 会**无条件吞掉竖向滚轮**——即使它竖向根本不可滚。典型如 `VerticalScrollBarVisibility="Disabled"`、仅横向可滚的宿主（代码块、标尺条、单行滚动区等）：光标悬停其上时，页面/外层宿主不会滚动。启用全应用平滑后快速滚动，光标扫过这类宿主（如演示页展开的 XAML 代码块）会出现"页面停顿、像被抢滚轮"的感觉——滚动变丝滑后，原生的停顿反而更醒目；把这类宿主折叠或让光标避开即可恢复。本库已保证平滑层不参与该行为（竖向不可滚的宿主不会被接管；完全未挂接的原生 `ScrollViewer` 同样吞滚轮）。若希望这类宿主把竖向滚轮交还外层（页面），在宿主上处理 `PreviewMouseWheel` 并经 `SmoothScrolling.ScrollByWheel` 转发——转发与真实滚轮共用同一补间路径与步长折算（连续转发、与真实滚轮混用都不会互相拉扯），外层未启用平滑时为瞬时滚动：

```csharp
codeScroll.PreviewMouseWheel += (s, e) =>
{
    if (s is ScrollViewer sv && sv.ScrollableHeight <= 0)
    {
        e.Handled = true;
        SmoothScrolling.ScrollByWheel(outerScrollViewer, e.Delta);
    }
};
```

- **需要像素级滚动与预生成缓存**：连续补间要求 `VirtualizingPanel.ScrollUnit="Pixel"`，并建议配合 `VirtualizingPanel.CacheLength="1,1"`（`CacheLengthUnit="Page"`）把容器生成提前到滚动到达之前，消除跨越条目边界时的实体化顿挫——两者本库列表样式均已内置。若把 `ScrollUnit` 改回 `Item`（按项滚动），行为自动退回 WPF 原生滚轮，不会出现阶梯状补间。
- **嵌套就近接管**：滚轮始终作用于离鼠标最近的滚动宿主——`ScrollViewer` 内嵌 `DataGrid`/`ListView`、条目模板自带滚动区时滚轮归最内层；内层滚到边界时不接力外层，均与 WPF 原生语义一致。
- **横向退化路径同步受益**：`jv:ListBox`/`jv:ListView` 横向模式的滚轮折算经同一补间出口，竖向滚不动退化为水平滚动时同样平滑，折算单位跟随面板的实际滚动单位。
- **虚拟化失效诊断（DEBUG）**：DEBUG 构建下，`jv:ListBox`/`jv:ListView`/`jv:DataGrid` 在滚动方向收到无约束（∞）测量（典型于无定高嵌套在 `StackPanel` 或未定高的外层 `ScrollViewer` 中）时，会向调试输出一次性警告——此时条目虚拟化已失效，请为控件设置 `Height`/`MaxHeight` 或避免外层滚动宿主；Release 构建零开销。

### TitleAssist

`atc:TitleAssist` 为 `TextBox` 和 `ComboBox` 在输入框外侧显示一个标题，提示该输入框的用途。标题内容为任意对象（`object`），可以直接设为 iconfont 字形文本。`TitleWidth` 可为标题区域指定固定宽度，用于表单式布局中输入框整列对齐。

| 附加属性 | 默认值 | 实际效果 |
| --- | --- | --- |
| `atc:TitleAssist.Title` | `null` | 标题内容；为 `null` 时不显示标题，也不占用布局空间（必填标识同样不显示） |
| `atc:TitleAssist.TitlePlacement` | `Top` | 标题位置：`Top` / `Bottom` / `Left` / `Right`，标题与输入框间距固定 4 DIP |
| `atc:TitleAssist.TitleWidth` | `NaN` | 标题区域固定宽度（DIP），四个方位统一生效；`NaN` 时自适应标题内容。表单式布局中统一设置后，不同长度的标题保持一致的标题—输入框间距，输入框整列对齐（`Left` 方位标题自动右对齐贴合输入框） |
| `atc:TitleAssist.TitleFontFamily` | `null` | 标题字体族；为 `null` 时继承控件自身字体。标题为 iconfont 字形时需设置为 iconfont |
| `atc:TitleAssist.TitleFontSize` | `NaN` | 标题字号；`NaN` 时继承控件自身字号 |
| `atc:TitleAssist.TitleForeground` | `null` | 标题颜色；为 `null` 时由默认样式提供主题次级文本色 |
| `atc:TitleAssist.TitleFontWeight` | `Normal` | 标题字重 |
| `atc:TitleAssist.IsRequired` | `false` | 必填标识开关；`true` 且已设置 `Title` 时在标题旁显示必填图标（与标题一起显示/隐藏） |
| `atc:TitleAssist.IsRequiredIcon` | `null` | 必填图标内容；为 `null` 时显示默认的主题 Danger 色小圆点，设置后替换默认圆点（如星号 `*`、iconfont 字形或任意 `object`，字体样式继承标题设置） |
| `atc:TitleAssist.IsRequiredIconPlacement` | `Right` | 必填图标相对标题文字的位置：`Right`（标题右侧）/ `Left`（标题左侧），图标与标题间距固定 4 DIP |

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

<!-- 必填标识：默认在标题右侧显示主题 Danger 色小圆点 -->
<jv:TextBox
    Width="220"
    atc:TitleAssist.Title="服务器地址"
    atc:TitleAssist.IsRequired="True" />

<!-- 自定义必填图标（星号），并放到标题左侧 -->
<jv:ComboBox
    Width="220"
    atc:TitleAssist.Title="采集模式"
    atc:TitleAssist.IsRequired="True"
    atc:TitleAssist.IsRequiredIcon="*"
    atc:TitleAssist.IsRequiredIconPlacement="Left" />
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

多个模板通过 WPF 的 `Border.CornerRadius` 依赖属性读取控件圆角，例如 `Button`、`CardButton`、`TextBox`、`PasswordBox`、`ComboBox`、`CheckBox`、`RadioButton`、`DatePicker`、`GroupBox`、`ListView`、`ListBox` 和 `ProgressBar`：

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

`jv:Button` 继承 WPF `Button`，保留 `Command`、`Click`、`ContentTemplate`、键盘焦点和访问键等标准行为。默认模板同时支持文字和 `atc:Icon` 图标；没有图标时不会保留前置空白。内容始终按 `FontSize` 以固定字号排版（与官方 `Button` 一致），控件不做任何自动缩放——空间不足时由 `FontSize`/尺寸自行调整。图标大小由 `atc:Icon.IconSize` 决定（同时是图标槽宽度与图标字号，默认 `14`），与按钮 `FontSize` 彼此独立。

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

悬停/按压采用**状态层（state layer）**反馈：不替换固定底色、也不降低整体透明度，而是在任意背景上叠一层固定透明度的纱——`Theme.Brush.State.HoverScrim`（浅色 8% 墨 `#14070B10` / 深色 8% 白 `#14FFFFFF`）与 `Theme.Brush.State.PressedScrim`（两主题各 15%：`#26070B10` / `#26FFFFFF`）。文字与图标全程保持实色，反馈比旧版整体变淡明显得多；鲜艳背景（如红色按钮）悬停/按压时只是「同色相加深一档」（深色主题反向提亮），不会跳到灰色系产生割裂，用户自定义背景同样成立。禁用态仍为整体 0.5 透明度 + 禁用表面。`NoBorderButtonStyle` 透明背景（幽灵按钮）的悬停/按压为纱色填充出圆角色块。

同一反馈方案已推广至其余可交互控件：**背景可自定义的按钮/卡片类**（`CardButton`、`ToolBarItem`、`ToolboxItem`、`ToolItem`）与**内部图标小按钮**（`MessageBar` / `ProgressBarWindow` 关闭按钮、`ImageViewer` 工具栏按钮、`DialogWindow` 标题栏按钮、`DatePicker` 日历导航/头部/下拉按钮）悬停/按压均为状态层纱色；`MenuBar` / `ContextMenu` / `TabControl` / `SideMenu` / `TreeView`、`ListBox` / `ListView` / `DataGrid`、`GroupBox` / `ExpanderPanel` 等中性表面上的列表/菜单项仍保留 `Surface.Hover` 灰底悬停高亮（行首箭头同理：`TreeView` 的展开箭头与 `ExpanderPanel` 的头部按钮用 `Surface.Hover` 灰底，`ComboBox` 的下拉箭头只做字形加深，三者都不叠状态层纱色）。两个纱色为 `DynamicResource` 主题令牌（`Themes/AppColors.*.xaml` 生成，守卫脚本断言其对卡片的 ΔL\* 落在可感知区间），宿主可整体覆盖。

`jv:Button` 可选一层只向下散开的浮起阴影（默认关闭，常态贴平，与 ComboBox / TextBox 等同级控件一致），设 `ShowShadow=True` 打开；按压或禁用时自动消失（贴回地面），与降透明度的状态反馈叠加使用。阴影取自新令牌 `Theme.ButtonShadow`（浅色 `BlurRadius=16 / ShadowDepth=5 / Opacity=0.18`，深色 `18 / 6 / 0.40`；载体色 `#26070B10` / `#B3070B10`），与 `Theme.PopupShadow` 同族同强度，只按控件尺寸收紧模糊与偏移：实测按钮下方 1–9px 相对压暗 `8.47%`，MessageBar 弹层同距离为 `9.50%`；`5–9px / 1–5px` 衰减比 `0.58` 对弹层 `0.63`，即同一族形状而非另立一套观感。Effect 只挂在模板内**不含任何子元素**的背景层 `Border` 上，文字仍走 ClearType；官方 `Button`、`NoBorderButtonStyle` 与 `CardButton`（自带模板）都不带这层阴影。宿主 App 在 `Application.Resources` 写同名键即可整体调淡或关掉（应用自身条目的优先级高于 `ThemeManager` 追加的主题字典，因此这一份覆盖值会同时用于浅色与深色，需要分档时自行取两套参数）：

```xml
<Application.Resources>
    <DropShadowEffect
        x:Key="Theme.ButtonShadow"
        BlurRadius="10"
        Direction="270"
        Opacity="0.08"
        ShadowDepth="3"
        Color="#1A0D1520" />
</Application.Resources>
```

独有依赖属性 `ShowShadow`（默认 `false`）：常态浮起阴影的开关。默认贴平，与 `ComboBox`、`TextBox` 等没有浮起感的同级控件并排时（表单行、工具条、对话框底栏）观感一致；需要浮起感强调的按钮（主操作、独立 CTA）可对单个按钮设 `true`，或用样式 Setter 批量/全局开启。只影响常态阴影：按压与禁用本就不显示阴影，状态层纱色的悬停/按压反馈、尺寸与文字渲染均不受影响。

```xml
<!--  逐个开启  -->
<jv:Button Content="确定" ShowShadow="True" />

<!--  整个 App 内所有 jv:Button 常态带浮起阴影  -->
<Style TargetType="{x:Type jv:Button}" BasedOn="{StaticResource {x:Type jv:Button}}">
    <Setter Property="ShowShadow" Value="True" />
</Style>
```

`NoBorderButtonStyle` 是可直接使用的无边框样式，适合标题栏等紧凑操作区。自定义普通按钮样式时，优先基于隐式类型样式 `{StaticResource {x:Type jv:Button}}`，避免与其他控件字典中的同名内部资源冲突。

### CardButton

`jv:CardButton` 继承 `jv:Button`，用于指标卡、快捷入口或带主数值的可点击卡片。

| 属性 | 效果 |
| --- | --- |
| `Title` | 卡片左上方标题，类型为 `object` |
| `Content` | 卡片主要内容或数值 |
| `MainColor` | 主要内容和图标颜色 |
| `atc:Icon.Icon` | 卡片右下角图标 |
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

`jv:ToggleButton` 继承 WPF `ToggleButton`，提供两套「轨道 + 滑块」开关模板：胶囊 `SwitchToggleButton_Radius`（轨道圆角 = 轨道高 / 2，默认隐式样式所用）与圆角矩形 `SwitchToggleButton_Rect`（外圆角取主题令牌 `Theme.SmallCornerRadius`，滑块圆角恒比外圆角小一个内缩量）。形状由模板决定，**不再有 `DisplayMode` 形状开关**（该依赖属性零消费方，自 `3.2.0` 起删除，XAML 上写 `DisplayMode` 会编译失败；枚举 `ShapeMode` 本身保留，`RadioButton` / `ProgressBar` 仍在用）。

| 属性 | 效果 |
| --- | --- |
| `IsChecked` | 标准可空选中状态 |
| `SwitchSize` | 开关整体高度，轨道宽度按 2:1 比例自动推导，默认 `20`，建议不小于 `12` |

开关采用「轨道 + 滑块」结构，几何由控件统一推导（`TrackHeight` / `TrackWidth` / `TrackPadding` / `TrackCornerRadius` / `ThumbSize` / `ThumbCornerRadius` / `ThumbTravel` 均为只读派生属性，仅供模板绑定，外部不应设置）。推导时先把尺寸换算成**整数设备像素**再折回 DIP，滑块的四边内缩由同一个内缩值决定，因此在 100% / 125% / 150% 等任意缩放下上下左右严格相等，不会出现一边多 1 像素；代价是渲染高度落在整数设备像素上，与 `SwitchSize` 设定值最多相差半个设备像素（125% 下 ≤0.4 DIP）。窗口换到不同缩放的显示器后会在下一次测量时按新比例重新吸附。胶囊模板圆角 = 轨道高 / 2（由只读派生属性 `TrackCornerRadius` 给出，滑块走 `ThumbCornerRadius`），圆角矩形模板使用 `Theme.SmallCornerRadius`，滑块圆角恒比轨道圆角小一个内缩量，内外圆角视觉吻合。圆角一律来自这些派生属性与主题令牌，**开关模板不读取 `Border.CornerRadius`**（与上文「Border.CornerRadius」一节的控件列表不同，设该属性不会改变开关形状）。切换时滑块以缓动动画滑动到对侧。

```xml
<jv:ToggleButton
    Content="Auto exposure"
    IsChecked="{Binding AutoExposure, Mode=TwoWay}"
    SwitchSize="22"
    Template="{StaticResource SwitchToggleButton_Radius}" />
```

普通开关需要显式选择 `SwitchToggleButton_Radius`（胶囊）或 `SwitchToggleButton_Rect`（圆角矩形）模板。原「`ExpanderButton` 箭头开关样式」（及 `ExpanderControlTemplate` 模板）自 `jv:TreeMenu` 更名 `jv:TreeView` 起已随其唯一消费方一并移除——`TreeView` 的展开图标改用 ExpanderPanel 同款 iconfont 旋转箭头（详见 [TreeView](#treeview-与-treemenuitem)）。

### RadioButton

`jv:RadioButton` 继承 WPF `RadioButton`，支持标准分组、命令和双向选中绑定。选中标记是矢量圆点（`EllipseGeometry`，几何中心即元素中心），不受图标字体的 em 盒留白影响，因此在任意缩放下都严格居中于选择框。

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

`jv:CheckBox` 继承 WPF `CheckBox`，保留 `IsChecked`、三态和命令行为，勾选标记为矢量对勾（圆头描边折线，几何包围盒等于标记元素盒），居中由布局保证，不再依赖图标字体度量。

依赖：主题资源、焦点样式。勾选标记改用矢量绘制后，模板不再读取 `atc:Icon.FontFamily`，默认样式里相应的 Setter 与 `Resources/Font/IconFont.xaml` 合并引用已一并移除；宿主自定义模板若要用图标字体，`atc:Icon.FontFamily` 的默认值本身就是内置 `iconfont`，且 `Themes/Generic.xaml` 仍合并了 `IconFont.xaml`，直接绑定即可。

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
| `atc:TitleAssist.Title` 系列（含 `TitleWidth` 固定宽度、`IsRequired` 必填标识） | 在输入框外侧显示用途标题，位置可选 `Top`/`Bottom`/`Left`/`Right`，支持 iconfont 与自定义字体样式，详见 [TitleAssist](#titleassist) |

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

### PasswordBox

`jv:PasswordBox` 是模板复合控件：WPF 原生 `PasswordBox` 为密封类无法派生，故模板内嵌一个原生密码框承载真实输入（IME、粘贴、`MaxLength` 等走原生管线），右侧眼睛按钮点击/长按显示明文，明文态为可编辑文本框并与掩码内容双向同步（显示中可直接改写密码）。相比原生控件的增强：`Password` 是真正的依赖属性，可双向绑定 ViewModel（原生密码不可绑定）。

| 属性 | 效果 |
| --- | --- |
| `Password`（依赖属性） | 密码内容，默认双向绑定；掩码输入、明文编辑、程序设值三者自动同步，变化触发路由事件 `PasswordChanged` |
| `RevealMode`（依赖属性） | 显示密码按钮的触发方式：`Click`（默认，点击切换显示/隐藏）或 `PressAndHold`（按下即显示，松开或按住移出按钮立即隐藏）；切换取值时自动收起明文 |
| `IsRevealed`（依赖属性） | 当前是否以明文显示，可双向绑定（如把「显示密码」挂到 ViewModel）；切换时键盘焦点跟随掩码框/明文框 |
| `IsError`（依赖属性） | 密码错误状态：`true` 时输入区背景叠加一层低透明度主题 `Status.Danger` 色（微微泛红），边框与文字不变，由业务侧验证失败时置位 |
| `PasswordChar`（依赖属性） | 掩码字符，默认 `●` |
| `MaxLength`（依赖属性） | 最大密码长度（0 不限制），同时作用于掩码框与明文框 |
| `atc:PlaceholderAssist.Placeholder` | 占位文本（支持 iconfont 字形），密码为空、未持焦点且未显示明文时显示 |
| `atc:Icon.Icon` / `atc:TitleAssist.Title` 系列 | 前置图标与外侧标题，语义同 `jv:TextBox`，详见 [Icon](#icon) / [TitleAssist](#titleassist) |

```xml
<!-- 点击眼睛显示明文；Password 可绑定（原生 PasswordBox 做不到），错误泛红由业务绑定驱动 -->
<jv:PasswordBox
    Width="260"
    atc:Icon.FontFamily="{DynamicResource IconFont}"
    atc:PlaceholderAssist.Placeholder="请输入密码"
    Password="{Binding LoginPassword}"
    IsError="{Binding PasswordInvalid}" />

<!-- 长按显示：按下即显示、松开立即隐藏 -->
<jv:PasswordBox Width="260" RevealMode="PressAndHold" />
```

安全说明：掩码态保持原生密码框行为（不可复制、内容不进剪贴板）；明文态（`IsRevealed="True"`）的明文可选中复制，与主流密码框一致。注意原生 `<PasswordBox>` 不提供本库接管样式（见「官方同名控件自动生效」），请使用 `jv:PasswordBox`。

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

**默认宽度**：默认 `MinWidth=140`（容纳 6 字占位文本与常见日期格式 + 日历按钮列）；模板内日期文本框与日历按钮为分列布局，日期文本不会延伸到按钮下方。宿主显式设置 `Width`/`MinWidth` 时以宿主值为准。

依赖：WPF `DatePicker`/`Calendar` 标准行为（`SelectedDateFormat`、`FirstDayOfWeek`、`BlackoutDates` 等）、主题滚动条与阴影令牌；附加属性 `atc:DatePickerAssist.PlaceHolder`（占位符）。

### Slider

`jv:Slider` 继承 WPF `Slider`，完整保留官方的拖拽、轨道分页、方向键与 `Home`/`End`、刻度、选择区段行为，另在滑块的上/下/左/右任意一侧附加一个可手动键入数值的数值框（模板部件 `PART_ValueBox`，外观复用库内 `jv:TextBox`）。合并 `Themes/Generic.xaml` 后，原生写法 `<Slider>` 与 `jv:Slider` 外观完全一致，数值框属 `jv:Slider` 专有——原生实例上自动折叠且不占布局。视觉上滑块为 16px 直角方形握手，轨道与选择区段均为直角矩形（无圆角），悬停描边高亮、拖拽填充主色。

| 属性 | 默认值 | 效果 |
| --- | --- | --- |
| `ShowValueBox` | `true` | 是否显示数值框；`false` 时整体 `Collapsed`，轨道立即占满腾出的空间，运行时切换即时生效 |
| `ValueBoxSide` | `Right` | 数值框停靠侧：`Left`/`Top`/`Right`/`Bottom` 四选一，横竖滑块均可任选。主轴侧限宽 `120`、交叉轴侧限宽 `160` 限高 `28`，避免数值框把轨道挤扁 |
| `ValueFormatString` | `null` | 数值框的显示格式（如 `F1`、`0.00`、`p0`），仅影响显示；`null` 时按当前区域性直接输出数值 |

**键入与提交**：输入过程中不改值，回车或数值框失焦时提交。按当前区域性解析（`NumberStyles.Float \| AllowThousands`，千分位按分组解析而非小数点），越界自动夹取到 `Minimum`/`Maximum`，非法文本（空串、非数字、`NaN`、无穷）不改值并把显示还原为当前值。写回使用 `SetCurrentValue`，双向绑定不受影响；回车不置 `Handled`，宿主的默认按钮等行为保持不变。

**宽度稳定性**：数值框按区间两端（`Minimum` / `Maximum` 经 `ValueFormatString` 格式化后）最宽的显示文本预留宽度，值变化（如 90 → 100）不再使数值框变宽、把轨道挤压出回弹；手动键入更长文本时仍允许临时增宽，提交后恢复。未设 `ValueFormatString` 时拖拽可能产生全精度小数文本（超出预留宽度会临时撑开数值框），需要紧凑定宽显示建议设置格式串（如 `F0`）。

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

`jv:ListBox` 继承 WPF `ListBox`，提供统一的悬停、选中、焦点和禁用状态，并默认启用 UI 虚拟化和回收模式。项目既可按默认的竖向列表排列，也可通过 `Orientation` 切换为横向带状列表并沿水平方向滑动。选中项以主色强调（背景 `Surface.Selected`、描边 `Accent.Primary`），点击获得键盘焦点后描边仍保持主色；青色焦点环只出现在未选中的聚焦项上。条目文字前景跟随控件 `Foreground`（默认为随主题切换的 `Text.Secondary`，选中项由触发器升为 `Text.Primary`、禁用态为 `State.DisabledForeground`）；在实例上设置 `Foreground` 会传导到所有条目，撤销覆盖用 `ClearValue(ForegroundProperty)`。

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

横向模式下鼠标滚轮同样左右滚动。WPF 的 `ScrollViewer` 只把滚轮用于竖直滚动，竖向滚不动时不会自动退化为水平滚动，`jv:ListBox` 在竖向不可滚动时补上这一步折算，折算量跟随面板的实际滚动单位（`ScrollUnit=Pixel` 或未启用逻辑滚动时按 `SystemParameters.WheelScrollLines` × 行高折算像素，`ScrollUnit=Item` 时按条目数折算）；条目模板内部自带滚动控件时（例如条目里还有 `ScrollViewer`）滚轮仍归内层控件。竖向列表、`Orientation` 行为以及原生 `<ListBox>` 完全沿用 WPF 原生滚轮行为。默认样式已启用像素滚动（`VirtualizingPanel.ScrollUnit=Pixel`）与平滑滚轮（`atc:SmoothScrolling`，见「附加属性 → SmoothScrolling」），横向折算与竖向滚轮共用同一补间出口。

自定义模板时请保留名为 `PART_ScrollViewer` 的 `ScrollViewer`（本库两个模板均如此命名），滚轮折算依赖该部件定位滚动宿主；缺少该部件时不会报错，只是退回 WPF 原生滚轮行为。

导航控件 `jv:SideMenu` 也有一个同名属性，但它继承的是 WPF `ListBox` 而非 `jv:ListBox`，其 `Orientation` 表示菜单项面板的排列方向，与本属性无关。

### ListView

`jv:ListView` 继承 WPF `ListView`，同时支持普通列表和标准 `GridView`。控件保留 WPF 的 `View` 管线，可以正常使用 `GridViewColumn.DisplayMemberBinding`、单元格模板和自定义 `ItemTemplate`。普通列表同样支持横向带状排列与水平滑动。选中项以主色强调（背景 `Surface.Selected`、描边 `Accent.Primary`），点击获得键盘焦点后描边仍保持主色；青色焦点环只出现在未选中的聚焦项上。条目文字前景跟随控件 `Foreground`（默认为随主题切换的 `Text.Secondary`，`GridView` 列单元格经行呈现器同样跟随；选中项由触发器升为 `Text.Primary`、禁用态为 `State.DisabledForeground`）；在实例上设置 `Foreground` 会传导到所有条目，撤销覆盖用 `ClearValue(ForegroundProperty)`。默认样式已启用像素滚动与平滑滚轮（`atc:SmoothScrolling`，见「附加属性 → SmoothScrolling」）。

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

`jv:DataGrid` 继承 WPF `DataGrid`，提供专属控件模板（官方 Aero2 宿主结构：`ScrollViewer`（自定义模板：列头、行视口 `PART_ScrollContentPresenter` 与纵/横滚动条）→ `ItemsPresenter` → `DataGridRowsPresenter`（DataGrid 默认 ItemsPanel，运行时命名 `PART_RowsPresenter`）；列头 `PART_ColumnHeadersPresenter` 位于 ScrollViewer 模板内，垂直滚动时保持固定）与库内统一的卡片式视觉：Sunken 列标题（悬停高亮、排序方向箭头）、透明单元格（行悬停与选中色直接透出、键盘焦点时底边切换为焦点色）、行悬停/选中高亮。默认启用行列虚拟化、像素滚动（`VirtualizingPanel.ScrollUnit=Pixel`）与平滑滚轮（`atc:SmoothScrolling`，见「附加属性 → SmoothScrolling」），关闭新增行、删除行和行高调整，并使用整行单选。合并 `Themes/Generic.xaml` 后，原生写法 `<DataGrid>` 直接生效，无需 `jv:` 前缀。

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

### DataPager

`jv:DataPager` 分页栏控件：首页 / 上一页 / 滑窗页码 / 下一页 / 末页按钮 + 每页条数选择器 + 总数文本，与 `atc:PagingAssist` 配套。ListBox / ListView / DataGrid 的模板页脚已内置，无需手写；其他 `ItemsControl` 或独立布局场景可直接摆放并显式绑定：

| 属性 | 默认值 | 效果 |
| --- | --- | --- |
| `Orientation` | `Horizontal` | 页码栏方向；模板页脚在 Left/Right 方位自动切换为 `Vertical` |
| `CurrentPage` | `1` | 当前页码（双向，读写即翻页） |
| `PageCount` / `TotalCount` | `1` / `0` | 总页数 / 条目总数（由 PagingAssist 同步） |
| `PageSize` | `0` | 每页条数（双向，选择器写入后同步回 PagingAssist.PageSize） |
| `PageSizeOptions` | `"10,20,50,100"` | 每页条数候选（逗号分隔；当前 PageSize 自动并入候选） |
| `ButtonCount` | `5` | 滑窗页码数 |

导航按钮由路由命令驱动（与 WPF Toolkit DataPager 同名）：`jv:DataPager.MoveToFirstPageCommand` / `MoveToPreviousPageCommand` / `MoveToNextPageCommand` / `MoveToLastPageCommand` / `MoveToPageCommand`（参数为目标页码）。按钮可用态由命令 `CanExecute` 自动控制；自定义模板时按钮绑定这些命令即可接入。`Orientation=Vertical` 时每页条数选择器与总数文本自动折叠，避免撑宽左/右停靠栏。
```xml
<StackPanel>
    <jv:ListBox x:Name="List" ItemsSource="{Binding Rows}"
                atc:PagingAssist.PageSize="10" />
    <!-- 独立摆放：显式绑定分页状态 -->
    <jv:DataPager
        CurrentPage="{Binding Path=(atc:PagingAssist.CurrentPage), ElementName=List, Mode=TwoWay}"
        PageCount="{Binding Path=(atc:PagingAssist.PageCount), ElementName=List}"
        TotalCount="{Binding Path=(atc:PagingAssist.TotalCount), ElementName=List}"
        PageSize="{Binding Path=(atc:PagingAssist.PageSize), ElementName=List, Mode=TwoWay}" />
</StackPanel>
```

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
| `Neutral`（`100`） | 中性标签，默认中性灰底（`Theme.Brush.Status.Neutral`），可用局部 `Background` 覆盖（如红/黄色块，覆盖后悬停变色自然失效） |

标签内容始终由 `Content` 提供，样式不会改写。各模式通过样式触发器注入默认图标（`atc:Icon.Icon`），可用局部值覆盖；图标为空时折叠图标区域（5px 图文间距全部由图标右侧 Margin 承载，折叠后不留残余间距，文字保持水平居中）。所有模式均读取 `atc:Icon.Icon`、`atc:Icon.FontFamily`、`atc:Icon.IconSize`（Boxed 默认 `8`、Borderless 默认 `14`）和 `atc:Icon.IconForeground`（默认跟随 `Foreground` 色系）；内容对齐落实 `HorizontalContentAlignment` / `VerticalContentAlignment`（样式默认 Center，拉伸或固定尺寸时内容按声明对齐）。

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

### CodeEditor

`jv:CodeEditor` 是完全封装 AvalonEdit 的代码编辑器控件：公共 API 只暴露 Junevy 类型，高级场景（折叠管理、自定义背景渲染、补全窗等）经 `InnerEditor` 逃生口取内部 `TextEditor` 实例。语法高亮按 `SyntaxLanguage` 加载 AvalonEdit 内置规则的**私有副本**（不改动全局共享定义），命名颜色统一映射到 `Theme.Brush.*` 主题画刷——关键字 = 主色（Cobalt）、类型词/属性名/选择器 = 次色（Coating）、字符串/字符 = 成功绿、数字 = 危险红、注释 = 三级灰、预处理指令 = 次级灰，深浅主题切换即时生效。外观口径与 `TextBox` 一致（发丝线边框、控件圆角/内边距令牌、聚焦 accent 边框、只读底色下沉、禁用蒙层）；等宽字体默认 `Consolas, Courier New`，可按实例覆盖。

```xml
<jv:CodeEditor Height="320" SyntaxLanguage="CSharp" ShowLineNumbers="True" />
```

| 依赖属性 | 说明 | 默认值 |
| --- | --- | --- |
| `Text` | 编辑器文本，双向绑定（`PropertyChanged` 节奏）；外部设置时整体替换文档（光标回起点、撤销栈清空，与 AvalonEdit 语义一致） | `""` |
| `SyntaxLanguage` | 语法高亮语言（`CodeLanguage` 枚举）：`None`/`CSharp`/`VisualBasic`/`Cpp`/`Java`/`JavaScript`/`Html`/`Css`/`Xml`/`Json`/`Sql`/`Python`/`Markdown`/`PowerShell`，运行时切换即时生效 | `None` |
| `ShowLineNumbers` | 是否显示行号（行号与分隔点线跟随 `Theme.Brush.Text.Tertiary`） | `True` |
| `IsReadOnly` | 只读态：内容不可编辑，底色下沉为 `Surface.Sunken` | `False` |
| `WordWrap` | 自动换行（换行时隐藏水平滚动条） | `False` |
| `CompletionProvider` | 补全提供者（`ICodeCompletionProvider`）：非空时启用补全——输入标识符字符/点号或 Ctrl+Space 弹出主题化补全弹窗；null 时关闭 | `null` |
| `InnerEditor`（只读属性） | 逃生口：内部 AvalonEdit `TextEditor` 实例，模板应用前为 `null` | — |

注意：属性名为 `SyntaxLanguage` 而非 `Language`——后者是 `FrameworkElement.Language`（xml:lang）的既有语义，刻意避开遮蔽。

#### 代码补全

核心库的补全是**挂载点设计**：`CodeEditor` 本身不带任何补全引擎，只提供 `ICodeCompletionProvider` 抽象（输入全文+光标 → 返回 `CodeCompletionItem` 列表）与主题化弹窗管线。宿主自行实现该接口即可接入任意引擎（关键词表、私有符号表、LSP 等），不挂载则零行为、零依赖：

```csharp
editor.CompletionProvider = new MyKeywordProvider(); // 任意 ICodeCompletionProvider 实现
```

**系统类 IntelliSense 用伴生包**（按需安装，会引入 Roslyn 依赖链——全链 34 包 / 约 40 MB 运行时程序集，不装则核心库保持仅 AvalonEdit 一个依赖）：

```bash
dotnet add package Junevy.Controls.CodeCompletion
```

```csharp
var provider = new Junevy.Controls.CodeCompletion.RoslynCodeCompletionProvider();
// 可选：追加宿主自有程序集/第三方库引用（须在首次补全前）
// provider.AdditionalReferences.Add(MetadataReference.CreateFromFile(@"...\MyLib.dll"));
editor.CompletionProvider = provider;
```

`RoslynCodeCompletionProvider` 引用 .NET 运行时目录中的基础类库（System.Console、System.Collections 等开箱即用，零额外包）；补全弹窗外观与主题画刷联动，深浅主题切换即时生效。交互语义：**弹窗随词存续**——词首字符开窗一次，词内续打由 AvalonEdit 内建的段追踪与子串过滤（`CompletionList.IsFiltering`）接管（不逐键重建窗口，替换语义正确且无闪烁）；点号切换到成员补全上下文；Esc 关闭；Ctrl+Space 强制弹出。已知限制：单文件发布形态不支持（MEF 需要按文件发现 Roslyn 程序集）；词内匹配为子串过滤而非 Roslyn 级模糊匹配；大文档逐键全量解析，超大文件建议配合只读或按需启用。

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

也可以在 `jv:ContextMenu` 内使用原生 `<MenuItem>`；默认样式会统一应用到子菜单。不要在上下文菜单中使用 `<jv:MenuItem>`，因为它是 `SideMenu` 的导航数据控件，不是 WPF 菜单项。

条目容器样式由库内 `JunevyMenuItemStyleSelector` 按条目类型分发（`MenuItem` → `JunevyContextMenuItemStyle`，`Separator` → 分隔符样式）——`Separator` 是 WPF `MenuBase` 的自带容器，若经 `ItemContainerStyle` 注入 TargetType=MenuItem 的样式，容器生成时会抛「样式不能应用于 Separator」直接闪退，因此库内条目样式一律走 `ItemContainerStyleSelector`。宿主自行设置 `ItemContainerStyle` 时菜单条目列表里不要再混放 `Separator`（`ItemsSource` 绑定场景不受影响——绑定列表里本就无法混放字面量分隔线）。

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

`jv:MenuItem` 是导航数据控件，继承 `ContentControl`，供 `SideMenu` 使用（`TreeView`（更名前 `TreeMenu`）自 `1.9.0` 起改用 `TreeMenuItem` 数据模型）。它与 WPF `MenuItem` 没有继承关系。

| 属性 | 效果 |
| --- | --- |
| `Title` | 导航标题 |
| `Icon` | 图标字体字符或任意内容 |
| `Orientation` | 图标与标题的排列方向 |
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
| `DisplayMode="Horizontal"` | 图标与标题横向排列（枚举 `SideMenuDisplayMode`） |
| `DisplayMode="Vertical"` | 紧凑图标模式（图标在上、标题在下），默认宽度调整为 `60`；隐藏 `Title` 即为纯图标导航栏 |
| `ItemHeight` | 固定项目高度；默认 `NaN`，使用内容自然高度 |
| `atc:Icon.FontFamily` | 所有菜单项图标字体 |
| `atc:Icon.IconSize` | 所有菜单项图标尺寸 |
| `ItemHoverBackground` | 菜单项悬停背景画刷；默认 `Surface.Base`（悬停项「浮起」变亮） |
| `SelectedItemBackground` | 选中项背景画刷；默认 `Surface.Sunken`（中性灰、无阴影）——浅色下与 `Background.Second` 同值，侧栏底色为 `Background.Second` 时选中填充不可见，可自定义 |

条目行为约定：

- **超宽标题省略号截断**：竖向导航禁用横向滚动条（`HorizontalScrollBarVisibility=Disabled`），测宽约束自视口逐层传入条目，长标题以 `CharacterEllipsis` 截断，不再把整条导航栏撑出横向滚动条；横向丝带菜单（`Orientation=Horizontal`）经样式触发器恢复 `Auto`，条目横向超出时滚动而非截断。
- **条目 ToolTip 与自动化名称随 `Title`**：`Title` 为空时不显示空 ToolTip。图标导航栏（隐藏 `Title`）若需要悬停提示，提示文本需另有来源（当前 `Title` 即提示来源，置空后无提示文案）。
- **`Icon` 与 `Title` 双空的条目整体隐藏**，不产生可点击的空白行。

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

选中与悬停的底色由两个画刷属性控制：

| 属性 | 默认 | 效果 |
| --- | --- | --- |
| `ItemHoverBackground` | `Surface.Base` | 悬停项「浮起」变亮 |
| `SelectedItemBackground` | `Surface.Sunken` | 选中项压暗（中性灰，无阴影、无强调蓝） |

需要自定义（如恢复强调蓝、品牌色或透明）时，直接在实例上设置画刷即可：

```xml
<jv:SideMenu ... SelectedItemBackground="{DynamicResource Theme.Brush.Surface.Selected}" />
```

选中项**不再套用阴影**（1.14.0 起移除；如需阴影可在 `ItemContainerStyle` 的 `IsSelected` 触发器中自行添加 `Effect`）。

### TreeView 与 TreeMenuItem

`jv:TreeView`（原 `jv:TreeMenu`，自 `TreeView` 更名起官方 `<TreeView>` 写法自动继承库样式）继承 WPF `TreeView`；`jv:TreeMenuItem` 是普通数据模型类（实现 `INotifyPropertyChanged`，不是控件），作为 `ItemsSource` 条目使用，由库内**唯一**的一份 `HierarchicalDataTemplate`（资源键 `TreeViewItemTemplate`）渲染 `Title` 与 `Icon`，层级由 `Children` 提供。数据模型不继承 `DispatcherObject`，可在任意线程构建。

两种行首观感共用这同一份条目模板——`DisplayMode` 只决定行首画箭头还是画指示条，**不再切换模板**（自 `3.2.0` 起 `TreeViewNormalItemTemplate` / `TreeViewIconItemTemplate` 合并为 `TreeViewItemTemplate`，两个旧键已不存在）。

合并 `Themes/Generic.xaml` 后，官方 `<TreeView>`（无 `jv:` 前缀）自动获得同一外观与交互；`jv` 专有能力经 `atc:TreeViewAssist` 附加属性承载，两类实例通用：

| 属性（`jv:TreeView` 实例 / `atc:TreeViewAssist` 附加） | 效果 |
| --- | --- |
| `DisplayMode`（枚举 `TreeViewDisplayMode`） | `Chevron`（默认）显示可点击的展开箭头；`Indicator` 折叠箭头，改由「选中的有子项节点」左侧的 accent 指示条标示层级归属。**两种观感都渲染图标与标题**，图标字号一律取 `atc:Icon.IconSize`。类型与取值自 `3.2.0` 更名（原 `DisplayMode` 的 `Normal` / `Icon`） |
| `IndentSize` | 子级相对本级的缩进宽度（DIP），默认 `10`（对应官方 `TreeViewItem` 的行首缩进观感）。经库内 `cvt:IndentSizeToMarginConverter` 落为子级承载区的左内缩 `Margin`，逐级累加，只影响子级、不影响本级行首 |
| `AutoExpandAncestors` | 选中节点时是否自动展开其所有祖先，默认 `true`；置 `false` 后宿主需自行展开。展开由容器自身的 `Selected` 事件驱动，只对**容器已生成**的节点生效——选中一个尚未生成容器的深层节点时，宿主仍需先展开路径上的祖先（或先 `jv:TreeView.ExpandAll()` 展开全部），见 [ExpanderBehavior](#expanderbehavior) 的生效边界 |
| `NavigateCommand` | **选中驱动**：选中项变化（单击、键盘 `↑`/`↓`、程序化 `IsSelected=true`）即执行，参数为新选中的数据项（模型对象，非容器）；清除选中与 `ExpandAll()`/`CollapseAll()` 引起的选中变化不触发；`CanExecute=false` 静默跳过；命令处理器内同步改选中不会二次触发（重定向请派发到 Dispatcher 队列） |
| `ItemHoverBackground` | 节点悬停背景画刷。**未设置或显式设 `null` 时回退到令牌 `Theme.Brush.Surface.Hover`**（模板的回退触发器，`DynamicResource` 引用，随主题切换） |
| `SelectedItemBackground` | 选中节点背景画刷。同样在 `null` 时回退到 `Theme.Brush.Surface.Sunken` 中性灰——宿主替换 `Style` 或把画刷置空时高亮不再整体消失 |

| `jv:TreeView` 实例方法 | 效果 |
| --- | --- |
| `ExpandAll()` | 递归展开 `Items` 里每个 `TreeMenuItem`（写数据模型，虚拟化下尚未生成的容器同样被覆盖），并把上一次 `CollapseAll()` 期间被上提的选中还原回原来的深层节点。条目不是 `TreeMenuItem` 时展开不生效——自定义模型需在自己的数据类上实现等价的递归展开 |
| `CollapseAll()` | 递归收起 `Items` 里每个 `TreeMenuItem`，并记住被 WPF 原生语义上提前的选中节点，供紧随的 `ExpandAll()` 还原 |

WPF 基类 `TreeView` **没有**这两个实例方法，官方 `<TreeView>` 只能逐节点调用 `TreeMenuItem.ExpandAll()` / `CollapseAll()`（不含选中还原）。

| `TreeMenuItem` 成员 | 效果 |
| --- | --- |
| `Children` | 子节点集合，构造时自动初始化，直接 `Add` 即可更新视图 |
| `IsExpanded` | 展开/收起状态，与容器 `TreeViewItem.IsExpanded` 双向绑定，可直接赋值控制节点展开；状态保存在数据模型上 |
| `IsSelected` | 选中状态，与容器 `TreeViewItem.IsSelected` 双向绑定，可直接赋值选中节点；赋值时若该节点容器尚未生成（祖先从未展开），需宿主先展开祖先，见上文 `AutoExpandAncestors` 的生效边界 |
| `Title` / `Icon` | 节点标题 / 图标（iconfont 字形或任意内容） |
| `ExpandAll()` / `CollapseAll()` | 递归展开 / 收起本节点及所有后代。**直接写数据模型而非遍历容器**，因此虚拟化下尚未生成的容器同样被覆盖；收起全部后再展开全部，深层节点的 `IsExpanded` 与容器一致。整树递归请用上面的 `jv:TreeView.ExpandAll()` / `CollapseAll()`（额外负责选中还原） |

| 其他附加属性 | 效果 |
| --- | --- |
| `atc:Icon.FontFamily` | 节点图标字体 |
| `atc:Icon.IconSize` | 节点图标字号（`Chevron` 与 `Indicator` 两种观感都生效；`3.2.0` 修复前 `Chevron` 模式完全不读取该值） |
| `atc:ExpanderBehavior.Enable` | 默认容器样式已启用；控制双击/`Enter` 展开切换、键鼠来源守卫、选中时展开祖先 |

```csharp
public ObservableCollection<TreeMenuItem> NavigationTree { get; } =
[
    new TreeMenuItem
    {
        Title = "Camera",
        Icon = "\uE66B",
        Children =
        {
            new TreeMenuItem { Title = "Live View", Icon = "\uE66B" },
            new TreeMenuItem { Title = "Settings", Icon = "\uE60F" }
        }
    }
];
```

```xml
<!-- jv:TreeView：实例属性直接设置 -->
<jv:TreeView
    x:Name="NavTree"
    atc:Icon.IconSize="18"
    DisplayMode="Chevron"
    IndentSize="12"
    ItemsSource="{Binding NavigationTree}"
    NavigateCommand="{Binding NavigateCommand}" />

<!-- 官方 <TreeView>：扩展能力经附加属性，外观与交互一致 -->
<TreeView
    atc:Icon.FontFamily="{DynamicResource IconFont}"
    atc:Icon.IconSize="18"
    atc:TreeViewAssist.DisplayMode="Indicator"
    atc:TreeViewAssist.IndentSize="12"
    atc:TreeViewAssist.AutoExpandAncestors="True"
    ItemsSource="{Binding NavigationTree}"
    atc:TreeViewAssist.NavigateCommand="{Binding NavigateCommand}" />
```

```csharp
// 整树展开 / 收起：递归写在数据模型上，虚拟化下尚未生成的容器同样被覆盖。
// CollapseAll 会记住被 WPF 原生语义「上提到祖先」的选中，紧随的 ExpandAll 把它还原回原来的深层节点。
NavTree.CollapseAll();
NavTree.ExpandAll();

// 官方 <TreeView> 没有这两个实例方法（WPF 基类未提供），逐根节点递归，不含选中还原：
foreach (var node in NavigationTree)
{
    node.ExpandAll();      // 或 node.CollapseAll();
}

// 已生成容器的节点：直接写选中即可，祖先由库自动展开（AutoExpandAncestors 默认 true）
node.IsSelected = true;

// 按路径选中「祖先还没展开过、容器尚未生成」的深层节点：先展开路径上的祖先，再写选中
foreach (var ancestor in ancestors)   // ancestors = 由根到该节点父级的各层，宿主查路径时顺手得到
{
    ancestor.IsExpanded = true;
}
node.IsSelected = true;
```

展开图标为 **ExpanderPanel 同款旋转箭头**（iconfont 字形 `&#xE650;`，展开旋转 90°，悬停加深、禁用置灰），宿主为 WPF 基类 `ToggleButton`（`ClickMode=Press`、`Focusable=False`，与官方 TreeViewItem 模板一致），不再是库内 `jv:ToggleButton` 开关样式；`HasItems=False` 的叶节点隐藏箭头占位，`Indicator` 观感折叠箭头——**蓝色 accent 指示条仅标示「选中的带子项节点」**（蓝色提示 = 真实选中，非选中分支不显示；所有行经隐藏占位预留指示条槽位，选中时布局不跳动）。

默认模板（`TreeViewItemTemplate`）按 `TreeMenuItem` 的约定渲染（`Title` / `Icon` / `Children`）。使用其他数据类型时，为 `TreeView` 提供自定义 `HierarchicalDataTemplate`（含 `ItemsSource` 绑定）即可；展开/激活行为按容器 `HasItems` 判断叶/枝，与数据类型无关，但展开/选中状态的保留要求条目暴露 `IsExpanded` / `IsSelected` 并在 `ItemContainerStyle` 里绑定（见 [ExpanderBehavior](#expanderbehavior)）。`TreeMenuItem` 不再提供 `TargetType` 之类的业务元数据属性（零消费方，`3.2.0` 起删除）——导航目标由宿主自己的模型或 `NavigateCommand` 参数承担。容器样式经 `ItemContainerStyle` 应用后沿层级递归传递到所有层级的 `TreeViewItem`。

交互约定：

- 单击选中节点（选中变化即触发 `NavigateCommand`，参数=数据项）；双击文件夹节点切换展开/收起；双击任意节点触发 `ItemDoubleClick` 路由事件（携带数据项；双击展开箭头不触发）。
- 键盘方向键沿用 WPF `TreeView` 原生行为：`↑`/`↓` 移动选择（选中变化即触发 `NavigateCommand`），`→`/`←` 展开/收起；`Enter` 切换分支展开（`jv:TreeView` 上不再执行命令——导航由选中驱动承担）。`Enter` 与双击都带**来源守卫**：焦点在行内子控件（编辑框、下拉、按钮）上时由该控件自己处理，行首箭头的单击也不会被双击重复触发。
- 悬停、选中、禁用三种视觉状态使用主题色区分，并作用于整行；长列表自动显示垂直滚动条。
- **悬停高亮仅作用于鼠标所在的行**：`IsMouseOver` 会随可视子树向上传染（悬停子行时父/祖先行的 `IsMouseOver` 也为真），模板以「子级承载区（`ItemsPresenter`）不悬停」为附加条件过滤——悬停子行时父/祖行不再误高亮，悬停父行自身仍正常高亮。
- 条目文字与图标的前景色跟随宿主 `Foreground`（默认为随主题切换的 `Text.Secondary`，禁用态使用 `State.DisabledForeground`）。系统 `TreeViewItem` 默认样式会把前景钉在恒黑的 `ControlTextBrush` 上（优先级高于属性继承），因此默认容器样式显式绑定宿主前景：在实例（`jv:TreeView` 或官方 `<TreeView>`）上设置 `Foreground` 会同步传导到所有层级的条目，撤销覆盖（`ClearValue`）后恢复主题色。
- **层级缩进由 `IndentSize`（默认 `10` DIP）驱动**：子级承载区（模板部件 `ItemHost`，即 `ItemsPresenter`）取左内缩 `Margin(l,0,0,0)`，逐级累加；`TreeViewAssist.IndentSize` 与 `jv:TreeView.IndentSize` 是同一 DP，官方 `<TreeView>` 也能用。默认样式不再有 `MaxWidth ← ActualWidth` 的钳制（`3.2.0` 移除：该绑定构成布局反馈环，折叠再展开时子树首次测量拿到 `MaxWidth=0` 会整棵消失）。
- **收起会把选中上提（WPF 原生语义，非本库行为）**：收起一条内含选中子孙的分支时，WPF 会把选中移到**最外层被收起的祖先**（三级 `root / branch / leaf` 实测落到 `root`，且全树只剩一个选中容器），本库因把 `IsSelected` 与容器双向绑定，模型也会如实变成「分支选中、原叶子未选中」。原生 `<TreeView>` 在同一场景判定完全一致（探针 C7 与一棵 `Style=null` 的纯 WPF 树同场对照）。`jv:TreeView.CollapseAll()` 会记住上提前的节点，紧随的 `jv:TreeView.ExpandAll()` 把选中还原回该节点；两者之间宿主自己改过选中则这次还原作废（探针 D1/D2/D3/D4）。逐节点的 `TreeMenuItem.CollapseAll()` 不承担这项还原。
- 根级与嵌套层级均使用 `VirtualizingStackPanel`，容器按需实例化；**本库刻意不复用 `TreeViewItem` 容器**（`VirtualizationMode` 取框架默认 `Standard`，不做 `Recycling`），展开/选中状态保存在数据模型上。需要留意 WPF 的实际行为：**收起分支只是隐藏子级承载区，已生成的容器仍留在可视树里**（探针 C2 以此断言状态写回），子级容器要等首次展开才生成，生成时由双向绑定从模型取状态——因此模型才是唯一权威，也正因为不回收容器，才不存在「复用容器把上一节点状态串到新节点」的问题，样式里无须重复设置 `VirtualizingPanel.IsVirtualizing`（框架默认已为 `true`）。

### TabControl 与 TabControlItem

`jv:TabControl` 继承 WPF `TabControl`，`jv:TabControlItem` 继承 WPF `TabItem`。它遵循标准的 `ItemsSource`、`ItemTemplate`、`ContentTemplate` 和容器生成规则；点击页签标题会切换对应内容。页签条是 `ScrollViewer`（`HorizontalScrollBarVisibility=Auto`）内的一行横向 `StackPanel`——**页签过多时横向滚动，不会自动换行**（滚动容器给面板无限宽度，换行永远不会发生），且页签条固定在顶部，因此**不支持 `TabStripPlacement` 的 Left/Bottom/Right 方向**（官方 `TabPanel` 能力未包含）。

**官方类型接管**：库字典中提供了官方 `TabControl` 的隐式样式（与 `ListBox` / `ComboBox` 等同一约定）。宿主把 `Themes/Generic.xaml` 合并进 `Application.Resources` 后，**原生 `<TabControl>` 无需任何前缀与配置即被库主题接管**：外壳、页签行与悬停/选中态与 `jv:TabControl` 一致，页签容器为原生 `TabItem`（`DefaultTabItemStyle`）。两个圆角属性注册为**附加属性**，因此官方 `<TabControl>` 同样可自定义：`jv:TabControl.HeaderCornerRadius="6"`、`jv:TabControl.ContentCornerRadius="0,0,12,12"`（未赋值时取默认值；官方类型下不再产生绑定失败跟踪——此前那条 `Warning 40` 只在宿主开启 `PresentationTraceSources.DataBindingSource` 跟踪时可见，默认 `Level=Off`）。`IsClosable`、`CloseTabCommand`、页签重命名与 `ItemHoverBackground` / `SelectedItemBackground` 仍是 `jv:TabControl` 的派生能力，官方 `<TabControl>` 不具备——需要这些能力请使用 `jv:TabControl` 与 `jv:TabControlItem`（原 `TabMenu` / `TabMenuItem` 自 3.1.0 起更名）。

内容区对齐遵循 WPF `TabControl` 官方契约：每次选中变化时，选中页签容器的 `HorizontalContentAlignment` / `VerticalContentAlignment` 会被同步到内容承载器（`PART_SelectedContentHost`）上。默认容器样式取 `Stretch`，内容（含 `ItemsSource` + `ContentTemplate` 用法与直接声明 `TabControlItem` 的自容器用法）自动撑满内容区；页签标题的对齐固定为左对齐垂直居中，不随这两个属性变化。若在 `ItemContainerStyle` 中把两个对齐属性改为非 `Stretch`，内容区将随之收缩为内容自然尺寸——这是原生 `TabControl` 的行为，不是缺陷。

| 属性/事件 | 效果 |
| --- | --- |
| `CanCloseLastTab` | 是否允许关闭最后一个页签，默认 `true` |
| `CanRename` | 控件级双击重命名开关，默认 **`false`**（默认不开放双击改名，需显式设 `True`）；能否进入重命名由两级开关共同决定——控件级 `TabControl.CanRename` 与条目级 `TabControlItem.CanRename` **同时为 `true`** 才允许 |
| `DisposeContentOnClose` | 关闭页签时的内容释放开关，默认 `false`（库**不做任何清理**，生命周期由调用方管理）。开启后两类路径都释放：**直接声明页签**（页签自身即数据项）清空 `Content` / `DataContext` 并对内容本身与内容元素 `DataContext` 中实现 `IDisposable` 的部分调用 `Dispose()`（默认关闭态下同一自容器页签可重新加回 `Items`）；**`ItemsSource` 条目**（条目即数据模型）对模型本身或条目元素 `DataContext` 中实现 `IDisposable` 的部分调用 `Dispose()`（容器随条目移除一并丢弃；释放发生在从集合移除之前——条目移除时生成器会清掉容器 `Content`） |
| `HeaderCornerRadius` | 页签头圆角，默认 `4`。**只接受上两角**：模板经筛选器丢弃下两角（与内容区衔接），因此 `HeaderCornerRadius="5,5,0,0"` 有效、`="0,0,5,5"` 会得到直角。**附加属性**：既可写在 `jv:TabControl` 上（`HeaderCornerRadius="6"`，C# 实例属性同名不变），也可写在官方 `<TabControl>` 上（`jv:TabControl.HeaderCornerRadius="6"`） |
| `ContentCornerRadius` | 内容区域圆角，默认 `8`。**只接受下两角**（上两角被丢弃），四角写法 `ContentCornerRadius="0,0,5,5"` 与 `"5"` 等价，`="0,0,3,9"` 可让左下 `9`、右下 `3` 分别生效；附加写法同上 |
| `IsClosable` | 是否显示关闭按钮，默认 `true`（仅影响外观，页签仍可通过 `CloseTab` / `CloseTabCommand` 关闭） |
| `TabClosing` | **路由事件**（`TabControl.TabClosingEvent`，`RoutingStrategy.Direct`，委托 `TabCloseEventHandler(object sender, TabCloseEventArgs e)`），关闭前派发；置 `TabCloseEventArgs.Cancel=true` 可取消 |
| `TabClosed` | **路由事件**（`TabControl.TabClosedEvent`，同一委托），页签已关闭后派发；`Cancel` 在本事件中无效 |
| `CloseTab(TabControlItem)` | 通过代码关闭指定页签（不属于本控件、或该页签处于编辑态时忽略） |
| `CloseTabCommand` | `TabControl` 上的静态 `RoutedCommand`，页签关闭按钮即绑定它；`CommandParameter` 传入 `TabControlItem` 时**优先**于「经事件源 / 可视树向上查找」的页签定位方式——宿主可在内容区自行发起命令并指定要关闭哪个页签 |
| `TabControlItem.Icon` | 页签图标 |
| `TabControlItem.IsEditing` | 双击文字标题进入编辑时的只读状态 |
| `TabControlItem.CanRename` | 条目级是否允许双击标题重命名，默认 `true`；设为 `false` 后双击不再进入编辑态，编辑中被禁用会立即退出编辑并保留当前文本。还需控件级 `TabControl.CanRename` 同为 `true` 才生效。双击仅在页签头自身的可视子树内触发重命名——内容区或嵌套控件的双击不会误触 |

关闭与选中行为：

- **选中位置跟随浏览器习惯**：关闭中间页签时选中停在同一位置（后一个页签顶上来），关闭最后一个页签时退回前一个（实现为 `Math.Min(index, Items.Count - 2)`）；`CanCloseLastTab=True` 关掉仅剩页签后无选中页签。
- **`TabClosing` / `TabClosed` 是可订阅的 WPF 路由事件**：经 `EventManager.RegisterRoutedEvent` 注册，`TabCloseEventArgs` 继承 `RoutedEventArgs`，因此 `e.Source`、`e.RoutedEvent` 均有值，也可用 `AddHandler` 在更上层挂接。路由策略刻意取 `Direct` 而非库内其他事件常用的 `Bubble`：关闭语义只属于发起页签所在的这台 `TabControl`，页签内容区里可嵌套另一台 `TabControl`，冒泡会让父级收到子级页签的关闭事件、父级的 `Cancel` 便可能误拦与它无关的关闭。
- **`ItemsSource` 不可写时不再静默失败**：数据源无法移除项时（`IList.IsReadOnly` 或 `IList.IsFixedSize` 为真——数组属于后者：它的 `IsReadOnly` 返回 `false`，但 `Remove` 必抛 `NotSupportedException`），关闭会抛 `InvalidOperationException` 并给出可执行的提示，而不是什么都不发生。`ObservableCollection<T>` 等可写、非定长的 `IList` 源仍由控件直接移除数据项。需要自管移除时机（或数据源不可写）时，走 `TabClosing` + `Cancel` 模式：

```xml
<jv:TabControl ItemsSource="{Binding Editors}" TabClosing="OnTabClosing" />
```

```csharp
private void OnTabClosing(object sender, TabCloseEventArgs e)
{
    if (e.Tab.DataContext is EditorItem item)
    {
        Editors.Remove(item);   // 从宿主自己的集合移除数据项
        e.Cancel = true;        // 页签消失由集合变更驱动，控件不再二次移除
    }
}
```

- **无障碍**：页签关闭按钮（模板部件 `PART_CloseButton`）带 `AutomationProperties.Name="关闭页签"`——按钮呈现的是 Unicode 私用区的图标字体字形，不命名时屏幕阅读器只会念出那个未分配码点。

直接声明页签：

```xml
<jv:TabControl CanCloseLastTab="False">
    <jv:TabControlItem Header="Camera 1" Icon="&#xE66B;">
        <local:CameraView />
    </jv:TabControlItem>
    <jv:TabControlItem Header="Logs">
        <local:LogView />
    </jv:TabControlItem>
</jv:TabControl>
```

绑定普通数据集合时，标准 `ItemTemplate` 控制页签标题，`ContentTemplate` 控制选中项内容：

```xml
<jv:TabControl ItemsSource="{Binding Editors}">
    <jv:TabControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Title}" />
        </DataTemplate>
    </jv:TabControl.ItemTemplate>
    <jv:TabControl.ContentTemplate>
        <DataTemplate>
            <ContentPresenter Content="{Binding View}" />
        </DataTemplate>
    </jv:TabControl.ContentTemplate>
</jv:TabControl>
```

只有需要把 `Icon` 等容器属性绑定到 ViewModel 时，才派生默认容器样式：

```xml
<jv:TabControl.ItemContainerStyle>
    <Style BasedOn="{StaticResource DefaultTabControlItemStyle}" TargetType="{x:Type jv:TabControlItem}">
        <Setter Property="Icon" Value="{Binding Icon}" />
    </Style>
</jv:TabControl.ItemContainerStyle>
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

条目布局约定：分组触发条目 `DisplayMode=IconAndTitle` 时条目高 60（`IconOnly` 保持默认 48）；弹层内 `ToolItem` 高 68（`IconOnly` 收缩为 40，悬停底覆盖整行）。容器模板常驻 `ScrollViewer`——分组条目超出工具箱高度时自动滚动（默认与紧凑档行为一致）；竖向导航式分组列不出现横向滚动条。键盘焦点使用库内焦点环样式（`DefaultControlFocusVisualStyle`）。

容器滚动沿用 WPF `ItemsControl` 的官方约定，三项均由样式提供、模板经 `TemplateBinding` 读取，宿主可在实例或自定义样式上覆盖：`ScrollViewer.VerticalScrollBarVisibility`（默认 `Auto`，紧凑档 `CompactToolboxStyle` 覆盖为 `Hidden`）、`ScrollViewer.HorizontalScrollBarVisibility`（默认 `Disabled`）、`ScrollViewer.CanContentScroll`（默认 `False`，即像素级滚动）。把 `ScrollViewer.CanContentScroll` 置 `True` 可回到按条目滚动——`ItemsPanel` 已是 `VirtualizingStackPanel`，此时条目才会真正进入虚拟化。

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

库内提供两组紧凑样式，将一级分组触发器压缩为**约 25 DIP 宽的窄列**（图标 12 DIP、条目最小 21×40 DIP、内边距/外边距收窄；Popup 内的 `ToolItem` 保持默认尺寸不变）：

- `CompactToolboxItemStyle`：紧凑的 `ToolboxItem` 项样式（宽度自适应拉伸，`MinWidth=21` 与窄列联动）。
- `CompactToolboxStyle`：紧凑的 `Toolbox` 容器样式——`Padding=1`、`MaxWidth=25` 兜底、默认使用紧凑项样式；窄列容不下竖向滚动条，条目溢出时滚动条隐藏（`ScrollViewer.VerticalScrollBarVisibility=Hidden`），滚轮与键盘仍可滚动。

```xml
<jv:Toolbox Style="{StaticResource CompactToolboxStyle}" ... />
<!-- 或保持默认容器样式，仅替换分组项样式 -->
<jv:Toolbox ItemContainerStyle="{StaticResource CompactToolboxItemStyle}" ... />
```

注意：在实例上以 `ItemContainerStyle` 指定自定义分组项样式会覆盖 `CompactToolboxStyle` 默认的紧凑项样式——容器的 `MaxWidth=25` 仍会钳住窄列宽度，但条目样式内固定的 `Width` / `MinWidth` 大于窄列可用宽度（约 21 DIP）时，图标会被裁剪。自定义项样式建议 `BasedOn="{StaticResource CompactToolboxItemStyle}"` 并避免固定宽度。


`Toolbox` 容器（`DefaultToolboxStyle` 与 `CompactToolboxStyle` 的根 `Border`）取**直角**，圆角令牌是专用的 `Theme.ToolboxCornerRadius`（默认 `0`）。原因：WPF 的 `Border` 圆角只裁自己的背景/描边，不裁子元素——角上要么是一个透明缺口（容器有 `Padding`，内容让开了角，于是漏出容器背后的表面；与左侧接壤的控件并排时就是一个小角），要么是内容顶在圆弧外的方角。需要恢复圆角时在 `Application.Resources` 覆盖该键即可，不会影响全局 `Theme.ControlCornerRadius`：

```xml
<Application.Resources>
    <CornerRadius x:Key="Theme.ToolboxCornerRadius">6</CornerRadius>
</Application.Resources>
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

布局经 `Mode` 枚举（`AppBarMode`）切换，隐式样式按值换模板（也可继续经 `Template="{StaticResource ...}"` 显式指定同名模板键）：

| `Mode` | 布局 | 使用的内容属性 |
| --- | --- | --- |
| `Default`（默认） | 图标 + 标题 / 分隔线 / 工具栏，右侧系统按钮 | `ToolBar` |
| `MenuBar` | 单行：图标 + 应用名 + 菜单栏 + 弹性空白 + 系统按钮 | `Menu` |
| `Expandable` | 抽屉开关（最左）+ 整条居中标题 + 系统按钮（最右） | `Drawer` |

| 属性/附加属性 | 效果 |
| --- | --- |
| `Content` | 应用标题或任意标题内容 |
| `ToolBar` | `jv:ToolBar` 实例；仅 `DefaultAppBar` 呈现 |
| `Menu` | WPF `Menu` 实例；仅 `MenuBarAppBar` 呈现 |
| `atc:Icon.Icon` | 左侧应用图标，可使用图标字体或 `Image` |
| `atc:Icon.FontFamily` | 应用图标和标题栏系统按钮字体 |
| `atc:Icon.IconSize` | 左侧应用图标区高度（同时为最小宽度）；内容宽于该值时（宽幅 Logo 图）图标区自动加宽，`Image` 不写显式宽高即按该高度等比缩放 |
| `Foreground` | 标题和应用图标颜色；图片本身不受影响 |
| `Mode` | 布局模式（见上表），默认 `Default` |
| `Drawer` | 抽屉内容（任意 `object`，尺寸由内容决定），仅 `Expandable` 呈现；为 `null` 时抽屉开关自动隐藏 |
| `IsDrawerOpen` | 抽屉是否展开（可双向绑定驱动）；点抽屉外部收起时自动写回 `false` |
| `AutoCloseOnDrawerClick` | 点击抽屉内容中未被处理的左键（列表项、菜单项）时自动收起抽屉；按钮等已处理点击的控件不触发 |
| `DrawerToggleIcon` | 抽屉开关按钮的 iconfont 字形（默认菜单字形） |
| `AppBarCaptionIconSize`（资源键） | 标题栏系统按钮（最小化 / 最大化 / 还原 / 关闭）与抽屉开关的图标字号，默认 `15`；图标字体统一为满幅 em（墨迹边长=字号）后，该值直接决定字形视觉尺寸，可在应用级 ResourceDictionary 覆写 |

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

图标区高度固定为 `IconSize`、最小宽度同为 `IconSize`：图标字体字形与方形图片的布局不受影响，宽幅 Logo 图（如字标图、3:1 横图）无需显式宽高，按高度等比缩放、图标区自动加宽；源图远小于显示尺寸差异大时建议设置 `RenderOptions.BitmapScalingMode="HighQuality"` 保证缩小画质。

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

`Mode=Expandable` 的抽屉经 `Popup` 从标题栏下缘向下悬浮（标题栏是布局元素，无法把展开区推进宿主的行里，故不复用 `ExpanderPanel` / `SidePanel`）：点抽屉外部收起，再次点击开关同样收起（`StaysOpen=False` 收起时会放行落点鼠标消息、使开关再收到一次 Click，控件内置 250ms 时间窗守卫把这次余波忽略掉，不会重复展开）。标题跨三列真居中——左右两侧宽度不等时依然居中。拖动仍交由宿主 `WindowChrome` caption 区，建议 `CaptionHeight="{Binding ActualHeight, ElementName=AppBar 实例名}"` 与栏高保持同步。

依赖：所在 `Window`、WPF `SystemCommands`（按钮命令绑定由 `AppBar` 自动补齐）、内置图标字体、`Button` 样式；`DefaultAppBar` 另依赖 `ToolBar`，`MenuBarAppBar` 另依赖 WPF `Menu`/`MenuItem`、`Separator` 与 `ContextMenu` 系列样式。自定义无边框窗口时仍需由应用配置 `WindowChrome`、`WindowStyle` 和拖动区域。

### InfoBar

`jv:InfoBar` 用户信息条：左侧头像（`AvatarSource` 图片，圆形裁切；未设置时显示 `UserName` 首字符的圆形字标），`Text` 布局（默认）在头像右侧显示名称与可选的设置按钮，`AvatarOnly` 布局仅显示头像、悬停经 ToolTip 显示名称。两种布局点击都会弹出**与控件等宽**的菜单：菜单项可在 XAML 中直接编写（作为控件的 `Items`），也可经 `ItemsSource` 绑定一个列表（项外观用 `ItemTemplate` 定制）；菜单经 `Popup` 悬浮，默认向上展开（适合侧栏 / 标题栏底部，`MenuPlacement` 可换），点菜单外部或再次点击收起；`AutoCloseOnMenuClick="True"`（默认）时点击菜单内的按钮项自动收起。菜单在**鼠标抬起**时弹出（按下阶段开 Popup 会与点击手势冲突）。设置按钮点击冒泡 `SettingsClick` 路由事件；`AvatarOnly` 布局指定宽度时头像居中显示。**数据项容器**：经 `ItemsSource` 绑定的字符串 / 模型项自动包装为整行 `Button`（全宽左对齐、悬停纱色，可用 `ItemContainerStyle` 覆盖），点击冒泡 `ButtonBase.Click`（同时触发自动收起，宿主可在 InfoBar 上经 `OriginalSource.DataContext` 区分菜单项）；XAML 中直接编写的元素（`jv:Button`、分隔线等）按原样使用、不再包装。

| 属性 | 默认值 | 效果 |
| --- | --- | --- |
| `UserName` | `null` | 用户名称；Text 布局显示、AvatarOnly 布局进 ToolTip，并决定字标首字符（属性不叫 `Name`——与 `FrameworkElement.Name` 冲突，XAML 里会被当成元素名） |
| `AvatarSource` | `null` | 头像图片（圆形裁切）；为 `null` 时显示首字符字标 |
| `DisplayMode` | `Text` | `Text`（头像 + 名称 + 设置按钮）/ `AvatarOnly`（仅头像） |
| `ShowSettingsButton` | `true` | 是否显示设置按钮（Text 布局），点击冒泡 `SettingsClick` |
| `IsMenuOpen` | `false` | 菜单是否展开（可双向绑定驱动）；点菜单外部收起时自动写回 `false` |
| `MenuPlacement` | `Top` | 菜单弹出方位（`PlacementMode`） |
| `AutoCloseOnMenuClick` | `true` | 点击菜单内的按钮项（Click 冒泡到菜单宿主）时自动收起菜单 |
| `Items` / `ItemsSource` / `ItemTemplate` | — | 继承自 `ItemsControl`：菜单项 XAML 编写或列表绑定 |
| `MenuContent` | `null` | 弹层改为任意面板内容（`ListBox`、`UserControl`、复杂布局均可），设置后替代菜单项列表；内容与 InfoBar 共享 `DataContext` |
| `MenuWidth` | `NaN` | 弹层宽度；`NaN` 时与控件等宽，设置后覆盖等宽约束 |
| `MenuMaxHeight` | `NaN` | 弹层最大高度；列表项较多时建议设置，避免超出屏幕 |

```xml
<jv:InfoBar
    Width="280"
    UserName="xuhill07"
    AutoCloseOnMenuClick="True"
    SettingsClick="OnInfoBarSettingsClick">
    <jv:Button Content="&#xE60F; 应用设置" HorizontalContentAlignment="Left"
               Style="{StaticResource NoBorderButtonStyle}" />
    <jv:Button Content="&#xE639; 退出登录" HorizontalContentAlignment="Left"
               Style="{StaticResource NoBorderButtonStyle}" />
</jv:InfoBar>

<!-- 绑定列表 + 图片头像 + 仅头像布局 -->
<jv:InfoBar
    Width="280"
    DisplayMode="AvatarOnly"
    UserName="xuhill07"
    AvatarSource="pack://application:,,,/Junevy.Controls;component/Resources/Pictures/Author.png"
    ItemsSource="{Binding MenuItems}"
    ItemTemplate="{StaticResource InfoBarMenuItemTemplate}" />

<!-- 面板模式:MenuContent 放任意内容(复杂布局 / UserControl 皆可) -->
<jv:InfoBar Width="280" UserName="操作面板" MenuWidth="360" MenuMaxHeight="240">
    <jv:InfoBar.MenuContent>
        <StackPanel>
            <TextBlock Margin="8,8,8,4" FontWeight="Bold" Text="快捷操作" />
            <ListBox BorderThickness="0" ItemsSource="{Binding Tasks}" />
        </StackPanel>
    </jv:InfoBar.MenuContent>
</jv:InfoBar>
```

面板内容与 InfoBar 共享 `DataContext`，内部按钮点击冒泡 `ButtonBase.Click`（`AutoCloseOnMenuClick=true` 时自动收起菜单）。`AppBar Mode="Expandable"` 的抽屉（`Drawer` 属性）本就接受任意面板内容，两者一致。

## 布局控件

### ExpanderPanel

`jv:ExpanderPanel` 继承 WPF `HeaderedContentControl`，提供可折叠的头部与内容区，支持平滑的展开/折叠过渡动画，并可通过模板重写、命令绑定与依赖属性绑定无缝集成到现有项目。`DisplayMode` 提供两种头部形态：**经典模式**（默认，窄条头部，四方向展开）与**卡片模式**（高头部卡片，内容向下展开）。

| 属性/事件/方法 | 默认值 | 效果 |
| --- | --- | --- |
| `Header` | `null` | 头部内容；经典模式即整条头部，卡片模式作为加粗标题（点击头部切换展开/折叠） |
| `Content` | `null` | 内容区 |
| `IsExpanded` | `true` | 是否展开；支持双向绑定 |
| `DisplayMode`（依赖属性） | `Classic` | 头部形态：`Classic` 窄条头部（旋转箭头 + Header，支持四方向）；`Card` 卡片式高头部——左侧图标 + 标题/补充说明 + 右侧扩展槽 + 展开箭头，整卡带底色描边，内容向下展开（`ExpandDirection` 不生效） |
| `Icon`（依赖属性） | `null` | 卡片模式头部左侧图标：iconfont 字形或任意内容；为 `null` 时图标槽折叠不占位。字体族/字号经 `atc:Icon.FontFamily` / `atc:Icon.IconSize` 配置（卡片默认 `20`） |
| `Description`（依赖属性） | `null` | 卡片模式标题下方的补充说明（次级色小号文字）；为 `null` 时整体折叠，头部自适应变矮 |
| `HeaderExtra`（依赖属性） | `null` | 卡片模式头部右侧扩展槽：可放开关、按钮等控件。槽内交互控件独立命中（按下即处理事件，不会误触发展开/折叠），空白区域点击仍穿透到头部照常切换 |
| `ExpandDirection` | `Down` | 展开方向（经典模式），与 WPF `Expander` 语义一致：`Down` 头部在上、内容向下展开；`Up` 头部在下、内容向上展开；`Left` 头部在右、内容向左展开；`Right` 头部在左、内容向右展开 |
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

卡片模式示例（`Header` 复用为标题；右侧开关独立于展开切换）：

```xml
<jv:ExpanderPanel
    DisplayMode="Card"
    Icon="&#xE60F;"
    Header="自动曝光"
    Description="启用后由相机驱动自动调整曝光时间与增益">
    <jv:ExpanderPanel.HeaderExtra>
        <StackPanel Orientation="Horizontal">
            <jv:ToggleButton IsChecked="{Binding AutoExposure}" />
            <TextBlock Margin="8,0,0,0" VerticalAlignment="Center" Text="{Binding AutoExposureText}" />
        </StackPanel>
    </jv:ExpanderPanel.HeaderExtra>
    <TextBlock Text="曝光策略内容区。" />
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
| `CornerRadius` | 主题 `Theme.ControlCornerRadius`（浅/深色均为 `6`） | 面板本体圆角，四个角可分别设置：`CornerRadius="4,4,4,4"` 按 **左上、右上、右下、左下** 顺序取值（第 3 位是右下、第 4 位是左下，与 `Border.CornerRadius` 一致）；单值 `"12"` 等价 `"12,12,12,12"`；`"0"` 为四角直角；`Side="Right"` 配 `"18,0,0,18"` 得到「贴住容器右侧的两角直角、自由边大圆角」。四角完全自由，不按 `Side` 筛选。行内属性可直接书写（不同于 `Border.CornerRadius` 必须走样式 Setter）；不设置时跟随主题令牌并随主题刷新，显式设置后不再被令牌覆盖；任一角为负数或 `NaN`/`Infinity` 时在赋值处抛 `ArgumentException` |
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

    <!--  圆角逐角设置，顺序是左上,右上,右下,左下：这里让贴住容器左边的两角成直角、自由边 18 圆角  -->
    <jv:SidePanel IsOpen="{Binding IsPanelOpen, Mode=TwoWay}" Side="Left" CornerRadius="0,18,18,0">
        <StackPanel Width="300" Margin="8">
            <TextBlock Text="自定义圆角的侧滑面板" />
        </StackPanel>
    </jv:SidePanel>
</Grid>
```

实现说明：收起时面板内容通过 `TranslateTransform` 完全平移出父容器并被裁剪，同时以透明度 0 兜底，根 Grid 的 `Background` 为空保证鼠标命中测试穿透到下层内容；展开时遮罩层淡入，命中测试由遮罩与面板正常接管。面板表面使用主题 `Surface.Raised`、`Theme.PopupShadow` 阴影与 `CornerRadius` 圆角：圆角默认值由样式 Setter 以 `DynamicResource` 注入 `Theme.ControlCornerRadius`（宿主覆盖该令牌或切换明暗主题都会刷新），宿主显式赋值后本地值优先、不再被令牌顶掉；模板把它绑到 `PART_Content` 的 `CornerRadius`，`DropShadowEffect` 按该几何投影，改圆角时阴影自适应，无需额外处理。滑出/收回动画采用 `SineEase`（`EaseInOut`）曲线：`250 ms` 内约 15 帧的采样下，`CubicEase` 的峰值速度达平均速度的 1.875 倍，中段单帧位移接近 68 DIP（约 85 物理像素）而首尾两帧几乎不动，观感上就是"起步一顿、中间一跳"；`SineEase` 的峰值/均值比为 π/2 ≈ 1.57，同帧数下峰值位移降到约 38 DIP 且首帧即有位移，实测帧间隔与硬件渲染档位（`RenderCapability.Tier=2`）均无变化，属于曲线分布而非性能问题。

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
| `CheckerboardBrush` | 透明像素下可见的棋盘格画刷，默认取 `TransparentBackground`（见「主题」） |
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
2. 集合元素已经是容器类型时，例如 `ToolboxItem`、`ToolItem`、`ToolBarItem`、`TabControlItem` 或原生 `MenuItem`，WPF 会直接使用该实例，并可能忽略 `ItemTemplate`。
3. 不要在 `Toolbox.ItemTemplate`、`ToolboxItem.ItemTemplate`、`ToolBar.ItemTemplate` 或 `TabControl.ItemTemplate` 中创建对应的容器类型，否则会形成嵌套容器。
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
| 菜单/导航 | `ContextMenu`、`ContextMenuItem`、`MenuItem`、`SideMenu`、`TreeView`、`TreeMenuItem`、`TabControl`、`TabControlItem`、`ToolBar`、`ToolBarItem`、`Toolbox`、`ToolboxItem`、`ToolItem` |
| 图像 | `ImageViewer` |

## 控件依赖速查

这里的“依赖”指控件正常工作时所依赖的 WPF 基类、同库控件、主题资源或附加属性；全部控件都依赖 `Themes/Generic.xaml` 提供的默认主题样式。

| 控件 | 主要依赖 | 相关附加属性 |
| --- | --- | --- |
| `AppBar` | WPF `ContentControl`、所在 `Window`、`SystemCommands`（命令绑定由控件自动补齐）、`Button`；`DefaultAppBar` 用 `ToolBar`，`MenuBarAppBar` 用 WPF `Menu`/`MenuItem` 与 `JunevyMenuBarStyle`、`JunevyContextMenuItemStyle` | `Icon.Icon`、`Icon.FontFamily`、`Icon.IconSize` |
| `Button` | WPF `Button`、焦点和主题资源 | `Icon.Icon`、`Icon.FontFamily`、`Icon.IconSize`、`Border.CornerRadius` |
| `CardButton` | `jv:Button`、主题资源 | `Icon.Icon`、`Icon.FontFamily`、`Border.CornerRadius` |
| `ToggleButton` | WPF `ToggleButton`、胶囊/圆角矩形两套开关模板（`SwitchToggleButton_Radius` / `SwitchToggleButton_Rect`）、`Theme.SmallCornerRadius` 与派生属性 `TrackCornerRadius` / `ThumbCornerRadius` | 无（开关圆角不走 `Border.CornerRadius`；形状扩展用的 `DisplayMode` 自 `3.2.0` 起删除） |
| `RadioButton` | WPF `RadioButton`、`ShapeMode`、焦点资源 | `Icon.FontFamily` 用于选中符号 |
| `CheckBox` | WPF `CheckBox`、焦点资源 | `Icon.FontFamily`、`Border.CornerRadius` |
| `TextBox` | WPF `TextBox`、`jv:Button` 清空按钮、`jv:Button` 命令按钮 | `Icon.Icon`、`Icon.FontFamily`、`ShowClear`（依赖属性）、`ShowCommandButton`/`CommandButtonCommand`/`CommandButtonCommandParameter`/`CommandButtonContent`（依赖属性）、`PlaceholderAssist.Placeholder`、`TitleAssist.Title` 系列（含 `IsRequired` 必填标识）、`Border.CornerRadius` |
| `ComboBox` | WPF `ComboBox`、`ComboBoxItem`、`jv:ToggleButton` | `PlaceholderAssist.Placeholder`、`TitleAssist.Title` 系列（含 `IsRequired` 必填标识）、`Border.CornerRadius` |
| `ComboBoxItem` | WPF `ComboBoxItem`、`DefaultComboBoxItemStyle` | 无 |
| `ListBox` | WPF `ListBox`、`ListBoxItem`、虚拟化和滚动资源 | 无 |
| `ListView` | WPF `ListView`、`GridView`、虚拟化和转换器 | `Border.CornerRadius` |
| `DataGrid` | WPF `DataGrid`、标准列/行/单元格容器、虚拟化、主题滚动条 | `atc:DataGridAssist.EmptyText` |
| `DatePicker` | WPF `DatePicker`/`Calendar`、官方模板部件契约、主题阴影令牌 | `atc:DatePickerAssist.PlaceHolder` |
| `Slider` | WPF `Slider`/`Track`/`Thumb`/`RepeatButton`/`TickBar` 部件契约、`DefaultTextBoxStyle`（数值框）、主色与下沉面等主题令牌、`DefaultControlFocusVisualStyle` | `ShowClear`（数值框显式关闭清空按钮） |
| `ToolTip` | WPF `ToolTip`、主题资源 | 无 |
| `Label` | WPF `Label`、状态和图标资源 | `Icon.Icon`、`Icon.FontFamily`、`Icon.IconSize`、`Icon.IconForeground`（均经相应模板） |
| `TextBlock` | WPF `ContentControl`、`ContentPresenter`、标准内容模板管线 | 无 |
| `Badge` | WPF `ContentControl`、`TranslateTransform` 停靠偏移、主题状态色令牌（`Theme.Brush.Status.Danger`、`Theme.Brush.Text.OnAccent`） | 无 |
| `MessageBar` | WPF `ContentControl`、`DispatcherTimer`、`jv:Button` 关闭按钮、主题资源 | `Icon.FontFamily`、`Icon.IconSize` |
| `MessageBarPresenter` | WPF `ContentControl`、承载 `MessageBar`，配合 `MessageBarService` | 无 |
| `DialogWindow` | WPF `Window`、`WindowChrome`、`SystemCommands`、主题资源（含阴影/圆角令牌） | 无 |
| `ContextMenu` | WPF `ContextMenu`、`MenuItem`、`Separator`、Popup/阴影资源 | 无 |
| `ContextMenuItem` | WPF `MenuItem`、`JunevyContextMenuItemStyle` | 无 |
| `MenuItem` | WPF `ContentControl`；作为 `SideMenu` 的导航数据 | 无 |
| `SideMenu` | WPF `ListBox`、`ListBoxItem`、导航数据模板 | `Icon.FontFamily`、`Icon.IconSize` |
| `TreeView` | WPF `TreeView`、`TreeMenuItem`（数据模型）、`TreeViewDisplayMode`、单一 `HierarchicalDataTemplate`（`TreeViewItemTemplate`）与容器样式（`DefaultTreeViewStyle` / `TreeViewItemContainerStyle`）、`cvt:IndentSizeToMarginConverter`、主题资源（`Surface.Hover` / `Surface.Sunken` 回退触发器） | `Icon.FontFamily`、`Icon.IconSize`、`ExpanderBehavior.Enable`、`TreeViewAssist`（`DisplayMode` / `IndentSize` / `AutoExpandAncestors` / `NavigateCommand` / `ItemHoverBackground` / `SelectedItemBackground`） |
| `TreeMenuItem` | 普通数据模型（`INotifyPropertyChanged`）、`ObservableCollection<TreeMenuItem>`；`ExpandAll()` / `CollapseAll()` 递归写模型；经所在 `TreeView` 使用图标附加属性 | 无 |
| `TabControl` | WPF `TabControl`、`TabControlItem`、`jv:TextBox`、`jv:Button` | `IsClosable`（控件自身属性）、`Icon.FontFamily`、`TabControl.HeaderCornerRadius` / `TabControl.ContentCornerRadius`（圆角为**附加属性**，官方 `<TabControl>` 也可用 `jv:TabControl.*` 写法自定义） |
| `TabControlItem` | WPF `TabItem`、`DefaultTabControlItemStyle`、`TabControl.CloseTabCommand` | 继承所在 `TabControl` 的相关附加属性 |
| `ToolBar` | WPF `ItemsControl`、`ToolBarItem`、虚拟化面板 | 无；图标由项目自身属性提供 |
| `ToolBarItem` | WPF `Button`、`DefaultToolBarItemStyle` | 无 |
| `Toolbox` | WPF `ItemsControl`、`ToolboxItem`、`Popup`、`UniformGrid`、当前显示器工作区 | `Icon.FontFamily`、`Icon.IconSize`、`Icon.IconForeground` 由分组和工具模板使用 |
| `ToolboxItem` | WPF `HeaderedItemsControl`、`ToolItem`、`DefaultToolboxItemStyle`、所属 `Toolbox` 的交互和布局参数 | `Icon.FontFamily`、`Icon.IconSize`、`Icon.IconForeground` |
| `ToolItem` | WPF `Button` 命令管线、`DefaultToolItemStyle`、WPF `DragDrop` | `Icon.FontFamily`、`Icon.IconSize`、`Icon.IconForeground` |
| `ImageViewer` | WPF `Image`、`MatrixTransform`、`BitmapSource`、`SaveFileDialog` | 无 |
| `ExpanderPanel` | WPF `HeaderedContentControl`、`ToggleButton`、`LayoutTransform` 过渡动画、主题资源 | 无 |
| `SidePanel` | WPF `ContentControl`、`TranslateTransform` 滑动动画（`SineEase`）、遮罩与主题阴影/圆角令牌（`Theme.Brush.Overlay.Backdrop`、`Theme.PopupShadow`、`Theme.ControlCornerRadius`）、宿主窗口级点击/失焦/最小化自动收回 | 无 |
| `GroupBox` | WPF `GroupBox`、主题资源（卡片、悬停、状态令牌） | `Border.CornerRadius` |

## 示例程序（Showcase）

`Samples/Junevy.Controls.Showcase` 是使用本控件库开发的分类展示程序，运行：`dotnet run --project Samples/Junevy.Controls.Showcase`（net8.0-windows）。

- **主窗口**：无边框 + `WindowChrome` + `jv:AppBar`（默认模板 DefaultAppBar；一个程序仅一个 AppBar，此处展示默认状态，工具栏按钮演示 `ThemeManager` 主题切换与打开 `jv:SidePanel` 设置抽屉）、`jv:SideMenu` 分类导航、`MessageBarService` 通知宿主。
- **分类页**（与「控件索引」的控件族分类一一对应）：按钮（Button、CardButton、ToggleButton、RadioButton）/ 输入与选择（CheckBox、TextBox、PasswordBox、GroupBox、ComboBox、DatePicker、Slider；含 PlaceholderAssist、TitleAssist、ShowClear、DatePickerAssist、Slider 数值框）/ 集合与数据（ListBox 竖向+横向、ListView GridView、DataGrid、EmptyText 空态、PagingAssist 分页）/ 文本与状态（Label 全模式、TextBlock、CodeEditor）/ 菜单与导航（SideMenu、原生 Menu 菜单栏样式、ContextMenu、TreeView 四区块演示、TabControl）/ 栏与工具（ToolBar、Toolbox 双样式、AppBar Expandable、InfoBar）/ 通知（Badge、MessageBar、MessageBarService、ToolTip）/ 进度条（ProgressBar 线性/环形/文本格式）/ 布局（ExpanderPanel 经典+卡片、SidePanel 滑出面板、Border.CornerRadius）/ 窗口与对话框（DialogWindow、ProgressBarWindow）/ 图像（ImageViewer、TransparentBackground）/ 图标字体。
- **演示区块（DemoSection）**：每个控件演示统一为「标题 + 用法说明 + 演示内容卡片 + XAML 源码块」结构（`Samples/Junevy.Controls.Showcase/Controls/DemoSection`），源码块带轻量语法高亮（`XamlHighlighter`，配色取主题令牌、随明暗主题切换）、「收起/展开代码」与「复制代码」按钮；片段文本集中在 `ShowcaseSnippets.cs`，与页面演示同步维护。

## 开发注意事项

- 主题相关值使用 `DynamicResource`，固定且不会切换的资源才使用 `StaticResource`。
- 自定义控件模板时保留 WPF 标准部件名称和内容管线，例如 `PART_ContentHost`、`PART_EditableTextBox`、`ItemsPresenter`、`ContentTemplate` 和 `ItemContainerStyle`。
- 图标字体字符通过 `Foreground` 着色；位图和固定填充的矢量图不会自动着色。
- 不要同时合并 `AppColors.Light.xaml` 与 `AppColors.Dark.xaml`，否则后合并的重复资源键会覆盖前者。
