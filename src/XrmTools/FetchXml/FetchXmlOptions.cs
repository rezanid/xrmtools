#nullable enable
namespace XrmTools.Options;

using Community.VisualStudio.Toolkit;
using System.ComponentModel;
using System.Runtime.InteropServices;

internal partial class OptionsProvider
{
    [ComVisible(true)]
    public class FetchXmlOptions : BaseOptionPage<Options.FetchXmlOptions> { }
}

internal class FetchXmlOptions : BaseOptionModel<FetchXmlOptions>
{
    [Category("FetchXML Preview")]
    [DisplayName("Enable FetchXML preview")]
    [Description("Determines if the preview window should be shown.")]
    [DefaultValue(true)]
    public bool EnableFetchXmlPreviewWindow { get; set; } = true;

    [Category("FetchXML Preview")]
    [DisplayName("Preview location")]
    [Description("Determines if the preview window should be shown on the side or below the document.")]
    [DefaultValue(FetchXmlPreviewLocation.Vertical)]
    [TypeConverter(typeof(EnumDescriptionConverter))]
    public FetchXmlPreviewLocation PreviewWindowLocation { get; set; } = FetchXmlPreviewLocation.Vertical;

    [Category("FetchXML Preview")]
    [DisplayName("Query execution mode")]
    [Description("Determines when the FetchXML query should be executed to update the preview window.")]
    [DefaultValue(FetchXmlExecutionMode.OnChange)]
    [TypeConverter(typeof(EnumDescriptionConverter))]
    public FetchXmlExecutionMode FetchXmlExecution { get; set; } = FetchXmlExecutionMode.OnChange;

    [Category("FetchXML Preview")]
    [DisplayName("Run query when document opens")]
    [Description("Run the query every time a FetchXML document is opened.")]
    [DefaultValue(false)]
    public bool RunQueryOnDocumentOpen { get; set; } = false;

    [Category("FetchXML Preview")]
    [DisplayName("Preview window width")]
    [Description("The width in pixels of the preview window.")]
    [DefaultValue(500)]
    [Browsable(false)] // hidden
    public int FetchXmlPreviewWindowWidth { get; set; } = 500;

    [Category("FetchXML Preview")]
    [DisplayName("Preview window height")]
    [Description("The height in pixels of the preview window.")]
    [DefaultValue(300)]
    [Browsable(false)] // hidden
    public int FetchXmlPreviewWindowHeight { get; set; } = 300;
}

public enum FetchXmlPreviewLocation
{
    [Description("On the side")]
    Vertical,
    [Description("Below the document")]
    Horizontal
}

public enum FetchXmlExecutionMode
{
    [Description("Manual (by pressing Execute)")]
    Manual,
    [Description("When query is saved")]
    OnSave,
    [Description("When query changes")]
    OnChange
}
#nullable restore
