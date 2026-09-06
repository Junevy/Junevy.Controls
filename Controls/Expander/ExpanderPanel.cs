using System;
using System.Windows;
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

    /// <summary>展开方向：Down / Up / Left / Right。</summary>
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

    /// <summary>状态切换时执行的命令。</summary>
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

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _contentHost = GetTemplateChild(PartContentHost) as FrameworkElement;
        _scaleTransform = GetTemplateChild(PartScaleTransform) as ScaleTransform;

        ApplyDirection();
        ApplyExpansionState(IsExpanded, animate: false);
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

        _scaleTransform.BeginAnimation(scaleProperty, null);
        _scaleTransform.BeginAnimation(vertical
            ? ScaleTransform.ScaleXProperty
            : ScaleTransform.ScaleYProperty, null);

        if (!animate || AnimationDuration.TimeSpan <= TimeSpan.Zero)
        {
            SetScale(isExpanded ? 1d : 0d);
            _contentHost.Visibility = isExpanded ? Visibility.Visible : Visibility.Collapsed;
            return;
        }

        double from = isExpanded ? 0d : 1d;
        double to = isExpanded ? 1d : 0d;

        if (isExpanded)
        {
            _contentHost.Visibility = Visibility.Visible;
        }

        var animation = new DoubleAnimation(from, to, AnimationDuration.TimeSpan)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        animation.Completed += (_, _) =>
        {
            if (!isExpanded)
            {
                _contentHost.Visibility = Visibility.Collapsed;
            }

            RaiseEvent(new RoutedEventArgs(isExpanded ? ExpandedEvent : CollapsedEvent, this));
        };

        _scaleTransform.BeginAnimation(scaleProperty, animation);
    }

    private void ApplyDirection()
    {
        if (_contentHost is null || _scaleTransform is null)
        {
            return;
        }

        _scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        _scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);

        _contentHost.RenderTransformOrigin = ExpandDirection switch
        {
            ExpandDirection.Up => new Point(0.5, 1),
            ExpandDirection.Left => new Point(1, 0.5),
            ExpandDirection.Right => new Point(0, 0.5),
            _ => new Point(0.5, 0)
        };

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
        panel.ToggleCommand?.Execute(panel.CommandParameter);
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
