#nullable enable
namespace XrmTools.WebApi.Messages;

using System;
using System.Net.Http;

/// <summary>
/// Contains the data from the DownloadAttachmentFileRequest.
/// </summary>
/// <remarks
/// This class must be instantiated by either:
/// - The Service.SendAsync<T> method
/// - The HttpResponseMessage.As<T> extension in Extensions.cs
/// </remarks>
public sealed class DownloadAttachmentFileResponse : HttpResponseMessage
{
    /// <summary>
    /// The requested annotation file value.
    /// </summary>
    public byte[] File => Convert.FromBase64String(Content.ReadAsStringAsync().GetAwaiter().GetResult());
}
#nullable restore