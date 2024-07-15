using Microsoft.Identity.Client;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmGen._Core;
namespace XrmGen.Xrm;

internal class XrmSchemaProvider
{
    private readonly string EnvironmentUrl;
    private readonly string ApplicationId;
    private readonly string ConnectionString;

    private static XrmSchemaProvider _instance;
    private static readonly object _lock = new();

    private readonly MetadataCache<IEnumerable<string>> _entityNamesCache;

    public XrmSchemaProvider(string environmentUrl, string applicationId)
    {
        EnvironmentUrl = environmentUrl;
        ApplicationId = applicationId;
        // Use the current user's Windows credentials
        //ConnectionString = $"AuthType=OAuth;Url={EnvironmentUrl};AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;" +
        //    "RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;" +
        //    "Username=reza.niroomand@aginsurance.be;" +
        //    "TokenCacheStorePath=C:\\Users\\G99202\\msal_cache.data" + 
        //    "Integrated Security=true;" +
        //"LoginPrompt=Auto";
        ConnectionString = $"AuthType=OAuth;Integrated Security=true;" +
            $"Url=https://aguflowt.crm4.dynamics.com/;" +
            //$"AppId=aee1fc68-f5fa-42e4-b4a8-7df64e6931a6;" +
            $"AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;" +
            $"RedirectUri=https://login.microsoftonline.com/common/oauth2/nativeclient;" +
            $"TokenCacheStorePath=C:\\Users\\G99202\\msal_cache.data;" +
            $"LoginPrompt=Auto";
        _entityNamesCache = new (FetchEntityNamesAsync);
    }

    public static XrmSchemaProvider Instance
    {
        get
        {
            if (_instance == null)
            {
                _Core.Logger.Log("XrmSchemaProvider is not initialized. Call Initialize method first.");
            }
            return _instance;
        }
    }

    public static void Initialize(string environmentUrl, string applicationId)
    {
        lock (_lock)
        {
            _instance ??= new XrmSchemaProvider(environmentUrl, applicationId);
        }
    }

    public Task<IEnumerable<string>> GetEntityNamesAsync() => _entityNamesCache.GetDataAsync();

    public Task RefreshEntityNamesCacheAsync() => _entityNamesCache.RefreshDataAsync();

    public async Task<IEnumerable<string>> FetchEntityNamesAsync()
    {
        using var serviceClient = new ServiceClient(ConnectionString);
        if (serviceClient.IsReady)
        {
            // Retrieve all entity metadata
            var request = new RetrieveAllEntitiesRequest
            {
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveAllEntitiesResponse) await serviceClient.ExecuteAsync(request);

            return response.EntityMetadata.Select(entity => entity.LogicalName);
        }
        else
        {
            // Log:
            // Failed to connect to Dataverse
            // serviceClient.LastError
            return [];
        }
    }

    private async Task<IEnumerable<string>> FetchAnotherMetadataAsync()
    {
        // Simulate another heavy method call
        await Task.Delay(1000); // Simulating delay
        return ["metadata1", "metadata2", "metadata3"];
    }

}
