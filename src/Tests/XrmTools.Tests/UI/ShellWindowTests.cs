namespace XrmTools.Tests.UI;

using System;
using System.Windows;
using Xunit;
using XrmTools.Shell.Helpers;
using ShellWindow = Shell.Controls.Window;

public class ShellWindowTests
{
    [Theory]
    [InlineData(1, 1, 13)]
    [InlineData(399, 1, 14)]
    [InlineData(1, 299, 16)]
    [InlineData(399, 299, 17)]
    [InlineData(200, 1, 12)]
    [InlineData(200, 299, 15)]
    [InlineData(1, 150, 10)]
    [InlineData(399, 150, 11)]
    [InlineData(200, 150, 0)]
    [InlineData(-1, 150, 0)]
    [InlineData(400, 150, 0)]
    public void ResizeHitTest_IdentifiesEdgesAndLeavesContentAlone(double x, double y, int expected)
        => Assert.Equal(expected, ShellWindow.HitTestResizeBorder(new Point(x, y), 400, 300, true));

    [Fact]
    public void ResizeHitTest_DisabledForFixedOrMaximizedWindows()
        => Assert.Equal(0, ShellWindow.HitTestResizeBorder(new Point(1, 1), 400, 300, false));

    [Theory]
    [InlineData(-1920, -1080)]
    [InlineData(-1, 500)]
    [InlineData(1200, -1)]
    [InlineData(1920, 1080)]
    public void CaptionCoordinates_PreserveSignedMonitorPositions(int x, int y)
    {
        var packed = new IntPtr(unchecked((int)((uint)(ushort)x | ((uint)(ushort)y << 16))));
        Assert.Equal(new Point(x, y), WindowInterop.PointFromLParam(packed));
    }
}
