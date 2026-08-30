#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Resources;
using XrmTools.Services;
using XrmTools.UI;
using Task = System.Threading.Tasks.Task;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.RegisterWebResourcesCmdId)]
internal sealed class RegisterWebResourcesCommand : BaseCommand<RegisterWebResourcesCommand>
{
    [Import]
    internal IWebResourceRegistrationService RegistrationService { get; set; } = null!;

    [Import]
    internal ILogger<RegisterWebResourcesCommand> Logger { get; set; } = null!;

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var activeItem = await VS.Solutions.GetActiveItemAsync();
        if (activeItem is not Project project) return;

        if (!await project.IsXrmToolsWebResourceProjectAsync())
        {
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "The selected project does not use XrmTools.WebResources.Sdk.");
            return;
        }

        if (!await VS.Build.ProjectIsUpToDateAsync(project))
        {
            Logger.LogInformation("The web-resource project is not up to date. Building it before registration.");
            if (!await project.BuildAsync())
            {
                Logger.LogWarning("Web-resource registration stopped because the project build failed.");
                return;
            }
        }

        var configurationName = await project.GetActiveConfigurationNameAsync();
        await VS.StatusBar.StartAnimationAsync(StatusAnimation.Sync);
        await VS.StatusBar.ShowMessageAsync("Registering web resources...");

        try
        {
            var result = await RegistrationService.RegisterAsync(
                project.FullPath,
                configurationName,
                new VsWebResourceRegistrationUI());
            if (!result.Succeeded)
            {
                Logger.LogWarning("Web-resource registration did not complete: {Message}", result.Message);
                await VS.MessageBox.ShowErrorAsync(Vsix.Name, result.Message);
                await VS.StatusBar.ShowMessageAsync("Web-resource registration failed.");
                return;
            }

            Logger.LogInformation("Web-resource registration completed: {Message}", result.Message);
            await VS.StatusBar.ShowMessageAsync(result.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An unexpected error occurred during web-resource registration.");
            await VS.MessageBox.ShowErrorAsync(
                Vsix.Name,
                "Web-resource registration failed due to an unexpected error. " + ex.Message);
        }
        finally
        {
            await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
        }
    }

    protected override async Task InitializeCompletedAsync()
    {
        try
        {
            var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
            componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
            EnsureDependencies();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "An error occurred while initializing the web-resource registration command.");
            await VS.MessageBox.ShowErrorAsync(
                Vsix.Name,
                "An error occurred while initializing the web-resource registration command. " + ex.Message);
        }
    }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            var item = await VS.Solutions.GetActiveItemAsync();
            if (item is not Project project)
            {
                Command.Visible = false;
                return;
            }

            var fastPath = UIContext.FromUIContextGuid(PackageGuids.XrmToolsWebResourceProjectUIRule)?.IsActive is true;
            Command.Visible = fastPath || await project.IsXrmToolsWebResourceProjectAsync().ConfigureAwait(false);
        });
    }

    [MemberNotNull(nameof(RegistrationService), nameof(Logger))]
    private void EnsureDependencies()
    {
        if (RegistrationService == null)
            throw new InvalidOperationException(string.Format(
                Strings.MissingServiceDependency,
                nameof(RegisterWebResourcesCommand),
                nameof(RegistrationService)));
        if (Logger == null)
            throw new InvalidOperationException(string.Format(
                Strings.MissingServiceDependency,
                nameof(RegisterWebResourcesCommand),
                nameof(Logger)));
    }
}
#nullable restore
