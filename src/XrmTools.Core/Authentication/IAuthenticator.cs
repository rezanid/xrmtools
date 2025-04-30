namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

internal interface IAuthenticator
{
    IAuthenticator NextAuthenticator { get; set; }

    Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationParameters parameters, bool clearTokenCache, Action<string> onMessageForUser = default, CancellationToken cancellationToken = default);

    bool CanAuthenticate(AuthenticationParameters parameters);

    Task<AuthenticationResult> TryAuthenticateAsync(
        AuthenticationParameters parameters, bool clearTokenCache, Action<string> onMessageForUser = default, CancellationToken cancellationToken = default);
}