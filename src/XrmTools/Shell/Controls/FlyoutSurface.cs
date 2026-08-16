namespace XrmTools.Shell.Controls;

using System.Windows;
using System.Windows.Controls;
using XrmTools.Shell.Helpers;

public class FlyoutSurface : ContentControl
{
    public static readonly double ControlMaxWidth = 660.0;
    public static readonly double Offset = 2.0;
    public static readonly double OffsetInvert = -FlyoutSurface.Offset;
    public static readonly double ShadowSize = 12.0;
    public static readonly Thickness ShadowMargin = new Thickness(FlyoutSurface.ShadowSize, FlyoutSurface.Offset, FlyoutSurface.ShadowSize, FlyoutSurface.ShadowSize);
    public static readonly double HorizontalOffsetInvert = -FlyoutSurface.ShadowSize;
    public static readonly double VerticalOffsetInvert = -FlyoutSurface.Offset;

    static FlyoutSurface()
    {
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FlyoutSurface), (PropertyMetadata)new FrameworkPropertyMetadata((object)typeof(FlyoutSurface)));
    }

    protected override Size MeasureOverride(Size constraint)
    {
        VisualTree.InvalidateMeasureToAncestor(this.Content as FrameworkElement, (FrameworkElement)this);
        return base.MeasureOverride(constraint);
    }
}
