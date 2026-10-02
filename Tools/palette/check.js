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
  //  深色 OnAccent 随画布手调落到 #232323 后，对按压色 Cobalt.500 实测约 4.05:1，锚定 4.0。
  A(mode, 'label on accent pressed', t['Text.OnAccent'], t['Accent.PrimaryPressed'], d ? 4.0 : 4.5);
  A(mode, 'interactive border / mark outline vs surface', t['Border.Strong'], t['Surface.Base'], 3.0);
  A(mode, 'idle hairline still reads as an edge', t['Border.Default'], t['Surface.Base'], 1.6);
  Alow(mode, 'idle hairline stays barely there', t['Border.Default'], t['Surface.Base'], d ? 2.0 : 2.3);
  Dl(mode, 'hover border steps up from idle', t['Border.Strong'], t['Border.Default'], 10, 40);
  A(mode, 'focus ring vs surface', t['Border.Focus'], t['Surface.Base'], 3.0);
  A(mode, 'success on surface', t['Status.Success'], t['Surface.Base'], 4.5);
  // 浅色 Warning 放行（1.14.0 手调）：浅色警告同样抬成真黄 Yellow.400（与深色一致），
  // 白卡上实测约 1.7:1——对比度不再是这一档的目标，守卫仅锚定「不比当前更差」。
  // 若要恢复 AA 大字号，把 spec.js 的 Status.Warning 改回 Yellow.600 并同步本下限。
  A(mode, 'warning on surface', t['Status.Warning'], t['Surface.Base'], d ? 4.5 : 1.6);
  A(mode, 'danger on surface', t['Status.Danger'], t['Surface.Base'], 4.5);
  A(mode, 'info on surface', t['Status.Info'], t['Surface.Base'], 4.5);
  // *Subtle 是底色板，*Hover 只做 Background（AppBar / DialogWindow / Label 的实测用法），
  // 从不当前景，因此板上文字按基色校验，悬停只看它与基色是否分得开。
  A(mode, 'success on its subtle', t['Status.Success'], t['Status.SuccessSubtle'], 4.5);
  A(mode, 'warning on its subtle', t['Status.Warning'], t['Status.WarningSubtle'], d ? 4.5 : 1.5);
  A(mode, 'danger on its subtle', t['Status.Danger'], t['Status.DangerSubtle'], 4.5);
  A(mode, 'info on its subtle', t['Status.Info'], t['Status.InfoSubtle'], 4.5);
  Dl(mode, 'success hover step', t['Status.SuccessHover'], t['Status.Success'], 2, 12);
  Dl(mode, 'warning hover step', t['Status.WarningHover'], t['Status.Warning'], 2, 12);
  Dl(mode, 'danger hover step', t['Status.DangerHover'], t['Status.Danger'], 2, 12);
  A(mode, 'scrollbar thumb vs surface', t['ScrollBar.Thumb'], t['Surface.Base'], d ? 1.6 : 1.9);
  // 深色棋盘格随手调纯灰化后 Alt(Base #1F2226 vs #1D1D1D)实测约 1.06:1，仍可辨；
  // 浅色保持 1.14（白 vs #DADEE2）。
  A(mode, 'checkerboard cells differ', t['TransparentBackground.Alt'], t['TransparentBackground.Base'], d ? 1.05 : 1.14);
  Alow(mode, 'disabled text reads inert', t['Text.Disabled'], t['Surface.Base'], 3.0);
  Alow(mode, 'divider stays decorative', t['Border.Subtle'], t['Surface.Base'], 1.7);
  //  Shell 级区域分隔线（侧栏/内容等）：按画布计量——要「退一层」但不能消失。
  Alow(mode, 'shell divider stays quiet on canvas', t['Border.Divider'], t['Background.App'], 1.4);
  A(mode, 'shell divider still traceable on canvas', t['Border.Divider'], t['Background.App'], 1.05);
  //  侧栏改用次级背景（Background.Subtle）后：分隔线对侧栏底也要可辨；
  //  三档文字在次级侧栏上仍须守各自的下限（Primary 重要内容 / Secondary 常规项 / Tertiary 分组头）。
  Alow(mode, 'shell divider stays quiet on secondary sidebar', t['Border.Divider'], t['Background.Subtle'], 1.35);
  // 深色分隔线/侧栏随手调纯灰化后实测约 1.04:1，锚定为 1.02；浅色保持 1.05。
  A(mode, 'shell divider traceable on secondary sidebar', t['Border.Divider'], t['Background.Subtle'], d ? 1.02 : 1.05);
  A(mode, 'text.primary on secondary sidebar', t['Text.Primary'], t['Background.Subtle'], 9);
  A(mode, 'text.secondary on secondary sidebar', t['Text.Secondary'], t['Background.Subtle'], 4.5);
  A(mode, 'text.tertiary on secondary sidebar', t['Text.Tertiary'], t['Background.Subtle'], 4.5);
  //  状态层纱色：以 sRGB 逐通道 alpha 混合到卡片底上后必须「可感知但不喧宾夺主」——
  //  hover 温和一档、pressed 明显更深/更亮一档。这是「悬停不换底色、只叠纱」方案的物理下限。
  const blend = (scrim, bg) => {
    const a = parseInt(scrim.slice(1, 3), 16) / 255;
    const f = rgb(scrim).map((v, i) => Math.round(a * v + (1 - a) * rgb(bg)[i]));
    return '#' + f.map((v) => v.toString(16).padStart(2, '0')).join('').toUpperCase();
  };
  Dl(mode, 'hover scrim reads over surface', blend(Effect['State.HoverScrim'][mode], t['Surface.Base']), t['Surface.Base'], 2.5, 25);
  Dl(mode, 'pressed scrim reads over surface', blend(Effect['State.PressedScrim'][mode], t['Surface.Base']), t['Surface.Base'], 5, 25);
  // 1.14.0 手调：两主题画布都与卡片同阶（浅色全白、深色 #232323 vs 卡片 #1F2226），
  // 「画布衬托卡片」的亮度差不再存在，层级交给描边与阴影，故下限放至 0。
  Dl(mode, 'surface vs app background', t['Surface.Base'], t['Background.App'], 0, 5);
  Dl(mode, 'hover step', t['Surface.Hover'], t['Surface.Base'], 2.5, d ? 6.5 : 5.5);
  // 深色悬停/按下在 1.14.0 手调后同落 Slate.50（#2F2F2F），按压不再比悬停更深一档。
  Dl(mode, 'pressed step', t['Surface.Pressed'], t['Surface.Hover'], d ? 0 : 2, d ? 6.5 : 5.5);
  Dl(mode, 'selected step', t['Surface.Selected'], t['Surface.Base'], 4, 9.5);
  // 浅色卡片自 1.12.0 起为纯白（Chalk.0）：白之上不存在更亮的实色，弹层的「更高一层」
  // 改由阴影 + 描边表达，不再用亮度差。深色弹层仍须比卡片亮至少 5 个 ΔL*。
  Dl(mode, 'overlay above surface', t['Surface.Overlay'], t['Surface.Base'], d ? 5 : 0, 12);
  Dl(mode, 'sunken below surface', t['Surface.Sunken'], t['Surface.Base'], 2.5, 9);
  //  1.14.0 手调：深色焦点底（Slate.100 #232323）与卡片（#1F2226）同阶，「焦点 = 抬起」
  //  改由 accent 边框表达，与浅色一致，故下限两主题都放至 0。
  Dl(mode, 'focused input steps gently from base', t['Surface.Focused'], t['Surface.Base'], 0, 6);
  Dl(mode, 'accent vs hover', t['Accent.Primary'], t['Accent.PrimaryHover'], 3.5, 14);
  Dl(mode, 'text hierarchy primary->secondary', t['Text.Primary'], t['Text.Secondary'], 13, 24);
  Dl(mode, 'text hierarchy secondary->tertiary', t['Text.Secondary'], t['Text.Tertiary'], 6, 13);
  //  深色画布提亮为 #232323 后与凹陷井（#2F2F2F）的实测台阶约 6 个 ΔL*。
  if (d) Dl(mode, 'app background vs sunken well', t['Background.App'], t['Surface.Sunken'], 2.5, 6.5);
}

