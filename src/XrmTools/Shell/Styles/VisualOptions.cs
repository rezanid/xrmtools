#nullable enable
namespace XrmTools.Shell.Styles;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

public class VisualOptions : ObservableObject, IVsShellPropertyEvents
{
    private const string AccessibilityRoot = "Control Panel\\Accessibility";
    private const string DynamicScrollBarsName = "DynamicScrollbars";
    private static readonly UIContext FeedbackEnabledContext = UIContext.FromUIContextGuid(new Guid("FC108F5B-6AB6-422E-BA84-2A33EFA464F3"));
    private static VisualOptions? instance;
    private Lazy<bool> areDynamicScrollBarsEnabled = new Lazy<bool>(GetDynamicScrollBars); private bool gradientsAllowed;
    private bool isFeedbackDisabledByPolicy;
    private double systemHorizontalScrollBarHeight;
    private double systemVerticalScrollBarWidth;

    //TODO: in Microsoft.VisualStudio.PlatformUI v18 chain to `base(false)`
    private VisualOptions()
      : base() //false)
    {
        Dispatcher.CurrentDispatcher.VerifyAccess();
        this.AdviseShellEvents();
        this.HookFeedbackUIContext();
        this.InitializeScrollBarValues();
    }

    public static VisualOptions Instance
    {
        get => VisualOptions.instance ?? (VisualOptions.instance = new VisualOptions());
    }

    public bool AreDynamicScrollBarsEnabled => this.areDynamicScrollBarsEnabled.Value;

    public bool GradientsAllowed
    {
        get => this.gradientsAllowed;
        private set
        {
            this.SetProperty<bool>(ref this.gradientsAllowed, value, nameof(GradientsAllowed));
        }
    }

    public bool IsFeedbackDisabledByPolicy
    {
        get => this.isFeedbackDisabledByPolicy;
        private set
        {
            this.SetProperty<bool>(ref this.isFeedbackDisabledByPolicy, value, nameof(IsFeedbackDisabledByPolicy));
        }
    }

    public double SystemHorizontalScrollBarHeight
    {
        get => this.systemHorizontalScrollBarHeight;
        private set
        {
            this.SetProperty<double>(ref this.systemHorizontalScrollBarHeight, value, nameof(SystemHorizontalScrollBarHeight));
        }
    }

    public double SystemVerticalScrollBarWidth
    {
        get => this.systemVerticalScrollBarWidth;
        private set
        {
            this.SetProperty<double>(ref this.systemVerticalScrollBarWidth, value, nameof(SystemVerticalScrollBarWidth));
        }
    }

    public BitmapScalingMode GetBitmapScalingMode(Visual visual)
    {
        if (visual == null)
            return BitmapScalingMode.Unspecified;
        double dpiX = visual.GetDpiX();
        return dpiX < 96.0 ? BitmapScalingMode.LowQuality : (dpiX % 96.0 != 0.0 ? BitmapScalingMode.HighQuality : BitmapScalingMode.NearestNeighbor);
    }

    private void AdviseShellEvents()
    {
        Dispatcher.CurrentDispatcher.VerifyAccess();
        if (!(ServiceProvider.GlobalProvider.GetService(typeof(SVsShell)) is IVsShell service))
            return;
        object pvar;
        if (ErrorHandler.Succeeded(service.GetProperty(-9061, out pvar)))
            this.UpdateGradientsAllowed((__VISUALEFFECTS)pvar);
        service.AdviseShellPropertyChanges((IVsShellPropertyEvents)this, out uint _);
    }

    int IVsShellPropertyEvents.OnShellPropertyChange(int propertyId, object value)
    {
        if (propertyId != -9061)
            return 0;
        this.UpdateGradientsAllowed((__VISUALEFFECTS)value);
        return 0;
    }

    private void UpdateGradientsAllowed(__VISUALEFFECTS value)
    {
        this.GradientsAllowed = (value & __VISUALEFFECTS.VISUALEFFECTS_Gradients) != 0;
    }

    private static bool GetDynamicScrollBars()
    {
        return VisualOptions.GetRegistryValueBool("Control Panel\\Accessibility", "DynamicScrollbars", true);
    }

    private static bool GetRegistryValueBool(string key, string name, bool fallbackValue)
    {
        using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(key, false))
            return registryKey?.GetValue(name) is int num ? num != 0 : fallbackValue;
    }

    private void InitializeScrollBarValues()
    {
        this.SystemHorizontalScrollBarHeight = this.ForceEvenNumber(SystemParameters.HorizontalScrollBarHeight);
        this.SystemVerticalScrollBarWidth = this.ForceEvenNumber(SystemParameters.VerticalScrollBarWidth);
        SystemParameters.StaticPropertyChanged += new PropertyChangedEventHandler(this.OnSystemParametersChanged);
    }

    private double ForceEvenNumber(double value) => value - Math.Floor(value) % 2.0;

    private void OnSystemParametersChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "HorizontalScrollBarHeight":
                this.SystemHorizontalScrollBarHeight = this.ForceEvenNumber(SystemParameters.HorizontalScrollBarHeight);
                break;
            case "VerticalScrollBarWidth":
                this.SystemVerticalScrollBarWidth = this.ForceEvenNumber(SystemParameters.VerticalScrollBarWidth);
                break;
        }
    }

    private void HookFeedbackUIContext()
    {
        Dispatcher.CurrentDispatcher.VerifyAccess();
        this.IsFeedbackDisabledByPolicy = !VisualOptions.FeedbackEnabledContext.IsActive;
        VisualOptions.FeedbackEnabledContext.UIContextChanged += (EventHandler<UIContextChangedEventArgs>)((s, e) => this.IsFeedbackDisabledByPolicy = !e.Activated);
    }
}