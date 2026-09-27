namespace XrmTools.PluginTrace;

using Community.VisualStudio.Toolkit;

internal sealed class TraceExplorerOptions : BaseOptionModel<TraceExplorerOptions>
{
    // Stored in the user's VS settings, independently of source-controlled project settings.
    public string SavedViewsJson { get; set; } = "[]";
}
