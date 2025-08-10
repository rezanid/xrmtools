namespace XrmTools.WebApi.Messages;

using System;
using System.Net.Http;

/// <summary>
/// Contains the data from the CommitAnnotationBlocksUploadRequest
/// </summary>
public sealed class CommitAnnotationBlocksUploadResponse : HttpResponseMessage
{
    /// <summary>
    /// The unique identifier of the stored annotation.
    /// </summary>
    public Guid AnnotationId { get; set; }

    /// <summary>
    /// The size of the stored annotation in bytes.
    /// </summary>
    public int FileSizeInBytes { get; set; }

}