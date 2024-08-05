#nullable enable
namespace XrmGen.Xrm;

using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

[Guid(PackageGuids.guidXrmSchemaProviderFactoryString)]
[ComVisible(true)]
public interface IXrmSchemaProviderFactory: IDisposable
{
    IXrmSchemaProvider GetOrNew(string environmentUrl, string applicationId);
    IXrmSchemaProvider Get(string environmentUrl);
    Task EnsureInitializedAsync(string environmentUrl, string applicationId);
}

[Export(typeof(IXrmSchemaProviderFactory))]
public class XrmSchemaProviderFactory : IXrmSchemaProviderFactory
{
    private static readonly Dictionary<string, IXrmSchemaProvider> Providers = [];
    private static readonly object _lock = new();
    private bool disposed = false;

    /// <summary>
    /// Returns an existing provider or creates a new one and caches it.
    /// </summary>
    /// <param name="environmentUrl"></param>
    /// <param name="applicationId"></param>
    /// <returns></returns>
    public IXrmSchemaProvider GetOrNew(string environmentUrl, string applicationId)
    {
        if (Providers.TryGetValue(environmentUrl, out var provider))
        {
            return provider;
        }
        return Initialize(environmentUrl, applicationId);
    }

    private IXrmSchemaProvider Initialize(string environmentUrl, string applicationId)
    {
        lock (_lock)
        {
            // To avoid race conditions:
            if (Providers.TryGetValue(environmentUrl, out var provider))
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

    /// <summary>
    /// Returns an existing provider.
    /// </summary>
    /// <param name="environmentUrl"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">When environment has not been initialized.</exception>
    public IXrmSchemaProvider Get(string environmentUrl)
    {
        if (Providers.TryGetValue(environmentUrl, out var provider))
        {
            return provider;
        }
        throw new InvalidOperationException(
            string.Format(Resources.Strings.EnvironmentNotInitialized, nameof(Get)));
    }

    /// <summary>
    /// Refreshes cached metadata
    /// </summary>
    /// <param name="environmentUrl"></param>
    /// <exception cref="InvalidOperationException">When environment has not been initialized.</exception>
    public static async Task RefreshCacheAsync(string environmentUrl)
    {
        if (!Providers.TryGetValue(environmentUrl, out var provider))
        {
            throw new InvalidOperationException(
                string.Format(Resources.Strings.EnvironmentNotInitialized, nameof(GetOrNew)));
        }
        await provider.RefreshCacheAsync();
    }

    /// <summary>
    /// Ensure a provider is initialized for the given environment. It is a best practice to initialize 
    /// the providers before they are needed. You will gain some performance boost this way.
    /// </summary>
    /// <param name="environmentUrl"></param>
    /// <param name="applicationId"></param>
    public async Task EnsureInitializedAsync(string environmentUrl, string applicationId)
    {
        if (Providers.ContainsKey(environmentUrl)) { return; }
        var provider = Initialize(environmentUrl, applicationId);
        await provider.RefreshCacheAsync();
    }

    #region IDisposable Support

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) { return; }
        if (disposing)
        {
            // Dispose managed resources
            lock (_lock)
            {
                foreach (var provider in Providers.Values)
                {
                    provider.Dispose();
                }
                Providers.Clear();
            }
        }

        // Free unmanaged resources (if any)
        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~XrmSchemaProviderFactory() => Dispose(false);

    #endregion
}
#nullable restore