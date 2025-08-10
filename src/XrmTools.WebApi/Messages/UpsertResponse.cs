namespace XrmTools.WebApi.Messages;

using System.Linq;
using System.Net.Http;

/// <summary>
/// Contains the response from the UpsertRequest
/// </summary>
/// <remarks>
/// This class must be instantiated by either:
/// - The Service.SendAsync<T> method
/// - The HttpResponseMessage.As<T> extension in Extensions.cs
/// </remarks>
public sealed class UpsertResponse : HttpResponseMessage
{
    /// <summary>
    /// A reference to the record.
    /// </summary>
    public EntityReference? EntityReference
    {
        get =>
            Headers.TryGetValues("OData-EntityId", out var values) ?
            new(uri: values.FirstOrDefault()) :
            null;
    }
}