# Junevy.Controls 更新日志

本文档记录 `Junevy.Controls` 控件库的历史变更与本次迭代内容。

## Button 浮起阴影

### 本次更新（新增 `Theme.ButtonShadow`，`jv:Button` 常态浮起、按压即贴回地面）— 2026-09-23

- **新增资源键** `Theme.ButtonShadow`（`Themes/AppColors.Light.xaml`、`Themes/AppColors.Dark.xaml` 各一份，`DropShadowEffect`）：浅色 `BlurRadius=16 / ShadowDepth=5 / Direction=270 / Opacity=0.18`，深色 `18 / 6 / 270 / 0.40`，颜色沿用 `Theme.Color.Effect.Shadow`。取向是**与 MessageBar 弹层（`Theme.PopupShadow`）同族同强度**，只把模糊与偏移按小控件尺寸收紧：实测按钮下方 1-9px 相对压暗 8.47%，弹层同距离为 9.50%；`5-9px / 1-5px` 衰减比 0.58 对弹层 0.63，即同一族渐变形状而非另立一套观感。
- **控件改动**：`Controls/Button/Button.xaml` 的 `JvButtonTemplate` 拆成三层——根 `Grid`（`RootHost`，承载悬停/按压/禁用的整体降透明度）、无子元素的背景层 `Border`（`ContentBorder`，底色/描边/圆角 + `Effect={DynamicResource Theme.ButtonShadow}`）、内容层 `Border`（`ContentHost`，只负责 `Padding` + `BorderThickness` 内缩）。`Effect` 只挂背景层是因为 WPF 位图特效会让被作用元素内的文字退出 ClearType。`IsPressed` 与 `IsEnabled=False` 触发器各加一条 `Effect={x:Null}`：按压即「贴回地面」，禁用态不该带浮起感。状态反馈的 `Opacity` 目标由 `ContentBorder` 改为 `RootHost`，否则只有背景变淡、文字不变。
- **作用域**：只有 `jv:Button` 的默认模板带阴影。官方 `Button`（`ButtonTemplate`）、`NoBorderButtonStyle` 均未改动；`jv:CardButton` 虽派生自 `jv:Button`，但自带 `CardButtonTemplate`，实测子树内无任何 `Effect`。全库检索确认没有其他控件模板内嵌 `jv:Button`。
- **迭代说明**：初版取 `10/3/0.18`，压暗强度对但衰减过硬（比值 0.22，读起来像贴边而不是散开），放宽到现值 `16/5` 后为 0.58，与弹层的 0.63 落在同一族。
- **踩坑（已修）**：把 `Padding` 从背景层移到内容宿主后，按钮实测尺寸从 `45.6x24.8` 缩到 `44x23.2`——`Border` 即使不画描边也会把 `BorderThickness` 计入内容内缩，拆分时漏了这一项。现由 `ContentHost` 这层无刷 `Border` 复现，并把尺寸写成硬断言防回归。
- **可覆盖性**：触发器用 `DynamicResource` 取该键，宿主在 `Application.Resources` 写同名键即可整体调淡或关掉，移除后回落主题值（已实测）。
- **版本号**：保持 `1.8.0` 不变——与棋盘格资源、SideMenu 条目阴影同属尚未发布的 1.8.0 迭代。
- 文档：README 资源键表补 `Theme.ButtonShadow` 一行；Button 小节说明常态阴影、按压/禁用时消失、ClearType 处理、不受影响的作用域与覆盖方式。
- 验证：net8.0-windows 探针工程（`.workbuddy/tmp/ButtonShadowProbe/`）37 项断言全部通过——结构（`RootHost`/`ContentBorder`/`ContentHost` 齐备、背景层与内容层同级、背景层 `VisualTreeHelper.GetChildrenCount == 0` 证明文字未被包进特效层、浅色参数 16/5/270/0.18）；作用域（官方 `Button`、`NoBorderButtonStyle`、`CardButton` 子树均无 `Effect`，带缩放宿主的 `jv:Button` 有）；尺寸基线（`jv:Button` 与未改动的官方 `Button` 逐字段一致 45.6x24.8，窄按钮 90x28.8，无边框 60x27.2）；像素剖面（向下 1-5 / 5-9 / 9-14px = 27.31 / 15.89 / 5.66，向上 1-5px = 2.97，上方不足下方的四分之一；与 `Theme.PopupShadow` 参照比较强度与衰减比）；**真实鼠标按压**（`SetForegroundWindow` + 前台校验 + 首次点击激活补偿，实测 `IsPressed=True`、背景层 `Effect` 归零、根 `Grid` 不透明度 0.65、抬起后阴影复现 16/0.18、`Click` 计数正常）；悬停 0.8 / 禁用 0.5 且禁用时不带阴影；`ApplyTheme(Dark)` 后活按钮重新解析为 18/6/0.40；`IsTextScaled` 两个宿主切换正常；应用级同名键覆盖优先于主题字典且移除后回落。**三项变异反证**：删掉模板上的 `Effect` 特性 → 12 项 FAIL；删掉 `IsPressed` 的 `Effect={x:Null}` → 仅「按压时阴影归零」1 项 FAIL；删掉 `ContentHost` 的 `BorderThickness` → 3 项尺寸断言 FAIL 并复现 44x23.2。快照 `button-light.png`、`button-pressed.png`、`button-dark.png` 人工核对：常态胶囊下方柔和渐隐、按压后干净贴地。
- 真机验证：`.workbuddy/tmp/ShowcaseButtonProbe/` 直接驱动运行中的 Showcase（UIA 定位 + 屏幕截图，不依赖库内部对象），7 项断言全部通过——以「按住不放的那一帧」作为同环境无阴影参照，浅色下 5 个 `jv:Button` 的近带（下缘 6-11px）回弹 15.81-16.47 级、远带（26-31px）漂移 0.00，深色下回弹 7.22-7.76 级；「禁用状态」「无边框样式」回弹 0.00，证明阴影确实只落在应有的按钮上且未被容器裁掉。
- 回归：`Junevy.Controls.csproj` Release 双目标框（net8.0-windows + net48）0 错误、警告数与改动前一致（40 个既有可空性提示）；Showcase 工程 0 错误；`.workbuddy/tmp/SideMenuShadowProbe/` 23 项、`.workbuddy/tmp/TransparentBgProbe/` 37 项断言重跑全部通过。探针工程为临时宿主，不入库。

## SideMenu 选中项阴影

### 本次更新（新增 `Theme.SideMenuItemShadow`，阴影只作用于被选中的导航条目）— 2026-09-23

