namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class IntegratedAuthenticator : DelegatingAuthenticator
{
    public override async Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationParameters parameters,
        bool clearTokenCache,
        Action<string> onMessageForUser = default,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Fresh app per attempt to avoid sticky broker state
        // Previously we cached the app instances in an AsyncDictionary<AuthenticationParameters, IPublicClientApplication>
        var app = (await CreateClientAppAsync(parameters, cancellationToken)).AsPublicClient();

        if (clearTokenCache)
        {
            await ClearTokenCacheAsync(app).ConfigureAwait(false);
        }

        var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
        var firstAccount = accounts.FirstOrDefault(a => string.Equals(a.HomeAccountId?.TenantId, parameters.Tenant, StringComparison.OrdinalIgnoreCase));

        try
        {
            return await app.AcquireTokenSilent(parameters.Scopes, firstAccount)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (MsalUiRequiredException)
        {
            // continue to interactive
        }
        catch (OperationCanceledException)
        {
            throw;
        }

        // Interactive using embedded web view to avoid WAM hangs
        // Add a hard timeout guard so we never wait forever if the broker/webview fails to signal cancel on repeated attempts
        var builder = app.AcquireTokenInteractive(parameters.Scopes)
            .WithPrompt(Prompt.SelectAccount)
            .WithUseEmbeddedWebView(true);

        var execTask = builder.ExecuteAsync(cancellationToken);
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(60));
        var completed = await Task.WhenAny(execTask, timeoutTask).ConfigureAwait(false);
        if (completed != execTask)
        {
            throw new OperationCanceledException("Interactive authentication timed out.", cancellationToken);
        }

        try
        {
            return await execTask.ConfigureAwait(false);
        }
        catch (MsalException ex) when (ex.ErrorCode == "authentication_canceled" || ex.ErrorCode == "access_denied" || ex.ErrorCode == "user_canceled")
        {
            throw new OperationCanceledException("User cancelled the authentication.", ex, cancellationToken);
        }
    }

    public override bool CanAuthenticate(AuthenticationParameters parameters)
        => parameters.UseCurrentUser || parameters.IsUncertainAuthFlow();
}