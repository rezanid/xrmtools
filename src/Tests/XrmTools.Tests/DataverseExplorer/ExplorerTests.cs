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
using XrmTools.Services;

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
    [Fact]
    public void RefreshMenuIsLimitedToUsefulNodes() => OnSta(() =>
    {
        var root = new CategoryNode();
        root.SetArtifactCategory("Assemblies");
        var package = new PackageNode();
        var provider = new RefreshExplorerCommandProvider();
        foreach (var node in new ExplorerNodeBase[] { root, package, new TableNode(), new AssemblyNode { Parent = root } })
        {
            node.ReloadAsync = _ => Task.FromResult<ExplorerNodeBase?>(node);
            node.RefreshAsync = () => Task.CompletedTask;
            Assert.Equal("Refresh", Assert.Single(provider.GetCommands(node)).Header);
        }
        foreach (var node in new ExplorerNodeBase[] { new PluginTypeNode(), new CustomApiNode(), new TableColumnNode(), new AssemblyNode { Parent = package }, new CategoryNode { Parent = package } })
        {
            node.ReloadAsync = _ => Task.FromResult<ExplorerNodeBase?>(node);
            node.RefreshAsync = () => Task.CompletedTask;
            Assert.Empty(provider.GetCommands(node));
        }
    });

    [Fact]
    public void BranchRefreshPreservesSearchExpansionAndSelectionWithoutChangingSiblings() => OnSta(() =>
    {
        var root = new CategoryNode();
        var old = LoadedAssembly("target", "Old name");
        var sibling = LoadedAssembly("other", "Other");
        root.Children.Add(old);
        root.Children.Add(sibling);
        var replacement = LoadedAssembly("target", "New name");
        old.ReloadAsync = _ => Task.FromResult<ExplorerNodeBase?>(replacement);
        using var vm = Create(root);
        vm.RefreshAsync().GetAwaiter().GetResult();
        old.IsExpanded = true;
        vm.SelectedNode = old.Children[0];
        vm.ApplySearchAsync("Needle", false, false).GetAwaiter().GetResult();
        var oldToken = old.Children[0].SessionToken;
        vm.RefreshNodeAsync(old).GetAwaiter().GetResult();
        Assert.Same(replacement, root.Children[0]);
        Assert.Same(sibling, root.Children[1]);
        Assert.True(replacement.IsExpanded);
        Assert.Same(replacement.Children[0], vm.SelectedNode);
        Assert.Equal("Needle", vm.SearchText);
        Assert.True(oldToken.IsCancellationRequested);
        Assert.False(sibling.SessionToken.IsCancellationRequested);
        Assert.Same(root, replacement.Parent);
    });

    [Fact]
    public void FailedRefreshKeepsExistingBranchAndCanRetry() => OnSta(() =>
    {
        var root = new CategoryNode();
        var old = LoadedAssembly("target", "Old");
        root.Children.Add(old);
        var attempts = 0;
        old.ReloadAsync = _ => ++attempts == 1
            ? Task.FromException<ExplorerNodeBase?>(new InvalidOperationException("offline"))
            : Task.FromResult<ExplorerNodeBase?>(LoadedAssembly("target", "New"));
        using var vm = Create(root);
        vm.RefreshAsync().GetAwaiter().GetResult();
        vm.SelectedNode = old.Children[0];
        vm.RefreshNodeAsync(old).GetAwaiter().GetResult();
        Assert.Same(old, root.Children[0]);
        Assert.NotNull(old.LoadError);
        Assert.False(old.SessionToken.IsCancellationRequested);
        Assert.Same(old.Children[0], vm.SelectedNode);
        vm.RefreshNodeAsync(old).GetAwaiter().GetResult();
        Assert.Equal("New", root.Children[0].DisplayName);
        Assert.Null(root.Children[0].LoadError);
    });

    [Fact]
    public void DeletedNodeIsRemovedAndOldMenusAndExpansionAreIgnored() => OnSta(() =>
    {
        var root = new CategoryNode();
        var old = new PackageNode { Id = "deleted", ReloadAsync = _ => Task.FromResult<ExplorerNodeBase?>(null) };
        root.Children.Add(old);
        using var vm = Create(root);
        vm.RefreshAsync().GetAwaiter().GetResult();
        vm.SelectedNode = old;
        var menu = Assert.Single(new RefreshExplorerCommandProvider().GetCommands(old));
        vm.RefreshNodeAsync(old).GetAwaiter().GetResult();
        Assert.Empty(root.Children);
        Assert.Null(vm.SelectedNode);
        Assert.False(menu.Command!.CanExecute(null));
        old.LoadChildrenAsync = _ => throw new InvalidOperationException("Detached load must not run");
        vm.LoadNodeAsync(old).GetAwaiter().GetResult();
        Assert.Null(old.LoadError);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RegistrationRefreshChangesOnlyAffectedBranchAndHandlesRemoval(bool removed) => OnSta(() =>
    {
        var environment = new Uri("https://example.crm.dynamics.com/api/data/v9.2/");
        var id = Guid.NewGuid();
        var calls = 0;
        var provider = new Provider(() =>
        {
            var root = new CategoryNode { Id = "Assemblies" };
            root.SetArtifactCategory("Assemblies");
            root.LoadChildrenAsync = _ =>
            {
                calls++;
                if (!removed || calls == 1) root.Children.Add(LoadedAssembly(id.ToString(), "Version " + calls));
                root.Children.Add(LoadedAssembly("unrelated", "Unrelated"));
                return Task.CompletedTask;
            };
            return root;
        });
        using var vm = new DataverseExplorerViewModel([provider], Mock.Of<ILogger>(), getEnvironmentUrl: () => Task.FromResult<Uri?>(environment));
        vm.RefreshAsync().GetAwaiter().GetResult();
        var original = vm.RootNodes[0];
        var unrelated = original.Children[1];
        vm.SelectedNode = original.Children[0].Children[0];
        vm.RefreshRegistrationAsync(new(new Uri("https://other.crm.dynamics.com/api/data/v9.2/"), null, id)).GetAwaiter().GetResult();
        Assert.Equal(1, calls);
        vm.RefreshRegistrationAsync(new(environment, null, id)).GetAwaiter().GetResult();
        Assert.Equal(2, calls);
        Assert.Same(unrelated, vm.RootNodes[0].Children.Last());
        Assert.False(unrelated.SessionToken.IsCancellationRequested);
        if (removed) Assert.Null(vm.SelectedNode);
        else
        {
            Assert.Equal("Version 2", vm.RootNodes[0].Children[0].DisplayName);
            Assert.Same(vm.RootNodes[0].Children[0].Children[0], vm.SelectedNode);
        }
    });

    [Fact]
    public void InFlightRefreshCannotPublishAfterFullRefresh() => OnSta(() =>
    {
        var pending = new TaskCompletionSource<ExplorerNodeBase?>();
        var old = new PackageNode { Id = "old", ReloadAsync = _ => pending.Task };
        var root = new CategoryNode();
        root.Children.Add(old);
        var latest = new CategoryNode();
        var calls = 0;
        using var vm = new DataverseExplorerViewModel([new Provider(() => ++calls == 1 ? root : latest)], Mock.Of<ILogger>());
        vm.RefreshAsync().GetAwaiter().GetResult();
        var refresh = vm.RefreshNodeAsync(old);
        vm.RefreshAsync().GetAwaiter().GetResult();
        pending.SetResult(new PackageNode { Id = "stale" });
        refresh.GetAwaiter().GetResult();
        Assert.Same(latest, vm.RootNodes.Single());
        Assert.Same(old, root.Children.Single());
        Assert.True(old.SessionToken.IsCancellationRequested);
    });

    private static AssemblyNode LoadedAssembly(string id, string name)
    {
        var node = new AssemblyNode { Id = id, DisplayName = name, AreChildrenLoaded = true };
        node.Children.Add(new PluginTypeNode { Id = "plugin", DisplayName = "Needle", Parent = node, AreChildrenLoaded = true });
        return node;
    }

    [Fact]
    public void PackageRefreshReloadsPreviouslyLoadedAssembliesAndLeavesUnopenedOnesLazy() => OnSta(() =>
    {
        var root = new CategoryNode();
        var old = new PackageNode { Id = "package", AreChildrenLoaded = true };
        var loaded = LoadedAssembly("loaded", "Loaded");
        loaded.IsExpanded = true;
        old.Children.Add(loaded);
        old.Children.Add(new AssemblyNode { Id = "lazy" });
        root.Children.Add(old);
        var replacement = new PackageNode { Id = "package" };
        var loadedCalls = 0;
        replacement.LoadChildrenAsync = _ =>
        {
            var assembly = new AssemblyNode { Id = "loaded" };
            assembly.LoadChildrenAsync = _ =>
            {
                loadedCalls++;
                assembly.Children.Add(new PluginTypeNode { Id = "plugin", DisplayName = "Updated plugin" });
                return Task.CompletedTask;
            };
            replacement.Children.Add(assembly);
            replacement.Children.Add(new AssemblyNode { Id = "lazy", LoadChildrenAsync = _ => throw new InvalidOperationException("Must remain lazy") });
            return Task.CompletedTask;
        };
        old.ReloadAsync = _ => Task.FromResult<ExplorerNodeBase?>(replacement);
        using var vm = Create(root);
        vm.RefreshAsync().GetAwaiter().GetResult();
        vm.SelectedNode = loaded.Children[0];
        vm.RefreshNodeAsync(old).GetAwaiter().GetResult();
        Assert.Equal(1, loadedCalls);
        Assert.Equal("Updated plugin", vm.SelectedNode!.DisplayName);
        Assert.True(replacement.Children[0].IsExpanded);
        Assert.True(replacement.Children[1].CanLoadChildren);
        Assert.Same(replacement.Children[0], vm.SelectedNode.Parent);
    });

    [Fact]
    public void RegistrationRefreshDiscoversNewPackagesWithoutLoadingTheirAssemblies() => OnSta(() =>
    {
        var environment = new Uri("https://example.crm.dynamics.com/api/data/v9.2/");
        var id = Guid.NewGuid();
        var calls = 0;
        var provider = new Provider(() =>
        {
            var root = new CategoryNode { Id = "Packages" };
            root.SetArtifactCategory("Packages");
            root.LoadChildrenAsync = _ =>
            {
                if (++calls > 1) root.Children.Add(new PackageNode
                {
                    Id = id.ToString(), PackageId = id,
                    LoadChildrenAsync = _ => throw new InvalidOperationException("New packages should remain lazy"),
                });
                return Task.CompletedTask;
            };
            return root;
        });
        using var vm = new DataverseExplorerViewModel([provider], Mock.Of<ILogger>(), getEnvironmentUrl: () => Task.FromResult<Uri?>(environment));
        vm.RefreshAsync().GetAwaiter().GetResult();
        Assert.Empty(vm.RootNodes[0].Children);
        vm.RefreshRegistrationAsync(new(environment, id, null)).GetAwaiter().GetResult();
        var package = Assert.IsType<PackageNode>(Assert.Single(vm.RootNodes[0].Children));
        Assert.Equal(id, package.PackageId);
        Assert.True(package.CanLoadChildren);
    });

    [Fact]
    public void DisposalUnsubscribesAndQueuedNotificationsAreIgnored() => OnSta(() =>
    {
        var registration = new Mock<IPluginRegistrationService>();
        var vm = new DataverseExplorerViewModel([], Mock.Of<ILogger>(), registration.Object);
        vm.Dispose();
        registration.VerifyRemove(service => service.RegistrationChanged -= It.IsAny<EventHandler<PluginRegistrationChangedEventArgs>>(), Times.Once);
        vm.RefreshRegistrationAsync(new(new Uri("https://example.crm.dynamics.com/api/data/v9.2/"), null, Guid.NewGuid())).GetAwaiter().GetResult();
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
