# 边框可覆盖性（BorderBrush / BorderThickness）全库治理 实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 让库内所有「宿主可见外框」真正消费 `Control.BorderBrush` / `Control.BorderThickness`，做到默认观感不变但宿主可覆盖（含 `Transparent` 与 `0`）。

**Architecture:** 统一到库内既有 Button 范式——模板层 `{TemplateBinding}`，默认值由默认样式 Setter 以 `DynamicResource` 注入主题令牌（样式 Setter 优先级低于实例本地值，故「保留默认 + 可覆盖」同时成立）。状态触发器只覆盖颜色、不再覆盖厚度，使 `BorderThickness="0"` 成为全状态无边框的可靠开关。

**Tech Stack:** WPF / XAML（net8.0-windows + net48 双 TFM）、`IValueConverter`、临时探测工程 + `RenderTargetBitmap` 像素断言 + 变异反证。

**Spec:** 本文件即 spec（2026-10-28 与用户逐条确认，无独立设计文档）。

## Global Constraints

- **契约（已裁决）**：`BorderBrush` 管**静息态**边框色，悬停/焦点/禁用态仍由主题接管（全库一致的状态反馈，不放开）；`BorderThickness` 管**全状态**，触发器内写死的厚度必须删掉或改成相对值。
- **不新增任何公开依赖属性**。治理只针对边框，**不动任何 `Background`**（TreeView L280 等写死的背景本轮不开放）。
- **ToggleButton 不参与本轮**（2026-10-29 用户指示）。`Controls/Button/ToggleButton.xaml` L30/L112 的硬编码与 `ToggleButton.xaml.cs:14` 的 `TrackBorderThickness` 常量镜像、以及 :89/:173 的 `State.DisabledSurface` 疑似笔误，全部**保持原状**，只在收尾单独列出交还用户。
- **TabControl 内容区边框透明时，页签之间的分割线一起消失**（已裁决：不拆独立分割线元素）。
- **B 类保留硬编码**（刻意内部分割线/装饰，不得开放）：所有 `PagingFooter` 分隔线、DataGrid 底部收边 Rectangle L265-270、GroupBox `ContentArea` L97、DialogWindow 标题栏下沿 L133、AppBar `MaxHeight=1` 分隔行 L122、ImageViewer 两处半透明底 L104/L185 + 5 条走 `Background` 的竖分隔线、MessageBar `AccentStripe`、ComboBox `FocusBorder` L421-423、TextBox `NoBorderTextBoxTemplate_WPF` L521、MenuBar 下拉 Popup L72-78、ContextMenu 子菜单 Popup L110-115、Toolbox 浮层 L130、CodeEditor 补全列表 L112、各控件指示条。
- **已裁决不得推翻**：TabControl 的 `HeaderCornerRadius`/`ContentCornerRadius` 方向筛选器保留；换行后「非末行选中页签底边缺 1px」保持现状。
- **不 commit / 不 add / 不 stash**，改动一律留在工作区（用户会自行 commit + bump）。
- 验收 = 探针断言 + 变异反证 + 截图目检；`dotnet build` 0 错误且不新增警告（历史 CS86xx 约 24 条不算新增）。仓库根 `dotnet build` 必失败（sln 引用三个磁盘上不存在的工程），一律对 `Junevy.Controls.csproj` 与 `Samples/Junevy.Controls.Showcase/Junevy.Controls.Showcase.csproj` 分别构建。
- 本执行环境**没有交互输入桌面**（`SetCursorPos` 返回 `ERROR_ACCESS_DENIED`），悬停/焦点态只做**结构断言**（触发器声明顺序、目标名、令牌键），行为层证明只能标「需实机目检」，不得假绿。
- 1px 线的像素证据必须在 **96dpi** 位图上取（`new RenderTargetBitmap((int)root.ActualWidth, (int)root.ActualHeight, 96, 96, Pbgra32)`，尺寸不乘 scale）；给人看的图另行放大。
- 同一时刻只允许一个执行者改库源码；变异反证期间不得并行跑其他库探针。

