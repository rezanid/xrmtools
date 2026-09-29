namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Broker;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
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
        try
        {
            var app = await apps.GetOrAddAsync(
                parameters,
                async (key, token) => await CreateWamPublicClientAsync(key, token).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

            if (clearTokenCache) await ClearTokenCacheAsync(app).ConfigureAwait(false);

            var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
            var firstAccount = accounts.FirstOrDefault(account => account.HomeAccountId?.TenantId == parameters.Tenant) ?? accounts.FirstOrDefault();
            try
            {
                return await app.AcquireTokenSilent(parameters.Scopes, firstAccount)
                    .ExecuteAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (MsalUiRequiredException)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                var vsUIShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
                IntPtr hwnd = IntPtr.Zero;
                if (vsUIShell != null && vsUIShell.GetDialogOwnerHwnd(out var candidate) == 0) hwnd = candidate;

                var interactive = app.AcquireTokenInteractive(parameters.Scopes).WithPrompt(Prompt.SelectAccount);
                if (firstAccount != null) interactive = interactive.WithAccount(firstAccount);
                if (hwnd != IntPtr.Zero) interactive = interactive.WithParentActivityOrWindow(hwnd);
                return await interactive.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // WAM is optional. A broker, app-registration, or native-runtime failure must not block sign-in.
            Debug.WriteLine($"WAM authentication is unavailable; falling back to the system browser. {ex.Message}");
            return null;
        }
    }

    public override bool CanAuthenticate(AuthenticationParameters parameters) => CanUseWam(parameters);

    internal static bool CanUseWam(AuthenticationParameters parameters) =>
        // WAM is for public client flows; exclude device flow and confidential client flows
        !parameters.UseDeviceFlow &&
        string.IsNullOrEmpty(parameters.ClientSecret) &&
        string.IsNullOrEmpty(parameters.CertificateThumbprint) &&
        (parameters.UseCurrentUser || parameters.IsUncertainAuthFlow());

    internal static async Task<IPublicClientApplication> CreateWamPublicClientAsync(
        AuthenticationParameters parameters,
        CancellationToken cancellationToken)
    {
        var builder = PublicClientApplicationBuilder.Create(parameters.ClientId);

        if (!string.IsNullOrEmpty(parameters.Authority)) builder = builder.WithAuthority(parameters.Authority);
        if (!string.IsNullOrEmpty(parameters.Tenant)) builder = builder.WithTenantId(parameters.Tenant);

        // WAM selects its broker redirect URI. It must be registered as
        // ms-appx-web://microsoft.aad.brokerplugin/{client-id} in Entra ID.
        builder = Microsoft.Identity.Client.Broker.BrokerExtension.WithBroker(builder, new BrokerOptions(BrokerOptions.OperatingSystems.Windows));

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
