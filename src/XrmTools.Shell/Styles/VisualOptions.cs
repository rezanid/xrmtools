#nullable enable
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;
using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace XrmTools.Shell.Styles;

public class VisualOptions : ObservableObject, IVsShellPropertyEvents
{
    private const string PolicyRoot = "Software\\Policies\\Microsoft\\VisualStudio\\Feedback";
    private const string FeedbackDisabledPolicy = "ProductFeedbackDisabled";
    private static VisualOptions? instance;
    private bool gradientsAllowed;
    private readonly Lazy<bool> isFeedbackDisabledByPolicy = new(GetFeedbackPolicy);

    private VisualOptions() // base(false) is not called because it is not available in the version of VisualStudio.Utilities used in this project.
    { 
        Dispatcher.CurrentDispatcher.VerifyAccess();
        if (ServiceProvider.GlobalProvider.GetService(typeof(SVsShell)) is not IVsShell service)
            return;
        if (ErrorHandler.Succeeded(service.GetProperty(-9061, out var pvar)))
            UpdateGradientsAllowed((__VISUALEFFECTS)pvar);
        service.AdviseShellPropertyChanges(this, out uint _);
    }

    public static VisualOptions Instance
    {
        get => instance ??= new VisualOptions();
    }

    public bool GradientsAllowed
    {
        get => gradientsAllowed;
        private set
        {
            SetProperty(ref gradientsAllowed, value, nameof(GradientsAllowed));
        }
    }

    public bool IsFeedbackDisabledByPolicy => isFeedbackDisabledByPolicy.Value;

    public BitmapScalingMode GetBitmapScalingMode(Visual visual)
    {
        if (visual == null)
            return BitmapScalingMode.Unspecified;
        double dpiX = visual.GetDpiX();
        return dpiX < 96.0 ? BitmapScalingMode.LowQuality : (dpiX % 96.0 != 0.0 ? BitmapScalingMode.HighQuality : BitmapScalingMode.NearestNeighbor);
    }

    private static bool GetFeedbackPolicy()
    {
        using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Policies\\Microsoft\\VisualStudio\\Feedback", false);
        return registryKey?.GetValue("ProductFeedbackDisabled") is int num && num != 0;
    }

    int IVsShellPropertyEvents.OnShellPropertyChange(int propertyId, object value)
    {
        if (propertyId != -9061)
            return 0;
        UpdateGradientsAllowed((__VISUALEFFECTS)value);
        return 0;
    }

    private void UpdateGradientsAllowed(__VISUALEFFECTS value)
    {
        GradientsAllowed = (value & __VISUALEFFECTS.VISUALEFFECTS_Gradients) != 0;
    }
}