#nullable enable
namespace XrmTools.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

public enum SolutionStorageType { Solution, SolutionUser }

public class SolutionSettings(SolutionStorageType storageType) : IXrmToolsSettings
{
    // Null values will cause an error if we try to save them in the solution file. We will skip null values when
    // we save the settings.
    private readonly Dictionary<string, string?> solutionSettings = new()
    {
        [XrmSettingKeys.EnvironmentUrl] = string.Empty,
        [XrmSettingKeys.DataverseConnectionString] = null,
        [XrmSettingKeys.DataversePluginTemplateFilePath] = null,
        [XrmSettingKeys.DataverseEntityTemplateFilePath] = null
    };
    private readonly Dictionary<string, string?> solutionUserSettings = new()
    {
        [XrmSettingKeys.EnvironmentUrl] = string.Empty,
        [XrmSettingKeys.DataverseConnectionString] = null,
        [XrmSettingKeys.DataversePluginTemplateFilePath] = null,
        [XrmSettingKeys.DataverseEntityTemplateFilePath] = null
    };

    public IEnumerable<string> Keys => 
        storageType == SolutionStorageType.Solution ? 
        solutionSettings.Where(kv => kv.Value != null).Select(kv => kv.Key).ToList() : 
        solutionUserSettings.Where(kv => kv.Value != null).Select(kv => kv.Key).ToList();
    public bool IsDirty { get; set; }

    public string? EnvironmentUrl() => Get(XrmSettingKeys.EnvironmentUrl);
    public string? ConnectionString() => Get(XrmSettingKeys.DataverseConnectionString);
    public string? PluginTemplateFilePath() => Get(XrmSettingKeys.DataversePluginTemplateFilePath);
    public string? EntityTemplateFilePath() => Get(XrmSettingKeys.DataverseEntityTemplateFilePath);
    public bool EnvironmentUrl(string? value) => Set(XrmSettingKeys.EnvironmentUrl, value);
    public bool ConnectionString(string? value) => Set(XrmSettingKeys.DataverseConnectionString, value);
    public bool PluginTemplateFilePath(string? value) => Set(XrmSettingKeys.DataversePluginTemplateFilePath, value);
    public bool EntityTemplateFilePath(string? value) => Set(XrmSettingKeys.DataverseEntityTemplateFilePath, value);

    public string? Get(string name)
    {
        if (storageType == SolutionStorageType.Solution)
        {
            solutionSettings.TryGetValue(name, out var value);
            return value;
        }
        else if (storageType == SolutionStorageType.SolutionUser)
        {
            solutionUserSettings.TryGetValue(name, out var value);
            return value;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public bool Set(string name, string? value)
    {
        if (storageType == SolutionStorageType.Solution)
        {
            solutionSettings[name] = value;
        }
        else if (storageType == SolutionStorageType.SolutionUser)
        {
            solutionUserSettings[name] = value;
        }
        else
        {
            throw new NotImplementedException();
        }
        IsDirty = true;
        return true;
    }
}
#nullable restore