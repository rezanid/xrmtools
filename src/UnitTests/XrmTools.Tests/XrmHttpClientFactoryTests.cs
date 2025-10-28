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
        Func<Task> act = async () => await _factory.CreateClientAsync(environment);

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
        _authenticationServiceMock.Setup(x => x.AuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(authResult);

        // Act
        var client = await _factory.CreateClientAsync(environment);

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
        var client = await _factory.CreateClientAsync(environment);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be(validToken.AccessToken);
        _authenticationServiceMock.Verify(x => x.AuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Never);
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
        _authenticationServiceMock.Setup(x => x.AuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(newToken);

        // Act
        var client = await _factory.CreateClientAsync(environment);

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

    [Fact]
    public async Task CreateHttpClientAsync_Should_Authenticate_Only_Once_When_Called_Concurrently()
    {
        // Arrange
        var environment = CreateFakeEnvironment();
        var authResult = CreateFakeAuthenticationResult();

        _authenticationServiceMock
            .Setup(x => x.AuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(100);
                return authResult;
            });

        // Act
        var tasks = new Task<XrmHttpClient>[5];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = _factory.CreateClientAsync(environment);
        }
        await Task.WhenAll(tasks);

        // Assert
        _authenticationServiceMock.Verify(x => x.AuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Once);
        foreach (var client in tasks)
        {
            client.Result.DefaultRequestHeaders.Authorization.Parameter.Should().Be(authResult.AccessToken);
        }
    }

    [Fact]
    public async Task CreateHttpClientAsync_Should_Retry_After_Authentication_Failure()
    {
        // Arrange
        var environment = CreateFakeEnvironment();
        var authResult = CreateFakeAuthenticationResult();
        var call = 0;
        _authenticationServiceMock
            .Setup(x => x.AuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                call++;
                if (call == 1)
                {
                    throw new InvalidOperationException("Auth failed");
                }
                return Task.FromResult(authResult);
            });

        // Act + Assert: first call fails
        await FluentActions.Invoking(async () => await _factory.CreateClientAsync(environment))
            .Should().ThrowAsync<InvalidOperationException>();

        // Second call should retry and succeed
        var client = await _factory.CreateClientAsync(environment);
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be(authResult.AccessToken);
        _authenticationServiceMock.Verify(x => x.AuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateHttpClientAsync_CallerCancellation_Does_Not_Cancel_Shared_Auth()
    {
        // Arrange
        var environment = CreateFakeEnvironment();
        var authResult = CreateFakeAuthenticationResult();

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _authenticationServiceMock
            .Setup(x => x.AuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(150);
                return authResult;
            });

        using var cts = new CancellationTokenSource();

        var waiting = _factory.CreateClientAsync(environment, cts.Token);
        var stillWaiting = _factory.CreateClientAsync(environment);

        // Cancel only the first caller
        cts.CancelAfter(10);

        await FluentActions.Invoking(async () => await waiting).Should().ThrowAsync<OperationCanceledException>();

        // The in-flight shared auth should complete and the second caller should succeed
        var client = await stillWaiting;
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be(authResult.AccessToken);

        _authenticationServiceMock.Verify(x => x.AuthenticateAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Once);
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
