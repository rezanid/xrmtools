namespace XrmTools.Http;

using Polly;
using System;
using System.Net.Http;
using System.Threading.Tasks;

public class HttpClientConfig
{
    public Func<HttpClient, Task> ConfigureClient { get; set; }
    public TimeSpan HandlerLifetime { get; set; } = TimeSpan.FromMinutes(2);
    //public AsyncPolicyWrap<HttpResponseMessage> ResiliencePolicy { get; set; }
    public IAsyncPolicy<HttpResponseMessage> ResiliencePolicy { get; set; }
    public DelegatingHandler[] CustomHandlers { get; set; } = [];
}