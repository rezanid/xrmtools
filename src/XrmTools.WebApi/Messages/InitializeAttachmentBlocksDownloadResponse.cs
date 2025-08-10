namespace XrmTools.WebApi.Messages;

/// <summary>
/// Contains the data from the InitializeAttachmentBlocksDownloadRequest
/// </summary>
public sealed class InitializeAttachmentBlocksDownloadResponse
{
    /// <summary>
    /// A token that uniquely identifies a sequence of related data blocks.
    /// </summary>
    public string? FileContinuationToken { get; set; }

    /// <summary>
    /// The size of the data file in bytes.
    /// </summary>
    public int FileSizeInBytes { get; set; }


    /// <summary>
    /// The name of the stored file.
    /// </summary>
    public string? FileName { get; set; }

}