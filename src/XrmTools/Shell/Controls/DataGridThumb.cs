namespace XrmTools.Shell.Controls;

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

internal class DataGridThumb : Thumb
{
    private Point originalDragPoint;
    private Point previousDragPoint;

    static DataGridThumb()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridThumb), new FrameworkPropertyMetadata(typeof(DataGridThumb)));
    }

    public bool IsSingleActionDragging { get; private set; }

    public void StartSingleActionDrag()
    {
        IsSingleActionDragging = true;
        originalDragPoint = PointToScreen(new Point(ActualWidth / 2.0, ActualHeight / 2.0));
        previousDragPoint = originalDragPoint;
        SetCursorPos((int)originalDragPoint.X, (int)originalDragPoint.Y);
        CaptureMouse();
        RaiseEvent(new DragStartedEventArgs(originalDragPoint.X, originalDragPoint.Y)
        {
            RoutedEvent = DragStartedEvent,
            Source = this,
        });
    }

    public void EndSingleActionDrag(bool canceled)
    {
        ReleaseMouseCapture();
        IsSingleActionDragging = false;
        var screen = PointToScreen(Mouse.GetPosition(this));
        RaiseEvent(new DragCompletedEventArgs(screen.X - previousDragPoint.X, screen.Y - previousDragPoint.Y, canceled)
        {
            RoutedEvent = DragCompletedEvent,
            Source = this,
        });
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (IsSingleActionDragging) EndSingleActionDrag(false);
        else base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!IsSingleActionDragging)
        {
            base.OnMouseMove(e);
            return;
        }

        var screen = PointToScreen(e.GetPosition(this));
        if (screen == previousDragPoint) return;
        e.Handled = true;
        RaiseEvent(new DragDeltaEventArgs(screen.X - previousDragPoint.X, screen.Y - previousDragPoint.Y)
        {
            RoutedEvent = DragDeltaEvent,
            Source = this,
        });
        previousDragPoint = screen;
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);
}
