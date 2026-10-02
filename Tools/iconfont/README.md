# 图标字体（iconfont）重绘工具链

`Resources/Font/iconfont.ttf`（线性/描边）与 `Resources/Font/iconfont-filled.ttf`
（面性/填充）的唯一真值来源。**不要直接手改 TTF**，改规格后重新生成。

| 文件 | 作用 |
| --- | --- |
| `spec.py` | 67 个图标的设计规格：24×24 网格上的 SVG 路径骨架，每图标含 `lin`（线性）与 `fill`（面性）两套表达 |
| `pipeline.py` | 渲染管线：描边转轮廓（skia）、布尔并/差、TrueType 绕向归一化、字形编译 |
| `emit.py` | 编译两套 TTF（保持旧字体的码点、字形名、1024 em 与 896/-128 度量），并输出验收拼图 |

```bash
python Tools/iconfont/emit.py   # 重建两套字体 -> Resources/Font/，对比图 -> .workbuddy/tmp/icons_check.png
```

## 设计约定

- 设计网格 24×24（y 向下），内容一般落在 `3..21`，编译时映射到 1024 em
  （ascent 896 / descent -128），与旧版 iconfont 完全一致。
- 线性版：骨架路径统一 2/24 描边（个别细节 1.7~1.9），圆头圆角连接。
- 面性版：主体实心填充 + 统一镂空细节（`cuts`），骨架加粗默认 3.3/24。
- 同语义重复码点（保存×3、设置×6、播放×5 等）全部保留、共用基元函数，
  仅允许齿数/孔径等微差，保证家族一致。
- 码点与字形名锁定：`emit.py` 会先从 `.workbuddy/tmp/iconfont.orig.ttf`
  （原始字体快照）读取 cmap 做全量校验，缺失/多出任何码点直接构建失败。

## 新增 / 修改一个图标

1. 在 `spec.py` 里定位（或新增）`icon(码点, 名字, ...)` 条目；新码点必须
   同时给出 `lin` 与 `fill`，否则面性版会退化为加粗描边。
2. 优先复用 `GEAR()` / `HOUSE()` / `FLOPPY()` / `PLAY()` / `BULB()` /
   `CAMERA()` / `STAR()` / `ARROW()` 等基元，保持风格统一。
3. 运行 `python Tools/iconfont/emit.py`，打开 `.workbuddy/tmp/icons_check.png`
   核对大字号（110px）与三联小字号（16px）两档渲染。
4. 同步更新仓库根目录 `CHANGELOG.md` 与 `README.md` 的图标说明。

## 依赖

Python 3.12+：`fontTools`、`skia-python`、`svgpathtools`、`Pillow`（仅预览图）。
