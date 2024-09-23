#nullable enable
namespace XrmTools.Xrm;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using XrmTools.Options;

[Guid(PackageGuids.guidXrmSchemaProviderFactoryString)]
[ComVisible(true)]
public interface IXrmSchemaProviderFactory: IDisposable
{
    IXrmSchemaProvider? GetOrAddActiveEnvironmentProvider();
    Task<IXrmSchemaProvider?> GetOrAddActiveEnvironmentProviderAsync();
    IXrmSchemaProvider Get(DataverseEnvironment environment);
    IXrmSchemaProvider GetOrNew(DataverseEnvironment environment);
    Task EnsureInitializedAsync(DataverseEnvironment environment);
}

[method: ImportingConstructor]
public class XrmSchemaProviderFactory([Import]IEnvironmentProvider environmentProvider) : IXrmSchemaProviderFactory
{
    private static readonly ConcurrentDictionary<string, IXrmSchemaProvider> Providers = [];
    private static readonly object _lock = new();
    private bool disposed = false;

    public IXrmSchemaProvider? GetOrAddActiveEnvironmentProvider()
        => environmentProvider.GetActiveEnvironment() is DataverseEnvironment env and { IsValid: true }
            ? GetOrNew(env)
            : null;

    public async Task<IXrmSchemaProvider?> GetOrAddActiveEnvironmentProviderAsync()
        => await environmentProvider.GetActiveEnvironmentAsync() is DataverseEnvironment env and { IsValid: true }
            ? GetOrNew(env)
            : null;

    /// <summary>
    /// Returns an existing provider.
    /// </summary>
    /// <param name="environmentUrl"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">When environment has not been initialized.</exception>
    public IXrmSchemaProvider Get(DataverseEnvironment environment)
    {
        if (!environment.IsValid) return default!;
        if (Providers.TryGetValue(environment.Url, out var provider))
        {
            return provider;
        }
        throw new InvalidOperationException($"Environment with the URL: {environment} has not been initialized.");
    }

    public IXrmSchemaProvider GetOrNew(DataverseEnvironment environment)
        => !environment.IsValid ? default! : Providers.GetOrAdd(environment.Url, (key) => Initialize(environment));
    
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
    public async Task EnsureInitializedAsync(DataverseEnvironment environment)
    {
        if (!environment.IsValid) return;
        var provider = Providers.AddOrUpdate(
            environment.Url,
            (key) => Initialize(environment),
            (key, existingProvider) => 
                existingProvider.Environment == environment ? existingProvider : Initialize(environment)
        );
        await provider.RefreshCacheAsync();
    }

    /// <summary>
    /// Use this method to initialize a new provider.
    /// </summary>
    private IXrmSchemaProvider Initialize(DataverseEnvironment environment)
    {
        if (!environment.IsValid) return default!;
        if (Providers.TryGetValue(environment.Url, out var provider))
        {
            return provider;
        }
        var cache = new MemoryCache(new MemoryCacheOptions());
        provider = new XrmSchemaProvider(environment, cache);
        Providers[environment.Url] = provider;
        return provider;
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