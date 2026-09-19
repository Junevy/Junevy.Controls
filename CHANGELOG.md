# Junevy.Controls 更新日志

本文档记录 `Junevy.Controls` 控件库的历史变更与本次迭代内容。

## AppBar

### 本次更新（新增模板与依赖属性）— 2026-09-19

- 新增 `MenuBarAppBar` 控件模板：单行布局「应用图标 + 应用名 + 菜单栏 + 弹性空白 + 最小化/最大化/关闭」，配合宿主 `Window` 的 `WindowChrome.WindowChrome` 作为无边框窗口标题栏使用。`SimpleAppBar` 与 `AppBar` 隐式样式未做任何改动，现有用法不受影响。
- 新增依赖属性 `Menu`（WPF `Menu`，默认 `null`）：菜单栏内容，仅 `MenuBarAppBar` 呈现。本模板不承载 `ToolBar`，`ToolBar` 与 `Menu` 二选一。类型在代码中保持全限定 `System.Windows.Controls.Menu`，因为兄弟命名空间 `Junevy.Controls.Controls.Menu` 会在 `Junevy.Controls.Controls.Bar` 内遮蔽同名类型。
- 新增 `Controls/Menu/MenuBar.xaml`（已注册到 `Themes/Generic.xaml`）：
  - `JunevyMenuBarStyle`（`TargetType=Menu`）：透明背景融入标题栏，`ItemContainerStyle` 指向顶层项样式。
  - `JunevyMenuBarItemStyle`（`TargetType=MenuItem`）：顶层项横向文字表头、`Theme.SmallCornerRadius` 悬停/按下高亮、子菜单 `Popup Placement="Bottom"` 且不画右箭头（原生 `Menu` 与 `ContextMenu` 的右向弹出语义不同）。其 `ItemContainerStyle` 指向 `JunevyContextMenuItemStyle` 并沿用该样式的 `DynamicResource` 自引用链，因此二级及更深层子菜单与 `jv:ContextMenu` 外观完全一致。
  - 两个样式均为 keyed 资源；`Menu` 的隐式样式只在 `MenuBarAppBar` 模板的 `Grid.Resources` 内注入，作用域限于本模板，不会改变消费方应用中其他原生 `Menu` 的外观。
- `WindowChrome` 适配：菜单栏与三个系统按钮标记 `WindowChrome.IsHitTestVisibleInChrome="True"`，图标、应用名与弹性空白保持为 chrome 拖动区（可拖动移动、双击最大化由 `WindowChrome` 处理）。
- 修正最大化按钮的触发器优先级问题（`MenuBarAppBar` 与 `DefaultAppBar`）：按钮的 `Content`/`Command`/`ToolTip` 只由样式与样式触发器设置，不再写本地值——本地值优先级高于样式触发器，会使 `WindowState=Maximized` 的触发器失效，按钮在最大化后仍停留在「最大化」字形与命令（点击无还原效果）。`DefaultAppBar` 原模板即存在此缺陷，本次一并修正；最小化与关闭按钮不受影响。
- 验证：控件库编译通过（net48 / net8.0-windows，0 错误，无新增警告）；临时探测工程 `AppBarMenuBarProbe` 端到端探测 17 组断言全部通过（模板已应用、`Menu` 命中模板内隐式样式、顶层项与下层项样式来源、三按钮存在/顺序/贴右边缘、菜单位于应用名与按钮之间、最大化按钮初始字形与命令、菜单区在 chrome 中可命中、空白区保持可拖动、菜单不撑破标题栏高度、子菜单弹出与向下方向、二级子菜单右向弹出、最大化后字形与命令切为还原、还原后恢复），浅色/深色主题、菜单展开、最大化窗口 PNG 快照人工核对通过。回归探测工程 `AppBarDefaultProbe` 8 组断言全部通过（`DefaultAppBar` 仍为隐式样式模板、三按钮存在、最小化/关闭字形未受影响、最大化按钮初始字形与命令、最大化后切为还原字形/命令/提示、还原后恢复原值），最大化窗口快照人工核对 `ToolBar` 布局与图标区渲染不变。探测工程验证后均已删除。

## Badge

### 本次更新（新增控件）— 2026-09-15

