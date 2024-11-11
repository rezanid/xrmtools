#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;
using XrmTools.Settings;

internal interface IEnvironmentSelector
{
    Task<DataverseEnvironment?> ChooseEnvironmentAsync(SettingsStorageTypes settingLevel);
}

[Export(typeof(IEnvironmentSelector))]
internal class EnvironmentSelector : IEnvironmentSelector
{
    [Import]
    ISettingsProvider? SettingsProvider { get; set; }

    [Import]
    ILogger<EnvironmentSelector>? Logger { get; set; }

    public async Task<DataverseEnvironment?> ChooseEnvironmentAsync(SettingsStorageTypes settingLevel)
    {
        var solutionItem = await FileHelper.FindActiveItemAsync();

        if (solutionItem?.Type != SolutionItemType.Project && solutionItem?.Type != SolutionItemType.Solution)
        {
            Logger.LogWarning("No project or solution is selected");
            return null;
        }

        var dialog = new EnvironmentSelectorDialog(settingLevel, SettingsProvider, solutionItem);
        if (dialog == null)
        {
            Logger.LogWarning("Environment level is not set to Solution or Project.");
            await VS.MessageBox.ShowAsync(
                "Environment level not selected", 
                "Please select an environment level other than Visual Studio in Tools > Options > Xrm Tools.", 
                Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING);
            return null;
        }
        if (dialog.ShowDialog() == true)
        {
            Logger.LogInformation("Environment selected");
            var viewmodel = (EnvironmentSelectorViewModel)dialog.DataContext;
            return viewmodel.Environment;
        }
        return null;
    }
}
#nullable restore