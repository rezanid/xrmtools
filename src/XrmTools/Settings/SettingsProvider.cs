#nullable enable
namespace XrmTools.Settings;
using Community.VisualStudio.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

[Guid(PackageGuids.guidSettingsProviderString)]
public interface ISettingsProvider 
{
    SolutionSettings SolutionSettings { get; }
    SolutionSettings SolutionUserSettings { get; }
    ProjectSettings ProjectSettings { get; }
    ProjectSettings ProjectUserSettings { get; }

    /// <summary>
    /// Searchs for the connection string in the following order:
    /// 1. Project user settings
    /// 2. Project settings
    /// 3. Solution user settings
    /// 4. Solution settings
    /// </summary>
    /// <returns></returns>
    Task<string?> ConnectionStringAsync();
    /// <summary>
    /// Searchs for the entity template file path in the following order:
    /// 1. Project user settings
    /// 2. Project settings
    /// 3. Solution user settings
    /// 4. Solution settings
    /// </summary>
    /// <returns>Full file path or null if the setting is not set.</returns>
    Task<string?> EntityTemplateFilePathAsync();
    /// <summary>
    /// Searchs for the plugin template file path in the following order:
    /// 1. Project user settings
    /// 2. Project settings
    /// 3. Solution user settings
    /// 4. Solution settings
    /// </summary>
    /// <returns>Full file path or null if the setting is not set.</returns>
    Task<string?> PluginTemplateFilePathAsync();
    /// <summary>
    /// Searchs for the global option sets template file path in the following order:
    /// 1. Project user settings
    /// 2. Project settings
    /// 3. Solution user settings
    /// 4. Solution settings
    /// </summary>
    /// <returns>Full file path or null if the setting is not set.</returns>
    Task<string?> GlobalOptionSetsTemplateFilePathAsync();
    /// <summary>
    /// Searchs for the GlobalOptionSets file path in the active project settings.
    /// </summary>
    /// <returns>Full file path or null if the setting is not set.</returns>
    Task<string?> GlobalOptionSetsFilePathAsync();
    Task DeleteEntityTemplateFilePathSettingAsync();
    Task DeletePluginTemplateFilePathSettingAsync();
}

[ComVisible(true)]
public class SettingsProvider : ISettingsProvider
{
    enum ResolveScope { Project, Solution }

    // NOTE!
    // * Solution settings (.sln, .suo) are kept in dictionaries, then loaded and replaced from the package.
    // * Project settings are directly written to and read from the project (and project user) file.
    public SolutionSettings SolutionSettings { get; } = new SolutionSettings(SolutionStorageType.Solution);
    public SolutionSettings SolutionUserSettings { get; } = new SolutionSettings(SolutionStorageType.SolutionUser);
    public ProjectSettings ProjectSettings { get; } = new ProjectSettings(ProjectStorageType.ProjectFile);
    public ProjectSettings ProjectUserSettings { get; } = new ProjectSettings(ProjectStorageType.UserFile);

    public IEnumerable<string> Keys => throw new NotImplementedException();

    public bool IsDirty => throw new NotImplementedException();

    /// <inheritdoc cref="ISettingsProvider.ConnectionStringAsync"/>
    public async Task<string?> ConnectionStringAsync()
        => await ProjectUserSettings.ConnectionStringAsync()
        ?? await ProjectSettings.ConnectionStringAsync()
        ?? SolutionUserSettings.ConnectionString()
        ?? SolutionSettings.ConnectionString();
    /// <inheritdoc cref="ISettingsProvider.EntityTemplateFilePathAsync"/>
    public async Task<string?> EntityTemplateFilePathAsync()
        => await ResolveFilePathAsync(await ProjectUserSettings.EntityTemplateFilePathAsync(), ResolveScope.Project)
        ?? await ResolveFilePathAsync(await ProjectSettings.EntityTemplateFilePathAsync(), ResolveScope.Project)
        ?? await ResolveFilePathAsync(SolutionUserSettings.EntityTemplateFilePath(), ResolveScope.Solution)
        ?? await ResolveFilePathAsync(SolutionSettings.EntityTemplateFilePath(), ResolveScope.Solution);
    /// <inheritdoc cref="ISettingsProvider.PluginTemplateFilePathAsync"/>
    public async Task<string?> PluginTemplateFilePathAsync()
        => await ResolveFilePathAsync(await ProjectUserSettings.PluginTemplateFilePathAsync(), ResolveScope.Project)
        ?? await ResolveFilePathAsync(await ProjectSettings.PluginTemplateFilePathAsync(), ResolveScope.Project)
        ?? await ResolveFilePathAsync(SolutionUserSettings.PluginTemplateFilePath(), ResolveScope.Solution)
        ?? await ResolveFilePathAsync(SolutionSettings.PluginTemplateFilePath(), ResolveScope.Solution);
    /// <inheritdoc cref="ISettingsProvider.GlobalOptionSetsTemplateFilePathAsync"/>
    public async Task<string?> GlobalOptionSetsTemplateFilePathAsync()
        => await ResolveFilePathAsync(await ProjectUserSettings.GlobalOptionSetsTemplateFilePathAsync(), ResolveScope.Project)
        ?? await ResolveFilePathAsync(await ProjectSettings.GlobalOptionSetsTemplateFilePathAsync(), ResolveScope.Project)
        ?? await ResolveFilePathAsync(SolutionUserSettings.GlobalOptionSetsTemplateFilePath(), ResolveScope.Solution)
        ?? await ResolveFilePathAsync(SolutionSettings.GlobalOptionSetsTemplateFilePath(), ResolveScope.Solution);