- **新增资源键** `Theme.SideMenuItemShadow`（`Themes/AppColors.Light.xaml`、`Themes/AppColors.Dark.xaml` 各一份，`DropShadowEffect`）：浅色 `BlurRadius=12 / ShadowDepth=5 / Direction=270 / Opacity=0.05`，深色 `14 / 5 / 270 / 0.20`，颜色沿用 `Theme.Color.Effect.Shadow`，随主题字典整体替换自动换色换强度。取向是**只向下散开的极淡柔光**：靠 `ShadowDepth` 把阴影核心从条目边缘推到下方 5px，四周（尤其上边缘）几乎不留压暗，避免紧贴胶囊形成一圈「描边感」。深色取值更重，是因为深色阴影色本身为 `#99000000`、深色菜单面（`#2B2B30`）可见压暗余量小，需更高的 `Opacity` 才能与浅色达到同样的绝对压暗量（浅色实测 5.49 级 / 深色 4.27 级）。
- **控件改动**：`Controls/Menu/SideMenu.xaml` 的 `ItemContainerStyle` —— `IsSelected` 触发器除原有的 `Surface.Selected` 背景外，再给条目加 `Effect`；未选中条目完全不变。`IsEnabled=False` 触发器补一条 `Effect={x:Null}`，否则「禁用且被选中」的条目会带着浮起阴影，与灰态语义矛盾。同时把条目模板从「`Border` 包 `ContentPresenter`」改为「`Grid` 内背景层 `Border` 与 `ContentPresenter` 同级」，`Effect` 只挂在背景层上：WPF 的位图特效会让被作用元素内的文字退出 ClearType，若沿用旧结构，选中项的标题会比其他条目发虚。改后阴影不影响任何文字渲染，且 `Root` 仍是背景/圆角的唯一承载者，各状态触发器无需改动。
- **迭代说明**：同一轮里先做过「给 SideMenu 模板根 `Border` 加贴边阴影（`Theme.SideMenuShadow`，朝内容侧投影）」，实测偏淡且与外层容器分隔线叠加后观感不理想，已回滚删除，未发布，键名一并改为条目用途。条目阴影本身也调了三版：`8/2/0.35` 太立体 → `6/1/0.20` 压到扁平后，实测阴影几乎全部贴在胶囊四周一圈（上边缘 1-5px 处仍有 3.40 级压暗，5-9px 处已归零），读起来像描边而不是投影 → 现值 `12/5/0.05` 用偏移换方向、用大模糊换渐变，上边缘压暗降到 0.00。
- **可覆盖性**：触发器用 `DynamicResource` 取该键，宿主在 `Application.Resources` 里直接定义同名键即可整体调淡或关掉（应用自身条目的优先级高于 `ThemeManager` 追加的 `MergedDictionaries`），已实测覆盖生效且移除后回落主题值。
- **版本号**：保持 `1.8.0` 不变——与棋盘格资源同属尚未发布的 1.8.0 迭代。
- 文档：README「主题」资源键表补充 `Theme.PopupShadow` / `Theme.SideMenuItemShadow` 两行，SideMenu 小节说明条目阴影与关闭方式。
- 验证：net8.0-windows 探针工程（`.workbuddy/tmp/SideMenuShadowProbe/`）23 项断言全部通过——模板根 `Border` 的 `Effect` 确认为 `null`（回滚生效）；选中项背景层 `Effect` 为 `DropShadowEffect` 且浅色参数 12/5/270/0.05，其余 4 个未选中条目 `Effect` 全为 `null`；承载阴影的 `Border` 无子元素，证明文字未被包进特效层；条目 `ActualHeight` 仍为 44。**选中/未选中两次渲染差分**：下方 1-9px 条带压暗 5.49/255（相对 2.15%），同图 `Theme.PopupShadow` 参照为 9.48%，即条目阴影约为 MessageBar 弹层的 1/4，并另设「相对压暗 ≤ 4%」的硬阈值守住扁平取向；**分布剖面**（本轮新增，直接对着「不要描边感」这条验收）向下 1-5 / 5-9 / 9-14 / 14-20px = 7.50 / 3.48 / 0.20 / 0.00，向上 1-5 / 5-9px = 0.00 / 0.00——即阴影确实向下散开、有渐变而非硬边、发散范围在 14px 内归零、上边缘完全没有压暗。条带外（菜单右侧 40px 处）差分 < 0.2 证明不外溢；选中底色实测 226.89 与 `#D6E4FF` 理论值 226.9 吻合，证明背景层重构后选中色未被阴影污染。**反向对照**：取消选中后条带回到基线、重新选中压暗量复现（Δ<0.2）；禁用且选中时条目 `Effect` 归零、恢复启用后回到 12/0.05；`ApplyTheme(Dark)` 后活条目重新解析为 14/5/0.20，相对深色菜单面（43.57）压暗 4.27 级。把浅色令牌临时改回上一版的贴边参数 `6/1/0.20` 重跑，5 项断言转为 FAIL，且剖面实测为「下方 1-5px=9.25、5-9px 起全部 0.00、上方 1-5px=3.40」——正是被否掉的描边形态，证明分布断言真的能捕获该缺陷而不是恒真。另实测宿主覆盖路径：在 `Application.Resources` 直接写同名键后，活条目的 `Effect` 立即换成覆盖值（`Opacity=0` 即关掉阴影），移除后回落主题字典的值。快照 `sidemenu-light.png`、`sidemenu-dark.png` 与 Showcase 真机截图（浅色首页、深色首页、深色「菜单与导航」页）人工核对：胶囊下方一圈柔和渐隐、上边缘干净。探针工程为临时宿主，不入库。
- 回归：`Junevy.Controls.csproj` Release 双目标框（net8.0-windows + net48）0 错误、警告数与改动前一致（40 个既有可空性提示）；Showcase 工程 0 错误 0 警告；`.workbuddy/tmp/TransparentBgProbe/` 37 项断言重跑全部通过。

## 透明背景棋盘格资源 `TransparentBackground`

### 本次更新（新增随主题切换的棋盘格 DrawingBrush 与 Geometry 资源，并归并 ImageViewer 的私有实现）— 2026-09-23

- **新增资源键**（`Themes/AppColors.Light.xaml` 与 `Themes/AppColors.Dark.xaml` 各一份，随主题字典整体替换而换色）：
  - `TransparentBackground.Geometry` —— 纯几何资源：一个 16×16 单元内的两块 8×8 方格（`GeometryGroup` + 两个 `RectangleGeometry`，`Rect=0,0,8,8` 与 `8,8,8,8`），不含颜色，供第三方自绘、做蒙版或自定义配色。
  - `TransparentBackground.Small` / `TransparentBackground` / `TransparentBackground.Large` —— 三档棋盘格 `DrawingBrush`，`TileMode=Tile`、`Viewbox` 固定 `0,0,16,16 Absolute`，只有 `Viewport` 取 `8/16/32`，因此方格实际为 **4px / 8px / 16px**，且**不随控件尺寸拉伸**（矢量平铺，非位图）。
  - 配色资源 `Theme.Color.TransparentBackground.Base/.Alt` 与 `Theme.Brush.TransparentBackground.Base/.Alt`：浅色 `#FFFFFF` / `#E0E4EA`，深色 `#2B2B30` / `#232327`（沿用原 ImageViewer 取值，视觉无回归）。
- **归并（破坏性）**：删除 `ImageViewer` 私有的同一套棋盘格定义 `Theme.Brush.ImageViewer.Checkerboard` 及其 `Theme.Color/Brush.ImageViewer.Checkerboard.Base/.Alt` 共 5 个键——两处独立定义会各自漂移、导致深浅配色不一致。`Controls/Image/ImageViewer.xaml` 的样式默认值改为 `CheckerboardBrush = {DynamicResource TransparentBackground}`，档位与外观均与改动前一致。迁移：`{DynamicResource Theme.Brush.ImageViewer.Checkerboard}` → `{DynamicResource TransparentBackground}`。
- **用法约束**：必须用 `DynamicResource`（`StaticResource` 会在加载期固化，切换主题不刷新）。`Resources/Pictures/Image.xaml` 的 `whiteCheckBoard` 位图保留不动，但已在 README 标注为历史遗留（不随主题变、会拉伸）。
- **版本号**：`Junevy.Controls.csproj` 的 `Version` 由 `1.7.9` 升到 **`1.8.0`**——新增公开资源键属功能性新增，同时删除 `Theme.Brush.ImageViewer.Checkerboard` 系列旧键为破坏性变更，按语义化版本取次版本号递增（`AssemblyInfo.cs` 不含显式版本特性，版本单一来源仍是 csproj）。
- 文档：README「主题」新增资源键行与《透明背景棋盘格》小节（含三档与 Geometry 用法），ImageViewer 属性表补充 `CheckerboardBrush`。
- Showcase：`Samples/Junevy.Controls.Showcase/Pages/WindowImagePage.xaml`「窗口与图像」页新增 `TransparentBackground` 演示组——三档棋盘格并排 + 用 `Path` 引用 `TransparentBackground.Geometry` 自绘，配合标题栏的主题切换按钮可直接看到换色效果。
- 验证：net8.0-windows 探针工程（`.workbuddy/tmp/TransparentBgProbe/`）37 项断言全部通过——资源解析（同一 `Geometry` 被三个 `DrawingGroup` 通过 `StaticResource` 共享引用可正常加载，无 Freezable 多引用异常）、Geometry 结构（Bounds=16×16、两块 8×8 位置逐块核对）、三档 `TileMode/Viewbox/Viewport` 参数、浅色配色取值、离屏渲染像素扫描（方格步长 4/8/16px；320×80 与 80×320 非方形控件横纵步长均为 8px 证明未被拉伸；全图仅 2 种颜色；单 16×16 单元四象限对角同色邻角异色证明是棋盘格而非条纹）、`ImageViewer` 默认 `CheckerboardBrush` 实际解析为非 null 的 Medium 档、`ThemeManager.ApplyTheme(Dark)` 后活元素 `DynamicResource` 重新解析到新 brush 实例且配色变为 `#2B2B30/#232327`、`ToggleTheme()` 回浅色后恢复 `#FFFFFF/#E0E4EA`。变异反证两次：把主题字典回退到改动前（无新键）→ 26 项 FAIL；把 Geometry 两块方格改成重叠的 `0,0,8,8` → 8 项 FAIL（步长、象限、颜色数均报错），确认断言可捕获缺陷。渲染快照 `board-light.png`、`board-dark.png`（三档并排）人工核对正常。探针工程为临时宿主，不入库。另用 `.workbuddy/tmp/ShowcaseBoardProbe/` 真实承载 `WindowImagePage` 跑 11 项断言：页面可视树中确实存在 Viewport 8/16/32 三档平铺画刷、`Path` 成功引用 `TransparentBackground.Geometry`（Bounds=16×16）、`ImageViewer.CheckerboardBrush` 走通用键解析为 16 档、`ApplyTheme(Dark)` 后页面画刷 alt 色由 `E0E4EA` 变为 `232327`；整页浅色/深色快照人工核对正常。



## DataGrid

### 本次更新（控件模板重构为官方 Aero2 宿主结构，修复行不生成/列头随滚动位移问题）— 2026-09-23

