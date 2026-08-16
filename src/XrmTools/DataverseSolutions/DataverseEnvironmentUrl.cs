#nullable enable
namespace XrmTools.DataverseSolutions;

using System;

internal static class DataverseEnvironmentUrl
{
    public static bool TryNormalize(string? url, out string? normalizedUrl)
    {
        normalizedUrl = null;
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var builder = new UriBuilder(uri)
        {
            Host = uri.Host.ToLowerInvariant(),
            Path = uri.AbsolutePath.TrimEnd('/'),
            Query = string.Empty,
            Fragment = string.Empty
        };

        if (string.IsNullOrEmpty(builder.Path))
        {
            builder.Path = "/";
        }

        var absolute = builder.Uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
        normalizedUrl = absolute;
        return true;
    }

    public static string Normalize(string url)
    {
        if (!TryNormalize(url, out var normalizedUrl) || normalizedUrl is null)
        {
            throw new InvalidOperationException($"The environment URL '{url}' is not valid.");
        }

        return normalizedUrl;
    }
}
#nullable restore
