#nullable enable
namespace XrmTools.WebApi.Messages;

using System;
using System.Net.Http;

/// <summary>
/// Contains the data to retrieve a the file of an attachment (activitymimeattachment).
/// 
public sealed class DownloadAttachmentFileRequest : HttpRequestMessage
{
    public DownloadAttachmentFileRequest(Guid activitymimeattachmentId)
    {
        string uriString = $"activitymimeattachments({activitymimeattachmentId})/body/$value";

        Method = HttpMethod.Get;
        RequestUri = new Uri(
            uriString: uriString,
            uriKind: UriKind.Relative);
    }
}
#nullable restore