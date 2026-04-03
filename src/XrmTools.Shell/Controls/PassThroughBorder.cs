using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

#nullable enable
namespace XrmTools.Shell.Controls;


internal class PassThroughBorder : Border
{
    public PassThroughBorder() => Background = Brushes.Transparent;

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            return new PointHitTestResult(null, hitTestParameters.HitPoint);
        return Mouse.PrimaryDevice.RightButton == MouseButtonState.Pressed ? new PointHitTestResult(null, hitTestParameters.HitPoint) : base.HitTestCore(hitTestParameters);
    }
}