- **问题修复（严重）**：旧模板在 `ScrollViewer` 内容 Grid 中直接声明 `<DataGridRowsPresenter x:Name="PART_RowsPresenter"/>` 且缺少 `ItemsPresenter`。经对照 WPF 官方源码确认：`DataGridRowsPresenter` 必须由 `ItemsPresenter` 从 DataGrid 默认 `ItemsPanel`（静态构造函数注册的 `DataGridRowsPresenter` 工厂，运行时自动命名 `PART_RowsPresenter`）实例化才会被置 `IsItemsHost=true` 并挂为 `InternalItemsHost`；模板中直接声明的面板 `IsItemsHost=false`，行容器不会生成（用户怀疑正确）。同时旧结构把列头放进 `ScrollViewer` 的滚动内容中，垂直滚动时列头会随行滚出视口；`PART_ScrollContentPresenter` 为空壳，滚动管线断裂。
- **模板重构（对齐官方 `Aero2.NormalColor.xaml` 的 DataGrid 模板）**：`Border → ScrollViewer（Focusable=false，自定义 Template）→ ItemsPresenter`。ScrollViewer 自定义模板内 3×3 Grid：(0,0) 全选按钮、(0,1) `PART_ColumnHeadersPresenter`（列头固定不随行滚动）、(1,0-1) `PART_ScrollContentPresenter`（`CanContentScroll` 经 TemplateBinding 绑定 ScrollViewer）、(1,2) `PART_VerticalScrollBar`、(2,1) 横向滚动条 `PART_HorizontalScrollBar`（首列宽度绑定 `NonFrozenColumnsViewportHorizontalOffset`，冻结列场景与列头保持对齐）；ScrollViewer 的 `Content` 即 `ItemsPresenter`，由它实例化行宿主并经 ScrollContentPresenter 呈现——完整接通 DataGrid 内部 `EnsureInternalScrollControls` 的 `FindVisualParent<ScrollContentPresenter/ScrollViewer>` 滚动管线。
- **保留 Junevy 视觉**：外层圆角 Border、底部 1px 收边线（声明在滚动条之下，无横向滚动条时可见、出现时被覆盖）、空态提示 `atc:DataGridAssist.EmptyText`（随模板迁入 ScrollViewer 模板内，绑定由 `TemplatedParent` 改为 `AncestorType={x:Type DataGrid}`，功能不变）、全选按钮/列头/行/单元格样式均未改动。
- **样式补充（官方行为）**：`DefaultDataGridStyle` 增加官方 MultiTrigger——`IsGrouping=true` 且 `VirtualizingPanel.IsVirtualizingWhenGrouping=false` 时强制 `ScrollViewer.CanContentScroll=false`（分组面板按像素滚动才能正确布局）。
- 验证：net8.0-windows 探针工程（离屏渲染 @4x）断言全部通过——行生成（1000 项仅生成 9 个 `DataGridRow` 容器）、宿主链（`PART_RowsPresenter` 命名正确、`IsItemsHost=True`、位于 `ScrollContentPresenter` 与 `ScrollViewer` 之内）、垂直滚动（`ScrollToVerticalOffset(500)` 后 `VerticalOffset=500.00`）、虚拟化（滚动后容器数 10，`VirtualizationMode=Recycling`，二次滚动保持 10）、列头固定（垂直滚动前后列头 Y=0.8 不变）、列头对齐（水平滚动 120 后第 2 列列头与行单元格 X 坐标 Δ=0.00）、空态提示可见；渲染快照人工核对正常（列头与单元格对齐、底部横向滚动条正常显示）。探针工程保留于 `.workbuddy/tmp/DataGridProbe/`。

## 悬停/按压反馈统一（透明度方案推广至其余可交互控件）

### 本次更新（CardButton、ToolBarItem、ToolboxItem、ToolItem 及各内部图标按钮悬停/按压改为透明度反馈）— 2026-09-21

- 新增公共资源字典 `Generic/Style/FeedbackOpacity.xaml`，定义 **`Control.Hover.Opacity`（0.8）** 与 **`Control.Pressed.Opacity`（0.65）** 两个通用透明度键；Button 继续使用其独立的 `Button.Hover.Opacity` / `Button.Pressed.Opacity` / `Button.Disabled.Opacity`（已文档化，保持不变）。
- **A 类——背景暴露给用户的按钮/卡片类**（与 Button 同构问题：鲜艳背景悬停会被浅蓝灰底覆盖）：`CardButton`、`ToolBarItem`、`ToolboxItem`（触发按钮）、`ToolItem` 的 `IsMouseOver`/`IsPressed` 触发器不再把底色替换为 `Theme.Brush.Surface.Hover`/`Surface.Pressed`，改为整体 `Opacity` 反馈（悬停 0.8、按压 0.65）。其中 `ToolBarItem` 按压反馈由原字面量 `Opacity=0.7` 统一为 `0.65`（与 Button 一致）。
- **B 类——控件内部图标小按钮**：`MessageBar` / `ProgressBarWindow` 右上角关闭按钮、`ImageViewer` 工具栏图标按钮、`DialogWindow` 标题栏按钮、`ToggleButton` 的 Expander 展开按钮、`DatePicker` 日历导航/头部/下拉按钮，悬停/按压同样改为降透明度；关闭按钮悬停时额外的文字加深（Foreground Setter）一并移除（整体变淡本身即为反馈）。
- **C 类——中性表面上的列表/菜单项保留灰底高亮**（悬停灰底是行项的正确 affordance，降透明度会让行内容变淡、悬停感知几乎消失）：`MenuBar` / `ContextMenu` / `TabMenu` / `SideMenu` / `TreeMenu` 项、`ListBox` / `ListView` / `DataGrid` 行与表头、`DatePicker` 日历日期/月份单元格、`GroupBox` / `ExpanderPanel` 标题行均未改动。
- `DialogWindow` 关闭按钮的悬停危险色（Windows 关闭按钮惯例，透明背景无鲜艳覆盖问题）保留不变；各控件禁用态行为均未改动。
- 验证：net48 / net8.0-windows 双目标编译 0 错误（既有可空性警告数量不变，新增代码零警告）；离屏渲染探针 71 组断言全部通过——12 组模板（CardButton / ToolBarItem / ToolboxItem / ToolItem / ToggleButton Expander / MessageBar 关闭按钮 / ProgressBarWindow 关闭按钮 / ImageViewer 工具按钮 / DialogWindow 标题栏按钮 / DatePicker 日历导航 / 头部 / 下拉按钮）悬停/按压触发器均为仅 Opacity（0.8/0.65）且无固定 Background Setter；CardButton 红底（200×100 @4x）正常态中心像素 R=255 G=0 B=0 A=255，悬停模拟态 R=204 G=0 B=0 A=204（红色纯度保持、整体变淡，未掺灰蓝——旧方案悬停为 Surface.Hover 时 G/B 约为 190/200）；渲染快照人工核对正常。探针验证后已删除。

## Button

### 本次更新（悬停/按压改为透明度反馈；IsTextScaled 改为只缩小不放大）— 2026-09-21

- **悬停/按压反馈改为降透明度**：`ButtonTemplate`、`JvButtonTemplate` 的 `IsMouseOver`/`IsPressed` 触发器不再把底色替换为 `Theme.Brush.Surface.Hover`/`Surface.Pressed`（旧行为下红色等鲜艳背景按钮一悬停即变浅蓝），改为整体 `Opacity` 反馈——悬停 `0.8`、按压 `0.65`（触发器靠后者生效，按压覆盖悬停）；禁用逻辑保持 `Opacity=0.5` + 底色重置 `Surface.Base` + 描边 `State.DisabledBorder`。三个透明度集中在 `Button.xaml` 资源 `Button.Hover.Opacity` / `Button.Pressed.Opacity` / `Button.Disabled.Opacity`，便于统一调整。
- **无边框模板一并透明度化**：`NoBorderButtonTemplate` 移除悬停警告黄/按压危险红的固定底色，统一按上述透明度反馈（透明背景下表现为内容整体变淡）。
- **IsTextScaled 语义修正——只缩小不放大**：新增模板内部缩放宿主 `ShrinkBox`（internal `Decorator`，仅 `JvButtonTemplate` 消费）：测量按无约束尺寸取得内容自然尺寸，并把期望尺寸钳制在可用空间内；布局按自然尺寸居中排版后经 `RenderTransform` 视觉等比缩小，缩放系数以 1 为上限。空间充足时保持原始字号（与官方 Button 一致），仅当按钮被挤压（显式尺寸或布局约束小于内容自然尺寸）时文字/图标等比缩小并保持居中——修复「未设置 Height 时默认字体非常大」的问题（旧实现 Viewbox 会把内容放大填满按钮）。`IsTextScaled=False` 仍为完全固定字号（挤压时也不缩小）。
- Showcase 按钮页移除整页 `IsTextScaled=False` 规避隐式样式（默认值已安全），缩放演示改为「内容超出按钮时文字等比缩小」示例；README 的 Button 章节补充悬停/按压反馈与 `IsTextScaled` 说明（此前未收录该依赖属性）。
- 验证：net48 / net8.0-windows 双目标编译 0 错误（既有可空性警告数量不变，新增代码零警告）；离屏渲染探针 21 组断言全部通过——拉伸 400×260 与显式 180×56 场景文字墨迹与参考基准逐像素同尺寸（95×47 @4x，修复前会等比放大填满容器）、挤压（Width=44）文字等比缩小且宽高比不变（2.03 vs 2.02）、`IsTextScaled=False` 固定字号回归、无限空间下 DesiredSize 等于自然尺寸（Δ=0）、三个模板悬停/按压触发器均为仅 Opacity（0.8/0.65）且无固定底色 Setter、禁用态实时生效（Opacity=0.5 + 底色/描边重置）；渲染快照人工核对正常。探针验证后已删除。

## Toolbox

### 本次更新（拖拽发起即收起弹出窗口）— 2026-09-21

