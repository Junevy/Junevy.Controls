
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
    0: '#FFFFFF', 25: '#FBFCFD', 50: '#F1F3F5', 75: '#EEF0F3', 100: '#E3E6EA', 150: '#DADEE2',
    200: '#CACFD5', 300: '#B0B7BE', 400: '#8C939C', 500: '#808891', 550: '#686F79',
    600: '#60666E', 700: '#494F57', 800: '#24282E', 900: '#171A1E', 1000: '#070B10',
  },
  Slate: {
    0: '#070B10', 25: '#111215', 50: '#16191C', 75: '#1F2226', 100: '#272B30', 125: '#2F3238',
    150: '#35393F', 175: '#454A51', 200: '#545960', 250: '#697078', 400: '#969BA4',
    500: '#AAB0B8', 700: '#C9CED4', 800: '#DDE0E5', 900: '#F1F3F5', 1000: '#FFFFFF',
  },
  Cobalt: {
    50: '#EFF6FF', 100: '#DCEBFD', 200: '#BFD8FA', 300: '#93BFF3', 400: '#639BE9', 450: '#4E90E8',
    500: '#3E82D6', 600: '#1F5FC4', 700: '#174CA4', 800: '#123C82', 900: '#0D2A5C', 950: '#16304D',
  },
  Coating: {
    50: '#E6F5F9', 100: '#D3EFF5', 200: '#A9E0EB', 300: '#74CBDC', 400: '#3FB4CB',
    500: '#1795B0', 600: '#00768D', 700: '#086475', 800: '#07505E', 950: '#0C3440',
  },
  /*  状态色：旧取值停在 L*38-46 / 土黄色相，读起来像褪色老海报。现在按 Lab 目标重解——
      彩度按色域上限拟合（二分找该 (L*,h) 射线上的最大可用 C*，不会越界后被逐通道裁切）。
      浅色用 600/700/100，深色用 400·300/300·350/950，两侧同时抬色彩活力。
      Yellow 是第二轮的结果：用户要「黄色，不是棕色」，故整族色相角从 66–72°（琥珀）
      推到 97–102°（真黄），深色因此能直接上 #E2C600；浅色受物理限制——压在近白卡片上
      要 4.5:1 就得压到 L*≤52，而那个亮度的黄已经发棕。用户选定「抬黄优先、放宽对比」，
      故浅色 600 取 L*56.9（文字 3.43:1，AA 大字号），守卫脚本对浅色 Warning 单独放行。 */
  Green: { 100: '#E7F8EB', 200: '#C0E8C9', 300: '#6DD38C', 400: '#58C278', 500: '#159A51', 600: '#057E42', 700: '#126A39', 950: '#173823' },
  Yellow: { 100: '#FFF6C4', 200: '#FBE79B', 300: '#EDD600', 400: '#E2C600', 500: '#C7AC00', 600: '#A08700', 700: '#947A00', 950: '#3E3503' },
  Red: { 100: '#FFF0EE', 200: '#FEC7C2', 300: '#E77465', 350: '#F88E81', 400: '#EA685F', 500: '#D74D44', 600: '#BE3E2B', 700: '#99352C', 950: '#48201D' },
};

/* ---------------- ROLE MAP ----------------
   Every key keeps the name the control templates already bind to, so the 32
   dictionaries re-skin from values alone.  [lightPrimitive, darkPrimitive]      */
const Roles = {
  'Background.App': ['Chalk.50', 'Slate.50'],
  'Background.Subtle': ['Chalk.75', 'Slate.100'],
  'Background.Second': ['Chalk.100', 'Slate.25'],
  'Background.Third': ['Chalk.150', 'Slate.175'],

  'Surface.Base': ['Chalk.25', 'Slate.75'],
  'Surface.Raised': ['Chalk.0', 'Slate.150'],
  'Surface.Sunken': ['Chalk.100', 'Slate.25'],
  'Surface.Overlay': ['Chalk.0', 'Slate.150'],
  'Surface.Hover': ['Chalk.75', 'Slate.100'],
  'Surface.Pressed': ['Chalk.100', 'Slate.125'],
  'Surface.Selected': ['Cobalt.100', 'Cobalt.950'],
  /*  获得焦点的输入框底色：比 Sunken 温和一档。Sunken（浅色 Chalk.100 / 深色 Slate.25）
      是给代码块、表格这类静态内陷区用的，压在输入框上时对比过强；焦点态只需要「比静止卡片
      挪半步」的提示，故浅色走 Chalk.75（ΔL* 5.0）、深色走 Slate.100（比卡片略亮，焦点=抬起）。 */
  'Surface.Focused': ['Chalk.75', 'Slate.100'],

  'Text.Primary': ['Chalk.800', 'Slate.800'],
  'Text.Secondary': ['Chalk.700', 'Slate.500'],
  'Text.Tertiary': ['Chalk.600', 'Slate.400'],
  'Text.Disabled': ['Chalk.300', 'Slate.175'],
  'Text.Inverse': ['Chalk.0', 'Slate.0'],
  'Text.OnAccent': ['Chalk.0', 'Slate.0'],

  'Border.Default': ['Chalk.300', 'Slate.175'],
  'Border.Subtle': ['Chalk.150', 'Slate.150'],
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
