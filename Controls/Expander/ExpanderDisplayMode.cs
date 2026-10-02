namespace Junevy.Controls.Controls.Expander;

/// <summary>
/// <see cref="ExpanderPanel"/> 头部的呈现模式。
/// </summary>
public enum ExpanderDisplayMode
{
    /// <summary>经典模式（默认）：窄条头部（旋转箭头 + Header 内容），支持四方向展开。</summary>
    Classic = 0,

    /// <summary>
    /// 卡片模式：高头部卡片——左侧图标（<see cref="ExpanderPanel.Icon"/>）、
    /// 标题（Header）与补充说明（<see cref="ExpanderPanel.Description"/>）、
    /// 右侧扩展槽（<see cref="ExpanderPanel.HeaderExtra"/>）与展开箭头；内容向下展开。
    /// </summary>
    Card = 1,
}