    public async Task DeleteEntityTemplateFilePathSettingAsync()
    {
        var value = await ProjectUserSettings.EntityTemplateFilePathAsync();
        if (!string.IsNullOrWhiteSpace(value))
        {
            await ProjectUserSettings.EntityTemplateFilePathAsync(null);
        }
        value = await ProjectSettings.EntityTemplateFilePathAsync();
        if (!string.IsNullOrWhiteSpace(value))
        {
            await ProjectSettings.EntityTemplateFilePathAsync(null);
        }
        value = SolutionUserSettings.EntityTemplateFilePath();
        if (!string.IsNullOrWhiteSpace(value))
        {
            SolutionUserSettings.EntityTemplateFilePath(null);
        }
        value = SolutionSettings.EntityTemplateFilePath();
        if (!string.IsNullOrWhiteSpace(value))
        {
            SolutionSettings.EntityTemplateFilePath(null);
        }
    }

    public async Task DeletePluginTemplateFilePathSettingAsync()
    {
        var value = await ProjectUserSettings.PluginTemplateFilePathAsync();
        if (!string.IsNullOrWhiteSpace(value))
        {
            await ProjectUserSettings.PluginTemplateFilePathAsync(null);
        }
        value = await ProjectSettings.PluginTemplateFilePathAsync();
        if (!string.IsNullOrWhiteSpace(value))
        {
            await ProjectSettings.PluginTemplateFilePathAsync(null);
        }
        value = SolutionUserSettings.PluginTemplateFilePath();
        if (!string.IsNullOrWhiteSpace(value))
        {
            SolutionUserSettings.PluginTemplateFilePath(null);
        }
        value = SolutionSettings.PluginTemplateFilePath();
        if (!string.IsNullOrWhiteSpace(value))
        {
            SolutionSettings.PluginTemplateFilePath(null);
        }
    }

    public async Task<string?> GlobalOptionSetsFilePathAsync()
    {
        var path = await ProjectSettings.GlobalOptionSetsFilePathAsync();
        if (string.IsNullOrWhiteSpace(path))
        {
            path = "GlobalOptionSets.cs";
        }
        return await ResolveFilePathAsync(path, ResolveScope.Project);
    }

    [return: NotNullIfNotNull(nameof(filePath))]
    private async Task<string?> ResolveFilePathAsync(string? filePath, ResolveScope resolveScope)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return null;

        if (filePath![0] == Path.DirectorySeparatorChar || filePath[0] == Path.AltDirectorySeparatorChar)
        {
            filePath = filePath[1..];
        }
        if (Path.IsPathRooted(filePath)) return filePath;

        if (resolveScope == ResolveScope.Project)
        {
            var proj = await VS.Solutions.GetActiveProjectAsync();
            if (proj != null)
            {
                return Path.Combine(Path.GetDirectoryName(proj.FullPath), filePath);
            }
        }
        var solution = await VS.Solutions.GetCurrentSolutionAsync();
        if (solution != null)
        {
            return Path.Combine(Path.GetDirectoryName(solution.FullPath), filePath);
        }
        return filePath;
    }
}
#nullable restore