# Junevy.Controls 更新日志

本文档记录 `Junevy.Controls` 控件库中 `TreeMenu` 与 `TreeMenuItem` 的历史变更与本次迭代内容。

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
