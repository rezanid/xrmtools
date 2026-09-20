#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Commands;
using XrmTools.DataverseExplorer.Models;
using XrmTools.Logging.Compatibility;
using XrmTools.Services;
using XrmTools.WebApi;

[Export(typeof(IExplorerCommandProvider))]
internal sealed class UnregisterExplorerCommandProvider : IExplorerCommandProvider
{
    private readonly IPluginRegistrationService _service;
    private readonly ILogger _logger;
    private readonly AsyncRelayCommand<UnregisterExplorerTarget> _command;

    [ImportingConstructor]
    public UnregisterExplorerCommandProvider(IPluginRegistrationService service, ILogger<UnregisterCommand> logger)
    {
        _service = service;
        _logger = logger;
        _command = new AsyncRelayCommand<UnregisterExplorerTarget>(ExecuteAsync,
            target => target != null && !target.CancellationToken.IsCancellationRequested);
    }

    public IEnumerable<ExplorerMenuItem> GetCommands(ExplorerNodeBase node)
    {
        var reference = node switch
        {
            PackageNode package when package.PackageId != Guid.Empty => new EntityReference("pluginpackages", package.PackageId),
            AssemblyNode assembly when assembly.AssemblyId != Guid.Empty && assembly.Parent is CategoryNode { ArtifactCategory: "Assemblies" }
                => new EntityReference("pluginassemblies", assembly.AssemblyId),
            _ => null,
        };
        if (reference == null) yield break;
        yield return new ExplorerMenuItem
        {
            Header = "Unregister from Dataverse",
            Command = _command,
            CommandParameter = new UnregisterExplorerTarget(reference, node.DisplayName, node.SessionToken),
        };
    }

    private async Task ExecuteAsync(UnregisterExplorerTarget? target)
    {
        if (target == null || target.CancellationToken.IsCancellationRequested) return;
        try
        {
            await UnregisterCommand.ExecuteTargetAsync(target.Reference, target.DisplayName, _service, _logger, target.CancellationToken);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing explorer unregistration.");
        }
    }
}

internal sealed class UnregisterExplorerTarget(EntityReference reference, string displayName, CancellationToken cancellationToken)
{
    public EntityReference Reference { get; } = reference;
    public string DisplayName { get; } = displayName;
    public CancellationToken CancellationToken { get; } = cancellationToken;
}
