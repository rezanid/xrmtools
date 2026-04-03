#nullable enable
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
using XrmTools.Shell.Helpers;
using XrmTools.Shell.Styles;
using XrmTools.Shell.Controls;

namespace XrmTools.Shell;

[TemplatePart(Name = PART_ContentRoot, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_CopyButton, Type = typeof(Button))]
[TemplatePart(Name = PART_GripBorder, Type = typeof(Border))]
[TemplatePart(Name = PART_ResizeBorder, Type = typeof(Border))]
[TemplatePart(Name = PART_TitleBorder, Type = typeof(Border))]
public class Window : System.Windows.Window
{
    private const string PART_ContentRoot = "PART_ContentRoot";
    private const string PART_CopyButton = "PART_CopyButton";
    private const string PART_GripBorder = "PART_GripBorder";
    private const string PART_ResizeBorder = "PART_ResizeBorder";
    private const string PART_TitleBorder = "PART_TitleBorder";
    private FrameworkElement? contentRoot;
    private Button? copyButton;
    private Border? gripBorder;
    private Border? resizeBorder;
    private Border? titleBorder;
    public static readonly double ControlMinHeight = 200.0;
    public static readonly double ControlMinWidth = 320.0;
    public static readonly DependencyProperty BorderColorProperty = Property.RegisterFull<Window, Color>(nameof(BorderColor), propertyChanged: new PropertyChangedCallback(BorderColorChanged));
    public static readonly DependencyProperty CanCopyProperty = Property.Register<Window, bool>(nameof(CanCopy));
    public static readonly DependencyProperty CanDragMoveProperty = Property.Register<Window, bool>(nameof(CanDragMove));
    public static readonly DependencyProperty CloseEndsProcessProperty = Property.RegisterFull<Window, bool>(nameof(CloseEndsProcess));
    public static readonly DependencyProperty CloseOnEscProperty = Property.Register<Window, bool>(nameof(CloseOnEsc));
    private static readonly DependencyPropertyKey DpiPropertyKey = Property.RegisterReadOnly<Window, double>(nameof(Dpi));
    public static readonly DependencyProperty DpiProperty = DpiPropertyKey.DependencyProperty;
    public static readonly DependencyProperty FooterContentProperty = Property.RegisterFull<Window, object>(nameof(FooterContent));
    private static readonly DependencyPropertyKey HandlePropertyKey = Property.RegisterReadOnly<Window, IntPtr>(nameof(Handle));
    public static DependencyProperty HandleProperty = HandlePropertyKey.DependencyProperty;
    public static readonly DependencyProperty NonClientContentProperty = Property.RegisterFull<Window, object>(nameof(NonClientContent));
    private static readonly DependencyPropertyKey UseDwmBorderPropertyKey = Property.RegisterReadOnly<Window, bool>(nameof(UseDwmBorder));
    public static readonly DependencyProperty UseDwmBorderProperty = UseDwmBorderPropertyKey.DependencyProperty;
    public static readonly DependencyProperty ShowFooterProperty = Property.RegisterFull<Window, bool>(nameof(ShowFooter));
    public static readonly DependencyProperty ShowTitleProperty = Property.RegisterFull<Window, bool>(nameof(ShowTitle));

