namespace XrmTools.Commands;

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;
using System;
using XrmTools.Helpers;
using XrmTools.UI;
using XrmTools.Options;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ComponentModelHost;
using XrmTools.Logging.Compatibility;
using XrmTools.Environments;

/// <summary>
/// Command handler to select the current environment.
/// </summary>
[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.SetEnvironmentCmdId)]
internal sealed class SelectEnvironmentCommand : BaseCommand<SelectEnvironmentCommand>
{
    [Import]
    public ILogger<NewPluginDefinitionFileCommand> Logger {  get; set; }
    [Import]
    public IEnvironmentSelector EnvironmentSelector { get; set; }
    [Import]
    public IEnvironmentProvider EnvironmentProvider {  get; set; }

    protected override async Task InitializeCompletedAsync()
    {
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>();
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
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
            Logger.LogInformation("Environment selected: " + environment);
        }
        else
        {
            Logger.LogInformation("No environment selected.");
            return;
        }

        await EnvironmentProvider.SetActiveEnvironmentAsync(environment);
    }

    private async Task<DataverseEnvironment> ChooseEnvironmentAsync(GeneralOptions options)
    {
        try
        {
            return await EnvironmentSelector.ChooseEnvironmentAsync(options.CurrentEnvironmentStorage);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while choosing environment: {0}" + ex.InnerException?.Message, ex);
            await VS.MessageBox.ShowErrorAsync("Error while choosing environment", ex.Message);
        }
        return null;
    }
}
