using XrmTools.Shell.Helpers;
using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace XrmTools.Shell.Controls;

public class FlyoutSurface : ContentControl
{
    public static readonly double ControlMaxWidth = 660.0;
    public static readonly double Offset = 2.0;
    public static readonly double OffsetInvert = -Offset;
    public static readonly double ShadowSize = 12.0;
    public static readonly Thickness ShadowMargin = new(ShadowSize, Offset, ShadowSize, ShadowSize);
    public static readonly double HorizontalOffsetInvert = -ShadowSize;
    public static readonly double VerticalOffsetInvert = -Offset;

    static FlyoutSurface()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FlyoutSurface), new FrameworkPropertyMetadata(typeof(FlyoutSurface)));
    }

    protected override Size MeasureOverride(Size constraint)
    {
        VisualTree.InvalidateMeasureToAncestor(Content as FrameworkElement, this);
        return base.MeasureOverride(constraint);
    }
}