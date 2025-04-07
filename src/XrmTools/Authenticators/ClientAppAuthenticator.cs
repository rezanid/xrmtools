namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class ClientAppAuthenticator : DelegatingAuthenticator
{
    public override async Task<AuthenticationResult> AuthenticateAsync(AuthenticationParameters parameters, bool clearTokenCache, Action<string> onMessageForUser = default, CancellationToken cancellationToken = default)
    {
        var app = await CreateClientAppAsync(parameters, cancellationToken);

        //TODO: Implement logging
        //ServiceClientTracing.Information($"[DeviceCodeAuthenticator] Calling AcquireTokenWithDeviceCode - Scopes: '{string.Join(", ", parameters.Scopes)}'");

        return await app.AsConfidentialClient().AcquireTokenForClient(parameters.Scopes).ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }

    public override bool CanAuthenticate(AuthenticationParameters parameters) =>
        !parameters.UseDeviceFlow &&
        !string.IsNullOrEmpty(parameters.ClientId) &&
        (!string.IsNullOrEmpty(parameters.ClientSecret) || !string.IsNullOrEmpty(parameters.CertificateThumbprint));

}