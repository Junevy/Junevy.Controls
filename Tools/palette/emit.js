/*  Generates Themes/AppColors.Light.xaml and Themes/AppColors.Dark.xaml
    from the locked Aperture spec in aperture-data.js.                    */
const fs = require('fs');
const path = require('path');
const { Pal, Roles, Effect, resolve, ratio, lstar, chroma } = require('./spec.js');

const IDX = { light: 0, dark: 1 };
const hex = (v) => v.toUpperCase();
const refOf = (theme, key) => Roles[key][IDX[theme]];
const at = (theme, key) => resolve(refOf(theme, key));

/* ---------------- guard: status colours do double duty ----------------
   Label / Badge / MessageBar / AppBar / DialogWindow use Status.* as a chip
   FILL (with Text.OnAccent on top) and, in Borderless* modes, as the FOREGROUND
   itself. Both readings must hold or one control type breaks.               */
function guard(theme) {
  const fails = [];
  const need = (label, got, min) => { if (got < min - 1e-9) fails.push(`${label} → ${got.toFixed(2)} (需 ≥ ${min})`); };
  const surface = at(theme, 'Surface.Base');
  const onAccent = at(theme, 'Text.OnAccent');

  for (const s of ['Info', 'Success', 'Warning', 'Danger']) {
    const fill = at(theme, `Status.${s}`);
    const subtle = at(theme, `Status.${s}Subtle`);
    // 浅色 Warning 是唯一的例外：真黄（色相角 97°+）在近白卡片上做不到 4.5:1，
    // 用户选定「抬黄优先」，故这一档放行到 AA 大字号（3.4 / 3.3 / 3.2）。深色不受影响。
    const warnFloor = s === 'Warning' && theme === 'light' ? { chip: 3.4, text: 3.3, onSubtle: 3.2 } : { chip: 4.5, text: 4.5, onSubtle: 4.5 };
    need(`${s} 色块上的 OnAccent 文字`, ratio(onAccent, fill), warnFloor.chip);
    const hoverKey = `Status.${s}Hover`;
    if (Roles[hoverKey]) {
      const hover = at(theme, hoverKey);
      need(`${s}Hover 色块上的 OnAccent 文字`, ratio(onAccent, hover), 3.0);
      // 悬停必须朝「更抢眼」的方向走：浅色变深、深色变亮，且三个状态一致。
      const step = lstar(hover) - lstar(fill);
      need(`${s} 悬停方向（应${theme === 'light' ? '变深' : '变亮'}）`, theme === 'light' ? -step : step, 2.0);
    }
    need(`${s} 作为无边框前景文字`, ratio(fill, surface), warnFloor.text);
    need(`${s} 色块相对表面的可辨度`, ratio(fill, surface), 3.0);
    need(`${s} 文字压在其 Subtle 底上`, ratio(fill, subtle), warnFloor.onSubtle);
    need(`${s}Subtle 与表面的台阶`, Math.abs(lstar(subtle) - lstar(surface)), 2.0);
  }
  for (const a of ['Primary', 'PrimaryHover', 'PrimaryPressed']) {
    need(`Accent.${a} 上的 OnAccent 文字`, ratio(onAccent, at(theme, `Accent.${a}`)), 4.0);
    need(`Accent.${a} 与 Surface.Hover 的区分`, Math.abs(lstar(at(theme, `Accent.${a}`)) - lstar(at(theme, 'Surface.Hover'))), 10);
  }
  for (const ground of ['Surface.Sunken', 'Surface.Focused', 'Surface.Selected', 'Surface.Pressed', 'Surface.Hover', 'Accent.PrimarySubtle', 'Border.Subtle']) {
    need(`Text.Primary 于 ${ground}`, ratio(at(theme, 'Text.Primary'), at(theme, ground)), 4.5);
  }
  need('正文达到 AAA', ratio(at(theme, 'Text.Primary'), surface), 7.0);
  need('次级文字达到 AA', ratio(at(theme, 'Text.Secondary'), surface), 4.5);
  need('焦点环相对表面', ratio(at(theme, 'Border.Focus'), surface), 3.0);
  // 边框分两级：静止态要「若有若无」，可交互态（悬停 / 勾选框描边 / 开关滑块）必须够 3:1。
  need('Border.Strong 相对表面（悬停与勾选框描边）', ratio(at(theme, 'Border.Strong'), surface), 3.0);
  need('Border.Default 相对表面（静止发丝线，下限）', ratio(at(theme, 'Border.Default'), surface), 1.6);
  need('静止发丝线不得回到 3:1', ratio(at(theme, 'Border.Strong'), surface) - ratio(at(theme, 'Border.Default'), surface), theme === 'light' ? 1.0 : 1.2);
  if (ratio(at(theme, 'Border.Default'), surface) > (theme === 'light' ? 2.3 : 2.0)) fails.push(`Border.Default 相对表面 ${ratio(at(theme, 'Border.Default'), surface).toFixed(2)}，已超出「若有若无」上限`);
  const peakNeutral = Math.max(...[...Object.values(Pal.Chalk), ...Object.values(Pal.Slate)].map(chroma));
  if (peakNeutral > 7) fails.push(`中性线峰值彩度 C*=${peakNeutral.toFixed(1)} > 7，纸面又变回蓝灰`);
  need('禁用文字不越界过亮', lstar(at(theme, 'Text.Disabled')), theme === 'light' ? 60 : 30);

  const covered = new Set(Object.keys(Roles));
  if (covered.size !== 46) fails.push(`角色表条目数 ${covered.size}，与规格 46 不符`);

  if (fails.length) {
    console.error(`拒绝生成 ${theme} — ${fails.length} 项未达标:\n  ` + fails.join('\n  '));
    process.exit(1);
  }
  console.log(`${theme}: ${Object.keys(Roles).length} 角色 / 状态双重职责校验通过`);
}

