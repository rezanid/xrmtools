namespace XrmGen.Xrm;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmGen.Xrm.Extensions;
using XrmGen.Xrm.Model;

public interface IXrmSchemaProvider : IDisposable
{
    string EnvironmentUrl { get; }
    bool IsReady { get; }
    Exception LastException { get; }
    string LastError { get; }
    Task<IEnumerable<EntityMetadata>> GetEntitiesAsync(CancellationToken cancellationToken);
    Task<EntityMetadata> GetEntityAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<IEnumerable<PluginAssemblyConfig>> GetPluginAssembliesAsync(CancellationToken cancellationToken);
    Task<IEnumerable<PluginTypeConfig>> GetPluginTypesAsync(Guid assemblyid, CancellationToken cancellationToken);
    Task RefreshCacheAsync();
}

internal class DataverseCacheKeys(string EnvironmentUrl)
{
    public string EntityDefinitions => $"{EnvironmentUrl}_EntityDefinitions";
    public string EntityDefinitionsExtensive(string entityLogicalName) => $"{EnvironmentUrl}_EntityDefinition_{entityLogicalName}";
    public string PluginAssemblies => $"{EnvironmentUrl}_PluginAssemblies";
    public string PluginTypes(Guid assemblyid) => $"{EnvironmentUrl}_PluginTypes_{assemblyid}";
}

public class XrmSchemaProvider(ServiceClient serviceClient, string environmentUrl, IMemoryCache cache) : IXrmSchemaProvider
{
    private bool disposed = false;
    private readonly DataverseCacheKeys cacheKeys = new (environmentUrl);
    private readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(30);
    private CancellationTokenSource cacheEvictionTokenSource = new ();

    public string EnvironmentUrl { get => environmentUrl; }
    public bool IsReady { get => serviceClient.IsReady; }
    public Exception LastException { get => serviceClient.LastException; }
    public string LastError { get => serviceClient.LastError; }

    public async Task<IEnumerable<EntityMetadata>> GetEntitiesAsync(CancellationToken cancellationToken)
        => !IsReady ? [] : await cache.GetOrCreateAsync(cacheKeys.EntityDefinitions, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheExpiration;
            entry.AddExpirationToken(new CancellationChangeToken(cancellationToken));
            return await FetchEntitiesAsync(serviceClient, cancellationToken);
        });

    public async Task<EntityMetadata> GetEntityAsync(string entityLogicalName, CancellationToken cancellationToken)
        => !IsReady ? null : await cache.GetOrCreateAsync(cacheKeys.EntityDefinitionsExtensive(entityLogicalName), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheExpiration;
            entry.AddExpirationToken(new CancellationChangeToken(cancellationToken));
            return await FetchEntityAsync(entityLogicalName, serviceClient, cancellationToken);
        });

    public EntityMetadata GetEntity(string entityLogicalName, CancellationToken cancellationToken)
        => !IsReady ? null : cache.GetOrCreate(cacheKeys.EntityDefinitionsExtensive(entityLogicalName), entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheExpiration;
            entry.AddExpirationToken(new CancellationChangeToken(cancellationToken));
            return FetchEntity(entityLogicalName, serviceClient);
        });

    public async Task<IEnumerable<PluginAssemblyConfig>> GetPluginAssembliesAsync(CancellationToken cancellationToken)
        => !IsReady ? [] : await cache.GetOrCreateAsync(cacheKeys.PluginAssemblies, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheExpiration;
            entry.AddExpirationToken(new CancellationChangeToken(cancellationToken));
            return await FetchPluginAssembliesAsync(serviceClient, cancellationToken);
        });

    public async Task<IEnumerable<PluginTypeConfig>> GetPluginTypesAsync(Guid assemblyid, CancellationToken cancellationToken)
        => !IsReady ?[] : await cache.GetOrCreateAsync(cacheKeys.PluginTypes(assemblyid), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheExpiration;
            entry.AddExpirationToken(new CancellationChangeToken(cancellationToken));
            return await FetchPluginTypesAsync(serviceClient, assemblyid, cancellationToken);
        });
    
    public Task RefreshCacheAsync()
    {
        cacheEvictionTokenSource?.Cancel();
        cacheEvictionTokenSource = new();
        //TODO: Add code here to fetch all the critical data and cache it.
        return Task.CompletedTask;
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

    private static EntityMetadata FetchEntity(
            string entityLogicalName, ServiceClient serviceClient)
    {
        if (!serviceClient.IsReady)
        {
            return null;
        }

        // Retrieve all entity's metadata
        var request = new RetrieveEntityRequest
        {
            LogicalName = entityLogicalName,
            EntityFilters = EntityFilters.Attributes,
            RetrieveAsIfPublished = true
        };

        var response = (RetrieveEntityResponse)serviceClient.Execute(request);

        return response.EntityMetadata;
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
        //TODO:
        //System.ServiceModel.CommunicationException: 'The underlying connection was closed: A connection that was expected to be kept alive was closed by the server.'
        //==>The HTTP request was forbidden with client authentication scheme 'Anonymous'. ==> 403 ==> client.LastError
        //The following line will also fail if developer doesn't have access to an EnvironmentUrl
        var response = await client.RetrieveMultipleAsync(
            PluginAssemblyConfig.CreateQuery(
                a => new { a.Name, a.Major, a.Minor, a.PublicKeyToken, a.SolutionId, a.Version }), 
            cancellationToken);

        if (response == null || response.Entities == null)
        {
            return [];
        }
        
        return response.Entities.Select(entity => entity.ToEntity<PluginAssemblyConfig>());
    }

    private static async Task<IEnumerable<PluginTypeConfig>> FetchPluginTypesAsync(
        ServiceClient client, Guid assemblyid, CancellationToken cancellationToken)
    {
        var query = PluginTypeConfig.CreateQuery(s => new { s.Name, s.TypeName })
            .WithCondition(
            new ConditionExpression(
                PluginTypeConfig.Select.ColumnName((e) => e.PluginAssemblyId), 
                ConditionOperator.Equal, 
                assemblyid));
        query.LinkWith(
            PluginTypeConfig.LinkWithSteps(s => new { s.Name, s.Stage })
            .LinkWith(PluginStepConfig.LinkWithImages(s => new { s.Name })));

        var pluginTypes = new List<PluginTypeConfig>();
        var response = await client.RetrieveMultipleAsync(query, cancellationToken);
        foreach (var entity in response.Entities)
        {
            var pluginType = pluginTypes.Find(pt => pt.Id == entity.Id);
            if (pluginType is null) 
            {
                pluginType = entity.ToEntity<PluginTypeConfig>();
                pluginTypes.Add(pluginType);
            }
            var step = PluginStepConfig.FromAlias(entity);
            if (step is not null)
            {
                pluginType.Steps.Add(step);

                var stepimage = PluginStepImageConfig.FromAlias(entity);
                if (stepimage is not null)
                {
                    step.Images.Add(stepimage);
                }
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