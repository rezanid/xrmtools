namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

internal abstract class DelegatingAuthenticator : IAuthenticator
{
    private AuthenticationParameters lastParameters;
    private IClientApplicationBase lastClientApp;

    public IAuthenticator NextAuthenticator { get; set; }

    public virtual async Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationParameters parameters, Action<string> onMessageForUser = default, CancellationToken cancellationToken = default)
    {
        var app = await GetClientAppAsync(parameters, cancellationToken);
        var accounts = await app.GetAccountsAsync();
        var firstAccount = accounts.FirstOrDefault();

        try
        {
            return await app.AcquireTokenSilent(parameters.Scopes, firstAccount)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (MsalUiRequiredException ex)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var vsUIShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
                if (0 != vsUIShell.GetDialogOwnerHwnd(out var phwnd)) return null;
                if (app.AsPublicClient() is PublicClientApplication appClient)
                {
                    return await appClient.AcquireTokenInteractive(parameters.Scopes)
                        .WithAccount(accounts.FirstOrDefault())
                        .WithPrompt(Prompt.SelectAccount)
                        .WithParentActivityOrWindow(phwnd)
                        .ExecuteAsync();
                }
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

        return null;
    }

    public abstract bool CanAuthenticate(AuthenticationParameters parameters);

    public virtual async Task<IClientApplicationBase> GetClientAppAsync(AuthenticationParameters parameters, CancellationToken cancellationToken)
    {
        if (lastParameters == parameters && lastClientApp != null) return lastClientApp;
        lastParameters = parameters;

        if (!parameters.UseDeviceFlow & (
            !string.IsNullOrEmpty(parameters.CertificateThumbprint) ||
            !string.IsNullOrEmpty(parameters.ClientSecret)))
        {
            return lastClientApp = CreateConfidentialClient(
                parameters.Authority,
                parameters.ClientId,
                parameters.ClientSecret,
                FindCertificate(parameters.CertificateThumbprint, parameters.CertificateStoreName),
                parameters.RedirectUri,
                parameters.Tenant);
        }

        return lastClientApp = await CreatePublicClientAsync(
            parameters.Authority,
            parameters.ClientId,
            parameters.RedirectUri,
            parameters.Tenant);
    }

    public async Task<AuthenticationResult> TryAuthenticateAsync(
        AuthenticationParameters parameters, Action<string> onMessageForUser = default, CancellationToken cancellationToken = default)
    {
        if (CanAuthenticate(parameters))
        {
            return await AuthenticateAsync(parameters, onMessageForUser, cancellationToken).ConfigureAwait(false);
        }

        if (NextAuthenticator != null)
        {
            return await NextAuthenticator.TryAuthenticateAsync(parameters, onMessageForUser, cancellationToken).ConfigureAwait(false);
        }

        return null;
    }

    private static IConfidentialClientApplication CreateConfidentialClient(
        string authority,
        string clientId = null,
        string clientSecret = null,
        X509Certificate2 certificate = null,
        string redirectUri = null,
        string tenantId = null)
    {
        var builder = ConfidentialClientApplicationBuilder.Create(clientId);

        builder = builder.WithAuthority(authority);

        if (!string.IsNullOrEmpty(clientSecret))
        { builder = builder.WithClientSecret(clientSecret); }

        if (certificate != null)
        { builder = builder.WithCertificate(certificate); }

        if (!string.IsNullOrEmpty(redirectUri))
        { builder = builder.WithRedirectUri(redirectUri); }

        if (!string.IsNullOrEmpty(tenantId))
        { builder = builder.WithTenantId(tenantId); }

        var client = builder.WithLogging((level, message, pii) =>
        {
            //TODO: Replace the following line when logging is in-place.
            //PartnerSession.Instance.DebugMessages.Enqueue($"[MSAL] {level} {message}");
        }).Build();

        return client;
    }

    private static async Task<IPublicClientApplication> CreatePublicClientAsync(
        string authority,
        string clientId = null,
        string redirectUri = null,
        string tenantId = null)
    {
        var builder = PublicClientApplicationBuilder.Create(clientId);
        
        if (!string.IsNullOrEmpty(authority)) builder.WithAuthority(authority);
        if (!string.IsNullOrEmpty(redirectUri)) builder.WithRedirectUri(redirectUri);
        if (!string.IsNullOrEmpty(tenantId)) builder.WithTenantId(tenantId);

        var publicClientApp = builder.WithLogging((level, message, pii) =>
        {
            // TODO: Replace the following line when logging is in-place.
            // PartnerSession.Instance.DebugMessages.Enqueue($"[MSAL] {level} {message}");
        }).Build();

        var storageProperties = new StorageCreationPropertiesBuilder("msal_cache.dat",
                MsalCacheHelper.UserRootDirectory)
            // No need to support non-Windows platforms yet.
            //.WithLinuxKeyring(
            //    "com.vs.xrmtools", MsalCacheHelper.LinuxKeyRingDefaultCollection, "Xrm Tools Credentials", 
            //    new("Version", "1"), new("Product", "Xrm Tools"))
            //.WithMacKeyChain("xrmtools_msal_service", "xrmtools_msa_account")
            .Build();

        // Create and register the cache helper to enable persistent caching
        var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
        //TODO: persisting token cache didn't work well in Win RT.
        //cacheHelper.VerifyPersistence();
        cacheHelper.RegisterCache(publicClientApp.UserTokenCache);

        return publicClientApp;
    }

    public static X509Certificate2 FindCertificate(
        string thumbprint,
        StoreName storeName)
    {
        if (thumbprint == null) return null;

        var source = new StoreLocation[2] { StoreLocation.CurrentUser, StoreLocation.LocalMachine };
        X509Certificate2 certificate = null;
        if (source.Any(storeLocation => TryFindCertificatesInStore(thumbprint, storeLocation, storeName, out certificate)))
        {
            return certificate;
        }
        return null;
    }

    private static bool TryFindCertificatesInStore(string thumbprint, StoreLocation location, StoreName storeName, out X509Certificate2 certificate)
    {
        X509Store store = null;
        X509Certificate2Collection col;

        if (string.IsNullOrEmpty(thumbprint)) throw new ArgumentNullException(nameof(thumbprint));

        try
        {
            store = new X509Store(storeName, location);
            store.Open(OpenFlags.ReadOnly);

            col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            certificate = col.Count == 0 ? null : col[0];

            return col.Count > 0;
        }
        finally
        {
            store?.Close();
        }
    }
}