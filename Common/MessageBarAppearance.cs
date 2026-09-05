namespace Junevy.Controls.Common;

/// <summary>
/// 严重程度外观，对应主题中的 <c>Theme.Brush.Status.*</c> 状态色。
/// </summary>
public enum MessageBarAppearance
{
    /// <summary>普通提示，使用 <c>Theme.Brush.Status.Info</c>。</summary>
    Informational,

    /// <summary>成功，使用 <c>Theme.Brush.Status.Success</c>。</summary>
    Success,

    /// <summary>警告，使用 <c>Theme.Brush.Status.Warning</c>。</summary>
    Warning,

    /// <summary>错误，使用 <c>Theme.Brush.Status.Danger</c>。</summary>
    Danger
}