    static Window()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(typeof(Window)));
        CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(ApplicationCommands.Copy, new ExecutedRoutedEventHandler(ExecuteCopy), new CanExecuteRoutedEventHandler(CanExecuteCopy)));
        CloseCommand = new Command(window => true, window => window.Close());
        MaximizeCommand = new Command(window => window.ResizeMode == ResizeMode.CanResize || window.ResizeMode == ResizeMode.CanResizeWithGrip, window => window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal);
        MinimizeCommand = new Command(window => window.ResizeMode != 0, window => window.WindowState = WindowState.Minimized);
    }

    public static ICommand CloseCommand { get; }

    public static ICommand MaximizeCommand { get; }

    public static ICommand MinimizeCommand { get; }

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    private static void BorderColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window || window.Handle == IntPtr.Zero)
            return;
        window.UseDwmBorder = Interop.SetDwmBorderColor(window, (Color)e.NewValue);
    }

    public bool CanCopy
    {
        get => (bool)GetValue(CanCopyProperty);
        set => SetValue(CanCopyProperty, Boxes.Box(value));
    }

    public bool CanDragMove
    {
        get => (bool)GetValue(CanDragMoveProperty);
        set => SetValue(CanDragMoveProperty, Boxes.Box(value));
    }

    public bool CloseEndsProcess
    {
        get => (bool)GetValue(CloseEndsProcessProperty);
        set => SetValue(CloseEndsProcessProperty, Boxes.Box(value));
    }

    public bool CloseOnEsc
    {
        get => (bool)GetValue(CloseOnEscProperty);
        set => SetValue(CloseOnEscProperty, Boxes.Box(value));
    }

    public double Dpi
    {
        get => (double)GetValue(DpiProperty);
        private set => SetValue(DpiPropertyKey, Boxes.Box(value));
    }

    public object FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    public IntPtr Handle
    {
        get => (IntPtr)GetValue(HandleProperty);
        private set => SetValue(HandlePropertyKey, value);
    }

    public object NonClientContent
    {
        get => GetValue(NonClientContentProperty);
        set => SetValue(NonClientContentProperty, value);
    }

    public bool UseDwmBorder
    {
        get => (bool)GetValue(UseDwmBorderProperty);
        private set => SetValue(UseDwmBorderPropertyKey, Boxes.Box(value));
    }

    public bool ShowFooter
    {
        get => (bool)GetValue(ShowFooterProperty);
        set => SetValue(ShowFooterProperty, Boxes.Box(value));
    }

    public bool ShowTitle
    {
        get => (bool)GetValue(ShowTitleProperty);
        set => SetValue(ShowTitleProperty, Boxes.Box(value));
    }

    public void ShowAt(int screenX, int screenY, double logicalWidth = 0.0, double logicalHeight = 0.0)
    {
        Helpers.Windows.SetInitialBounds(this, screenX, screenY, logicalWidth, logicalHeight);
        Show();
    }

    public void ShowCentered()
    {
        Dispatcher.VerifyAccess();
        Helpers.Windows.CenterOnOwner(ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell, this, false);
        Show();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public new bool? ShowDialog() => throw new NotSupportedException();

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        contentRoot = (FrameworkElement)GetTemplateChild("PART_ContentRoot");
        copyButton = (Button)GetTemplateChild("PART_CopyButton");
        gripBorder = (Border)GetTemplateChild("PART_GripBorder");
        resizeBorder = (Border)GetTemplateChild("PART_ResizeBorder");
        titleBorder = (Border)GetTemplateChild("PART_TitleBorder");
    }

    internal bool? BaseShowDialog() => base.ShowDialog();

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        if (SizeToContent == SizeToContent.Manual)
            return base.ArrangeOverride(arrangeBounds);
        double height = AdjustValueForDpi(Sizes.ResizeGripSize, Dpi);
        Size size1 = new Size(2.0 * height, height);
        Size size2 = new Size(Math.Max(arrangeBounds.Width - size1.Width, 0.0), Math.Max(arrangeBounds.Height - size1.Height, 0.0));
        if (size2.Height > 0.0 && size2.Width > 0.0)
            contentRoot!.Arrange(new Rect(0.0, 0.0, size2.Width, size2.Height));
        return arrangeBounds;
    }

    protected virtual void CopyContentToClipboard()
    {
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (SizeToContent == SizeToContent.Manual)
            return base.MeasureOverride(availableSize);
        MaxHeight = AdjustValueForDpi(MaxHeight, Dpi);
        MaxWidth = AdjustValueForDpi(MaxWidth, Dpi);
        MinHeight = AdjustValueForDpi(MinHeight, Dpi);
        MinWidth = AdjustValueForDpi(MinWidth, Dpi);
        double height = AdjustValueForDpi(Sizes.ResizeGripSize, Dpi);
        Size windowSize = base.MeasureOverride(availableSize);
        Size nonClientSize = new Size(2.0 * height, height);
        Size size = ClientMeasure(windowSize, nonClientSize);
        windowSize.Width = SizeToContent != SizeToContent.Height || double.IsNaN(Width) ? size.Width + nonClientSize.Width : AdjustValueForDpi(Width, Dpi);
        windowSize.Height = SizeToContent != SizeToContent.Width || double.IsNaN(Height) ? size.Height + nonClientSize.Height : AdjustValueForDpi(Height, Dpi);
        windowSize.Height = Math.Max(Math.Min(windowSize.Height, MaxHeight), MinHeight);
        windowSize.Width = Math.Max(Math.Min(windowSize.Width, MaxWidth), MinWidth);
        return windowSize;

        Size ClientMeasure(Size windowSize, Size nonClientSize)
        {
            contentRoot!.Measure(new Size(Math.Max(windowSize.Width - nonClientSize.Width, 0.0), Math.Max(windowSize.Height - nonClientSize.Height, 0.0)));
            return contentRoot.DesiredSize;
        }
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        Interop.CloakWindow(Handle, false);
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        Dpi = newDpi.PixelsPerInchX;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (CloseOnEsc && e.Key == Key.Escape && e.KeyboardDevice.Modifiers == ModifierKeys.None)
        {
            e.Handled = true;
            Close();
        }
        else
            base.OnKeyDown(e);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (ShowTitle)
            return;
        if (!CanDragMove)
            return;
        try
        {
            DragMove();
        }
        catch (InvalidOperationException)
        {
        }
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.Property.Name)
        {
            case "CanCopy":
                CommandManager.InvalidateRequerySuggested();
                break;
            case "ResizeMode":
                ((Command)CloseCommand).RaiseCanExecuteChanged();
                ((Command)MaximizeCommand).RaiseCanExecuteChanged();
                ((Command)MinimizeCommand).RaiseCanExecuteChanged();
                break;
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        Handle = new WindowInteropHelper(this).Handle;
        UseDwmBorder = Interop.SetDwmBorderColor(this, BorderColor);
        Dpi = Handle.GetWindowDpi();
        Interop.CloakWindow(Handle, true);
        HwndSource.FromHwnd(Handle)?.AddHook(new HwndSourceHook(WndProc));
        base.OnSourceInitialized(e);
    }

    private static double AdjustValueForDpi(double value, double dpi)
    {
        if (double.IsInfinity(value))
            return value;
        if (value == 0.0)
            return 0.0;
        if (dpi == 0.0 || dpi == 96.0)
            return value;
        double num = dpi / 96.0;
        return (int)(value * num) / num;
    }

    private static void CanExecuteCopy(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Window window)
            e.CanExecute = window.CanCopy;
        else
            e.CanExecute = false;
    }

    private static void ExecuteCopy(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is not Window window)
            return;
        try
        {
            window.CopyContentToClipboard();
        }
        catch
        {
        }
        _ = new Callout
        {
            Content = "Copied",
            PlacementTarget = window.copyButton,
            PreferredPlacement = PlacementMode.Left,
            StaysOpen = false,
            IsOpen = true
        };
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        Dispatcher.VerifyAccess();
        var num = (uint)msg switch
        {
            2 => OnWmDestroy(),
            24 => OnWmShowWindow(wParam),
            131 => OnWmNcCalcSize(wParam, lParam, ref handled),
            132 => OnWmNcHitTest(lParam, ref handled),
            165 => OnWmNcRButtonUp(wParam, lParam, ref handled),
            _ => IntPtr.Zero,
        };
        return num;
    }

    private IntPtr OnWmDestroy()
    {
        HwndSource.FromHwnd(Handle)?.RemoveHook(new HwndSourceHook(WndProc));
        return IntPtr.Zero;
    }

    private IntPtr OnWmNcCalcSize(IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = true;
        RECT structure = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
        if ((bool)PInvoke.IsZoomed((HWND)Handle))
        {
            PInvoke.DefWindowProc((HWND)Handle, 131U, (WPARAM)wParam.ToUIntPtr(), (LPARAM)lParam);
            structure = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
            structure.top -= (int)Math.Ceiling(this.LogicalToDeviceUnitsY(SystemParameters.CaptionHeight)) + 1;
            Interop.GetTaskBarInfo(out var position, out var autoHide);
            if (autoHide)
            {
                switch (position)
                {
                    case Dock.Left:
                        ++structure.left;
                        break;
                    case Dock.Top:
                        ++structure.top;
                        break;
                    case Dock.Right:
                        --structure.right;
                        break;
                    case Dock.Bottom:
                        --structure.bottom;
                        break;
                }
            }
        }
        else
        {
            int deviceUnitsX = (int)this.LogicalToDeviceUnitsX(Sizes.ResizeGripSize);
            structure.left += deviceUnitsX;
            structure.right -= deviceUnitsX;
            structure.bottom -= deviceUnitsX;
        }
        Marshal.StructureToPtr(structure, lParam, true);
        return IntPtr.Zero;
    }

    private IntPtr OnWmNcHitTest(IntPtr lParam, ref bool handled)
    {
        if (PresentationSource.FromVisual(this)?.CompositionTarget == null)
            return IntPtr.Zero;
        double num = 2.0 * Sizes.ResizeGripSize;
        Point point = PointFromScreen(lParam.PointFromLParam());
        HitTestResult? result = null;
        VisualTreeHelper.HitTest(this, new HitTestFilterCallback(OnHitTestFilter), new HitTestResultCallback(OnHitTestResult), new PointHitTestParameters(point));
        if (result?.VisualHit == resizeBorder)
        {
            handled = true;
            if (point.X <= num)
                return new IntPtr(13L);
            return point.X >= resizeBorder!.ActualWidth - num ? new IntPtr(14L) : new IntPtr(12L);
        }
        if (result?.VisualHit == titleBorder)
        {
            handled = true;
            return new IntPtr(2L);
        }
        if (result?.VisualHit != gripBorder)
            return IntPtr.Zero;
        handled = true;
        return new IntPtr(17L);

        static HitTestFilterBehavior OnHitTestFilter(DependencyObject potentialHitTestTarget)
        {
            return potentialHitTestTarget is not FrameworkElement frameworkElement || !frameworkElement.IsHitTestVisible ? HitTestFilterBehavior.ContinueSkipSelfAndChildren : HitTestFilterBehavior.Continue;
        }

        HitTestResultBehavior OnHitTestResult(HitTestResult potentialResult)
        {
            result = potentialResult;
            return HitTestResultBehavior.Stop;
        }
    }

    private IntPtr OnWmNcRButtonUp(IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (wParam.ToInt64() != 2L)
            return IntPtr.Zero;
        handled = true;
        PInvoke.PostMessage((HWND)Handle, 787U, (WPARAM)UIntPtr.Zero, (LPARAM)lParam);
        return IntPtr.Zero;
    }

    private IntPtr OnWmShowWindow(IntPtr wParam)
    {
        Dispatcher.VerifyAccess();
        if (wParam.ToInt64() == 0L)
            return IntPtr.Zero;
        IVsUIShell? service = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
        if (string.IsNullOrEmpty(Title))
        {
            string pbstrAppName = "";
            int? appName = service?.GetAppName(out pbstrAppName);
            int ok = (int)HRESULT.S_OK;
            if (appName.GetValueOrDefault() == ok & appName.HasValue)
                Title = pbstrAppName;
        }
        return IntPtr.Zero;
    }

    private class Command : ICommand
    {
        public Command(Func<Window, bool> query, Action<Window> action)
        {
            Query = query;
            Action = action;
        }

        public event EventHandler? CanExecuteChanged;

        private Action<Window> Action { get; }

        private Func<Window, bool> Query { get; }

        public bool CanExecute(object parameter) => parameter is Window window && Query(window);

        public void Execute(object parameter)
        {
            if (parameter is not Window window)
                return;
            Action(window);
        }

        public void RaiseCanExecuteChanged()
        {
            EventHandler? canExecuteChanged = CanExecuteChanged;
            if (canExecuteChanged == null)
                return;
            canExecuteChanged(this, EventArgs.Empty);
        }
    }
}