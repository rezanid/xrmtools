namespace XrmTools.Authentication;

using Microsoft.Identity.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

internal interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(
        DataverseEnvironment environment,
        bool allowInteraction,
        Action<string> onMessageForUser, CancellationToken cancellationToken);
}