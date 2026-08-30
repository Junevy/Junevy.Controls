# Hover Toolbox Control Design

## 1. 目标

为 `Junevy.Controls` 增加一套独立的 WPF 悬浮工具箱控件，提供类似工业视觉软件工具栏的一级分组体验：

- `Toolbox` 常驻显示一层 `ToolboxItem`。
- 鼠标停留在 `ToolboxItem` 上约 150 ms 后展开其悬浮工具面板。
- 悬浮面板紧邻触发项，并根据可用空间在右、左、下、上之间回退。
- 面板默认总宽度为 300 DIP，每行固定 4 个工具，超出后向下换行。
- 面板中的 `ToolItem` 默认显示上方图标和下方标题。
- `ToolItem` 可点击执行命令，也可拖到 Canvas；拖放载荷是工具数据，而不是 UI 控件实例。
- 支持 `ItemsSource`、样式、数据模板、键盘访问以及浅色/深色主题。

## 2. 非目标

本次不实现以下能力：

- 多级树形工具分类。
- 可停靠窗口、浮动窗口或 AvalonDock 集成。
- 工具搜索、收藏、最近使用、分页和虚拟化网格。
- Canvas 节点工厂、连线、撤销重做和流程序列化。
- 修改现有 `ToolBar`、`ToolBarItem`、`ListBox` 或 `ListView` 的行为。

## 3. 技术选型

采用独立控件族，而不是继承现有 `ToolBar`：

```text
Toolbox : ItemsControl
└── ToolboxItem : HeaderedItemsControl
    ├── PART_TriggerButton
    └── PART_Popup
        └── ScrollViewer
            └── ItemsPresenter
                └── UniformGrid (Columns = 6)
                    └── ToolItem : Button
```

选择理由：

- 现有 `ToolBar` 使用单向 `VirtualizingStackPanel`，不提供网格换行。
- `ListView` 的列和详细信息语义与本需求不匹配。
- `ListBox` 的核心语义是选择，而工具项的核心语义是命令和拖放。
- `ItemsControl + UniformGrid` 可以直接表达“固定列数、自动增加行”。
- `Popup` 能越过父级裁剪区域，适合紧邻工具栏显示悬浮面板。

## 4. 控件职责

### 4.1 Toolbox

`Toolbox` 是一级分组容器和唯一的交互协调者：

- 生成 `ToolboxItem` 容器。
- 统一管理打开/关闭计时器。
- 保证任意时刻最多只有一个分组展开。
- 管理当前活动项、窗口失活、卸载和拖动状态。
- 向子项提供面板宽度、列数、最大高度、位置偏好和拖放格式。

公开属性：

```csharp
public sealed class Toolbox : ItemsControl
{
    public Orientation Orientation { get; set; }             // Vertical
    public TimeSpan OpenDelay { get; set; }                  // 150 ms
    public TimeSpan CloseDelay { get; set; }                 // 300 ms
    public double PopupWidth { get; set; }                   // 300 DIP
    public int ColumnCount { get; set; }                     // 4
    public double PopupMaxHeight { get; set; }               // 480 DIP
    public ToolboxPopupPlacement PopupPlacement { get; set; }// Auto
    public string DragDataFormat { get; set; }               // Junevy.Controls.Tool
    public ToolboxItem? ActiveItem { get; }

    public void ClosePopup();
}
```

`OpenDelay`、`CloseDelay` 不允许负值；`PopupWidth` 必须大于 0；`ColumnCount` 至少为 1；`PopupMaxHeight` 必须大于 0。无效值由依赖属性验证回调拒绝。

### 4.2 ToolboxItem

`ToolboxItem` 表示一级触发分组。它继承 `HeaderedItemsControl`，标准的 `Items`、`ItemsSource`、`ItemTemplate` 和 `ItemContainerStyle` 继续有效。

公开属性：

```csharp
public sealed class ToolboxItem : HeaderedItemsControl
{
    public object? Icon { get; set; }
    public string? Title { get; set; }
    public ToolboxDisplayMode DisplayMode { get; set; } // IconOnly
    public bool IsOpen { get; }
}
```

`ToolboxItem` 负责生成内部 `ToolItem` 容器，但不自行决定何时打开；鼠标、点击和键盘意图都提交给所属 `Toolbox`。

### 4.3 ToolItem

`ToolItem` 表示实际工具，继承 `Button`，因此保留 `Command`、`CommandParameter`、`Click`、焦点和禁用状态。

公开属性：

```csharp
public sealed class ToolItem : Button
{
    public object? Icon { get; set; }
    public string? Title { get; set; }
    public ToolboxDisplayMode DisplayMode { get; set; } // IconAndTitle
    public bool IsDragEnabled { get; set; }              // true
    public object? DragData { get; set; }
    public string? DragDataFormat { get; set; }
}
```

普通数据对象作为子项时，`ToolboxItem` 自动把该数据对象作为生成容器的默认 `DragData`。显式声明 `ToolItem` 时，调用者可以直接设置 `DragData`。

### 4.4 枚举

```csharp
public enum ToolboxDisplayMode
{
    IconOnly,
    IconAndTitle
}

public enum ToolboxPopupPlacement
{
    Auto,
    Right,
    Left,
    Bottom,
    Top
}
```

## 5. 交互状态

打开规则：

1. 鼠标进入一个启用且包含子项的触发按钮。
2. `Toolbox` 取消旧的待打开目标，并为新目标启动 `OpenDelay`。
3. 计时结束时再次验证目标仍有效且鼠标仍在触发区。
4. 关闭旧 `ActiveItem`，再打开新项，保证不出现两个 Popup。

