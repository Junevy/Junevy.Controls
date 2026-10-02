"""iconfont 重绘管线:SVG 路径 -> TTF 字形。

设计约定:
- 设计网格 24x24 (y 向下,SVG 习惯),构建时映射到 1024 em (ascent 896 / descent -128)。
- 线性版: 骨架路径 + 统一描边 (默认 2/24,圆头圆角),skia 转填充轮廓。
- 面性版: 同骨架加粗 + 主体填充,统一视觉重量。
- 码点/字形名与旧版 iconfont.ttf 完全一致。
"""
import math

import skia
from svgpathtools import parse_path, Line, CubicBezier, QuadraticBezier, Arc
from fontTools.pens.ttGlyphPen import TTGlyphPen

EM = 1024
GRID = 24
SCALE = EM / GRID
ASCENT = 896
STROKE = 2.0          # 线性版描边宽 (24 网格)
STROKE_BOLD = 3.2     # 面性版骨架加粗宽


def path_from_svg(d):
    """SVG d 字符串 -> skia.Path。

    svgpathtools 会丢掉子路径/闭合信息,这里按连续性切分子路径:
    端点与起点重合的子路径闭合,其余保持开放 (开放性影响描边结果)。
    圆弧统一拆成三次贝塞尔。
    """
    p = skia.Path()
    sp = parse_path(d)
    sub_start = None
    prev_end = None

    def emit(seg):
        nonlocal prev_end
        if isinstance(seg, Line):
            p.lineTo(seg.end.real, seg.end.imag)
        elif isinstance(seg, CubicBezier):
            p.cubicTo(seg.control1.real, seg.control1.imag,
                      seg.control2.real, seg.control2.imag,
                      seg.end.real, seg.end.imag)
        elif isinstance(seg, QuadraticBezier):
            p.quadTo(seg.control.real, seg.control.imag,
                     seg.end.real, seg.end.imag)
        elif isinstance(seg, Arc):
            for c in seg.as_cubic_curves():
                p.cubicTo(c.control1.real, c.control1.imag,
                          c.control2.real, c.control2.imag,
                          c.end.real, c.end.imag)
        prev_end = seg.end

    for seg in sp:
        continuous = (prev_end is not None
                      and abs(seg.start.real - prev_end.real) < 1e-6
                      and abs(seg.start.imag - prev_end.imag) < 1e-6)
        if not continuous:
            if sub_start is not None and (
                    abs(prev_end.real - sub_start.real) < 1e-6
                    and abs(prev_end.imag - sub_start.imag) < 1e-6):
                p.close()
            sub_start = seg.start
            p.moveTo(seg.start.real, seg.start.imag)
        emit(seg)
    if sub_start is not None and (
            abs(prev_end.real - sub_start.real) < 1e-6
            and abs(prev_end.imag - sub_start.imag) < 1e-6):
        p.close()
    return p


def _matrix():
    # y 翻转: 设计 y=0(顶) -> 896, y=24(底) -> -128
    m = skia.Matrix()
    m.setScaleTranslate(SCALE, -SCALE, 0, ASCENT)
    return m


def _rewind(path):
    """翻转后的路径用 skia 简化,保证 TrueType 需要的绕向由 transform 自动处理。"""
    m = _matrix()
    p = skia.Path(path)
    p.transform(m)
    return p


def iter_path(path):
    """把 skia.Path 拆成 (verb, points, conic_weight) 序列,conic 细分为二次曲线。"""
    it = skia.Path.Iter(path, False)
    while True:
        verb, pts = it.next()
        if verb == skia.Path.kDone_Verb:
            break
        w = it.conicWeight() if verb == skia.Path.kConic_Verb else None
        yield verb, pts, w


