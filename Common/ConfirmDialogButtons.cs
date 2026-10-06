namespace Junevy.Controls.Common;

/// <summary>
/// 确认对话框的按钮组合，决定 <c>ConfirmDialogWindow</c> 页脚区显示哪些按钮。
/// </summary>
public enum ConfirmDialogButtons
{
    /// <summary>仅「确定」：用于告知一个必须知晓的结果（如「操作已完成」），关闭等价于确定。</summary>
    Ok,

    /// <summary>「确定」+「取消」：操作前确认，默认值。</summary>
    OkCancel,

    /// <summary>仅「取消」：用于二次确认某项放弃动作（如「确定放弃本次编辑？」）。</summary>
    Cancel
}