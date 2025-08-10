namespace XrmTools.WebApi.Messages;

using System.Net.Http;

/// <summary>
/// Contains the data from the GetColumnValueRequest.
/// </summary>
/// <remarks>
/// This class must be instantiated by either:
/// - The Service.SendAsync<T> method
/// - The HttpResponseMessage.As<T> extension in Extensions.cs
/// </remarks>
public sealed class DownloadFileResponse : HttpResponseMessage
{
    /// <summary>
    /// The requested file column  value.
    /// </summary>
    public byte[] File => Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
}
