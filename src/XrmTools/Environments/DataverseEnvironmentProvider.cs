#nullable enable
namespace XrmTools;

using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.Options;
using XrmTools.Settings;

[Export(typeof(IEnvironmentProvider))]
public class DataverseEnvironmentProvider : IEnvironmentProvider
{
    public static event Action<DataverseEnvironment>? EnvironmentChanged;

    [Import]
    ISettingsProvider SettingsProvider { get; set; } = null!;
    [Import]
    IXrmHttpClientFactory HttpClientFactory { get; set; } = null!;

    public async Task<DataverseEnvironment?> GetActiveEnvironmentAsync(bool allowInteraction)
    {
        var environment = (await GeneralOptions.GetLiveInstanceAsync()).CurrentEnvironmentStorage switch
        {
            SettingsStorageTypes.Options => (await GeneralOptions.GetLiveInstanceAsync().ConfigureAwait(false)).CurrentEnvironment,
            SettingsStorageTypes.Solution => GetEnvironmentFromSolution(),
            SettingsStorageTypes.SolutionUser => GetEnvironmentFromSolutionUserFile(),
            SettingsStorageTypes.Project => await GetEnvironmentFromProjectAsync().ConfigureAwait(false),
            SettingsStorageTypes.ProjectUser => await GetEnvironmentFromProjectUserFileAsync().ConfigureAwait(false),
            _ => GeneralOptions.Instance.CurrentEnvironment,
        };

        if (environment?.IsValid != true) return null;

        await VerifyAuthenticatedAsync(environment, allowInteraction).ConfigureAwait(false);

        return environment;
    }

    public async Task SetActiveEnvironmentAsync(DataverseEnvironment environment, bool allowInteraction)
    {
        if (environment?.IsValid != true) return;

        var options = await GeneralOptions.GetLiveInstanceAsync();
        switch (options.CurrentEnvironmentStorage)
        {
            case SettingsStorageTypes.Options:
                options.CurrentEnvironment = environment;
                await options.SaveAsync();
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

        await VerifyAuthenticatedAsync(environment, allowInteraction).ConfigureAwait(false);

        EnvironmentChanged?.Invoke(environment);
    }

    public async Task<IList<DataverseEnvironment>> GetAvailableEnvironmentsAsync() 
    {
        var options = await GeneralOptions.GetLiveInstanceAsync();
        return options?.Environments ?? [];
    }

    private DataverseEnvironment? GetEnvironmentFromSolution()
    {
        var url = SettingsProvider.SolutionSettings.EnvironmentUrl();
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private DataverseEnvironment? GetEnvironmentFromSolutionUserFile()
    {
        var url = SettingsProvider.SolutionUserSettings.EnvironmentUrl();
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private async Task<DataverseEnvironment?> GetEnvironmentFromProjectAsync()
    {
        var url = await SettingsProvider.ProjectSettings.EnvironmentUrlAsync();
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private async Task<DataverseEnvironment?> GetEnvironmentFromProjectUserFileAsync()
    {
        var url = await SettingsProvider.ProjectUserSettings.EnvironmentUrlAsync();
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private void SetEnvironmentInSolution(DataverseEnvironment? environment) => SettingsProvider.SolutionSettings.EnvironmentUrl(environment?.Url);
    private void SetEnvironmentInSolutionUserFile(DataverseEnvironment? environment) => SettingsProvider.SolutionUserSettings.EnvironmentUrl(environment?.Url);
    private async Task SetEnvironmentInProjectAsync(DataverseEnvironment? environment) => await SettingsProvider.ProjectSettings.EnvironmentUrlAsync(environment?.Url);
    private async Task SetEnvironmentInProjectUserFileAsync(DataverseEnvironment? environment) => await SettingsProvider.ProjectUserSettings.EnvironmentUrlAsync(environment?.Url);
    private async Task VerifyAuthenticatedAsync(
        DataverseEnvironment environment,
        bool allowInteraction)
    {
        if (environment?.IsValid != true) return;
        if (environment.IsAutehnticated) return;

        try
        {
            if (allowInteraction)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            }
            await HttpClientFactory.PreAuthenticateAsync(environment, allowInteraction);
        }
        catch (Exception) { }
    }
}
#nullable restore