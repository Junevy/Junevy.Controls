using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace Junevy.Controls.Controls.Expander;

/// <summary>
/// 为 <see cref="ExpanderPanel"/> 暴露 UI 自动化的 ExpandCollapse 模式，
/// 使辅助工具（讲述人、UIA 客户端）能够读取并切换展开/折叠状态。
/// </summary>
public class ExpanderPanelAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider
{
    public ExpanderPanelAutomationPeer(ExpanderPanel owner)
        : base(owner)
    {
    }

    private ExpanderPanel OwnerPanel => (ExpanderPanel)Owner;

    public override object GetPattern(PatternInterface patternInterface)
    {
        return patternInterface == PatternInterface.ExpandCollapse ? this : base.GetPattern(patternInterface);
    }

    protected override string GetClassNameCore()
    {
        return "ExpanderPanel";
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Group;
    }

    public ExpandCollapseState ExpandCollapseState
    {
        get
        {
            bool isExpanded = OwnerPanel.IsExpanded;
            return isExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
        }
    }

    public void Expand()
    {
        OwnerPanel.SetCurrentValue(ExpanderPanel.IsExpandedProperty, true);
    }

    public void Collapse()
    {
        OwnerPanel.SetCurrentValue(ExpanderPanel.IsExpandedProperty, false);
    }
}
