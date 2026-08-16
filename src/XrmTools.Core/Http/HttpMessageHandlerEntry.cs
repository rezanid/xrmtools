#nullable enable
namespace XrmTools.Http;
using System;
using System.Net.Http;
using System.Threading;

internal class HttpMessageHandlerEntry(HttpMessageHandler handler, DateTimeOffset creationTime, TimeProvider? timeProvider = null)
{
    private readonly TimeProvider timeProvider = timeProvider ?? TimeProvider.System;
    private int usageCount = 0;
    private readonly DateTimeOffset creationTime = creationTime;
    public HttpMessageHandler Handler { get; } = handler;
    public TimeSpan Lifetime { get; } = TimeSpan.FromMinutes(10);

    public void IncrementUsage() => Interlocked.Increment(ref usageCount);

    public void DecrementUsage() => Interlocked.Decrement(ref usageCount);

    public bool CanDispose() => usageCount <= 0 && (timeProvider.GetUtcNow() - creationTime) > Lifetime;
}
#nullable restore