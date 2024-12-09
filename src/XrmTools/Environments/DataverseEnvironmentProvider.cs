﻿#nullable enable
namespace XrmTools;
using System.Linq;
using System.Threading.Tasks;
using XrmTools.Options;
using System.ComponentModel.Composition;
using XrmTools.Settings;
using Microsoft.VisualStudio.Shell;
using XrmTools.Environments;

internal class DataverseEnvironmentProvider : IEnvironmentProvider
{
    private readonly ISettingsProvider settingsProvider;

    [ImportingConstructor]
    public DataverseEnvironmentProvider([Import] ISettingsProvider settingsProvider)
    {
        this.settingsProvider = settingsProvider; 
        var options = GeneralOptions.Instance;
        options.OptionsChanged += (s, e) =>
        {
            ThreadHelper.JoinableTaskFactory.Run(async () => await SetActiveEnvironmentAsync(options.CurrentEnvironment));
        };
    }

    public async Task<DataverseEnvironment?> GetActiveEnvironmentAsync()
    => (await GeneralOptions.GetLiveInstanceAsync()).CurrentEnvironmentStorage switch
    {
        SettingsStorageTypes.Options => (await GeneralOptions.GetLiveInstanceAsync().ConfigureAwait(false)).CurrentEnvironment,
        SettingsStorageTypes.Solution => GetEnvironmentFromSolution(),
        SettingsStorageTypes.SolutionUser => GetEnvironmentFromSolutionUserFile(),
        SettingsStorageTypes.Project => await GetEnvironmentFromProjectAsync().ConfigureAwait(false),
        SettingsStorageTypes.ProjectUser => await GetEnvironmentFromProjectUserFileAsync().ConfigureAwait(false),
        _ => GeneralOptions.Instance.CurrentEnvironment,
    };

    public DataverseEnvironment? GetActiveEnvironment()
    => (GeneralOptions.Instance).CurrentEnvironmentStorage switch
    {
        SettingsStorageTypes.Options => GeneralOptions.Instance.CurrentEnvironment,
        SettingsStorageTypes.Solution => GetEnvironmentFromSolution(),
        SettingsStorageTypes.SolutionUser => GetEnvironmentFromSolutionUserFile(),
        SettingsStorageTypes.Project => GetEnvironmentFromProjectAsync().ConfigureAwait(false).GetAwaiter().GetResult(),
        SettingsStorageTypes.ProjectUser => GetEnvironmentFromProjectUserFileAsync().ConfigureAwait(false).GetAwaiter().GetResult(),
        _ => GeneralOptions.Instance.CurrentEnvironment,
    };

    public async Task SetActiveEnvironmentAsync(DataverseEnvironment environment)
    {
        var options = await GeneralOptions.GetLiveInstanceAsync();
        switch (options.CurrentEnvironmentStorage)
        {
            case SettingsStorageTypes.Options:
                SetEnvironmentInSolution(null);
                SetEnvironmentInSolutionUserFile(null);
                await SetEnvironmentInProjectAsync(null);
                break;
            case SettingsStorageTypes.Solution:
                SetEnvironmentInSolution(environment);
                SetEnvironmentInSolutionUserFile(null);
                await SetEnvironmentInProjectAsync(null);
                break;
            case SettingsStorageTypes.SolutionUser:
                SetEnvironmentInSolutionUserFile(environment);
                SetEnvironmentInSolution(null);
                await SetEnvironmentInProjectAsync(null);
                break;
            case SettingsStorageTypes.Project:
                await SetEnvironmentInProjectAsync(environment);
                SetEnvironmentInSolution(null);
                SetEnvironmentInSolutionUserFile(null);
                break;
            case SettingsStorageTypes.ProjectUser:
                await SetEnvironmentInProjectUserFileAsync(environment);
                SetEnvironmentInSolution(null);
                SetEnvironmentInSolutionUserFile(null);
                break;
        }
    }

    private DataverseEnvironment? GetEnvironmentFromSolution()
    {
        var url = settingsProvider.SolutionSettings.EnvironmentUrl();
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private DataverseEnvironment? GetEnvironmentFromSolutionUserFile()
    {
        var url = settingsProvider.SolutionUserSettings.EnvironmentUrl();
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private async Task<DataverseEnvironment?> GetEnvironmentFromProjectAsync()
    {
        var url = await settingsProvider.ProjectSettings.EnvironmentUrlAsync();
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private async Task<DataverseEnvironment?> GetEnvironmentFromProjectUserFileAsync()
    {
        var url = await settingsProvider.ProjectUserSettings.EnvironmentUrlAsync();
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private void SetEnvironmentInSolution(DataverseEnvironment? environment) => settingsProvider.SolutionSettings.EnvironmentUrl(environment?.Url);
    private void SetEnvironmentInSolutionUserFile(DataverseEnvironment? environment) => settingsProvider.SolutionUserSettings.EnvironmentUrl(environment?.Url);
    private async Task SetEnvironmentInProjectAsync(DataverseEnvironment? environment) => await settingsProvider.ProjectSettings.EnvironmentUrlAsync(environment?.Url);
    private async Task SetEnvironmentInProjectUserFileAsync(DataverseEnvironment? environment) => await settingsProvider.ProjectUserSettings.EnvironmentUrlAsync(environment?.Url);
}
#nullable restore