- 新增 `Badge` 角标控件（`jv:Badge`，继承 WPF `ContentControl`）：类似手机应用右上角的未读提醒，包裹任意内容（按钮、图标、菜单项等）并在内容的角落叠加小圆点或未读数角标，包裹层自身 `Focusable=False`、`IsTabStop=False`，不参与焦点与 Tab 导航。
- 新增依赖属性：
  - `Count`（默认 `0`）：未读数；小于等于 0 时角标隐藏，大于 `MaxCount` 显示 `99+` 形式。
  - `MaxCount`（默认 `99`，校验非负）：数字显示上限。
  - `IsDot`（默认 `false`）：纯圆点模式（固定 10x10，不显示数字），显示/隐藏仍由 `Count` 控制。
  - `Corner`（`BadgeCorner` 枚举，默认 `TopRight`）：停靠角落四选一，运行时切换立即生效。
  - `OffsetX` / `OffsetY`（默认 `0`）：在角落停靠位置基础上的像素级微调。
- 停靠几何：内层 Grid 按内容自然尺寸收紧（复用 `HorizontalContentAlignment`/`VerticalContentAlignment`，默认 Left/Top），保证角标锚定内容自身角落而非包裹层被父容器拉伸后的角落；停靠偏移按角标实际尺寸实时计算（角标尺寸随内容变化经 `SizeChanged` 重算），并补偿内容自身的 `Margin`，外边距不会造成角标脱离视觉角落；`OffsetX/OffsetY` 或内容替换（`ContentProperty` OverrideMetadata）即时重算。
- 命中测试：角标 `IsHitTestVisible=False`，点击穿透到被包裹的内容。
- 视觉：`Theme.Brush.Status.Danger` 背景 + `Theme.Brush.Text.OnAccent` 文字（深浅主题自动切换），数字角标 16x16 胶囊（最小宽 16、`Padding=4,0`、圆角 8），纯圆点 10x10（圆角 5）。
- 默认样式注册到 `Themes/Generic.xaml`，命名空间 `Junevy.Controls.Controls.Badge` 加入 `github.com.junevy` XML 命名空间。
- 验证：控件库编译通过（net48 / net8.0-windows，新增代码 0 警告 0 错误）；`BadgeProbe` 端到端探测 9 组断言全部通过（默认隐藏与布局无侵入、数字角标中心对准内容角落、99+ 上限、纯圆点切换、运行时切角、偏移微调、重新隐藏、命中穿透、PNG 快照）。

## SidePanel

### 本次更新（新增控件）— 2026-09-15

- 新增 `SidePanel` 侧滑面板控件（`jv:SidePanel`，继承 WPF `ContentControl`）：作为浮层放置在布局容器（通常为 `Grid`）中，`IsOpen`（bool，双向绑定默认开启）绑定 `true` 时面板从 `Side` 指定的边缘滑出，叠加显示在兄弟内容上方；`false` 时完全滑出可视区域、不占用任何布局空间。
- 新增依赖属性：
  - `Side`（`SidePanelSide` 枚举：`Left`/`Right`/`Top`/`Bottom`，默认 `Left`）：滑出方向；`Left`/`Right` 垂直填满、水平停靠对应边缘，`Top`/`Bottom` 水平填满、垂直停靠对应边缘；运行时切换立即生效。
  - `AnimationDuration`（默认 `250ms`）：滑出/收回过渡动画时长，滑动 + 淡入淡出同步过渡（CubicEase EaseInOut）；`0` 表示无动画直接切换。
  - `IsBackdropEnabled`（默认 `true`）与 `BackdropBrush`（默认主题 `Theme.Brush.Overlay.Backdrop`）：展开时在面板背后淡入半透明遮罩。
- 新增路由事件 `Opened`/`Closed`：滑出/收回动画完成后触发；动画时长为 `0` 时随状态切换立即触发，模板未加载时发生的状态切换会在模板应用后补发，保证事件不丢失。
- Grid 浮层约定：直接放入 `Grid`（不指定 Row/Column）时自动跨满父 Grid 的所有列/行（`ColumnSpan`/`RowSpan` = 定义数），使用者显式设置的跨距不被覆盖；面板宽高由 `Content` 决定，可通过内容的 `Width`/`Height` 指定。
- 收起状态的实现细节：内容经 `TranslateTransform` 平移出父容器并被根 `ClipToBounds` 裁剪，同时透明度归 0 兜底（防止主题阴影在边缘残留），根 Grid `Background` 为空保证收起时鼠标命中测试完全穿透到下层内容；展开后遮罩与面板正常接管命中测试。
- 快速连续切换的防抖处理：状态版本号校验，旧动画完成回调不会覆盖新状态；反向切换时先清除两个轴上的残留动画，避免中途换向闪烁。
- 面板视觉：主题 `Surface.Raised` 表面、`Theme.PopupShadow` 阴影、主题圆角、`Border.Default` 细边框，浅色/深色主题随 `DynamicResource` 自动切换。
- 默认样式注册到 `Themes/Generic.xaml`，命名空间 `Junevy.Controls.Controls.Panel` 加入 `github.com.junevy` XML 命名空间。
- 验证：控件库编译通过（net48 / net8.0-windows，新增代码 0 警告 0 错误）；`SidePanelProbe` 端到端探测 10 组断言全部通过（初始收起位移/透明度/遮罩折叠、自动跨满、收起命中穿透、滑出动画后事件与状态、展开命中、收回恢复、运行时切换 Side=Top、禁用遮罩、0 时长动画事件、PNG 快照）。

