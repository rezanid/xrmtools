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
using XrmTools.Options;

/// <summary>
/// Command handler to select the current environment.
/// </summary>
[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.SetEnvironmentCmdId)]
internal sealed class SelectEnvironmentCommand : BaseDICommand
{
    private readonly ILogger<NewPluginDefinitionFileCommand> logger;
    private readonly IEnvironmentSelector environmentSelector;
    private readonly IEnvironmentProvider environmentProvider;

    public SelectEnvironmentCommand(
        DIToolkitPackage package, 
        ILogger<NewPluginDefinitionFileCommand> logger, 
        IEnvironmentSelector environmentSelector,
        IEnvironmentProvider environmentProvider) : base(package)
    {
        this.logger = logger;
        this.environmentSelector = environmentSelector;
        this.environmentProvider = environmentProvider;
    }

    protected override void BeforeQueryStatus(EventArgs e)
        => ThreadHelper.JoinableTaskFactory.Run(SetCommandVisibilityAsync);

    private async Task SetCommandVisibilityAsync()
    {
        var activeItem = await VS.Solutions.GetActiveItemAsync();
        var options = await GeneralOptions.GetLiveInstanceAsync();
        Command.Visible = activeItem.Type switch
        {
            SolutionItemType.Solution => options.CurrentEnvironmentStorage is SettingsStorageTypes.Solution or SettingsStorageTypes.SolutionUser,
            SolutionItemType.Project => options.CurrentEnvironmentStorage is SettingsStorageTypes.Project or SettingsStorageTypes.ProjectUser,
            _ => false
        };
    }

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
    }

    private async Task<DataverseEnvironment> ChooseEnvironmentAsync(GeneralOptions options)
    {
        try
        {
            return await environmentSelector.ChooseEnvironmentAsync(options.CurrentEnvironmentStorage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while choosing environment: {0}" + ex.InnerException?.Message, ex);
            await VS.MessageBox.ShowErrorAsync("Error while choosing environment", ex.Message);
        }
        return null;
    }
}
