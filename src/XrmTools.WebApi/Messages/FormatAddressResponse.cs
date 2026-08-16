#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System.Net.Http;

/// <summary>
/// Contains the response from the FormatAddressRequest
/// </summary>
public sealed class FormatAddressResponse : HttpResponseMessage
{
    // Cache the async content
    private string? _content;

    //Provides JObject for property getters
    private JObject JObject
    {
        get
        {
            _content ??= Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return JObject.Parse(_content);
        }
    }

    /// <summary>
    /// Gets the formatted address
    /// </summary>
    public string? Address => (string?)JObject.GetValue(nameof(Address));
}
#nullable restore