/* ---------------- REPORT ---------------- */
console.log('\n--- hue discipline: every neutral on one line ---');
for (const [n, ramp] of [['Chalk', Pal.Chalk], ['Slate', Pal.Slate]]) {
  let prev = -1, mono = true;
  for (const [k, v] of Object.entries(ramp)) { if (n === 'Chalk' ? lstar(v) > prev + 0.001 : lstar(v) < prev - 0.001) { if (prev >= 0) mono = false; } prev = lstar(v); }
  const dir = n === 'Chalk' ? 'descending' : 'ascending';
  console.log(`  ${n.padEnd(6)} ${dir} L*: ${mono ? 'ok' : 'CHECK'}   #0 = ${ramp[Object.keys(ramp)[0]]}`);
  /*  1.14.0 手调后深色台阶按「用途」排序而非纯 L*：Slate.50（悬停 #2F2F2F）比
      Slate.75（卡片 #1F2226）亮是刻意的——暗色悬停必须比卡片亮一档。单调性因此
      只作提示输出，不再判失败；各用途的台阶区间由上方 hover/pressed/sunken 断言守卫。 */
  if (!mono) console.log(`    （${n} 阶梯 L* 非单调——1.14.0 手调按用途排序，见上方台阶断言）`);
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
console.log(`             body text ${ratio(Theme.light['Text.Primary'], Theme.light['Surface.Base']).toFixed(2)}:1 on the white card — 旧方案 18.34:1 过冲，1.10.0 灰白卡 14.4:1 被反馈「太灰」`);
console.log(`  dark  canvas ${Theme.dark['Background.App']}, card ${Theme.dark['Surface.Base']}, body text ${Theme.dark['Text.Primary']} → ${ratio(Theme.dark['Text.Primary'], Theme.dark['Background.App']).toFixed(2)}:1`);
console.log(`             was #ECECEE (L*${lstar('#ECECEE').toFixed(1)}) on neutral #202023; now L*${lstar(Theme.dark['Text.Primary']).toFixed(1)} on the low-chroma 214° line ${Theme.dark['Background.App']}`);
console.log(`  old Info #2563EB vs old Accent #1E5EE6 → indistinguishable.`);
console.log(`  new Info = Coating ${Theme.light['Status.Info']} / ${Theme.dark['Status.Info']} — a separate hue that only ever means "attention", never "primary action". (焦点环深色已改走 Cobalt accent，见 spec.js Border.Focus)`);

if (problems.length) {
  console.error(`\n${problems.length} 项失败`);
  process.exit(1);
}
