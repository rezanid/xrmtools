#nullable enable
namespace XrmTools.Core.Repositories;

using System;

/// <summary>
/// Configuration interface for repository caching
/// </summary>
public interface ICacheConfiguration
{
    /// <summary>
    /// Cache expiration time
    /// </summary>
    TimeSpan CacheExpiration { get; }
    
    /// <summary>
    /// Whether to use sliding expiration (resets expiry on access)
    /// </summary>
    bool UseSlidingExpiration { get; }
}

/// <summary>
/// Default cache configuration implementation
/// </summary>
public class CacheConfiguration : ICacheConfiguration
{
    public TimeSpan CacheExpiration { get; init; } = TimeSpan.FromMinutes(30);
    public bool UseSlidingExpiration { get; init; } = false;
}

/// <summary>
/// Sliding cache configuration for repositories that need frequent cache refreshes
/// </summary>
public class SlidingCacheConfiguration : ICacheConfiguration
{
    public TimeSpan CacheExpiration { get; init; } = TimeSpan.FromMinutes(10);
    public bool UseSlidingExpiration { get; init; } = true;
}
#nullable restore