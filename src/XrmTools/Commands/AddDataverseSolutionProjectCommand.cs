#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
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
            var initialParentDirectory = Path.GetDirectoryName(solution.FullPath)
                ?? throw new InvalidOperationException("The current solution must be saved before adding a project.");

            var request = await DataverseSolutionProjectDialog.ShowDialogAsync(
                initialParentDirectory,
                RepositoryFactory,
                Package.DisposalToken).ConfigureAwait(false);
            if (request is null)
            {
                return;
            }

            var projectFilePath = await ProjectCreationService.CreateAsync(request, Package.DisposalToken).ConfigureAwait(false);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);
            var dte = await Package.GetServiceAsync(typeof(DTE)) as DTE2;
            if (dte?.Solution is null)
            {
                throw new InvalidOperationException("Visual Studio solution automation is not available.");
            }

            dte.Solution.AddFromFile(projectFilePath, Exclusive: false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Could not create a Dataverse solution project.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, ex.Message);
        }
    }
}
#nullable restore
