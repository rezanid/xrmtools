namespace XrmTools.Core.Repositories;

using System;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.WebApi;

internal class XrmRepository(IWebApiService service) : IDisposable, IAsyncDisposable
{
    protected readonly IWebApiService service = service;
    private readonly MemoryCache cache = MemoryCache.Default;
    private readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(30);

    private bool disposedValue;

    protected async Task<T> GetOrCreateCacheItemAsync<T>(string cacheKey, Func<Task<T>> fetchFunction, CancellationToken cancellationToken)
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

    protected T GetOrCreateCacheItem<T>(string cacheKey, Func<T> fetchFunction)
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

    private void Dispose(bool disposing)
    {
        if (!disposedValue && disposing)
        {
            if (service is IDisposable disposable)
            {
                disposable.Dispose();
            }
            disposedValue = true;
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (service is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (service is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }
}
