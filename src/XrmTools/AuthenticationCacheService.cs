namespace XrmTools.Authentication;

using Microsoft.Identity.Client;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Http;
using XrmTools.Tokens;

[Export(typeof(IAuthenticationCacheService))]
internal class AuthenticationCacheService : IAuthenticationCacheService
{
    private readonly ITokenExpanderService tokenExpander;
    private readonly Lazy<IXrmHttpClientFactory> httpClientFactory;

    [ImportingConstructor]
    public AuthenticationCacheService(
        ITokenExpanderService tokenExpander,
        Lazy<IXrmHttpClientFactory> httpClientFactory)
    {
        this.tokenExpander = tokenExpander;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task ClearEnvironmentTokenCacheAsync(DataverseEnvironment environment, CancellationToken cancellationToken = default)
    {
        if (environment == null)
        {
            throw new ArgumentNullException(nameof(environment));
        }

        if (string.IsNullOrWhiteSpace(environment.ConnectionString))
        {
            return;
        }

        var connectionString = tokenExpander.ExpandTokens(environment.ConnectionString);
        var authParams = AuthenticationParameters.Parse(connectionString);
        if (!DelegatingAuthenticator.UsesPersistentUserTokenCache(authParams))
        {
            return;
        }

        authParams = await AuthenticationParameterResolver.EnsureTenantAsync(authParams, httpClientFactory.Value, cancellationToken).ConfigureAwait(false);
        var app = await DelegatingAuthenticator.CreatePublicClientAsync(
            authParams.Authority,
            authParams.ClientId,
            authParams.RedirectUri,
            authParams.Tenant).ConfigureAwait(false);

        await DelegatingAuthenticator.RemoveAccountsAsync(
            app,
            account => IsMatchingAccount(account, authParams),
            cancellationToken).ConfigureAwait(false);
    }

    private static bool IsMatchingAccount(IAccount account, AuthenticationParameters authParams)
    {
        if (account == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(authParams.Tenant))
        {
            return string.Equals(account.HomeAccountId?.TenantId, authParams.Tenant, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
