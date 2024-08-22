namespace XrmGen.Xrm;

using Microsoft.Identity.Client;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using XrmGen.Xrm.Extensions;
using XrmGen.Xrm.Model;

public interface IXrmSchemaProvider : IDisposable
{
    public Task<IEnumerable<string>> GetEntityNamesAsync(CancellationToken cancellationToken);
    public IEnumerable<string> GetEntityNames();
    public Task<IEnumerable<EntityMetadata>> GetEntitiesAsync(CancellationToken cancellationToken);
    public IEnumerable<EntityMetadata> GetEntities();
    public Task<EntityMetadata> GetEntityAsync(string entityLogicalName, CancellationToken cancellationToken);
    public EntityMetadata GetEntity(string entityLogicalName);
    public Task<IEnumerable<PluginAssemblyConfig>> GetPluginAssembliesAsync(CancellationToken cancellationToken);
    public Task<IEnumerable<PluginTypeConfig>> GetPluginTypesAsync(Guid assemblyid, CancellationToken cancellationToken);
    public Task RefreshCacheAsync();
}
/*
[EntityLogicalName("pluginassembly")]
public class PluginAssembly : Entity
{
    [AttributeLogicalName("name")]
    public string Name
    {
        get => (string)this["name"];
        set => this["name"] = value;
    }
    [AttributeLogicalName("major")]
    public int Major
    {
        get => (int)this["major"];
        set => this["major"] = value;
    }

    [AttributeLogicalName("minor")]
    public int Minor
    {
        get => (int)this["minor"];
        set => this["minor"] = value;
    }

    [AttributeLogicalName("publickeytoken")]
    public string PublicKeyToken
    {
        get => (string)this["publickeytoken"];
        set => this["publickeytoken"] = value;
    }

    [AttributeLogicalName("solutionid")]
    public EntityReference SolutionId
    {
        get => (EntityReference)this["solutionid"];
        set => this["solutionid"] = value;
    }

    [AttributeLogicalName("version")]
    public string Version
    {
        get => (string)this["version"];
        set => this["version"] = value;
    }

    public List<PluginType> PluginTypes { get; set; } = [];
}

[EntityLogicalName("plugintype")]
public class PluginType
{
    [AttributeLogicalName("name")]
    public string Name { get; set; }

    [AttributeLogicalName("typename")]
    public string TypeName { get; set; }
}*/

public class XrmSchemaProvider(ServiceClient serviceClient) : IXrmSchemaProvider
{
    private readonly MetadataCache<IEnumerable<EntityMetadata>> entityCache = new(
        async (cancellation) => await FetchEntitiesAsync(serviceClient, cancellation));
    private readonly MetadataCache<IEnumerable<PluginAssemblyConfig>> pluginAssemblyCache = new(
        async (cancellation) => await FetchPluginAssembliesAsync(serviceClient, cancellation));
    private readonly Dictionary<Guid, MetadataCache<IEnumerable<PluginTypeConfig>>> pluginTypesCache = [];
    private readonly Dictionary<string, MetadataCache<EntityMetadata>> metadataExtensiveCache = [];
    private readonly object _lock = new();
    private bool disposed = false;

    public async Task<IEnumerable<string>> GetEntityNamesAsync(CancellationToken cancellationToken)
    {
        if (serviceClient.IsReady)
        {
            var entities = await GetEntitiesAsync(cancellationToken);
            return entities.Select(entity => entity.LogicalName);
        }
        return [];
    }

    public IEnumerable<string> GetEntityNames()
    {
        if (serviceClient.IsReady)
        {
            var entities = GetEntities();
            return entities.Select(entity => entity.LogicalName);
        }
        return [];
    }

    public async Task<IEnumerable<EntityMetadata>> GetEntitiesAsync(CancellationToken cancellationToken) 
        => await entityCache.GetDataAsync(cancellationToken);

    public IEnumerable<EntityMetadata> GetEntities() => entityCache.GetData();