- **行为变更**：从 Popup 内的 `ToolItem` 发起拖放的那一刻（移动超出系统拖拽阈值、进入 `DoDragDrop` 之前），所属弹出窗口立即收起；旧行为为拖拽全程弹窗保持展开，拖放结束后才按指针位置决定是否收起。
- **收起方式满足「不参与命中测试」**：弹出层内容 `PART_PopupRoot` 先置为 `Collapsed`（不渲染、命中测试不可达，避免 `PopupAnimation=Fade` 淡出期间残留可见/可命中，拖放落点不会被弹层拦截），再经 `IsOpen=false` 关闭弹层；再次展开时由 `ToolboxItem.SetIsOpen` 恢复内容可见性。
- **状态一致性**：收起即清空 `ActiveItem` 与活动项状态（与正常收起一致）；`_dragOwner` 拖拽标记保留至拖拽完成，拖拽期间 `RequestOpen`（悬停触发器/弹层）与 `SetActiveItem`（点击/键盘切换入口）被短路，弹窗不会在拖拽中重新展开；`ClosePopup` 顺带取消未决的悬停展开计时器，消除「快速按下并拖动」与 `OpenDelay`（默认 150ms）展开延迟的竞态。
- 实现位置：`Toolbox.NotifyDragStarted`（收起弹窗 + 保持拖拽标记）、`Toolbox.RequestOpen`/`SetActiveItem`（拖拽进行中短路）、`ToolboxItem.HidePopupForDrag`（新增 internal 方法：`PART_PopupRoot` 置 `Collapsed`）、`ToolboxItem.SetIsOpen`（展开前恢复 `Visible`）。
- README 的 Toolbox 章节同步补充拖拽收起行为说明。
- 验证：net48 / net8.0-windows 双目标编译 0 错误（既有可空性警告数量不变）；离屏渲染探针断言全部通过（Toolbox 15 组 + Slider 10 组）——悬停展开回归（`RequestOpen` 路径正常）、拖拽进行中 `Popup.IsOpen=False` 且 `PART_PopupRoot.Visibility=Collapsed`、`ActiveItem` 清空、拖拽进行中 `RequestOpen`/`Toggle` 均不重新展开、拖拽结束后保持收起、再次悬停可正常展开且 `PopupRoot` 恢复 `Visible`、`ClosePopup` 后状态干净。探针验证后已删除。

## Slider

### 本次更新（滑块、轨道与选择区段全部改为直角）— 2026-09-21

- `SliderThumbStyle` 滑块移除 `ThumbBorder` 的 `CornerRadius="2"`：由 16px 方形 + 2px 小圆角改为 16px 直角方形握手。
- 轨道（`SliderRepeatButtonStyle` 内层 `Rail`）与选择区段（`PART_SelectionRange`）移除 `CornerRadius="{DynamicResource Theme.SmallCornerRadius}"`，统一为直角矩形；Slider 自此不再依赖 `Theme.SmallCornerRadius` 令牌（README 依赖清单同步移除）。
- 数值框外观仍复用库内 `jv:TextBox`（其圆角来自 TextBox 默认样式的 `Theme.ControlCornerRadius`），本次未改动；悬停描边高亮、拖拽填充主色、禁用置灰等状态行为不变，横/纵滑块共用同一模板一并生效。
- 验证：离屏渲染探针 10 组断言全部通过——横向状态下 `ThumbBorder`、`Rail`、`PART_SelectionRange` 三处模板部件的 `CornerRadius` 回读均为 `0,0,0,0`，`IsSelectionRangeEnabled` 时选择区段可见性正常；纵向滑块同模板复验（三处 `CornerRadius` 回读均为 0）。探针验证后已删除。

## TextBox / ComboBox

### 本次更新（TitleAssist 新增 TitleWidth 固定宽度附加属性）— 2026-09-21

- `atc:TitleAssist` 新增 **`TitleWidth`** 附加属性（double，默认 `NaN`），为标题区域指定固定宽度（DIP），面向表单式布局场景：`TitlePlacement=Left` 时各输入框标题长短不一会导致输入框左缘参差不齐，统一设置 `TitleWidth` 后所有输入框按同一间距整列对齐。
- **四个方位统一生效**：`Top` / `Bottom` / `Left` / `Right` 的标题呈现器均消费该宽度（默认模板经 `Width="{TemplateBinding atc:TitleAssist.TitleWidth}"` 绑定）。
- **对齐行为**：`Left` 方位标题内容在固定宽度内**右对齐贴合输入框**（保持固定 4 DIP 间距），`Right`/`Top`/`Bottom` 方位保持默认对齐；`TitleWidth` 为 `NaN` 时呈现器自适应标题内容，与既有行为完全一致（无破坏性变更）。
- 实现：`AttachedProperties/TitleAssist.cs` 注册 `TitleWidthProperty`（`RegisterAttached`，NaN 默认值）+ `GetTitleWidth`/`SetTitleWidth` 访问器；`jv:TextBox` 与 `jv:ComboBox` 两份默认模板的 4 个标题呈现器（`TitleTop`/`TitleBottom`/`TitleLeft`/`TitleRight`）统一加宽度绑定，其中 `TitleLeft` 呈现器设 `TextBlock.TextAlignment="Right"`（经附加属性继承送达内部文本；自动宽度下无视觉影响）。
- Showcase 输入页新增「固定宽度表单对齐」演示（用户名 / 电子邮箱地址 / 部门三行，`TitleWidth=110`，标题长短不一、输入框整列对齐）；README 的 TitleAssist 示例同步修正了旧版 `Tag` 占位符残留写法（改用 `atc:PlaceholderAssist.Placeholder`）。
- 验证：net48 / net8.0-windows 双目标编译 0 错误；离屏渲染探针 15 组断言全部通过——固定宽度生效（TextBox/ComboBox 的 `TitleLeft`、`TitleTop` 呈现器 `ActualWidth==120`）、未设置时自适应回归（NaN 默认值、呈现器宽度 41.6）、`TitleWidth=120` 时两个不同长度标题的输入框左缘精确同位（144 DIP）且自动宽度用例不受影响（65.6 DIP）、`TextAlignment=Right` 经继承链生效、像素级断言标题墨迹右缘距呈现器右缘仅 0.5 DIP（右对齐贴边）；渲染快照人工核对正常。探针验证后已删除。

## Slider

### 本次更新（滑块外观微调：正圆改为 2px 小圆角方形）— 2026-09-21

- `SliderThumbStyle` 滑块由 16px 正圆（`CornerRadius=8`）改为 16px 方形握手 + **2px 小圆角**：拖动滑块时与轨道、数值框的直角风格更协调，消除正圆带来的割裂感。
- 轨道与选择区段的圆角（`Theme.SmallCornerRadius`）不变；悬停描边高亮、拖拽填充主色、禁用置灰等状态行为不变；横/纵滑块共用同一滑块样式，一并生效。
- 验证：net48 / net8.0-windows 双目标编译 0 错误；离屏渲染探针 4 组断言通过（普通/选择区段/纵向/禁用四种状态下 `ThumbBorder.CornerRadius` 回读均为 2）；渲染快照人工核对正常。探针验证后已删除。

## TextBox

### 本次更新（新增内部命令按钮 CommandButton）— 2026-09-20

- `jv:TextBox` 内部新增命令按钮（模板部件 `PART_CommandButton`，与清空按钮同位显示在文本框内右侧），配套 4 个依赖属性（均注册在 `Junevy.Controls.Controls.Text.TextBox` 上，可直接设在 `jv:TextBox` 或原生 `TextBox` 实例上；样式/模板中需 `local:TextBox.*` 限定形式引用）：
  - **`ShowCommandButton`**（bool，默认 `false`）：是否显示命令按钮。
  - **`CommandButtonCommand`**（ICommand）：命令按钮点击时执行的命令，可绑定 ViewModel 命令；按钮可用性随命令 `CanExecute` 自动启停（`CanExecute=false` 时按钮禁用置灰）。
  - **`CommandButtonCommandParameter`**（object）：传递给命令的参数。
  - **`CommandButtonContent`**（object）：按钮内容（文本或 iconfont 字形），字体族跟随 `atc:Icon.FontFamily`，字号/颜色继承控件自身取值；为 `null` 时显示空白占位，建议显式设置。
- **与清空按钮互斥**：`ShowClear=true`（清空按钮显示）时命令按钮强制隐藏；需要显示命令按钮应设 `ShowClear="False"` + `ShowCommandButton="True"`。互斥由默认模板 `MultiTrigger` 实现：仅「`ShowCommandButton=true` 且 `ShowClear=false`」时命令按钮 `Visible`，其余状态保持元素默认 `Collapsed`。
- 实现：命令/参数/内容经 `RelativeSource TemplatedParent` 绑定送达模板内 `bt:Button`（复用 `TextBoxCloseButtonStyle` 无边框样式，宽 26、`Focusable=False` 不参与 Tab 焦点，与清空按钮一致）；`NoBorder` 系列极简模板不含该按钮（与清空按钮处理一致）。
- Showcase 输入页新增演示（命令经 `MessageBarService` 弹出通知，参数演示 `CommandButtonCommandParameter`）。
- 验证：net48 / net8.0-windows 双目标编译 0 错误（既有可空性警告数量不变，新增代码零警告）；离屏渲染探针 17 组断言全部通过——默认态命令按钮隐藏、`ShowCommandButton=true`+`ShowClear=false` 时显示且清空按钮折叠、两开关同开时清空按钮胜出（互斥）、运行时动态切换往返、内容字形渲染、`Command`/`CommandParameter` 模板绑定送达按钮、`CanExecute=false` 自动禁用；渲染快照人工核对正常（含禁用置灰态）。探针验证后已删除。

