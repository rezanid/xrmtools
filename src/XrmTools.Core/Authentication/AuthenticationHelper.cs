namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

internal static class AuthenticationHelper
{
    public static AuthenticationResult Authenticate(AuthenticationParameters connectionString)
    {
        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }
        if (!string.IsNullOrEmpty(connectionString.CertificateThumbprint))
        {
            var certificate = FindCertificate(connectionString.CertificateThumbprint, StoreName.My) ?? throw new InvalidOperationException($"No certificate found with thumbprint '{connectionString.CertificateThumbprint}'.");
            return Authenticate(connectionString.Authority, connectionString.ClientId, connectionString.Resource, certificate);
        }
        return Authenticate(connectionString.Authority, connectionString.ClientId, connectionString.Resource, connectionString.ClientSecret);
    }

    public static AuthenticationResult Authenticate(string authority, string clientId, string resource, X509Certificate2 clientCert)
    {
        var confidentialClient = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithCertificate(clientCert)
            .WithAuthority(authority)
            .Build();
        return confidentialClient.AcquireTokenForClient(new string[] { $"{resource}/.default" }).ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static AuthenticationResult Authenticate(string authority, string clientId, string resource, string clientSecret)
    {
        var confidentialClient = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(authority)
            .Build();
        return confidentialClient.AcquireTokenForClient(new string[] { $"{resource}/.default" }).ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

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