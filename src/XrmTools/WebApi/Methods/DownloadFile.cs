#nullable enable
namespace XrmTools.WebApi.Methods;

using System.Threading;
using System.Threading.Tasks;
using XrmTools.WebApi.Messages;

internal static partial class Extensions
{
    public static async Task<byte[]?> DownloadFileAsync(
        this IWebApiService service,
        EntityReference entityReference,
        string property,
        bool returnFullSizedImage = false,
        CancellationToken cancellationToken = default)
    {
        var request = new DownloadFileRequest(entityReference, property, returnFullSizedImage);
        var response = await service.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return response.File;
    }
}