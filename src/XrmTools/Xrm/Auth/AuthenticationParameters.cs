namespace XrmTools.Xrm.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;

internal class AuthenticationParameters
{
    public string Authority { get; set; }
    public string Resource { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string CertificateThumbprint { get; set; }
    public string TenantId { get; set; }
    public IEnumerable<string> Scopes { get; set; }
    public bool UseDeviceFlow { get; set; }
    public bool UseCurrentUser { get; set; }
    public string RedirectUri { get; set; }

    public IAccount Account { get; set; }

    public static AuthenticationParameters Parse(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
        var connectionProperties = 
            connectionString.Split([';'], StringSplitOptions.RemoveEmptyEntries)
            .ToDictionary(
                s => s[..s.IndexOf('=')].Trim(), 
                s => s[(s.IndexOf('=') + 1)..].Trim(), 
                StringComparer.OrdinalIgnoreCase);
        var resource = connectionProperties.TryGetValue("resource", out var url) 
            ? url 
            : connectionProperties.TryGetValue("url", out url) 
            ? url 
            : throw new ArgumentException("Connection string should contain either Url or Resource. Both are missing.");
        if (string.IsNullOrEmpty(resource))
        {
            throw new ArgumentException("Either Resource or Url is required.");
        }
        if (!resource.EndsWith("/")) resource += "/";
        var parameters = new AuthenticationParameters
        {
            Authority = connectionProperties.TryGetValue("authority", out var authority) ? authority : null,
            ClientId = connectionProperties.TryGetValue("clientid", out var clientid) ? clientid : "51f81489-12ee-4a9e-aaae-a2591f45987d",
            RedirectUri = connectionProperties.TryGetValue("redirecturi", out var redirecturi) ? redirecturi : "app://58145B91-0C36-4500-8554-080854F2AC97",
            Resource = resource,
            ClientSecret = connectionProperties.TryGetValue("clientsecret", out var secret) ? secret : null,
            CertificateThumbprint = connectionProperties.TryGetValue("thumbprint", out var thumbprint) ? thumbprint : null,
            TenantId = connectionProperties.TryGetValue("tenantid", out var tenantid) ? tenantid : null,
            Scopes = connectionProperties.TryGetValue("scopes", out var scopes) ? scopes.Split(',') : [new Uri(new Uri(resource, UriKind.Absolute), ".default").ToString()],
            UseDeviceFlow = connectionProperties.TryGetValue("device", out var device) && bool.Parse(device),
            UseCurrentUser = connectionProperties.TryGetValue("integrated security", out var defaultcreds) && bool.Parse(defaultcreds)
        };
        if (string.IsNullOrEmpty(parameters.Authority) && string.IsNullOrEmpty(parameters.TenantId))
        {
            throw new ArgumentException("Either Authority or TenantId must be provided.");
        }
        if (string.IsNullOrEmpty(parameters.Authority))
        {
            parameters.Authority = $"https://login.microsoftonline.com/{parameters.TenantId}/oauth2/authorize";
        }
        return parameters;
    }
}