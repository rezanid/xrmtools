namespace XrmTools.Core.Tests.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;
using FluentAssertions;
using System.Threading;
using System.Net.Http;
using Microsoft.Identity.Client;
using XrmTools.Authentication;
using XrmTools.Environments;
using XrmTools.Logging.Compatibility;
using XrmTools.Http;
using System.Collections.Concurrent;

public class XrmHttpClientFactoryTests
{
    private readonly Mock<IEnvironmentProvider> _environmentProviderMock;
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;
    private readonly Mock<ILogger<XrmHttpClientFactory>> _loggerMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly XrmHttpClientFactory _factory;

    public XrmHttpClientFactoryTests()
    {
        _timeProvider = new FakeTimeProvider();
        _environmentProviderMock = new Mock<IEnvironmentProvider>();
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _loggerMock = new Mock<ILogger<XrmHttpClientFactory>>();

        _factory = new XrmHttpClientFactory(
            _timeProvider,
            _environmentProviderMock.Object,
            _authenticationServiceMock.Object,
            _loggerMock.Object
        );
    }
    
    [Fact]
    public async Task CreateHttpClientAsync_Should_Throw_When_No_Environment_Selected()
    {
        // Arrange
        _environmentProviderMock.Setup(x => x.GetActiveEnvironmentAsync()).ReturnsAsync((DataverseEnvironment)null);

        // Act
        Func<Task> act = async () => await _factory.CreateClientAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("No environment selected.");
    }
    
    [Fact]
    public async Task CreateHttpClientAsync_Should_Throw_When_ConnectionString_Is_Empty()
    {
        // Arrange
        var environment = new DataverseEnvironment { Name = "Test", ConnectionString = string.Empty };
        _environmentProviderMock.Setup(x => x.GetActiveEnvironmentAsync()).ReturnsAsync(environment);

        // Act
        Func<Task> act = async () => await _factory.CreateHttpClientAsync(environment);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Environment 'Test' connection string is empty.");
    }

    [Fact]
    public async Task CreateHttpClientAsync_Should_Return_HttpClient_When_Environment_Is_Valid()
    {
        // Arrange
        var environment = CreateFakeEnvironment();
        _environmentProviderMock.Setup(x => x.GetActiveEnvironmentAsync()).ReturnsAsync(environment);

        var authResult = CreateFakeAuthenticationResult();
        _authenticationServiceMock.Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticationParameters>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(authResult);

        // Act
        var client = await _factory.CreateHttpClientAsync(environment);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be(authResult.AccessToken);
    }

    [Fact]
    public async Task CreateHttpClientAsync_Should_Reuse_Authentication_Token_If_Valid()
    {
        // Arrange
        var environment = CreateFakeEnvironment();
        _environmentProviderMock.Setup(x => x.GetActiveEnvironmentAsync()).ReturnsAsync(environment);

        var validToken = CreateFakeAuthenticationResult();
 
        typeof(XrmHttpClientFactory).GetField("_tokenCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_factory, new ConcurrentDictionary<string, AuthenticationResult> { [environment.ConnectionString] = validToken });

        // Act
        var client = await _factory.CreateHttpClientAsync(environment);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be(validToken.AccessToken);
        _authenticationServiceMock.Verify(x => x.AuthenticateAsync(It.IsAny<AuthenticationParameters>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateHttpClientAsync_Should_Request_New_Token_If_Expired()
    {
        // Arrange
        var environment = CreateFakeEnvironment();
        _environmentProviderMock.Setup(x => x.GetActiveEnvironmentAsync()).ReturnsAsync(environment);

        var expiredToken = CreateFakeAuthenticationResult(isValid: false);
        typeof(XrmHttpClientFactory).GetField("_tokenCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_factory, new ConcurrentDictionary<string, AuthenticationResult> { [environment.ConnectionString] = expiredToken });

        var newToken = CreateFakeAuthenticationResult();
        _authenticationServiceMock.Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticationParameters>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(newToken);

        // Act
        var client = await _factory.CreateHttpClientAsync(environment);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be(newToken.AccessToken);
    }
    
    [Fact]
    public void DisposeAsync_Should_Dispose_Handlers_When_Unused()
    {
        // Arrange
        var environment = CreateFakeEnvironment();
        var handler = new FakeHttpMessageHandler();
        var handlerEntry = new HttpMessageHandlerEntry(handler, _timeProvider.GetUtcNow());
        var handlerPool = new ConcurrentDictionary<DataverseEnvironment, Lazy<HttpMessageHandlerEntry>>(
            [new(
                environment, new(() => handlerEntry))]);
        typeof(XrmHttpClientFactory).GetField("_handlerPool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_factory, handlerPool);
        _ = handlerPool.TryGetValue(environment, out var lazyHandler) ? lazyHandler.Value : null;

        // Act
        _timeProvider.Advance(handlerEntry.Lifetime);

        // Assert
        handlerEntry.CanDispose().Should().BeTrue();
        handler.DisposeCount.Should().Be(1);
    }

    private AuthenticationResult CreateFakeAuthenticationResult(bool isValid = true, string name = "FakeToken")
        => new (
            name, 
            true, 
            name, 
            expiresOn: isValid ? _timeProvider.GetUtcNow().AddMinutes(5) : _timeProvider.GetUtcNow().AddMinutes(-5), 
            extendedExpiresOn: isValid ? _timeProvider.GetUtcNow().AddMinutes(6) : _timeProvider.GetUtcNow().AddMinutes(-4),
            tenantId: null, 
            account: null, 
            idToken: null,
            scopes: [], 
            correlationId: Guid.Empty);
    private DataverseEnvironment CreateFakeEnvironment()
        => new ()
        {
            Name = "Test Environment",
            ConnectionString = "TenantId=1234567890;Url=https://test.crm4.dynamics.com"
        };
}

public class FakeHttpMessageHandler : HttpMessageHandler
{
    public int DisposeCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
    }

    protected override void Dispose(bool disposing)
    {
        DisposeCount++;
        base.Dispose(disposing);
    }
}