---

### Task 1: Thickness 筛选转换器

**Files:**
- Create: `Converters/ThicknessFilterConverter.cs`
- Modify: `Controls/Menu/TabControl.xaml:20-21` 区域（注册实例）
- Test: `.workbuddy/tmp/BorderProbe/`

**Interfaces:**
- Produces: `Junevy.Controls.Converters.ThicknessFilterConverter`，属性 `string Mode { get; set; }`，取值 `"All"`（默认，透传）/ `"NoBottom"`（底边归零）/ `"NegativeTop"`（输出 `Thickness(0, -Top, 0, 0)`）。Task 3 以 `x:Key="NoBottomThickness"`、`x:Key="NegativeTopMargin"` 消费。

- [ ] **Step 1: 写失败断言**

在探针里对三种 Mode 各断言输出：输入 `new Thickness(1,1,1,1)` → `NoBottom` 得 `1,1,1,0`；`NegativeTop` 得 `0,-1,0,0`；输入 `new Thickness(3)` → `NoBottom` 得 `3,3,3,0`、`NegativeTop` 得 `0,-3,0,0`；输入非 Thickness（如 `null`）→ 透传 `new Thickness(0)`，不抛。

- [ ] **Step 2: 跑一遍，确认因类型不存在而编译失败**
- [ ] **Step 3: 实现**

```csharp
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Junevy.Controls.Converters
{
    /// <summary>
    /// Thickness 筛选转换器：按 Mode 输出边框厚度。
    /// NoBottom 用于选中页签（去掉与内容区衔接的底边，且保留宿主设定的其余三边）；
    /// NegativeTop 把内容区上移自身上边线厚度，使该线延伸进页签行充当页签间分割线。
    /// </summary>
    public class ThicknessFilterConverter : IValueConverter
    {
        /// <summary>
        /// 变换方式，默认原样透传。
        /// </summary>
        public string Mode { get; set; } = "All";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Thickness thickness)
            {
                return new Thickness(0d);
            }

            string mode = parameter as string ?? Mode;
            return mode switch
            {
                "NoBottom" => new Thickness(thickness.Left, thickness.Top, thickness.Right, 0d),
                "NegativeTop" => new Thickness(0d, -thickness.Top, 0d, 0d),
                _ => thickness
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
```

- [ ] **Step 4: 跑断言确认全绿**
- [ ] **Step 5: 变异反证**：把 `"NoBottom"` 分支改成 `_ => thickness` 透传，断言 Step 1 中 NoBottom 用例 FAIL；还原。

---

### Task 2: TreeView 根外框

**Files:**
- Modify: `Controls/Menu/TreeView.xaml:281-282`（模板）
- Modify: `Controls/Menu/TreeView.xaml:255-269`（`DefaultTreeViewStyle` 补 Setter）

**Interfaces:**
- Consumes: 无
- Produces: 官方 `<TreeView>` 与 `jv:TreeView` 的根外框开始消费实例 `BorderBrush`/`BorderThickness`。

- [ ] **Step 1: 写失败断言**

夹具：默认 `<TreeView>`（不设任何边框属性）+ 覆盖实例 `BorderBrush="Transparent"` + 覆盖实例 `BorderThickness="0"` + 覆盖实例 `BorderBrush="#FF00FF00" BorderThickness="3"`。
断言：沿可视树取根模板第一个 `Border`（ItemsPresenter 的祖先）：
- 默认实例：`BorderBrush` 与 `Theme.Brush.Border.Default` **同一实例**（`ReferenceEquals`），`BorderThickness == new Thickness(1)` → 证明默认观感未变。
- Transparent 实例：`BorderBrush is SolidColorBrush sb && sb.Color == Colors.Transparent`。
- 0 实例：`BorderThickness == new Thickness(0)`。
- 自定义实例：色为 `#FF00FF00`、厚度 3。
再断言该 Border 的 `Background` **仍是** `Theme.Brush.Surface.Base` 实例（本轮刻意不开放背景）。

