#nullable enable
namespace XrmTools.Options;
using Community.VisualStudio.Toolkit;
using System;
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
    public DataverseEnvironmentList Environments { get; set; } = [];

    [Category("Power Platform Environments")]
    [DisplayName("Current environment kept in")]
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
#nullable restore