/* ---------------- palette block (byte-identical in both files) ---------------- */
const RAMP_ORDER = ['Chalk', 'Slate', 'Cobalt', 'Coating', 'Green', 'Yellow', 'Red'];
const RAMP_NOTE = {
  Chalk: '纸白到墨黑，仍落在 214° 那条线上，但彩度封顶 C*≤7（旧值 16）——灰就是灰，蓝交给 Cobalt。浅色主题自上而下取用。',
  Slate: '与 Chalk 同一条线的另一端读起：Slate.0 与 Chalk.1000 是同一滴墨，同样低彩度。',
  Cobalt: '主色 — 光学蓝。按钮、选中、进度、活动指示。',
  Coating: '次色 — 镜头镀膜的青。只承担「信息」与「焦点」，绝不当第二个主色。',
  Green: '成功态。深色侧用 400/300，浅色侧用 600/700，彩度按色域上限重解过去除褪色感。',
  Yellow: '警告态。色相角 97–102°——真黄。深色直接用 400/300，浅色受近白底限制只能取 600/700（见 check.js 的浅色 Warning 例外）。',
  Red: '危险态。浅色基色由 L*38 的砖红抬到 L*45 的朱红。',
};
const EFFECT_REF = {
  'Effect.Shadow': 'Palette.Effect.Shadow',
  'Overlay.Backdrop': 'Palette.Overlay.Backdrop',
  'State.DisabledVeil': 'Palette.State.DisabledVeil',
};
const DISABLED_REF = { light: 'Chalk.300', dark: 'Slate.250' };
const GROUP_LABEL = {
  Background: '背景', Surface: '表面 — 由低到高的层级', Text: '文字', Border: '边框',
  Accent: '主色 / 次色', Status: '状态 — 同时充当色块填充与前景文字，取值已按双重职责校准',
  State: '禁用态', ScrollBar: '滚动条', TransparentBackground: '透明棋盘格底色',
};

/* 按前缀分组输出，保留旧文件的分段可读性 */
function roleLines(theme, kind) {
  const lines = [];
  let group = null;
  for (const key of Object.keys(Roles)) {
    const g = key.split('.')[0];
    if (g !== group) {
      group = g;
      lines.push('');
      lines.push(`    <!--  ${GROUP_LABEL[g]}  -->`);
    }
    const ref = refOf(theme, key);
    if (kind === 'color') {
      lines.push(`    <Color x:Key="Theme.Color.${key}">${hex(at(theme, key))}</Color>  <!--  = Palette.${ref}  -->`);
    } else {
      lines.push(`    <SolidColorBrush x:Key="Theme.Brush.${key}" Color="{StaticResource Palette.${ref}}" />`);
    }
  }
  return lines;
}

