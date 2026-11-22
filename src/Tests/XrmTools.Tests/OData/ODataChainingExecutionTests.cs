namespace XrmTools.Tests.OData;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Moq;
using XrmTools;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.OData;
using Xunit;

public class ODataChainingExecutionTests
{
    private readonly DataverseEnvironment environment = new() { ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=tenant" };
    private readonly Mock<IEnvironmentSelection> selection = new();
    private readonly Mock<IXrmHttpClientFactory> authentication = new();
    private readonly Mock<IODataTransport> transport = new();

    public ODataChainingExecutionTests()
    {
        selection.Setup(s => s.GetSelectedEnvironmentAsync()).ReturnsAsync(environment);
        transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataResponse { StatusCode = 200 });
    }

    [Fact]
    public async Task ValidatesSubstitutedDestinationBeforeAuthentication()
    {
        var document = ODataDocument.Parse("# @name a\nGET /WhoAmI\n### Next\nGET {{a.response.body.$.url}}");
        var session = Captured(document, "{\"url\":\"https://other.example/api/data/v9.2/accounts\"}", "principal");
        var run = session.Begin(document, document.Requests[1], environment);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().SendAsync(document, document.Requests[1], environment, CancellationToken.None, session, run));
        authentication.Verify(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        transport.Verify(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RejectsSubstitutedHeaderNewlinesBeforeAuthentication()
    {
        var document = ODataDocument.Parse("# @name a\nGET /WhoAmI\n### Next\nGET /accounts\nX-Test: {{a.response.body.$.value}}");
        var session = Captured(document, "{\"value\":\"one\\ntwo\"}", "principal");
        var run = session.Begin(document, document.Requests[1], environment);
        await Assert.ThrowsAsync<FormatException>(() => Service().SendAsync(document, document.Requests[1], environment, CancellationToken.None, session, run));
        authentication.Verify(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        transport.Verify(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("different")]
    [InlineData(null)]
    public async Task RejectsChangedOrUnverifiablePrincipalBeforeDispatch(string principal)
    {
        var document = ODataDocument.Parse("# @name a\nGET /WhoAmI\n### Next\nGET /accounts({{a.response.body.$.id}})");
        var session = Captured(document, "{\"id\":\"123\"}", principal);
        var run = session.Begin(document, document.Requests[1], environment);
        authentication.Setup(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>())).ReturnsAsync(UserToken("alice"));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Service().SendAsync(document, document.Requests[1], environment, CancellationToken.None, session, run));
        Assert.Contains("identity", error.Message);
        transport.Verify(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendsWithMatchingUserAndDoesNotSendReferencedRequest()
    {
        var token = UserToken("alice");
        var document = ODataDocument.Parse("# @name a\nGET /WhoAmI\n### Next\nGET /accounts({{a.response.body.$.id}})");
        var session = Captured(document, "{\"id\":\"123\"}", ODataExecutionService.Principal(token));
        var run = session.Begin(document, document.Requests[1], environment);
        authentication.Setup(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>())).ReturnsAsync(token);
        await Service().SendAsync(document, document.Requests[1], environment, CancellationToken.None, session, run);
        transport.Verify(t => t.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri.AbsolutePath.EndsWith("/accounts(123)")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CaptureChangedDuringAuthenticationPreventsDispatch()
    {
        var token = UserToken("alice");
        var document = ODataDocument.Parse("# @name a\nGET /WhoAmI\n### Next\nGET /accounts({{a.response.body.$.id}})");
        var session = Captured(document, "{\"id\":\"123\"}", ODataExecutionService.Principal(token));
        var run = session.Begin(document, document.Requests[1], environment);
        authentication.Setup(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>()))
            .Returns(() => { session.Begin(document, document.Requests[0], environment); return Task.FromResult(token); });
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().SendAsync(document, document.Requests[1], environment, CancellationToken.None, session, run));
        transport.Verify(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(204, true)]
    [InlineData(400, false)]
    public async Task OnlySuccessfulNamedResponsesAreEligibleForCapture(int statusCode, bool expected)
    {
        var document = ODataDocument.Parse("# @name a\nGET /WhoAmI");
        var session = Captured(document, "old", "principal");
        var run = session.Begin(document, document.Requests[0], environment);
        Assert.Null(session.Get("a"));
        authentication.Setup(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>())).ReturnsAsync(UserToken("alice"));
        transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ODataResponse { StatusCode = statusCode });
        var response = await Service().SendAsync(document, document.Requests[0], environment, CancellationToken.None, session, run);
        Assert.Equal(expected, response.CanCapture);
    }

    [Fact]
    public async Task EnvironmentChangedAfterDispatchPreventsCaptureButKeepsResponse()
    {
        var document = ODataDocument.Parse("# @name a\nGET /WhoAmI");
        var session = new ODataCaptureSession();
        var run = session.Begin(document, document.Requests[0], environment);
        var other = new DataverseEnvironment { ConnectionString = "Url=https://other.crm.dynamics.com;TenantId=tenant" };
        selection.SetupSequence(s => s.GetSelectedEnvironmentAsync()).ReturnsAsync(environment).ReturnsAsync(environment).ReturnsAsync(other);
        authentication.Setup(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>())).ReturnsAsync(UserToken("alice"));
        var response = await Service().SendAsync(document, document.Requests[0], environment, CancellationToken.None, session, run);
        Assert.Equal(200, response.StatusCode);
        Assert.False(response.CanCapture);
        session.Complete(run, new ODataCapturedResponse(), CancellationToken.None);
        Assert.Null(session.Get("a"));
    }

    [Fact]
    public void UsesStableUserAndApplicationIdentitiesAndFailsClosed()
    {
        Assert.Equal(ODataExecutionService.Principal(UserToken("alice")), ODataExecutionService.Principal(UserToken("alice")));
        Assert.NotEqual(ODataExecutionService.Principal(UserToken("alice")), ODataExecutionService.Principal(UserToken("bob")));
        var app = AppToken("{\"tid\":\"tenant\",\"oid\":\"object\",\"appid\":\"client\"}");
        Assert.Equal("app:tenant:object:client", ODataExecutionService.Principal(app));
        Assert.Null(ODataExecutionService.Principal(AppToken("{\"tid\":\"tenant\",\"oid\":\"object\",\"appid\":\"client\",\"scp\":\"read\"}")));
        Assert.Null(ODataExecutionService.Principal(AppToken("{}")));
    }

    private ODataCaptureSession Captured(ODataDocument document, string body, string principal)
    {
        var session = new ODataCaptureSession();
        var run = session.Begin(document, document.Requests[0], environment);
        session.Complete(run, new ODataCapturedResponse { Body = body, Principal = principal, Headers = new Dictionary<string, string>() }, CancellationToken.None);
        return session;
    }

    private ODataExecutionService Service() => new(selection.Object, authentication.Object, transport.Object);
    private static AuthenticationResult UserToken(string name)
    {
        var account = new Mock<IAccount>();
        account.SetupGet(a => a.HomeAccountId).Returns(new AccountId(name, name, "tenant"));
        return new AuthenticationResult("token", false, "id", DateTimeOffset.UtcNow.AddMinutes(10), DateTimeOffset.UtcNow.AddMinutes(10), "tenant", account.Object, null, [], Guid.Empty);
    }
    private static AuthenticationResult AppToken(string json)
    {
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return new AuthenticationResult("header." + encoded + ".signature", false, null, DateTimeOffset.UtcNow.AddMinutes(10), DateTimeOffset.UtcNow.AddMinutes(10), "tenant", null, null, [], Guid.Empty);
    }
}
