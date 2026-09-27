namespace XrmTools.PluginTrace;

using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

[Guid(WindowGuidString)]
internal sealed class TraceExplorerWindow : ToolWindowPane
{
    public const string WindowGuidString = "d7254f2c-17f9-4ab3-927e-cd09ca3f4bdf";

    public TraceExplorerWindow(TraceExplorerService service) : base(null)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        Caption = "Plugin Trace Explorer";
        BitmapImageMoniker = KnownMonikers.Log;
        var control = new TraceExplorerControl(service);
        Community.VisualStudio.Toolkit.Themes.SetUseVsTheme(control, true);
        Content = control;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Content is TraceExplorerControl control) control.Dispose();
        base.Dispose(disposing);
    }
}
