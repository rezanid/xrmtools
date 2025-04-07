namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class DeviceCodeAuthenticator : DelegatingAuthenticator
{
    public override async Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationParameters parameters,
        bool clearTokenCache,
        Action<string> onMessageForUser = default,
        CancellationToken cancellationToken = default)
    {
        var app = await CreateClientAppAsync(parameters, cancellationToken);

        // Attempt to get a token silently from the cache
        var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
        var account = accounts.FirstOrDefault();
        if (account != null)
        {
            try
            {
                var silentResult = await app.AcquireTokenSilent(parameters.Scopes, account).ExecuteAsync(cancellationToken).ConfigureAwait(false);
                if (silentResult != null) { return silentResult; }
            }
            catch (MsalUiRequiredException)
            {
                // Silent acquisition failed, user interaction required
            }
        }

        return await app.AsPublicClient().AcquireTokenWithDeviceCode(parameters.Scopes, callback =>
        {
            // Provide the user instructions
            onMessageForUser(callback.Message);
            return Task.CompletedTask;
        }).ExecuteAsync(cancellationToken);
    }

    public override bool CanAuthenticate(AuthenticationParameters parameters) =>
        parameters.UseDeviceFlow &&
        !string.IsNullOrEmpty(parameters.ClientId);
}