#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using XrmTools.DataverseSolutions;
using XrmTools.Logging.Compatibility;
using XrmTools.UI;
using XrmTools.Xrm.Repositories;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.AddDataverseSolutionProjectCmdId)]
internal sealed class AddDataverseSolutionProjectCommand : BaseCommand<AddDataverseSolutionProjectCommand>
{
    [Import]
    internal IDataverseSolutionProjectCreationService ProjectCreationService { get; set; } = null!;

    [Import]
    internal IRepositoryFactory RepositoryFactory { get; set; } = null!;

    [Import]
    internal ILogger<AddDataverseSolutionProjectCommand> Logger { get; set; } = null!;

    protected override async Task InitializeCompletedAsync()
    {
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
    }

    protected override void BeforeQueryStatus(EventArgs e)
        => Command.Enabled = !ProjectCreationService.IsBusy;

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        try
        {
            var solution = await VS.Solutions.GetCurrentSolutionAsync().ConfigureAwait(false)
                ?? throw new InvalidOperationException("Open a solution before adding a Dataverse solution project.");
            var solutionDir = Path.GetDirectoryName(solution.FullPath)
                ?? throw new InvalidOperationException("The current solution must be saved before adding a project.");

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);
            var dte = await Package.GetServiceAsync(typeof(DTE)) as DTE2;
            if (dte?.Solution is null)
            {
                throw new InvalidOperationException("Visual Studio solution automation is not available.");
            }

            string? selectedSolutionFolderUniqueName = null;
            foreach (SelectedItem selectedItem in dte.SelectedItems)
            {
                var project = selectedItem.Project;
                if (project?.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")
                {
                    selectedSolutionFolderUniqueName = project.UniqueName;
                    break;
                }
            }

            var projectFilePath = await DataverseSolutionProjectDialog.ShowDialogAsync(
                solutionDir,
                RepositoryFactory,
                async (request, token) =>
                {
                    try { return await ProjectCreationService.CreateAsync(request, token).ConfigureAwait(false); }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Could not create a Dataverse solution project.");
                        throw;
                    }
                },
                Package.DisposalToken).ConfigureAwait(false);
            if (projectFilePath is null)
            {
                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);

            if (!string.IsNullOrEmpty(selectedSolutionFolderUniqueName))
            {
                var solutionService = await Package.GetServiceAsync<SVsSolution, IVsSolution>().ConfigureAwait(true);
                if (solutionService is not IVsSolution6 solutionService6)
                {
                    throw new InvalidOperationException("Visual Studio solution hierarchy automation is not available.");
                }

                ErrorHandler.ThrowOnFailure(solutionService.GetProjectOfUniqueName(selectedSolutionFolderUniqueName, out var parentHierarchy));
                ErrorHandler.ThrowOnFailure(solutionService6.AddExistingProject(projectFilePath, parentHierarchy, out _));
            }
            else
            {
                dte.Solution.AddFromFile(projectFilePath, Exclusive: false);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Could not create a Dataverse solution project.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, ex.Message);
        }
        finally
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Command.Enabled = !ProjectCreationService.IsBusy;
            if (await Package.GetServiceAsync(typeof(SVsUIShell)) is IVsUIShell shell)
                shell.UpdateCommandUI(0);
        }
    }
}
#nullable restore
