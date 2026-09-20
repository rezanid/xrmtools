#nullable enable
namespace XrmTools.Tests.DataverseExplorer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using XrmTools.DataverseExplorer.Models;
using XrmTools.DataverseExplorer.Services;
using XrmTools.DataverseExplorer.ViewModels;
using XrmTools.Logging.Compatibility;
using XrmTools.CodeGen.CustomApi;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;

public sealed class ExplorerTests
{
    [Fact]
    public void SearchPreservesIdentityAndSupportsNewNodeTypes() => OnSta(() =>
    {
        var root = new CategoryNode { DisplayName = "Root" };
        var child = new FutureNode { DisplayName = "Needle", Parent = root };
        var sibling = new FutureNode { DisplayName = "Other", Parent = root };
        root.Children.Add(child);
        root.Children.Add(sibling);
        using var vm = Create(root);
        vm.RefreshAsync().GetAwaiter().GetResult();
        vm.ApplySearchAsync("Needle", false, false).GetAwaiter().GetResult();
        Assert.Same(root, vm.VisibleRoots.Cast<ExplorerNodeBase>().Single());
        Assert.Same(child, root.VisibleChildren.Cast<ExplorerNodeBase>().Single());
        Assert.Same(root, child.Parent);
        Assert.Equal(2, root.Children.Count);
        vm.ClearSearchAsync().GetAwaiter().GetResult();
        Assert.Equal(2, root.VisibleChildren.Cast<ExplorerNodeBase>().Count());
    });

    [Fact]
    public void ExpandingDuringSearchUpdatesTheOriginalTreeAndLoadsOnlyOnce() => OnSta(() =>
    {
        var root = new CategoryNode { DisplayName = "Root" };
        var child = new FutureNode { DisplayName = "Needle", Parent = root };
        root.Children.Add(child);
        var loads = 0;
        child.LoadChildrenAsync = _ =>
        {
            loads++;
            child.Children.Add(new FutureNode { DisplayName = "Needle child", Parent = child });
            return Task.CompletedTask;
        };
        using var vm = Create(root);
        vm.RefreshAsync().GetAwaiter().GetResult();
        vm.ApplySearchAsync("Needle", false, false).GetAwaiter().GetResult();
        vm.LoadNodeAsync(child).GetAwaiter().GetResult();
        vm.LoadNodeAsync(child).GetAwaiter().GetResult();
        vm.ClearSearchAsync().GetAwaiter().GetResult();
        Assert.Single(child.Children);
        Assert.Same(child, root.Children.Single());
        Assert.Equal(1, loads);
        Assert.False(child.CanLoadChildren);
    });

    [Fact]
    public void FailedCategoryDoesNotBlockOthersAndCanBeRetried() => OnSta(() =>
    {
        var calls = 0;
        var failed = new CategoryNode();
        failed.LoadChildrenAsync = _ => ++calls == 1 ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask;
        var healthy = new CategoryNode { LoadChildrenAsync = _ => Task.CompletedTask };
        using var vm = new DataverseExplorerViewModel([new Provider(() => failed), new Provider(() => healthy)], Mock.Of<ILogger>());
        vm.RefreshAsync().GetAwaiter().GetResult();
        Assert.NotNull(failed.LoadError);
        Assert.True(failed.CanLoadChildren);
        Assert.True(healthy.AreChildrenLoaded);
        vm.LoadNodeAsync(failed).GetAwaiter().GetResult();
        Assert.Null(failed.LoadError);
        Assert.True(failed.AreChildrenLoaded);
    });

    [Fact]
    public void RefreshIgnoresOldLoadsAndClearsSelection() => OnSta(() =>
    {
        var pending = new TaskCompletionSource<bool>();
        var oldRoot = new CategoryNode { LoadChildrenAsync = _ => pending.Task };
        var newRoot = new CategoryNode { LoadChildrenAsync = _ => Task.CompletedTask };
        var calls = 0;
        using var vm = new DataverseExplorerViewModel([new Provider(() => ++calls == 1 ? oldRoot : newRoot)], Mock.Of<ILogger>());
        var first = vm.RefreshAsync();
        vm.SelectedNode = oldRoot;
        vm.RefreshAsync().GetAwaiter().GetResult();
        pending.SetResult(true);
        first.GetAwaiter().GetResult();
        Assert.Same(newRoot, vm.RootNodes.Single());
        Assert.Null(vm.SelectedNode);
        Assert.False(vm.IsLoading);
        Assert.False(oldRoot.AreChildrenLoaded);
    });

    [Fact]
    public void NodeStateRaisesBindingNotificationsAndCollapseAllUpdatesDescendants() => OnSta(() =>
    {
        var root = new CategoryNode { IsExpanded = true };
        var child = new FutureNode { IsExpanded = true, Parent = root, LoadChildrenAsync = _ => Task.CompletedTask };
        root.Children.Add(child);
        using var vm = Create(root);
        vm.RefreshAsync().GetAwaiter().GetResult();
        var notifications = new List<string?>();
        child.PropertyChanged += (_, e) => notifications.Add(e.PropertyName);
        vm.CollapseAllCommand.Execute(null);
        child.AreChildrenLoaded = true;
        Assert.False(root.IsExpanded);
        Assert.False(child.IsExpanded);
        Assert.Contains(nameof(ExplorerNodeBase.IsExpanded), notifications);
        Assert.Contains(nameof(ExplorerNodeBase.CanLoadChildren), notifications);
    });

