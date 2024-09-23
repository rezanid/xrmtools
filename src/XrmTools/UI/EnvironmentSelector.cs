#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using XrmTools.Options;

internal interface IEnvironmentSelector
{
    Task<DataverseEnvironment?> ChooseEnvironmentAsync(EnvironmentSettingLevel settingLevel);
}

internal class EnvironmentSelector(ISettingsProvider settingsProvider, ILogger<AssemblySelector> logger) : IEnvironmentSelector
{
    public async Task<DataverseEnvironment?> ChooseEnvironmentAsync(EnvironmentSettingLevel settingLevel)
    {
        var solutionItem = settingLevel == EnvironmentSettingLevel.Options ? null : await VS.Solutions.GetActiveItemAsync();

        if (solutionItem?.Type != SolutionItemType.Project && solutionItem?.Type != SolutionItemType.Solution)
        {
            logger.LogWarning("No project or solution is selected");
            return null;
        }

        var dialog = settingLevel switch 
        {
            EnvironmentSettingLevel.Solution => new EnvironmentSelectorDialog(settingsProvider, solutionItem, false),
            EnvironmentSettingLevel.SolutionUser => new EnvironmentSelectorDialog(settingsProvider, solutionItem, true),
            EnvironmentSettingLevel.Project => new EnvironmentSelectorDialog(settingsProvider, solutionItem, false),
            EnvironmentSettingLevel.ProjectUser => new EnvironmentSelectorDialog(settingsProvider, solutionItem, true),
            _ => throw new ArgumentOutOfRangeException(nameof(settingLevel))
        };
        if (dialog.ShowDialog() == true)
        {
            logger.LogInformation("Environment selected");
            var viewmodel = (EnvironmentSelectorViewModel)dialog.DataContext;
            return viewmodel.Environment;
        }
        return null;
    }
}
#nullable restore