    public async Task<EntityMetadata> GetEntityAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        if (metadataExtensiveCache.TryGetValue(entityLogicalName, out var cache))
        {
            return await cache.GetDataAsync(cancellationToken);
        }
        lock (_lock)
        {
            if (!metadataExtensiveCache.TryGetValue(entityLogicalName, out cache))
            {
                cache = new(async (cancellation) =>
                {
                    if (cancellation == CancellationToken.None)
                    {
                        using CancellationTokenSource cancellationTokenSource = new(10000);
                        return await FetchEntityAsync(entityLogicalName, serviceClient, cancellationTokenSource.Token);
                    }
                    else
                    {
                        return await FetchEntityAsync(entityLogicalName, serviceClient, cancellation);
                    }
                });
                metadataExtensiveCache[entityLogicalName] = cache;
            }
        }
        return await cache.GetDataAsync(cancellationToken);
    }

    public EntityMetadata GetEntity(string entityLogicalName)
    {
        if (metadataExtensiveCache.TryGetValue(entityLogicalName, out var cache))
        {
            return cache.GetData();
        }
        lock (_lock)
        {
            if (!metadataExtensiveCache.TryGetValue(entityLogicalName, out cache))
            {
                cache = new(async (cancellation) => {
                    return await FetchEntityAsync(entityLogicalName, serviceClient, cancellation);
                });
                metadataExtensiveCache[entityLogicalName] = cache;
            }
        }
        return cache.GetData();
    }

    public async Task<IEnumerable<PluginAssemblyConfig>> GetPluginAssembliesAsync(CancellationToken cancellationToken) 
        => await pluginAssemblyCache.GetDataAsync(cancellationToken);

    public async Task<IEnumerable<PluginTypeConfig>> GetPluginTypesAsync(Guid assemblyid, CancellationToken cancellationToken)
    {
        if (pluginTypesCache.TryGetValue(assemblyid, out var cache))
        {
            return await cache.GetDataAsync(cancellationToken);
        }
        lock (_lock)
        {
            if (!pluginTypesCache.TryGetValue(assemblyid, out cache))
            {
                cache = new(async (cancellation) =>
                {
                    if (cancellation == CancellationToken.None)
                    {
                        using CancellationTokenSource cancellationTokenSource = new(10000);
                        return await FetchPluginTypesAsync(serviceClient, assemblyid, cancellationTokenSource.Token);
                    }
                    else
                    {
                        return await FetchPluginTypesAsync(serviceClient, assemblyid, cancellation);
                    }
                });
                pluginTypesCache[assemblyid] = cache;
            }
        }
        return await cache.GetDataAsync(cancellationToken);
    }

    public async Task RefreshCacheAsync()
    {
        await entityCache.RefreshDataAsync();
        foreach (var cache in metadataExtensiveCache.Values.ToList())
        {
            // TODO: Improve to use only one fetch for all entities.
            await cache.RefreshDataAsync();
        }
    }

    private static async Task<EntityMetadata> FetchEntityAsync(
        string entityLogicalName, ServiceClient serviceClient, CancellationToken cancellationToken)
    {
        if (serviceClient.IsReady)
        {
            // Retrieve all entity metadata
            var request = new RetrieveEntityRequest
            {
                LogicalName = entityLogicalName,
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse) await serviceClient.ExecuteAsync(request, cancellationToken);

            return response.EntityMetadata;
        }
        else
        {
            // Log:
            // Failed to connect to Dataverse
            // serviceClient.LastError
            return null;
        }
    }

    private static async Task<IEnumerable<EntityMetadata>> FetchEntitiesAsync(
        ServiceClient client, CancellationToken cancellationToken)
    {
        if (client.IsReady)
        {
            // Retrieve all entity metadata
            var request = new RetrieveAllEntitiesRequest
            {
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveAllEntitiesResponse) await client.ExecuteAsync(request, cancellationToken);

            return response.EntityMetadata;
        }
        else
        {
            // Log:
            // Failed to connect to Dataverse
            // serviceClient.LastError
            return [];
        }
    }

    private static async Task<IEnumerable<PluginAssemblyConfig>> FetchPluginAssembliesAsync(
        ServiceClient client, CancellationToken cancellationToken)
    {
        var query = new QueryExpression("pluginassembly")
        {
            ColumnSet = new ColumnSet("name", "major", "minor", "publickeytoken", "solutionid", "version"),
            PageInfo = new PagingInfo
            {
                Count = 5000,
                PageNumber = 1
            }
        };

        //System.ServiceModel.CommunicationException: 'The underlying connection was closed: A connection that was expected to be kept alive was closed by the server.'
        var response = await client.RetrieveMultipleAsync(query, cancellationToken);

        if (response == null || response.Entities == null)
        {
            return [];
        }
        
        return response.Entities.Select(entity => entity.ToEntity<PluginAssemblyConfig>());
    }

    private static async Task<IEnumerable<PluginTypeConfig>> FetchPluginTypesAsync(
        ServiceClient client, Guid assemblyid, CancellationToken cancellationToken)
    {
        var query = new QueryExpression(PluginTypeConfig.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(PluginTypeConfig.GetColumnsFromExpression(s => new { s.Name, s.TypeName })),
            Criteria = new FilterExpression()
            {
                Conditions =
                {
                    new ConditionExpression("pluginassemblyid", ConditionOperator.Equal, assemblyid)
                }
            },
        };
        query.LinkWith(PluginTypeConfig.LinkWithSteps(s => new { s.Name, s.Stage })
             .LinkWith(PluginStepConfig.LinkWithImages(s => new { s.Name })));

        var pluginTypes = new List<PluginTypeConfig>();
        var response = await client.RetrieveMultipleAsync(query, cancellationToken);
        foreach (var entity in response.Entities)
        {
            var pluginType = pluginTypes.Find(pt => pt.Id == entity.Id);
            if (pluginType is null) {
                pluginType = entity.ToEntity<PluginTypeConfig>();
                pluginTypes.Add(pluginType);
            }
            var step = PluginStepConfig.FromAlias(entity);
            if (step is not null)
            {
                pluginType.Steps.Add(step);
            }
        }

        return pluginTypes;
    }

    #region IDisposable Support

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                serviceClient?.Dispose();
            }

            // Free unmanaged resources (if any)
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Finalizer to ensure resources are released if Dispose is not called
    ~XrmSchemaProvider() => Dispose(false);

    #endregion
}