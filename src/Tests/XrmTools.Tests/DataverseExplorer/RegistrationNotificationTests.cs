#nullable enable
namespace XrmTools.Tests.DataverseExplorer;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using XrmTools.Analyzers;
using XrmTools.Environments;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model.Configuration;
using XrmTools.Services;
using XrmTools.UI;
using XrmTools.Validation;
using XrmTools.WebApi;
using XrmTools.WebApi.Batch;
using XrmTools.WebApi.Entities;
using XrmTools.Xrm;
using XrmTools.Xrm.Repositories;

public sealed class RegistrationNotificationTests
{
    private static readonly DataverseEnvironment Environment = new() { ConnectionString = "https://example.crm.dynamics.com" };

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public async Task RegistrationNotifiesAfterCommittedChangesIncludingFailedFollowup(bool package, bool failInitial, bool failFollowup)
    {
        var file = Path.GetTempFileName();
        try
        {
            var model = new PluginAssemblyConfig
            {
                Name = "Example.Plugins", FilePath = file,
                Package = package ? new PluginPackage { Name = "Example.Package", Version = "1.0.0", Content = "AA==" } : null,
            };
            var metadata = new Mock<IXrmMetaDataService>();
            metadata.Setup(service => service.ParseProjectPluginsAsync(file, It.IsAny<CancellationToken>())).ReturnsAsync(model);
            var validator = new Mock<IValidationService>();
            validator.Setup(service => service.ValidateIfValidatorAvailableAsync(model, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidationResult.Success!);
            var messages = new Mock<ISdkMessageRepository>();
            messages.Setup(repository => repository.GetForEntitiesAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<SdkMessage>());
            var repositories = new Mock<IRepositoryFactory>();
            repositories.Setup(factory => factory.CreateRepository<ISdkMessageRepository>()).Returns(messages.Object);
            var api = new Mock<IWebApiService>(MockBehavior.Strict);
            api.Setup(service => service.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<PluginAssembly>>>(), false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ODataQueryResponse<PluginAssembly>());
            var batchCount = 0;
            api.Setup(service => service.SendAsync(It.IsAny<WebApiRequest<BatchResponse>>(), true, It.IsAny<CancellationToken>()))
                .Returns<WebApiRequest<BatchResponse>, bool, CancellationToken>((request, _, token) =>
                {
                    Assert.Equal(new Uri(Environment.BaseServiceUrl!, "$batch"), request.RequestUri);
                    batchCount++;
                    if (batchCount == 2 && failFollowup) throw new InvalidOperationException("Follow-up failed");
                    return BatchAsync(batchCount == 1 && failInitial, token);
                });
            var service = new PluginRegistrationService(api.Object, EnvironmentProvider(), metadata.Object, repositories.Object,
                Mock.Of<ILogger<PluginRegistrationService>>(), validator.Object);
            var changes = new List<PluginRegistrationChangedEventArgs>();
            // Listener failures must not turn a committed registration into a failed command.
            service.RegistrationChanged += (_, _) => throw new InvalidOperationException("Broken listener");
            service.RegistrationChanged += (_, change) => changes.Add(change);
            var result = await service.RegisterAsync(new RegistrationInput(file, true, null), Mock.Of<IPluginRegistrationUI>(), TestContext.Current.CancellationToken);
            Assert.Equal(!failInitial && !failFollowup, result.Succeeded);
            if (failInitial) Assert.Empty(changes);
            else
            {
                var change = Assert.Single(changes);
                Assert.Equal(Environment.BaseServiceUrl, change.EnvironmentUrl);
                Assert.Equal(model.Id, change.AssemblyId);
                Assert.Equal(model.Package?.Id, change.PackageId);
            }
            Assert.Equal(package && !failInitial ? 2 : 1, batchCount);
        }
        finally { File.Delete(file); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UnregisterNotifiesOnlyAfterConfirmedDeletion(bool failed)
    {
        var api = new Mock<IWebApiService>(MockBehavior.Strict);
        api.Setup(service => service.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<PluginAssembly>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataQueryResponse<PluginAssembly>());
        api.Setup(service => service.SendAsync(It.IsAny<WebApiRequest<BatchResponse>>(), true, It.IsAny<CancellationToken>()))
            .Returns<WebApiRequest<BatchResponse>, bool, CancellationToken>((_, _, token) => BatchAsync(failed, token));
        var service = new PluginRegistrationService(api.Object, EnvironmentProvider(), null!, null!, Mock.Of<ILogger<PluginRegistrationService>>(), null!);
        var changes = new List<PluginRegistrationChangedEventArgs>();
        service.RegistrationChanged += (_, change) => changes.Add(change);
        var id = Guid.NewGuid();
        var result = await service.UnregisterAsync(new EntityReference("pluginpackages", id), TestContext.Current.CancellationToken);
        Assert.Equal(!failed, result.Succeeded);
        if (failed) Assert.Empty(changes);
        else
        {
            var change = Assert.Single(changes);
            Assert.Equal(id, change.PackageId);
            Assert.Null(change.AssemblyId);
            Assert.Equal(Environment.BaseServiceUrl, change.EnvironmentUrl);
        }
    }

    private static IEnvironmentProvider EnvironmentProvider()
    {
        var provider = new Mock<IEnvironmentProvider>();
        provider.Setup(service => service.GetActiveEnvironmentAsync(true)).ReturnsAsync(Environment);
        return provider.Object;
    }

    private static async Task<BatchResponse> BatchAsync(bool failed, CancellationToken token)
    {
        using var part = new HttpResponseMessage(failed ? HttpStatusCode.BadRequest : HttpStatusCode.NoContent);
        if (failed) part.Content = new StringContent("{\"error\":{\"code\":\"0x1\",\"message\":\"Test failure\"}}", System.Text.Encoding.UTF8, "application/json");
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new MultipartContent("mixed", "batch_test") };
        ((MultipartContent)response.Content).Add(new HttpMessageContent(part));
        using var request = new BatchRequest(Environment.BaseServiceUrl!);
        return await request.CreateResponseAsync(response, token);
    }
}
