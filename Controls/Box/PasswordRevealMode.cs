namespace Junevy.Controls.Controls.Box
{
    /// <summary>
    /// 密码明文显示按钮的触发方式。
    /// </summary>
    public enum PasswordRevealMode
    {
        /// <summary>点击切换：单击按钮显示明文，再次单击隐藏。</summary>
        Click = 0,

        /// <summary>长按显示：按下按钮即显示明文，松开（或按住移出按钮）立即隐藏。</summary>
        PressAndHold = 1,
    }
}