/* 阴影：只换色相，不换浓度。实测 WPF 的 DropShadowEffect 不参与 Color 的 alpha
   （同 Opacity 下把色 alpha 从 #1A 换到 #FF，离屏渲染的压暗量一位都不变），
   浓度只由 Opacity 决定，故 Opacity 必须原样保留，不能按 alpha 等比回调。
   载体色仍写成 ARGB：它同时供 Theme.Brush.Effect.Shadow 当画刷用，画刷是吃 alpha 的。 */
const SHADOWS = {
  light: {
    color: '#26070B10',
    items: [['Theme.PopupShadow', 18, 6, 0.18], ['Theme.ButtonShadow', 16, 5, 0.18], ['Theme.SideMenuItemShadow', 12, 5, 0.05]],
  },
  dark: {
    color: '#B3070B10',
    items: [['Theme.PopupShadow', 22, 8, 0.4], ['Theme.ButtonShadow', 18, 6, 0.4], ['Theme.SideMenuItemShadow', 14, 5, 0.2]],
  },
};

/* ---------------- assemble one theme file ---------------- */
function emit(theme) {
  const out = [];
  const push = (...s) => out.push(...s);
  const section = (title) => push(`
    <!--
        ============================================================
        ${title}
        ============================================================
    -->`);

  push(HEAD[theme]);

  section(`原语层 Palette.{Family}.{Step} — 唯一真值来源`);
  for (const fam of RAMP_ORDER) {
    push('');
    push(`    <!--  ${fam} — ${RAMP_NOTE[fam]}  -->`);
    for (const step of Object.keys(Pal[fam]).map(Number).sort((a, b) => a - b)) {
      push(`    <Color x:Key="Palette.${fam}.${step}">${hex(Pal[fam][step])}</Color>`);
    }
  }
  push('');
  push('    <!--  投影、遮罩与禁用蒙层的载体色带 alpha，且两主题取值不同，故同属原语层  -->');
  for (const [k, ref] of Object.entries(EFFECT_REF)) {
    push(`    <Color x:Key="${ref}">${hex(Effect[k][theme])}</Color>`);
  }

  section(`角色色 Theme.Color.* — 兼容层，逐条镜像 ${theme === 'light' ? 'Chalk' : 'Slate'} 原语`);
  push(...roleLines(theme, 'color'));
  for (const [k, ref] of Object.entries(EFFECT_REF)) {
    push(`    <Color x:Key="Theme.Color.${k}">${hex(Effect[k][theme])}</Color>  <!--  = ${ref}  -->`);
  }
  push(`    <Color x:Key="Theme.Color.Status.Disable">${hex(resolve(DISABLED_REF[theme]))}</Color>  <!--  = Palette.${DISABLED_REF[theme]}  -->`);

  section('角色画刷 Theme.Brush.* — 控件模板只认这一层');
  push(...roleLines(theme, 'brush'));
  push('');
  push('    <!--  投影 / 遮罩 / 禁用蒙层 / 禁用前景四把画刷。Status.Disable 在旧库里只有浅色有且写死中性灰，现两主题都补、并取回色相线上  -->');
  for (const ref of ['Palette.Effect.Shadow', 'Palette.Overlay.Backdrop', 'Palette.State.DisabledVeil', `Palette.${DISABLED_REF[theme]}`]) {
    const key = ref === `Palette.${DISABLED_REF[theme]}` ? 'Status.Disable' : ref.replace('Palette.', '');
    push(`    <SolidColorBrush x:Key="Theme.Brush.${key}" Color="{StaticResource ${ref}}" />`);
  }

  push(TRANSPARENT_BG);
  push(LAYOUT[theme]);
  return out.join('\n');
}

