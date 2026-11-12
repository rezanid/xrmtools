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

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.AddPcfProjectCmdId)]
internal sealed class AddPcfProjectCommand : BaseCommand<AddPcfProjectCommand>
{
    [Import] internal IPcfProjectService Service { get; set; } = null!;
    [Import] internal ILogger<AddPcfProjectCommand> Logger { get; set; } = null!;
    protected override async Task InitializeCompletedAsync()
    {
        var model = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        model?.DefaultCompositionService.SatisfyImportsOnce(this);
    }
    protected override void BeforeQueryStatus(EventArgs e) => Command.Enabled = !Service.IsBusy;
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        try
        {
            var solution = await VS.Solutions.GetCurrentSolutionAsync().ConfigureAwait(false)
                ?? throw new InvalidOperationException("Open a solution before adding a PCF project.");
            var directory = Path.GetDirectoryName(solution.FullPath);
            if (string.IsNullOrEmpty(directory)) throw new InvalidOperationException("Save the solution before adding a PCF project.");
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);
            var dteService = await Package.GetServiceAsync(typeof(DTE));
            if (dteService is null) throw new InvalidOperationException("Visual Studio automation is unavailable.");
            if (dteService is not DTE2 dte || dte.Solution is null) throw new InvalidOperationException("Visual Studio solution automation is unavailable.");
            string? folder = null;
            foreach (SelectedItem selected in dte.SelectedItems)
            {
                if (selected.Project?.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}") { folder = selected.Project.UniqueName; break; }
            }
            var path = await PcfCreateDialog.ShowAsync(directory, async (request, token) =>
            {
                try { return await Service.CreateAsync(request, token).ConfigureAwait(false); }
                catch (Exception ex) { Logger.LogError(ex, "PCF project creation failed."); throw; }
            }, Package.DisposalToken).ConfigureAwait(false);
            if (path is null) return;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);
            try
            {
                if (folder is not null)
                {
                    var solutionService = await Package.GetServiceAsync<SVsSolution, IVsSolution>().ConfigureAwait(true);
                    if (solutionService is not IVsSolution6 service6) throw new InvalidOperationException($"Could not add the project to the selected solution folder. Add '{path}' manually.");
                    ErrorHandler.ThrowOnFailure(solutionService.GetProjectOfUniqueName(folder, out var hierarchy));
                    ErrorHandler.ThrowOnFailure(service6.AddExistingProject(path, hierarchy, out _));
                }
                else dte.Solution.AddFromFile(path, Exclusive: false);
            }
            catch (Exception ex) { throw new InvalidOperationException($"Project was created at '{path}' but could not be added to the solution. Add it manually. {ex.Message}", ex); }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Logger.LogError(ex, "Could not add PCF project."); await VS.MessageBox.ShowErrorAsync(Vsix.Name, ex.Message); }
        finally
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Command.Enabled = !Service.IsBusy;
            if (await Package.GetServiceAsync(typeof(SVsUIShell)) is IVsUIShell shell) shell.UpdateCommandUI(0);
        }
    }
}
#nullable restore
