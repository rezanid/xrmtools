#nullable enable
namespace XrmTools.WebApi.Messages;

using System;
using System.Linq;
using System.Net.Http;

/// <summary>
/// Contains the data from the InitializeChunkedFileUploadRequest
/// </summary>
/// <remarks
/// This class must be instantiated by either:
/// - The Service.SendAsync<T> method
/// - The HttpResponseMessage.As<T> extension in Extensions.cs
///</remarks>
public sealed class InitializeChunkedFileUploadResponse : HttpResponseMessage
{
    public Uri Url => Headers.Location;
    public int ChunkSize => int.Parse(Headers.GetValues("x-ms-chunk-size").First());

}
#nullable restore