namespace XrmTools.Authentication;

using Microsoft.Identity.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
internal interface IAuthenticationService
{
    /// <summary>May return null when no token is available, for example during silent authentication.</summary>
    Task<AuthenticationResult?> AuthenticateAsync(
        DataverseEnvironment environment,
        bool allowInteraction,
        Action<string> onMessageForUser, CancellationToken cancellationToken);
}
