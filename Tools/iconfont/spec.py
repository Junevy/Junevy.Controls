"""67 个图标的重绘规格 (24x24 设计网格, y 向下)。

每个图标: {'cp': 码点, 'lin': {...}, 'fill': {...}}
  strokes: 描边骨架 (d 字符串列表)
  fills:   填充形状 (d 字符串列表, 单闭合形状; 子路径孔洞用 even-odd)
  cuts:    面性版中从整体差集减去的描边区域 (如白色对勾)
  sw:      线性版描边宽 (默认 2.0)
  sw_bold: 面性版骨架加粗宽 (默认 3.3)

'lin' 缺省时回退到 strokes/sw; 'fill' 缺省时 = fills 填充 ∪ strokes 加粗 − cuts。
所有同语义重复图标 (保存/设置/播放/相机等) 共用同一套基元函数保证一致。
"""
import math

STROKE = 2.0
STROKE_BOLD = 3.3


# ---------- 基元 ----------

def _p(v):
    return ("%.2f" % v).rstrip("0").rstrip(".")


def L(*pts):
    return "M" + "L".join(f"{_p(x)} {_p(y)}" for x, y in pts)


def _arc_seg(cx, cy, r, a1, a2):
    """角度 a1->a2 (deg, y-down) 的一段 A 命令 (span<=180)。"""
    rad1, rad2 = math.radians(a1), math.radians(a2)
    x1, y1 = cx + r * math.cos(rad1), cy + r * math.sin(rad1)
    x2, y2 = cx + r * math.cos(rad2), cy + r * math.sin(rad2)
    sweep = 1 if a2 > a1 else 0
    large = 1 if abs(a2 - a1) > 180 else 0
    return x1, y1, f"A{_p(r)} {_p(r)} 0 {large} {sweep} {_p(x2)} {_p(y2)}"


def C(cx, cy, r):
    """整圆。"""
    return (f"M{_p(cx + r)} {_p(cy)}"
            f"A{_p(r)} {_p(r)} 0 1 1 {_p(cx - r)} {_p(cy)}"
            f"A{_p(r)} {_p(r)} 0 1 1 {_p(cx + r)} {_p(cy)}Z")


