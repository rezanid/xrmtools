namespace XrmTools.WebApi.Messages;

using System.Linq;
using System.Net.Http;

/// <summary>
/// Contains the response from the CreateRequest
/// </summary>
/// <remarks>
/// This class must be instantiated by either:
/// - The Service.SendAsync<T> method
/// - The HttpResponseMessage.As<T> extension in Extensions.cs
/// </remarks>
public sealed class CreateResponse : HttpResponseMessage
{
    /// <summary>
    /// A reference to the record created.
    /// </summary>
    public EntityReference? EntityReference
    {
        get =>
            Headers.TryGetValues("OData-EntityId", out var values) ?
            new(uri: values.FirstOrDefault()) :
            null;
    }
}