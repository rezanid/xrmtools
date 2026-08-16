namespace XrmTools.Shell.Controls;

using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

internal class PassThroughBorder : Border
{
    public PassThroughBorder() => this.Background = (Brush)Brushes.Transparent;

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            return (HitTestResult)new PointHitTestResult((Visual)null, hitTestParameters.HitPoint);
        return Mouse.PrimaryDevice.RightButton == MouseButtonState.Pressed ? (HitTestResult)new PointHitTestResult((Visual)null, hitTestParameters.HitPoint) : base.HitTestCore(hitTestParameters);
    }
}
