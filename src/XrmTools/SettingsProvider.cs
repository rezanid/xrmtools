#nullable enable
namespace XrmTools;

using Community.VisualStudio.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using XrmTools.Options;

internal interface ISettingsRepository
{
    IEnumerable<string> SolutionSettingKeys { get; }
    IEnumerable<string> SolutionUserSettingKeys { get; }

    string? GetSolutionSetting(string key);
    string? GetSolutionUserSetting(string key);
    void SetSolutionSetting(string key, string value);
    void SetSolutionUserSetting(string key, string value);

    IEnumerable<string> ProjectSettingKeys { get; }
    IEnumerable<string> ProjectUserSettingKeys { get; }

    Task<string?> GetProjectSettingAsync(string key);
    Task<string?> GetProjectUserSettingAsync(string key);
    Task<bool> SetProjectSettingAsync(string key, string value);
    Task<bool> SetProjectUserSettingAsync(string key, string value);
}

[Guid(PackageGuids.guidEnvironmentProviderString)]
[ComVisible(true)]
internal interface ISettingsProvider
{
    GeneralOptions Options { get; }
    Task<GeneralOptions> GetOptionsAsync();

    SolutionSettingsAccessor SolutionSettings { get; }
    SolutionSettingsAccessor SolutionUserSettings { get; }
    ProjectSettingsAccessor ProjectSettings { get; }
    ProjectSettingsAccessor ProjectUserSettings { get; }
}

[Export(typeof(ISettingsProvider))]
[ComVisible(true)]
internal class SettingsProvider : ISettingsProvider, ISettingsRepository
{
    // Solution settings (.sln, .suo) are kept in dictionaries and loaded and replaced from ther package.
    // Project settings are directly written to and read from the project (and project user) file.
    private readonly Dictionary<string, string?> solutionSettings = new()
    {
        ["EnvironmentUrl"] = null
    };
    private readonly Dictionary<string, string?> solutionUserSettings = new()
    {
        ["EnvironmentUrl"] = null
    };
    private GeneralOptions? options;

    public async Task<GeneralOptions> GetOptionsAsync()
    {
        if (options is not null) return options;
        options = await GeneralOptions.GetLiveInstanceAsync();
        return options;
    }
    public GeneralOptions Options 
    {
        get => options ??= GeneralOptions.Instance;
    }

    IEnumerable<string> ISettingsRepository.SolutionSettingKeys => solutionSettings.Keys;
    IEnumerable<string> ISettingsRepository.SolutionUserSettingKeys => solutionUserSettings.Keys;
    IEnumerable<string> ISettingsRepository.ProjectSettingKeys => 
        ["EnvironmentUrl", "ConnectionString", "PluginCodeGenTemplateFilePath", "EntityCodeGenTemplateFilePath"];
    IEnumerable<string> ISettingsRepository.ProjectUserSettingKeys =>
        ["EnvironmentUrl", "ConnectionString", "PluginCodeGenTemplateFilePath", "EntityCodeGenTemplateFilePath"];

    public SolutionSettingsAccessor SolutionSettings { get; }
    public SolutionSettingsAccessor SolutionUserSettings { get; }
    public ProjectSettingsAccessor ProjectSettings { get; }
    public ProjectSettingsAccessor ProjectUserSettings { get; }

    public SettingsProvider()
    {
        SolutionSettings = new SolutionSettingsAccessor(AsRepository().GetSolutionSetting);
        SolutionUserSettings = new SolutionSettingsAccessor(AsRepository().GetSolutionUserSetting);
        ProjectSettings = new ProjectSettingsAccessor(AsRepository().GetProjectSettingAsync);
        ProjectUserSettings = new ProjectSettingsAccessor(AsRepository().GetProjectUserSettingAsync);
    }

    void ISettingsRepository.SetSolutionSetting(string key, string value) => solutionSettings[key] = value;
    void ISettingsRepository.SetSolutionUserSetting(string key, string value) => solutionUserSettings[key] = value;
    async Task<bool> ISettingsRepository.SetProjectSettingAsync(string key, string value)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj is null) return false;
        return await proj.TrySetAttributeAsync(key, value, ProjectStorageType.ProjectFile);
    }
    async Task<bool> ISettingsRepository.SetProjectUserSettingAsync(string key, string value)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj is null) return false;
        return await proj.TrySetAttributeAsync(key, value, ProjectStorageType.ProjectFile);
    }

    string? ISettingsRepository.GetSolutionSetting(string key) => solutionSettings.TryGetValue(key, out var value) ? value : null;
    string? ISettingsRepository.GetSolutionUserSetting(string key) => solutionUserSettings.TryGetValue(key, out var value) ? value : null;
    async Task<string?> ISettingsRepository.GetProjectSettingAsync(string key)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj is null) return null;
        return await proj.GetAttributeAsync(key, ProjectStorageType.ProjectFile);
    }
    async Task<string?> ISettingsRepository.GetProjectUserSettingAsync(string key)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj is null) return null;
        return await proj.GetAttributeAsync(key, ProjectStorageType.UserFile);
    }

    private ISettingsRepository AsRepository() => (ISettingsRepository)this;
}

/// <summary>
/// Strongly typed accessor for solution settings.
/// </summary>
internal class SolutionSettingsAccessor(Func<string, string?> accessorFunction)
{
    public string? EnvironmentUrl { get => accessorFunction("EnvironmentUrl"); }
}

/// <summary>
/// Strongly typed accessor for project settings.
/// </summary>
/// <param name="accessorFunction"></param>
internal class ProjectSettingsAccessor(Func<string, Task<string?>> accessorFunction)
{
    public Task<string?> GetEnvironmentUrlAsync() => accessorFunction("EnvironmentUrl");
    public Task<string?> GetConnectionStringAsync() => accessorFunction("ConnectionString");
    public Task<string?> GetPluginCodeGenTemplateFilePathAsync() => accessorFunction("PluginCodeGenTemplateFilePath");
    public Task<string?> GetEntityCodeGenTemplateFilePathAsync() => accessorFunction("EntityCodeGenTemplateFilePath");
}
#nullable restore