#nullable enable
namespace XrmTools.Settings;
using Community.VisualStudio.Toolkit;
using System;
using System.Collections.Generic;
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
}

[ComVisible(true)]
public class SettingsProvider : ISettingsProvider
{
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
        => await ResolveFilePathAsync(await ProjectUserSettings.EntityTemplateFilePathAsync(), true, false)
        ?? await ResolveFilePathAsync(await ProjectSettings.EntityTemplateFilePathAsync(), true, false)
        ?? await ResolveFilePathAsync(SolutionUserSettings.EntityTemplateFilePath(), false, true)
        ?? await ResolveFilePathAsync(SolutionSettings.EntityTemplateFilePath(), false, true);
    /// <inheritdoc cref="ISettingsProvider.PluginTemplateFilePathAsync"/>
    public async Task<string?> PluginTemplateFilePathAsync()
        => await ResolveFilePathAsync(await ProjectUserSettings.PluginTemplateFilePathAsync(), true, false)
        ?? await ResolveFilePathAsync(await ProjectSettings.PluginTemplateFilePathAsync(), true, false)
        ?? await ResolveFilePathAsync(SolutionUserSettings.PluginTemplateFilePath(), false, true)
        ?? await ResolveFilePathAsync(SolutionSettings.PluginTemplateFilePath(), false, true);
    /// <inheritdoc cref="ISettingsProvider.GlobalOptionSetsTemplateFilePathAsync"/>
    public async Task<string?> GlobalOptionSetsTemplateFilePathAsync()
        => await ResolveFilePathAsync(await ProjectUserSettings.GlobalOptionSetsTemplateFilePathAsync(), true, false)
        ?? await ResolveFilePathAsync(await ProjectSettings.GlobalOptionSetsTemplateFilePathAsync(), true, false)
        ?? await ResolveFilePathAsync(SolutionUserSettings.GlobalOptionSetsTemplateFilePath(), false, true)
        ?? await ResolveFilePathAsync(SolutionSettings.GlobalOptionSetsTemplateFilePath(), false, true);
    public async Task<string?> GlobalOptionSetsFilePathAsync()
    {
        var path = await ProjectSettings.GlobalOptionSetsFilePathAsync();
        if (string.IsNullOrWhiteSpace(path))
        {
            path = "GlobalOptionSets.cs";
        }
        return await ResolveFilePathAsync(path, true, false);
    }


    private async Task<string?> ResolveFilePathAsync(string? filePath, bool atProjectLevel, bool atSolutionLevel)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return null;

        if (filePath![0] == Path.DirectorySeparatorChar || filePath[0] == Path.AltDirectorySeparatorChar)
        {
            filePath = filePath[1..];
        }
        if (Path.IsPathRooted(filePath)) return filePath;

        if (atProjectLevel)
        {
            var proj = await VS.Solutions.GetActiveProjectAsync();
            if (proj != null)
            {
                return Path.Combine(Path.GetDirectoryName(proj.FullPath), filePath);
            }
        }
        if (atSolutionLevel)
        {
            var solution = await VS.Solutions.GetCurrentSolutionAsync();
            if (solution != null)
            {
                return Path.Combine(Path.GetDirectoryName(solution.FullPath), filePath);
            }
        }
        return null;
    }
}
#nullable restore