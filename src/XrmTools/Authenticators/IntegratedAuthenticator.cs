namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
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

        var appBase = await CreateClientAppAsync(parameters, cancellationToken).ConfigureAwait(false);
        var app = appBase.AsPublicClient()
                  ?? throw new InvalidOperationException("IntegratedAuthenticator requires a public client application.");

        if (clearTokenCache)
        {
            await ClearTokenCacheAsync(app).ConfigureAwait(false);
        }

        var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
        var firstAccount = accounts
            .FirstOrDefault(
            a => string.Equals(a.HomeAccountId?.TenantId,
                   parameters.Tenant,
                   StringComparison.OrdinalIgnoreCase));

        if (firstAccount is not null)
        {
            try
            {
                return await app.AcquireTokenSilent(parameters.Scopes, firstAccount)
                                .ExecuteAsync(cancellationToken)
                                .ConfigureAwait(false);
            }
            catch (MsalUiRequiredException)
            {
                // fall through to interactive
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        var builder = app.AcquireTokenInteractive(parameters.Scopes)
            .WithParentActivityOrWindow(WindowHelper.GetDialogOwnerHandle())
            .WithPrompt(Prompt.SelectAccount)
            .WithUseEmbeddedWebView(false);

        if (firstAccount is not null)
        {
            builder = builder.WithAccount(firstAccount);
        }

        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        // Prefer the system browser to avoid the legacy IE-based embedded web view on .NET Framework.
        var execTask = builder.ExecuteAsync(linkedCts.Token);
        var timeoutTask = Task.Delay(TimeSpan.FromMinutes(2));

        var completed = await Task.WhenAny(execTask, timeoutTask).ConfigureAwait(true);
        if (completed != execTask)
        {
            timeoutCts.Cancel();
            throw new OperationCanceledException("Interactive authentication timed out.", cancellationToken);
        }

        try
        {
            return await execTask.ConfigureAwait(true);
        }
        catch (MsalClientException ex) when (ex.ErrorCode == "loopback_redirect_uri")
        {
            throw new InvalidOperationException(
                "Interactive authentication requires a loopback redirect URI when using the system browser. Configure the app registration with RedirectUri=http://localhost, or omit RedirectUri to use the built-in loopback default.",
                ex);
        }
        catch (MsalException ex) when (
            ex.ErrorCode == "authentication_canceled" ||
            ex.ErrorCode == "access_denied" ||
            ex.ErrorCode == "user_canceled")
        {
            throw new OperationCanceledException("User cancelled the authentication.", ex, cancellationToken);
        }
    }

    public override bool CanAuthenticate(AuthenticationParameters parameters)
        => parameters.UseCurrentUser || parameters.IsUncertainAuthFlow();
}