## ToggleButton

### 本次更新（开关尺寸与视觉重构）— 2026-09-15

- 新增 `SwitchSize` 依赖属性（默认 `20`，建议不小于 `12`）：只需设置一个属性即可调整开关整体大小，轨道宽度按 2:1 比例自动推导，任何尺寸下宽高比例恒定，不再出现设置不合理宽高导致的视觉变形。
- **移除 `SwitchWidth` / `SwitchHeight`（破坏性变更）**：原来两个独立尺寸属性允许任意比例组合（如 40×40），会导致轨道严重变形；所有使用旧属性的代码需迁移到 `SwitchSize`。
- 两个开关模板（`SwitchToggleButton_Radius` / `SwitchToggleButton_Rect`）由"轨道均分左右两半填充"重构为经典"轨道 + 滑块"结构：
  - 滑块直径 = `SwitchSize` − 4（扣除左右边框 1 + 内边距 1），与轨道内壁严丝合缝，任何尺寸下都不会露出缝隙或溢出。
  - 切换选中状态时滑块以 200ms 缓动动画（QuadraticEase EaseOut）滑动到对侧，动画行程由控件根据 `SwitchSize` 实时计算，尺寸变化后动画依然正确。
  - 未选中滑块为 `Theme.Brush.Border.Strong`，选中为 `Theme.Brush.Accent.Primary`，禁用状态沿用原有禁用配色方案。
- 圆角数学修正：胶囊模板轨道圆角 = `SwitchSize` / 2（正圆弧），滑块圆角 = 滑块直径 / 2，恒比轨道圆角小 2 DIP（内缩量一致），内外圆角视觉吻合；矩形模板滑块圆角 = 外圆角(4) − 内缩量(2) = 2。修复了旧实现内层直接沿用外层圆角导致内角"发胖"的问题。
- 移除 `DefaultToggleButton` 样式中已无消费者的 `Border.CornerRadius` 设置，胶囊圆角改由 `SwitchSize` 推导。
- 验证：控件库编译通过（net48 / net8.0-windows，0 警告 0 错误），现有测试套件全部通过。

## DialogWindow

### 本次更新（按内容自适应尺寸）— 2026-09-14

- 修复对话框出现"默认宽高"的问题：`DialogWindow` 本身未设置 `Width`/`Height`，但 WPF `Window` 在二者为 NaN 时会向操作系统请求 `CW_USEDEFAULT` 默认尺寸，导致塞入的 `UserControl` 四周出现大片空白边距。
- 构造函数中启用 `SizeToContent = SizeToContent.WidthAndHeight`：窗口尺寸完全由注入的内容（如 UserControl）决定，内容多大窗口就多大；需要固定尺寸时给内容控件设置显式 `Width`/`Height` 即可。
- 最大化状态处理：WPF 对 `SizeToContent = WidthAndHeight` 的窗口不执行系统最大化（实测窗口保持内容尺寸），因此重写 `WindowState` 属性的 Coerce 回调，在最大化生效前先切换为 `Manual`，最大化才能真实铺满屏幕；还原普通状态后由 `StateChanged` 恢复 `WidthAndHeight` 继续按内容自适应。
- 验证：`DialogProbe` 端到端探测通过——固定尺寸内容（窗口 = 内容 + 阴影边距 + 边框 + 标题栏）、内容尺寸变化跟随、无显式宽高的自然尺寸内容、最大化铺满工作区、还原后尺寸与 `SizeToContent` 均正确恢复。
- 附带说明：`SizeToContent = WidthAndHeight` 下 WPF 官方契约禁用拖拽边缘缩放，符合对话框语义；`ShadowMargin` 的缩放热区在对话框模式下不再生效。

## 主题滚动条（ScrollBar）

### 本次更新（横向滚动修复）

