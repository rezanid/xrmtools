#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.Analyzers;
using XrmTools.Environments;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Services;
using XrmTools.UI;
using XrmTools.WebApi;
using XrmTools.Xrm.Repositories;
using System.Diagnostics.CodeAnalysis;
using XrmTools.Resources;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.UnregisterPluginCmdId)]
internal sealed class UnregisterCommand : BaseCommand<UnregisterCommand>
{
    [Import]
    internal IWebApiService WebApiService { get; set; } = null!;

    [Import]
    internal IEnvironmentProvider EnvironmentProvider { get; set; } = null!;

    [Import]
    internal IXrmMetaDataService MetaDataService { get; set; } = null!;

    [Import]
    internal IRepositoryFactory RepositoryFactory { get; set; } = null!;

    [Import]
    internal ILogger<RegisterPluginCommand> Logger { get; set; } = null!;

    [Import]
    internal IPluginRegistrationService PluginRegistrationService { get; set; } = null!;

    override protected async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var activeItem = await VS.Solutions.GetActiveItemAsync();
        if (activeItem is null || activeItem.FullPath is null || !(activeItem.Type is SolutionItemType.Project or SolutionItemType.PhysicalFile)) return;

        var project = activeItem.Type == SolutionItemType.Project ? (Project)activeItem : activeItem.FindParent(SolutionItemType.Project) as Project;
        if (project is null)
        {
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "The selected item is not a project or part of a project.");
            return;
        }

        var ui = new VsPluginRegistrationUI();
        var confirmed = await ui.ConfirmUnregsiterAssemblyAsync(project.Name);
        if (!confirmed) return;

        await VS.StatusBar.StartAnimationAsync(StatusAnimation.General);
        await VS.StatusBar.ShowMessageAsync("Unregistering from Dataverse...");

        try
        {
            var input = new RegistrationInput(
                itemFullPath: activeItem.FullPath,
                isProject: true,
                nugetPackagePath: null);

            var result = await PluginRegistrationService!.UnregisterAsync(input, ui);

            if (!result.Succeeded)
            {
                await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
                await VS.StatusBar.ShowMessageAsync("Unregistration failed.");
                await VS.MessageBox.ShowErrorAsync(Vsix.Name, result.Message);
                return;
            }

            await VS.StatusBar.ShowMessageAsync(result.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An unexpected error occurred during plugin registration.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "Unregistration failed due to an unexpected error. " + ex.Message);
        }
        finally
        {
            await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
        }
    }


    protected override async Task InitializeCompletedAsync()
    {
        //Command.Supported = false;
        try
        {
            var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
            componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
            EnsureDependencies();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "An error occurred while initializing the RegisterPluginCommand.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "An error occurred while initializing the RegisterPluginCommand. " + ex.Message);
            return;
        }
    }

    [MemberNotNull(nameof(Logger), nameof(MetaDataService), nameof(WebApiService),
        nameof(EnvironmentProvider), nameof(RepositoryFactory))]
    private void EnsureDependencies()
    {
        if (Logger == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(Logger)));
        if (MetaDataService == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(MetaDataService)));
        if (WebApiService == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(WebApiService)));
        if (EnvironmentProvider == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(EnvironmentProvider)));
        if (RepositoryFactory == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(RepositoryFactory)));
        if (PluginRegistrationService == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(PluginRegistrationService)));
    }
}
#nullable restore