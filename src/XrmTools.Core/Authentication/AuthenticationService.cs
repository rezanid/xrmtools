namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Tokens;

internal class AuthenticationService(
    ITokenExpanderService tokenExpander,
    IAuthenticator authenticator) : IAuthenticationService
{
    public IAuthenticator Authenticator { get; set; } = authenticator;

    public async Task<AuthenticationResult> AuthenticateAsync(
        DataverseEnvironment environment,
        Action<string> onMessageForUser = default,
        CancellationToken cancellationToken = default)
    {
        if (environment == null) { throw new ArgumentNullException(nameof(environment)); }
        var current = Authenticator;
        var connectionString = tokenExpander.ExpandTokens(environment.ConnectionString);
        var authParams = AuthenticationParameters.Parse(connectionString);
        while (current != null && !current.CanAuthenticate(authParams))
        {
            current = current.NextAuthenticator;
        }
        if (current == null)
        {
            throw new InvalidOperationException("Unable to detect required authentication flow. Please check the input parameters and try again.");
        }
        return await current?.AuthenticateAsync(authParams, onMessageForUser, cancellationToken);
    }
}