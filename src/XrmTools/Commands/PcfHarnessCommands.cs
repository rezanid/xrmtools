#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using XrmTools.Logging.Compatibility;
using XrmTools.Pcf;

internal abstract class PcfHarnessCommand<TCommand> : BaseCommand<TCommand> where TCommand : class, new()
{
    protected abstract bool StopHarness { get; }

    [Import]
    internal ILogger<TCommand> Logger { get; set; } = null!;

    protected override async Task InitializeCompletedAsync()
    {
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
    }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var session = GetSession();
        Command.Visible = session is not null;
        Command.Enabled = session is not null && (StopHarness ? session.IsRunning : !session.IsRunning);
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        try
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);
            var session = GetSession() ?? throw new InvalidOperationException("Select a loaded PCF project.");
            if (StopHarness) session.Stop();
            else await session.StartAsync(false, Package.DisposalToken);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Could not change the PCF local harness session.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, ex.Message);
        }
    }

    private PcfHarnessSession? GetSession()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var dte = (DTE?)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE));
        if (dte is null || dte.SelectedItems.Count != 1) return null;
        var selected = dte.SelectedItems.Item(1);
        var project = selected.Project ?? selected.ProjectItem?.ContainingProject;
        if (project is null || !string.Equals(Path.GetExtension(project.FullName), ".pcfproj", StringComparison.OrdinalIgnoreCase)) return null;
        var context = project as IVsBrowseObjectContext ?? project.Object as IVsBrowseObjectContext;
        return context?.UnconfiguredProject.Services.ExportProvider.GetExportedValue<PcfHarnessSession>();
    }
}

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.StartPcfHarnessCmdId)]
internal sealed class StartPcfHarnessCommand : PcfHarnessCommand<StartPcfHarnessCommand>
{
    protected override bool StopHarness => false;
}

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.StopPcfHarnessCmdId)]
internal sealed class StopPcfHarnessCommand : PcfHarnessCommand<StopPcfHarnessCommand>
{
    protected override bool StopHarness => true;
}
#nullable restore
