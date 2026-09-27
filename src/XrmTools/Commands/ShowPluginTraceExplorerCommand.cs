namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Threading.Tasks;
using XrmTools.PluginTrace;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.ShowPluginTraceExplorerCmdId)]
internal sealed class ShowPluginTraceExplorerCommand : BaseCommand<ShowPluginTraceExplorerCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        try
        {
            var window = await Package.FindWindowPaneAsync(typeof(TraceExplorerWindow), 0, true, Package.DisposalToken);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);
            if (window is not TraceExplorerWindow explorer || explorer.Frame is not IVsWindowFrame frame)
                throw new InvalidOperationException("Visual Studio did not create the Plugin Trace Explorer window.");
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(frame.Show());
        }
        catch (OperationCanceledException) when (Package.DisposalToken.IsCancellationRequested) { }
        catch (Exception ex)
        {
            await VS.MessageBox.ShowErrorAsync("Plugin Trace Explorer", "Could not open the window.\n\n" + ex.GetBaseException().Message);
        }
    }
}
