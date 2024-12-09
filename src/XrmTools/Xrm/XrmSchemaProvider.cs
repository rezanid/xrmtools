namespace XrmTools.Xrm;

using System.Runtime.Caching;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Xrm.Extensions;
using XrmTools.Xrm.Model;
using Microsoft.Xrm.Sdk;

internal class DataverseCacheKeys(string EnvironmentUrl)
{
    public string EntityDefinitions => $"{EnvironmentUrl}_EntityDefinitions";
    public string EntityDefinitionsExtensive(string entityLogicalName) => $"{EnvironmentUrl}_EntityDefinition_{entityLogicalName}";
    public string PluginAssemblies => $"{EnvironmentUrl}_PluginAssemblies";
    public string PluginTypes(Guid assemblyid) => $"{EnvironmentUrl}_PluginTypes_{assemblyid}";
}

internal class ServiceClient(string connectionstring) : IDisposable
{
    private bool disposedValue;

    public bool IsReady { get; }
    public Exception LastException { get; }
    public string LastError { get; }

    internal Task<RetrieveMultipleResponse> RetrieveMultipleAsync(QueryExpression queryExpression, CancellationToken cancellationToken)
        => throw new NotImplementedException();
    internal OrganizationResponse Execute(OrganizationRequest request) => throw new NotImplementedException();
    internal Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request, CancellationToken cancellationToken) 
        => throw new NotImplementedException();

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ServiceClient()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public class XrmSchemaProvider(DataverseEnvironment environment, string connectionString) : IXrmSchemaProvider
{
    private bool disposed = false;
    private readonly DataverseEnvironment environment = environment;
    private readonly MemoryCache cache = MemoryCache.Default;
    private readonly DataverseCacheKeys cacheKeys = new(environment.Url);
    private readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(30);
    private CancellationTokenSource cacheEvictionTokenSource = new ();
    private ServiceClient ServiceClient { get; init; } = new(connectionString);

    public DataverseEnvironment Environment { get => environment; }
    public bool IsReady { get => ServiceClient.IsReady; }
    public Exception LastException { get => ServiceClient.LastException; }
    public string LastError { get => ServiceClient.LastError; }

    public async Task<IEnumerable<EntityMetadata>> GetEntitiesAsync(CancellationToken cancellationToken)
        => !IsReady ? [] : await GetOrCreateCacheItemAsync(cacheKeys.EntityDefinitions, async () =>
        {
            return await FetchEntitiesAsync(ServiceClient, cancellationToken);
        }, cancellationToken);

    public async Task<EntityMetadata> GetEntityAsync(string entityLogicalName, CancellationToken cancellationToken)
        => !IsReady ? null : await GetOrCreateCacheItemAsync(cacheKeys.EntityDefinitionsExtensive(entityLogicalName), async () =>
        {
            return await FetchEntityAsync(entityLogicalName, ServiceClient, cancellationToken);
        }, cancellationToken);

    public EntityMetadata GetEntity(string entityLogicalName)
        => !IsReady ? null : GetOrCreateCacheItem(cacheKeys.EntityDefinitionsExtensive(entityLogicalName), () =>
        {
            return FetchEntity(entityLogicalName, ServiceClient);
        });

    public async Task<IEnumerable<PluginAssemblyConfig>> GetPluginAssembliesAsync(CancellationToken cancellationToken)
        => !IsReady ? [] : await GetOrCreateCacheItemAsync(cacheKeys.PluginAssemblies, async () =>
        {
            return await FetchPluginAssembliesAsync(ServiceClient, cancellationToken);
        }, cancellationToken);

    public async Task<IEnumerable<PluginTypeConfig>> GetPluginTypesAsync(Guid assemblyid, CancellationToken cancellationToken)
        => !IsReady ?[] : await GetOrCreateCacheItemAsync(cacheKeys.PluginTypes(assemblyid), async () =>
        {
            return await FetchPluginTypesAsync(ServiceClient, assemblyid, cancellationToken);
        }, cancellationToken);
    
    public Task RefreshCacheAsync()
    {
        cacheEvictionTokenSource?.Cancel();
        cacheEvictionTokenSource = new();
        //TODO: Add code here to fetch all the critical data and cache it.
        return Task.CompletedTask;
    }

    private async Task<T> GetOrCreateCacheItemAsync<T>(string cacheKey, Func<Task<T>> fetchFunction, CancellationToken cancellationToken)
    {
        if (cache.Contains(cacheKey))
        {
            return (T)cache.Get(cacheKey);
        }

        var data = await fetchFunction();
        CacheItemPolicy policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.Add(cacheExpiration)
        };
        cache.Add(cacheKey, data, policy);
        return data;
    }

    private T GetOrCreateCacheItem<T>(string cacheKey, Func<T> fetchFunction)
    {
        if (cache.Contains(cacheKey))
        {
            return (T)cache.Get(cacheKey);
        }

        var data = fetchFunction();
        CacheItemPolicy policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.Add(cacheExpiration)
        };
        cache.Add(cacheKey, data, policy);
        return data;
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
                a => new {a.PluginAssemblyId, a.Name, a.PublicKeyToken, a.SolutionId, a.Version, a.IsolationMode, a.SourceType }), 
            cancellationToken);

        if (response == null || response.EntityCollection.Entities == null)
        {
            return [];
        }
        
        return response.EntityCollection.Entities.Select(entity => entity.ToEntity<PluginAssemblyConfig>());
    }

    private static async Task<IEnumerable<PluginTypeConfig>> FetchPluginTypesAsync(
        ServiceClient client, Guid assemblyid, CancellationToken cancellationToken)
    {
        var query = PluginTypeConfig.CreateQuery(s => new { s.PluginTypeId, s.Name, s.TypeName, s.FriendlyName, s.Description, s.WorkflowActivityGroupName })
            .WithCondition(
            new ConditionExpression(
                PluginTypeConfig.Select.ColumnName((e) => e.PluginAssemblyId), 
                ConditionOperator.Equal, 
                assemblyid));
        query.LinkWith(
            //s.CustomConfiguration
            PluginTypeConfig.LinkWithSteps(s => new {s.PluginStepId, s.Name, s.Stage, s.AsyncAutoDelete, s.Description, s.FilteringAttributes, s.InvocationSource, s.Mode, s.Rank, s.SdkMessageId, s.State, s.SupportedDeployment})
            .LinkWith(
                PluginStepConfig.LinkWithImages(s => new {s.PluginStepImageId, s.Name })));//, s.ImageAttributes, s.EntityAlias, s.ImageType, s.MessagePropertyName })));

        var pluginTypes = new List<PluginTypeConfig>();
        var response = await client.RetrieveMultipleAsync(query, cancellationToken);
        foreach (var entity in response.EntityCollection.Entities)
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
                ServiceClient?.Dispose();
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