def ARC(cx, cy, r, a1, a2):
    """圆弧 (自动拆 >180°)。"""
    segs = []
    span = a2 - a1
    steps = max(1, int(abs(span) // 170) + 1)
    step = span / steps
    x1, y1, s = _arc_seg(cx, cy, r, a1, a1 + step)
    segs.append(f"M{_p(x1)} {_p(y1)}" + s)
    for i in range(1, steps):
        segs.append(_arc_seg(cx, cy, r, a1 + i * step, a1 + (i + 1) * step)[2])
    return "".join(segs)


def RR(x, y, w, h, r):
    """圆角矩形。"""
    return (f"M{_p(x + r)} {_p(y)}H{_p(x + w - r)}"
            f"A{_p(r)} {_p(r)} 0 0 1 {_p(x + w)} {_p(y + r)}V{_p(y + h - r)}"
            f"A{_p(r)} {_p(r)} 0 0 1 {_p(x + w - r)} {_p(y + h)}H{_p(x + r)}"
            f"A{_p(r)} {_p(r)} 0 0 1 {x} {_p(y + h - r)}V{_p(y + r)}"
            f"A{_p(r)} {_p(r)} 0 0 1 {_p(x + r)} {y}Z")


def ARROW(x1, y1, x2, y2, head=4.2, angle=30):
    """带箭头线段 -> [杆, 头]。"""
    ang = math.atan2(y2 - y1, x2 - x1)
    a1 = ang + math.radians(180 - angle)
    a2 = ang - math.radians(180 - angle)
    w1 = (x2 + head * math.cos(a1), y2 + head * math.sin(a1))
    w2 = (x2 + head * math.cos(a2), y2 + head * math.sin(a2))
    return [L((x1, y1), (x2, y2)), L(w1, (x2, y2), w2)]


def CHECK(pts):
    return L(*pts)


def DOT(cx, cy, r):
    return C(cx, cy, r)


def STAR(cx, cy, r_out, r_in, n=5, rot=-90):
    pts = []
    for i in range(n * 2):
        r = r_out if i % 2 == 0 else r_in
        a = math.radians(rot + i * 180.0 / n)
        pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return "M" + "L".join(f"{_p(x)} {_p(y)}" for x, y in pts) + "Z"


def GEAR(teeth=8, cx=12, cy=12, r_out=9.6, r_root=7.3, hole=3.3):
    """齿轮: [齿形轮廓, 中孔]。"""
    pts = []
    step = 360.0 / teeth
    tooth_half = step * 0.24
    root_half = step * 0.27
    slope = 0.55
    for i in range(teeth):
        base = i * step - 90
        for ang, r in [
            (base - tooth_half, r_out), (base + tooth_half, r_out),
            (base + tooth_half + slope * (root_half - tooth_half), r_root),
            (base + step - root_half - slope * (root_half - tooth_half), r_root),
        ]:
            rad = math.radians(ang)
            pts.append((cx + r * math.cos(rad), cy + r * math.sin(rad)))
    d = "M" + "L".join(f"{_p(x)} {_p(y)}" for x, y in pts) + "Z"
    return [d, C(cx, cy, hole)]


def HOUSE(roof_w=18, door=False):
    """房子: [屋顶线, 墙体(开口朝上)]; door=True 时返回含门洞的墙体填充。"""
    x1 = 12 - roof_w / 2
    x2 = 12 + roof_w / 2
    roof = L((x1, 11.6), (12, 4.2), (x2, 11.6))
    bx1, bx2 = 12 - roof_w * 0.34, 12 + roof_w * 0.34
    body = L((bx1, 9.9), (bx1, 19.8), (bx2, 19.8), (bx2, 9.9))
    if not door:
        return [roof, body]
    dx1, dx2 = 10.4, 13.6
    body_fill = (f"M{_p(bx1)} 9.9V19.8H{_p(bx2)}V9.9H{_p(dx2)}V15.4"
                 f"A1.6 1.6 0 0 0 {_p(dx2 - 3.2)} 15.4V9.9H{_p(bx1)}Z")
    return [roof, body_fill]


def FOLDER(r=1.6):
    return [RR(3.2, 5.6, 17.6, 13.6, r).replace(
        "M4.8 5.6H19.2", "M4.8 5.6H9.2L11.2 8H19.2", 1)]


def FOLDER_SHAPE():
    """文件夹实心形状 (带标签孔洞): 用于面性。"""
    return [("M4.8 5.6H19.2A1.6 1.6 0 0 1 20.8 7.2V17.6A1.6 1.6 0 0 1 19.2 19.2"
             "H4.8A1.6 1.6 0 0 1 3.2 17.6V7.2A1.6 1.6 0 0 1 4.8 5.6Z")]


def FLOPPY(corner_cut=False):
    """软盘: [外形, 挡片孔, 标签孔]。"""
    if corner_cut:
        body = ("M5 4.6H16.4L19.4 7.6V17.6A2 2 0 0 1 17.4 19.6H5"
                "A2 2 0 0 1 3 17.6V6.6A2 2 0 0 1 5 4.6Z")
    else:
        body = RR(3, 4.6, 16.4, 15, 2)
    shutter = "M8.4 4.6H16V10H8.4Z"
    label = "M7 12.6H17V19.6H7Z"
    return [body, shutter, label]


def CAMERA(body=RR(2.8, 6.9, 18.4, 12.4, 2.4), bump="M8.8 6.9L9.8 4.7H14.2L15.2 6.9",
           lens=(12, 13.1, 3.5), dot=(18.3, 9.4, 0.95)):
    return {"body": body, "bump": bump, "lens": lens, "dot": dot}


def PLAY(solid_round=False):
    if solid_round:
        return ("M8.9 5.7Q8.1 5.2 8.1 6.2V17.8Q8.1 18.8 8.9 18.3L18.9 12.5"
                "Q19.7 12 18.9 11.5Z")
    return L((8.2, 5.3), (18.9, 12), (8.2, 18.7)) + "Z"


def BULB(cx=12, cy=10.2, r=4.3, rays=False):
    parts = [C(cx, cy, r), L((cx - 1.8, cy + r + 2.2), (cx + 1.8, cy + r + 2.2)),
             L((cx - 1.3, cy + r + 4.4), (cx + 1.3, cy + r + 4.4))]
    if rays:
        for a in (-155, -115, -90, -65, -25, 180, 0):
            rad = math.radians(a)
            p1 = (cx + (r + 1.7) * math.cos(rad), cy + (r + 1.7) * math.sin(rad))
            p2 = (cx + (r + 3.6) * math.cos(rad), cy + (r + 3.6) * math.sin(rad))
            if a in (180, 0):
                p1 = (cx + (r + 1.5) * math.cos(rad), cy - 0.4 + (r + 1.5) * math.sin(rad))
                p2 = (cx + (r + 3.2) * math.cos(rad), cy - 0.4 + (r + 3.2) * math.sin(rad))
            parts.append(L(p1, p2))
    return parts


# ---------- 字母 (Rtu / TCP 徽标) ----------

def LETTERS_RTU(x=6.8, top=8.4, bot=12.9):
    w = 1.05
    r1 = x + 2.1
    return [
        # R
        L((x, bot), (x, top)) + f"H{_p(r1)}A{_p(w)} {_p(w)} 0 0 1 {_p(r1)} {_p(top + 2 * w)}"
        f"H{x}M{_p(r1)} {_p(top + 2 * w)}L{_p(r1 + 1.3)} {bot}",
        # t
        L((x + 3.4, top + 0.3), (x + 3.4, bot - 0.2)) + f"Q{_p(x + 3.4)} {bot} {_p(x + 4.4)} {bot}",
        L((x + 2.5, top + 1.7), (x + 4.4, top + 1.7)),
        # u
        f"M{_p(x + 5.8)} {top}V{_p(bot - 0.9)}Q{_p(x + 5.8)} {bot} {_p(x + 6.85)} {bot}"
        f"Q{_p(x + 7.9)} {bot} {_p(x + 7.9)} {_p(bot - 0.9)}V{top}",
    ]


def LETTERS_TCP(x=6.9, top=8.4, bot=12.9):
    return [
        # T
        L((x, top), (x + 2.8, top)),
        L((x + 1.4, top), (x + 1.4, bot)),
        # C
        f"M{_p(x + 4.9)} {_p(top + 0.9)}A{_p(2.05)} {_p(2.05)} 0 1 0 {_p(x + 4.9)} {_p(bot - 0.9)}",
        # P
        L((x + 6.4, bot), (x + 6.4, top)) + f"H{_p(x + 8.0)}A{_p(1.45)} {_p(1.45)} 0 0 1 {_p(x + 8.0)} {_p(top + 2.9)}H{_p(x + 6.4)}",
    ]


# ---------- 图标规格 ----------

ICONS = []


def icon(cp, name, lin=None, fill=None, strokes=(), fills=(), cuts=(), sw=None, sw_bold=None):
    ICONS.append({
        "cp": cp, "name": name,
        "lin": lin or {"strokes": list(strokes), "fills": list(fills), "sw": sw},
        "fill": fill or {"strokes": list(strokes), "fills": list(fills),
                         "cuts": list(cuts), "sw_bold": sw_bold},
    })


# --- Modbus 家族 -----------------------------------------------------------
_frame = RR(3, 4, 18, 16, 2.6)
_rtu = LETTERS_RTU()
_tcp = LETTERS_TCP()
_underline = L((7, 15.9), (17, 15.9))
icon(0xE600, "a-ModbusRtujieru", strokes=[_frame, *_rtu, _underline], sw=1.8,
     fill={"fills": [_frame], "cuts": [*_rtu, _underline], "sw_bold": 0})
icon(0xE601, "a-ModbusTCPjieru", strokes=[_frame, *_tcp, _underline], sw=1.8,
     fill={"fills": [_frame], "cuts": [*_tcp, _underline], "sw_bold": 0})

# --- 坐标系家族 -------------------------------------------------------------
icon(0xE602, "zuobiaobiaoding",
     strokes=[C(12, 12, 5.6), L((12, 2.8), (12, 7.2)), L((12, 16.8), (12, 21.2)),
              L((2.8, 12), (7.2, 12)), L((16.8, 12), (21.2, 12))],
     fills=[DOT(12, 12, 1.5)],
     fill={"fills": [DOT(12, 12, 1.5)], "strokes": [C(12, 12, 5.6),
          L((12, 2.8), (12, 7.2)), L((12, 16.8), (12, 21.2)),
          L((2.8, 12), (7.2, 12)), L((16.8, 12), (21.2, 12))], "sw_bold": 2.9})
_zx_ax = [L((6, 20.6), (6, 4.2)), L((6, 17.6), (20.6, 17.6)),
          L((4.3, 6.4), (6, 3.4), (7.7, 6.4)), L((17.6, 15.9), (20.6, 17.6), (17.6, 19.3))]
_zx_trend = ARROW(9.4, 14.4, 17.6, 6.6)
icon(0xE603, "zuobiaoxi", strokes=_zx_ax + _zx_trend)
icon(0xE69E, "zhijiaozuobiaoxi",
     strokes=_zx_ax + [L((9.2, 15.2), (11.2, 13.2)), L((12.8, 11.6), (14.8, 9.6)),
                       L((16.4, 8), (18.4, 6)),
                       L((16.4, 4.9), (19.6, 4.9), (19.6, 8.1))])
_o3 = C(12, 13.2, 1.7)
_z_axis = ARROW(12, 11.4, 12, 3.6)
_y_axis = ARROW(13.8, 13.2, 20.4, 13.2)
_x_axis = ARROW(10.6, 14.5, 4.6, 20.2)
icon(0xE606, "zhuye-zuobiaoxi-chuangjianzuobiaoxi",
     strokes=[_o3, *_z_axis, *_y_axis, *_x_axis], sw=1.9)

# --- 配方/趋势/报告 ----------------------------------------------------------
_flask = ("M9.4 3.6H14.6M10.4 3.8V9.2L4.9 18.1A1.9 1.9 0 0 0 6.5 21H17.5"
          "A1.9 1.9 0 0 0 19.1 18.1L13.6 9.2V3.8")
icon(0xE604, "peifang", strokes=[_flask, L((7.4, 15.2), (16.6, 15.2))])
_qx_ax = [L((4.2, 20.6), (4.2, 4.4)), L((4.2, 20.6), (20.6, 20.6)),
          L((2.6, 6.6), (4.2, 3.6), (5.8, 6.6)), L((18.4, 18.9), (20.6, 20.6), (18.4, 22.3))]
_qx_line = ARROW(7, 16.8, 18.2, 7.2, head=3.6)
icon(0xE608, "qushi1", strokes=_qx_ax + _qx_line, sw=1.9,
     fill={"strokes": _qx_ax + _qx_line, "sw_bold": 3.1,
           "fills": ["M7 19.6L11 14.4L13.4 16.2L18.2 9.8V19.6Z"]})
icon(0xE87B, "qushi", strokes=_qx_ax, sw=1.9,
     fill={"strokes": _qx_ax, "sw_bold": 3.1,
           "fills": ["M7 19.6L11 14.4L13.4 16.2L18.2 9.8V19.6Z"]})
_bars = [RR(4.6, 14.6, 3.6, 6, 0.7), RR(10.2, 11.2, 3.6, 9.4, 0.7), RR(15.8, 7.4, 3.6, 13.2, 0.7)]
_bao_arrow = ARROW(4.6, 11.8, 17.6, 3.9, head=3.8)
icon(0xE646, "baogao", strokes=_bars + _bao_arrow, sw=1.9,
     fill={"fills": _bars, "strokes": _bao_arrow, "sw_bold": 3.0})

# --- 设置/齿轮家族 (保持家族一致, 齿数/孔径微差) -------------------------------
icon(0xE60A, "shezhi2", strokes=GEAR(), sw=1.7,
     fill={"strokes": GEAR(), "sw_bold": 2.6})
icon(0xE628, "shezhi1", strokes=GEAR(), sw=1.9)
icon(0xE67E, "Settings", strokes=GEAR(hole=3.0))
_g6 = GEAR(teeth=6, r_out=9.8, r_root=7.5, hole=3.5)
icon(0xE666, "mendianshezhi", strokes=_g6)
_g8big = GEAR(hole=4.3)
icon(0xE6A5, "a-shezhi-shucaidanshezhi", strokes=_g8big)
_hex = ("M12 3.2L19.6 7.6V16.4L12 20.8L4.4 16.4V7.6Z")
icon(0xE66B, "shezhi", strokes=[_hex, C(12, 12, 3.6)],
     fill={"fills": [_hex + C(12, 12, 3.6)]})

# --- 相机家族 ----------------------------------------------------------------
cam = CAMERA()
icon(0xE60C, "Camera",
     strokes=[cam["body"], cam["bump"], C(*cam["lens"][:2], cam["lens"][2])],
     fills=[DOT(*cam["dot"])],
     fill={"fills": [cam["body"], cam["bump"] + "Z"],
           "cuts": [C(*cam["lens"][:2], cam["lens"][2]), "F:" + DOT(*cam["dot"])],
           "sw_bold": 0})
icon(0xE67F, "xiangji1",
     strokes=[RR(3, 7, 18, 12, 2.2), "M9 7L10 5H14L15 7", C(12, 13.1, 3.2)],
     fills=[DOT(17.9, 9.6, 0.9)],
     fill={"fills": [RR(3, 7, 18, 12, 2.2), "M9 7L10 5H14L15 7Z"],
           "cuts": [C(12, 13.1, 3.2), "F:" + DOT(17.9, 9.6, 1.1)], "sw_bold": 0})
icon(0xE72C, "xiangji",
     strokes=[RR(3.4, 7.4, 17.2, 11.4, 2.2), "M9.4 7.4L10.3 5.6H13.7L14.6 7.4",
              C(12, 13.1, 2.9)],
     fill={"fills": [RR(3.4, 7.4, 17.2, 11.4, 2.2), "M9.4 7.4L10.3 5.6H13.7L14.6 7.4Z"],
           "cuts": [C(12, 13.1, 2.9)], "sw_bold": 0})
icon(0xE6CA, "shexiangji",
     strokes=[RR(3.2, 7.2, 12.8, 9.6, 2.2), L((16, 10.6), (20.4, 8.2), (20.4, 15.8), (16, 13.4)) + "Z"],
     fill={"fills": [RR(3.2, 7.2, 12.8, 9.6, 2.2),
                     L((16.6, 10.9), (20.4, 8.7), (20.4, 15.3), (16.6, 13.1)) + "Z"]})

# --- 窗口/状态 ----------------------------------------------------------------
icon(0xE60E, "status_min", strokes=[L((4.2, 12), (19.8, 12))])
icon(0xE639, "status_close", strokes=[L((5.2, 5.2), (18.8, 18.8)), L((18.8, 5.2), (5.2, 18.8))])
icon(0xE691, "status_max",
     strokes=ARROW(4.8, 19.2, 17.6, 6.4, head=4.0) + ARROW(19.2, 4.8, 6.4, 17.6, head=4.0),
     sw=1.9)
icon(0xE650, "zhankai",
     strokes=[L((4.6, 5.2), (11.2, 12), (4.6, 18.8)), L((12.8, 5.2), (19.4, 12), (12.8, 18.8))])
icon(0xE772, "huanyuanchuangkoubili",
     strokes=[RR(3.4, 4.4, 17.2, 15.2, 2), L((3.4, 9), (20.6, 9)),
              L((11.6, 9), (11.6, 19.6)),
              L((14.2, 13.4), (18.2, 13.4), (18.2, 17.4))], sw=1.9)

# --- 勾选/状态圈家族 -----------------------------------------------------------
_chk = L((7.6, 12.2), (10.9, 15.4), (16.6, 8.4))
icon(0xE612, "gouxuankuang-yigouxuan", strokes=[RR(3.4, 3.4, 17.2, 17.2, 3.6), _chk],
     fill={"fills": [RR(3.4, 3.4, 17.2, 17.2, 3.6)], "cuts": [_chk], "sw_bold": 0})
icon(0xE613, "ok", strokes=[C(12, 12, 8.6), _chk],
     fill={"fills": [C(12, 12, 8.6)], "cuts": [_chk], "sw_bold": 0})
_xmk = [L((9.2, 9.2), (14.8, 14.8)), L((14.8, 9.2), (9.2, 14.8))]
icon(0xE61A, "fail", strokes=[C(12, 12, 8.6), *_xmk],
     fill={"fills": [C(12, 12, 8.6)], "cuts": _xmk, "sw_bold": 0})
_pls = [L((12, 7.6), (12, 16.4)), L((7.6, 12), (16.4, 12))]
icon(0xE64F, "xinjian", strokes=[C(12, 12, 8.6), *_pls],
     fill={"fills": [C(12, 12, 8.6)], "cuts": _pls, "sw_bold": 0})
icon(0xE651, "icon", strokes=[C(12, 12, 8.6), L((12, 11.2), (12, 16.6))],
     fills=[DOT(12, 8.2, 1.25)],
     fill={"fills": [C(12, 12, 8.6)],
           "cuts": [L((12, 11), (12, 16.8)), "F:" + DOT(12, 8, 1.5)], "sw_bold": 0})
icon(0xE61B, "yunhang1", strokes=[C(12, 12, 8.6), L((10, 8.8), (10, 15.2)), L((14, 8.8), (14, 15.2))],
     fill={"fills": [C(12, 12, 8.6)],
           "cuts": [L((9.9, 8.6), (9.9, 15.4)), L((14.1, 8.6), (14.1, 15.4))], "sw_bold": 0})
icon(0xE67D, "tingzhi", strokes=[C(12, 12, 8.6), RR(8.9, 8.9, 6.2, 6.2, 1)],
     fill={"fills": [C(12, 12, 8.6)], "cuts": [RR(8.9, 8.9, 6.2, 6.2, 1)], "sw_bold": 0})
icon(0xE932, "warning1",
     strokes=[L((12, 3.9), (21.2, 19.4), (2.8, 19.4)) + "Z",
              L((12, 10.2), (12, 14.6)), DOT(12, 17.2, 0.4)],
     fill={"fills": [L((12, 3.9), (21.2, 19.4), (2.8, 19.4)) + "Z"],
           "cuts": [L((12, 10), (12, 14.8)), "F:" + DOT(12, 17.4, 1.3)], "sw_bold": 0})
icon(0xE981, "radio-button-kuai",
     strokes=[C(12, 12, 8.4), C(12, 12, 3.4)],
     fill={"fills": [C(12, 12, 8.4) + C(12, 12, 3.4), DOT(12, 12, 1.7)]})

# --- 运行/播放家族 --------------------------------------------------------------
icon(0xE640, "yunhang3", strokes=[PLAY()],
     fill={"fills": [PLAY()]})
icon(0xE610, "a-yunhangyunhangzhongzhunbeizhong", strokes=[PLAY(solid_round=True)],
     fill={"fills": [PLAY(solid_round=True)]})
icon(0xE7DC, "run-solid", strokes=[PLAY(solid_round=True)],
     fill={"fills": [PLAY(solid_round=True)]})
icon(0xE809, "yunhang", strokes=[PLAY()],
     fill={"fills": [PLAY()]})
icon(0xE6C1, "yunhang2", strokes=[PLAY(solid_round=True)])

# --- 保存家族 ------------------------------------------------------------------
_f2 = FLOPPY()
icon(0xE63F, "baocun",
     strokes=[_f2[0], _f2[1], L((7, 12.8), (17, 12.8), (17, 19.6), (7, 19.6)) + "Z"],
     fill={"fills": [_f2[0] + _f2[1] + "M7 12.8H17V19.6H7Z"]})
icon(0xE65C, "baocun2",
     strokes=[RR(4.4, 4.4, 15.2, 15.2, 1.8), "M8.6 4.4H15.6V9.6H8.6Z",
              L((7.4, 13), (16.6, 13), (16.6, 19.6), (7.4, 19.6)) + "Z"],
     fill={"fills": [RR(4.4, 4.4, 15.2, 15.2, 1.8) + "M8.6 4.4H15.6V9.6H8.6Z"
                     + "M7.4 13H16.6V19.6H7.4Z"]})
icon(0xE67C, "baocun1",
     strokes=[RR(3.6, 3.6, 16.8, 16.8, 2.6), "M8.8 3.6H15.2V9.8H8.8Z",
              L((7.6, 13.4), (16.4, 13.4), (16.4, 20.4), (7.6, 20.4)) + "Z"], sw=1.9,
     fill={"fills": [RR(3.6, 3.6, 16.8, 16.8, 2.6) + "M8.8 3.6H15.2V9.8H8.8Z"
                     + "M7.6 13.4H16.4V20.4H7.6Z"]})

# --- 文件夹家族 ----------------------------------------------------------------
_fol = FOLDER_SHAPE()[0]
icon(0xE60F, "wenjianjia1", strokes=[
    "M3.4 7.2A1.8 1.8 0 0 1 5.2 5.4H9.4L11.4 7.6H18.8A1.8 1.8 0 0 1 20.6 9.4V17"
    "A1.8 1.8 0 0 1 18.8 18.8H5.2A1.8 1.8 0 0 1 3.4 17Z"])
icon(0xE80C, "wenjianjia",
     strokes=["M3.4 7.2A1.8 1.8 0 0 1 5.2 5.4H9.4L11.4 7.6H18.8A1.8 1.8 0 0 1 20.6 9.4V17"
              "A1.8 1.8 0 0 1 18.8 18.8H5.2A1.8 1.8 0 0 1 3.4 17Z"],
     fill={"fills": ["M3.4 17V7.2A1.8 1.8 0 0 1 5.2 5.4H9.4L11.4 7.6H18.8A1.8 1.8 0 0 1 20.6 9.4V17"
                     "A1.8 1.8 0 0 1 18.8 18.8H5.2A1.8 1.8 0 0 1 3.4 17Z"]})
icon(0xEAC5, "24gf-folderStar",
     strokes=["M3.4 7.2A1.8 1.8 0 0 1 5.2 5.4H9.4L11.4 7.6H18.8A1.8 1.8 0 0 1 20.6 9.4V17"
              "A1.8 1.8 0 0 1 18.8 18.8H5.2A1.8 1.8 0 0 1 3.4 17Z",
              STAR(12, 13.9, 3.3, 1.45)],
     fill={"fills": ["M3.4 17V7.2A1.8 1.8 0 0 1 5.2 5.4H9.4L11.4 7.6H18.8A1.8 1.8 0 0 1 20.6 9.4V17"
                     "A1.8 1.8 0 0 1 18.8 18.8H5.2A1.8 1.8 0 0 1 3.4 17Z"],
           "cuts": [STAR(12, 13.9, 3.5, 1.55)], "sw_bold": 0})

# --- 主页家族 ------------------------------------------------------------------
_h1 = HOUSE()
icon(0xE65D, "home1", strokes=_h1,
     fill={"fills": [HOUSE(door=True)[1]], "strokes": [_h1[0]], "sw_bold": 3.0})
_h2 = HOUSE()
_leaf = "M15.7 5.3Q17.3 2.6 20.3 3.3Q19.8 6.3 16.7 5.9Z"
icon(0xE7FE, "home2", strokes=_h2 + [_leaf],
     fill={"fills": [HOUSE(door=True)[1]], "strokes": [_h2[0], _leaf], "sw_bold": 3.0})

# --- 用户/添加 ------------------------------------------------------------------
icon(0xE7B2, "user1", strokes=[C(12, 8, 3.7),
     "M4.9 19.8A7.1 7.1 0 0 1 19.1 19.8"],
     fill={"fills": ["M12 4.3A3.7 3.7 0 1 1 12 11.7A3.7 3.7 0 1 1 12 4.3Z",
                     "M4.9 19.8A7.1 7.1 0 0 1 19.1 19.8Z"]})
icon(0xE619, "tianjia_huaban", strokes=[L((12, 4.2), (12, 19.8)), L((4.2, 12), (19.8, 12))])

# --- 灯光家族 ------------------------------------------------------------------
_bulb_base = [L((10.2, 16.7), (13.8, 16.7)), L((10.7, 19), (13.3, 19))]
icon(0xE6D1, "tishi", strokes=BULB(),
     fill={"fills": [C(12, 10.2, 4.3)], "strokes": _bulb_base})
icon(0xE617, "tianjiaguangyuan", strokes=BULB(cy=10.6, r=3.9, rays=True), sw=1.9)
_jg = ("M14.6 4.4V19.6A0 0 0 0 1 14.6 19.6"
       "A7.6 7.6 0 0 0 14.6 4.4")
icon(0xE73D, "jinguangdengguan",
     strokes=[L((14.6, 4.4), (14.6, 19.6)),
              "M14.6 4.4A7.6 7.6 0 0 1 14.6 19.6",
              L((3.6, 6.8), (8.8, 8.6)), L((2.8, 12), (8.4, 12)), L((3.6, 17.2), (8.8, 15.4))],
     sw=1.9)

# --- PLC 触点 ------------------------------------------------------------------
icon(0xE6F2, "LDshangshengyanchudian",
     strokes=[L((6.4, 4), (6.4, 20)), L((17.6, 4), (17.6, 20)),
              L((2.8, 12), (6.4, 12)), L((17.6, 12), (21.2, 12)),
              L((12, 16.6), (12, 8.2)),
              L((9.6, 10.6), (12, 7.4), (14.4, 10.6))], sw=1.9)

# --- 网络/工作流 ----------------------------------------------------------------
_net_nodes = [RR(9.2, 3.4, 5.6, 4.6, 1), RR(3.2, 16, 5.6, 4.6, 1), RR(15.2, 16, 5.6, 4.6, 1)]
_net_lines = [L((12, 8), (12, 12.2), (6, 12.2), (6, 16)),
              L((12, 12.2), (18, 12.2), (18, 16))]
icon(0xE62E, "wangluoxitong",
     strokes=_net_nodes + _net_lines, sw=1.9,
     fill={"fills": _net_nodes, "strokes": _net_lines, "sw_bold": 2.7})
icon(0xE6BC, "workflow",
     strokes=[L((12, 8.6), (15.4, 12), (12, 15.4), (8.6, 12)) + "Z",
              RR(16.6, 4.2, 4.8, 4.4, 0.9), RR(16.6, 15.4, 4.8, 4.4, 0.9),
              L((15.4, 12), (19, 12), (19, 8.6)),
              L((19, 15.4), (19, 12)) + "M8.6 12H5V8.6"], sw=1.9)

# --- 波形/模板/文档 ---------------------------------------------------------------
icon(0xE6CB, "yuzhifenge",
     strokes=[RR(3.4, 5.4, 17.2, 13.2, 1.8),
              L((5.8, 12.4), (7.2, 9.6), (8.6, 14.4), (10, 8.4), (11.4, 14.8),
                (12.8, 10.4), (14, 12.4), (15.6, 9.8), (17, 12.8), (18.2, 11.6))], sw=1.8)
_back_page = RR(8.2, 3.6, 12.2, 12.2, 1.6)
_front_page = RR(3.6, 8.2, 12.2, 12.2, 1.6)
icon(0xE66A, "mobancaidan",
     strokes=[_back_page, _front_page], sw=1.9,
     fill={"fills": [_back_page, _front_page],
           "cuts": [_front_page], "sw_cut": 1.5})
_doc = "M6.4 3.6H13.8L17.8 7.6V20.4H6.4Z"
_doc_fold = L((13.8, 3.6), (13.8, 7.6), (17.8, 7.6))
_doc_plus = [L((12.1, 12.6), (12.1, 18)), L((9.4, 15.3), (14.8, 15.3))]
icon(0xE662, "chuangjiantubiao",
     strokes=[_doc, _doc_fold, *_doc_plus], sw=1.9,
     fill={"fills": [_doc], "cuts": [_doc_fold, *_doc_plus], "sw_cut": 2.4})

# --- 另存为/导出 ------------------------------------------------------------------
icon(0xE627, "lingcunwei1",
     strokes=["M18.6 10.4V17.8A1.9 1.9 0 0 1 16.7 19.7H6.3A1.9 1.9 0 0 1 4.4 17.8V7.4"
              "A1.9 1.9 0 0 1 6.3 5.5H13",
              L((13.2, 10.8), (19.8, 4.2)),
              L((15.4, 3.9), (20.1, 3.9), (20.1, 8.6))], sw=1.9)
icon(0xE685, "lingcunwei",
     strokes=[RR(3.6, 3.6, 13.8, 13.8, 3),
              L((13.2, 13.2), (19.8, 19.8)),
              L((15.4, 20.6), (20.7, 20.7), (20.6, 15.4))],
     fill={"fills": [RR(3.6, 3.6, 13.8, 13.8, 3)],
           "cuts": ["M17.8 9.4A7 7 0 0 0 9.4 17.8",
                    L((17.2, 12.4), (17.9, 8.7), (14.2, 9.4))],
           "strokes": [L((13.2, 13.2), (19.8, 19.8)),
                       L((15.4, 20.6), (20.7, 20.7), (20.6, 15.4))],
           "sw_bold": 3.0})

# --- 其他 ----------------------------------------------------------------------
_icon_test_fill = ("M12 3.4A8.6 8.6 0 1 1 12 20.6A8.6 8.6 0 0 1 12 3.4Z"
                   "M12 6.6A5.4 5.4 0 0 0 12 17.4Z")
icon(0xE661, "icon-test",
     strokes=[C(12, 12, 8.6), DOT(12, 12, 3.0)],
     fill={"fills": [_icon_test_fill, DOT(12, 12, 2.2)]})
_arc = ARC(12, 12, 8.2, -50, 235)
# 弧线终点切向箭头: sweep=1 时切向角 = 终角 + 90
_tip = (12 + 8.2 * math.cos(math.radians(235)), 12 + 8.2 * math.sin(math.radians(235)))
_dir = math.radians(235 + 90)
_w1 = (_tip[0] + 3.6 * math.cos(_dir + math.radians(150)), _tip[1] + 3.6 * math.sin(_dir + math.radians(150)))
_w2 = (_tip[0] + 3.6 * math.cos(_dir - math.radians(150)), _tip[1] + 3.6 * math.sin(_dir - math.radians(150)))
icon(0xE68D, "total",
     strokes=[_arc, L(_w1, _tip, _w2)], sw=2.0)
icon(0xE761, "cj",
     strokes=[RR(4, 4, 11.4, 11.4, 1.8), RR(8.6, 8.6, 11.4, 11.4, 1.8)], sw=1.9,
     fill={"fills": [RR(4, 4, 11.4, 11.4, 1.8), RR(8.6, 8.6, 11.4, 11.4, 1.8)]})
_route = "M7.4 18.6H13A4 4 0 0 0 13 10.6H11A4 4 0 0 1 11 5.4H16.6"
icon(0xE692, "lujing",
     strokes=[_route], fills=[DOT(5.4, 18.6, 2.0), DOT(18.6, 5.4, 2.0)],
     fill={"fills": [DOT(5.4, 18.6, 2.4), DOT(18.6, 5.4, 2.4)],
           "strokes": [_route], "sw_bold": 2.9})
icon(0xE9AE, "modbus",
     strokes=[L((4.6, 19), (4.6, 5.4), (12, 13.8), (19.4, 5.4), (19.4, 19))],
     fill={"strokes": [L((4.6, 19), (4.6, 5.4), (12, 13.8), (19.4, 5.4), (19.4, 19))],
           "sw_bold": 3.6})
icon(0xEA89, "24gf-stop", strokes=[RR(4.8, 4.8, 14.4, 14.4, 2.6)],
     fill={"fills": [RR(4.8, 4.8, 14.4, 14.4, 2.6)]})
icon(0xE611, "icon-", strokes=[CHECK(((4.6, 12.6), (10, 18), (19.4, 6.2)))],
     fill={"strokes": [CHECK(((4.6, 12.6), (10, 18), (19.4, 6.2)))], "sw_bold": 3.6})
