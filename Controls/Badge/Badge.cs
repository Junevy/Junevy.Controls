using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Badge;

/// <summary>
/// 角标控件：类似手机应用右上角的未读提醒，包裹任意内容（按钮、图标、菜单项等），
/// 在内容的指定角落叠加一个小圆点或未读数角标（超过 <see cref="MaxCount"/> 显示 <c>99+</c> 形式）。
/// </summary>
/// <remarks>
/// 用法：<c>&lt;jv:Badge Count="5"&gt;&lt;Button Content="消息"/&gt;&lt;/jv:Badge&gt;</c>。
/// 角标以自身中心停靠在包裹内容的角落上（默认微微外溢），不影响内容自身的布局尺寸；
/// <see cref="OffsetX"/>/<see cref="OffsetY"/> 可在角落基础上做像素级微调。
/// </remarks>
[TemplatePart(Name = PartBadge, Type = typeof(Border))]
[TemplatePart(Name = PartTranslate, Type = typeof(TranslateTransform))]
[TemplatePart(Name = PartText, Type = typeof(TextBlock))]
public class Badge : ContentControl
{
    private const string PartBadge = "PART_Badge";
    private const string PartTranslate = "PART_Translate";
    private const string PartText = "PART_Text";

    private Border? _badge;
    private TranslateTransform? _translate;
    private TextBlock? _text;

    public static readonly DependencyProperty CountProperty =
        DependencyProperty.Register(
            nameof(Count),
            typeof(int),
            typeof(Badge),
            new PropertyMetadata(0, OnBadgePropertyChanged));

    public static readonly DependencyProperty MaxCountProperty =
        DependencyProperty.Register(
            nameof(MaxCount),
            typeof(int),
            typeof(Badge),
            new PropertyMetadata(99, OnBadgePropertyChanged),
            value => value is int count && count >= 0);

    public static readonly DependencyProperty IsDotProperty =
        DependencyProperty.Register(
            nameof(IsDot),
            typeof(bool),
            typeof(Badge),
            new PropertyMetadata(false, OnBadgePropertyChanged));

    public static readonly DependencyProperty CornerProperty =
        DependencyProperty.Register(
            nameof(Corner),
            typeof(BadgeCorner),
            typeof(Badge),
            new PropertyMetadata(BadgeCorner.TopRight, OnBadgePropertyChanged));

    public static readonly DependencyProperty OffsetXProperty =
        DependencyProperty.Register(
            nameof(OffsetX),
            typeof(double),
            typeof(Badge),
            new PropertyMetadata(0d, OnBadgePropertyChanged));

    public static readonly DependencyProperty OffsetYProperty =
        DependencyProperty.Register(
            nameof(OffsetY),
            typeof(double),
            typeof(Badge),
            new PropertyMetadata(0d, OnBadgePropertyChanged));

    static Badge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Badge),
            new FrameworkPropertyMetadata(typeof(Badge)));

        // 内容替换后内容 Margin 可能变化，停靠偏移需要按新内容重算。
        ContentProperty.OverrideMetadata(
            typeof(Badge),
            new FrameworkPropertyMetadata(OnBadgePropertyChanged));
    }

    /// <summary>未读数；小于等于 0 时角标隐藏，大于 <see cref="MaxCount"/> 时显示 <c>99+</c> 形式。</summary>
    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    /// <summary>未读数显示上限；超出后显示 <c>{MaxCount}+</c>，默认 99。</summary>
    public int MaxCount
    {
        get => (int)GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }

    /// <summary>是否以纯圆点显示（不显示数字）；显示/隐藏仍由 <see cref="Count"/> 控制。</summary>
    public bool IsDot
    {
        get => (bool)GetValue(IsDotProperty);
        set => SetValue(IsDotProperty, value);
    }

    /// <summary>角标停靠的内容角落，默认右上角；运行时切换立即生效。</summary>
    public BadgeCorner Corner
    {
        get => (BadgeCorner)GetValue(CornerProperty);
        set => SetValue(CornerProperty, value);
    }

    /// <summary>在角落停靠位置基础上的水平微调偏移（像素）。</summary>
    public double OffsetX
    {
        get => (double)GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

    /// <summary>在角落停靠位置基础上的垂直微调偏移（像素）。</summary>
    public double OffsetY
    {
        get => (double)GetValue(OffsetYProperty);
        set => SetValue(OffsetYProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_badge is not null)
        {
            _badge.SizeChanged -= OnBadgeSizeChanged;
        }

        _badge = GetTemplateChild(PartBadge) as Border;
        _translate = GetTemplateChild(PartTranslate) as TranslateTransform;
        _text = GetTemplateChild(PartText) as TextBlock;

        if (_badge is not null)
        {
            _badge.SizeChanged += OnBadgeSizeChanged;
        }

        ApplyBadgeState();
    }

    private static void OnBadgePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Badge)d).ApplyBadgeState();
    }

    private void OnBadgeSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // 数字位数变化（如 5 → 99+）或圆点/数字模式切换会改变角标尺寸，需要重新计算停靠偏移。
        ApplyBadgeState();
    }

    private bool IsLeftCorner => Corner is BadgeCorner.TopLeft or BadgeCorner.BottomLeft;

    private bool IsTopCorner => Corner is BadgeCorner.TopLeft or BadgeCorner.TopRight;

    /// <summary>应用角标状态：显隐、文本与停靠偏移。</summary>
    private void ApplyBadgeState()
    {
        if (_badge is null || _translate is null)
        {
            return;
        }

        bool visible = Count > 0;
        _badge.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        if (!visible)
        {
            return;
        }

        if (!IsDot && _text is not null)
        {
            _text.Text = Count > MaxCount ? $"{MaxCount}+" : Count.ToString();
        }

        // 从折叠切回可见时布局可能尚未重排，先强制刷新拿到角标实际尺寸再计算偏移。
        _badge.UpdateLayout();
        UpdateBadgeTransform();
    }

    /// <summary>
    /// 计算停靠偏移：角标中心对准包裹内容（视觉边界，补偿内容自身的 Margin）的指定角落，
    /// 再叠加 OffsetX/OffsetY 微调。
    /// </summary>
    private void UpdateBadgeTransform()
    {
        if (_badge is null || _translate is null)
        {
            return;
        }

        var contentMargin = (Content as FrameworkElement)?.Margin ?? default;
        double width = _badge.ActualWidth;
        double height = _badge.ActualHeight;

        double x = IsLeftCorner ? -width / 2 + contentMargin.Left : width / 2 - contentMargin.Right;
        double y = IsTopCorner ? -height / 2 + contentMargin.Top : height / 2 - contentMargin.Bottom;
        _translate.X = x + OffsetX;
        _translate.Y = y + OffsetY;
    }
}
