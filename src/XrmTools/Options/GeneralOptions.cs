#nullable enable
namespace XrmGen.Options;

using Community.VisualStudio.Toolkit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

internal partial class OptionsProvider
{
    [ComVisible(true)]
    public class GeneralOptions : BaseOptionPage<Options.GeneralOptions> { }
}

public class GeneralOptions : BaseOptionModel<GeneralOptions>
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
    public List<DataverseEnvironment> Environments { get; set; } = [];
}

[DisplayName("Power Platform Environment")]
[Description("Properties of a Power Platform environment.")]
public class DataverseEnvironment
{
    [DisplayName("Environment Name")]
    [Description("The name of the environment, so you can easily identify it.")]
    [DefaultValue("Contoso Dev")]
    public string? Name { get; set; }

    [DisplayName("Environment URL")]
    [Description("The URL of the environment.")]
    [DefaultValue("https://contoso.crm.dynamics.com")]
    public string? Url { get; set; }

    [DisplayName("Connection String")]
    [Description("The connection string to the environment according to https://learn.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect.")]
    [DefaultValue("AuthType=OAuth;Url=https://contoso.crm.dynamics.com;Integrated Security=True")]
    public string? ConnectionString { get; set; }
}
#nullable restore