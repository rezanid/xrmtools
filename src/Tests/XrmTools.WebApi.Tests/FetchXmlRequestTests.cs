namespace XrmTools.WebApi.Tests;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using XrmTools.WebApi.Batch;
using XrmTools.WebApi.Messages;
using Xunit;

public class FetchXmlRequestTests
{
    [Fact]
    public void Constructor_DoesNotForceCountForTopQueries()
    {
        using var request = new FetchXmlRequest("accounts", "<fetch top=\"1\"><entity name=\"account\" /></fetch>");

        Assert.DoesNotContain("$count", request.RequestUri!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BatchError_PreservesDataverseODataMessage()
    {
        const string boundary = "batchresponse_751fdee8-e5de-4634-b989-973dce26787a";
        const string expectedMessage = "The top attribute can't be specified with paging attribute returntotalrecordcount";
        var body = $"--{boundary}\r\n"
            + "Content-Type: application/http\r\n"
            + "Content-Transfer-Encoding: binary\r\n\r\n"
            + "HTTP/1.1 400 Bad Request\r\n"
            + "REQ_ID: 066c59e0-7df9-4d72-9232-416550afdba9\r\n"
            + "X-Content-Type-Options: nosniff\r\n"
            + "Content-Type: application/json; odata.metadata=minimal; odata.streaming=true\r\n"
            + "OData-Version: 4.0\r\n\r\n"
            + $"{{\"error\":{{\"code\":\"0x80040203\",\"message\":\"{expectedMessage}\"}}}}\r\n"
            + $"--{boundary}--\r\n";

        using var rawBatchResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8),
        };
        rawBatchResponse.Content.Headers.ContentType =
            MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={boundary}");

        var cancellationToken = TestContext.Current.CancellationToken;
        using var batchRequest = new BatchRequest(new Uri("https://example.crm.dynamics.com/api/data/v9.2/"));
        var batchResponse = await batchRequest.CreateResponseAsync(rawBatchResponse, cancellationToken);
        using var innerResponse = Assert.Single(await batchResponse.ParseResponseAsync(cancellationToken));
        using var fetchRequest = new FetchXmlRequest("accounts", "<fetch><entity name=\"account\" /></fetch>");

        var exception = await Assert.ThrowsAsync<ServiceException>(
            () => fetchRequest.CreateResponseAsync(innerResponse, cancellationToken));

        Assert.Equal(expectedMessage, exception.Message);
        Assert.Equal("0x80040203", exception.ODataError?.Error?.Code);
        Assert.Equal("066c59e0-7df9-4d72-9232-416550afdba9", exception.RequestId);
    }
}
