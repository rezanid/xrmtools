namespace XrmTools.Tests.PluginTrace;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Moq;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.OData;
using XrmTools.PluginTrace;
using Xunit;

public class TraceExplorerServiceTests
{
    private readonly Mock<IEnvironmentSelection> selection = new();
    private readonly Mock<IXrmHttpClientFactory> authentication = new();
    private readonly Mock<IODataTransport> transport = new();
    private readonly DataverseEnvironment environment = new() { Name = "Dev", ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test" };
    private readonly TraceExplorerService service;

    public TraceExplorerServiceTests()
    {
        selection.Setup(s => s.GetSelectedEnvironmentAsync()).ReturnsAsync(environment);
        authentication.Setup(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticationResult("token", false, "token", DateTimeOffset.UtcNow.AddMinutes(10), DateTimeOffset.UtcNow.AddMinutes(10), null, null, null, [], Guid.Empty));
        service = new TraceExplorerService(selection.Object, authentication.Object, transport.Object);
    }

    [Fact]
    public async Task PollingIsNonInteractiveAndReturnsNewestTracesFirst()
    {
        var targets = new List<string>();
        transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Callback<HttpRequestMessage, CancellationToken>((m, _) => targets.Add(m.RequestUri.AbsoluteUri));
        transport.SetupSequence(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(2, "https://contoso.crm.dynamics.com/api/data/v9.2/plugintracelogs?$skiptoken=next"))
            .ReturnsAsync(Page(1));
        var result = await service.QueryAsync(environment, new TraceFilter(), false, TestContext.Current.CancellationToken);
        Assert.Equal(2, result.Records.Count);
        Assert.True(result.Records[0].CreatedOn > result.Records[1].CreatedOn);
        Assert.False(result.Truncated);
        authentication.Verify(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), false, It.IsAny<CancellationToken>()), Times.Exactly(2));
        authentication.Verify(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RejectsCrossEnvironmentPagingLinksBeforeAnotherAuthentication()
    {
        transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, "https://other.example/api/data/v9.2/plugintracelogs"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.QueryAsync(environment, new TraceFilter(), false, TestContext.Current.CancellationToken));
        transport.Verify(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        authentication.Verify(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnvironmentChangeDuringAuthenticationPreventsDispatch()
    {
        selection.SetupSequence(s => s.GetSelectedEnvironmentAsync()).ReturnsAsync(environment)
            .ReturnsAsync(environment with { ConnectionString = "Url=https://prod.crm.dynamics.com" });
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.QueryAsync(environment, new TraceFilter(), true, TestContext.Current.CancellationToken));
        transport.Verify(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnvironmentChangeDuringResponseDiscardsTheResponse()
    {
        selection.SetupSequence(s => s.GetSelectedEnvironmentAsync()).ReturnsAsync(environment).ReturnsAsync(environment)
            .ReturnsAsync(environment with { ConnectionString = "Url=https://prod.crm.dynamics.com" });
        transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(Page(1));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.QueryAsync(environment, new TraceFilter(), false, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FailedFilterSurfacesServerMessageWithoutReturningPartialResults()
    {
        transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataResponse { StatusCode = 400, Status = "400 Bad Request", Body = "{\"error\":{\"message\":\"Invalid filter expression\"}}" });
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.QueryAsync(environment, new TraceFilter(), true, TestContext.Current.CancellationToken));
        Assert.Contains("Invalid filter expression", exception.Message);
    }

    [Fact]
    public async Task CancellationDoesNotAuthenticateOrDispatch()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.QueryAsync(environment, new TraceFilter(), false, cancellation.Token));
        authentication.Verify(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReadsAndUpdatesTheOrganizationTraceLoggingSetting()
    {
        var organizationId = Guid.NewGuid();
        var changes = new List<TraceLoggingConfiguration>();
        service.TraceLoggingConfigurationChanged += (target, configuration) =>
        {
            Assert.Equal(environment, target);
            changes.Add(configuration);
        };
        var requests = new List<(HttpMethod Method, string Url, string? IfMatch, string Body)>();
        transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => requests.Add((request.Method, request.RequestUri.AbsoluteUri,
                request.Headers.TryGetValues("If-Match", out var ifMatch) ? ifMatch.Single() : null,
                request.Content?.ReadAsStringAsync().GetAwaiter().GetResult() ?? "")))
            .ReturnsAsync(new ODataResponse { StatusCode = 200, Body = $"{{\"value\":[{{\"organizationid\":\"{organizationId:D}\",\"plugintracelogsetting\":1}}]}}" });

        var configuration = await service.GetTraceLoggingAsync(environment, TestContext.Current.CancellationToken);
        Assert.Equal(organizationId, configuration.OrganizationId);
        Assert.Equal(TraceLoggingMode.Exceptions, configuration.Mode);
        Assert.Contains("organizations?$select=organizationid,plugintracelogsetting", requests[0].Url);

        await service.SetTraceLoggingAsync(environment, organizationId, TraceLoggingMode.All, TestContext.Current.CancellationToken);
        var update = requests[1];
        Assert.Equal("PATCH", update.Method.Method);
        Assert.Contains($"organizations({organizationId:D})", update.Url);
        Assert.Equal("*", update.IfMatch);
        Assert.Equal("{\"plugintracelogsetting\":2}", update.Body);
        Assert.Equal(new[] { TraceLoggingMode.Exceptions, TraceLoggingMode.All }, changes.Select(c => c.Mode));
        Assert.All(changes, c => Assert.Equal(organizationId, c.OrganizationId));
    }

    [Fact]
    public async Task FailedLoggingUpdateDoesNotPublishAConfigurationChange()
    {
        var changed = false;
        service.TraceLoggingConfigurationChanged += (_, _) => changed = true;
        transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataResponse { StatusCode = 403, Status = "Forbidden", Body = "{}" });
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SetTraceLoggingAsync(environment, Guid.NewGuid(), TraceLoggingMode.Off, TestContext.Current.CancellationToken));
        Assert.False(changed);
    }

    private static ODataResponse Page(int second, string next = null)
    {
        var response = new Dictionary<string, object>
        {
            ["value"] = new[] { new { plugintracelogid = Guid.NewGuid(), createdon = new DateTimeOffset(2026, 9, 22, 12, 0, second, TimeSpan.Zero) } }
        };
        if (next != null) response["@odata.nextLink"] = next;
        return new ODataResponse { StatusCode = 200, Body = JsonSerializer.Serialize(response) };
    }
}