## 示例程序

### 本次更新（新增 Showcase 展示程序）— 2026-09-20

- 新增 `Samples/Junevy.Controls.Showcase`（net8.0-windows，已加入解决方案 Samples 分组并纳入 git 追踪）：使用本控件库的分类展示程序，运行方式 `dotnet run --project Samples/Junevy.Controls.Showcase`。
- 主窗口演示：无边框窗口 + `WindowChrome` + `jv:AppBar` 默认模板（DefaultAppBar，一个程序仅一个，展示默认状态；应用侧给 `jv:ToolBar` 设置 `WindowChrome.IsHitTestVisibleInChrome="True"` 使工具栏按钮在标题栏区内可点击，该附加属性可继承）、`jv:SideMenu` 八分类导航、`jv:SidePanel` 设置抽屉（CloseOnOutsideClick 自动收回）、`MessageBarService` 通知宿主、`ThemeManager` 浅色/深色切换。
- 分类页与虚拟数据（`SampleData`）：
  - 按钮：Button（图标/禁用/无边框样式/圆角）、CardButton 指标卡、ToggleButton 胶囊/矩形模板、RadioButton 圆形/方形分组。
  - 输入与选择：CheckBox 三态；TextBox 演示 PlaceholderAssist（含 iconfont 占位符）、Icon 前置图标、ShowClear、原生 TextBox 借用外观；TitleAssist 四方位标题 + iconfont/自定义字体样式；ComboBox（ItemsSource 绑定、声明项、可编辑、默认文案、原生占位）；DatePicker（DatePickerAssist.PlaceHolder）；Slider（ValueBoxSide 四侧、ValueFormatString、竖向、ShowValueBox=False）。
  - 集合与数据：ListBox 竖向/横向滑动、ListView GridView、DataGrid（虚拟数据 + 只读 + 排序）、DataGridAssist.EmptyText 空态提示。
  - 文本与状态：Label 全部 DisplayMode、TextBlock、ToolTip。
  - 通知：Badge（数值/圆点/四角/MaxCount/偏移微调）、MessageBar 布局内声明（IsShown/Show/Hide）、MessageBarService 五种外观 + Clear。
  - 布局：GroupBox（默认折叠/初始折叠/不可折叠/标题内交互元素）、ExpanderPanel（四方向 + AnimationDuration + ToggleCommand）、Border.CornerRadius 样式 Setter 用法。
  - 菜单与导航：ContextMenu（命令/快捷键/IsCheckable/子菜单）、TreeMenu（NavigateCommand + ExpanderBehavior 默认启用）、TabMenu（CanCloseLastTab=False + TabClosing 拦截「保护页」）、ToolBar 横/纵向、Toolbox 显式分组。
  - 窗口与图像：ImageViewer（FitToWindow/ActualSize，源为库内 pack URI 图片）、DialogWindow 代码实例化（内容含 TitleAssist 控件）。
- 实现备注（后续维护注意）：
  - `Border.CornerRadius` 的逐实例 attribute 写法（`<jv:Button Border.CornerRadius="8">`）会被 WPF 标记编译器拒绝（MC3015，`Border` 未提供附加属性 Get/Set 访问器），须改经样式 Setter 或代码 `SetValue` 设置；README 用法已同步修正。
  - `jv:Button` 默认 `IsTextScaled=True`：Viewbox 缩放宿主使按钮测量值占满可用空间（实测 600x200 容器内 DesiredSize 达 320x200），内容自适应布局（WrapPanel/横向 StackPanel）中应设 `IsTextScaled="False"`；展示页经页面隐式样式统一关闭，并保留一个固定尺寸（180x56）的缩放正面演示。
  - `Badge` 隐式样式将 `Foreground` 设为 `Text.OnAccent`（白色）并沿视觉树继承，包裹依赖继承前景色的内容时需显式设置前景色。
- 验证：Showcase（net8.0-windows）编译 0 错误 0 警告；离屏渲染探针（2x DPI）渲染主窗口浅色/深色主题、SidePanel 打开/收回、8 个分类页、MessageBar 泵 Dispatcher 帧（Visibility=Visible、Opacity=1.00）与角标行放大裁剪；断言全部通过——AppBar ToolBarItem 数量 ≥2、SidePanel IsOpen 读写回读一致、jv:TextBox 占位符空值显示/有值隐藏、MessageBar 动画完成后可见；渲染快照人工核对正常。探针验证后已删除。

## TextBox / ComboBox

### 本次更新（占位符统一为 PlaceholderAssist 附加属性，破坏性变更）— 2026-09-20

- 新增附加属性 **`atc:PlaceholderAssist.Placeholder`**（类 `Junevy.Controls.AttachedProperties.PlaceholderAssist`，类型 `object`，默认 `null`）：在 `TextBox` / `ComboBox` 内部显示占位内容，提示用户应输入或选择什么。内容为任意对象，可直接设为 iconfont 字形文本（占位符字体族跟随 `atc:Icon.FontFamily`，两个控件的默认样式均已内置 iconfont）。占位内容仅在控件无值时显示——`TextBox` 为文本为空且未聚焦，`ComboBox` 为未选中项；有值后自动隐藏。`jv:` 派生类型与原生 `<TextBox>`/`<ComboBox>` 借用默认外观的场景均生效。
- **破坏性变更（升级需同步修改 XAML）**：
  - **`ComboBox.PlaceHolder` 依赖属性已删除**（string，原默认值 `"Select an item..."`）。旧写法 `PlaceHolder="..."`（直接属性语法）不再存在，引用它的 XAML 会直接报编译错误，需改为 `atc:PlaceholderAssist.Placeholder="..."`。为保持 UI 连续性，`jv:ComboBox` 的隐式样式保留历史默认文案 `"Select an item..."`（未设置占位符时仍显示）；原生 `<ComboBox>` 默认无占位内容（与旧行为一致）。
  - **`TextBox` 占位符不再读取 `Tag`**：默认模板原先把 `Tag` 当占位文本，现改为读取 `atc:PlaceholderAssist.Placeholder`。原来写在 `Tag` 上的占位文案会静默失效（不报编译错误，但不再显示），需改为附加属性写法；`Tag` 恢复为普通用途。
  - 注意区分：`DatePicker` 的占位符仍是 `atc:DatePickerAssist.PlaceHolder`（string），本次不涉及。
- 实现要点：两个控件的占位符呈现元素由 `TextBlock` 改为 `ContentPresenter`（占位内容为 `object`，可承载任意内容）；禁用态触发器对占位符颜色的 Setter 相应改为附加属性形式 `Property="TextElement.Foreground"`。`ComboBox` 默认样式新增 `atc:Icon.FontFamily` Setter（与 TextBox 一致，为占位符 iconfont 提供字体）。
- 验证：net48 / net8.0-windows 双目标编译 0 错误（既有可空性警告数量不变）；离屏渲染探针 10 组断言全部通过——`jv:TextBox` 有占位符且空文本未聚焦时显示、有文本时隐藏、未设置时无内容、原生 `<TextBox>` 经附加属性生效、`Tag` 不再被模板消费（占位内容来自附加属性而非 Tag）；`jv:ComboBox` 默认文案保留、显式 iconfont 占位符未选中时显示、选中后隐藏、原生 `<ComboBox>` 生效、iconfont 字体族正确传递到占位符呈现器；渲染快照人工核对显示正常。探针验证后已删除。

## TextBox / ComboBox

### 本次更新（新增 TitleAssist 附加属性）— 2026-09-20

- 新增附加属性集 **`atc:TitleAssist`**（类 `Junevy.Controls.AttachedProperties.TitleAssist`），为 `TextBox` 与 `ComboBox` 在输入框外侧显示用途标题，内容为任意 `object`（可直接设 iconfont 字形文本）：
  - `Title`（object，默认 `null`）：标题内容；为 `null` 时不显示标题且不占用布局空间。
  - `TitlePlacement`（枚举 `TitlePlacement`：`Top`/`Bottom`/`Left`/`Right`，默认 `Top`）：标题方位，与输入框间距固定 4 DIP。
  - `TitleFontFamily`（默认 `null`）/ `TitleFontSize`（默认 `NaN`）/ `TitleForeground`（默认 `null`）/ `TitleFontWeight`（默认 `Normal`）：标题字体样式自定义；字体族/字号未设置时继承控件自身取值，颜色未设置时由默认样式提供主题次级文本色（`Theme.Brush.Text.Secondary`）。
- 实现要点（后续维护勿回退）：
  - 两个控件模板的 TargetType 是原生类型，模板通过 `{TemplateBinding atc:TitleAssist.*}` 读取，`jv:` 派生类型与原生 `<TextBox>`/`<ComboBox>` 借用默认外观的场景均生效。
  - 模板外层为 3×3 Grid，四个方位各一个 `ContentPresenter`（`TitleTop`/`TitleBottom`/`TitleLeft`/`TitleRight`），模板触发器按 `TitlePlacement` 切换显隐（无标题时全部折叠）。
  - 「未设置即继承」通过 `PriorityBinding` 实现：首选绑定（附加属性值经 `NullToUnsetValueConverter`，null/NaN 返回 `UnsetValue`）失败后自动落到第二绑定（控件自身 `FontFamily`/`FontSize`/`Foreground`）。**勿回退为单 Binding**——单绑定转换器返回 `UnsetValue` 时属性会被置为元数据默认值（字号 12），阻断属性继承（已实测验证）。
  - 新增 `Converters/NullToUnsetValueConverter.cs`、`AttachedProperties/TitlePlacement.cs`。
