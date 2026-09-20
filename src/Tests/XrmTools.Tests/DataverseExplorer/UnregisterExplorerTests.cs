#nullable enable
namespace XrmTools.Tests.DataverseExplorer;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using XrmTools.Commands;
using XrmTools.DataverseExplorer.Models;
using XrmTools.DataverseExplorer.Services;
using XrmTools.Environments;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Attributes;
using XrmTools.Services;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;

public sealed class UnregisterExplorerTests
{
    [Fact]
    public void MenuTargetsPackagesAndStandaloneAssembliesButNotPackagedAssemblies()
    {
        var provider = new UnregisterExplorerCommandProvider(Mock.Of<IPluginRegistrationService>(), Mock.Of<ILogger<UnregisterCommand>>());
        var root = new CategoryNode();
        root.SetArtifactCategory("Assemblies");
        var assembly = new AssemblyNode { AssemblyId = Guid.NewGuid(), DisplayName = "Standalone", Parent = root };
        var package = new PackageNode { PackageId = Guid.NewGuid(), DisplayName = "Package" };
        var assemblyMenu = Assert.Single(provider.GetCommands(assembly));
        var assemblyTarget = Assert.IsType<UnregisterExplorerTarget>(assemblyMenu.CommandParameter);
        Assert.Equal(assembly.AssemblyId, assemblyTarget.Reference.Id);
        Assert.Equal("pluginassemblies", assemblyTarget.Reference.SetName);
        var packageTarget = Assert.IsType<UnregisterExplorerTarget>(Assert.Single(provider.GetCommands(package)).CommandParameter);
        Assert.Equal(package.PackageId, packageTarget.Reference.Id);
        Assert.Equal("pluginpackages", packageTarget.Reference.SetName);
        assembly.Parent = package;
        Assert.Empty(provider.GetCommands(assembly));
        Assert.Empty(provider.GetCommands(root));
        Assert.Empty(provider.GetCommands(new CustomApiNode()));
    }

    [Fact]
    public void OldMenuCannotExecuteAfterItsTreeSessionIsCancelled()
    {
        using var session = new CancellationTokenSource();
        var root = new CategoryNode { SessionToken = session.Token };
        var package = new PackageNode { PackageId = Guid.NewGuid(), Parent = root };
        var provider = new UnregisterExplorerCommandProvider(Mock.Of<IPluginRegistrationService>(), Mock.Of<ILogger<UnregisterCommand>>());
        var menu = Assert.Single(provider.GetCommands(package));
        Assert.True(menu.Command!.CanExecute(menu.CommandParameter));
        session.Cancel();
        Assert.False(menu.Command.CanExecute(menu.CommandParameter));
    }

    [Fact]
    public void PackageDeletionIncludesDependenciesFromEveryAssemblyBeforeDeletingPackage()
    {
        var packageId = Guid.NewGuid();
        var first = new PluginType
        {
            Id = Guid.NewGuid(),
            Steps = [new SdkMessageProcessingStep { Id = Guid.NewGuid(), Stage = Stages.PreOperation }],
            CustomApi = [new CustomApi { Id = Guid.NewGuid() }],
        };
        var second = new PluginType { Id = Guid.NewGuid() };
        var requests = PluginRegistrationService.CreateUnregisterRequests(new EntityReference("pluginpackages", packageId),
            [new PluginAssembly { PluginTypes = [first] }, new PluginAssembly { PluginTypes = [second] }]);
        try
        {
            Assert.Equal(new[]
            {
                $"sdkmessageprocessingsteps({first.Steps[0].Id})", $"customapis({first.CustomApi[0].Id})",
                $"plugintypes({first.Id})", $"plugintypes({second.Id})", $"pluginpackages({packageId})",
            }, requests.Select(request => request.RequestUri!.OriginalString));
        }
        finally { foreach (var request in requests) request.Dispose(); }
    }

    [Fact]
    public async Task ServerSidePackageMembershipPreventsIndependentAssemblyDeletion()
    {
        var api = new Mock<IWebApiService>(MockBehavior.Strict);
        api.Setup(service => service.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<PluginAssembly>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataQueryResponse<PluginAssembly> { Value = [new PluginAssembly { Package = new PluginPackage { Id = Guid.NewGuid() } }] });
        var environment = new Mock<IEnvironmentProvider>();
        environment.Setup(provider => provider.GetActiveEnvironmentAsync(true)).ReturnsAsync(new DataverseEnvironment { ConnectionString = "https://example.crm.dynamics.com" });
        var service = new PluginRegistrationService(api.Object, environment.Object, null!, null!, Mock.Of<ILogger<PluginRegistrationService>>(), null!);
        var result = await service.UnregisterAsync(new EntityReference("pluginassemblies", Guid.NewGuid()), TestContext.Current.CancellationToken);
        Assert.False(result.Succeeded);
        Assert.Contains("belongs to a plugin package", result.Message);
        Assert.Single(api.Invocations); // Query only; no deletion batch was submitted.
    }

    [Fact]
    public async Task CancelledSessionDoesNotQueryOrDelete()
    {
        var api = new Mock<IWebApiService>(MockBehavior.Strict);
        var environment = new Mock<IEnvironmentProvider>(MockBehavior.Strict);
        var service = new PluginRegistrationService(api.Object, environment.Object, null!, null!, Mock.Of<ILogger<PluginRegistrationService>>(), null!);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.UnregisterAsync(new EntityReference("pluginpackages", Guid.NewGuid()), cancellation.Token));
        Assert.Empty(api.Invocations);
        Assert.Empty(environment.Invocations);
    }
}
