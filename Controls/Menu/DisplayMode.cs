namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// <see cref="TreeView"/> 的行首观感：<see cref="Chevron"/> 显示可点击的展开箭头；
    /// <see cref="Indicator"/> 折叠箭头，改由选中有子项节点左侧的 accent 指示条标示层级归属。
    /// 两种观感都渲染图标与标题，图标字号一律取 <see cref="Junevy.Controls.AttachedProperties.Icon.IconSize"/>。
    /// </summary>
    public enum TreeViewDisplayMode
    {
        /// <summary>行首显示展开箭头（默认）。</summary>
        Chevron,

        /// <summary>行首不显示箭头；选中的有子项节点左侧显示 accent 指示条。</summary>
        Indicator,
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
