namespace XrmTools.Tests.OData;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using XrmTools.OData;
using Xunit;

public class ODataResponseFormatterTests
{
    [Fact]
    public async Task RawIncludesBothMessagesAndBodiesButRedactsCredentialHeaders()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://example.test/api/data/v9.2/accounts")
        { Content = new StringContent("{\"name\":\"café\"}") };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "secret-token");
        request.Headers.Add("Cookie", "session=secret-cookie");
        request.Headers.Add("Prefer", "return=representation");
        using var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent("{\"id\":1}") };
        response.Headers.Add("Set-Cookie", "session=secret-response-cookie");
        string raw = await ODataResponseFormatter.RawAsync(request, response, "{\"id\":1}");
        Assert.Contains("=== REQUEST ===", raw);
        Assert.Contains("POST https://example.test/api/data/v9.2/accounts HTTP/1.1", raw);
        Assert.Contains("Prefer: return=representation", raw);
        Assert.Contains("{\"name\":\"café\"}", raw);
        Assert.Contains("=== RESPONSE ===", raw);
        Assert.Contains("HTTP/1.1 201 Created", raw);
        Assert.EndsWith("{\"id\":1}", raw);
        Assert.Contains("Authorization: [redacted]", raw);
        Assert.DoesNotContain("secret", raw);
        Assert.Equal("secret-token", request.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task RawHandlesBodylessRequestsAndResponses()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/WhoAmI");
        using var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        Assert.Contains("204 No Content", await ODataResponseFormatter.RawAsync(request, response, ""));
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1048576, "1 MB")]
    public void FormatsBodySize(long bytes, string expected) => Assert.Equal(expected, ODataResponseFormatter.Size(bytes));

    [Theory]
    [InlineData(0, "0 ms")]
    [InlineData(999, "999 ms")]
    [InlineData(1000, "1 s")]
    public void FormatsElapsedTime(double milliseconds, string expected) => Assert.Equal(expected, ODataResponseFormatter.Time(milliseconds));
}
