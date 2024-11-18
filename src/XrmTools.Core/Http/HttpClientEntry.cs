namespace XrmTools.Http;

using Microsoft.VisualStudio.Threading;
using System;
using System.Net.Http;

public class HttpClientEntry(AsyncLazy<HttpClient> client, DateTimeOffset createdAt)
{
    public AsyncLazy<HttpClient> Client { get; } = client;
    public DateTimeOffset CreatedAt { get; } = createdAt;
}