- 验证：net48 / net8.0-windows 双目标编译 0 错误（既有可空性警告数量不变）；离屏渲染探针 12 组断言全部通过——`jv:TextBox` 四个方位标题可见性与几何位置正确（以输入框 Border 为基准，间距 4 DIP）、无标题时不占布局空间、原生 `<TextBox>`、`jv:ComboBox`、iconfont 字形 + 自定义字体族/字号/字重/颜色、未设置字号时正确继承控件自身值（而非元数据默认 12）；渲染快照人工核对四个方位与 iconfont 标题显示正常。探针验证后已删除。

## TextBox / TreeMenu

### 本次更新（API 归属整理）— 2026-09-20

- **`ShowClear` 由附加属性改为 `jv:TextBox` 自身依赖属性（破坏性变更）**：上一轮引入的 `atc:TextBoxAssist.ShowClear`（类 `Junevy.Controls.AttachedProperties.TextBoxAssist`）已删除。判定依据：全库检索确认除 TextBox 自身模板外没有任何控件消费该属性语义——`Slider` 数值框只是复用 TextBox 外观并在样式里显式关闭清空按钮，不构成独立消费者，故按语义归属收编为 `TextBox.ShowClear` 依赖属性（bool，默认 `false`，`DefaultTextBoxStyle` 默认样式仍设为 `true`）。XAML 用法由 `atc:TextBoxAssist.ShowClear="True"` 改为 `ShowClear="True"`（直接属性语法仅 `jv:TextBox` 可用）。
  - 实现要点（后续维护勿回退）：`DefaultTextBoxStyle` 与 `DefaultTextBoxTemplate` 的 TargetType 是原生 `TextBox`（库支持原生实例借用外观），因此样式 Setter 与模板绑定统一使用限定形式——`<Setter Property="local:TextBox.ShowClear" ... />` 与 `{Binding Path=(local:TextBox.ShowClear), RelativeSource={RelativeSource TemplatedParent}}`。依赖属性值可经 `SetValue` 写入任意 TextBox 实例的属性存储，原生实例行为不变。
  - `Slider.xaml` 数值框样式同步改为 `<Setter Property="txt:TextBox.ShowClear" Value="False" />`（`txt` = `clr-namespace:Junevy.Controls.Controls.Text`）。
- **`DisplayMode` 枚举迁出 `AttachedProperties` 目录**：唯一消费者是 TreeMenu，参照 `LabelDisplayMode` 位于 `Controls/Text/` 的先例迁至 `Controls/Menu/DisplayMode.cs`，命名空间 `Junevy.Controls.AttachedProperties` → `Junevy.Controls.Controls.Menu`。`TreeMenu.xaml` 中 `x:Static atc:DisplayMode.Icon` 同步改为 `x:Static local:DisplayMode.Icon`；若有外部代码通过 `using Junevy.Controls.AttachedProperties` 使用该枚举，需同步更新 using（枚举成员与数值不变，XAML 中 `DisplayMode="Icon"` 的常规用法不受影响）。
- 验证：net48 / net8.0-windows 双目标编译 0 错误（40 项均为改动前既有可空性警告，本次改动文件 0 警告）；离屏渲染探针实测 5 组用例全部符合预期——`jv:TextBox` 默认（样式置 `true`）清空按钮可见、显式 `ShowClear=false` 隐藏、显式 `ShowClear=true` 可见、原生 `<TextBox>` 隐式样式借用外观时清空按钮可见、原生实例 `SetValue(JvTextBox.ShowClearProperty, false)` 后隐藏；渲染快照人工核对与预期一致；TreeMenu（`DisplayMode=Icon`）模板加载与渲染无异常，确认 `x:Static local:DisplayMode.Icon` 解析正常。探针验证后已删除。

## TextBox

### 本次更新（附加属性重构 + 清空按钮修复）— 2026-09-20

- **附加属性语义化重构（破坏性变更，升级需同步修改 XAML）**：
  - 删除 `AttachedProperties.AttachFuc` 杂烩类，按归属控件拆分：
    - `TextBox` 清空按钮显隐改用新附加属性 **`atc:TextBoxAssist.ShowClear`**（bool，类 `Junevy.Controls.AttachedProperties.TextBoxAssist`，默认 `false`）。`DefaultTextBoxStyle` 默认样式将其设为 `true`，常规用法无需手动设置；`Slider` 数值框等复用 TextBox 外观的场景显式设为 `false` 关闭。
    - `TabMenu` 页签关闭按钮显隐改用 **`TabMenu.IsClosable`** 控件自身依赖属性（bool，默认 `true`），不再走附加属性。
  - 删除 `AttachFuc.DisplayMode`（已注册但无模板读取的死属性）与历史拼写兼容属性 `AttachFuc.DispalyMode`。`AttachedProperties.DisplayMode` 枚举保留，`TreeMenu.DisplayMode` 仍正常使用。
  - 旧写法 `atc:AttachFuc.IsClosable` / `atc:AttachFuc.DisplayMode` / `atc:AttachFuc.DispalyMode` 在新版本中不再存在，引用它们的 XAML 会直接报编译错误，需按上表改为新 API。README 附加属性章节与控件依赖速查表已同步更新。
- **修复清空按钮字形错误导致的"✕ 不居中"**：清空按钮字形由 `E606` 改为 **`E639`**，与 `TabMenu` 页签、`MessageBar`、`ProgressBarWindow` 的关闭按钮统一。经离屏渲染放大确认，`E606` 实际是三维坐标轴图标（em 框内墨迹天然不对称），并非关闭"✕"——此前"不居中"的观感即源于此；换成 `E639` 后字形在按钮内自然居中。
- **修复清空按钮悬停色块超出 TextBox**：按钮加 `Margin=1`，悬停色块整体收在边框内侧，不再压住边框线与圆角。
- 验证：离屏渲染探针（net8.0-windows，`Themes/Generic.xaml` 实际加载）4 倍 DPI 像素级测量：清空按钮四周均位于 TextBox 边框内侧 1.6px（无越界）；`✕` 墨迹外接框中心与按钮几何中心偏差 dx=-0.16px、dy=-0.21px（视觉居中）；`ShowClear=false/true` 动态切换按钮正常隐藏/显示。控件库 net8.0-windows 与 net48 双目标编译通过，0 错误，无新增警告。

## Slider

### 本次更新（新增控件）— 2026-09-20

- 新增 `Slider` 滑块控件：`Controls/Box/Slider.cs`（`jv:Slider`，继承 `System.Windows.Controls.Slider`）+ `Controls/Box/Slider.xaml`（已注册到 `Themes/Generic.xaml`）。官方的拖拽、轨道分页、方向键/`Home`/`End`、刻度、选择区段行为全部沿用，新增能力是在滑块上/下/左/右任意一侧放置一个可手动键入数值的数值框（模板部件 `PART_ValueBox`，外观复用库内 `DefaultTextBoxStyle`）。命名空间 `Junevy.Controls.Controls.Box` 已在 `github.com.junevy` 的 `XmlnsDefinition` 内，`AssemblyInfo.cs` 无需改动。
- 新增依赖属性：
  - `ShowValueBox`（bool，默认 `true`）：数值框显隐；`false` 时数值框 `Collapsed`，轨道立即占满腾出的空间，运行时切换即时生效。
  - `ValueBoxSide`（`SliderValueBoxSide` 枚举 `Left`/`Top`/`Right`/`Bottom`，默认 `Right`）：数值框停靠侧，横竖滑块均可任选四侧；主轴侧限宽 `120`，交叉轴侧限宽 `160`、限高 `28`，避免数值框挤扁轨道。枚举文件 `Controls/Box/SliderValueBoxSide.cs`。
  - `ValueFormatString`（string，默认 `null`）：数值框显示格式（`F1`、`0.00`、`p0` 等），仅影响显示，键入时仍按数值解析。