- [ ] **Step 2: 跑一遍，确认 Transparent/0/自定义三组 FAIL（当前模板不读）**
- [ ] **Step 3: 改模板**（L281-282）

```xml
                        <Border
                            Background="{DynamicResource Theme.Brush.Surface.Base}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            ClipToBounds="True"
                            CornerRadius="{DynamicResource Theme.ControlCornerRadius}">
```

- [ ] **Step 4: 补默认样式 Setter**（插在 `<Setter Property="Foreground" …/>` 之后）

```xml
        <Setter Property="BorderBrush" Value="{DynamicResource Theme.Brush.Border.Default}" />
        <Setter Property="BorderThickness" Value="1" />
```

- [ ] **Step 5: 跑断言确认全绿 + 变异反证**：删掉 Step 4 的两条 Setter，默认实例那条必须 FAIL（证明 Setter 承重）；还原。

---

### Task 3: TabControl 内容区 + 两种页签 + 写死厚度 + 死 Setter

**Files:**
- Modify: `Controls/Menu/TabControl.xaml:20-21`（注册转换器）
- Modify: `Controls/Menu/TabControl.xaml:29-30`（`DefaultTabItemStyle` 的 `BorderThickness` 死值 → 真值 + 补 `BorderBrush`）
- Modify: `Controls/Menu/TabControl.xaml:54-55`、`:100`（原生页签）
- Modify: `Controls/Menu/TabControl.xaml:127-128`、`:220`（`jv:TabControlItem` 页签）
- Modify: `Controls/Menu/TabControl.xaml:246-250`（`DefaultTabControlStyle` 补 Setter）
- Modify: `Controls/Menu/TabControl.xaml:279-285`（内容区 Border）

**Interfaces:**
- Consumes: Task 1 的 `NoBottomThickness` / `NegativeTopMargin`
- Produces: `TabControl.BorderBrush/BorderThickness` → 内容区；`TabItem.BorderBrush/BorderThickness` → 页签（WPF 原生语义，二者不联动，需宿主分别设置）。

- [ ] **Step 1: 写失败断言**

夹具四台：默认 `jv:TabControl`；内容区 `BorderBrush="Transparent"`；内容区 `BorderThickness="0"`；页签 `TabItem.BorderBrush="Transparent"`。
断言组：
- C1 默认内容区 Border 的 `BorderBrush` = `Theme.Brush.Border.Default` 实例、`BorderThickness == 1`、`Margin == new Thickness(0,-1,0,0)`。
- C2 Transparent 生效。
- C3 厚度 0 生效且 `Margin.Top == 0`（不再上移 1px）。
- C4 厚度 3 生效且 `Margin.Top == -3`。
- C5 页签容器 `border` 默认色/厚度同令牌值。
- C6 **选中**页签 `BorderThickness` 底边为 0、其余三边等于容器自身厚度（默认 1 → `1,1,1,0`，与改动前逐字段一致）。
- C7 宿主把页签厚度设 `0` 时，选中页签四边全 0（证明触发器不再写死绝对值）。
- C8 内容区 `BorderBrush=Transparent` 时，页签间分割线随之消失（同一 Border，已裁决行为）——结构断言：内容区 Border 的 `BorderBrush` 为 Transparent。
- C9 `DefaultTabItemStyle` 上 `BorderThickness` Setter 的值 = 模板实际取到的值（不再有「Setter 0 / 模板 1」的死值矛盾）。

- [ ] **Step 2: 跑一遍，C2/C3/C4/C7 预期 FAIL**
- [ ] **Step 3: 注册转换器**（紧接现有 `BottomCornerRadius` 之后）

