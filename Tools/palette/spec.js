
/* ---------------- color math ---------------- */
const rgb = (h) => { let x = h.replace('#', ''); return [parseInt(x.slice(0, 2), 16), parseInt(x.slice(2, 4), 16), parseInt(x.slice(4, 6), 16)]; };
const lin = (c) => { c /= 255; return c <= 0.04045 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4); };
const lum = (h) => { const [r, g, b] = rgb(h); return 0.2126 * lin(r) + 0.7152 * lin(g) + 0.0722 * lin(b); };
const ratio = (a, b) => { const la = lum(a), lb = lum(b); return (Math.max(la, lb) + 0.05) / (Math.min(la, lb) + 0.05); };
const lstar = (h) => { const Y = lum(h); return Y > 0.0030825 ? 116 * Math.pow(Y, 1 / 3) - 16 : 4.518 * Y; };
/*  CIELAB 彩度 C*：衡量「这一滴有多蓝/多黄」。中性线用它封顶，
    语义色用它检查是否还有力气（复古感的量化写法就是 C* 偏低 + 色相角偏移）。 */
const chroma = (h) => {
  const [r, g, b] = rgb(h).map((v) => { v /= 255; return v <= 0.04045 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4); });
  const f = (t) => (t > 0.008856 ? Math.cbrt(t) : 7.787 * t + 16 / 116);
  const X = f((0.4124 * r + 0.3576 * g + 0.1805 * b) / 0.95047);
  const Y = f(0.2126 * r + 0.7152 * g + 0.0722 * b);
  const Z = f((0.0191 * r + 0.1192 * g + 0.9508 * b) / 1.08883);
  return Math.hypot(500 * (X - Y), 200 * (Y - Z));
};

/* ---------------- PRIMITIVES ----------------
   Two neutral ramps on one very cool-neutral line, and the neutral line is
   deliberately ALMOST grey: every step keeps its L* and its Lab hue angle but
   carries only 40% of the chroma the old ramps had (peak C* 16.2 → 6.5). The
   blue identity now lives in Cobalt/Coating alone instead of tinting the paper
   itself. Chalk reads top-down (light theme), Slate reads bottom-up (dark
   theme); Chalk.1000 and Slate.0 are the same ink, so the two ramps are one
   system. L* is preserved step-for-step, which is why every ΔL* step assertion
   and every text-contrast assertion still holds after the re-skin.            */
// 规格只有一份：Pal / Roles / Effect 来自 spec.js，校验器与生成器共用同一张表
const Pal = {
  Chalk: {
    0: '#FFFFFF', 50: '#F1F3F5', 75: '#EBEDEF', 100: '#E3E6EA', 150: '#DADEE2', 300: '#B0B7BE', 400: '#8C939C', 550: '#686F79',
    600: '#60666E', 700: '#494F57', 800: '#24282E',
  },
  Slate: {
    0: '#070B10', 25: '#111215', 50: '#16191C', 75: '#1F2226', 100: '#272B30', 125: '#2F3238',
    150: '#35393F', 175: '#454A51', 250: '#697078', 400: '#969BA4',
    500: '#AAB0B8', 800: '#DDE0E5',
  },
  Cobalt: {
    50: '#EFF6FF', 100: '#DCEBFD', 400: '#639BE9', 450: '#4E90E8',
    500: '#3E82D6', 600: '#1F5FC4', 700: '#174CA4', 800: '#123C82', 950: '#16304D',
  },
  Coating: {
    50: '#E6F5F9', 300: '#74CBDC', 600: '#00768D', 950: '#0C3440',
  },
  /*  状态色：旧取值停在 L*38-46 / 土黄色相，读起来像褪色老海报。现在按 Lab 目标重解——
      彩度按色域上限拟合（二分找该 (L*,h) 射线上的最大可用 C*，不会越界后被逐通道裁切）。
      浅色用 600/700/100，深色用 400·300/300·350/950，两侧同时抬色彩活力。
      Yellow 是第二轮的结果：用户要「黄色，不是棕色」，故整族色相角从 66–72°（琥珀）
      推到 97–102°（真黄），深色因此能直接上 #E2C600；浅色受物理限制——压在近白卡片上
      要 4.5:1 就得压到 L*≤52，而那个亮度的黄已经发棕。用户选定「抬黄优先、放宽对比」，
      故浅色 600 取 L*56.9（文字 3.43:1，AA 大字号），守卫脚本对浅色 Warning 单独放行。 */
  Green: { 100: '#E7F8EB', 300: '#6DD38C', 400: '#58C278', 600: '#057E42', 700: '#126A39', 950: '#173823' },
  Yellow: { 100: '#FFF6C4', 300: '#EDD600', 400: '#E2C600', 600: '#A08700', 700: '#947A00', 950: '#3E3503' },
  Red: { 100: '#FFF0EE', 300: '#E77465', 350: '#F88E81', 600: '#BE3E2B', 700: '#99352C', 950: '#48201D' },
};