/* ---------------- head / tail ---------------- */
const HEAD = {
  light: `<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!--
        浅色主题 — “Aperture · Diffuse”（漫射）

        取景器与工业相机的世界：中性色仍排在同一条 214° 线上，但彩度封顶 C*≤7，
        灰就是灰——蓝不再染在纸面上，只由 Cobalt 这一个角色承担。

        · 不刺眼：卡片底封在 #FBFCFD（Y≈0.97），不再拿纯白当大面积纸面；
          正文对比停在 14.4:1 —— 越过 AAA 的 7:1，却退出了 18:1 的「焊工面罩」区间。
        · 层级靠曝光差而不是靠黑线：画布 #F1F3F5 让位于卡片 #FBFCFD，
          悬浮、按下、选中各自只推进一级 L*。静止边框是 1.97:1 的发丝线（若有若无），
          只有悬停、勾选框描边这类「需要确认交互」的状态才提到 3:1 以上。
        · 语义分工：Cobalt 只做主操作；Coating（镀膜青）只做信息与焦点环。
          旧方案 Info #2563EB 与 Accent #1E5EE6 几乎同色，如今两者不再混淆。
        · 状态色去掉褪色感：警告由琥珀 #A85C07 抬成真黄 #A08700（Lab 色相角 64°→92°），
          砖红 #A82828（L*38）→ 朱红 #BE3E2B（L*45），橄榄绿 #157A46 → #057E42。
          唯一的让步在警告：真黄要压到 4.5:1 就得回到 L*≤52，那已经是棕色，故浅色警告
          停在 3.4:1（AA 大字号），check.js 与 emit.js 对这一档单独放行，其余状态照守 4.5:1。

        命名约定：
        Palette.{Family}.{Step}        — 原语，唯一真值来源
        Theme.Color.{Role}.{Variant}   — 角色色（兼容层，见下）
        Theme.Brush.{Role}.{Variant}   — 角色画刷，控件模板只认这一层
        Theme.{LayoutToken}            — CornerRadius / Thickness / DropShadowEffect

        兼容层为何逐条写死十六进制：WPF 的 Color 资源不能引用另一个 Color，
        只有 SolidColorBrush 能通过 Color="{StaticResource …}" 取 Color。
        故 Theme.Color.* 只能是 Palette 的字面镜像（每行已注明出处），
        由生成脚本保证两者同步；改色请改 Palette 后重新生成两个主题文件。
    -->`,
  dark: `<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!--
        深色主题 — “Aperture · Umbra”（本影）

        与浅色共用同一条低彩度灰阶线，只是从另一端读起：Slate.0 与 Chalk.1000
        是同一滴墨。因此深色不是另一套配色，而是同一套色卡的另一种曝光。

        · 不累眼：底面 #16191C → 卡片 #1F2226，避开纯黑（纯黑会把边框逼成灰白）；
          正文 L* 收在 89 而非 93+，消除暗底亮字的眩光边缘，卡片上仍有 12.1:1。
        · 层级靠表面台阶：本主题把高度差交给 ΔL* 递进的表面，
          所以边框可以放心安静——静止态 1.79:1 只剩一层轮廓，悬停 / 勾选框描边才给到 3.19:1；
          暗色下过强的边框会连成一片网格。
        · 中性线彩度同样封顶 C*≤7（旧值 16），深色不再整体泛蓝。
        · 语义分工：Cobalt 只做主操作；Coating（镀膜青）只做信息与焦点环。
        · 警告用真黄 #E2C600：深色底给得起亮度，压在画布上 10.35:1、墨字压在上面 11.57:1，
          不必像浅色那样在「够黄」与「可读」之间让步。

        命名约定：
        Palette.{Family}.{Step}        — 原语，唯一真值来源
        Theme.Color.{Role}.{Variant}   — 角色色（兼容层，见下）
        Theme.Brush.{Role}.{Variant}   — 角色画刷，控件模板只认这一层
        Theme.{LayoutToken}            — CornerRadius / Thickness / DropShadowEffect

        兼容层为何逐条写死十六进制：WPF 的 Color 资源不能引用另一个 Color，
        只有 SolidColorBrush 能通过 Color="{StaticResource …}" 取 Color。
        故 Theme.Color.* 只能是 Palette 的字面镜像（每行已注明出处），
        由生成脚本保证两者同步；改色请改 Palette 后重新生成两个主题文件。
    -->`,
};

