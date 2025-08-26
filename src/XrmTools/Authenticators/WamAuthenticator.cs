namespace XrmTools.Authentication;
/* WAM (Windows Authentication Manager) Authenticator
   fails to authenticate when used in Visual Studio 2022, because it is not able to load 
   the "msalruntime_arm64.dll from runtimes\win-arm64\native directory.
*/
/* NOTE! Only versio 7.74.1 is compamtible with VS2022, and even that fails during authentication. */

using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Broker;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Microsoft.Identity.Client.Extensions.Msal;

internal class WamAuthenticator : DelegatingAuthenticator
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
            async (k, ct) => await CreateWamPublicClientAsync(k, ct).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);

        if (clearTokenCache)
        {
            await ClearTokenCacheAsync(app).ConfigureAwait(false);
        }

        var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
        var firstAccount = accounts.FirstOrDefault(a => a.HomeAccountId?.TenantId == parameters.Tenant) ?? accounts.FirstOrDefault();

        try
        {
            result = await app.AcquireTokenSilent(parameters.Scopes, firstAccount)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);

            if (result != null) return result;
        }
        catch (MsalUiRequiredException)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                var vsUIShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));

                IntPtr hwnd = IntPtr.Zero;
                if (vsUIShell != null && 0 == vsUIShell.GetDialogOwnerHwnd(out var phwnd))
                {
                    hwnd = phwnd;
                }

                var interactiveBuilder = app.AcquireTokenInteractive(parameters.Scopes)
                    .WithPrompt(Prompt.SelectAccount);

                if (firstAccount != null)
                {
                    interactiveBuilder = interactiveBuilder.WithAccount(firstAccount);
                }

                if (hwnd != IntPtr.Zero)
                {
                    interactiveBuilder = interactiveBuilder.WithParentActivityOrWindow(hwnd);
                }

                result = await interactiveBuilder.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                if (result != null) return result;
            }
            catch (MsalException)
            {
                //TODO: Logging
            }
        }
        catch (Exception)
        {
            //TODO: Logging
        }

        return result;
    }

    public override bool CanAuthenticate(AuthenticationParameters parameters) =>
        // WAM is for public client flows; exclude device flow and confidential client flows
        !parameters.UseDeviceFlow &&
        string.IsNullOrEmpty(parameters.ClientSecret) &&
        string.IsNullOrEmpty(parameters.CertificateThumbprint) &&
        (parameters.UseCurrentUser || parameters.IsUncertainAuthFlow());

    private static async Task<IPublicClientApplication> CreateWamPublicClientAsync(
        AuthenticationParameters parameters,
        CancellationToken cancellationToken)
    {
        var builder = PublicClientApplicationBuilder.Create(parameters.ClientId);

        if (!string.IsNullOrEmpty(parameters.Authority)) builder = builder.WithAuthority(parameters.Authority);
        if (!string.IsNullOrEmpty(parameters.Tenant)) builder = builder.WithTenantId(parameters.Tenant);

        // Prefer a default redirect URI for WAM if none provided
        if (!string.IsNullOrEmpty(parameters.RedirectUri))
        {
            builder = builder.WithRedirectUri(parameters.RedirectUri);
        }
        else
        {
            builder = builder.WithDefaultRedirectUri();
        }

        // Enable WAM (Windows broker)

        // With the following code:
        builder = Microsoft.Identity.Client.Broker.BrokerExtension.WithBroker(builder, new BrokerOptions(BrokerOptions.OperatingSystems.Windows));
        //builder = builder.WithBroker(new Microsoft.Identity.Client.BrokerOptions(Microsoft.Identity.Client.BrokerOptions.OperatingSystems.Windows));

        var publicClientApp = builder.WithLogging((level, message, pii) =>
        {
            // TODO: Replace the following line when logging is in-place.
            // PartnerSession.Instance.DebugMessages.Enqueue($"[MSAL] {level} {message}");
        }).Build();

        // Persistent cache registration (same approach as DelegatingAuthenticator.CreatePublicClientAsync)
        var storageProperties = new StorageCreationPropertiesBuilder("msal_cache.dat",
            MsalCacheHelper.UserRootDirectory)
            .Build();

        var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
        // cacheHelper.VerifyPersistence(); // Optional; disabled in DelegatingAuthenticator
        cacheHelper.RegisterCache(publicClientApp.UserTokenCache);

        return publicClientApp;
    }
}
