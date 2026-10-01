namespace Junevy.Controls.Controls.Bar
{
    /// <summary>
    /// AppBar 的布局模式。
    /// </summary>
    public enum AppBarMode
    {
        /// <summary>
        /// 图标 + 标题 / 分隔线 / 工具栏，右侧系统按钮（默认）。
        /// </summary>
        Default,

        /// <summary>
        /// 单行：图标 + 应用名 + 菜单栏 + 弹性空白 + 系统按钮。
        /// </summary>
        MenuBar,

        /// <summary>
        /// 抽屉开关（最左）+ 整条居中的标题 + 系统按钮（最右）；
        /// 配合 <see cref="AppBar.Drawer"/> 显示下拉抽屉。
        /// </summary>
        Expandable,
    }
}