const TRANSPARENT_BG = `
    <!--
        ============================================================
        透明棋盘格 TransparentBackground（几何绘制，非位图）
        一个 16x16 单元含两个 8x8 格，三把键只差 Viewport，故格子尺寸可缩放而图案不变形。
        ============================================================
    -->
    <!--  单个 16x16 单元中的深色格，可当作普通几何复用。  -->
    <GeometryGroup x:Key="TransparentBackground.Geometry">
        <RectangleGeometry Rect="0,0,8,8" />
        <RectangleGeometry Rect="8,8,8,8" />
    </GeometryGroup>

    <DrawingBrush
        x:Key="TransparentBackground.Small"
        TileMode="Tile"
        Viewbox="0,0,16,16"
        ViewboxUnits="Absolute"
        Viewport="0,0,8,8"
        ViewportUnits="Absolute">
        <DrawingBrush.Drawing>
            <DrawingGroup>
                <GeometryDrawing Brush="{StaticResource Theme.Brush.TransparentBackground.Base}" Geometry="M0,0 H16 V16 H0 Z" />
                <GeometryDrawing Brush="{StaticResource Theme.Brush.TransparentBackground.Alt}" Geometry="{StaticResource TransparentBackground.Geometry}" />
            </DrawingGroup>
        </DrawingBrush.Drawing>
    </DrawingBrush>

    <DrawingBrush
        x:Key="TransparentBackground"
        TileMode="Tile"
        Viewbox="0,0,16,16"
        ViewboxUnits="Absolute"
        Viewport="0,0,16,16"
        ViewportUnits="Absolute">
        <DrawingBrush.Drawing>
            <DrawingGroup>
                <GeometryDrawing Brush="{StaticResource Theme.Brush.TransparentBackground.Base}" Geometry="M0,0 H16 V16 H0 Z" />
                <GeometryDrawing Brush="{StaticResource Theme.Brush.TransparentBackground.Alt}" Geometry="{StaticResource TransparentBackground.Geometry}" />
            </DrawingGroup>
        </DrawingBrush.Drawing>
    </DrawingBrush>

    <DrawingBrush
        x:Key="TransparentBackground.Large"
        TileMode="Tile"
        Viewbox="0,0,16,16"
        ViewboxUnits="Absolute"
        Viewport="0,0,32,32"
        ViewportUnits="Absolute">
        <DrawingBrush.Drawing>
            <DrawingGroup>
                <GeometryDrawing Brush="{StaticResource Theme.Brush.TransparentBackground.Base}" Geometry="M0,0 H16 V16 H0 Z" />
                <GeometryDrawing Brush="{StaticResource Theme.Brush.TransparentBackground.Alt}" Geometry="{StaticResource TransparentBackground.Geometry}" />
            </DrawingGroup>
        </DrawingBrush.Drawing>
    </DrawingBrush>`;

const LAYOUT = {
  light: shadowBlock('light', `
    <!--
        ============================================================
        布局令牌 — 本次只换配色，几何与阴影参数（含 Opacity）逐条保持原样；
        实测 DropShadowEffect 不读取 Color 的 alpha，浓度只由 Opacity 决定，
        改色相不会改变浓淡，因此这里不存在需要回调的地方。
        ============================================================
    -->
    <CornerRadius x:Key="Theme.SmallCornerRadius">4</CornerRadius>
    <CornerRadius x:Key="Theme.ControlCornerRadius">6</CornerRadius>
    <CornerRadius x:Key="Theme.LargeCornerRadius">8</CornerRadius>
    <!--  Toolbox 容器取直角：圆角处既不画背景也不画内容，是个透明缺口，与左侧控件接壤时会漏出背后表面形成小角  -->
    <CornerRadius x:Key="Theme.ToolboxCornerRadius">0</CornerRadius>
    <Thickness x:Key="Theme.ControlPadding">12,6</Thickness>
    <Thickness x:Key="Theme.ContentMargin">8,4</Thickness>`),
  dark: shadowBlock('dark', `
    <!--
        ============================================================
        布局令牌 — 本次只换配色，几何与阴影参数（含 Opacity）逐条保持原样。
        DropShadowEffect 不读取 Color 的 alpha，浓淡只由 Opacity 决定，
        所以换掉深色载体（#99000000 → #B3070B10）不会改变最终浓度。
        ============================================================
    -->
    <CornerRadius x:Key="Theme.SmallCornerRadius">4</CornerRadius>
    <CornerRadius x:Key="Theme.ControlCornerRadius">6</CornerRadius>
    <CornerRadius x:Key="Theme.LargeCornerRadius">8</CornerRadius>
    <!--  Toolbox 容器取直角：圆角处既不画背景也不画内容，是个透明缺口，与左侧控件接壤时会漏出背后表面形成小角  -->
    <CornerRadius x:Key="Theme.ToolboxCornerRadius">0</CornerRadius>
    <Thickness x:Key="Theme.ControlPadding">12,6</Thickness>
    <Thickness x:Key="Theme.ContentMargin">8,4</Thickness>`),
};

