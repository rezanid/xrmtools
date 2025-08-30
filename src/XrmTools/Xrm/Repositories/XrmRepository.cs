namespace XrmTools.Core.Repositories;

using System;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.WebApi;

internal class XrmRepository : IDisposable, IAsyncDisposable
{
    protected readonly IWebApiService service;
    private readonly MemoryCache cache = MemoryCache.Default;
    private readonly ICacheConfiguration cacheConfiguration;
    private bool disposedValue;

    public XrmRepository(IWebApiService service)
        : this(service, new CacheConfiguration())
    {
    }

    public XrmRepository(IWebApiService service, ICacheConfiguration cacheConfiguration)
    {
        this.service = service;
        this.cacheConfiguration = cacheConfiguration;
    }

    protected async Task<T> GetOrCreateCacheItemAsync<T>(string cacheKey, Func<Task<T>> fetchFunction, CancellationToken cancellationToken)
    {
        if (cache.Contains(cacheKey))
        {
            var cachedItem = (T)cache.Get(cacheKey);

            // For sliding expiration, update the cache item to reset the expiry
            if (cacheConfiguration.UseSlidingExpiration)
            {
                var policy = CreateCacheItemPolicy();
                cache.Set(cacheKey, cachedItem, policy);
            }

            return cachedItem;
        }

        var data = await fetchFunction().ConfigureAwait(false);
        var cachePolicy = CreateCacheItemPolicy();
        cache.Add(cacheKey, data, cachePolicy);
        return data;
    }

    protected T GetOrCreateCacheItem<T>(string cacheKey, Func<T> fetchFunction)
    {
        if (cache.Contains(cacheKey))
        {
            var cachedItem = (T)cache.Get(cacheKey);

            // For sliding expiration, update the cache item to reset the expiry
            if (cacheConfiguration.UseSlidingExpiration)
            {
                var policy = CreateCacheItemPolicy();
                cache.Set(cacheKey, cachedItem, policy);
            }

            return cachedItem;
        }

        var data = fetchFunction();
        var cachePolicy = CreateCacheItemPolicy();
        cache.Add(cacheKey, data, cachePolicy);
        return data;
    }

    private CacheItemPolicy CreateCacheItemPolicy()
    {
        var policy = new CacheItemPolicy();

        if (cacheConfiguration.UseSlidingExpiration)
        {
            policy.SlidingExpiration = cacheConfiguration.CacheExpiration;
        }
        else
        {
            policy.AbsoluteExpiration = DateTimeOffset.Now.Add(cacheConfiguration.CacheExpiration);
        }

        return policy;
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