#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using System.Collections.Generic;
using System.ComponentModel.Composition;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using XrmTools.DataverseExplorer.Models;

/// <summary>Contributes menu entries for an actual tree node, including during search.</summary>
internal interface IExplorerCommandProvider
{
    IEnumerable<ExplorerMenuItem> GetCommands(ExplorerNodeBase node);
}

internal sealed class ExplorerMenuItem
{
    public string Header { get; init; } = string.Empty;
    public string? ToolTip { get; init; }
    public bool IsEnabled { get; init; } = true;
    public ICommand? Command { get; init; }
    public object? CommandParameter { get; init; }
    public IReadOnlyList<ExplorerMenuItem> Children { get; init; } = [];
}

[Export(typeof(IExplorerCommandProvider))]
internal sealed class RefreshExplorerCommandProvider : IExplorerCommandProvider
{
    public IEnumerable<ExplorerMenuItem> GetCommands(ExplorerNodeBase node)
    {
        if (node.RefreshAsync == null || node.ReloadAsync == null) yield break;
        if (node is not (CategoryNode { Parent: null } or PackageNode or TableNode or AssemblyNode { Parent: CategoryNode { ArtifactCategory: "Assemblies" } })) yield break;
        yield return new ExplorerMenuItem
        {
            Header = "Refresh",
            Command = new AsyncRelayCommand(node.RefreshAsync, () => !node.SessionToken.IsCancellationRequested),
        };
    }
}
