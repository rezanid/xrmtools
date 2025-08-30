namespace XrmTools.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

internal record AuthenticationParameters
{
    public const string DefaultClientId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
    public const string DefaultRedirectUrl = "app://58145B91-0C36-4500-8554-080854F2AC97";

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

    public static bool TryParse(string connectionString, out AuthenticationParameters authenticationParameters)
    {
        try
        {
            authenticationParameters = Parse(connectionString);
            return true;
        }
        catch {  }
        authenticationParameters = null;
        return false;
    }

    public static AuthenticationParameters Parse(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
        string resource = null;
        Dictionary<string, string> dictionary = null;
        if (connectionString.IndexOf('=') <= 0)
        {
            if (connectionString.StartsWith("https://"))
            {
                resource = connectionString;
                if (!resource.EndsWith("/")) resource += "/";
                dictionary = new Dictionary<string, string>() { ["resource"] = connectionString, ["integrated security"] = true.ToString() };
            }
            else
            {
                throw new InvalidOperationException("Connection string is invalid. Please check your environment setting in Tools > Options > Xrm Tools");
            }
        }
        else
        {
            dictionary =
                connectionString.Split([';'], StringSplitOptions.RemoveEmptyEntries)
                .ToDictionary(
                    s => s[..s.IndexOf('=')].Trim(),
                    s => s[(s.IndexOf('=') + 1)..].Trim(),
                    StringComparer.OrdinalIgnoreCase);
            resource = 
                dictionary.TryGetValue("resource", out var url) 
                ? url 
                : dictionary.TryGetValue("url", out url) 
                ? url 
                : throw new ArgumentException("Connection string should contain either Url or Resource. Both are missing.");
        }
        if (string.IsNullOrEmpty(resource))
        {
            throw new ArgumentException("Either Resource or Url is required.");
        }
        if (!resource.EndsWith("/")) resource += "/";

        var parameters = new AuthenticationParameters
        {
            Authority = dictionary.TryGetValue("authority", out var authority) ? authority : null,
            ClientId = dictionary.TryGetValue("clientid", out var clientid) ? clientid : DefaultClientId,
            RedirectUri = dictionary.TryGetValue("redirecturi", out var redirecturi) ? redirecturi : DefaultRedirectUrl,
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
        if (string.IsNullOrEmpty(parameters.Authority) && !string.IsNullOrEmpty(parameters.Tenant))
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

    public string Build()
    {
        // If only Resource and UseCurrentUser (integrated security) are set, use the simplified form
        bool isDefaultClientId = string.Equals(ClientId, DefaultClientId, StringComparison.OrdinalIgnoreCase);
        bool isDefaultRedirect = string.Equals(RedirectUri, DefaultRedirectUrl, StringComparison.OrdinalIgnoreCase);
        bool hasNoSecret = string.IsNullOrEmpty(ClientSecret) && string.IsNullOrEmpty(CertificateThumbprint);
        bool hasNoAuthority = string.IsNullOrEmpty(Authority);
        bool hasNoTenant = string.IsNullOrEmpty(Tenant);
        bool hasNoScopes = Scopes == null || (Scopes.Count() == 1 && Scopes.First().Equals(new Uri(new Uri(Resource, UriKind.Absolute), ".default").ToString(), StringComparison.OrdinalIgnoreCase));
        bool isIntegrated = UseCurrentUser && hasNoSecret && hasNoAuthority && hasNoTenant && isDefaultClientId && isDefaultRedirect && hasNoScopes;

        if (!string.IsNullOrEmpty(Resource) && isIntegrated)
        {
            // Use the simplified form
            return Resource.TrimEnd('/');
        }

        var parts = new List<string>();

        if (!string.IsNullOrEmpty(Authority))
            parts.Add($"Authority={Authority}");

        if (!string.IsNullOrEmpty(ClientId) && !isDefaultClientId)
            parts.Add($"ClientId={ClientId}");

        if (!string.IsNullOrEmpty(ClientSecret))
            parts.Add($"ClientSecret={ClientSecret}");

        if (!string.IsNullOrEmpty(CertificateThumbprint))
            parts.Add($"Thumbprint={CertificateThumbprint}");

        if (CertificateStoreName != default && CertificateStoreName != StoreName.My)
            parts.Add($"CertificateStore={CertificateStoreName}");

        if (!string.IsNullOrEmpty(Tenant))
            parts.Add($"TenantId={Tenant}");

        if (!isDefaultRedirect && !string.IsNullOrEmpty(RedirectUri))
            parts.Add($"RedirectUri={RedirectUri}");

        if (Scopes != null && !hasNoScopes)
            parts.Add($"Scopes={string.Join(",", Scopes)}");

        if (UseDeviceFlow)
            parts.Add("Device=true");

        if (UseCurrentUser)
            parts.Add("Integrated Security=true");

        if (!string.IsNullOrEmpty(Resource))
        {
            if (parts.Count == 0)
            {
                return Resource.TrimEnd('/');
            }
            parts.Add($"Uri={Resource.TrimEnd('/')}");
        }

        return string.Join(";", parts);
    }

    public bool IsUncertainAuthFlow()
        => string.IsNullOrEmpty(ClientSecret) && !string.IsNullOrEmpty(CertificateThumbprint) && !UseDeviceFlow;
}