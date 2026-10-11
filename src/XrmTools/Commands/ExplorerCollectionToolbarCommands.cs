#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using XrmTools.DataverseExplorer.Views;

/// <summary>Native toolbar routing for the active collection session.</summary>
internal abstract class ExplorerCollectionToolbarCommand<T> : BaseCommand<T> where T : class, new()
{
    protected abstract ICommand? Action { get; }
    protected abstract string Label { get; }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        var action = Action;
        Command.Visible = action != null;
        Command.Enabled = action?.CanExecute(null) == true;
        Command.Text = Label;
    }

    protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var action = Action;
        if (action?.CanExecute(null) != true) return Task.CompletedTask;
        if (action is IAsyncRelayCommand asyncAction) return asyncAction.ExecuteAsync(null);
        action.Execute(null);
        return Task.CompletedTask;
    }
}

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.ExecuteExplorerCollectionCmdId)]
internal sealed class ExecuteExplorerCollectionCommand : ExplorerCollectionToolbarCommand<ExecuteExplorerCollectionCommand>
{
    protected override ICommand? Action => DataverseExplorerWindow.Current?.Collection?.ExecuteCommand;
    protected override string Label => DataverseExplorerWindow.Current?.Collection?.ActionLabel ?? "Generate";
}

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.CancelExplorerCollectionCmdId)]
internal sealed class CancelExplorerCollectionCommand : ExplorerCollectionToolbarCommand<CancelExplorerCollectionCommand>
{
    protected override ICommand? Action => DataverseExplorerWindow.Current?.Collection?.CancelCommand;
    protected override string Label => "Cancel collection operation";
}

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.SelectMatchingExplorerCollectionCmdId)]
internal sealed class SelectMatchingExplorerCollectionCommand : ExplorerCollectionToolbarCommand<SelectMatchingExplorerCollectionCommand>
{
    protected override ICommand? Action => DataverseExplorerWindow.Current?.Collection?.SelectMatchingCommand;
    protected override string Label => "Select matching items";
}

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.ClearExplorerCollectionSelectionCmdId)]
internal sealed class ClearExplorerCollectionSelectionCommand : ExplorerCollectionToolbarCommand<ClearExplorerCollectionSelectionCommand>
{
    protected override ICommand? Action => DataverseExplorerWindow.Current?.Collection?.ClearSelectionCommand;
    protected override string Label => "Clear selection";
}
