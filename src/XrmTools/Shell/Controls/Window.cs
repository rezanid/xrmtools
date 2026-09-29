#nullable enable
namespace XrmTools.Shell.Controls;

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using XrmTools.Shell.Helpers;
using XrmTools.Shell.Styles;
using Microsoft.VisualStudio.Shell;

/// <summary>
/// A themed WPF frame following the Shell.Styles window pattern: a WPF caption and
/// native non-client hit testing, without a dependency on VS's private UI assemblies.
/// </summary>
[TemplatePart(Name = "PART_TitleBorder", Type = typeof(System.Windows.Controls.Border))]
public class Window : System.Windows.Window
{
    private FrameworkElement? titleBorder;
    private HwndSource? source;

    public static readonly DependencyProperty FooterContentProperty = Property.RegisterFull<Window, object>(nameof(FooterContent));
    public static readonly DependencyProperty ShowMinimizeButtonProperty = Property.Register<Window, bool>(nameof(ShowMinimizeButton), true);
    public static readonly DependencyProperty ShowMaximizeButtonProperty = Property.Register<Window, bool>(nameof(ShowMaximizeButton), true);
    public static readonly DependencyProperty IsCloseButtonEnabledProperty = Property.Register<Window, bool>(nameof(IsCloseButtonEnabled), true, propertyChanged: OnCommandStateChanged);
    public static readonly DependencyProperty CloseOnEscProperty = Property.Register<Window, bool>(nameof(CloseOnEsc));
    public static readonly DependencyProperty BorderColorProperty = Property.Register<Window, Color>(nameof(BorderColor), propertyChanged: OnBorderColorChanged);
    private static readonly DependencyPropertyKey UseDwmBorderPropertyKey = Property.RegisterReadOnly<Window, bool>(nameof(UseDwmBorder));
    public static readonly DependencyProperty UseDwmBorderProperty = UseDwmBorderPropertyKey.DependencyProperty;

