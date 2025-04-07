#nullable enable
namespace XrmTools.WebApi.Messages;

using System;
using System.Net.Http;

/// <summary>
/// Contains the data to retrieve a the file of an annotation.
/// 
public sealed class DownloadAnnotationFileRequest : HttpRequestMessage
{
    public DownloadAnnotationFileRequest(Guid annotationId)
    {
        string uriString = $"annotations({annotationId})/documentbody/$value";

        Method = HttpMethod.Get;
        RequestUri = new Uri(
            uriString: uriString,
            uriKind: UriKind.Relative);
    }
}
#nullable restore