关闭规则：

1. 鼠标同时离开触发按钮和 Popup 后启动 `CloseDelay`。
2. 鼠标在关闭前返回任一区域时取消关闭。
3. 拖动过程中禁止自动关闭。
4. 拖放完成后根据指针位置决定保留或关闭。
5. 窗口失活、最小化、控件卸载、活动项禁用或清空时立即关闭。

点击和键盘规则：

- 点击触发按钮切换对应 Popup。
- `Enter` 或 `Space` 切换 Popup。
- `Escape` 关闭 Popup 并把焦点还给触发按钮。
- Popup 打开后，方向键可以在工具项之间移动焦点。
- `ToolboxItem` 只有 Icon 时，`Title` 同时用于 ToolTip 和 `AutomationProperties.Name`。

## 6. 布局

- `PopupWidth` 默认 300 DIP，指的是 Popup 边框的总宽度。
- `ColumnCount` 默认 4，使用 `UniformGrid.Columns` 严格固定列数。
- 边框、Padding 和垂直滚动条占用宽度后，剩余内容宽度平均分给 4 列；不再额外硬编码项宽，避免互相冲突。
- `ToolItem` 默认高度 68 DIP，图标建议 24 至 28 DIP。
- `ToolboxItem` 和 `ToolItem` 的图标与可见标题之间保留 5 DIP 间距。
- 标题单行居中，使用省略号，不允许文本改变列宽或行高。
- 水平滚动条始终禁用；内容超过有效最大高度时启用垂直滚动。
- 最后一行不足 4 项时保持左对齐的网格位置，不拉伸为更大的项目。

## 7. Popup 定位与多屏

`PART_Popup` 使用 `PlacementMode.Custom` 和 `CustomPopupPlacementCallback` 返回候选位置：

- 垂直 `Toolbox` 的 Auto 顺序：Right、Left、Bottom、Top。
- 水平 `Toolbox` 的 Auto 顺序：Bottom、Top、Right、Left。
- 显式指定方向时，该方向优先，其他方向仍作为空间不足时的回退。

候选位置使用 WPF DIP 计算。当前显示器工作区通过目标窗口句柄和 Win32 `MonitorFromWindow`/`GetMonitorInfo` 获取，再通过当前 `PresentationSource` 的 `TransformFromDevice` 转换为 DIP。有效最大高度为：

```text
min(PopupMaxHeight, 当前显示器工作区高度 - 16 DIP)
```

窗口移动、尺寸变化或跨显示器后，如果 Popup 仍打开，则触发一次无视觉跳变的重新定位。不得使用只代表主显示器的 `SystemParameters.WorkArea`。

## 8. 拖放

- 左键按下时记录起点，不立即拖动。
- 移动距离超过 `SystemParameters.MinimumHorizontalDragDistance` 或 `MinimumVerticalDragDistance` 后开始拖动。
- 使用 `DragDropEffects.Copy`，因为从工具箱拖到 Canvas 的语义是创建新实例。
- `DataObject` 的格式来自 `DragDataFormat`，默认值为 `Junevy.Controls.Tool`。
- 载荷是 `DragData`；不得把 `ToolItem`、模板元素或其他 UI 对象作为载荷。
- 一旦启动拖动，本次鼠标序列不得再触发 Button 的 `Click`。
- `IsDragEnabled=false` 时保持普通 Button 行为。

Canvas 消费示例：

```csharp
private void Canvas_OnDrop(object sender, DragEventArgs e)
{
    const string format = "Junevy.Controls.Tool";
    if (!e.Data.GetDataPresent(format))
    {
        return;
    }

    object toolDefinition = e.Data.GetData(format);
    Point position = e.GetPosition((IInputElement)sender);
    viewModel.AddTool(toolDefinition, position);
}
```

## 9. 样式与主题

- 新增 `Controls/Toolbox/Toolbox.xaml`，由 `Themes/Generic.xaml` 合并。
- 颜色、边框、阴影和圆角全部使用现有 `Theme.*` 动态资源。
- 默认模板使用 `Icon.FontFamily`、`Icon.IconSize` 和 `Icon.IconForeground` 附加属性；图标颜色由 `IconForeground` 控制，标题颜色由控件 `Foreground` 独立控制。
- `ToolboxItem` 和 `ToolItem` 共用图标在上、标题在下的视觉构成，但保留独立样式键，避免语义耦合。
- 不修改现有 `DefaultToolBarItemStyle`，避免对 AppBar 和已有用户造成回归。

## 10. 验收标准

1. 默认 150 ms 后打开，快速划过不打开过期分组。
2. 任意时刻最多一个 Popup 打开。
3. 300 DIP 面板每行严格 4 项，第 5 项进入第二行，第 9 项进入第三行。
4. 大量工具触发垂直滚动，不出现水平滚动或列数变化。
5. 从触发项移动到 Popup 不闪退；离开两者约 300 ms 后关闭。
6. 拖动期间 Popup 不关闭，Drop 后不会额外执行 Click。
7. 触发项靠近屏幕边缘时可回退到其他方向，Popup 不超出当前显示器工作区。
8. 多显示器和不同 DPI 下定位正确。
9. `ItemsSource` 数据项和显式声明容器两种用法都正常。
10. `net8.0-windows` 与 `net48` 均能编译，现有控件样式不回归。
