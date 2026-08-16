#nullable enable
namespace XrmTools;

using System;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

[DisplayName("Power Platform Environment")]
[Description("Properties of a Power Platform environment.")]
[DefaultProperty(nameof(Name))]
//[TypeConverter(typeof(DataverseEnvironmentConverter))]
public record DataverseEnvironment
{
    private string? _url = string.Empty;
    private string? _connectionstring = string.Empty;
    private bool isValidConnectionString = false;

    [DisplayName("Environment Name")]
    [Description("The name of the environment, so you can easily identify it.")]
    [DefaultValue("Contoso Dev")]
    public string? Name { get; set; }

    [DisplayName("Environment URL")]
    [Description("The URL of the environment.")]
    [DefaultValue("https://contoso.crm.dynamics.com")]
    [ReadOnly(true)]
    //[Browsable(false)]
    public string? Url { get => _url; }

    [Browsable(false)]
    public Uri? BaseServiceUrl => new(new Uri(Url), "/api/data/v9.2/");

    [DisplayName("Connection String")]
    [Description("The connection string to the environment according to https://learn.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect.")]
    [DefaultValue("AuthType=OAuth;Url=https://contoso.crm.dynamics.com;Integrated Security=True")]
    public string? ConnectionString
    {
        get => _connectionstring;
        set
        {
            _connectionstring = value;
            var segments = value?.Split([';'], StringSplitOptions.RemoveEmptyEntries);
            if (segments is null)
            {
                isValidConnectionString = false;
                return;
            }
            if (segments.Length == 1)
            {
                _url = segments[0];
                if (!_url.StartsWith("https://")) { _url = "https://" + _url; }
            }
            else
            {
                _url = segments.FirstOrDefault(s => s.StartsWith("Url=", StringComparison.OrdinalIgnoreCase))?[4..];
            }
            isValidConnectionString = !string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(_url) && Uri.TryCreate(_url, UriKind.Absolute, out _);
        }
    }

    public bool AllowCookies { get; set; } = false;

    [MemberNotNullWhen(true, nameof(Url), nameof(ConnectionString))]
    [Browsable(false)]
    public bool IsValid { get => isValidConnectionString; }

    [Browsable(false)]
    public bool IsAutehnticated { get; set; }

    public virtual bool Equals(DataverseEnvironment? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // Only consider Url and ConnectionString in equality
        return (Name is null && other.Name is null && string.IsNullOrEmpty(ConnectionString) && string.IsNullOrEmpty(other.ConnectionString))
            || (Name is not null && other.Name is not null && ConnectionString == other.ConnectionString);
    }

    public override int GetHashCode()
    {
        int hash = 17;
        //hash = hash * 23 + (Url?.GetHashCode() ?? 0);
        hash = hash * 23 + (ConnectionString?.GetHashCode() ?? 0);
        return hash;
    }

    //public override string? ToString()
    //{
    //    return !string.IsNullOrEmpty(Name) ? Name : "New";
    //}

    public static DataverseEnvironment Empty => new();

}
#nullable restore