namespace XrmTools.WebApi.Messages;

using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Contains the data from the GetColumnValueRequest.
/// </summary>
public sealed class DownloadFileResponse
{
    /// <summary>
    /// The requested file column  value.
    /// </summary>
    public byte[]? File {  get; private set; }

    private DownloadFileResponse(byte[]? file) => File = file;

    internal async static Task<DownloadFileResponse> FromAsync(HttpResponseMessage raw)
        => new(await raw.Content.ReadAsByteArrayAsync().ConfigureAwait(false));

}