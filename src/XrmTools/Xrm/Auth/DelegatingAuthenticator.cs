using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace XrmTools.Xrm.Auth;
internal abstract class DelegatingAuthenticator : IAuthenticator
{
    public IAuthenticator NextAuthenticator { get; set; }

    public virtual async Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationParameters parameters, Action<string> onMessageForUser = default, CancellationToken cancellationToken = default)
    {
        var app = GetClient(parameters);

        var account = parameters.Account ?? (await app.GetAccountsAsync()).FirstOrDefault();
        if (account == null) { return null; }

        try
        {
            return await app.AcquireTokenSilent(parameters.Scopes, account)
                .ExecuteAsync(CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (MsalUiRequiredException)
        {
            //TODO: Needs logging
        }

        return null;
    }

    public abstract bool CanAuthenticate(AuthenticationParameters parameters);

    public virtual IClientApplicationBase GetClient(AuthenticationParameters parameters, string redirectUri = null)
    {
        if (!parameters.UseDeviceFlow & (
            !string.IsNullOrEmpty(parameters.CertificateThumbprint) ||
            !string.IsNullOrEmpty(parameters.ClientSecret)))
        {
            return CreateConfidentialClient(
                parameters.Authority,
                parameters.ClientId,
                parameters.ClientSecret,
                FindCertificate(parameters.CertificateThumbprint),
                redirectUri,
                parameters.TenantId);
        }

        return CreatePublicClient(
            parameters.Authority,
            parameters.ClientId,
            redirectUri,
            parameters.TenantId);
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

    private static IPublicClientApplication CreatePublicClient(
        string authority,
        string clientId = null,
        string redirectUri = null,
        string tenantId = null)
    {
        var builder = PublicClientApplicationBuilder.Create(clientId)
            .WithAuthority(authority);

        if (!string.IsNullOrEmpty(redirectUri))
        { builder = builder.WithRedirectUri(redirectUri); }

        if (!string.IsNullOrEmpty(tenantId))
        { builder = builder.WithTenantId(tenantId); }

        var client = builder.WithLogging((level, message, pii) =>
        {
            // TODO: Replace the following line when logging is in-place.
            // PartnerSession.Instance.DebugMessages.Enqueue($"[MSAL] {level} {message}");
        }).Build();

        return client;
    }

    public static X509Certificate2 FindCertificate(string thumbprint) => FindCertificate(thumbprint, StoreName.My);

    public static X509Certificate2 FindCertificate(
        string thumbprint,
        StoreName storeName)
    {
        if (thumbprint == null)
        { return null; }

        var source = new StoreLocation[2] { StoreLocation.CurrentUser, StoreLocation.LocalMachine };
        X509Certificate2 certificate = null;
        if (source.Any(storeLocation => TryFindCertificatesInStore(thumbprint, storeLocation, storeName, out certificate)))
        {
            return certificate;
        }
        return null;
    }

    private static bool TryFindCertificatesInStore(string thumbprint, StoreLocation location, out X509Certificate2 certificate)
        => TryFindCertificatesInStore(thumbprint, location, StoreName.My, out certificate);

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