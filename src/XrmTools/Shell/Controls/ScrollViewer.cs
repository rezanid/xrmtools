#nullable enable
namespace XrmTools.Shell.Controls;

using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

public class ScrollViewer : System.Windows.Controls.ScrollViewer
{
    const int WM_MOUSEHWHEEL = 0x020E;
    public static readonly double ScrollBarReservedSize = 10.0;

    static ScrollViewer() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollViewer), new FrameworkPropertyMetadata(typeof(ScrollViewer)));

    public ScrollViewer()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        var hwndSource = PresentationSource.FromVisual(this);
        (hwndSource as HwndSource)?.RemoveHook(Hook);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var hwndSource = PresentationSource.FromVisual(this);
        (hwndSource as HwndSource)?.AddHook(Hook);
    }

    private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_MOUSEHWHEEL:
                int tilt = (short)HIWORD(wParam);
                OnMouseTilt(tilt);
                return (IntPtr)1;
        }
        return IntPtr.Zero;
    }

    private static ushort LOWORD(IntPtr ptr) => (ushort)((ulong)ptr.ToInt64() & 0xFFFF);

    private static ushort HIWORD(IntPtr ptr) => (ushort)(((ulong)ptr.ToInt64() >> 16) & 0xFFFF);

    private void OnMouseTilt(int tilt)
    {
        if (!IsVisible || tilt == 0 || !IsMouseOver) return;

        if (tilt > 0)
        {
            ScrollInfo.MouseWheelLeft();
            //LineLeft();
        }
        else
        {
            ScrollInfo.MouseWheelRight();
            //LineRight();
        }
    }

    /*protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (ScrollInfo == null || TemplatedParent is TextBoxBase)
            return;
        bool flag = Keyboard.PrimaryDevice.Modifiers == ModifierKeys.Shift;
        if (e.Delta < 0)
        {
            if (flag && ScrollInfo.CanHorizontallyScroll)
                ScrollInfo.MouseWheelRight();
            else if (ScrollInfo.CanVerticallyScroll)
                ScrollInfo.MouseWheelDown();
        }
        else if (e.Delta > 0)
        {
            if (flag && ScrollInfo.CanHorizontallyScroll)
                ScrollInfo.MouseWheelLeft();
            else if (ScrollInfo.CanVerticallyScroll)
                ScrollInfo.MouseWheelUp();
        }
        e.Handled = true;
    }*/
}
#nullable restore