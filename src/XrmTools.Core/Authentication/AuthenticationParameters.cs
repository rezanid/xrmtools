namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

internal record AuthenticationParameters
{
    public string Authority { get; set; }
    public string Resource { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string CertificateThumbprint { get; set; }
    public StoreName CertificateStoreName { get; set; }
    public string Tenant { get; set; }
    public IEnumerable<string> Scopes { get; set; }
    public bool UseDeviceFlow { get; set; }
    public bool UseCurrentUser { get; set; }
    public string RedirectUri { get; set; }

    public IAccount Account { get; set; }

    public static AuthenticationParameters Parse(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
        var dictionary =
            connectionString.Split([';'], StringSplitOptions.RemoveEmptyEntries)
            .ToDictionary(
                s => s[..s.IndexOf('=')].Trim(), 
                s => s[(s.IndexOf('=') + 1)..].Trim(),
                StringComparer.OrdinalIgnoreCase);
        var resource = dictionary.TryGetValue("resource", out var url) 
            ? url 
            : dictionary.TryGetValue("url", out url) 
            ? url 
            : throw new ArgumentException("Connection string should contain either Url or Resource. Both are missing.");
        if (string.IsNullOrEmpty(resource))
        {
            throw new ArgumentException("Either Resource or Url is required.");
        }
        if (!resource.EndsWith("/")) resource += "/";

        var parameters = new AuthenticationParameters
        {
            Authority = dictionary.TryGetValue("authority", out var authority) ? authority : null,
            ClientId = dictionary.TryGetValue("clientid", out var clientid) ? clientid : "51f81489-12ee-4a9e-aaae-a2591f45987d",
            RedirectUri = dictionary.TryGetValue("redirecturi", out var redirecturi) ? redirecturi : "app://58145B91-0C36-4500-8554-080854F2AC97",
            Resource = resource,
            ClientSecret = dictionary.TryGetValue("clientsecret", out var secret) ? secret : null,
            CertificateThumbprint = dictionary.TryGetValue("thumbprint", out var thumbprint) ? thumbprint : null,
            Tenant = dictionary.TryGetValue("tenantid", out var tenant) 
            ? tenant 
            : dictionary.TryGetValue("tenant", out tenant) 
            ? tenant 
            : null,
            Scopes = dictionary.TryGetValue("scopes", out var scopes) ? scopes.Split(',') : [new Uri(new Uri(resource, UriKind.Absolute), ".default").ToString()],
            UseDeviceFlow = dictionary.TryGetValue("device", out var device) && bool.Parse(device),
            UseCurrentUser = dictionary.TryGetValue("integrated security", out var defaultcreds) && bool.Parse(defaultcreds)
        };
        if (string.IsNullOrEmpty(parameters.Authority) && string.IsNullOrEmpty(parameters.Tenant))
        {
            throw new ArgumentException("Either Authority or TenantId must be provided.");
        }
        if (string.IsNullOrEmpty(parameters.Authority))
        {
            parameters.Authority = $"https://login.microsoftonline.com/{parameters.Tenant}/oauth2/authorize";
        }
        return parameters;
    }

    private StoreName ExtractStoreName(Dictionary<string, string> parameters)
    {

        if (parameters.TryGetValue("certificatestore", out var certificateStore))
        {
            if (Enum.TryParse(certificateStore, true, out StoreName storeName))
            {
                return storeName;
            }
            else
            {
                //TODO: Log warning.
            }
        }
        return StoreName.My;
    }

    public bool IsUncertainAuthFlow()
        => string.IsNullOrEmpty(ClientSecret) && !string.IsNullOrEmpty(CertificateThumbprint) && !UseDeviceFlow;
}