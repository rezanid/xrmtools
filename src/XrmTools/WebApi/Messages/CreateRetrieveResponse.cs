#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System.Net.Http;

/// <summary>
/// Contains the data from the a CreateRetrieveRequest
/// </summary>
/// <remarks>
/// This class must be instantiated by either:
/// - The Service.SendAsync<T> method
/// - The HttpResponseMessage.As<T> extension in Extensions.cs
///</remarks>
public sealed class CreateRetrieveResponse : HttpResponseMessage
{
    /// <summary>
    /// The record created.
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