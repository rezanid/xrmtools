#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using System.Collections.Generic;
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