def _contours_of(path):
    """skia.Path -> 每个闭合轮廓的段列表。

    段: ('L', (x1,y1), (x2,y2)) 或 ('Q', (x1,y1), (cx,cy), (x2,y2))。
    conic 按 pow2=2 细分为二次; 三次按中点拆成两段二次。
    """
    contours = []
    cur = []
    start = None
    cur_pt = None

    def emit(seg):
        cur.append(seg)

    def finish():
        nonlocal cur
        if len(cur) >= 2:
            contours.append(cur)
        cur = []

    for verb, pts, w in iter_path(path):
        p = [(q.x(), q.y()) for q in pts]
        if verb == skia.Path.kMove_Verb:
            finish()
            start = cur_pt = p[0]
        elif verb == skia.Path.kLine_Verb:
            emit(("L", cur_pt, p[1]))
            cur_pt = p[1]
        elif verb == skia.Path.kQuad_Verb:
            emit(("Q", cur_pt, p[1], p[2]))
            cur_pt = p[2]
        elif verb == skia.Path.kConic_Verb:
            quads = skia.Path.ConvertConicToQuads(pts[0], pts[1], pts[2], w, 2)
            qp = [(q.x(), q.y()) for q in quads]
            for i in range(1, len(qp) - 1, 2):
                emit(("Q", cur_pt, qp[i], qp[i + 1]))
                cur_pt = qp[i + 1]
        elif verb == skia.Path.kCubic_Verb:
            p0, p1, p2, p3 = p
            mid = ((p0[0] + 3 * p1[0] + 3 * p2[0] + p3[0]) / 8,
                   (p0[1] + 3 * p1[1] + 3 * p2[1] + p3[1]) / 8)
            c1 = (p0[0] + (p1[0] - p0[0]) * 0.75, p0[1] + (p1[1] - p0[1]) * 0.75)
            c2 = (p3[0] + (p2[0] - p3[0]) * 0.75, p3[1] + (p2[1] - p3[1]) * 0.75)
            emit(("Q", cur_pt, c1, mid))
            emit(("Q", mid, c2, p3))
            cur_pt = p3
        elif verb == skia.Path.kClose_Verb:
            finish()
            start = cur_pt = None
    finish()
    return contours


def _contour_polyline(contour):
    """嵌套判定用的折线近似 (段端点连线)。"""
    return [seg[1] for seg in contour] + [contour[-1][-1]]


def _point_in_poly(pt, poly):
    x, y = pt
    inside = False
    n = len(poly)
    for i in range(n):
        x1, y1 = poly[i]
        x2, y2 = poly[(i + 1) % n]
        if (y1 > y) != (y2 > y):
            xin = (x2 - x1) * (y - y1) / (y2 - y1) + x1
            if x < xin:
                inside = not inside
    return inside


def _signed_area(poly):
    s = 0.0
    for i in range(len(poly)):
        x1, y1 = poly[i]
        x2, y2 = poly[(i + 1) % len(poly)]
        s += x1 * y2 - x2 * y1
    return s / 2.0


def _reverse_contour(contour):
    """反转轮廓段顺序与每段方向,几何不变。"""
    rev = []
    for seg in reversed(contour):
        if seg[0] == "L":
            rev.append(("L", seg[2], seg[1]))
        else:
            rev.append(("Q", seg[3], seg[2], seg[1]))
    return rev


def normalize_winding(contours):
    """按嵌套深度归一化绕向: TrueType y-up 外轮廓顺时针(负面积), 奇数深度反向。"""
    polys = [_contour_polyline(c) for c in contours]
    n = len(contours)
    depth = [0] * n
    for i in range(n):
        for j in range(n):
            if i != j and _point_in_poly(polys[i][0], polys[j]):
                depth[i] += 1
    fixed = []
    for contour, poly, d in zip(contours, polys, depth):
        want_cw = (d % 2 == 0)
        is_cw = _signed_area(poly) < 0
        fixed.append(contour if want_cw == is_cw else _reverse_contour(contour))
    return fixed


def skia_path_to_glyph(path):
    """skia.Path -> TTGlyph 字形 (二次曲线保留, 绕向归一化)。"""
    pen = TTGlyphPen(None)
    for contour in normalize_winding(_contours_of(path)):
        pen.moveTo(contour[0][1])
        for seg in contour:
            if seg[0] == "L":
                pen.lineTo(seg[2])
            else:
                pen.qCurveTo(seg[2], seg[3])
        pen.closePath()
    return pen.glyph()


def stroke_to_fill(d, width):
    """描边路径 -> 填充轮廓 (圆头/圆角)。width 为 24 网格设计单位。"""
    src = path_from_svg(d)
    paint = skia.Paint(
        Style=skia.Paint.kStroke_Style,
        StrokeWidth=width,
        StrokeCap=skia.Paint.kRound_Cap,
        StrokeJoin=skia.Paint.kRound_Join,
    )
    out = skia.Path()
    ok = paint.getFillPath(src, out)
    if not ok:
        raise RuntimeError("getFillPath failed for: " + d)
    return out


def fill_path_svg(d, even_odd=True):
    """填充形状; 默认 even-odd, 使子路径孔洞与方向无关。"""
    p = path_from_svg(d)
    if even_odd:
        p.setFillType(skia.PathFillType.kEvenOdd)
    return p


