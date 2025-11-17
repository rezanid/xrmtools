#nullable enable
namespace XrmTools.WebApi;

using System.Text.Json.Serialization;

public class ODataError
{
    public Error? Error { get; set; }
}

public class Error
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
#nullable restore