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

public class XrmHttpClientFactoryTests
{
    [Fact]
    public async Task CreateHttpClientAsync_Should_Create_New_HttpClient()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var environmentProviderMock = new Mock<IEnvironmentProvider>();
        var authenticationServiceMock = new Mock<IAuthenticationService>();
        var loggerMock = new Mock<ILogger<XrmHttpClientFactory>>();

        var environment = new DataverseEnvironment
        {
            Name = "TestEnvironment",
            ConnectionString = "TestConnectionString"
        };
        environmentProviderMock.Setup(m => m.GetActiveEnvironmentAsync())
                               .ReturnsAsync(environment);
        authenticationServiceMock.Setup(
            m => m.AuthenticateAsync(
                It.IsAny<AuthenticationParameters>(),
                It.IsAny<Action<string>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateFakeAuthenticationResult());

        var factory = new XrmHttpClientFactory(
            fakeTimeProvider,
            environmentProviderMock.Object,
            authenticationServiceMock.Object,
            loggerMock.Object);

        // Act
        var client = await factory.CreateHttpClientAsync();

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be("FakeToken");
    }

    [Fact]
    public async Task RecycleHandlersAsync_Should_Not_Disrupt_Ongoing_Requests()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var environmentProviderMock = new Mock<IEnvironmentProvider>();
        var authenticationServiceMock = new Mock<IAuthenticationService>();
        var loggerMock = new Mock<ILogger<XrmHttpClientFactory>>();

        var environment = CreateFakeEnvironment();
        environmentProviderMock.Setup(m => m.GetActiveEnvironmentAsync())
                               .ReturnsAsync(environment);
        authenticationServiceMock.Setup(m => m.AuthenticateAsync(It.IsAny<AuthenticationParameters>(),
            It.IsAny<Action<string>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateFakeAuthenticationResult());

        var factory = new XrmHttpClientFactory(
            fakeTimeProvider,
            environmentProviderMock.Object,
            authenticationServiceMock.Object,
            loggerMock.Object);

        var client = await factory.CreateHttpClientAsync();

        // Simulate an ongoing request
        var requestTask = Task.Run(async () =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://fakeurl.com");
            using var response = await client.SendAsync(request, CancellationToken.None);
        });

        // Act
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(6)); // Advance time to trigger recycling
        //TODO: await factory.RecycleHandlersAsync();

        // Assert - Ensure the ongoing request completes successfully
        await requestTask;
    }

    [Fact]
    public async Task DisposeAsync_Should_Clean_Up_All_Resources()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var environmentProviderMock = new Mock<IEnvironmentProvider>();
        var authenticationServiceMock = new Mock<IAuthenticationService>();
        var loggerMock = new Mock<ILogger<XrmHttpClientFactory>>();

        var environment = new DataverseEnvironment
        {
            Name = "TestEnvironment",
            ConnectionString = "TestConnectionString"
        };
        environmentProviderMock.Setup(m => m.GetActiveEnvironmentAsync())
                               .ReturnsAsync(environment);
        authenticationServiceMock.Setup(
            m => m.AuthenticateAsync(It.IsAny<AuthenticationParameters>(),
            It.IsAny<Action<string>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateFakeAuthenticationResult());

        var factory = new XrmHttpClientFactory(
            fakeTimeProvider,
            environmentProviderMock.Object,
            authenticationServiceMock.Object,
            loggerMock.Object);

        await factory.CreateHttpClientAsync(); // Create at least one client

        // Act
        await factory.DisposeAsync();

        // Assert
        // Check if timer was disposed
        // Note: You would need to expose a property or method in XrmHttpClientFactory to validate this, or use internal access.
        await factory.Invoking(async f => await f.CreateHttpClientAsync())
               .Should().ThrowAsync<ObjectDisposedException>("because the factory was disposed");
    }

    private AuthenticationResult CreateFakeAuthenticationResult()
        => new("FakeToken", true, "FakeToken", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(1), null, null, null, [], Guid.Empty);
    private DataverseEnvironment CreateFakeEnvironment()
        => new ()
        {
            Name = "Test Environment",
            ConnectionString = "TenantId=1234567890;Url=https://test.crm4.dynamics.com"
        };
}