```xml
    <cvt:ThicknessFilterConverter x:Key="NoBottomThickness" Mode="NoBottom" />
    <cvt:ThicknessFilterConverter x:Key="NegativeTopMargin" Mode="NegativeTop" />
```

- [ ] **Step 4: 修 `DefaultTabItemStyle` 的容器默认值**（L29-30 区域，`BorderThickness` 原为压死在模板字面量下的死值）

```xml
        <Setter Property="Padding" Value="0" />
        <Setter Property="BorderBrush" Value="{DynamicResource Theme.Brush.Border.Default}" />
        <Setter Property="BorderThickness" Value="1" />
```

- [ ] **Step 5: 两处页签模板改 TemplateBinding**（L54-55 与 L127-128，两份模板逐字相同，都要改）

```xml
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
```

- [ ] **Step 6: 选中态去底边改成相对值**（L100 与 L220 两处 `<Setter TargetName="border" Property="BorderThickness" Value="1,1,1,0" />`）

```xml
                            <Setter TargetName="border" Property="BorderThickness" Value="{Binding BorderThickness, RelativeSource={RelativeSource TemplatedParent}, Converter={StaticResource NoBottomThickness}}" />
```

> **本步必须先单独验证**：`ControlTemplate.Triggers` 的 `Setter.Value` 里用 `RelativeSource=TemplatedParent` + Converter 能否解析。判据走 C6/C7 两条断言（读**选中容器**的 `border.BorderThickness` 生效值，不看推断）。若解析失败（生效值退化为 `Thickness(0)` 或绑定表达式 `Status != Active`），改用回退方案：`Path=(attach:Icon.FontFamily)` 同族的具名 `RelativeSource={RelativeSource AncestorType={x:Type TabControl}}` 不可用（会丢页签级覆盖），届时改为「宿主厚度 × 是否选中」由容器 `BorderThickness` 属性直读——不得未验证就写进 CHANGELOG。

- [ ] **Step 7: 内容区 Border 开放**（L279-285）

```xml
                        <Border
                            Grid.Row="1"
                            Margin="{Binding RelativeSource={RelativeSource TemplatedParent}, Path=BorderThickness, Converter={StaticResource NegativeTopMargin}}"
                            Background="{DynamicResource Theme.Brush.Background.App}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="{Binding RelativeSource={RelativeSource TemplatedParent}, Path=(local:TabControl.ContentCornerRadius), Converter={StaticResource BottomCornerRadius}}">
```

- [ ] **Step 8: `DefaultTabControlStyle` 补 Setter**（插在 `<Setter Property="Background" …/>` 之后）

```xml
        <Setter Property="BorderBrush" Value="{DynamicResource Theme.Brush.Border.Default}" />
        <Setter Property="BorderThickness" Value="1" />
```

- [ ] **Step 9: 跑断言全绿 + 变异反证**
  - M：删 Step 8 的 `BorderThickness` Setter → C1 FAIL（默认厚度退化 0）。
  - M：把 `NegativeTopMargin` 的 `Mode` 改 `"All"` → C1/C3/C4 的 Margin 断言 FAIL。
  - M：Step 6 退回写死 `1,1,1,0` → C7 FAIL。
  - 每个变异跑前 `diff -q` 备份确认变异真的落地（`sed` 缩进写错会静默无操作）。

---

### Task 4: TextBox 聚焦态写死厚度 + 零引用死模板

**Files:**
- Modify: `Controls/Text/TextBox.xaml:474-478`（删 IsFocused 的 `BorderThickness` Setter）
- Modify: `Controls/Text/TextBox.xaml:502-518`（删整块 `NoBorderTextBoxTemplate`）

**Interfaces:**
- Consumes: 无（主模板 L323-324 与样式 L545-546 已合规）
- Produces: `BorderThickness="0"` 在聚焦态仍为 0。

