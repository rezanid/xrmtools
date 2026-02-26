namespace XrmTools.Shell.Controls;

using XrmTools.Shell.Helpers;
using System.Windows;
using System.Windows.Controls;

public class Indicator : Border
{
    public static readonly double ControlHeight = 16.0;
    public static readonly double ControlWidth = 3.0;
    public static readonly DependencyProperty OrientationProperty = Property.RegisterFull<Indicator, Orientation>(nameof(Orientation));

    static Indicator() => DefaultStyleKeyProperty.OverrideMetadata(typeof(Indicator), new FrameworkPropertyMetadata(typeof(Indicator)));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, Boxes.Box<Orientation>(value));
    }
}

