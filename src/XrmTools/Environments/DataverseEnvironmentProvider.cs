﻿#nullable enable
namespace XrmTools;

using System.Linq;
using System.Threading.Tasks;
using XrmTools.Options;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

[Guid(PackageGuids.guidEnvironmentProviderString)]
[ComVisible(true)]
public interface IEnvironmentProvider
{
    Task<DataverseEnvironment?> GetActiveEnvironmentAsync();
    //Task SetActiveEnvironmentAsync(DataverseEnvironment environment);
}

[Export(typeof(IEnvironmentProvider))]
[ComVisible(true)]
[method: ImportingConstructor]
internal class DataverseEnvironmentProvider([Import]ISettingsProvider settingsProvider) : IEnvironmentProvider
{
    public async Task<DataverseEnvironment?> GetActiveEnvironmentAsync()
        => (await GeneralOptions.GetLiveInstanceAsync()).EnvironmentSettingLevel switch
        {
            EnvironmentSettingLevel.Options => GeneralOptions.Instance.CurrentEnvironment,
            EnvironmentSettingLevel.Solution => GetEnvironmentFromSolution(),
            EnvironmentSettingLevel.SolutionUser => GetEnvironmentFromSolutionUserFile(),
            EnvironmentSettingLevel.Project => await GetEnvironmentFromProjectAsync().ConfigureAwait(false),
            EnvironmentSettingLevel.ProjectUser => await GetEnvironmentFromProjectUserFileAsync().ConfigureAwait(false),
            _ => GeneralOptions.Instance.CurrentEnvironment,
        };

    //public async Task SetActiveEnvironmentAsync(DataverseEnvironment environment)
    //{
    //    var options = await GeneralOptions.GetLiveInstanceAsync();
    //    switch (options.EnvironmentSettingLevel)
    //    {
    //        case EnvironmentSettingLevel.Options:
    //            GeneralOptions.Instance.CurrentEnvironment = environment;
    //            SetEnvironmentInSolution(null);
    //            SetEnvironmentInSolutionUserFile(null);
    //            await SetEnvironmentInProjectAsync(null);
    //            await SetEnvironmentInProjectUserFileAsync(null);
    //            break;
    //        case EnvironmentSettingLevel.Solution:
    //            SetEnvironmentInSolution(environment);
    //            GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
    //            SetEnvironmentInSolutionUserFile(null);
    //            await SetEnvironmentInProjectAsync(null);
    //            await SetEnvironmentInProjectUserFileAsync(null);
    //            break;
    //        case EnvironmentSettingLevel.SolutionUser:
    //            SetEnvironmentInSolutionUserFile(environment);
    //            GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
    //            SetEnvironmentInSolution(null);
    //            await SetEnvironmentInProjectAsync(null);
    //            await SetEnvironmentInProjectUserFileAsync(null);
    //            break;
    //        case EnvironmentSettingLevel.Project:
    //            await SetEnvironmentInProjectAsync(environment);
    //            GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
    //            SetEnvironmentInSolution(null);
    //            SetEnvironmentInSolutionUserFile(null);
    //            await SetEnvironmentInProjectUserFileAsync(null);
    //            break;
    //        case EnvironmentSettingLevel.ProjectUser:
    //            await SetEnvironmentInProjectUserFileAsync(environment);
    //            GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
    //            SetEnvironmentInSolution(null);
    //            SetEnvironmentInSolutionUserFile(null);
    //            await SetEnvironmentInProjectAsync(null);
    //            break;
    //    }
    //}

    private DataverseEnvironment? GetEnvironmentFromSolution()
    {
        var url = settingsProvider.SolutionSettings.EnvironmentUrl;
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private DataverseEnvironment? GetEnvironmentFromSolutionUserFile()
    {
        var url = settingsProvider.SolutionUserSettings.EnvironmentUrl;
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private async Task<DataverseEnvironment?> GetEnvironmentFromProjectAsync()
    {
        var url = await settingsProvider.ProjectSettings.GetEnvironmentUrlAsync();
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private async Task<DataverseEnvironment?> GetEnvironmentFromProjectUserFileAsync()
    {
        var url = await settingsProvider.ProjectUserSettings.GetEnvironmentUrlAsync();
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    //private void SetEnvironmentInSolution(DataverseEnvironment environment) => package.DataverseUrlSetting = environment?.Url;
    //private void SetEnvironmentInSolutionUserFile(DataverseEnvironment environment) => package.DataverseUrlOption = environment?.Url;
    //private async Task SetEnvironmentInProjectAsync(DataverseEnvironment environment)
    //{
    //    var project = await VS.Solutions.GetActiveProjectAsync();
    //    if (project is null) return;
    //    if (environment?.Url is null)
    //    {
    //        await project.RemoveAttributeAsync("DataverseUrl", ProjectStorageType.ProjectFile);
    //        return;
    //    }
    //    await project.TrySetAttributeAsync("DataverseUrl", environment.Url, ProjectStorageType.ProjectFile);
    //}
    //private async Task SetEnvironmentInProjectUserFileAsync(DataverseEnvironment environment)
    //{
    //    var project = await VS.Solutions.GetActiveProjectAsync();
    //    if (project is null) return;
    //    if (environment?.Url is null)
    //    {
    //        await project.RemoveAttributeAsync("DataverseUrl", ProjectStorageType.UserFile);
    //        return;
    //    }
    //    await project.TrySetAttributeAsync("DataverseUrl", environment.Url, ProjectStorageType.UserFile);
    //}
}
#nullable restore