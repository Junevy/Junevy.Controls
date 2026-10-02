"""编译图标字体: spec.py + pipeline.py -> iconfont.ttf (线性) / iconfont-filled.ttf (面性)。

用法: python Tools/iconfont/emit.py [--out DIR]
默认输出到 仓库根/Resources/Font/, 预览图输出到 .workbuddy/tmp/。
"""
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(os.path.dirname(HERE))
sys.path.insert(0, HERE)

import spec  # noqa: E402
from pipeline import glyph_from_spec  # noqa: E402
from fontTools.fontBuilder import FontBuilder  # noqa: E402
from fontTools.ttLib import TTFont, newTable  # noqa: E402


def build(style, family, psname, out_path):
    fb = FontBuilder(1024, isTTF=True)
    order = [".notdef"] + [ic["name"] for ic in spec.ICONS]
    fb.setupGlyphOrder(order)
    fb.setupCharacterMap({ic["cp"]: ic["name"] for ic in spec.ICONS})
    from fontTools.pens.ttGlyphPen import TTGlyphPen
    glyphs = {".notdef": TTGlyphPen(None).glyph()}
    for ic in spec.ICONS:
        glyphs[ic["name"]] = glyph_from_spec(ic["lin"] if style == "linear" else ic["fill"], style)
    fb.setupGlyf(glyphs)
    fb.setupHorizontalMetrics({n: (1024, 0) for n in order})
    fb.setupHorizontalHeader(ascent=896, descent=-128, lineGap=0)
    fb.setupNameTable({
        "familyName": family,
        "styleName": "Regular",
        "fullName": family,
        "psName": psname,
        "version": "Version 2.0",
    })
    fb.setupOS2(sTypoAscender=896, sTypoDescender=-128, sTypoLineGap=0,
                usWinAscent=896, usWinDescent=128,
                usWeightClass=400, usWidthClass=5, fsType=0)
    fb.setupPost(postTableFormat=2.0)
    fb.save(out_path)
    return out_path


def render_sheets(paths, out_png):
    """paths: [(label, ttf_path)] -> 大/小两档字号的对比长图。"""
    from PIL import Image, ImageDraw, ImageFont
    cell, big, small = 170, 110, 16
    cols = 8
    rows = (len(spec.ICONS) + cols - 1) // cols
    band = 34
    img = Image.new("RGB", (cols * cell, rows * (cell + band) * len(paths)), "white")
    dr = ImageDraw.Draw(img)
    for pi, (label, path) in enumerate(paths):
        fbig = ImageFont.truetype(path, big)
        fsml = ImageFont.truetype(path, small)
        ybase = pi * rows * (cell + band)
        for i, ic in enumerate(spec.ICONS):
            r, c = divmod(i, cols)
            x0, y0 = c * cell, ybase + r * (cell + band)
            ch = chr(ic["cp"])
            dr.text((x0 + 20, y0 + 8), ch, font=fbig, fill="black")
            for k in range(3):
                dr.text((x0 + 20 + k * 44, y0 + 130), ch, font=fsml, fill="black")
            dr.text((x0 + 4, y0 + cell - 2), f"{ic['name'][:16]} U+{ic['cp']:04X}",
                    font=ImageFont.load_default(size=11), fill="red")
            dr.rectangle([x0, y0, x0 + cell, y0 + cell + band], outline="#ccc")
        dr.text((8, ybase - 2), label, fill="blue")
    img.save(out_png)


def main():
    out_dir = os.path.join(ROOT, "Resources", "Font")
    orig_path = os.path.join(out_dir, "iconfont.ttf")
    snap = os.path.join(ROOT, ".workbuddy", "tmp", "iconfont.orig.ttf")
    os.makedirs(os.path.dirname(snap), exist_ok=True)
    if not os.path.exists(snap):
        import shutil
        shutil.copy2(orig_path, snap)
    original = TTFont(snap)
    cmap = original.getBestCmap()
    names = {cp: name for cp, name in cmap.items()}

    spec_cps = {ic["cp"] for ic in spec.ICONS}
    missing = set(names) - spec_cps
    extra = spec_cps - set(names)
    if missing or extra:
        raise SystemExit(f"码点不匹配 缺失={sorted(hex(c) for c in missing)} "
                         f"多余={sorted(hex(c) for c in extra)}")
    # 使用原始字形名 (含原始空格前缀等), 保证 post 表与旧字体一致
    for ic in spec.ICONS:
        ic["name"] = names[ic["cp"]]

    lin = build("linear", "iconfont", "iconfont",
                os.path.join(out_dir, "iconfont.ttf"))
    fil = build("filled", "iconfont-filled", "iconfont-filled",
                os.path.join(out_dir, "iconfont-filled.ttf"))

    tmp = os.path.join(ROOT, ".workbuddy", "tmp")
    os.makedirs(tmp, exist_ok=True)
    render_sheets([("LINEAR", lin), ("FILLED", fil)], os.path.join(tmp, "icons_check.png"))

    # 码点映射回归校验
    for p in (lin, fil):
        f = TTFont(p)
        assert f.getBestCmap().keys() == set(names.keys()), p
        assert f["head"].unitsPerEm == 1024
        assert f["hhea"].ascent == 896 and f["hhea"].descent == -128
    print("OK:", lin)
    print("OK:", fil)


if __name__ == "__main__":
    main()