- 修复横向滚动条被压成细竖条的问题：原样式的 `Width=8` 对两个方向都生效，横向滚动条因此只有 8px 宽并贴在容器底部，看起来像一条竖着的滚动条。现在按方向分别给尺寸——竖向 `Width/MinWidth=8`，横向 `Height/MinHeight=8`。
- 四个尺寸属性都必须显式书写：系统主题样式同样会设置它们（竖向 `MinWidth`、横向 `MinHeight` 为系统滚动条厚度），只写 `Width`/`Height` 会被 `Min*` 顶掉。
- 修复横向滚动条点击、拖动完全无效的问题，共两处原因：
  - 翻页按钮对横向条仍使用竖向命令 `PageUp`/`PageDown`。WPF 的 `ScrollBar.OnScrollCommand` 只按方向映射命令（横向只认 `PageLeft`/`PageRight`），命令因此被忽略；现在横向模板改用 `PageLeft`/`PageRight`。
  - `Track` 对两个方向都写死 `IsDirectionReversed=True`，与官方横向的 `False` 相反，滑块位置与拖动方向都不正确。
- 移除用 `LayoutTransform` 镜像翻转伪造横向下拉的做法，改为按方向各提供一套模板（官方主题同样如此），`Track` 方向、`IsDirectionReversed`、翻页命令全部与 WPF 官方一致。
- 轨道空白处的翻页热区改为透明仍可命中的 `RepeatButton` 模板，不再依赖系统 `RepeatButton` 外观加 `Opacity=0`。

## ListBox / ListView

### 本次更新（横向滑动）

- `jv:ListBox`、`jv:ListView` 新增 `Orientation` 依赖属性（默认 `Vertical`）：设为 `Horizontal` 后项目自左向右排列、水平滚动条按需显示、垂直滚动条关闭；运行时切换立即生效，无需重建控件。
- 横向模式下补齐鼠标滚轮折算。WPF 的 `ScrollViewer.OnMouseWheel` 只做竖直滚动并无条件把事件标记为已处理，竖向滚不动时不会退化为水平滚动；控件在「横向 + 竖向不可滚动 + 横向可滚动」时按 `SystemParameters.WheelScrollLines` 折算为水平滚动，条目模板内部的滚动控件仍优先获得滚轮。
- `jv:ListView` 的横向模式要求未设置 `View`，使用 `GridView` 时列表维持竖向，避免破坏列布局与表头。

## ExpanderPanel

### 本次更新（评审修复）

- 修复 `ExpandDirection.Left/Right` 与 WPF `Expander` 语义相反的问题：`Left` 现在表示内容向左展开、头部停靠右侧；`Right` 相反。
- 展开/折叠动画由 `RenderTransform` 改为 `LayoutTransform`：动画期间周围布局同步收缩，修复折叠结束瞬间内容区"先占位、后跳变"的问题。
- `Expanded`/`Collapsed` 事件改为随状态切换立即触发（与 WPF `Expander` 一致），修复动画时长为 `0` 或模板尚未加载时事件不触发的问题。
- `ToggleCommand` 执行前检查 `CanExecute`。
- 新增 `ExpanderPanelAutomationPeer`，暴露 UIA `ExpandCollapse` 自动化模式。

## TreeMenu / TreeMenuItem

### 本次更新（交互与视觉升级）

**交互设计**

- 新增键盘激活：叶节点按 `Enter` 触发 `TreeMenu.NavigateCommand`，文件夹节点按 `Enter` 切换展开/收起。
- 修复双击展开箭头时的重复切换问题：展开箭头为 `ToggleButton`，单击即切换，双击不再额外触发一次，避免“展开后立刻收起”的抖动。
- 统一激活语义：单击选中、双击激活（叶节点导航 / 文件夹展开）、`↑/↓/←/→` 沿用 WPF `TreeView` 原生方向键行为。

**视觉 UI**

- 悬停、选中、禁用三种状态改为在 `TreeViewItem` 容器整行生效，并使用主题色区分：
  - 悬停：`Theme.Brush.Surface.Hover`
  - 选中：`Theme.Brush.Surface.Selected`
  - 禁用：`Theme.Brush.State.DisabledSurface`
- 滚动条由硬编码 `Hidden` 改为样式绑定，长列表自动显示垂直滚动条（`VerticalScrollBarVisibility=Auto`，水平方向 `Disabled`）。
- 新增 `SnapsToDevicePixels` 与 `UseLayoutRounding`，改善高 DPI 与多显示器下的像素对齐与渲染清晰度。

### 历史版本

- `c6dc2c9` `Feat: Optimized those controls` — 优化控件细节。
- `1a4f638` `Feat: Fix TreeMenu multi-item layout exception.` — 修复多条目布局异常。
- `5433de8` `fix: inherit TreeMenu item font size from parent` — 修复条目字号未继承父级的问题。
- `006ddcc` `Redesign the theme color scheme(Light and Dark theme)` — 主题色彩系统重构，支持浅色/深色主题。
- `e698190` `1) update dark mode style; 2) fixed some bugs.` — 更新深色模式并修复若干问题。
- `9c473d9` `TreeMenu and ExpanderButton added.` — 首次引入 TreeMenu 与 ExpanderButton。