    static Window()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(typeof(Window)));
        CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(SystemCommands.CloseWindowCommand,
            (s, e) => ((Window)s).Close(), (s, e) => e.CanExecute = ((Window)s).IsCloseButtonEnabled));
        CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(SystemCommands.MinimizeWindowCommand,
            (s, e) => SystemCommands.MinimizeWindow((Window)s), (s, e) => e.CanExecute = ((Window)s).ResizeMode != ResizeMode.NoResize));
        CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(SystemCommands.MaximizeWindowCommand,
            (s, e) => SystemCommands.MaximizeWindow((Window)s), (s, e) => e.CanExecute = ((Window)s).CanResize));
        CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(SystemCommands.RestoreWindowCommand,
            (s, e) => SystemCommands.RestoreWindow((Window)s), (s, e) => e.CanExecute = ((Window)s).CanResize));
    }

    public object FooterContent { get => GetValue(FooterContentProperty); set => SetValue(FooterContentProperty, value); }
    public bool ShowMinimizeButton { get => (bool)GetValue(ShowMinimizeButtonProperty); set => SetValue(ShowMinimizeButtonProperty, value); }
    public bool ShowMaximizeButton { get => (bool)GetValue(ShowMaximizeButtonProperty); set => SetValue(ShowMaximizeButtonProperty, value); }
    public bool IsCloseButtonEnabled { get => (bool)GetValue(IsCloseButtonEnabledProperty); set => SetValue(IsCloseButtonEnabledProperty, value); }
    public bool CloseOnEsc { get => (bool)GetValue(CloseOnEscProperty); set => SetValue(CloseOnEscProperty, value); }
    public Color BorderColor { get => (Color)GetValue(BorderColorProperty); set => SetValue(BorderColorProperty, value); }
    public bool UseDwmBorder => (bool)GetValue(UseDwmBorderProperty);
    private bool CanResize => ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        titleBorder = GetTemplateChild("PART_TitleBorder") as FrameworkElement;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        source?.AddHook(WndProc);
        UpdateFrameTheme();
        UpdateBorderColor();
        if (source != null)
            WindowInterop.RefreshFrame(source.Handle);
    }

    protected override void OnClosed(EventArgs e)
    {
        source?.RemoveHook(WndProc);
        source = null;
        base.OnClosed(e);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        if (!IsCloseButtonEnabled)
            e.Cancel = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled && CloseOnEsc && IsCloseButtonEnabled && e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
        {
            Close();
            e.Handled = true;
        }
    }

    private static void OnCommandStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => CommandManager.InvalidateRequerySuggested();
    private static void OnBorderColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Window)d).UpdateBorderColor();

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        UpdateFrameTheme();
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);
        UpdateFrameTheme();
    }

    private void UpdateFrameTheme()
    {
        // These are the theme resources used by Shell.Styles, not a dependency on its private assembly.
        var category = new Guid("5af241b7-5627-4d12-bfb1-2b67d11127d7");
        string name = IsActive ? "EnvironmentBorder" : "EnvironmentBorderInactive";
        var colorKey = new ThemeResourceKey(category, name, ThemeResourceKeyType.BackgroundColor);
        var brushKey = new ThemeResourceKey(category, name, ThemeResourceKeyType.BackgroundBrush);
        SetResourceReference(BorderColorProperty, TryFindResource(colorKey) != null ? colorKey :
            IsActive ? ShellColors.AccentFillDefaultColorKey : ShellColors.ControlStrokeDefaultColorKey);
        SetResourceReference(BorderBrushProperty, TryFindResource(brushKey) != null ? brushKey :
            IsActive ? ShellColors.AccentFillDefaultBrushKey : ShellColors.ControlStrokeDefaultBrushKey);
    }

    private void UpdateBorderColor()
    {
        if (source != null)
            SetValue(UseDwmBorderPropertyKey, WindowInterop.SetBorderColor(source.Handle, BorderColor));
    }

    private IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (message)
        {
            case WindowInterop.WmNcCalcSize:
                // Shell.Styles retains the native sides/bottom. Removing them also removes
                // the DWM outline, shadow and automatic rounded corners on Windows 11.
                var rect = Marshal.PtrToStructure<WindowInterop.Rect>(lParam);
                if (WindowInterop.IsZoomed(hwnd) && WindowInterop.TryGetWorkArea(hwnd, out var work))
                    rect = work;
                else
                {
                    int grip = (int)(Sizes.ResizeGripSize * VisualTreeHelper.GetDpi(this).DpiScaleX);
                    rect.Left += grip;
                    rect.Right -= grip;
                    rect.Bottom -= grip;
                }
                Marshal.StructureToPtr(rect, lParam, false);
                handled = true;
                return IntPtr.Zero;
            case WindowInterop.WmNcHitTest:
                if (source?.CompositionTarget == null)
                    break;
                Point point = PointFromScreen(WindowInterop.PointFromLParam(lParam));
                if (!WindowInterop.GetClientRect(hwnd, out var client))
                    break;
                Point clientSize = source.CompositionTarget.TransformFromDevice.Transform(new Point(client.Right, client.Bottom));
                // Let Windows handle the retained non-client resize edges.
                if (point.X < 0 || point.Y < 0 || point.X >= clientSize.X || point.Y >= clientSize.Y)
                    break;
                int hit = HitTestResizeBorder(point, clientSize.X, clientSize.Y, CanResize && WindowState == WindowState.Normal);
                if (hit != 0)
                {
                    handled = true;
                    return new IntPtr(hit);
                }
                if (titleBorder != null && titleBorder.IsVisible)
                {
                    var captionPoint = TranslatePoint(point, titleBorder);
                    if (captionPoint.X >= 0 && captionPoint.Y >= 0 && captionPoint.X < titleBorder.ActualWidth && captionPoint.Y < titleBorder.ActualHeight)
                    {
                        handled = true;
                        return new IntPtr(WindowInterop.HtCaption);
                    }
                }
                // Explicit HTCLIENT prevents the default frame from treating WPF caption buttons as native buttons.
                handled = true;
                return new IntPtr(1);
            case WindowInterop.WmNcRButtonUp when wParam.ToInt64() == WindowInterop.HtCaption:
                WindowInterop.ShowSystemMenu(this, WindowInterop.PointFromLParam(lParam));
                handled = true;
                break;
        }
        return IntPtr.Zero;
    }

    internal static int HitTestResizeBorder(Point point, double width, double height, bool canResize)
    {
        if (!canResize || point.X < 0 || point.Y < 0 || point.X >= width || point.Y >= height)
            return 0;
        const double grip = 6; // WPF logical units; PointFromScreen accounts for the current monitor's DPI.
        bool left = point.X < grip, right = point.X >= width - grip;
        bool top = point.Y < grip, bottom = point.Y >= height - grip;
        if (top) return left ? 13 : right ? 14 : 12;
        if (bottom) return left ? 16 : right ? 17 : 15;
        return left ? 10 : right ? 11 : 0;
    }
}
