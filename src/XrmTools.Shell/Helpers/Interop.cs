#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
//using Windows.Win32.UI.Shell;
//using Windows.Win32.Graphics.Dwm;
//using Windows.Win32.UI.Shell;

namespace XrmTools.Shell.Helpers;

internal static class Interop
{
    private static bool? CanDwmColorBorder { get; set; }

    public static unsafe void CloakWindow(IntPtr hwnd, bool cloak)
    {
        PInvoke.DwmSetWindowAttribute((HWND)hwnd, global::Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE.DWMWA_CLOAK, &cloak, 4U);
    }

    public static unsafe void GetTaskBarInfo(out Dock position, out bool autoHide)
    {
        global::Windows.Win32.UI.Shell.APPBARDATA appbardata1 = new global::Windows.Win32.UI.Shell.APPBARDATA
{
    cbSize = (uint)Marshal.SizeOf(typeof(global::Windows.Win32.UI.Shell.APPBARDATA))
};
        global::Windows.Win32.UI.Shell.APPBARDATA appbardata2 = new global::Windows.Win32.UI.Shell.APPBARDATA
        {
            cbSize = (uint)Marshal.SizeOf(typeof(global::Windows.Win32.UI.Shell.APPBARDATA))
        };
        autoHide = false;
        position = Dock.Bottom;
        if (PInvoke.SHAppBarMessage(4U, &appbardata1) == 1UL)
            autoHide = true;
        if (PInvoke.SHAppBarMessage(5U, &appbardata2) <= 0UL)
            return;
        position = (Dock)appbardata2.uEdge;
    }

    public static Point PointFromLParam(this IntPtr lParam)
    {
        long int64 = lParam.ToInt64();
        return new Point((short)(int64 & ushort.MaxValue), (short)(int64 >> 16 /*0x10*/ & ushort.MaxValue));
    }

    public static IntPtr PointToLParam(this Point point)
    {
        return new IntPtr(0 + (int)point.X + ((int)point.Y << 16 /*0x10*/));
    }

    public static unsafe bool SetDwmBorderColor(Window window, Color color)
    {
        bool? canDwmColorBorder = CanDwmColorBorder;
        bool flag = false;
        if (canDwmColorBorder.GetValueOrDefault() == flag & canDwmColorBorder.HasValue)
            return false;
        COLORREF colorref = new COLORREF((uint)((int)color.B << 16 /*0x10*/ | (int)color.G << 8) | (uint)color.R);
        HRESULT hresult = PInvoke.DwmSetWindowAttribute((HWND)window.Handle, global::Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE.DWMWA_BORDER_COLOR, &colorref, 4U);
        if (!CanDwmColorBorder.HasValue)
            Interop.CanDwmColorBorder = new bool?(hresult != HRESULT.E_INVALIDARG);
        return hresult == HRESULT.S_OK;
    }

    public static unsafe UIntPtr ToUIntPtr(this IntPtr pointer) => new(pointer.ToPointer());
}