- [ ] **Step 1: 写失败断言**
  - X1 默认 `<TextBox>`：模板根 `Border` 色=令牌、厚度=1。
  - X2 `BorderThickness="0"` + `ApplyTemplate` + 置 `IsFocused=True`（程序化 `Focus()`，若环境拿不到焦点则改判触发器声明断言并标注需实机）：`Border.BorderThickness == 0`。**改动前此条 FAIL**。
  - X3 `BorderBrush="Transparent"` 静息态生效。
  - X4 `NoBorderTextBoxTemplate` 资源键在合并后的字典里**查不到**（已删）；`NoBorderTextBoxTemplate_WPF` **仍在**（被 TabControl.xaml:189 消费，不得误删）。

- [ ] **Step 2: 跑一遍确认 X2 FAIL、X4 前半 FAIL**
- [ ] **Step 3: 删掉聚焦态写死的厚度那一行**（保留同 Trigger 内其余两条 Setter；注意 L473/L481 有历史乱码注释，**只删 L476 一行，不要顺手改写注释**）

```xml
            <Trigger Property="IsFocused" Value="True">
                <Setter TargetName="Border" Property="BorderBrush" Value="{DynamicResource Theme.Brush.Accent.Primary}" />
                <Setter TargetName="Border" Property="Background" Value="{DynamicResource Theme.Brush.Surface.Focused}" />
            </Trigger>
```

- [ ] **Step 4: 删除 `NoBorderTextBoxTemplate` 整块**（`<ControlTemplate x:Key="NoBorderTextBoxTemplate" TargetType="{x:Type local:TextBox}">` 到其闭合标签；其 `TextBoxHost` 配的是 `BorderBrush=Accent.Primary` + `BorderThickness=0`，画刷永不可见，且 grep 确认库内与 Showcase 零引用）
- [ ] **Step 5: 全绿 + 变异反证**：把 L476 加回去 → X2 必须 FAIL；删掉 `_WPF` 模板 → X4 后半与 TabControl 页签重命名探针 FAIL。
- [ ] **Step 6: 观察项（不设判据，只记录）**：厚度 3 时清空按钮的 `Margin="1"` 是否让悬停色块压到边框线；把实测截图留在报告里，如实写「厚度 >1 时内边距补偿不足」。

---

### Task 5: CheckBox + RadioButton 标记 + 死 CornerRadius Setter

**Files:**
- Modify: `Controls/Box/CheckBox.xaml:20-21`、`:102-107`
- Modify: `Controls/Button/RadioButton.xaml:34-35`、`:131-132`、`:209-218`

**Interfaces:**
- Consumes: 无
- Produces: 两个标记的 `CheckBorder` 消费实例边框属性；均匀厚度下对勾/圆点仍居中。

- [ ] **Step 1: 写失败断言**
  - K1 默认 `<CheckBox>`：`CheckBorder` 色=令牌、厚度=1.5。
  - K2 `BorderBrush="Transparent"` 生效。
  - K3 `BorderThickness="0"` 生效，且 `CheckBorder.ActualWidth/Height` 仍为 18（固定宽高，厚度不计入外尺寸）。
  - K4 厚度 0/1.5/3 三种下，**勾选后** `CheckMark` 的墨迹中心与 `CheckBorder` 中心偏差 ≤0.5 DIP（96dpi 位图差分；无法构造则退化为结构断言 + 标注需实机）。
  - K5 `DefaultCheckBoxStyle` 与 `DefaultRadioButtonStyle` 上**不存在** `Border.CornerRadius` Setter（模板自取令牌，该 Setter 是死值）。
  - R1-R5 对 `jv:RadioButton` 两套模板（`Rect` / `Circular`）各跑一遍同构断言——**两份模板必须同步**，否则 `DisplayMode` 一换行为就分叉。

- [ ] **Step 2: 跑一遍确认 Transparent/0 组 FAIL**
- [ ] **Step 3: 模板改 TemplateBinding**（CheckBox L20-21；RadioButton L34-35 与 L131-132，共三处，两份 RadioButton 模板逐字同改）

