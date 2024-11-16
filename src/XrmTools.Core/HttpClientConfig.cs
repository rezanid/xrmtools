namespace XrmTools;

using Polly.Wrap;
using System;
using System.Net.Http;

public class HttpClientConfig
{
    public Action<HttpClient> ConfigureClient { get; set; }
    public TimeSpan HandlerLifetime { get; set; } = TimeSpan.FromMinutes(2);
    public AsyncPolicyWrap<HttpResponseMessage> ResiliencePolicy { get; set; }
    public DelegatingHandler[] CustomHandlers { get; set; } = [];
}