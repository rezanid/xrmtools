namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Http;
using XrmTools.Tokens;
using System.Net.Http;
using System.ComponentModel.Composition;
using XrmTools.Options;

[Export(typeof(IAuthenticationService))]
internal class AuthenticationService : IAuthenticationService
{
    bool cleanTokenCache = false;

    private readonly ITokenExpanderService tokenExpander;
    private readonly Lazy<IXrmHttpClientFactory> httpClientFactory;

    [ImportingConstructor]
    public AuthenticationService(
        ITokenExpanderService tokenExpander,
        Lazy<IXrmHttpClientFactory> httpClientFactory)
    {
        this.tokenExpander = tokenExpander;
        this.httpClientFactory = httpClientFactory;
        GeneralOptions.Saved += (options) =>
        {
            cleanTokenCache = true;
        };
    }

    public IAuthenticator Authenticator { get; set; } = new ClientAppAuthenticator
    {
        NextAuthenticator = new DeviceCodeAuthenticator
        {
            NextAuthenticator = new IntegratedAuthenticator()
        }
    };

    private readonly IAuthenticator wamAuthenticator = new WamAuthenticator();

    public async Task<AuthenticationResult> AuthenticateAsync(
        DataverseEnvironment environment,
        bool allowInteraction,
        Action<string> onMessageForUser = default, CancellationToken cancellationToken = default)
    {
        if (environment == null) { throw new ArgumentNullException(nameof(environment)); }
        if (!environment.IsValid) 
        { 
            throw new InvalidOperationException("Authentication failed. The current environment is not valid. Please make sure the environment configuration is correct in Tools > Options > Xrm Tools."); 
        }
        var connectionString = tokenExpander.ExpandTokens(environment.ConnectionString);
        var authParams = await AuthenticationParameterResolver.EnsureTenantAsync(
            AuthenticationParameters.Parse(connectionString),
            httpClientFactory.Value,
            cancellationToken).ConfigureAwait(false);
        var options = await GeneralOptions.GetLiveInstanceAsync();
        if (allowInteraction && options?.UseWindowsAccountManager == true && wamAuthenticator.CanAuthenticate(authParams))
        {
            var wamResult = await wamAuthenticator.AuthenticateAsync(authParams, cleanTokenCache, onMessageForUser, cancellationToken).ConfigureAwait(false);
            if (wamResult != null)
            {
                cleanTokenCache = false;
                return wamResult;
            }
        }

        var current = Authenticator;
        while (current != null && !current.CanAuthenticate(authParams)) current = current.NextAuthenticator;
        if (current == null)
        {
            throw new InvalidOperationException("Unable to detect required authentication flow. Please check the input parameters and try again.");
        }

        var result = await current?.AuthenticateAsync(authParams, cleanTokenCache, onMessageForUser, cancellationToken);

        cleanTokenCache = false;

        return result;
    }
}
