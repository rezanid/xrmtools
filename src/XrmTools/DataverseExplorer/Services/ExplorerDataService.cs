#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Repositories;
using XrmTools.DataverseExplorer.Models;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model.Configuration;
using XrmTools.Xrm.Repositories;

/// <summary>
/// Implementation of the explorer data service.
/// Manages data loading from repositories and maintains a simple in-memory index.
/// </summary>
[Export(typeof(IExplorerDataService))]
internal sealed class ExplorerDataService : IExplorerDataService
{
    private readonly IPluginAssemblyRepository _assemblyRepository;
    private readonly IPluginTypeRepository _pluginTypeRepository;
    private readonly ILogger _logger;
    private readonly Dictionary<Guid, AssemblyNode> _assemblyCache = [];
    private bool _assembliesLoaded;

    [ImportingConstructor]
    public ExplorerDataService(
        [Import] IRepositoryFactory repositoryFactory,
        [Import] ILogger<IExplorerDataService> logger)
    {
        _assemblyRepository = repositoryFactory.CreateRepository<IPluginAssemblyRepository>();
        _pluginTypeRepository = repositoryFactory.CreateRepository<IPluginTypeRepository>();
        _logger = logger;
    }

    public async Task<IEnumerable<AssemblyNode>> LoadAssembliesAsync(CancellationToken cancellationToken)
    {
        if (_assembliesLoaded)
        {
            return _assemblyCache.Values;
        }

        try
        {
            var configs = await _assemblyRepository.GetAsync(cancellationToken);
            _assemblyCache.Clear();

            foreach (var config in configs)
            {
                var assemblyId = config.Id ?? Guid.Empty;
                var node = new AssemblyNode
                {
                    Id = assemblyId.ToString(),
                    AssemblyId = assemblyId,
                    DisplayName = config.Name ?? "Unknown Assembly",
                    Description = string.Empty,
                    PublicKeyToken = config.PublicKeyToken,
                    Version = config.Version,
                    IsolationMode = config.IsolationMode?.ToString(),
                    SourceType = config.SourceType?.ToString(),
                    AreChildrenLoaded = false
                };
                _assemblyCache[assemblyId] = node;
            }

            _assembliesLoaded = true;
            return _assemblyCache.Values;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assemblies from Dataverse");
            throw;
        }
    }

    public async Task<IEnumerable<ExplorerNodeBase>> LoadAssemblyChildrenAsync(
        AssemblyNode assembly, CancellationToken cancellationToken)
    {
        if (assembly.AreChildrenLoaded)
        {
            return assembly.Children;
        }

        try
        {
            var pluginTypes = await _pluginTypeRepository.GetAsync(assembly.AssemblyId, cancellationToken);
            assembly.Children.Clear();

            foreach (var config in pluginTypes)
            {
                var pluginTypeId = config.Id ?? Guid.Empty;
                var typeNode = new PluginTypeNode
                {
                    Id = pluginTypeId.ToString(),
                    PluginTypeId = pluginTypeId,
                    DisplayName = config.FriendlyName ?? config.TypeName ?? "Unknown Type",
                    Description = config.Description ?? string.Empty,
                    TypeName = config.TypeName,
                    FriendlyName = config.FriendlyName,
                    WorkflowActivityGroupName = config.WorkflowActivityGroupName,
                    Parent = assembly,
                    AreChildrenLoaded = false
                };

                // Add steps if they exist
                if (config.Steps is { Count: > 0 })
                {
                    foreach (var step in config.Steps)
                    {
                        var stepId = step.Id ?? Guid.Empty;
                        var stepNode = new PluginStepNode
                        {
                            Id = stepId.ToString(),
                            StepId = stepId,
                            DisplayName = step.Name ?? "Unknown Step",
                            Description = step.Description ?? string.Empty,
                            Stage = step.Stage?.ToString(),
                            Mode = step.Mode?.ToString(),
                            Rank = step.Rank,
                            SdkMessageId = step.Message?.Id?.ToString(),
                            StateCode = step.StateCode?.ToString(),
                            AsyncAutoDelete = step.AsyncAutoDelete,
                            FilteringAttributes = step.FilteringAttributes,
                            InvocationSource = step.InvocationSource?.ToString(),
                            SupportedDeployment = step.SupportedDeployment?.ToString(),
                            Parent = typeNode,
                            AreChildrenLoaded = false
                        };

                        // Add images if they exist
                        if (step.Images is { Count: > 0 })
                        {
                            foreach (var image in step.Images)
                            {
                                var imageId = image.Id ?? Guid.Empty;
                                var imageNode = new PluginImageNode
                                {
                                    Id = imageId.ToString(),
                                    ImageId = imageId,
                                    DisplayName = image.Name ?? image.ImageType?.ToString() ?? "Unknown Image",
                                    Description = image.Description ?? string.Empty,
                                    ImageType = image.ImageType?.ToString(),
                                    MessagePropertyName = image.MessagePropertyName,
                                    Attributes = image.Attributes,
                                    EntityAlias = image.EntityAlias,
                                    Parent = stepNode
                                };
                                stepNode.Children.Add(imageNode);
                            }
                            stepNode.AreChildrenLoaded = true;
                        }

                        typeNode.Children.Add(stepNode);
                    }
                    typeNode.AreChildrenLoaded = true;
                }

                assembly.Children.Add(typeNode);
            }

            assembly.AreChildrenLoaded = true;
            return assembly.Children;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plugin types for assembly {0}", assembly.DisplayName);
            throw;
        }
    }

    public Task<IEnumerable<ExplorerNodeBase>> LoadPluginTypeChildrenAsync(
        PluginTypeNode pluginType, CancellationToken cancellationToken)
    {
        // Children are already loaded when we expand the assembly.
        // This method is for potential future lazy loading scenarios.
        return Task.FromResult(pluginType.Children.AsEnumerable());
    }

    public Task<IEnumerable<PluginImageNode>> LoadPluginStepChildrenAsync(
        PluginStepNode step, CancellationToken cancellationToken)
    {
        // Children are already loaded when we expand the assembly.
        // This method is for potential future lazy loading scenarios.
        return Task.FromResult(step.Children.OfType<PluginImageNode>());
    }

    public void ClearCache()
    {
        _assemblyCache.Clear();
        _assembliesLoaded = false;
    }

    public IEnumerable<ExplorerNodeBase> Search(string searchTerm, IEnumerable<ExplorerNodeBase> nodes)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return nodes;
        }

        var lowerTerm = searchTerm.ToLowerInvariant();
        var results = new List<ExplorerNodeBase>();

        foreach (var node in nodes)
        {
            if (MatchesSearch(node, lowerTerm))
            {
                results.Add(node);
            }

            // Recursively search children
            var childResults = Search(searchTerm, node.Children);
            results.AddRange(childResults.Where(r => !results.Contains(r)));
        }

        return results;
    }

    private static bool MatchesSearch(ExplorerNodeBase node, string lowerTerm)
    {
        return node.DisplayName.IndexOf(lowerTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
               (!string.IsNullOrWhiteSpace(node.Description) &&
                node.Description.IndexOf(lowerTerm, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}

#nullable restore
