namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// TreeView 的显示模式：<see cref="Normal"/> 显示图标与标题，<see cref="Icon"/> 仅显示图标。
    /// </summary>
    public enum DisplayMode
    {
        Normal,
        Icon,
    }

    /// <summary>
    /// SideMenu 的显示模式：<see cref="Horizontal"/> 图标与标题横向排列，
    /// <see cref="Vertical"/> 图标在上、标题在下（紧凑图标导航，默认宽度 60）。
    /// </summary>
    public enum SideMenuDisplayMode
    {
        Horizontal,
        Vertical,
    }
}
