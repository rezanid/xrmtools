namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Community.VisualStudio.Toolkit.DependencyInjection;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using XrmTools.Helpers;
using XrmTools.UI;
using XrmTools.Xrm.Model;
using XrmTools.Xrm;
using System.ComponentModel.Composition;
using XrmTools.Options;

/// <summary>
/// Command handler to select the current environment.
/// </summary>
//TODO: Add the correct command GUID and ID, then wire it up to the package.
[Command(PackageGuids.guidXrmCodeGenPackageCmdSetString, PackageIds.SetXrmPluginGeneratorCommandId)]
internal sealed class SelectEnvironmentCommand(
    DIToolkitPackage package, 
    ILogger<GenerateRegistrationFileCommand> logger, 
    IEnvironmentSelector environmentSelector,
    IEnvironmentProvider environmentProvider) : BaseDICommand(package)
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var activeItem = await FileHelper.FindActiveItemAsync();

        if (activeItem == null)
        {
            await VS.MessageBox.ShowErrorAsync(
                Vsix.Name,
                "Could not determine where to create the new file. Select a file or folder in Solution Explorer and try again.");
            return;
        }

        if (activeItem.Type != SolutionItemType.Solution && activeItem.Type != SolutionItemType.Project)
        {
            await VS.MessageBox.ShowErrorAsync(
                Vsix.Name,
                "Select a project or solution in Solution Explorer and try again.");
            return;
        }

        var options = await GeneralOptions.GetLiveInstanceAsync();
        var environment = await ChooseEnvironmentAsync(options);
        if (environment is not null)
        {
            logger.LogInformation("Environment selected: " + environment);
        }
        else
        {
            logger.LogInformation("No environment selected.");
            return;
        }

        await environmentProvider.SetActiveEnvironmentAsync(environment);
        /*switch (options.EnvironmentSettingLevel)
        {
            case EnvironmentSettingLevel.Solution:
                settingsProvider.SolutionSettings.EnvironmentUrl = environment.Url;
                break;
            case EnvironmentSettingLevel.SolutionUser:
                settingsProvider.SolutionUserSettings.EnvironmentUrl = environment.Url;
                break;
            case EnvironmentSettingLevel.Project:
                await settingsProvider.ProjectSettings.SetEnvironmentUrlAsync(environment.Url);
                break;
            case EnvironmentSettingLevel.ProjectUser:
                await settingsProvider.ProjectUserSettings.SetEnvironmentUrlAsync(environment.Url);
                break;
            case EnvironmentSettingLevel.Options:
                settingsProvider.Options.CurrentEnvironment = environment;
                break;
        }*/
    }

    private async Task<DataverseEnvironment> ChooseEnvironmentAsync(GeneralOptions options)
    {
        try
        {
            return await environmentSelector.ChooseEnvironmentAsync(options.EnvironmentSettingLevel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while choosing environment: {0}" + ex.InnerException?.Message, ex);
            await VS.MessageBox.ShowErrorAsync("Error while choosing environment", ex.Message);
        }
        return null;
    }
}
