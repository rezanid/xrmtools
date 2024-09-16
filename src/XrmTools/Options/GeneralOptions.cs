#nullable enable
namespace XrmGen.Options;

using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using XrmGen.Configuration;

internal partial class OptionsProvider
{
    [ComVisible(true)]
    public class GeneralOptions : BaseOptionPage<Options.GeneralOptions> { }
}

public class GeneralOptions : BaseOptionModel<GeneralOptions>
{
    public event EventHandler? OptionsChanged;

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

    [Category("Power Platform Environments")]
    [DisplayName("Current Environment Level")]
    [Description("Current Environment is set at which level")]
    [DefaultValue(EnvironmentSettingLevel.Options)]
    [TypeConverter(typeof(EnumDescriptionConverter))]
    public EnvironmentSettingLevel EnvironmentSettingLevel { get; set; } = EnvironmentSettingLevel.Options;

    [Category("Power Platform Environments")]
    [DisplayName("Current Environment")]
    [Description("The current environment. only applicable when Current environment level is set to Visual Studio Profile.")]
    public DataverseEnvironment CurrentEnvironment { get; set; } = new DataverseEnvironment();

    VsOptionsConfigurationProvider? ConfigurationProvider { get; set; }

    public override void Save()
    {
        // Ensure that the list is not null
        Environments ??= [];

        // Remove any empty entries
        Environments.RemoveAll(e => string.IsNullOrWhiteSpace(e.Name) && string.IsNullOrWhiteSpace(e.Url) && string.IsNullOrWhiteSpace(e.ConnectionString));

        OptionsChanged?.Invoke(this, EventArgs.Empty);
        
        base.Save();
    }

    public override void Load()
    {
        base.Load();

        // Ensure that the list is not null
        Environments ??= [];
    }
}

public enum EnvironmentSettingLevel
{
    [Description("Visual Studio Options")]
    Options,
    Solution,
    Project,
    [Description("Solution (User)")]
    SolutionUser,
    [Description("Project (User)")]
    ProjectUser
}

[DisplayName("Power Platform Environment")]
[Description("Properties of a Power Platform environment.")]
[DefaultProperty(nameof(Name))]
[TypeConverter(typeof(ExpandableObjectConverter))]
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