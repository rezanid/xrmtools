namespace XrmGen.Xrm;

using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public interface IXrmSchemaProvider : IDisposable
{
    public Task<IEnumerable<string>> GetEntityNamesAsync();
    public IEnumerable<string> GetEntityNames();
    public Task<IEnumerable<EntityMetadata>> GetEntitiesAsync();
    public IEnumerable<EntityMetadata> GetEntities();
    public Task<EntityMetadata> GetEntityAsync(string entityLogicalName);
    public EntityMetadata GetEntity(string entityLogicalName);
    public Task RefreshCacheAsync();
}

public class XrmSchemaProvider(ServiceClient serviceClient) : IXrmSchemaProvider
{
    private readonly MetadataCache<IEnumerable<EntityMetadata>> metadataCache = new(
        async () =>
        {
            using CancellationTokenSource tcs = new(10000);
            return await FetchEntitiesAsync(serviceClient, tcs.Token);
        });
    private readonly Dictionary<string, MetadataCache<EntityMetadata>> metadataExtensiveCache = [];
    private readonly object _lock = new();
    private bool disposed = false;

    public async Task<IEnumerable<string>> GetEntityNamesAsync()
    {
        if (serviceClient.IsReady)
        {
            var entities = await GetEntitiesAsync();
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

    public async Task<IEnumerable<EntityMetadata>> GetEntitiesAsync() => await metadataCache.GetDataAsync();

    public IEnumerable<EntityMetadata> GetEntities() => metadataCache.GetData();

    public async Task<EntityMetadata> GetEntityAsync(string entityLogicalName)
    {
        if (metadataExtensiveCache.TryGetValue(entityLogicalName, out var cache))
        {
            return await cache.GetDataAsync();
        }
        lock (_lock)
        {
            if (!metadataExtensiveCache.TryGetValue(entityLogicalName, out cache))
            {
                cache = new(async () =>
                {
                    using CancellationTokenSource cancellationTokenSource = new(10000);
                    return await FetchEntityAsync(entityLogicalName, serviceClient, cancellationTokenSource.Token);
                });
                metadataExtensiveCache[entityLogicalName] = cache;
            }
        }
        return await cache.GetDataAsync();
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
                cache = new(async () => {
                    using CancellationTokenSource cancellationTokenSource = new(10000);
                    return await FetchEntityAsync(entityLogicalName, serviceClient, cancellationTokenSource.Token);
                });
                metadataExtensiveCache[entityLogicalName] = cache;
            }
        }
        return cache.GetData();
    }

    public async Task RefreshCacheAsync()
    {
        await metadataCache.RefreshDataAsync();
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
