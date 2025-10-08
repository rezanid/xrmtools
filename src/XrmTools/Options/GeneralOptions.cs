#nullable enable
namespace XrmTools.Options;
using Community.VisualStudio.Toolkit;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using XrmTools.Logging.Compatibility;

internal partial class OptionsProvider
{
    [ComVisible(true)]
    public class GeneralOptions : BaseOptionPage<Options.GeneralOptions> { }
}

internal class GeneralOptions : BaseOptionModel<GeneralOptions>
{
    [Category("Logging")]
    [DisplayName("Logging Level")]
    [Description("Setting the logging level to Trace will have performance implications.")]
    [DefaultValue(LogLevel.Information)]
    [TypeConverter(typeof(EnumConverter))]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    [Category("Power Platform Environments")]
    [DisplayName("Environments")]
    [Description("List of Power Platform environments.")]
    [DefaultValue(typeof(List<DataverseEnvironment>), "")]
    public DataverseEnvironmentList Environments { get; set; } = [];

    [Category("Power Platform Environments")]
    [DisplayName("Current Environment Scope")]
    [Description("Where to remember your selected environment")]
    [DefaultValue(SettingsStorageTypes.Options)]
    [TypeConverter(typeof(EnumDescriptionConverter))]
    public SettingsStorageTypes CurrentEnvironmentStorage { get; set; } = SettingsStorageTypes.Options;

    [Category("Power Platform Environments")]
    [DisplayName("Current Environment")]
    [Description("The current environment. only applicable when Current environment level is set to Visual Studio Options.")]
    [TypeConverter(typeof(CurrentEnvironmentConverter))]
    [Editor(typeof(CurrentEnvironmentEditor), typeof(UITypeEditor))]
    public DataverseEnvironment CurrentEnvironment { get; set; } = DataverseEnvironment.Empty;

    [Category("FetchXML Preview")]
    [DisplayName("Enable previewing FetchXML result")]
    [Description("Determines if the preview window should be shown.")]
    [DefaultValue(true)]
    public bool EnableFetchXmlPreviewWindow { get; set; } = true;

    [Category("FetchXML Preview")]
    [DisplayName("FetchXML preview location")]
    [Description("Determines if the preview window should be shown on the side or below the document.")]
    [DefaultValue(FetchXmlPreviewLocation.Vertical)]
    [TypeConverter(typeof(EnumDescriptionConverter))]
    public FetchXmlPreviewLocation PreviewWindowLocation { get; set; } = FetchXmlPreviewLocation.Vertical;

    [Category("FetchXML Preview")]
    [DisplayName("FetchXML preview window width")]
    [Description("The width in pixels of the preview window.")]
    [DefaultValue(500)]
    [Browsable(false)] // hidden
    public int FetchXmlPreviewWindowWidth { get; set; } = 500;

    [Category("FetchXML Preview")]
    [DisplayName("FetchXML preview window height")]
    [Description("The height in pixels of the preview window.")]
    [DefaultValue(300)]
    [Browsable(false)] // hidden
    public int FetchXmlPreviewWindowHeight { get; set; } = 300;

    [Category("Advanced")]
    [DisplayName("Proxy")]
    [Description("Use a proxy server for all communications with Power Platform. Requires restarting Visual Studio.")]
    [DefaultValue("")]
    public string Proxy { get; set; } = string.Empty;

    [Browsable(false)]
    public bool IsFirstRun { get; set; } = true;

    public override void Save()
    {
        // Ensure that the list is not null
        Environments ??= [];

        // Remove any empty entries
        Environments.RemoveAll(e => string.IsNullOrWhiteSpace(e.Name) && string.IsNullOrWhiteSpace(e.Url) && string.IsNullOrWhiteSpace(e.ConnectionString));

        base.Save();
    }

    public override void Load()
    {
        base.Load();

        // Ensure that the list is not null
        Environments ??= [];
    }
}

public enum FetchXmlPreviewLocation
{
    [Description("On the side")]
    Vertical,
    [Description("Below the document")]
    Horizontal
}
#nullable restore