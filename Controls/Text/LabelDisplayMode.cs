namespace Junevy.Controls.Controls.Text
{
    /// <summary>
    /// Label 的显示模式。
    /// </summary>
    public enum LabelDisplayMode
    {
        /// <summary>
        /// 错误色块标签（默认，红色背景与固定错误图标）。
        /// </summary>
        Error = 0,

        /// <summary>
        /// 成功色块标签（绿色背景与固定成功图标）。
        /// </summary>
        Success = 1,

        /// <summary>
        /// 警告色块标签（黄色背景与固定警告图标）。
        /// </summary>
        Warning = -1,

        /// <summary>
        /// 无边框错误提示（红色图标 + 文本，无背景色块）。
        /// </summary>
        BorderlessError = 10,

        /// <summary>
        /// 无边框警告提示（黄色图标 + 文本，无背景色块）。
        /// </summary>
        BorderlessWarning = -11,

        /// <summary>
        /// 无边框通知提示（主题色图标 + 文本，无背景色块）。
        /// </summary>
        BorderlessNotice = 11,

        /// <summary>
        /// 中性标签：背景跟随 <see cref="Background"/>，图标由 <c>atc:Icon.Icon</c> 提供，为空时折叠图标区域。
        /// </summary>
        Neutral = 100,
    }
}
