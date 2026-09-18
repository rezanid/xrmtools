namespace XrmTools.Tests.OData;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.OData;
using Xunit;

public class ODataTransportTests
{
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
