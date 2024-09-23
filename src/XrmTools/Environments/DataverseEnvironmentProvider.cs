#nullable enable
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
    DataverseEnvironment? GetActiveEnvironment();
    Task<DataverseEnvironment?> GetActiveEnvironmentAsync();
    Task SetActiveEnvironmentAsync(DataverseEnvironment environment);
}

[ComVisible(true)]
[method: ImportingConstructor]
internal class DataverseEnvironmentProvider([Import]ISettingsRepository settingsRepo) : IEnvironmentProvider
{
    ISettingsProvider settingsProvider = (ISettingsProvider)settingsRepo;
    public async Task<DataverseEnvironment?> GetActiveEnvironmentAsync()
    => (await GeneralOptions.GetLiveInstanceAsync()).EnvironmentSettingLevel switch
    {
        EnvironmentSettingLevel.Options => (await GeneralOptions.GetLiveInstanceAsync().ConfigureAwait(false)).CurrentEnvironment,
        EnvironmentSettingLevel.Solution => GetEnvironmentFromSolution(),
        EnvironmentSettingLevel.SolutionUser => GetEnvironmentFromSolutionUserFile(),
        EnvironmentSettingLevel.Project => await GetEnvironmentFromProjectAsync().ConfigureAwait(false),
        EnvironmentSettingLevel.ProjectUser => await GetEnvironmentFromProjectUserFileAsync().ConfigureAwait(false),
        _ => GeneralOptions.Instance.CurrentEnvironment,
    };

    public DataverseEnvironment? GetActiveEnvironment()
    => (GeneralOptions.Instance).EnvironmentSettingLevel switch
    {
        EnvironmentSettingLevel.Options => GeneralOptions.Instance.CurrentEnvironment,
        EnvironmentSettingLevel.Solution => GetEnvironmentFromSolution(),
        EnvironmentSettingLevel.SolutionUser => GetEnvironmentFromSolutionUserFile(),
        EnvironmentSettingLevel.Project => GetEnvironmentFromProjectAsync().ConfigureAwait(false).GetAwaiter().GetResult(),
        EnvironmentSettingLevel.ProjectUser => GetEnvironmentFromProjectUserFileAsync().ConfigureAwait(false).GetAwaiter().GetResult(),
        _ => GeneralOptions.Instance.CurrentEnvironment,
    };

    public async Task SetActiveEnvironmentAsync(DataverseEnvironment environment)
    {
        var options = await GeneralOptions.GetLiveInstanceAsync();
        switch (options.EnvironmentSettingLevel)
        {
            case EnvironmentSettingLevel.Options:
                //GeneralOptions.Instance.CurrentEnvironment = environment;
                SetEnvironmentInSolution(null);
                SetEnvironmentInSolutionUserFile(null);
                await SetEnvironmentInProjectAsync(null);
                await SetEnvironmentInProjectUserFileAsync(null);
                break;
            case EnvironmentSettingLevel.Solution:
                SetEnvironmentInSolution(environment);
                //GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
                SetEnvironmentInSolutionUserFile(null);
                await SetEnvironmentInProjectAsync(null);
                await SetEnvironmentInProjectUserFileAsync(null);
                break;
            case EnvironmentSettingLevel.SolutionUser:
                SetEnvironmentInSolutionUserFile(environment);
                //GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
                SetEnvironmentInSolution(null);
                await SetEnvironmentInProjectAsync(null);
                await SetEnvironmentInProjectUserFileAsync(null);
                break;
            case EnvironmentSettingLevel.Project:
                await SetEnvironmentInProjectAsync(environment);
                //GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
                SetEnvironmentInSolution(null);
                SetEnvironmentInSolutionUserFile(null);
                await SetEnvironmentInProjectUserFileAsync(null);
                break;
            case EnvironmentSettingLevel.ProjectUser:
                await SetEnvironmentInProjectUserFileAsync(environment);
                //GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
                SetEnvironmentInSolution(null);
                SetEnvironmentInSolutionUserFile(null);
                await SetEnvironmentInProjectAsync(null);
                break;
        }
    }

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
    private void SetEnvironmentInSolution(DataverseEnvironment environment) => settingsProvider.SolutionSettings.EnvironmentUrl = environment?.Url;
    private void SetEnvironmentInSolutionUserFile(DataverseEnvironment environment) => settingsProvider.SolutionUserSettings.EnvironmentUrl = environment?.Url;
    private async Task SetEnvironmentInProjectAsync(DataverseEnvironment environment) => await settingsProvider.ProjectSettings.SetEnvironmentUrlAsync(environment?.Url);
    private async Task SetEnvironmentInProjectUserFileAsync(DataverseEnvironment environment) => await settingsProvider.ProjectUserSettings.SetEnvironmentUrlAsync(environment?.Url);
}
#nullable restore