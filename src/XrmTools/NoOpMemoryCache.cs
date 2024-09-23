namespace XrmTools;

using Microsoft.Extensions.Caching.Memory;
using System;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

public class NoOpMemoryCache : IMemoryCache
{
    public static NoOpMemoryCache Instance { get; } = new NoOpMemoryCache();

    public ICacheEntry CreateEntry(object key) => new NoOpCacheEntry(key);

    public void Dispose()
    {
        // Nothing to dispose
    }

    public void Remove(object key)
    {
        // Nothing to remove
    }

    public bool TryGetValue(object key, out object value)
    {
        value = null;
        // Always return false to indicate no cache hit
        return false; 
    }

    private class NoOpCacheEntry(object key) : ICacheEntry
    {
        public object Key { get; } = key;

        public object Value
        {
            get => null;
            set { }
        }

        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public IList<IChangeToken> ExpirationTokens => [];
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks => [];
        public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

        public long? Size { get; set; }

        public void Dispose()
        {
            // No-op dispose
        }
    }
}