/* ---------------- ROLE MAP ----------------
   Every key keeps the name the control templates already bind to, so the 32
   dictionaries re-skin from values alone.  [lightPrimitive, darkPrimitive]      */
const Roles = {
  'Background.App': ['Chalk.50', 'Slate.50'],
  /*  Background.Subtle:次级背景——侧栏这类「次要区域」的底色(Showcase 左侧菜单栏用),
      比主画布深一档(ΔL* 2.1),使主背景在其右侧读起来更白;此前为 Chalk.75 原值,
      与画布仅差 ΔL* 1.0、肉眼不可辨,故按侧栏用途重解。 */
  'Background.Subtle': ['Chalk.75', 'Slate.100'],
  'Background.Second': ['Chalk.100', 'Slate.25'],
  'Background.Third': ['Chalk.150', 'Slate.175'],

  /*  Surface.Base 浅色取 Chalk.0 纯白：用户反馈 1.10.0 的 #FBFCFD 灰白卡片整体发灰、
      不适合阅读——阅读面就该是纸白，画布退为衬托（ΔL* 4.3，仍在 2.5–5 守卫区间）。
      正文对比由 14.4:1 提到 15.4:1；弹层不再比卡片亮，改靠阴影与描边分层（见 check.js）。 */
  'Surface.Base': ['Chalk.0', 'Slate.75'],
  'Surface.Raised': ['Chalk.0', 'Slate.150'],
  'Surface.Sunken': ['Chalk.100', 'Slate.25'],
  'Surface.Overlay': ['Chalk.0', 'Slate.150'],
  /*  Hover 随 Chalk.75 深化迁到 Chalk.50:白卡上悬停 ΔL* 4.25(守卫 2.5-5.5),
      数值与画布相同属巧合——悬停发生在卡片表面,画布在其下层级。 */
  'Surface.Hover': ['Chalk.50', 'Slate.100'],
  'Surface.Pressed': ['Chalk.100', 'Slate.125'],
  'Surface.Selected': ['Cobalt.100', 'Cobalt.950'],
  /*  获得焦点的输入框底色：1.10.0 时卡片是 #FBFCFD，焦点底走 Chalk.75「比卡片挪半步」；
      1.12.0 卡片提为纯白后，挪半步反而比静止态更灰——用户反馈「点击编辑时内部太灰」。
      故浅色焦点底 = Chalk.0 与卡片同白，焦点态改由 accent 边框表达；
      深色仍抬亮一档（焦点 = 抬起，ΔL* 4.2）。 */
  'Surface.Focused': ['Chalk.0', 'Slate.100'],

  'Text.Primary': ['Chalk.800', 'Slate.800'],
  'Text.Secondary': ['Chalk.700', 'Slate.500'],
  'Text.Tertiary': ['Chalk.600', 'Slate.400'],
  'Text.Disabled': ['Chalk.300', 'Slate.175'],
  'Text.Inverse': ['Chalk.0', 'Slate.0'],
  'Text.OnAccent': ['Chalk.0', 'Slate.0'],

  'Border.Default': ['Chalk.300', 'Slate.175'],
  'Border.Subtle': ['Chalk.150', 'Slate.150'],
  /*  区域分隔线（Shell 级）：侧栏/内容、页签区这类「分区而非控件」的界线。
      比控件边框全部再退一档，且按画布（Background.App）计量而非卡片——
      1.10.0 把 Showcase 侧栏线交给 Border.Subtle 时，侧栏还画了 Surface.Base 底，
      色阶差 + 线双重边缘被用户反馈「太明显」；现在侧栏与内容共用画布底，
      只剩这条线：浅色 1.22:1 / 深色 1.24:1（对画布）。 */
  'Border.Divider': ['Chalk.150', 'Slate.125'],
  'Border.Strong': ['Chalk.550', 'Slate.250'],
  'Border.Focus': ['Coating.600', 'Coating.300'],

  'Accent.Primary': ['Cobalt.600', 'Cobalt.450'],
  'Accent.PrimaryHover': ['Cobalt.700', 'Cobalt.400'],
  'Accent.PrimaryPressed': ['Cobalt.800', 'Cobalt.500'],
  'Accent.PrimarySubtle': ['Cobalt.50', 'Cobalt.950'],
  'Accent.Secondary': ['Coating.600', 'Coating.300'],
  'Accent.SecondarySubtle': ['Coating.50', 'Coating.950'],

  'Status.Info': ['Coating.600', 'Coating.300'],
  'Status.InfoSubtle': ['Coating.50', 'Coating.950'],
  'Status.Success': ['Green.600', 'Green.400'],
  'Status.SuccessHover': ['Green.700', 'Green.300'],
  'Status.SuccessSubtle': ['Green.100', 'Green.950'],
  'Status.Warning': ['Yellow.600', 'Yellow.400'],
  'Status.WarningHover': ['Yellow.700', 'Yellow.300'],
  'Status.WarningSubtle': ['Yellow.100', 'Yellow.950'],
  'Status.Danger': ['Red.600', 'Red.300'],
  'Status.DangerHover': ['Red.700', 'Red.350'],
  'Status.DangerSubtle': ['Red.100', 'Red.950'],

  'State.DisabledSurface': ['Chalk.100', 'Slate.75'],
  'State.DisabledBorder': ['Chalk.150', 'Slate.125'],
  'State.DisabledForeground': ['Chalk.300', 'Slate.175'],

  'ScrollBar.Thumb': ['Chalk.300', 'Slate.175'],
  'ScrollBar.ThumbHover': ['Chalk.400', 'Slate.250'],

  'TransparentBackground.Base': ['Chalk.0', 'Slate.75'],
  'TransparentBackground.Alt': ['Chalk.150', 'Slate.25'],
};
// alpha-carrying effect colors, per theme, expressed directly
const Effect = {
  'Effect.Shadow': { light: '#26070B10', dark: '#B3070B10' },       // 15% / 70% of the same ink (#070B10 = Chalk.1000 = Slate.0)
  'Overlay.Backdrop': { light: '#99070B10', dark: '#B3070B10' },
  /*  禁用蒙层：盖在日历/日期选择弹层上的半透明 veil。两主题必须反着走——
      浅色用 63% 纸白把它「洗淡」，深色用 63% 最暗面把它「压暗」。
      旧实现只在浅色写死 #A0FFFFFF，深色下会把日历冲成奶白块（实测同区域平均 L* 26.1 → 51.7）。 */
  'State.DisabledVeil': { light: '#A0FFFFFF', dark: '#A0111215' },  // = Chalk.0 / Slate.25，各加 A0 alpha
  /*  状态层（state layer）：悬停/按压的通用反馈。不替换背景、也不降整体透明度，
      而是在任意底色上叠一层固定透明度的「纱」——浅色是墨纱（越悬停越深），
      深色是白纱（越悬停越亮），与状态色的悬停方向约定一致。这样鲜艳底色
      （红按钮、绿卡片、用户自定义色）hover 时只是「同色相加深/提亮一档」，
      不会跳到灰色系产生割裂；文字与图标完全不参与变淡，反馈更清楚。
      hover 8% / pressed 15%，对卡片的 ΔL* 见 check.js 的状态层断言。 */
  'State.HoverScrim': { light: '#14070B10', dark: '#14FFFFFF' },    // 8% 墨 / 8% 白
  'State.PressedScrim': { light: '#26070B10', dark: '#26FFFFFF' },  // 15% 墨 / 15% 白
};

