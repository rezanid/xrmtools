namespace XrmTools.WebApi;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public abstract class WebApiRequest<TResponse> : HttpRequestMessage
{
    public abstract Task<TResponse> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct);
}