    [Fact]
    public void CustomApiMenuHasTypedGenerationCommands() => OnSta(() =>
    {
        var api = new CustomApiNode { CustomApiId = Guid.NewGuid() };
        var provider = new CustomApiCommandProvider(Mock.Of<ICustomApiClientGenerator>(), Mock.Of<ILogger<CustomApiCommandProvider>>());
        var menu = Assert.Single(provider.GetCommands(api));
        Assert.Equal("Generate client code", menu.Header);
        Assert.Equal(new[] { "C# (for plugins)", "TypeScript (Dataverse forms)", "HTTP", "OData" }, menu.Children.Select(item => item.Header));
        Assert.All(menu.Children, item =>
        {
            Assert.True(item.IsEnabled);
            Assert.NotNull(item.Command);
            Assert.Equal(api.CustomApiId, Assert.IsType<GenerateClientTarget>(item.CommandParameter).ApiId);
        });
        Assert.Empty(provider.GetCommands(new AssemblyNode()));
        Assert.Empty(provider.GetCommands(new CustomApiParameterNode()));
    });

    [Fact]
    public void BuiltInRootsSeparatePackagesAndStandaloneAssemblies() => OnSta(() =>
    {
        var data = new Mock<IExplorerDataService>();
        var package = new PackageNode { PackageId = Guid.NewGuid() };
        var assembly = new AssemblyNode { AssemblyId = Guid.NewGuid() };
        data.Setup(service => service.LoadPackagesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { package });
        data.Setup(service => service.LoadAssembliesAsync(It.IsAny<CancellationToken>(), null)).ReturnsAsync(new[] { assembly });
        data.Setup(service => service.LoadTablesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<TableNode>());
        using var vm = new DataverseExplorerViewModel(
            [new TablesCategoryProvider(data.Object), new PluginAssembliesCategoryProvider(data.Object), new PluginPackagesCategoryProvider(data.Object)], Mock.Of<ILogger>());
        vm.RefreshAsync().GetAwaiter().GetResult();
        Assert.Equal(new[] { "Plugin Packages", "Plugin Assemblies", "Tables" }, vm.RootNodes.Select(node => node.DisplayName));
        Assert.Same(package, vm.RootNodes[0].Children.Single());
        Assert.Same(assembly, vm.RootNodes[1].Children.Single());
        Assert.Same(vm.RootNodes[0], package.Parent);
        Assert.Same(vm.RootNodes[1], assembly.Parent);
    });

    private static DataverseExplorerViewModel Create(CategoryNode root) => new([new Provider(() => root)], Mock.Of<ILogger>());

    [Fact]
    public void PackageLoaderUsesMetadataOnlyAndLoadsAssembliesForTheSelectedPackage() => OnSta(() =>
    {
        var packageId = Guid.NewGuid();
        var requests = new List<string>();
        var webApi = new Mock<IWebApiService>();
        webApi.Setup(service => service.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<PluginPackage>>>(), false, It.IsAny<CancellationToken>()))
            .Callback<WebApiRequest<ODataQueryResponse<PluginPackage>>, bool, CancellationToken>((request, _, _) => requests.Add(request.RequestUri!.OriginalString))
            .ReturnsAsync(new ODataQueryResponse<PluginPackage> { Value = [new PluginPackage { Id = packageId, Name = "Contoso", Version = "1.2.3" }] });
        webApi.Setup(service => service.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<PluginAssembly>>>(), false, It.IsAny<CancellationToken>()))
            .Callback<WebApiRequest<ODataQueryResponse<PluginAssembly>>, bool, CancellationToken>((request, _, _) => requests.Add(request.RequestUri!.OriginalString))
            .ReturnsAsync(new ODataQueryResponse<PluginAssembly> { Value = [new PluginAssembly { Id = Guid.NewGuid(), Name = "Contoso.Plugins" }] });
        var data = new ExplorerDataService(webApi.Object, Mock.Of<ILogger<ExplorerDataService>>());
        var package = Assert.Single(data.LoadPackagesAsync(CancellationToken.None).GetAwaiter().GetResult());
        Assert.Equal(packageId, package.PackageId);
        Assert.Equal("1.2.3", package.Version);
        Assert.True(package.CanLoadChildren);
        package.LoadChildrenAsync!(CancellationToken.None).GetAwaiter().GetResult();
        Assert.Same(package, Assert.Single(package.Children).Parent);
        data.LoadAssembliesAsync(CancellationToken.None).GetAwaiter().GetResult();
        Assert.Contains("_packageid_value eq " + packageId, requests[1]);
        Assert.Contains("_packageid_value eq null", requests[2]);
        Assert.All(requests, request => Assert.DoesNotContain("content", request));
    });
    private sealed class Provider(Func<CategoryNode> create) : IExplorerCategoryProvider
    {
        public int Order => 0;
        public CategoryNode CreateRoot() => create();
    }
    private sealed class FutureNode : ExplorerNodeBase
    {
        public override string ArtifactCategory => "Future";
    }
    private static void OnSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)), "Explorer test timed out.");
        if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