/* 需要新色值时用这三个函数按目标 L* 反解，不要手挑 hex：
   palOf('Cobalt', 600)                    取原语
   hsl(214, 60, 50)                        HSL 转 hex
   solveToLstar(45.6, { h, s })            固定色相与饱和，解出最接近目标 L* 的 hex
   solveInRamp(Pal.Yellow, 56.9)           在已有阶梯里找最接近目标 L* 的一档  */
const hsl = (hue, sat, lig) => {
  const a01 = sat / 100;
  const l01 = lig / 100;
  const k = (n) => (n + hue / 30) % 12;
  const a = a01 * Math.min(l01, 1 - l01);
  const f = (n) => l01 - a * Math.max(-1, Math.min(k(n) - 3, Math.min(9 - k(n), 1)));
  const to = (x) => Math.round(255 * x).toString(16).padStart(2, '0').toUpperCase();
  return '#' + to(f(0)) + to(f(8)) + to(f(4));
};

const palOf = (family, step) => {
  const fam = Pal[family];
  if (!fam) throw new Error('未知色族 ' + family);
  const value = fam[step];
  if (!value) throw new Error(family + ' 没有 ' + step + ' 档');
  return value;
};

const solveToLstar = (target, { h, s, from = 0, to = 100, tol = 0.05 } = {}) => {
  let lo = from;
  let hi = to;
  let best = hsl(h, s, from);
  for (let i = 0; i < 48; i++) {
    const mid = (lo + hi) / 2;
    const candidate = hsl(h, s, mid);
    if (Math.abs(lstar(candidate) - target) < tol) best = candidate;
    if (lstar(candidate) < target) lo = mid;
    else hi = mid;
  }
  return best;
};

const solveInRamp = (ramp, target) => Object.entries(ramp)
  .sort((x, y) => Math.abs(lstar(x[1]) - target) - Math.abs(lstar(y[1]) - target))[0];

const resolve = (ref) => { const [f, k] = ref.split('.'); return Pal[f][k]; };
const Theme = {
  light: Object.fromEntries(Object.entries(Roles).map(([r, [l]]) => [r, resolve(l)])),
  dark: Object.fromEntries(Object.entries(Roles).map(([r, [, d]]) => [r, resolve(d)])),
};

module.exports = { Pal, Roles, Effect, Theme, resolve, ratio, lstar, lum, rgb, hsl, chroma, palOf, solveToLstar, solveInRamp };
