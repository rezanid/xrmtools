#nullable enable
namespace XrmTools.WebApi.Messages;

using System.Net.Http;

/// <summary>
/// Contains the response from the GetCollectionCountRequest
/// </summary>
/// <remarks>
/// This class must be instantiated by either:
/// - The Service.SendAsync<T> method
/// - The HttpResponseMessage.As<T> extension in Extensions.cs
/// </remarks>
public sealed class GetCollectionCountResponse : HttpResponseMessage
{
    /// <summary>
    /// Gets the number of records in the collection
    /// </summary>
    public int Count
    {
        get
        {
            return int.Parse(Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }
    }
}
