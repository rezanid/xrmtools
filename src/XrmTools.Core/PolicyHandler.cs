namespace XrmTools;

using Polly;
using System.Net.Http;
using System.Threading.Tasks;

public class PolicyHandler(HttpMessageHandler innerHandler, IAsyncPolicy<HttpResponseMessage> policy) : DelegatingHandler(innerHandler)
{
    private readonly IAsyncPolicy<HttpResponseMessage> _policy = policy;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        return _policy != null ? _policy.ExecuteAsync(ct => base.SendAsync(request, ct), cancellationToken) : base.SendAsync(request, cancellationToken);
    }
}