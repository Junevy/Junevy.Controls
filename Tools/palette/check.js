/* ---------------- color math ---------------- */
const rgb = (h) => { let x = h.replace('#', ''); return [parseInt(x.slice(0, 2), 16), parseInt(x.slice(2, 4), 16), parseInt(x.slice(4, 6), 16)]; };
const lin = (c) => { c /= 255; return c <= 0.04045 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4); };
const lum = (h) => { const [r, g, b] = rgb(h); return 0.2126 * lin(r) + 0.7152 * lin(g) + 0.0722 * lin(b); };
const ratio = (a, b) => { const la = lum(a), lb = lum(b); return (Math.max(la, lb) + 0.05) / (Math.min(la, lb) + 0.05); };
const lstar = (h) => { const Y = lum(h); return Y > 0.0030825 ? 116 * Math.pow(Y, 1 / 3) - 16 : 4.518 * Y; };

/* ---------------- PRIMITIVES ----------------
   Two neutral ramps sharing one very cool-neutral hue line at low chroma
   (peak C* ≈ 6, capped at 7 by an assertion below): the paper is almost grey,
   so the blue identity is carried by Cobalt/Coating alone.
   Chalk reads top-down (light theme), Slate reads bottom-up (dark theme);
   Chalk.1000 and Slate.0 are the same ink, so the two ramps are one system.          */
// 规格只有一份：Pal / Roles / Effect 来自 spec.js，校验器与生成器共用同一张表
const { Pal, Roles, Effect, chroma } = require('./spec.js');


const resolve = (ref) => { const [f, k] = ref.split('.'); return Pal[f][k]; };
const Theme = {
  light: Object.fromEntries(Object.entries(Roles).map(([r, [l]]) => [r, resolve(l)])),
  dark: Object.fromEntries(Object.entries(Roles).map(([r, [, d]]) => [r, resolve(d)])),
};

/* ---------------- VALIDATION ---------------- */
const problems = [];
const checks = [];
const A = (mode, label, fg, bg, min) => { const r = ratio(fg, bg); const ok = r >= min; checks.push([mode, label, r, min, ok]); if (!ok) problems.push(`${mode} · ${label}: ${r.toFixed(2)} < ${min}  (${fg} on ${bg})`); };
const Alow = (mode, label, fg, bg, max) => { const r = ratio(fg, bg); const ok = r <= max; checks.push([mode, label, r, max, ok]); if (!ok) problems.push(`${mode} · ${label}: ${r.toFixed(2)} > ${max}, should stay quiet  (${fg} on ${bg})`); };
const Dl = (mode, label, a, b, lo, hi) => { const d = Math.abs(lstar(a) - lstar(b)); const ok = d >= lo && d <= hi; checks.push([mode, label, d, `${lo}-${hi}`, ok]); if (!ok) problems.push(`${mode} · ${label}: dL*=${d.toFixed(1)} outside [${lo},${hi}]`); };

