#nullable enable
namespace XrmGen.Xrm;

using Microsoft.Extensions.Caching.Memory;
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
    IXrmSchemaProvider GetOrNew(string environmentUrl);
    IXrmSchemaProvider Get(string environmentUrl);
    Task EnsureInitializedAsync(string environmentUrl);
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
    public IXrmSchemaProvider GetOrNew(string environmentUrl)
    {
        if (Providers.TryGetValue(environmentUrl, out var provider))
        {
            return provider;
        }
        return Initialize(environmentUrl);
    }

    private IXrmSchemaProvider Initialize(string environmentUrl)
        => Initialize(environmentUrl, $"AuthType=OAuth;Integrated Security=true;Url={environmentUrl};LoginPrompt=Never");

    /// <summary>
    /// Use this method to initialize a new provider.
    /// </summary>
    /// <param name="environmentUrl"></param>
    /// <param name="connectionString">Examples: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect#connection-string-parameters</param>
    /// <returns></returns>
    private IXrmSchemaProvider Initialize(string environmentUrl, string connectionString)
    {
        lock (_lock)
        {
            // To avoid race conditions:
            if (Providers.TryGetValue(environmentUrl, out var provider))
            {
                return provider;
            }
            var cache = new MemoryCache(new MemoryCacheOptions());
            provider = new XrmSchemaProvider(new ServiceClient(connectionString), environmentUrl, cache);
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
        throw new InvalidOperationException($"Environment with the URL: {environmentUrl} has not been initialized.");
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
    public async Task EnsureInitializedAsync(string environmentUrl)
    {
        if (Providers.ContainsKey(environmentUrl)) { return; }
        var provider = Initialize(environmentUrl);
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