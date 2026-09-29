#nullable enable
namespace XrmTools.Shell.Helpers;

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

/// <summary>System-only interop for Shell windows. Handles and message parameters remain pointer-sized.</summary>
internal static class WindowInterop
{
    internal const int WmNcCalcSize = 0x83;
    internal const int WmNcHitTest = 0x84;
    internal const int WmNcRButtonUp = 0xA5;
    internal const int HtCaption = 2;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MonitorInfo
    {
        public int Size;
        public Rect Monitor, Work;
        public uint Flags;
    }

    internal static Point PointFromLParam(IntPtr value)
    {
        long packed = value.ToInt64();
        // Coordinates are signed, including monitors to the left/above the primary display.
        return new Point((short)(packed & 0xffff), (short)((packed >> 16) & 0xffff));
    }

    internal static bool TryGetWorkArea(IntPtr handle, out Rect work)
    {
        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (GetMonitorInfo(MonitorFromWindow(handle, 2), ref info))
        {
            work = info.Work;
            return true;
        }
        work = default;
        return false;
    }

    internal static bool SetBorderColor(IntPtr handle, Color color)
    {
        int value = color.R | (color.G << 8) | (color.B << 16);
        // Unsupported attributes return a failure on older Windows; the template draws a fallback border.
        return DwmSetWindowAttribute(handle, 34 /* DWMWA_BORDER_COLOR */, ref value, sizeof(int)) >= 0;
    }

    internal static void ShowSystemMenu(System.Windows.Window window, Point screenPoint)
    {
        // WPF expects logical screen coordinates; Win32 messages and PointToScreen return device pixels.
        var target = PresentationSource.FromVisual(window)?.CompositionTarget;
        if (target != null)
            SystemCommands.ShowSystemMenu(window, target.TransformFromDevice.Transform(screenPoint));
    }

    internal static void RefreshFrame(IntPtr handle) =>
        SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 0x0037 /* FRAMECHANGED | NOACTIVATE | NOZORDER | NOMOVE | NOSIZE */);

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("user32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsZoomed(IntPtr hwnd);

    [DllImport("user32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetClientRect(IntPtr hwnd, out Rect rect);

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

    [DllImport("user32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int width, int height, uint flags);
}
