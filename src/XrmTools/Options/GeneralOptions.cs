#nullable enable
namespace XrmTools.Options;

using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using XrmTools.Logging;

internal partial class OptionsProvider
{
    [ComVisible(true)]
    public class GeneralOptions : BaseOptionPage<XrmTools.Options.GeneralOptions> { }
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
    public DataverseEnvironment CurrentEnvironment { get; set; } = DataverseEnvironment.Empty;

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

        UpdateLoggingLevel();
    }

    private void UpdateLoggingLevel()
    {
        var serviceProvider = VS.GetRequiredService<SToolkitServiceProvider<XrmToolsPackage>, IToolkitServiceProvider<XrmToolsPackage>>();
        var loggerFilterOptions = serviceProvider.GetRequiredService<IConfigureOptions<LoggerFilterOptions>>();
        loggerFilterOptions.Configure(new LoggerFilterOptions { MinLevel = LogLevel });
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
[TypeConverter(typeof(DataverseEnvironmentConverter))]
public record DataverseEnvironment
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

    [MemberNotNullWhen(true, nameof(Url), nameof(ConnectionString))]
    public bool IsValid() => !string.IsNullOrWhiteSpace(Url) && !string.IsNullOrWhiteSpace(ConnectionString);

    public virtual bool Equals(DataverseEnvironment? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // Only consider Url and ConnectionString in equality
        return Url == other.Url && ConnectionString == other.ConnectionString;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (Url?.GetHashCode() ?? 0);
        hash = hash * 23 + (ConnectionString?.GetHashCode() ?? 0);
        return hash;
    }

    public static DataverseEnvironment Empty => new ();
}

public class DataverseEnvironmentConverter : ExpandableObjectConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is DataverseEnvironment environment)
        {
            // Return the desired format when the object is collapsed
            if (DataverseEnvironment.Empty.Equals(environment))
            {
                return "Empty";
            }
            return $"{environment.Name} ({environment.Url})";
        }

        // Call the base class to handle other conversions
        return base.ConvertTo(context, culture, value, destinationType);
    }
}

#nullable restore