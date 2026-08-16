namespace XrmTools.Authentication;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Http;

internal static class AuthenticationParameterResolver
{
    public static async Task<AuthenticationParameters> EnsureTenantAsync(
        AuthenticationParameters authParams,
        IXrmHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(authParams.Tenant))
        {
            var url = authParams.Resource;
            using var httpClient = await httpClientFactory.CreateClientAsync(DataverseEnvironment.Empty).ConfigureAwait(false);
            using var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken).ConfigureAwait(false);
            var authUrl = response.Headers.Location;
            if (authUrl != null)
            {
                var tenantSeparator = authUrl.AbsolutePath.IndexOf('/', 1);
                if (tenantSeparator > 1)
                {
                    var tenantId = authUrl.AbsolutePath[1..tenantSeparator];
                    authParams.Tenant = tenantId;
                    if (string.IsNullOrEmpty(authParams.Authority))
                    {
                        authParams.Authority = $"https://login.microsoftonline.com/{tenantId}";
                    }
                }
            }
        }

        return authParams;
    }
}
