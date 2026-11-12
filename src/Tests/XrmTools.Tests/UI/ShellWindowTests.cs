namespace XrmTools.Tests.UI;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
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
    [InlineData(false)]
    [InlineData(true)]
    public Task FooterProgress_ReplacesSeparatorWithoutChangingLayout_AndRemainsOptIn(bool hasFooter)
    {
        var completion = new TaskCompletionSource<bool>();
        var thread = new Thread(() =>
        {
            try
            {
                var button = new Button { Content = "Close" };
                var window = new Shell.Controls.DialogWindow
                {
                    Width = 500, SizeToContent = SizeToContent.Height,
                    Content = new TextBlock { Text = "Content" }, FooterContent = hasFooter ? button : null
                };
                try
                {
                    window.Show();
                    Flush();
                    var progress = (Shell.Controls.ProgressControl)window.Template.FindName("FooterProgress", window);
                    var border = (Border)window.Template.FindName("FooterBorder", window);
                    Assert.False(window.IsFooterProgressRunning);
                    Assert.False(progress.IsVisible);
                    var height = window.ActualHeight;
                    var brush = border.BorderBrush;
                    var buttonPosition = hasFooter ? button.TranslatePoint(new Point(), window) : new Point();
                    window.IsFooterProgressRunning = true;
                    Flush();
                    Assert.Equal(hasFooter, progress.IsVisible);
                    Assert.Equal(height, window.ActualHeight);
                    if (hasFooter)
                    {
                        Assert.Same(Brushes.Transparent, border.BorderBrush);
                        Assert.Equal(buttonPosition, button.TranslatePoint(new Point(), window));
                        Assert.Equal(border.ActualWidth, progress.ActualWidth);
                        Assert.Equal(3, progress.ActualHeight);
                    }
                    window.IsFooterProgressRunning = false;
                    Flush();
                    Assert.False(progress.IsVisible);
                    Assert.False(progress.IsRunning);
                    Assert.Same(brush, border.BorderBrush);
                    Assert.Equal(height, window.ActualHeight);
                    if (hasFooter) Assert.Equal(buttonPosition, button.TranslatePoint(new Point(), window));
                }
                finally { window.Close(); }
                completion.SetResult(true);
            }
            catch (Exception ex) { completion.SetException(ex); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;

        static void Flush() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
    }

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
