#nullable enable
namespace XrmTools;

using Community.VisualStudio.Toolkit;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

internal interface ISettingsRepository
{
    bool IsDirty { get; set; }

    IEnumerable<string> SolutionSettingKeys { get; }
    IEnumerable<string> SolutionUserSettingKeys { get; }

    string? GetSolutionSetting(string key);
    string? GetSolutionUserSetting(string key);
    void SetSolutionSetting(string key, string? value);
    void SetSolutionUserSetting(string key, string? value);

    IEnumerable<string> ProjectSettingKeys { get; }
    IEnumerable<string> ProjectUserSettingKeys { get; }

    Task<string?> GetProjectSettingAsync(string key);
    Task<string?> GetProjectUserSettingAsync(string key);
    Task<bool> SetProjectSettingAsync(string key, string? value);
    Task<bool> SetProjectUserSettingAsync(string key, string? value);
}

[Guid(PackageGuids.guidEnvironmentProviderString)]
[ComVisible(true)]
internal interface ISettingsProvider
{
    SolutionSettingsAccessor SolutionSettings { get; }
    SolutionSettingsAccessor SolutionUserSettings { get; }
    ProjectSettingsAccessor ProjectSettings { get; }
    ProjectSettingsAccessor ProjectUserSettings { get; }
}

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

    bool ISettingsRepository.IsDirty { get; set; }

    public SettingsProvider()
    {
        SolutionSettings = new SolutionSettingsAccessor(AsRepository().GetSolutionSetting, AsRepository().SetSolutionSetting);
        SolutionUserSettings = new SolutionSettingsAccessor(AsRepository().GetSolutionUserSetting, AsRepository().SetSolutionUserSetting);
        ProjectSettings = new ProjectSettingsAccessor(AsRepository().GetProjectSettingAsync, AsRepository().SetProjectSettingAsync);
        ProjectUserSettings = new ProjectSettingsAccessor(AsRepository().GetProjectUserSettingAsync, AsRepository().SetProjectUserSettingAsync);
    }

    void ISettingsRepository.SetSolutionSetting(string key, string? value)
    {
        solutionSettings[key] = value;
        AsRepository().IsDirty = true;
    }
    void ISettingsRepository.SetSolutionUserSetting(string key, string? value) => solutionUserSettings[key] = value;
    async Task<bool> ISettingsRepository.SetProjectSettingAsync(string key, string? value)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj is null) return false;
        var result = await proj.RemoveAttributeAsync(key, ProjectStorageType.UserFile);
        result = await proj.RemoveAttributeAsync(key);
        return await proj.TrySetAttributeAsync(key, value, ProjectStorageType.ProjectFile);
    }
    async Task<bool> ISettingsRepository.SetProjectUserSettingAsync(string key, string? value)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj is null) return false;
        var result = await proj.RemoveAttributeAsync(key, ProjectStorageType.ProjectFile);
        return await proj.TrySetAttributeAsync(key, value, ProjectStorageType.UserFile);
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
//internal class SolutionSettingsAccessor(Func<string, string?> getterFunction, Action<string, string?> setterFunction)
//{
//    public string? EnvironmentUrl { get => getterFunction("EnvironmentUrl"); set => setterFunction("EnvironmentUrl", value); }
//}

/// <summary>
/// Strongly typed accessor for project settings.
/// </summary>
//internal class ProjectSettingsAccessor(Func<string, Task<string?>> getterFunction, Func<string, string?, Task> setterFunction)
//{
//    public Task<string?> GetEnvironmentUrlAsync() => getterFunction("EnvironmentUrl");
//    public Task SetEnvironmentUrlAsync(string? value) => setterFunction("EnvironmentUrl", value);
//    public Task<string?> GetConnectionStringAsync() => getterFunction("ConnectionString");
//    public Task SetConnectionStringAsync(string? value) => setterFunction("ConnectionString", value);
//    public Task<string?> GetPluginCodeGenTemplateFilePathAsync() => getterFunction("DataversePluginTemplateFilePath");
//    public Task SetPluginCodeGenTemplateFilePathAsync(string? value) => setterFunction("DataversePluginTemplateFilePath", value);
//    public Task<string?> GetEntityCodeGenTemplateFilePathAsync() => getterFunction("DataverseEntityTemplateFilePath");
//    public Task SetEntityCodeGenTemplateFilePathAsync(string? value) => setterFunction("DataverseEntityTemplateFilePath", value);
//}

