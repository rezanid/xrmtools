namespace XrmTools.Tests.OData;

using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.OData;
using Xunit;

public class ODataTransportTests
{
    [Fact]
    public async Task SpillsLargeResponseButChainsValuesBeyondThePreview()
    {
        string body = "{\"padding\":\"" + new string('a', 5 * 1024 * 1024) + "\",\"id\":\"later\"}";
        using var handler = new DisposingHandler(HttpStatusCode.OK, body);
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://example.test/accounts") { Content = new StringContent("{}") };
        using var response = await ODataTransport.SendCoreAsync(client, request, CancellationToken.None);
        Assert.Equal((long)body.Length, response.ContentByteLength);
        Assert.Equal(ODataTransport.PreviewLimit, response.Body.Length);
        Assert.Contains("truncated", response.Raw);
        var stored = response.StoredBody!;
        using var capture = new ODataCapturedResponse { StoredBody = stored.Retain() };
        response.Dispose();
        var document = ODataDocument.Parse("GET /accounts({{list.response.body.$.id}})");
        Assert.Equal("/accounts(later)", document.Resolve(document.Requests[0], _ => capture, _ => { }).Target);
        using var reader = new StreamReader(stored.Open());
        Assert.Equal(body, await reader.ReadToEndAsync());
    }

    [Theory]
    [InlineData("POST", HttpStatusCode.NoContent, "")]
    [InlineData("PATCH", HttpStatusCode.OK, "{\"updated\":true}")]
    [InlineData("POST", HttpStatusCode.BadRequest, "{\"error\":\"invalid\"}")]
    public async Task FormatsResponseAfterRequestContentHasBeenDisposed(string method, HttpStatusCode status, string body)
    {
        using var handler = new DisposingHandler(status, body);
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(new HttpMethod(method), "https://example.test/api/data/v9.2/accounts")
        { Content = new StringContent("{\"name\":\"Test Account\"}") };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "secret-token");

        var result = await ODataTransport.SendCoreAsync(client, request, CancellationToken.None);

        Assert.Equal(1, handler.SendCount);
        Assert.Equal("{\"name\":\"Test Account\"}", handler.SentBody);
        Assert.Equal((int)status, result.StatusCode);
        Assert.Equal(body, result.Body);
        Assert.Contains(handler.SentBody, result.Raw);
        Assert.Contains("=== RESPONSE ===", result.Raw);
        Assert.Contains("Authorization: [redacted]", result.Raw);
        Assert.DoesNotContain("secret-token", result.Raw);
    }

    [Fact]
    public async Task CapturesResponseAndContentHeadersWithoutCredentialHeaders()
    {
        using var handler = new ResponseHeadersHandler();
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api/data/v9.2/accounts");
        var result = await ODataTransport.SendCoreAsync(client, request, CancellationToken.None);
        Assert.Equal("https://example.test/account", result.CapturableHeaders["odata-entityid"]);
        Assert.Equal("application/json", result.CapturableHeaders["content-type"]);
        Assert.False(result.CapturableHeaders.ContainsKey("Set-Cookie"));
        Assert.False(result.CapturableHeaders.ContainsKey("WWW-Authenticate"));
        Assert.False(result.CapturableHeaders.ContainsKey("X-Api-Key"));
        Assert.Contains("Set-Cookie: [redacted]", result.Headers);
    }

    private sealed class ResponseHeadersHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
            response.Headers.TryAddWithoutValidation("OData-EntityId", "https://example.test/account");
            response.Headers.TryAddWithoutValidation("Set-Cookie", "secret=value");
            response.Headers.TryAddWithoutValidation("WWW-Authenticate", "Bearer");
            response.Headers.TryAddWithoutValidation("X-Api-Key", "secret");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return Task.FromResult(response);
        }
    }

    private sealed class DisposingHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public int SendCount { get; private set; }
        public string SentBody { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SendCount++;
            SentBody = await request.Content.ReadAsStringAsync();
            // Reproduce Framework content ownership even if the test runtime changes.
            request.Content.Dispose();
            return new HttpResponseMessage(status) { Content = new StringContent(body) };
        }
    }
}
