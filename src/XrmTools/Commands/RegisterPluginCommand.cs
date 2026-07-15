#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using XrmTools.Analyzers;
using XrmTools.Environments;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Resources;
using XrmTools.Services;
using XrmTools.UI;
using XrmTools.WebApi;
using XrmTools.Xrm.Repositories;
using static XrmTools.Helpers.ProjectExtensions;
using Task = System.Threading.Tasks.Task;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.RegisterPluginCmdId)]
internal sealed class RegisterPluginCommand : BaseCommand<RegisterPluginCommand>
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

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var activeItem = await VS.Solutions.GetActiveItemAsync();
        if (activeItem is null || activeItem.FullPath is null || !(activeItem.Type is SolutionItemType.Project or SolutionItemType.PhysicalFile)) return;

        var project = activeItem.Type == SolutionItemType.Project ? (Project)activeItem : activeItem.FindParent(SolutionItemType.Project) as Project;
        if (project is null)
        {
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "The selected item is not a project or part of a project.");
            return;
        }

        var projectIsUpToDate = await VS.Build.ProjectIsUpToDateAsync(project);
        if (!projectIsUpToDate)
        {
            var buildSucceeded = await project.BuildAsync();
            if (!buildSucceeded) return;
        }

        var generatePackage = project.GetBuildProperty<bool>(BuildProperties.GeneratePackageOnBuild);
        var nugetFilePath = generatePackage ? Path.Combine(project.FullPath, project.GetOutputPackagePath()) : null;

        await VS.StatusBar.StartAnimationAsync(StatusAnimation.General);
        await VS.StatusBar.ShowMessageAsync("Registering plugin(s)...");

        try
        {
            var input = new RegistrationInput(
                itemFullPath: activeItem.FullPath,
                isProject: activeItem.Type == SolutionItemType.Project,
                nugetPackagePath: nugetFilePath);

            var ui = new VsPluginRegistrationUI();
            var result = await PluginRegistrationService!.RegisterAsync(input, ui);

            if (!result.Succeeded)
            {
                await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
                await VS.StatusBar.ShowMessageAsync("Plugin registration failed.");
                await VS.MessageBox.ShowErrorAsync(Vsix.Name, result.Message);
                return;
            }

            await VS.StatusBar.ShowMessageAsync(result.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An unexpected error occurred during plugin registration.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "Plugin registration failed due to an unexpected error. " + ex.Message);
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
            var item = await VS.Solutions.GetActiveItemAsync();
            Command.Visible = await IsVisibleAsync(item, uiContext?.IsActive is true).ConfigureAwait(false);
        });     

        static async Task<bool> IsVisibleAsync(SolutionItem? item, bool isUiContextActive)
        {
            if (item is null)
                return false;

            if (item.Type == SolutionItemType.Project && item is Project project)
                return isUiContextActive || await project.IsXrmToolsPluginProjectAsync().ConfigureAwait(false);

            if (item.Type == SolutionItemType.PhysicalFile && item is PhysicalFile file)
                return await file.IsXrmPluginFileAsync().ConfigureAwait(false);

            return false;
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