for (const mode of ['light', 'dark']) {
  const t = Theme[mode], d = mode === 'dark';
  const min = d ? 1 : -1;
  A(mode, 'text.primary on app background', t['Text.Primary'], t['Background.App'], 10);
  A(mode, 'text.primary on surface', t['Text.Primary'], t['Surface.Base'], 10);
  A(mode, 'text.primary on overlay/popup', t['Text.Primary'], t['Surface.Overlay'], 8);
  A(mode, 'text.secondary on surface', t['Text.Secondary'], t['Surface.Base'], 6);
  A(mode, 'text.tertiary on surface', t['Text.Tertiary'], t['Surface.Base'], 4.5);
  A(mode, 'text.tertiary on hover', t['Text.Tertiary'], t['Surface.Hover'], 4.5);
  A(mode, 'text.tertiary on pressed', t['Text.Tertiary'], t['Surface.Pressed'], 4.5);
  A(mode, 'text.tertiary on selected', t['Text.Tertiary'], t['Surface.Selected'], 4.5);
  A(mode, 'text.primary on selected', t['Text.Primary'], t['Surface.Selected'], 7);
  A(mode, 'text.primary on accent-subtle', t['Text.Primary'], t['Accent.PrimarySubtle'], 7);
  A(mode, 'accent as link text on surface', t['Accent.Primary'], t['Surface.Base'], 4.5);
  A(mode, 'label on accent button', t['Text.OnAccent'], t['Accent.Primary'], 4.5);
  A(mode, 'label on accent hover', t['Text.OnAccent'], t['Accent.PrimaryHover'], 4.5);
  A(mode, 'label on accent pressed', t['Text.OnAccent'], t['Accent.PrimaryPressed'], 4.5);
  A(mode, 'interactive border / mark outline vs surface', t['Border.Strong'], t['Surface.Base'], 3.0);
  A(mode, 'idle hairline still reads as an edge', t['Border.Default'], t['Surface.Base'], 1.6);
  Alow(mode, 'idle hairline stays barely there', t['Border.Default'], t['Surface.Base'], d ? 2.0 : 2.3);
  Dl(mode, 'hover border steps up from idle', t['Border.Strong'], t['Border.Default'], 10, 40);
  A(mode, 'focus ring vs surface', t['Border.Focus'], t['Surface.Base'], 3.0);
  A(mode, 'success on surface', t['Status.Success'], t['Surface.Base'], 4.5);
  // 浅色 Warning 单独放行：真黄（色相角 97°+）压在近白卡片上做不到 4.5:1，压到能做到的
  // 亮度就又回棕色了。用户选定「抬黄优先」，故浅色这档守 AA 大字号 3.3，深色仍守 4.5。
  A(mode, 'warning on surface', t['Status.Warning'], t['Surface.Base'], d ? 4.5 : 3.3);
  A(mode, 'danger on surface', t['Status.Danger'], t['Surface.Base'], 4.5);
  A(mode, 'info on surface', t['Status.Info'], t['Surface.Base'], 4.5);
  // *Subtle 是底色板，*Hover 只做 Background（AppBar / DialogWindow / Label 的实测用法），
  // 从不当前景，因此板上文字按基色校验，悬停只看它与基色是否分得开。
  A(mode, 'success on its subtle', t['Status.Success'], t['Status.SuccessSubtle'], 4.5);
  A(mode, 'warning on its subtle', t['Status.Warning'], t['Status.WarningSubtle'], d ? 4.5 : 3.2);
  A(mode, 'danger on its subtle', t['Status.Danger'], t['Status.DangerSubtle'], 4.5);
  A(mode, 'info on its subtle', t['Status.Info'], t['Status.InfoSubtle'], 4.5);
  Dl(mode, 'success hover step', t['Status.SuccessHover'], t['Status.Success'], 2, 12);
  Dl(mode, 'warning hover step', t['Status.WarningHover'], t['Status.Warning'], 2, 12);
  Dl(mode, 'danger hover step', t['Status.DangerHover'], t['Status.Danger'], 2, 12);
  A(mode, 'scrollbar thumb vs surface', t['ScrollBar.Thumb'], t['Surface.Base'], d ? 1.6 : 1.9);
  A(mode, 'checkerboard cells differ', t['TransparentBackground.Alt'], t['TransparentBackground.Base'], 1.14);
  Alow(mode, 'disabled text reads inert', t['Text.Disabled'], t['Surface.Base'], 3.0);
  Alow(mode, 'divider stays decorative', t['Border.Subtle'], t['Surface.Base'], 1.7);
  Dl(mode, 'surface vs app background', t['Surface.Base'], t['Background.App'], 2.5, 5);
  Dl(mode, 'hover step', t['Surface.Hover'], t['Surface.Base'], 2.5, 5.5);
  Dl(mode, 'pressed step', t['Surface.Pressed'], t['Surface.Hover'], 2, 5.5);
  Dl(mode, 'selected step', t['Surface.Selected'], t['Surface.Base'], 4, 9.5);
  Dl(mode, 'overlay above surface', t['Surface.Overlay'], t['Surface.Base'], d ? 5 : 0.5, 12);
  Dl(mode, 'sunken below surface', t['Surface.Sunken'], t['Surface.Base'], 2.5, 9);
  Dl(mode, 'focused input steps gently from base', t['Surface.Focused'], t['Surface.Base'], 3, 6);
  Dl(mode, 'accent vs hover', t['Accent.Primary'], t['Accent.PrimaryHover'], 3.5, 14);
  Dl(mode, 'text hierarchy primary->secondary', t['Text.Primary'], t['Text.Secondary'], 13, 24);
  Dl(mode, 'text hierarchy secondary->tertiary', t['Text.Secondary'], t['Text.Tertiary'], 6, 13);
  if (d) Dl(mode, 'app background vs sunken well', t['Background.App'], t['Surface.Sunken'], 2.5, 5);
}

