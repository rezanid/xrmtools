#nullable enable
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace XrmTools.Shell.Helpers;

internal static class Windows
{
    public static void CenterOnOwner(IVsUIShell? uiShell, XrmTools.Shell.Window window, bool assignOwner = true)
    {
        window.Dispatcher.VerifyAccess();
        WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
        IntPtr ownerHwnd = windowInteropHelper.Owner;
        int num1;
        if (ownerHwnd == IntPtr.Zero)
        {
            int? dialogOwnerHwnd = uiShell?.GetDialogOwnerHwnd(out ownerHwnd);
            int ok = (int)HRESULT.S_OK;
            num1 = dialogOwnerHwnd.GetValueOrDefault() == ok & dialogOwnerHwnd.HasValue ? 1 : 0;
        }
        else
            num1 = 0;
        int num2 = assignOwner ? 1 : 0;
        if ((num1 & num2) != 0)
            windowInteropHelper.Owner = ownerHwnd;
        if (ownerHwnd == IntPtr.Zero)
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        else if (window.Handle == IntPtr.Zero)
            window.SourceInitialized += new EventHandler(OnSourceInitialized);
        else if (!window.IsLoaded)
            window.Loaded += new RoutedEventHandler(OnWindowLoaded);
        else
            SetWindowBounds();

        void OnSourceInitialized(object sender, EventArgs e)
        {
            window.SourceInitialized -= new EventHandler(OnSourceInitialized);
            SetWindowBounds();
        }

        void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            window.Loaded -= new RoutedEventHandler(OnWindowLoaded);
            SetWindowBounds();
        }

        void SetWindowBounds()
        {
            if (!(bool)PInvoke.GetWindowRect((HWND)ownerHwnd, out var lpRect1) || !(bool)PInvoke.GetWindowRect((HWND)window.Handle, out var lpRect2))
            {
                window.Top = 0.0;
                window.Left = 0.0;
            }
            else
                Windows.SetWindowBounds(window, (int)Math.Round((double)lpRect1.left + (double)(lpRect1.Width - lpRect2.Width) / 2.0), (int)Math.Round((double)lpRect1.top + (double)(lpRect1.Height - lpRect2.Height) / 2.0));
        }
    }

    public static void SetInitialBounds(
      Window window,
      int screenX,
      int screenY,
      double logicalWidth = 0.0,
      double logicalHeight = 0.0)
    {
        if (window.Handle == IntPtr.Zero)
            window.SourceInitialized += new EventHandler(OnSourceInitialized);
        else if (!window.IsLoaded)
            window.Loaded += new RoutedEventHandler(OnWindowLoaded);
        else
            SetWindowBounds(window, screenX, screenY, logicalWidth, logicalHeight);

        void OnSourceInitialized(object sender, EventArgs e)
        {
            window.SourceInitialized -= new EventHandler(OnSourceInitialized);
            SetWindowBounds(window, screenX, screenY, logicalWidth, logicalHeight);
        }

        void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            window.Loaded -= new RoutedEventHandler(OnWindowLoaded);
            SetWindowBounds(window, screenX, screenY, logicalWidth, logicalHeight);
        }
    }

    private static void SetWindowBounds(
      Window window,
      int screenX,
      int screenY,
      double logicalWidth = 0.0,
      double logicalHeight = 0.0)
    {
        if (logicalWidth <= 0.0)
            logicalWidth = window.Width;
        if (logicalHeight <= 0.0)
            logicalHeight = window.Height;
        PInvoke.SetWindowPos((HWND)window.Handle, (HWND)IntPtr.Zero, screenX, screenY, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER);
        int cx = (int)(window.Handle.GetWindowDpiScale() * Math.Max(window.MinWidth, logicalWidth));
        int cy = (int)(window.Handle.GetWindowDpiScale() * Math.Max(window.MinHeight, logicalHeight));
        PInvoke.SetWindowPos((HWND)window.Handle, (HWND)IntPtr.Zero, 0, 0, cx, cy, SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER);
    }
}