namespace Junevy.Controls.AttachedProperties
{
    /// <summary>
    /// 标题内容在「标题区域」内的水平靠齐方式。
    /// <para>
    /// 标题区域的可移动空间由 <see cref="TitleAssist.TitleWidth"/> 决定：设置了固定宽度时，区域就是该宽度
    /// 并锚在行首（上/下方位）或所在列的起始处，靠齐发生在这段固定宽度内部；
    /// 未设置时，标题在控件上/下方（<see cref="TitlePlacement.Top"/> / <see cref="TitlePlacement.Bottom"/>）
    /// 的区域等于控件输入区的整行宽度（因此靠右即贴输入框右端），在左/右侧
    /// （<see cref="TitlePlacement.Left"/> / <see cref="TitlePlacement.Right"/>）的区域等于标题自身内容的宽度
    /// （没有多余空间，靠齐不产生可见位移）。
    /// </para>
    /// <para>
    /// 靠齐作用于「标题文字 + 必填标识」组成的整组，两者始终一起移动，不会拆开。
    /// </para>
    /// </summary>
    public enum TitleAlignment
    {
        /// <summary>
        /// 靠标题区域左侧（默认）。
        /// </summary>
        Left,

        /// <summary>
        /// 靠标题区域右侧。
        /// </summary>
        Right,

        /// <summary>
        /// 在标题区域内水平居中。
        /// </summary>
        Center,
    }
}
