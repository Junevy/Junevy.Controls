using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Junevy.Controls.Controls.Expander;

/// <summary>
/// 可折叠面板控件：提供头部与内容区，支持展开/折叠的平滑过渡动画，
/// 可通过 <see cref="IsExpanded"/> 双向绑定状态、<see cref="ExpandDirection"/>
/// 配置展开方向、<see cref="AnimationDuration"/> 配置动画时长。
/// </summary>
[TemplatePart(Name = PartContentHost, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartScaleTransform, Type = typeof(ScaleTransform))]
public class ExpanderPanel : HeaderedContentControl
{
    private const string PartContentHost = "PART_ContentHost";
    private const string PartScaleTransform = "PART_ScaleTransform";

    private FrameworkElement? _contentHost;
    private ScaleTransform? _scaleTransform;

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(ExpanderPanel),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsExpandedChanged));

    public static readonly DependencyProperty ExpandDirectionProperty =
        DependencyProperty.Register(
            nameof(ExpandDirection),
            typeof(ExpandDirection),
            typeof(ExpanderPanel),
            new PropertyMetadata(ExpandDirection.Down, OnExpandDirectionChanged));

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(
            nameof(AnimationDuration),
            typeof(Duration),
            typeof(ExpanderPanel),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(200))),
            IsValidAnimationDuration);

    public static readonly DependencyProperty ToggleCommandProperty =
        DependencyProperty.Register(
            nameof(ToggleCommand),
            typeof(ICommand),
            typeof(ExpanderPanel),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(ExpanderPanel),
            new PropertyMetadata(null));

    public static readonly RoutedEvent ExpandedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Expanded),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ExpanderPanel));

    public static readonly RoutedEvent CollapsedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Collapsed),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ExpanderPanel));

    static ExpanderPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ExpanderPanel),
            new FrameworkPropertyMetadata(typeof(ExpanderPanel)));
    }

    /// <summary>是否展开；支持双向绑定。</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>展开方向：Down / Up / Left / Right；Left 表示内容向左展开（头部在右侧），与 WPF Expander 语义一致。</summary>
    public ExpandDirection ExpandDirection
    {
        get => (ExpandDirection)GetValue(ExpandDirectionProperty);
        set => SetValue(ExpandDirectionProperty, value);
    }

    /// <summary>展开/折叠过渡动画时长；为 <see cref="Duration.Automatic"/> 或 <see cref="Duration.Forever"/> 时视为无效。</summary>
    public Duration AnimationDuration
    {
        get => (Duration)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>状态切换时执行的命令；可用（CanExecute）时才执行。</summary>
    public ICommand? ToggleCommand
    {
        get => (ICommand?)GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }

    /// <summary>传递给 <see cref="ToggleCommand"/> 的参数。</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public event RoutedEventHandler Expanded
    {
        add => AddHandler(ExpandedEvent, value);
        remove => RemoveHandler(ExpandedEvent, value);
    }

    public event RoutedEventHandler Collapsed
    {
        add => AddHandler(CollapsedEvent, value);
        remove => RemoveHandler(CollapsedEvent, value);
    }

    /// <summary>切换展开/折叠状态。</summary>
    public void Toggle()
    {
        SetCurrentValue(IsExpandedProperty, !IsExpanded);
    }

    /// <summary>供派生类响应展开/折叠状态变化。</summary>
    protected virtual void OnIsExpandedChanged(bool isExpanded)
    {
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ExpanderPanelAutomationPeer(this);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _contentHost = GetTemplateChild(PartContentHost) as FrameworkElement;
        _scaleTransform = GetTemplateChild(PartScaleTransform) as ScaleTransform;

        ApplyDirection();
        ApplyExpansionState(IsExpanded, animate: false);
    }

    private void ApplyExpansionState(bool isExpanded, bool animate)
    {
        if (_contentHost is null || _scaleTransform is null)
        {
            return;
        }

        bool vertical = IsVerticalDirection;
        DependencyProperty scaleProperty = vertical
            ? ScaleTransform.ScaleYProperty
            : ScaleTransform.ScaleXProperty;

        // 停掉两个方向上残留的动画，避免中途反向切换时闪烁。
        _scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        _scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);

        if (!animate || AnimationDuration.TimeSpan <= TimeSpan.Zero)
        {
            SetScale(isExpanded ? 1d : 0d);
            _contentHost.Visibility = isExpanded ? Visibility.Visible : Visibility.Collapsed;
            return;
        }

        if (isExpanded)
        {
            _contentHost.Visibility = Visibility.Visible;
        }

        var animation = new DoubleAnimation(isExpanded ? 0d : 1d, isExpanded ? 1d : 0d, AnimationDuration.TimeSpan)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        animation.Completed += (_, _) =>
        {
            if (!isExpanded)
            {
                _contentHost.Visibility = Visibility.Collapsed;
            }
        };

        _scaleTransform.BeginAnimation(scaleProperty, animation);
    }

    private void ApplyDirection()
    {
        if (_contentHost is null)
        {
            return;
        }

        // LayoutTransform 参与布局，动画期间周围元素同步收缩；
        // 对齐方向决定内容向头部一侧收缩。
        _contentHost.HorizontalAlignment = ExpandDirection switch
        {
            ExpandDirection.Left => HorizontalAlignment.Right,
            ExpandDirection.Right => HorizontalAlignment.Left,
            _ => HorizontalAlignment.Stretch
        };

        _contentHost.VerticalAlignment = ExpandDirection switch
        {
            ExpandDirection.Up => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Top
        };

        if (_scaleTransform is null)
        {
            return;
        }

        _scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        _scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);

        SetScale(IsExpanded ? 1d : 0d);
        _contentHost.Visibility = IsExpanded ? Visibility.Visible : Visibility.Collapsed;
    }

    private bool IsVerticalDirection => ExpandDirection is ExpandDirection.Down or ExpandDirection.Up;

    private void SetScale(double value)
    {
        if (_scaleTransform is null)
        {
            return;
        }

        if (IsVerticalDirection)
        {
            _scaleTransform.ScaleY = value;
        }
        else
        {
            _scaleTransform.ScaleX = value;
        }
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (ExpanderPanel)d;
        bool isExpanded = (bool)e.NewValue;
        panel.ApplyExpansionState(isExpanded, animate: true);

        // 事件跟随状态变化触发，与 WPF Expander 一致；不依赖动画完成，
        // 避免 0 时长动画或模板尚未加载时事件丢失。
        panel.RaiseEvent(new RoutedEventArgs(isExpanded ? ExpandedEvent : CollapsedEvent, panel));

        ICommand? command = panel.ToggleCommand;
        object? parameter = panel.CommandParameter;
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }

        panel.OnIsExpandedChanged(isExpanded);
    }

    private static void OnExpandDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ExpanderPanel)d).ApplyDirection();
    }

    private static bool IsValidAnimationDuration(object value)
    {
        return value is Duration duration && duration.HasTimeSpan && duration.TimeSpan >= TimeSpan.Zero;
    }
}