```xml
                    BorderBrush="{TemplateBinding BorderBrush}"
                    BorderThickness="{TemplateBinding BorderThickness}"
```

- [ ] **Step 4: 两个默认样式各补一对 Setter**（`DefaultCheckBoxStyle` 与 RadioButton 的样式；缺失会让默认厚度从 1.5 退化成 WPF 默认 0，默认观感直接变）

```xml
        <Setter Property="BorderBrush" Value="{DynamicResource Theme.Brush.Border.Default}" />
        <Setter Property="BorderThickness" Value="1.5" />
```

- [ ] **Step 5: 删死 Setter**：CheckBox.xaml:107 与 RadioButton.xaml:218 的 `<Setter Property="Border.CornerRadius" Value="{DynamicResource Theme.SmallCornerRadius}" />`（模板里 `CornerRadius` 直接取令牌、不读它）。
- [ ] **Step 6: 全绿 + 变异反证**：删 Step 4 的 `BorderThickness` Setter → K1 FAIL；只改 RadioButton 一份模板 → 另一份的 R 组 FAIL（证明「两份都要改」这条有判别力）。
- [ ] **Step 7: 记录限制**：标记类控件只支持**均匀**厚度；非均匀（如 `1,1,1,0`）会让标记偏心 0.5 DIP。写进 README，不做强制夹取（收窄契约是库既有风格，但夹取属新增行为）。

---

### Task 6: ComboBox 下拉浮层

**Files:**
- Modify: `Controls/Box/ComboBox.xaml:450-451`

**Interfaces:**
- Consumes: 样式 L557-558 已有的 `BorderBrush`/`BorderThickness` Setter
- Produces: 主体框与下拉浮层共用同一对实例属性。

- [ ] **Step 1: 写失败断言** B1 默认浮层 `DropDownBorder` 色=令牌/厚度=1；B2 `BorderBrush="Transparent"` 后主体与浮层同时透明；B3 `BorderThickness="0"` 后两者同时 0；B4 `FocusBorder`（光晕层）**仍**是写死的厚度 1 与 `Opacity=0`（B 类，不得被误开放）；B5 `PART_EditableTextBox.BorderThickness` 仍为 0。
- [ ] **Step 2: 跑一遍确认 B2/B3 FAIL**
- [ ] **Step 3: 改 L450-451**

```xml
                                    BorderBrush="{TemplateBinding BorderBrush}"
                                    BorderThickness="{TemplateBinding BorderThickness}"
```

- [ ] **Step 4: 全绿 + 变异反证**（把 `FocusBorder` 也改成 TemplateBinding → B4 必须 FAIL，证明「只开放该开放的」）。
- [ ] **Step 5: 记录**：`BorderThickness="0"` 会让 `AllowsTransparency=True` 的浮层彻底失去轮廓、与页面内容糊在一起——这是宿主选择的语义，README 必须写明。

---

### Task 7: 六处浮层/窗口外框

**Files / 逐条编辑（每处 = 模板改 TB + 该控件默认样式补一对 Setter）:**

| # | 文件 | 模板行（现值 → 新值） | 补 Setter 的样式 | 默认值 |
|---|---|---|---|---|
| 1 | `Controls/Dialog/DialogWindow.xaml` | 115 `Border.Default`→TB Brush；116 `1`→TB Thick | `:99` 隐式 `TargetType=local:DialogWindow` | `Border.Default` / `1` |
| 2 | `Controls/Bar/AppBar.xaml` | 451 `Border.Default`→TB Brush；452 `1`→TB Thick | `:464` 隐式 `TargetType=local:AppBar` | `Border.Default` / `1` |
| 3 | `Controls/Bar/InfoBar.xaml` | 118 `Border.Default`→TB Brush；119 `1`→TB Thick | `:172-178` `DefaultInfoBar` 所属样式 | `Border.Default` / `1` |
| 4 | `Controls/Bar/MessageBar.xaml` | 65 `Border.Default`→TB Brush；66 `1`→TB Thick | `:157-163` MessageBar 样式 | `Border.Default` / `1` |
| 5 | `Controls/Image/ImageViewer.xaml` | 81 `Border.Default`→TB Brush；82 `1`→TB Thick | `:73-75` 隐式样式 | `Border.Default` / `1` |
| 6 | `Controls/Progress/ProgressBarWindow.xaml` | 68 `Border.Default`→TB Brush；69 `1`→TB Thick | `:60-61` 隐式样式 | `Border.Default` / `1` |

