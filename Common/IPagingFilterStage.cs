namespace Junevy.Controls.Common;

/// <summary>
/// 分页阶段契约。协调器先把视图过滤到「宿主条件 + 原有排序」，再让分页据此算页窗口与计数，
/// 最后把分页谓词组合进视图过滤器——顺序不可颠倒，否则页窗口会按未筛选的 population 计算。
/// </summary>
internal interface IPagingFilterStage
{
    /// <summary>
    /// 视图此刻已应用宿主过滤且保持排序；枚举它即得分页的输入 population。
    /// 实现方在此统计总数、钳制页码、收集当前页成员。
    /// </summary>
    void Prepare();

    /// <summary>分页谓词：该条目是否落在当前页窗口内。</summary>
    bool Accept(object item);
}
