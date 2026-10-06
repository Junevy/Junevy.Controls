namespace Junevy.Controls.Common;

/// <summary>
/// 确认对话框的操作结果。关闭标题栏 ✕、<c>Esc</c>、<c>Alt+F4</c> 一律归为
/// <see cref="Cancel"/>——不存在「既没确认也没取消」的第三种结果。
/// </summary>
public enum ConfirmDialogResult
{
    /// <summary>点了「确定」。</summary>
    Confirm,

    /// <summary>点了「取消」，或以关闭按钮 / <c>Esc</c> / <c>Alt+F4</c> 关闭。</summary>
    Cancel
}