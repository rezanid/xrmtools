namespace XrmTools.WebApi.Messages;

using System;

/// <summary>
/// Contains the data from the CommitFileBlocksUploadRequest
/// </summary>
public sealed class CommitFileBlocksUploadResponse
{
    /// <summary>
    /// The unique identifier of the stored File.
    /// </summary>
    public Guid FileId { get; set; }

    /// <summary>
    /// The size of the stored File in bytes.
    /// </summary>
    public int FileSizeInBytes { get; set; }
}