- 键入与提交：输入过程中不改值，回车（`PreviewKeyDown`，不置 `Handled`，宿主的默认按钮等行为不受影响）或数值框失焦时提交；按当前区域性以 `NumberStyles.Float | AllowThousands` 解析，越界夹取到 `Minimum`/`Maximum`，非法文本（空串、非数字、`NaN`、无穷）不改值并把显示还原为当前值。写回一律用 `SetCurrentValue(ValueProperty, …)`，双向绑定不被截断。`OnValueChanged`/`OnMaximumChanged`/`OnMinimumChanged` 三个既有 protected 重写负责同步显示，未新增事件。
- 模板结构：横竖共用一套 5×5 网格模板（中心单元格放 `PART_Track`，四周 Auto 行/列放刻度与数值框，未使用的行/列自动收为 0）。交叉轴尺寸一律固定并居中——轨道细条 `4`、滑块 `16x16` 圆形、刻度 `4`，避免 `Track` 分配给元素的交叉轴空间影响视觉厚度。已分页段取 `Theme.Brush.Accent.Primary`、未分页段 `Theme.Brush.Surface.Sunken`、禁用态 `Theme.Brush.State.DisabledSurface`/`DisabledBorder`、选择区段 `Theme.Brush.Accent.Secondary`，焦点框沿用 `DefaultControlFocusVisualStyle`，深浅主题自动切换。
- 隐式样式：`DefaultSliderStyle`（keyed）+ 原生 `Slider` 与 `local:Slider` 各一条隐式样式。合并 `Themes/Generic.xaml` 后原生写法 `<Slider>` 直接获得本库外观；数值框的显隐与停靠侧绑定到 `jv:Slider` 自有属性，原生实例上绑定失败落到 `FallbackValue=Collapsed`，因此自动缺席且不留空隙。
- 实测确认的 WPF 平台约束（决定了实现方式，后续维护不要回退）：
  - `ControlTemplate.Triggers` 内的 `DataTrigger` 使用 `RelativeSource={RelativeSource TemplatedParent}` **不会解析**（setter 永不生效，实测数值框四侧全部停在默认位置）。因此数值框的显隐与停靠侧改由 `SliderValueBoxStyle` 的 `Style.Triggers` + `AncestorType={x:Type local:Slider}` 绑定驱动，模板内的 `PART_ValueBox` 不得再写任何与停靠侧相关的本地值（本地值会压过样式）。
  - 模板**不得**写 `PART_Track` 的 `Orientation`/`IsDirectionReversed`/`Value`/`Minimum`/`Maximum`：`Track.OnPreApplyTemplate` 会把这些属性经 `BindToTemplatedParent` 自动绑定到控件本体，但仅在属性仍为默认值时绑定；触发器一旦写入非默认值即顶掉该绑定，使用方再也改不动 `Slider.IsDirectionReversed`。
  - 官方纵向滑块本就是**最小值在下、向上增大**（`Track` 纵向 Normal 布局为 `|Inc|Thumb|Dec|`），模板不做任何反向处理。
  - `Track.IsDirectionReversed` 的依赖属性元数据不含 `AffectsMeasure`/`AffectsArrange`，运行时翻转方向需由外部再触发一次重排（探测中以改高度 + `UpdateLayout` 复现）。
  - `Slider` 定位 `PART_SelectionRange` 时只写 `Canvas.Left`/`Canvas.Top` 与主轴长度（横向 `Width`、纵向 `Height`），交叉轴厚度与居中必须由模板提供，故套一层 `Canvas` 宿主；该宿主 `IsHitTestVisible=False`，保证区段覆盖处的点击仍落到分页按钮。
  - 原实现的自写越界夹取（`Math.Max/Min`）已删除：`RangeBase.Value` 自带强制回调会把值夹进 `[Minimum, Maximum]`，去掉后全部越界断言仍通过（见下方变异反证）；保留的是 `NaN`/无穷拦截——强制回调对这两个值不做夹取，放行即会污染 `Value`。
  - `ValueFormatString` 原样交给 BCL，控件不做二次解释：.NET 自定义数字格式串会按字面复制无法识别的字符，实测 11 个畸形候选（`(bad format`、`0.0.0`、`%%`、`\q`、`e+e`、`#.#.#`、`..`、`0#`、`#0`、`0E+0%`、`'abc`）在 .NET 8.0.31 与 .NET Framework 4.8 上都不抛 `FormatException`，故 `catch (FormatException)` 分支为防御性代码（`double.ToString(string, IFormatProvider)` 的公开契约允许抛出）。
- 验证：控件库编译通过（net48 / net8.0-windows，0 错误，20 项均为改动之外的既有警告，三个新增文件 0 警告 0 错误）。临时探测工程 `SliderProbe` 跑 18 组用例、165 项断言，连续两轮全部通过；输入全部走 Win32 `SetCursorPos` + `SendInput` 真实点击/拖拽/键入（非 `RaiseEvent` 合成），每次点击前先断言"命中的元素正是目标"，并打印按下/抬起/`Thumb.DragStarted` 计数时间线。覆盖点：模板部件与隐式样式（含原生 `<Slider>` 反向对照——数值框自动缺席、轨道细条与滑块仍齐备）、`Value` 驱动滑块几何（含 0/100 端点）、数值框外观继承库内 `TextBox`、真实点击轨道半区按 `LargeChange` 分页、方向键/`Home`/`End`（横向忽略上下键、纵向向上增大，焦点归属逐例断言）、键入 + 回车提交、越界夹取与非法还原（含 `NaN`/`1E999`）、小数与负区间（zh-CN 区域性、千分位按分组解析）、失焦提交、真实拖拽后 `Track → Value` 反向同步、隐藏数值框不占布局、四侧布局不重叠与运行时切换、`ValueFormatString` 与 BCL 输出逐串对照、纵向（默认方向、轨道厚度、分页方向、`IsDirectionReversed` 可反向）、刻度显隐与位置、禁用态、主题切换、选择区段几何与点击穿透。**变异反证**三组：把 `Left` 侧停靠列改错并在纵向触发器里写入 `PART_Track.IsDirectionReversed` → 5 项断言失败（用例 12、14 精确复现）；删除自写夹取 → 全部通过，据此确认该夹取冗余并删除；去掉区段宿主的 `IsHitTestVisible=False` → 2 项失败并复现"区段吞点击"。`slider_light.png`/`slider_dark.png`/`slider_selection.png`（含选择区段可见状态）快照人工核对，四侧布局、刻度位置、禁用态与深浅主题配色与库内风格一致。探测工程验证后已删除。

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
- 修复系统按钮点击后变灰、无法还原窗口（`MenuBarAppBar` 与 `DefaultAppBar`）。根因：WPF 只**声明**了 `SystemCommands` 的 `Minimize/Maximize/Restore/CloseWindowCommand` 四个 `RoutedCommand`，**从不注册任何处理器**（`dotnet/wpf` 的 `SystemCommands.cs` 仅有命令声明与 `PostMessage(WM_SYSCOMMAND)` 静态方法；`Window`/`WindowChrome` 内均无 `SystemCommands` 绑定）。命令在可视化树上找不到 `CommandBinding` 时 `CanExecute` 恒为 `false`，而 `ButtonBase : CommandSource` 会据此自动禁用按钮——因此窗口最大化后重新模板化的还原按钮直接不可点击，宿主未自行注册绑定时三个按钮同样会在部分场景下失效。
- 新增 `Common/WindowSystemCommands.cs`（internal）：`EnsureRegistered(Window)` 为宿主 `Window` 补齐缺失的 `SystemCommands` 绑定，已注册的命令一律不覆盖、不重复注册（可多 `AppBar`、多次切换模板复用）。执行体调用 `SystemCommands.MinimizeWindow/MaximizeWindow/RestoreWindow`、`window.Close()`；`CanExecute` 按系统语义判定——`ResizeMode=NoResize` 时禁用最大化/还原，最大化/还原仅在 `WindowState` 与可调整大小状态匹配时可用；补齐后触发一次 `CommandManager.InvalidateRequerySuggested()`，让早于绑定生成的按钮重新查询可用性。
- `AppBar` 在 `OnApplyTemplate` 与首次 `Loaded` 时调用上述方法（每个实例只请求一次）。消费方若要完全接管，可自行注册这四个命令——先注册即生效，`AppBar` 不会替换；`DialogWindow` 与 `ProgressBarWindow` 已在自身构造函数内注册，行为不变，本次未改动。
- 验证：控件库编译通过（net48 / net8.0-windows，0 错误，无新增警告）；临时探测工程 `AppBarMenuBarProbe` 端到端探测 17 组断言全部通过（模板已应用、`Menu` 命中模板内隐式样式、顶层项与下层项样式来源、三按钮存在/顺序/贴右边缘、菜单位于应用名与按钮之间、最大化按钮初始字形与命令、菜单区在 chrome 中可命中、空白区保持可拖动、菜单不撑破标题栏高度、子菜单弹出与向下方向、二级子菜单右向弹出、最大化后字形与命令切为还原、还原后恢复），浅色/深色主题、菜单展开、最大化窗口 PNG 快照人工核对通过。回归探测工程 `AppBarDefaultProbe` 8 组断言全部通过（`DefaultAppBar` 仍为隐式样式模板、三按钮存在、最小化/关闭字形未受影响、最大化按钮初始字形与命令、最大化后切为还原字形/命令/提示、还原后恢复原值），最大化窗口快照人工核对 `ToolBar` 布局与图标区渲染不变。探测工程验证后均已删除。
- 系统按钮修复的验证：先复现根因——未注册绑定的宿主上四个 `SystemCommands` 命令 `CanExecute` 全为 `false`、`Execute` 无效果，仅缺少 `RestoreWindowCommand` 绑定时可稳定复现「最大化后还原按钮不可点击」；同时以 `WM_NCHITTEST`（返回 `HTCLIENT`）、`WindowFromPoint`、WPF 命中测试排除 `WindowChrome` 因素。修复后临时探测工程 `AppBarMaxProbe` 12 组断言在 net48 与 net8.0-windows、宿主绑定「全部缺失 / 部分注册 / 全部注册」三种组合下共 6 次运行全部通过（三按钮 `IsEnabled=True`；点击后窗口最大化、字形切为还原、命令切为 `RestoreWindowCommand` 且还原按钮保持可点击；再次点击窗口还原、命令与字形还原后可用性不变；`DefaultAppBar` 按钮同样可用；两个 `AppBar` 只注册一组共 4 条绑定；宿主已注册的 `[exec] Maximize`/`[exec] Restore` 处理器仍然优先生效）。另以 Win32 `SendInput` 真实点击（非 `Invoke`）完成最大化→还原往返，坐标落在还原按钮上且 `IsMouseOver=True`，最大化窗口快照人工核对菜单栏渲染正常。控件库重新编译通过（0 错误，40 项均为改动之外的既有警告）。探测工程验证后已删除。

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