**Interfaces:**
- Consumes: 无
- Produces: 六个宿主可见外框可覆盖。**不动** DialogWindow 标题栏下沿、AppBar `MaxHeight=1` 分隔行、ImageViewer 半透明底与 `Background` 分隔线、MessageBar `AccentStripe`。

- [ ] **Step 1: 写失败断言**（每控件一组）F-n-1 默认色=令牌/厚度=1；F-n-2 Transparent 生效；F-n-3 厚度 0 生效；F-n-4 该控件的 B 类内部线**仍**写死（防误伤）。DialogWindow/ProgressBarWindow 是 `Window`，断言用 `GetTemplateChild`/可视树取 `RootBorder`；二者构造期本地设过 `Background=Transparent`，需确认 Setter 未被本地值遮蔽。
- [ ] **Step 2: 跑一遍确认 F-n-2/F-n-3 全 FAIL**
- [ ] **Step 3: 按表逐处改模板 + 补 Setter**（六处 Setter 全部缺失，必须成对补齐，否则默认变无边框）
- [ ] **Step 4: 全绿 + 变异反证**：任选一处（ImageViewer）删掉 Setter → F-5-1 FAIL；把 AppBar L122 分隔行误改成 TB → F-2-4 FAIL。
- [ ] **Step 5: 记录两条语义**：`jv:MessageBar` 由 `MessageBarService` 创建、宿主拿不到实例，只能靠隐式样式覆盖；`jv:AppBar` 的 `BorderBrush` 只作用于 `Mode=Expandable` 的抽屉浮层，Default/MenuBar 模式下无可见效果（分隔行属 B 类不开放）。

---

### Task 8: MenuBar 根 Border 补属性

**Files:**
- Modify: `Controls/Menu/MenuBar.xaml:126-131`（补两个属性）、`:113-119`（`JunevyMenuBarStyle` 补 Setter）

**Interfaces:**
- Consumes: 无
- Produces: `<Menu>` 的 `BorderBrush`/`BorderThickness` 不再静默无效。

- [ ] **Step 1: 写断言** N1 默认 Menu 根 Border `BorderThickness == 0`（**当前不画框，改后必须仍不画**）；N2 宿主设 `BorderThickness="1" BorderBrush="#FFFF0000"` 后根 Border 取到该值；N3 下拉 Popup 面板（L72-78）仍写死令牌/1。
- [ ] **Step 2: 跑 N2 确认 FAIL（现状属性缺失，宿主值无处可去）**
- [ ] **Step 3: 补模板属性 + 样式 Setter**

```xml
                                        BorderBrush="{TemplateBinding BorderBrush}"
                                        BorderThickness="{TemplateBinding BorderThickness}"
```

```xml
        <Setter Property="BorderBrush" Value="{DynamicResource Theme.Brush.Border.Default}" />
        <Setter Property="BorderThickness" Value="0" />
```

> 厚度默认必须给 `0`（不是 1），否则菜单栏凭空多一圈框、违反「默认观感不变」。

- [ ] **Step 4: 全绿 + 变异反证**：Setter 的 `0` 改 `1` → N1 FAIL。

---

### Task 9: 全量回归、构建与截图

