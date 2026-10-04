using System.Windows;
using System.Windows.Controls;

namespace Junevy.Controls.Controls.Menu
{
    /// <summary>
    /// 菜单条目容器样式选择器：<see cref="System.Windows.Controls.Separator"/> 是自带容器
    /// （<c>MenuBase.IsItemItsOwnContainerOverride</c> 对 Separator 返回 true），而
    /// <c>JunevyContextMenuItemStyle</c> 的 TargetType 是 <see cref="MenuItem"/>——
    /// 经 <c>ItemContainerStyle</c> 统一注入时，容器生成阶段会因样式类型不匹配抛
    /// <see cref="InvalidOperationException"/>（宿主右键即闪退）。选择器按条目类型分发：
    /// MenuItem → 条目样式；Separator → 分隔符样式；其余返回 null（走默认外观）。
    /// 样式解析优先走 <see cref="Application.Current"/>（接管约定：宿主把 Themes/Generic.xaml
    /// 合并进应用资源），回退容器自身资源链（窗口级合并同样生效）；生成中的容器尚未挂树，
    /// 仅靠容器链解析会静默落空。
    /// </summary>
    public class JunevyMenuItemStyleSelector : StyleSelector
    {
        /// <summary>共享实例（样式字典以 StaticResource 引用，无逐实例状态）。</summary>
        public static readonly JunevyMenuItemStyleSelector Default = new();

        public override Style? SelectStyle(object item, DependencyObject container)
        {
            var lookup = container as FrameworkElement;
            var app = Application.Current;
            return item switch
            {
                Separator => Resolve(app, lookup, "JunevyContextMenuSeparatorStyle"),
                MenuItem => Resolve(app, lookup, "JunevyContextMenuItemStyle"),
                _ => null
            };
        }

        private static Style? Resolve(Application? app, FrameworkElement? container, string key)
        {
            return app?.TryFindResource(key) as Style
                ?? container?.TryFindResource(key) as Style;
        }
    }
}
