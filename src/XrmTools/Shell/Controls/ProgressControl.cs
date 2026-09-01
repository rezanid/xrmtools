namespace XrmTools.Shell.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using XrmTools.Shell.Helpers;
using XrmTools.Shell.Styles;

[TemplatePart(Name = IndicatorPartName, Type = typeof(FrameworkElement))]
public class ProgressControl : Control
{
    private const string IndicatorPartName = "PART_Indicator";
    private static readonly Duration AnimationDuration = new(TimeSpan.FromSeconds(1.5));
    private FrameworkElement? indicator;

    public static readonly DependencyProperty IsRunningProperty =
        Property.Register<ProgressControl, bool>(nameof(IsRunning), propertyChanged: IsRunningChanged);

    public static readonly DependencyProperty KindProperty =
        Property.Register<ProgressControl, ProgressKind>(nameof(Kind));

    public static readonly DependencyProperty MessageProperty =
        Property.RegisterFull<ProgressControl, string>(nameof(Message));

    public static readonly DependencyProperty ProgressProperty =
        Property.RegisterFull<ProgressControl, double>(nameof(Progress), propertyChanged: ProgressChanged);

    public static readonly DependencyProperty RingDiameterProperty =
        Property.RegisterFull<ProgressControl, double>(nameof(RingDiameter));

    static ProgressControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ProgressControl),
            new FrameworkPropertyMetadata(typeof(ProgressControl)));
    }

    public ProgressControl()
    {
        Loaded += (_, _) => ToggleAnimation(IsRunning && IsVisible);
        Unloaded += (_, _) => ToggleAnimation(false);
    }

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, Boxes.Box(value));
    }

    public ProgressKind Kind
    {
        get => (ProgressKind)GetValue(KindProperty);
        set => SetValue(KindProperty, Boxes.Box(value));
    }

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, Boxes.Box(value));
    }

    public double RingDiameter
    {
        get => (double)GetValue(RingDiameterProperty);
        set => SetValue(RingDiameterProperty, Boxes.Box(value));
    }

    public override void OnApplyTemplate()
    {
        ToggleAnimation(false);
        base.OnApplyTemplate();
        indicator = GetTemplateChild(IndicatorPartName) as FrameworkElement;
        ToggleAnimation(IsRunning && IsVisible);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == IsVisibleProperty || e.Property == VisibilityProperty)
        {
            ToggleAnimation(IsRunning && IsVisible && Visibility == Visibility.Visible);
        }
    }

    private static void IsRunningChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is ProgressControl progressControl && progressControl.IsIndeterminate)
        {
            progressControl.ToggleAnimation((bool)e.NewValue && progressControl.IsVisible);
        }
    }

    private static void ProgressChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var progress = (double)e.NewValue;
        if (progress is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(Progress));
    }

    private bool IsIndeterminate => Kind is ProgressKind.BarIndeterminate or ProgressKind.RingIndeterminate;

    private void ToggleAnimation(bool isRunning)
    {
        if (indicator is null || !IsIndeterminate) return;

        if (!isRunning)
        {
            if (indicator.RenderTransform is TranslateTransform translate)
            {
                translate.BeginAnimation(TranslateTransform.XProperty, null);
            }
            else if (indicator.RenderTransform is RotateTransform rotate)
            {
                rotate.BeginAnimation(RotateTransform.AngleProperty, null);
            }
            indicator.RenderTransform = null;
            return;
        }

        if (indicator.RenderTransform is TranslateTransform or RotateTransform) return;

        if (Kind == ProgressKind.BarIndeterminate)
        {
            var transform = new TranslateTransform();
            var animation = new DoubleAnimation(-indicator.ActualWidth, ActualWidth, AnimationDuration)
            {
                RepeatBehavior = RepeatBehavior.Forever,
            };
            animation.Freeze();
            indicator.RenderTransform = transform;
            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
        else
        {
            var transform = new RotateTransform();
            var animation = new DoubleAnimation(0, 360, AnimationDuration)
            {
                RepeatBehavior = RepeatBehavior.Forever,
            };
            animation.Freeze();
            indicator.RenderTransform = transform;
            transform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }
    }
}
