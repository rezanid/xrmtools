namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class IntegratedAuthenticator : DelegatingAuthenticator
{
    //TODO: The following dictionary is IDisposable.
    private readonly AsyncDictionary<AuthenticationParameters, IPublicClientApplication> apps = new();

    public override async Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationParameters parameters,
        bool clearTokenCache,
        Action<string> onMessageForUser = default, 
        CancellationToken cancellationToken = default)
    {
        AuthenticationResult result = null;
        var app = await apps.GetOrAddAsync(
            parameters, 
            async (k, ct) => (await CreateClientAppAsync(k, ct)).AsPublicClient(),
            cancellationToken);

        if (clearTokenCache)
        {
            await ClearTokenCacheAsync(app).ConfigureAwait(false);
        }

        var accounts = await app.GetAccountsAsync();
        var firstAccount = accounts.FirstOrDefault(a => a.HomeAccountId.TenantId == parameters.Tenant);

        try
        {
            result = await app.AcquireTokenSilent(parameters.Scopes, firstAccount)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (MsalUiRequiredException)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var vsUIShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
                if (0 != vsUIShell.GetDialogOwnerHwnd(out var phwnd)) return null;
                result = await app.AcquireTokenInteractive(parameters.Scopes)
                    .WithAccount(accounts.FirstOrDefault())
                    .WithPrompt(Prompt.SelectAccount)
                    .WithParentActivityOrWindow(phwnd)
                    .ExecuteAsync().ConfigureAwait(false);
            }
            catch (MsalException)
            {
                //TODO: Logging
            }
        }
        catch (Exception)
        {
            //TODO: Logging.
        }
        if (result == null)
        {
            return await app.AcquireTokenByIntegratedWindowsAuth(parameters.Scopes)
                .ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }
        return result;
    }
    public override bool CanAuthenticate(AuthenticationParameters parameters) 
        => parameters.UseCurrentUser || parameters.IsUncertainAuthFlow();
}