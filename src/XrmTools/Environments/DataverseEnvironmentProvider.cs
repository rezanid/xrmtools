#nullable enable
namespace XrmGen;

using System.Linq;
using System.Threading.Tasks;
using XrmGen.Options;
using Community.VisualStudio.Toolkit;

public class EnvironmentProvider(XrmGenPackage package)
{
    public async Task<DataverseEnvironment?> GetActiveEnvironmentAsync()
        => GeneralOptions.Instance.EnvironmentSettingLevel switch
        {
            EnvironmentSettingLevel.Options => GeneralOptions.Instance.CurrentEnvironment,
            EnvironmentSettingLevel.Solution => GetEnvironmentFromSolution(),
            EnvironmentSettingLevel.SolutionUser => GetEnvironmentFromSolutionUserFile(),
            EnvironmentSettingLevel.Project => await GetEnvironmentFromProjectAsync().ConfigureAwait(false),
            EnvironmentSettingLevel.ProjectUser => await GetEnvironmentFromProjectUserFileAsync().ConfigureAwait(false),
            _ => GeneralOptions.Instance.CurrentEnvironment,
        };

    public async Task SetActiveEnvironmentAsync(DataverseEnvironment environment)
    {
        switch (GeneralOptions.Instance.EnvironmentSettingLevel)
        {
            case EnvironmentSettingLevel.Options:
                GeneralOptions.Instance.CurrentEnvironment = environment;
                SetEnvironmentInSolution(null);
                SetEnvironmentInSolutionUserFile(null);
                await SetEnvironmentInProjectAsync(null);
                await SetEnvironmentInProjectUserFileAsync(null);
                break;
            case EnvironmentSettingLevel.Solution:
                SetEnvironmentInSolution(environment);
                GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
                SetEnvironmentInSolutionUserFile(null);
                await SetEnvironmentInProjectAsync(null);
                await SetEnvironmentInProjectUserFileAsync(null);
                break;
            case EnvironmentSettingLevel.SolutionUser:
                SetEnvironmentInSolutionUserFile(environment);
                GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
                SetEnvironmentInSolution(null);
                await SetEnvironmentInProjectAsync(null);
                await SetEnvironmentInProjectUserFileAsync(null);
                break;
            case EnvironmentSettingLevel.Project:
                await SetEnvironmentInProjectAsync(environment);
                GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
                SetEnvironmentInSolution(null);
                SetEnvironmentInSolutionUserFile(null);
                await SetEnvironmentInProjectUserFileAsync(null);
                break;
            case EnvironmentSettingLevel.ProjectUser:
                await SetEnvironmentInProjectUserFileAsync(environment);
                GeneralOptions.Instance.CurrentEnvironment = new DataverseEnvironment();
                SetEnvironmentInSolution(null);
                SetEnvironmentInSolutionUserFile(null);
                await SetEnvironmentInProjectAsync(null);
                break;
        }
    }

    private DataverseEnvironment? GetEnvironmentFromSolution()
    {
        var url = package.DataverseUrlSetting;
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private void SetEnvironmentInSolution(DataverseEnvironment environment) => package.DataverseUrlSetting = environment?.Url;
    private DataverseEnvironment? GetEnvironmentFromSolutionUserFile()
    {
        var url = package.DataverseUrlOption;
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private void SetEnvironmentInSolutionUserFile(DataverseEnvironment environment) 
        => package.DataverseUrlOption = environment?.Url;
    private async Task<DataverseEnvironment?> GetEnvironmentFromProjectAsync()
    {
        var project = await VS.Solutions.GetActiveProjectAsync();
        if (project == null) return null;
        var url = await project.GetAttributeAsync("DataverseUrl", ProjectStorageType.ProjectFile);
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private async Task SetEnvironmentInProjectAsync(DataverseEnvironment environment)
    {
        var project = await VS.Solutions.GetActiveProjectAsync();
        if (project is null) return;
        if (environment?.Url is null)
        {
            await project.RemoveAttributeAsync("DataverseUrl", ProjectStorageType.ProjectFile);
            return;
        }
        await project.TrySetAttributeAsync("DataverseUrl", environment.Url, ProjectStorageType.ProjectFile);
    }
    private async Task<DataverseEnvironment?> GetEnvironmentFromProjectUserFileAsync()
    {
        var project = await VS.Solutions.GetActiveProjectAsync();
        if (project == null) return null;
        var url = await project.GetAttributeAsync("DataverseUrl", ProjectStorageType.UserFile);
        if (string.IsNullOrWhiteSpace(url)) return null;
        return GeneralOptions.Instance.Environments.FirstOrDefault(e => e.Url == url);
    }
    private async Task SetEnvironmentInProjectUserFileAsync(DataverseEnvironment environment)
    {
        var project = await VS.Solutions.GetActiveProjectAsync();
        if (project is null) return;
        if (environment?.Url is null)
        {
            await project.RemoveAttributeAsync("DataverseUrl", ProjectStorageType.UserFile);
            return;
        }
        await project.TrySetAttributeAsync("DataverseUrl", environment.Url, ProjectStorageType.UserFile);
    }
}
#nullable restore