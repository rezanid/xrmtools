#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System.Net.Http;

// This class must be instantiated by either:
// - The Service.SendAsync<T> method
// - The HttpResponseMessage.As<T> extension in Extensions.cs

/// <summary>
/// Contains the data from the InitializeFromRequest
/// </summary>
public sealed class InitializeFromResponse : HttpResponseMessage
{
    /// <summary>
    /// The record returned.
    /// </summary>
    public JObject Record
    {
        get
        {
            return JObject.Parse(Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }
    }
}
#nullable restore