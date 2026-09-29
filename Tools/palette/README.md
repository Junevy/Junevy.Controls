# 配色脚本（Aperture 色卡的唯一真值来源）

`Themes/AppColors.Light.xaml` 与 `Themes/AppColors.Dark.xaml` 由这里生成，**不要手改生成结果**。

| 文件 | 作用 |
| --- | --- |
| `spec.js` | 原语阶梯 `Pal`、角色映射 `Roles`、效果色 `Effect`，以及 `hsl()` / `palOf()` / `solveToLstar()` / `solveInRamp()` 取色工具 |
| `check.js` | 91 条对比度与感知台阶断言，两主题各跑一遍 |
| `emit.js` | 生成两个主题字典；生成前先跑状态色「双重职责」守卫与中性线彩度封顶，不达标直接退出 |

```bash
node Tools/palette/check.js    # 只看数
node Tools/palette/emit.js     # 校验 + 写回 Themes/AppColors.*.xaml
```

## 改一个颜色的正确姿势

1. 在 `spec.js` 里定位要改的档位。**需要新色值时按目标 L\* 反解，不要手挑 hex**：
   ```js
   const sp = require('./spec.js');
   sp.solveInRamp(sp.Pal.Yellow, 56.9);             // 已有阶梯里最接近 L*56.9 的一档
   sp.solveToLstar(45.6, { h: 40.6, s: 90 });       // 固定色相饱和，解出该 L* 的 hex
   sp.lstar('#A08700'); sp.ratio('#A08700', '#FBFCFD'); sp.chroma('#A08700');
   ```
2. 只改 `Pal`（原语）或 `Roles`（角色 → 原语的映射）。`Theme.Color.*` / `Theme.Brush.*` 都是派生物。
3. `node Tools/palette/check.js` 必须 91/91，再 `node Tools/palette/emit.js`。
   `emit.js` 的守卫会检查：状态色当色块填充时文字是否够清楚、当无边框前景时是否够 4.5:1、
   悬停是否朝「更抢眼」的方向走（浅色变深 / 深色变亮）、色板与表面是否分得开、
   中性线峰值彩度是否 ≤7（超过就说明手挑的蓝灰又回来了）、静止边框是否还落在「若有若无」的 1.6–2.3 区间、
   焦点底色是否只比卡片挪半步（ΔL\* 3–6，不许借用 `Surface.Sunken` 的大台阶）。
   唯一的放行项是**浅色** `Status.Warning`：真黄（色相角 88° 以上）压在近白卡片上做不到 4.5:1，
   故 `check.js` / `emit.js` 对浅色单独写下 3.3 / 3.2 / 3.4 的下限，深色仍走 4.5。
4. 跑一次 Showcase 探针离屏截图复核肉眼观感——数字过了不等于好看。

## 为什么 `Theme.Color.*` 要逐条写死十六进制

WPF 里 `Color` 资源不能引用另一个 `Color`，只有 `SolidColorBrush` 能通过
`Color="{StaticResource …}"` 取 `Color`。所以 `Theme.Color.*` 只能是 `Palette.*` 的字面镜像，
每行用行尾注释标出处。`emit.js` 写完会回读文件做三项自检：两主题 `Palette` 阶梯逐行一致、
每条镜像等于它声称的原语、没有悬空的 `StaticResource` 引用。

## 阴影换色不要「补偿」不透明度

`DropShadowEffect` 合成时**不读取 `Color` 的 alpha 通道**，浓淡只由 `Opacity` 决定。
离屏实测：同一 `Opacity=0.40` 下把色值从 `#1A070B10` 换成 `#FF070B10`，控件下方 1–9px 的
相对压暗量恒为 `21.93%`，一位不变；只把 `Opacity` 从 `0.18` 调到 `0.12` 才降到 `6.19%`。

所以换阴影载体色时只换色相，`BlurRadius` / `ShadowDepth` / `Opacity` 三项一律照抄旧值。
按「色 alpha × Opacity 守恒」去反向回调 `Opacity` 是错的——那样浅色三档会凭空淡 34%、深色淡 17%。
（`Palette.Effect.Shadow` 仍写成 ARGB：它同时供 `Theme.Brush.Effect.Shadow` 当画刷，画刷吃 alpha。）
