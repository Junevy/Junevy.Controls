namespace Junevy.Controls.Controls.Bar
{
    /// <summary>
    /// InfoBar 的显示布局。
    /// </summary>
    public enum InfoBarDisplayMode
    {
        /// <summary>
        /// 头像 + 名称（+ 可选设置按钮），整行可点击弹出菜单（默认）。
        /// </summary>
        Text,

        /// <summary>
        /// 仅显示头像；悬停时经 ToolTip 显示名称，点击弹出菜单。
        /// </summary>
        AvatarOnly,
    }
}
