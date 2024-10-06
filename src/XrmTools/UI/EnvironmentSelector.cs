#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Options;

internal interface IEnvironmentSelector
{
    Task<DataverseEnvironment?> ChooseEnvironmentAsync(SettingsStorageTypes settingLevel);
}

internal class EnvironmentSelector(ISettingsProvider settingsProvider, ILogger<AssemblySelector> logger) : IEnvironmentSelector
{
    public async Task<DataverseEnvironment?> ChooseEnvironmentAsync(SettingsStorageTypes settingLevel)
    {
        var solutionItem = await FileHelper.FindActiveItemAsync();

        if (solutionItem?.Type != SolutionItemType.Project && solutionItem?.Type != SolutionItemType.Solution)
        {
            logger.LogWarning("No project or solution is selected");
            return null;
        }

        var dialog = new EnvironmentSelectorDialog(settingLevel, settingsProvider, solutionItem);
        if (dialog == null)
        {
            logger.LogWarning("Environment level is not set to Solution or Project.");
            await VS.MessageBox.ShowAsync(
                "Environment level not selected", 
                "Please select an environment level other than Visual Studio in Tools > Options > Xrm Tools.", 
                Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING);
            return null;
        }
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