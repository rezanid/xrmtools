#nullable enable
namespace XrmTools.WebApi.Messages;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the data to perform the WhoAmI function
/// </summary>
public sealed class WhoAmIRequest : WebApiRequest<WhoAmIResponse>
{
    /// <summary>
    /// Initializes the WhoAmIRequest
    /// </summary>
    public WhoAmIRequest()
    {
        Method = HttpMethod.Get;
        RequestUri = new Uri(
            uriString: "WhoAmI", 
            uriKind: UriKind.Relative);
    }

    public override Task<WhoAmIResponse> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct)
        => raw.CastAsync<WhoAmIResponse>();
}
#nullable restore