/* ---------------- REPORT ---------------- */
console.log('\n--- hue discipline: every neutral on one line ---');
for (const [n, ramp] of [['Chalk', Pal.Chalk], ['Slate', Pal.Slate]]) {
  let prev = -1, mono = true;
  for (const [k, v] of Object.entries(ramp)) { if (n === 'Chalk' ? lstar(v) > prev + 0.001 : lstar(v) < prev - 0.001) { if (prev >= 0) mono = false; } prev = lstar(v); }
  const dir = n === 'Chalk' ? 'descending' : 'ascending';
  console.log(`  ${n.padEnd(6)} ${dir} L*: ${mono ? 'ok' : 'CHECK'}   #0 = ${ramp[Object.keys(ramp)[0]]}`);
  if (!mono) problems.push(`${n} 阶梯 L* 不单调`);
  /*  中性线彩度上限 7：纸面必须「几乎是灰」，蓝只交给 Cobalt 承担。
      旧值是 C*≈16 的蓝灰，整体读起来偏蓝，已按用户意见压下来。 */
  const peak = Math.max(...Object.values(ramp).map(chroma));
  checks.push([n.toLowerCase(), 'neutral ramp chroma cap', peak, 7, peak <= 7]);
  if (peak > 7) problems.push(`${n} 峰值彩度 C*=${peak.toFixed(1)} > 7，中性线又变回蓝灰`);
  console.log(`  ${n.padEnd(6)} peak chroma C* = ${peak.toFixed(1)}  (cap 7)`);
}

const pass = checks.filter(c => c[4]).length;
console.log(`\n${pass}/${checks.length} checks passed\n`);
if (problems.length) { console.log('PROBLEMS:'); problems.forEach(p => console.log('  ✗ ' + p)); }

console.log('\n--- the argument in numbers ---');
console.log(`  light card ${Theme.light['Surface.Base']} (Y=${lum(Theme.light['Surface.Base']).toFixed(3)}), canvas ${Theme.light['Background.App']} (Y=${lum(Theme.light['Background.App']).toFixed(3)})`);
console.log(`             body text ${ratio(Theme.light['Text.Primary'], Theme.light['Surface.Base']).toFixed(2)}:1  — was 18.34:1 on pure white`);
console.log(`  dark  canvas ${Theme.dark['Background.App']}, card ${Theme.dark['Surface.Base']}, body text ${Theme.dark['Text.Primary']} → ${ratio(Theme.dark['Text.Primary'], Theme.dark['Background.App']).toFixed(2)}:1`);
console.log(`             was #ECECEE (L*${lstar('#ECECEE').toFixed(1)}) on neutral #202023; now L*${lstar(Theme.dark['Text.Primary']).toFixed(1)} on the low-chroma 214° line ${Theme.dark['Background.App']}`);
console.log(`  old Info #2563EB vs old Accent #1E5EE6 → indistinguishable.`);
console.log(`  new Info/Focus = Coating ${Theme.light['Status.Info']} / ${Theme.dark['Status.Info']} — a separate hue that only ever means "attention", never "primary action".`);

if (problems.length) {
  console.error(`\n${problems.length} 项失败`);
  process.exit(1);
}
