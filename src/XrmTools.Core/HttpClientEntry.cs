namespace XrmTools;
using System;
using System.Net.Http;

public class HttpClientEntry(Lazy<HttpClient> client, DateTimeOffset createdAt)
{
    public Lazy<HttpClient> Client { get; } = client;
    public DateTimeOffset CreatedAt { get; } = createdAt;
}