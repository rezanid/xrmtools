namespace XrmTools.Tests.OData;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Moq;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.OData;
using Xunit;

public class ODataExecutionServiceTests
{
    private readonly Mock<IEnvironmentSelection> selection = new();
    private readonly Mock<IXrmHttpClientFactory> authentication = new();
    private readonly Mock<IODataTransport> transport = new();
    private readonly DataverseEnvironment environment = new() { Name = "Dev", ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test" };

    public ODataExecutionServiceTests()
    {
        selection.Setup(s => s.GetSelectedEnvironmentAsync()).ReturnsAsync(environment);
        authentication.Setup(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Token());
        transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataResponse { Status = "HTTP/1.1 200 OK" });
    }

    [Fact]
    public async Task SendsWithFreshAuthenticationForEachExecution()
    {
        var service = new ODataExecutionService(selection.Object, authentication.Object, transport.Object);
        var document = ODataDocument.Parse("GET /WhoAmI");
        await service.SendAsync(document, document.Requests[0], environment, TestContext.Current.CancellationToken);
        await service.SendAsync(document, document.Requests[0], environment, TestContext.Current.CancellationToken);
        authentication.Verify(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>()), Times.Exactly(2));
        transport.Verify(t => t.SendAsync(It.Is<HttpRequestMessage>(m => m.Headers.Authorization.Parameter == "token"), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AbortsWhenEnvironmentChangesDuringAuthentication()
    {
        selection.SetupSequence(s => s.GetSelectedEnvironmentAsync()).ReturnsAsync(environment)
            .ReturnsAsync(new DataverseEnvironment { Name = "Prod", ConnectionString = "Url=https://prod.crm.dynamics.com;TenantId=test" });
        await Assert.ThrowsAsync<InvalidOperationException>(() => SendAsync("POST /new_Api", TestContext.Current.CancellationToken));
        transport.Verify(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidatesDestinationBeforeAuthentication()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => SendAsync("GET https://other.example/api/data/v9.2/WhoAmI", TestContext.Current.CancellationToken));
        authentication.Verify(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DoesNotRetryFailedMutations()
    {
        transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Failure"));
        await Assert.ThrowsAsync<HttpRequestException>(() => SendAsync("POST /new_Api", TestContext.Current.CancellationToken));
        transport.Verify(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectsExpiredToken()
    {
        authentication.Setup(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>())).ReturnsAsync(Token(-1));
        await Assert.ThrowsAsync<InvalidOperationException>(() => SendAsync("GET /WhoAmI", TestContext.Current.CancellationToken));
        transport.Verify(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancellationBeforeSendDoesNotAuthenticate()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => SendAsync("GET /WhoAmI", cts.Token));
        authentication.Verify(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancellationDuringAuthenticationPreventsDispatch()
    {
        using var cts = new CancellationTokenSource();
        authentication.Setup(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>()))
            .Returns(() => { cts.Cancel(); return Task.FromResult(Token()); });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => SendAsync("POST /new_Api", cts.Token));
        transport.Verify(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangedCredentialsOnSameHostPreventDispatch()
    {
        var changed = environment with { ConnectionString = environment.ConnectionString + ";ClientId=another-user" };
        selection.SetupSequence(s => s.GetSelectedEnvironmentAsync()).ReturnsAsync(environment).ReturnsAsync(changed);
        await Assert.ThrowsAsync<InvalidOperationException>(() => SendAsync("GET /WhoAmI", TestContext.Current.CancellationToken));
        transport.Verify(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StaleDisplayedEnvironmentDoesNotStartAuthentication()
    {
        selection.Setup(s => s.GetSelectedEnvironmentAsync()).ReturnsAsync((DataverseEnvironment)null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => SendAsync("GET /WhoAmI", TestContext.Current.CancellationToken));
        authentication.Verify(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }



    private Task<ODataResponse> SendAsync(string text, CancellationToken cancellationToken)
    {
        var document = ODataDocument.Parse(text);
        return new ODataExecutionService(selection.Object, authentication.Object, transport.Object)
            .SendAsync(document, document.Requests[0], environment, cancellationToken);
    }

    private static AuthenticationResult Token(int minutes = 10) => new("token", false, "token", DateTimeOffset.UtcNow.AddMinutes(minutes),
        DateTimeOffset.UtcNow.AddMinutes(minutes), null, null, null, [], Guid.Empty);
}
