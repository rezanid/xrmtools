#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Threading;
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

        var input = new RegistrationInput(activeItem.FullPath, isProject: true, nugetPackagePath: null);
        await RunUnregisterAsync(() => PluginRegistrationService.UnregisterAsync(input, ui), Logger);
    }

    internal static async Task<bool> ExecuteTargetAsync(EntityReference target, string displayName,
        IPluginRegistrationService service, ILogger logger, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var kind = target.SetName == "pluginpackages" ? "package" : "assembly";
        var confirmed = await VS.MessageBox.ShowConfirmAsync(Vsix.Name,
            $"Are you sure you want to unregister the plugin {kind} '{displayName}' from Dataverse? Its plugin registrations will also be removed.");
        if (!confirmed || cancellationToken.IsCancellationRequested) return false;
        return await RunUnregisterAsync(() => service.UnregisterAsync(target, cancellationToken), logger);
    }

    private static async Task<bool> RunUnregisterAsync(Func<Task<PluginRegistrationResult>> unregister, ILogger logger)
    {
        await VS.StatusBar.StartAnimationAsync(StatusAnimation.General);
        await VS.StatusBar.ShowMessageAsync("Unregistering from Dataverse...");
        try
        {
            var result = await unregister();
            await VS.StatusBar.ShowMessageAsync(result.Succeeded ? result.Message : "Unregistration failed.");
            if (!result.Succeeded) await VS.MessageBox.ShowErrorAsync(Vsix.Name, result.Message);
            return result.Succeeded;
        }
        catch (OperationCanceledException)
        {
            await VS.StatusBar.ShowMessageAsync("Unregistration cancelled. Refresh the explorer to verify the current registration state.");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during plugin unregistration.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "Unregistration failed due to an unexpected error. " + ex.Message);
            return false;
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

    protected override void BeforeQueryStatus(EventArgs e)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            var uiContext = UIContext.FromUIContextGuid(PackageGuids.XrmToolsPluginProjectUIRule);
            var project = await VS.Solutions.GetActiveProjectAsync();
            Command.Visible = project is not null
                && (uiContext?.IsActive is true || await project.IsXrmToolsPluginProjectAsync().ConfigureAwait(false));
        });
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