function shadowBlock(theme, head) {
  const cfg = SHADOWS[theme];
  const notes = {
    'Theme.PopupShadow': '弹层专用：深色底面上可压暗的余量小，故同族参数下不透明度须高于浅色才看得出轮廓。',
    'Theme.ButtonShadow': '小控件（jv:Button）专用：与 Theme.PopupShadow 同族同强度，只把模糊与偏移按控件尺寸收紧，避免大弹层的柔光在小按钮上糊成一团。',
    'Theme.SideMenuItemShadow': 'SideMenu 选中项专用：只向下散开的柔光，靠偏移把阴影从条目四周推到下方，避免紧贴边缘形成一圈「描边感」；只作用于条目背景层。',
  };
  const body = cfg.items.map(([key, blur, depth, op]) => `
    <!--  ${notes[key]}  -->
    <DropShadowEffect
        x:Key="${key}"
        BlurRadius="${blur}"
        Direction="270"
        Opacity="${op.toFixed(2)}"
        ShadowDepth="${depth}"
        Color="{StaticResource Palette.Effect.Shadow}" />`).join('');
  return `${head}
${body}
</ResourceDictionary>
`;
}

/* ---------------- run ---------------- */
guard('light');
guard('dark');

const repoRoot = path.resolve(__dirname, '..', '..');
const outDir = path.join(repoRoot, 'Themes');
const written = [];
for (const theme of ['light', 'dark']) {
  const text = emit(theme);
  const file = path.join(outDir, `AppColors.${theme[0].toUpperCase()}${theme.slice(1)}.xaml`);
  fs.writeFileSync(file, text, 'utf8');
  written.push(file);
  console.log(`wrote Themes/AppColors.${theme[0].toUpperCase()}${theme.slice(1)}.xaml (${text.split('\n').length} 行)`);
}

/* post-write invariants, read back from the files themselves:
   1) the two Palette ramps are identical, 2) every Theme.Color mirror equals the
   Palette step it claims, 3) no StaticResource points at an undefined key.      */
const rampLines = (f) => fs.readFileSync(f, 'utf8').split('\n').filter((l) => /x:Key="Palette\.(Chalk|Slate|Cobalt|Coating|Green|Yellow|Red)\./.test(l));
const [lf, df] = written;
if (rampLines(lf).join() !== rampLines(df).join()) { console.error('两主题的 Palette 阶梯不一致'); process.exit(1); }

const drift = [];
for (const f of written) {
  const src = fs.readFileSync(f, 'utf8');
  const code = src.replace(/<!--[\s\S]*?-->/g, '');
  const name = path.basename(f);
  const defined = new Map();
  const values = new Map();
  for (const l of src.split('\n')) {
    const c = l.match(/<(?:Color|SolidColorBrush|GeometryGroup|DrawingBrush)[^>]*x:Key="([^"]+)"/);
    if (c) defined.set(c[1], true);
    const v = l.match(/<Color x:Key="([^"]+)">([^<]+)<\/Color>/);
    if (v) values.set(v[1], v[2]);
  }
  for (const l of src.split('\n')) {
    const mirror = l.match(/<Color x:Key="(Theme\.Color\.[^"]+)">([^<]+)<\/Color>\s*<!--\s*=\s*(Palette\.[\w.]+)\s*-->/);
    if (mirror) {
      const want = values.get(mirror[3]);
      if (!want) drift.push(`${name}: ${mirror[3]} 未定义`);
      else if (want !== mirror[2]) drift.push(`${name}: ${mirror[1]} 应为 ${want} 实为 ${mirror[2]}`);
    }
  }
  for (const use of code.matchAll(/="\{StaticResource ([^}]+)\}"/g)) {
    if (!defined.has(use[1])) drift.push(`${name}: 引用了未定义的 ${use[1]}`);
  }
  const brushCount = (src.match(/<SolidColorBrush/g) || []).length;
  const colorCount = (src.match(/<Color x:Key="Theme\.Color\./g) || []).length;
  const EXPECT = Object.keys(Roles).length + 4;
  if (colorCount !== EXPECT) drift.push(`${name}: Theme.Color 条目 ${colorCount}，应为 ${EXPECT}`);
  if (brushCount !== EXPECT) drift.push(`${name}: 画刷条目 ${brushCount}，应为 ${EXPECT}`);
}
if (drift.length) { console.error('镜像漂移 / 悬空引用:\n  ' + drift.join('\n  ')); process.exit(1); }
console.log('Palette 一致 · 镜像无漂移 · 无悬空引用');
