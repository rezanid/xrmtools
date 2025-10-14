#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;
using XrmTools.Settings;
using XrmTools.Xrm.Repositories;

internal interface IEnvironmentSelector
{
    Task<DataverseEnvironment?> ChooseEnvironmentAsync(SettingsStorageTypes settingLevel);
}

[Export(typeof(IEnvironmentSelector))]
internal class EnvironmentSelector : IEnvironmentSelector
{
    public event EventHandler<DataverseEnvironment>? EnvironmentChanged;

    [Import]
    ISettingsProvider? SettingsProvider { get; set; }

    [Import]
    ILogger<EnvironmentSelector>? Logger { get; set; }

    [Import]
    IRepositoryFactory? RepositoryFactory { get; set; }

    public async Task<DataverseEnvironment?> ChooseEnvironmentAsync(SettingsStorageTypes settingLevel)
    {
        if (SettingsProvider == null) throw new InvalidOperationException(nameof(SettingsProvider));
        var solutionItem = await FileHelper.FindActiveItemAsync();

        if (solutionItem?.Type != SolutionItemType.Project && solutionItem?.Type != SolutionItemType.Solution)
        {
            Logger.LogWarning("No project or solution is selected");
            return null;
        }

        var environment = await EnvironmentSelectorDialog.ShowDialogAsync(
            settingLevel, SettingsProvider, solutionItem, RepositoryFactory, Logger);
        if (environment != null)
        {
            Logger.LogInformation("Environment selected");
            EnvironmentChanged?.Invoke(this, environment);
            return environment;
        }

        return null;
    }
}
#nullable restore