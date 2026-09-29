
/* ---------------- color math ---------------- */
const rgb = (h) => { let x = h.replace('#', ''); return [parseInt(x.slice(0, 2), 16), parseInt(x.slice(2, 4), 16), parseInt(x.slice(4, 6), 16)]; };
const lin = (c) => { c /= 255; return c <= 0.04045 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4); };
const lum = (h) => { const [r, g, b] = rgb(h); return 0.2126 * lin(r) + 0.7152 * lin(g) + 0.0722 * lin(b); };
const ratio = (a, b) => { const la = lum(a), lb = lum(b); return (Math.max(la, lb) + 0.05) / (Math.min(la, lb) + 0.05); };
const lstar = (h) => { const Y = lum(h); return Y > 0.0030825 ? 116 * Math.pow(Y, 1 / 3) - 16 : 4.518 * Y; };

/* ---------------- PRIMITIVES ----------------
   Two neutral ramps, one hue line (211-215 deg), saturation bell-curved so the
   paper end and the ink end stay clean while the middle carries the blue identity.
   Chalk reads top-down (light theme), Slate reads bottom-up (dark theme);
   Chalk.1000 and Slate.0 are the same ink, so the two ramps are one system.          */
const Pal = {
  Chalk: {
    0: '#FFFFFF', 25: '#FAFCFE', 50: '#EEF3F9', 75: '#EAF1F7', 100: '#DFE7EF', 150: '#D5DFE9',
    200: '#C3D0DE', 300: '#A7B8CB', 400: '#8294AB', 500: '#75899F', 550: '#5C7089',
    600: '#56677B', 700: '#3E5065', 800: '#1C2937', 900: '#111A24', 1000: '#070B10',
  },
  Slate: {
    0: '#070B10', 25: '#0D1218', 50: '#121922', 75: '#1A222D', 100: '#212B38', 125: '#283340',
    150: '#2E3A48', 175: '#3D4B5C', 200: '#4B5A6C', 250: '#5F7185', 400: '#8D9CB1',
    500: '#A2B1C4', 700: '#C2CFDE', 800: '#D8E1EC', 900: '#EEF3F9', 1000: '#FFFFFF',
  },
  Cobalt: {
    50: '#EFF6FF', 100: '#DCEBFD', 200: '#BFD8FA', 300: '#93BFF3', 400: '#639BE9', 450: '#4E90E8',
    500: '#3E82D6', 600: '#1F5FC4', 700: '#174CA4', 800: '#123C82', 900: '#0D2A5C', 950: '#16304D',
  },
  Coating: {
    50: '#ECF8FB', 100: '#D3EFF5', 200: '#A9E0EB', 300: '#74CBDC', 400: '#3FB4CB',
    500: '#1795B0', 600: '#0B7C91', 700: '#086475', 800: '#07505E', 950: '#0C3440',
  },
  Green: { 100: '#D8F3E3', 200: '#A8E2C2', 300: '#6FCF9B', 400: '#45C48A', 500: '#1F9D5B', 600: '#157A46', 700: '#0F6036', 950: '#0E3526' },
  Amber: { 100: '#FCF0D6', 200: '#F7DDA6', 300: '#EDC066', 400: '#DFA432', 500: '#C08412', 600: '#916308', 700: '#7A4A04', 950: '#3E2A08' },
  Red: { 100: '#FCE2E2', 200: '#F6B9B9', 300: '#EE7B7B', 350: '#F29C9C', 400: '#E4646A', 500: '#C33A3A', 600: '#A82828', 700: '#8A1F1F', 950: '#4E1C1C' },
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

  'Text.Primary': ['Chalk.800', 'Slate.800'],
  'Text.Secondary': ['Chalk.700', 'Slate.500'],
  'Text.Tertiary': ['Chalk.600', 'Slate.400'],
  'Text.Disabled': ['Chalk.300', 'Slate.175'],
  'Text.Inverse': ['Chalk.0', 'Slate.0'],
  'Text.OnAccent': ['Chalk.0', 'Slate.0'],

  'Border.Default': ['Chalk.400', 'Slate.200'],
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
  'Status.Warning': ['Amber.600', 'Amber.400'],
  'Status.WarningHover': ['Amber.700', 'Amber.300'],
  'Status.WarningSubtle': ['Amber.100', 'Amber.950'],
  'Status.Danger': ['Red.600', 'Red.300'],
  'Status.DangerHover': ['Red.700', 'Red.350'],
  'Status.DangerSubtle': ['Red.100', 'Red.950'],

  'State.DisabledSurface': ['Chalk.100', 'Slate.75'],
  'State.DisabledBorder': ['Chalk.150', 'Slate.175'],
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
  'State.DisabledVeil': { light: '#A0FFFFFF', dark: '#A00D1218' },  // = Chalk.0 / Slate.25，各加 A0 alpha
};

/* 需要新色值时用这三个函数按目标 L* 反解，不要手挑 hex：
   palOf('Cobalt', 600)                    取原语
   hsl(214, 60, 50)                        HSL 转 hex
   solveToLstar(45.6, { h, s })            固定色相与饱和，解出最接近目标 L* 的 hex
   solveInRamp(Pal.Amber, 45.6)            在已有阶梯里找最接近目标 L* 的一档  */
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

module.exports = { Pal, Roles, Effect, Theme, resolve, ratio, lstar, lum, rgb, hsl, palOf, solveToLstar, solveInRamp };
