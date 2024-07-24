using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
namespace XrmGen.Xrm;

public interface IXrmSchemaProviderFactory
{
    IXrmSchemaProvider Get(string environmentUrl, string applicationId);
}

[Export(typeof(IXrmSchemaProviderFactory))]
internal class XrmSchemaProviderFactory : IXrmSchemaProviderFactory
{
    private static readonly Dictionary<string, IXrmSchemaProvider> Providers = [];

    private static readonly object _lock = new();

    public IXrmSchemaProvider Get(string environmentUrl, string applicationId)
    {
        if (Providers.TryGetValue(environmentUrl, out var provider))
        {
            return provider;
        }
        lock (_lock)
        {
            // To avoid race conditions:
            if (Providers.TryGetValue(environmentUrl, out provider))
            {
                return provider;
            }
            var connectionString = $"AuthType=OAuth;Integrated Security=true;" +
                $"Url=https://aguflowt.crm4.dynamics.com/;" +
                $"AppId={applicationId};" +
                $"RedirectUri=https://login.microsoftonline.com/common/oauth2/nativeclient;" +
                $"TokenCacheStorePath=C:\\Users\\G99202\\msal_cache.data;" +
                $"LoginPrompt=Auto";
            provider = new XrmSchemaProvider(new ServiceClient(connectionString));
            Providers[environmentUrl] = provider;
            return provider;
        }
    }

    public static async Task RefreshCacheAsync(string environmentUrl)
    {
        if (!Providers.TryGetValue(environmentUrl, out var provider))
        {
            throw new InvalidOperationException("Environment not initialized. You will have to first initialize the environment.");
        }
        await provider.RefreshCacheAsync();
    }
}