- [ ] **Step 1:** 库双 TFM 构建：`dotnet build Junevy.Controls.csproj -c Debug`，要求 0 错误、警告数不高于改动前基线（先记录基线）。
- [ ] **Step 2:** Showcase 构建：`dotnet build Samples/Junevy.Controls.Showcase/Junevy.Controls.Showcase.csproj -c Debug`。
- [ ] **Step 3:** 探针全量跑一遍（含所有 Task 的组），记录 `report.txt` 与 N 组断言数；断言无条件打印实际值，不要只在失败时打印。
- [ ] **Step 4:** 出 96dpi 证据图：每台控件并排「默认 / Transparent / 厚度 0 / 厚度 3」四列，另出一张 2 DIU→1px 放大图供目检；每张图加「末行必须整行不透明」的出图完整性自检。
- [ ] **Step 5:** 主题切换复核：在活元素上 `SetResourceReference` 后切 Dark 再读，断言默认态边框色跟着主题换（证明没被 `StaticResource` 固化）。
- [ ] **Step 6:** 变异反证收尾四步：还原源码 → 库 `--no-incremental` 重建 → Showcase 重建 → 探针再跑一遍全绿。**缺一不得删探针**（否则 `bin/` 里留的是最后一个变异版的产物）。

---

### Task 10: Showcase + 文档

- [ ] **Step 1:** Showcase 对应页各加一组「默认 / Transparent / 厚度 0 / 厚度 3」对照演示（涉及 TreeView、TabControl、CheckBox、RadioButton、ComboBox、MessageBar、InfoBar、ImageViewer、DialogWindow、ProgressBarWindow、Menu），片段收进 `ShowcaseSnippets.cs`。
- [ ] **Step 2:** README：按控件章节各补一行可照抄写法 + **契约原文**——`BorderBrush` 只管静息态、悬停/焦点/禁用仍走主题；要**全状态无边框**请写 `BorderThickness="0"`；标记类控件只支持均匀厚度；ComboBox 浮层厚度 0 会失去轮廓；AppBar 的边框只在 Expandable 模式生效；`TabControl.BorderBrush` 不联动页签，页签要单独在 `TabItem` 上设。
- [ ] **Step 3:** CHANGELOG：先 `git log` + 查 `Junevy.Controls.csproj` 的 `<Version>`（当前 3.3.0，用户会自行 bump），条目写新版本；注明更新时间、影响面（**行为兼容、默认观感不变**）、以及「ToggleButton 本轮未纳入」这一条。
- [ ] **Step 4:** 更新 skill `junevy-controls`：第 7 节高频陷阱加「`BorderBrush` 只作用于静息态，全状态无边框用 `BorderThickness="0"`」；第 5 节控件清单同步。按 AGENTS.md 要求检索 Skill 是否需更新——本轮功能变更需要。
- [ ] **Step 5:** 单独交还两份清单：① 本轮**未动**的 ToggleButton 边框（含 `:89`/`:173` 的 `State.DisabledSurface` 疑似笔误、`xaml.cs:14` 常量镜像）；② 全库 B 类保留清单（说明为何刻意写死）。
- [ ] **Step 6:** 不 commit、不 add、不 stash；`git status` 复核改动面后停在待验收的工作区。

---

## Self-Review 结论

- **Spec 覆盖**：三类失效（模板不读 / 触发器写死厚度 / 属性缺失）分别落在 Task 1-8、Task 3-4、Task 8；契约与 B 类保留写在 Global Constraints；ToggleButton 已整体移出并在 Task 10 Step 5 单独交还。
- **无占位符**：Task 7 用表格把六处的文件、行号、新旧值、目标样式、默认值逐行写全，不依赖「参照上文」。
- **待验证而非推断的一处**：Task 3 Step 6 的「Trigger Setter.Value 里 `RelativeSource=TemplatedParent` + Converter 是否解析」，已写成必须靠 C6/C7 实测判定并给出退路，禁止先写进 CHANGELOG。
