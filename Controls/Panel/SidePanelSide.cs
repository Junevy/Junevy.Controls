namespace Junevy.Controls.Controls.Panel;

/// <summary>
/// SidePanel 的滑出方向：表示面板从父容器的哪一个边缘滑出。
/// </summary>
public enum SidePanelSide
{
    /// <summary>从左边缘滑出，内容停靠左侧、垂直方向默认填满。</summary>
    Left,

    /// <summary>从右边缘滑出，内容停靠右侧、垂直方向默认填满。</summary>
    Right,

    /// <summary>从上边缘滑出，内容停靠顶部、水平方向默认填满。</summary>
    Top,

    /// <summary>从下边缘滑出，内容停靠底部、水平方向默认填满。</summary>
    Bottom,
}
