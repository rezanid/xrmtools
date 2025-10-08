namespace XrmTools.Resiliency;
using System;
using System.Net.Http;

internal static class RetryAfterHelper
{
    /// <summary>
    /// Parses Retry-After value from the relevant HTTP response header.
    /// If not found then it will return <see cref="TimeSpan.Zero"/> and false.
    /// </summary>
    internal static bool TryParse(HttpResponseMessage response, TimeProvider timeProvider, out TimeSpan retryAfter)
    {
        var header = response.Headers.RetryAfter;
        if (header is null)
        {
            retryAfter = TimeSpan.Zero;
            return false;
        }

        // Retry-After: <http-date>
        if (header.Date is DateTimeOffset date)
        {
            var remaining = date - timeProvider.GetUtcNow();
            retryAfter = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
            return true;
        }

        // Retry-After: <delta-seconds>
        if (header.Delta is TimeSpan delta)
        {
            retryAfter = delta < TimeSpan.Zero ? TimeSpan.Zero : delta;
            return true;
        }

        // Header present but not usable
        retryAfter = TimeSpan.Zero;
        return false;
    }
}
