#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using Microsoft.VisualStudio.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Helpers;
using XrmTools.DataverseExplorer.Models;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Methods;

/// <summary>
/// Implementation of the explorer data service.
/// Manages data loading from repositories and maintains a simple in-memory index.
/// </summary>
[Export(typeof(IExplorerDataService))]
[method: ImportingConstructor]
internal sealed class ExplorerDataService(
    [Import] IWebApiService webApi,
    [Import] ILogger<ExplorerDataService> logger) : IExplorerDataService
{
    private const string assembliesQuery = "pluginassemblies?$select=pluginassemblyid,name,description,publickeytoken,solutionid,version,isolationmode,sourcetype";
    private const string plugintypesQuery = "plugintypes?" +
        "$filter=_pluginassemblyid_value eq '{0}'" +
        "&$select=plugintypeid,name,typename,friendlyname,description,workflowactivitygroupname&" +
        "$expand=plugintype_sdkmessageprocessingstep(" +
            "$select=sdkmessageprocessingstepid,name,stage,asyncautodelete,description,filteringattributes,invocationsource,mode,rank,sdkmessageid,statecode,supporteddeployment;" +
            "$expand=sdkmessageprocessingstepid_sdkmessageprocessingstepimage(" +
                "$select=sdkmessageprocessingstepimageid,name,imagetype,messagepropertyname,attributes,entityalias))," +
        "CustomAPIId($select=displayname,uniquename,isfunction,bindingtype,workflowsdkstepenabled,isprivate,statecode,name,allowedcustomprocessingsteptype,executeprivilegename,boundentitylogicalname,description,statuscode;" +
            "$expand=CustomAPIRequestParameters($select=displayname,uniquename,name,statecode,statuscode,logicalentityname,description,type,isoptional)," +
            "CustomAPIResponseProperties($select=displayname,uniquename,name,statecode,statuscode,logicalentityname,description,type))";

    private readonly ILogger _logger = logger;
    private readonly Dictionary<Guid, AssemblyNode> _assemblyCache = [];
    private bool _assembliesLoaded;

    public async Task<IEnumerable<AssemblyNode>> LoadAssembliesAsync(CancellationToken cancellationToken)
    {
        if (_assembliesLoaded)
        {
            return _assemblyCache.Values;
        }
        ODataQueryResponse<PluginAssembly>? queryResponse;
        try
        {
            queryResponse = await webApi.RetrieveMultipleAsync<PluginAssembly>(
                assembliesQuery, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assemblies from Dataverse.");
            throw;
        }

        _assemblyCache.Clear();
        foreach (var assembly in queryResponse.Value)
        {
            var assemblyId = assembly.Id ?? Guid.Empty;
            var node = new AssemblyNode
            {
                ImageMoniker = KnownMonikers.Assembly,
                Id = assemblyId.ToString(),
                AssemblyId = assemblyId,
                DisplayName = assembly.Name ?? "Unknown Assembly",
                Description = string.Empty,
                PublicKeyToken = assembly.PublicKeyToken,
                Version = assembly.Version,
                IsolationMode = assembly.IsolationMode?.ToString(),
                SourceType = assembly.SourceType?.ToString(),
                AreChildrenLoaded = false
            };
            _assemblyCache[assemblyId] = node;
        }

        _assembliesLoaded = true;
        return _assemblyCache.Values;
    }

    public async Task<IEnumerable<ExplorerNodeBase>> LoadAssemblyChildrenAsync(AssemblyNode assembly, CancellationToken cancellationToken)
    {
        ODataQueryResponse<PluginType>? queryResponse;
        try
        {
            queryResponse= await webApi.RetrieveMultipleAsync<PluginType>(
                plugintypesQuery.FormatWith(assembly.AssemblyId), cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plugin types for assembly {0}", assembly.DisplayName);
            throw;
        }
        assembly.Children.Clear();
        foreach (var plugin in queryResponse.Value)
        {
            if (plugin.CustomApi is { Count: > 0 })
            {
                foreach (var api in plugin.CustomApi)
                {
                    assembly.Children.Add(ConvertToCustomApi(api, assembly));
                }
            }
            else
            {
                assembly.Children.Add(ConvertToPluginNode(plugin, assembly));
            }
        }
        assembly.AreChildrenLoaded = true;
        return assembly.Children;
    }

    private static PluginTypeNode ConvertToPluginNode(PluginType plugin, AssemblyNode assembly)
    {
        var pluginNode = new PluginTypeNode
        {
            ImageMoniker = KnownMonikers.Part,
            Id = (plugin.Id ?? Guid.Empty).ToString(),
            PluginTypeId = plugin.Id ?? Guid.Empty,
            DisplayName = plugin.FriendlyName ?? plugin.TypeName ?? "Unknown Type",
            Description = plugin.Description ?? string.Empty,
            TypeName = plugin.TypeName,
            FriendlyName = plugin.FriendlyName,
            WorkflowActivityGroupName = plugin.WorkflowActivityGroupName,
            Parent = assembly,
            AreChildrenLoaded = false
        };
        pluginNode.Children.Clear();
        foreach (var step in plugin.Steps)
        {
            pluginNode.Children.Add(ConvertToPluginStepNode(step, pluginNode));
        }
        pluginNode.AreChildrenLoaded = true;
        return pluginNode;
    }

    private static CustomApiNode ConvertToCustomApi(CustomApi api, AssemblyNode assembly)
    {
        var apiNode = new CustomApiNode
        {
            ImageMoniker = KnownMonikers.WebAPI,
            Id = (api.Id ?? Guid.Empty).ToString(),
            CustomApiId = api.Id ?? Guid.Empty,
            DisplayName = api.DisplayName ?? api.Name ?? "Unknown API",
            Description = api.Description ?? string.Empty,
            Name = api.Name,
            Parent = assembly,
            AreChildrenLoaded = false
        };
        apiNode.Children.Clear();
        foreach (var input in api.RequestParameters)
        {
            apiNode.Children.Add(new CustomApiParameterNode
            {
                ImageMoniker = KnownMonikers.Parameter,
                Id = (input.Id ?? Guid.Empty).ToString(),
                ParameterId = input.Id ?? Guid.Empty,
                DisplayName = input.Name ?? "Unknown Input Parameter",
                Description = string.Empty,
                Name = input.Name,
                ParameterType = input.TypeName,
                IsOptional = input.IsOptional,
                Parent = apiNode
            });
        }
        foreach (var output in api.ResponseProperties)
        {
            apiNode.Children.Add(new CustomApiResponseNode
            {
                ImageMoniker = KnownMonikers.Property,
                Id = (output.Id ?? Guid.Empty).ToString(),
                ResponseId = output.Id ?? Guid.Empty,
                DisplayName = output.Name ?? "Unknown Response Property",
                Description = string.Empty,
                Name = output.Name,
                PropertyType = output.TypeName,
                Parent = apiNode
            });
        }
        apiNode.AreChildrenLoaded = true;
        return apiNode;
    }

    private static PluginStepNode ConvertToPluginStepNode(SdkMessageProcessingStep step, PluginTypeNode plugin)
    {
        var stepNode = new PluginStepNode
        {
            ImageMoniker = KnownMonikers.Step,
            Id = (step.Id ?? Guid.Empty).ToString(),
            StepId = step.Id ?? Guid.Empty,
            DisplayName = step.Name ?? "Unknown Step",
            Description = step.Description ?? string.Empty,
            FilteringAttributes = step.FilteringAttributes,
            AsyncAutoDelete = step.AsyncAutoDelete,
            InvocationSource = step.InvocationSource.ToString(),
            Mode = step.Mode.ToString(),
            Rank = step.Rank,
            Parent = plugin,
            SdkMessageId = step.SdkMessageFilter?.SdkMessageId?.ToString(),
            Stage = step.Stage.ToString(),
            StateCode = step.StateCode.ToString(),
            SupportedDeployment = step.SupportedDeployment.ToString(),
            AreChildrenLoaded = true
        };
        stepNode.Children.Clear();
        foreach (var image in step.Images)
        {
            stepNode.Children.Add(ConvertToStepImage(image, stepNode));
        }
        stepNode.AreChildrenLoaded = true;
        return stepNode;
    }

    private static PluginImageNode ConvertToStepImage(SdkMessageProcessingStepImage image, PluginStepNode step) => new()
    {
        ImageMoniker = KnownMonikers.Image,
        Id = (image.Id ?? Guid.Empty).ToString(),
        ImageId = image.Id ?? Guid.Empty,
        ImageType = image.ImageType.ToString(),
        Description = image.Description ?? string.Empty,
        DisplayName = image.Name ?? "Unknown Image",
        EntityAlias = image.EntityAlias ?? string.Empty,
        Attributes = image.Attributes ?? string.Empty,
        MessagePropertyName = image.MessagePropertyName ?? string.Empty,
        Parent = step,
    };

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