def union(paths):
    result = paths[0]
    for p in paths[1:]:
        result = skia.Op(result, p, skia.PathOp.kUnion_PathOp)
    return result


SW_CUT = 2.7  # 面性版镂空 (cuts) 默认描边宽


def glyph_from_spec(spec, style):
    """spec: {'strokes': [d...], 'fills': [d...], 'cuts': [d...], 'sw', 'sw_bold'}
    style: 'linear' | 'filled'。

    linear = strokes 描边 ∪ fills 填充;
    filled = fills 填充 ∪ strokes 按 sw_bold 加粗 − cuts (cuts 中 'F:' 前缀
             表示按填充形状直接挖除, 否则按 SW_CUT 描边后挖除)。
    """
    parts = []
    if style == "linear":
        sw = spec.get("sw") or STROKE
        for d in spec.get("strokes", []):
            parts.append(stroke_to_fill(d, sw))
        for d in spec.get("fills", []):
            parts.append(fill_path_svg(d))
    else:
        bold = spec.get("sw_bold")
        for d in spec.get("fills", []):
            parts.append(fill_path_svg(d))
        if bold is None or bold > 0:
            sw = bold or STROKE_BOLD
            for d in spec.get("strokes", []):
                parts.append(stroke_to_fill(d, sw))
        for d in spec.get("cuts", []):
            if d.startswith("F:"):
                parts.append(fill_path_svg(d[2:]))
            else:
                parts.append(stroke_to_fill(d, SW_CUT))

    body = union(parts) if len(parts) > 1 else parts[0]
    if style == "filled" and spec.get("cuts"):
        sw_cut = spec.get("sw_cut") or SW_CUT
        cutter = union([stroke_to_fill(d[2:], sw_cut) if d.startswith("F:")
                        else fill_path_svg(d[2:])
                        for d in spec["cuts"] if d.startswith("F:")]
                       + [stroke_to_fill(d, sw_cut)
                          for d in spec["cuts"] if not d.startswith("F:")])
        body = skia.Op(body, cutter, skia.PathOp.kDifference_PathOp)
    body.transform(_matrix())
    return skia_path_to_glyph(body)


# ---------- 常用图元助手 (24 网格) ----------

def arrow_line(x1, y1, x2, y2, head=4.5, head_angle=32):
    """带箭头线段: 返回 (线, 箭头两翼) 两条 stroke 路径。"""
    dx, dy = x2 - x1, y2 - y1
    ang = math.atan2(dy, dx)
    a1 = ang + math.radians(180 - head_angle)
    a2 = ang - math.radians(180 - head_angle)
    p1 = (x2 + head * math.cos(a1), y2 + head * math.sin(a1))
    p2 = (x2 + head * math.cos(a2), y2 + head * math.sin(a2))
    fmt = lambda p: (round(p[0], 2), round(p[1], 2))
    (x1, y1), (x2, y2) = fmt((x1, y1)), fmt((x2, y2))
    w1, w2 = fmt(p1), fmt(p2)
    return [f"M{x1} {y1}L{x2} {y2}", f"M{w1[0]} {w1[1]}L{x2} {y2}L{w2[0]} {w2[1]}"]


def gear_path(cx=12, cy=12, r_out=9.7, r_root=7.2, teeth=8, hole=3.3):
    """参数化齿轮: 返回 [外轮廓(齿形), 中孔圆] 两条 d 字符串。

    外轮廓供线性版描边/面性版填充; 中孔在面性版中随填充镂空 (反向绕向),
    线性版描边成内圈。
    """
    pts = []
    step = 360.0 / teeth
    tooth_half = step * 0.24
    root_half = step * 0.26
    slope = 0.5
    for i in range(teeth):
        base = i * step - 90
        for ang, r in [
            (base - tooth_half, r_out), (base + tooth_half, r_out),
            (base + tooth_half + slope * (root_half - tooth_half), r_root),
            (base + step - root_half - slope * (root_half - tooth_half), r_root),
        ]:
            rad = math.radians(ang)
            pts.append((cx + r * math.cos(rad), cy + r * math.sin(rad)))
    d = "M" + "L".join(f"{x:.2f} {y:.2f}" for x, y in pts) + "Z"
    hole_d = (
        f"M{cx + hole} {cy}A{hole} {hole} 0 1 1 {cx - hole} {cy}"
        f"A{hole} {hole} 0 1 1 {cx + hole} {cy}Z"
    )
    return [d, hole_d]
