namespace Junevy.Controls.Common;

/// <summary>
/// 确认对话框的默认按钮（响应 <c>Enter</c> 的那颗）。仅 <see cref="ConfirmDialogButtons.OkCancel"/>
/// 有两颗按钮、此项才有意义；<see cref="ConfirmDialogButtons.Ok"/> 与
/// <see cref="ConfirmDialogButtons.Cancel"/> 只有一颗按钮，它天然就是默认按钮。
/// </summary>
public enum ConfirmDialogDefaultButton
{
    /// <summary>「确定」为默认按钮，默认值（回车即确认）。</summary>
    Confirm,

    /// <summary>「取消」为默认按钮——破坏性操作建议取此项，避免误触回车。</summary>
    Cancel
}