public interface IXrmToolsSettings
{
    Task<string?> EnvironmentUrlAsync();
    Task<string?> ConnectionStringAsync();
    Task<string?> PluginTemplateFilePathAsync();
    Task<string?> EntityTemplateFilePathAsync();
    Task<bool> EnvironmentUrlAsync(string value);
    Task<bool> ConnectionStringAsync(string value);
    Task<bool> PluginTemplateFilePathAsync(string value);
    Task<bool> EntityTemplateFilePathAsync(string value);
}

public class ProjectSettings(ProjectStorageType storageType) : IXrmToolsSettings
{
    public async Task<string?> EnvironmentUrlAsync() => await GetSettingAsync("EnvironmentUrl");
    public async Task<string?> ConnectionStringAsync() => await GetSettingAsync("DataverseConnectionString");
    public async Task<string?> PluginTemplateFilePathAsync() => await GetSettingAsync("DataversePluginTemplateFilePath");
    public async Task<string?> EntityTemplateFilePathAsync() => await GetSettingAsync("DataverseEntityTemplateFilePath");
    public async Task<bool> EnvironmentUrlAsync(string value) => await SetSettingAsync("EnvironmentUrl", value);
    public async Task<bool> ConnectionStringAsync(string value) => await SetSettingAsync("DataverseConnectionString", value);
    public async Task<bool> PluginTemplateFilePathAsync(string value) => await SetSettingAsync("DataversePluginTemplateFilePath", value);
    public async Task<bool> EntityTemplateFilePathAsync(string value) => await SetSettingAsync("DataverseEntityTemplateFilePath", value);

    private async Task<string?> GetSettingAsync(string name)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj == null) return null;
        return await proj.GetAttributeAsync(name);
    }

    private async Task<bool> SetSettingAsync(string name, string? value)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj == null) return false;
        return await proj.TrySetAttributeAsync(name, value, storageType);
    }
}

public enum SolutionStorageType { Solution, SolutionUser }

public class SolutionSettings(SolutionStorageType storageType) : IXrmToolsSettings
{
    private readonly Dictionary<string, string?> solutionSettings = new()
    {
        ["EnvironmentUrl"] = null,
        ["ConnectionString"] = null,
        ["PluginCodeGenTemplateFilePath"] = null,
        ["EntityCodeGenTemplateFilePath"] = null
    };
    private readonly Dictionary<string, string?> solutionUserSettings = new()
    {
        ["EnvironmentUrl"] = null,
        ["ConnectionString"] = null,
        ["PluginCodeGenTemplateFilePath"] = null,
        ["EntityCodeGenTemplateFilePath"] = null
    };

    public async Task<string?> EnvironmentUrlAsync() => await GetSettingAsync("EnvironmentUrl");
    public async Task<string?> ConnectionStringAsync() => await GetSettingAsync("DataverseConnectionString");
    public async Task<string?> PluginTemplateFilePathAsync() => await GetSettingAsync("DataversePluginTemplateFilePath");
    public async Task<string?> EntityTemplateFilePathAsync() => await GetSettingAsync("DataverseEntityTemplateFilePath");
    public async Task<bool> EnvironmentUrlAsync(string value) => await SetSettingAsync("EnvironmentUrl", value);
    public async Task<bool> ConnectionStringAsync(string value) => await SetSettingAsync("DataverseConnectionString", value);
    public async Task<bool> PluginTemplateFilePathAsync(string value) => await SetSettingAsync("DataversePluginTemplateFilePath", value);
    public async Task<bool> EntityTemplateFilePathAsync(string value) => await SetSettingAsync("DataverseEntityTemplateFilePath", value);

    private Task<string?> GetSettingAsync(string name)
    {
        if (storageType == SolutionStorageType.Solution)
        {
            solutionSettings.TryGetValue(name, out var value);
            return Task.FromResult(value);
        }
        else if (storageType == SolutionStorageType.SolutionUser)
        {
            solutionUserSettings.TryGetValue(name, out var value);
            return Task.FromResult(value);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private Task<bool> SetSettingAsync(string name, string? value)
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
        return Task.FromResult(true);
    }
}
#nullable restore