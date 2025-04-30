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
        get
        {
            if (Headers != null &&
                Headers.Contains("OData-EntityId") &&
                Headers.GetValues("OData-EntityId") != null)
            {
                return new EntityReference(Headers.GetValues("OData-EntityId").FirstOrDefault());
            }
            else
            {
                return null;
            }
        }
    }
}