### 本次修复（收回时机与宿主切换按钮冲突）— 2026-09-20

- 修复 2026-09-19 的 `CloseOnOutsideClick` 引入的回归：宿主按最常见写法「按钮 + `IsOpen` 双向绑定 + 命令把布尔值取反」控制面板时，点击按钮后面板**折叠后立刻又展开**。
- 根因（实测时间线，非推测）：收回动作当时发生在窗口级 `PreviewMouseDown`（按下），而按钮的 `Click` 由 `ButtonBase` 在**抬起**阶段触发、且早于 `MouseUpEvent` 冒泡到宿主窗口。按下瞬间控件把 `IsOpen` 写成 `false` 并经双向绑定回写数据源，命令随后读到的是已被收回的 `false`，取反得到 `true` → 面板重新展开。改动前代码的探测记录：`889ms 窗口看到按下 IsOpen=True` → `890ms IsOpen=False vm=False`（控件写回）→ `958ms 宿主按钮 Click vm=False` → `959ms IsOpen=True vm=True`（命令取反）→ `1220ms Opened`。
- 修复方式：收回判定拆成"按下标记 + 抬起执行"两步。窗口级 `PreviewMouseDown`（`handledEventsToo: true`）只记录"本次手势起于面板本体之外、且按下时面板已展开"，**不再立即写值**；窗口级 `Mouse.MouseUpEvent`（同样接管已处理事件）在按钮命令已同步执行完之后才判定：宿主此时若已自行收回（切换按钮场景）或本次点击本就是把面板打开（按下时为收起态，挂起条件不成立），控件都不再重复动作。
- 语义变化（对使用方可见）：外部点击的收回时机由"按下"改为"抬起"，与 WPF `Popup`  light-dismiss 及常见抽屉实现一致；内/外结论仍以**按下位置**为准（按下后拖出或拖入本体不改变本次判定），因此遮罩、空白区、兄弟按钮点击的收回结果与 2026-09-19 版本完全相同。
- 未改动部分：`Deactivated`/最小化收回、`CloseOnOutsideClick=false` 的整体关闭、`SetCurrentValue` 写回（不破坏绑定）、`PopupRoot` 内点击天然豁免、句柄随 `Loaded`/`Unloaded`/属性切换幂等挂摘。`Controls/Panel/SidePanel.cs` 之外的文件无改动。
- 验证：控件库重新编译通过（net48 / net8.0-windows，0 错误，40 项均为改动之外的既有警告）。临时探测工程 `SidePanelToggleProbe` 以 Win32 `SendInput` 真实点击跑 12 组用例、84 项断言，连续两轮全部通过；每次点击前断言"命中的元素正是目标"，点击后断言"按下与抬起确实送达窗口"（区分环境吞点击与控件逻辑错误），并逐用例打印 `IsOpen`/VM 值/`Opened`/`Closed`/`Click` 时间线。**反向对照**：同一探测回跑改动前的提交版本，用例 1、2、12 复现回弹（时间线见上），证明断言确实能捕获该缺陷而非静默假通过。覆盖点：无遮罩时按钮命令收回不回弹、遮罩开启且按钮在面板 Grid 之外（AppBar 场景）收回不回弹、收起态点击按钮展开后不被同一次抬起收回、`CloseOnOutsideClick=false` 时完全由命令掌控、遮罩点击收回并回写绑定、空白区点击收回、面板内按钮点击保持展开、`ButtonBase` 已 `Handled` 抬起的兄弟按钮仍触发收回、失焦收回、面板内 `ComboBox` 下拉项真实选中且面板不收回、开关关闭时遮罩与失焦均不收回、连续两次点击（展开动画期间反向）最终状态与事件计数正确；`case1_closed`/`case2_closed`/`case3_open` PNG 快照人工核对收回与展开的视觉状态正确。探测工程验证后已删除。


### 本次更新（点击外部自动收回 + 滑动曲线优化）— 2026-09-19

- 新增依赖属性 `CloseOnOutsideClick`（bool，默认 `true`）：展开时点击面板本体以外的区域、宿主窗口失焦（`Deactivated`）或最小化（`StateChanged`）都自动收回面板；置为 `false` 后收回完全由宿主通过 `IsOpen`/`Toggle()` 控制。
- 修复"展开后点击其他（空白）区域不会自动折叠"。根因：`SidePanel` 此前**没有任何输入处理**——`PART_Backdrop` 只做视觉呈现（无 `MouseDown` 处理），`IsBackdropEnabled=false` 时面板以外根本没有命中面，因此空白区域点击完全无响应。
- 实现方式（本节描述为当时的实现，收回时机已在 2026-09-20 由"按下"改为"抬起"，见上一节）：在宿主 `Window` 上 `AddHandler(Mouse.PreviewMouseDownEvent, handler, handledEventsToo: true)`。预览路由保证先于兄弟控件取得点击，`handledEventsToo` 保证兄弟控件已把 `MouseDown` 标记为处理后仍不漏判；处理过程**不设置** `Handled`，所以面板以外的按钮等控件照常响应同一次点击。挂接/摘除时机与 `Toolbox` 的宿主窗口管理语义保持一致：`Loaded` 且开关为 `true` 时挂接，`Unloaded` 或开关置 `false` 时完整摘除三类句柄（幂等，重复挂接先 detach）。
- 内/外判定基准为 `PART_Content`（遮罩虽是模板组成部分，语义上属于"面板以外"），并采用"祖先链命中本体 + 点击点几何位于本体范围内"双条件：后者保证多个抽屉叠放时，压在别人遮罩下的本体检索不会被误判为外部点击。收回通过 `SetCurrentValue(IsOpenProperty, false)` 写回，双向绑定时数据源同步更新且不破坏绑定。
- 面板内 `ComboBox`、`ContextMenu` 等弹层内容位于独立 `PopupRoot` 顶层窗口，宿主窗口级处理收不到其中的点击，因此操作面板内弹层不会误收回。
- 滑动/淡入曲线由 `CubicEase`（EaseInOut）改为 `SineEase`（EaseInOut），时长仍为 `250ms`（滑动、本体淡入、遮罩淡出三处同步替换）。用户反馈的"展开时有点卡顿"经实测确认**不是性能也不是掉帧**：`RenderCapability.Tier=2`（全硬件渲染）、DPI 125%、60Hz，动画期间帧间隔中位 14~17ms 无 >20ms 空洞、`Measure`/`Arrange` 计数为 0（无重排）、UI 线程排队延迟 ≤4ms；关闭 `Effect`、关闭 `ClipToBounds`、加 `BitmapCache`、换成 200 行重内容各变体测得的帧数与帧间隔均与基线一致（仅人为把阴影 `BlurRadius` 调到 120 才复现出 9 帧/29.8ms/5 个 >20ms 空洞）。真正的原因是曲线速度分布：`CubicEase` 峰值速度为均速 1.875 倍，250ms 内仅约 15 帧时中段单帧位移达 63~68 DIP（约 80~85 物理像素）而首三帧仅 0.4/3.0/8.2 DIP，观感即"起步停顿 + 中段一跳"；`SineEase` 峰/均值比为 π/2≈1.57，实测同一动画的逐帧位移由 `0.4 3.0 8.2 … 68.4 …` 变为 `4.0 31.0 … 38.2 … 4.0`，峰值位移下降约 44% 且首帧即有位移。`ExpanderPanel` 等其他控件的曲线未改动。
- 附带确认（对使用方有意义）：宿主应用必须自行合并 `Themes/Generic.xaml`，否则 `Theme.Brush.Overlay.Backdrop` 解析为 `null`，遮罩 `Background` 为空而不参与命中测试——这也是遮罩看起来"完全没反应"的常见外部原因。
- 验证：控件库编译通过（net48 / net8.0-windows，0 错误，40 项均为改动之外的既有警告）。临时探测工程 `SidePanelDismissProbe` 以 Win32 `SendInput` 真实点击（非 `RaiseEvent` 合成）跑 11 组用例、123 项断言，连续两轮全部通过且每次点击前都断言"命中的元素正是目标"（避免点错位置也判通过的静默假阳性）：遮罩空白点击收回、面板内按钮点击保持展开且 `Click` 触发、`IsBackdropEnabled=false` 时点击兄弟按钮既收回又 `e.Handled=false`、`CloseOnOutsideClick=false` 时遮罩点击与失焦均不收回、双向绑定回写 `IsOpen`、失焦收回、最小化收回、双抽屉几何判定（A 保持 + B 收回）、`PopupRoot` 下拉项真实点击选中 `Beta` 且面板不收回、句柄随属性切换与 `Unloaded` 正确摘除。临时性能探测工程 `SidePanelPerfProbe` 输出上述帧间隔/逐帧位移/曲线数学对照表。两个探测工程验证后均已删除。

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
