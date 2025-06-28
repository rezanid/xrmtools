namespace XrmTools.UI;
using XrmTools.Authentication;
using System;

internal class EnvironmentModel : ViewModelBase
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private bool _useConnectionString;
    public bool UseConnectionString
    {
        get => _useConnectionString;
        set => SetProperty(ref _useConnectionString, value);
    } 

    private string _connectionString;
    public string ConnectionString
    {
        get => _connectionString;
        set
        {
            if (SetProperty(ref _connectionString, value))
                UpdatePropertiesFromConnectionString();
        }
    }

    private string _authType;
    public string AuthType
    {
        get => _authType;
        set
        {
            if (SetProperty(ref _authType, value))
            {
                UpdateConnectionStringFromProperties();
                OnPropertyChanged(nameof(IsCertificateAuth));
                OnPropertyChanged(nameof(IsClientAuth));
                OnPropertyChanged(nameof(IsInteractiveAuth));
                if (IsCertificateAuth)
                {
                    ClientSecret = null;
                }
                else if (IsClientAuth)
                {
                    CertificateThumbprint = null;
                }
                else
                {
                    ClientSecret = null;
                    CertificateThumbprint = null;
                }
                
            }

        }
    }

    private string _environmentUrl;
    public string EnvironmentUrl
    {
        get => _environmentUrl;
        set
        {
            if (SetProperty(ref _environmentUrl, value))
                UpdateConnectionStringFromProperties();
        }
    }

    private string _tenantId;
    public string TenantId
    {
        get => _tenantId;
        set
        {
            if (SetProperty(ref _tenantId, value))
                UpdateConnectionStringFromProperties();
        }
    }

    private string _clientId;
    public string ClientId
    {
        get => _clientId;
        set
        {
            if (SetProperty(ref _clientId, value))
                UpdateConnectionStringFromProperties();
        }
    }

    private string _clientSecret;
    public string ClientSecret
    {
        get => _clientSecret;
        set
        {
            if (SetProperty(ref _clientSecret, value))
                UpdateConnectionStringFromProperties();
        }
    }

    private string _certificateThumbprint;
    public string CertificateThumbprint
    {
        get => _certificateThumbprint;
        set
        {
            if (SetProperty(ref _certificateThumbprint, value))
                UpdateConnectionStringFromProperties();
        }
    }

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }

    public bool IsClientAuth => AuthType == AuthenticationTypes.ClientSecret;
    public bool IsCertificateAuth => AuthType == AuthenticationTypes.Certificate;
    public bool IsInteractiveAuth => AuthType == AuthenticationTypes.Interactive;

    private bool _suppressConnectionStringUpdate = false;

    public EnvironmentModel()
    {
        AuthType = AuthenticationTypes.Interactive;
    }

    private void UpdatePropertiesFromConnectionString()
    {
        if (_suppressConnectionStringUpdate) return;
        _suppressConnectionStringUpdate = true;
        try
        {
            if (!string.IsNullOrWhiteSpace(_connectionString) && AuthenticationParameters.TryParse(_connectionString, out var authParams))
            {
                AuthType = authParams switch
                {
                    { ClientSecret: not (null or "") } => AuthenticationTypes.ClientSecret,
                    { CertificateThumbprint: not (null or "") } => AuthenticationTypes.Certificate,
                    _ => AuthenticationTypes.Interactive
                };
                EnvironmentUrl = authParams.Resource ?? EnvironmentUrl;
                TenantId = authParams.Tenant ?? TenantId;
                // Only set ClientId if it's not the default
                ClientId = (authParams.ClientId != null && !string.Equals(authParams.ClientId, AuthenticationParameters.DefaultClientId, StringComparison.OrdinalIgnoreCase))
                    ? authParams.ClientId
                    : ClientId;
                ClientSecret = authParams.ClientSecret ?? ClientSecret;
                CertificateThumbprint = authParams.CertificateThumbprint ?? CertificateThumbprint;
            }
        }
        finally
        {
            _suppressConnectionStringUpdate = false;
        }
    }

    private void UpdateConnectionStringFromProperties()
    {
        if (_suppressConnectionStringUpdate) return;
        _suppressConnectionStringUpdate = true;
        try
        {
            // Use AuthenticationParameters.Build to create a connection string from properties
            var authParams = new AuthenticationParameters
            {
                Resource = EnvironmentUrl,
                Tenant = TenantId,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                CertificateThumbprint = CertificateThumbprint
            };
            ConnectionString = authParams.Build();
        }
        finally
        {
            _suppressConnectionStringUpdate = false;
        }
    }
}
