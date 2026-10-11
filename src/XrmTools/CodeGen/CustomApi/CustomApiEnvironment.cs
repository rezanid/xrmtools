#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using System;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.WebApi;

internal static class CustomApiEnvironment
{
    internal static async Task<Uri> RequireAsync(IWebApiService webApi, Uri expected, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var current = await webApi.GetBaseUrlAsync().ConfigureAwait(false) ?? throw new InvalidOperationException("Select a Dataverse environment first.");
        token.ThrowIfCancellationRequested();
        if (!string.Equals(Organization(current), Organization(expected), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The Dataverse environment changed. Start the operation again.");
        return current;
    }
    private static string Organization(Uri uri)
    {
        var value = uri.AbsoluteUri.TrimEnd('/');
        var index = value.IndexOf("/api/data/", StringComparison.OrdinalIgnoreCase);
        return index < 0 ? value : value.Substring(0, index);
    }
}
