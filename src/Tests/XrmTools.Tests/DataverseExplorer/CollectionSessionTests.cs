#nullable enable
namespace XrmTools.Tests.DataverseExplorer;

using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Xunit;
using XrmTools.DataverseExplorer.Models;
using XrmTools.DataverseExplorer.Services;
using XrmTools.DataverseExplorer.ViewModels;
using XrmTools.DataverseExplorer.Views;
using XrmTools.Logging.Compatibility;
using XrmTools.Shell.Styles;

public sealed class CollectionSessionTests
{
    private static readonly Uri Environment = new("https://example.crm.dynamics.com/api/data/v9.2/");

    [Fact]
    public Task SearchAndGroupSelectionKeepHiddenSelectionsAndUseMatchingScope() => OnSta(() =>
    {
        var op = new Operation();
        using var session = Session(op);
        session.LoadAsync().GetAwaiter().GetResult();
        session.Groups[0].Children[0].IsChecked = true;
        Assert.Null(session.Groups[0].IsChecked);
        session.SearchText = "new_B";
        session.Groups[0].IsChecked = true;
        Assert.True(session.Groups[0].Children.All(r => r.IsChecked == true));
        Assert.Equal("2 selected · 1 matching", session.SelectionSummary);
        session.Groups[0].IsChecked = false;
        session.SearchText = "";
        Assert.True(session.Groups[0].Children[0].IsChecked);
        Assert.False(session.Groups[0].Children[1].IsChecked);
        session.ClearSelectionCommand.Execute(null);
        Assert.False(session.ExecuteCommand.CanExecute(null));
    });

    [Fact]
    public Task RefreshReconcilesIdentitiesAndReportsRemovedSelections() => OnSta(() =>
    {
        var op = new Operation(); using var session = Session(op);
        session.LoadAsync().GetAwaiter().GetResult(); session.SelectMatchingCommand.Execute(null);
        op.Candidates = [Candidate("b", "Renamed B", "new_B")];
        session.LoadAsync().GetAwaiter().GetResult();
        Assert.True(session.Groups[0].Children[0].IsChecked);
        Assert.Contains("1 previously selected", session.Status);
        Assert.Equal("1 selected · 1 matching", session.SelectionSummary);

    });

    [Fact]
    public Task ExecutionCapturesHiddenSelectionAndFailureKeepsItForRetry() => OnSta(() =>
    {
        var closed = false;
        var op = new Operation { FailExecution = true };
        using var session = Session(op, () => closed = true);
        session.LoadAsync().GetAwaiter().GetResult(); session.SelectMatchingCommand.Execute(null);
        session.SearchText = "new_A";
        session.ExecuteCommand.ExecuteAsync(null).GetAwaiter().GetResult();
        Assert.False(closed); Assert.Equal(2, op.Executed.Count);
        Assert.Contains("test failure", session.Status);
        Assert.True(session.ExecuteCommand.CanExecute(null));
        op.FailExecution = false;
        session.ExecuteCommand.ExecuteAsync(null).GetAwaiter().GetResult(); Assert.True(closed);
    });

    [Fact]
    public Task DisposedSessionIgnoresLateDiscovery() => OnSta(() =>
    {
        var pending = new TaskCompletionSource<IReadOnlyList<ExplorerCollectionCandidate>>();
        var op = new Operation { Pending = pending.Task };
        var session = Session(op); var load = session.LoadAsync();
        Assert.False(session.ExecuteCommand.CanExecute(null));
        session.Dispose(); pending.SetResult(op.Candidates); load.GetAwaiter().GetResult();
        Assert.Empty(session.Groups); Assert.True(op.LastToken.IsCancellationRequested);
    });

    [Fact]
    public Task CancellationPreventsLateExecutionFromClosingAnotherWorkspace() => OnSta(() =>
    {
        var pending = new TaskCompletionSource<bool>();
        var closed = false;
        var op = new Operation { ExecutionPending = pending.Task };
        var session = Session(op, () => closed = true);
        session.LoadAsync().GetAwaiter().GetResult(); session.SelectMatchingCommand.Execute(null);
        var execution = session.ExecuteCommand.ExecuteAsync(null);
        Assert.True(session.IsBusy);
        session.Groups[0].Children[0].IsChecked = false;
        Assert.True(session.Groups[0].Children[0].IsChecked);
        session.Dispose(); pending.SetResult(true); execution.GetAwaiter().GetResult();
        Assert.False(closed); Assert.True(op.ExecutionToken.IsCancellationRequested);
    });

    [Fact]
    public Task WorkspaceControlsLoadAndSearchBindingsSelectCandidates() => OnSta(() =>
    {
        var op = new Operation(); using var session = Session(op);
        session.LoadAsync().GetAwaiter().GetResult();
        var control = new ExplorerCollectionControl { DataContext = session };
        control.Resources.MergedDictionaries.Add(DefaultStyles.Instance);
        control.Measure(new Size(300, 600)); control.Arrange(new Rect(0, 0, 300, 600)); control.UpdateLayout();
        var search = (TextBox)control.FindName("SearchBox");
        search.Text = "new_B";
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal("new_B", session.SearchText);
        var tree = (XrmTools.Shell.Controls.TreeView)control.FindName("CandidateTree");
        Assert.Single(tree.Items);
        session.SelectMatchingCommand.Execute(null);
        Assert.Equal("1 selected · 1 matching", session.SelectionSummary);
        var previews = System.Environment.GetEnvironmentVariable("XRMTOOLS_COLLECTION_PREVIEW_DIR");
        if (previews != null)
        {
            Directory.CreateDirectory(previews);
            session.SearchText = "";
            foreach (var dark in new[] { false, true })
            {
                control.Background = dark ? Brushes.Black : Brushes.White;
                control.Foreground = dark ? Brushes.White : Brushes.Black;
                control.Resources[ShellColors.TextFillPrimaryBrushKey] = control.Foreground;
                control.Resources[ShellColors.ControlFillDefaultBrushKey] = dark ? Brushes.DimGray : Brushes.White;
                control.Resources[ShellColors.ControlFillTransparentBrushKey] = control.Background;
                control.Resources[ShellColors.ControlAltFillSecondaryBrushKey] = dark ? Brushes.DimGray : Brushes.White;
                control.Resources[ShellColors.ControlStrongStrokeDefaultBrushKey] = dark ? Brushes.LightGray : Brushes.Gray;
                control.Resources[ShellColors.AccentFillDefaultBrushKey] = Brushes.RoyalBlue;
                control.Resources[ShellColors.TextOnAccentFillPrimaryBrushKey] = Brushes.White;
                control.Resources[ShellColors.ControlStrokeDefaultBrushKey] = Brushes.Gray;
                control.Measure(new Size(300, 600)); control.Arrange(new Rect(0, 0, 300, 600)); control.UpdateLayout();
                var bitmap = new RenderTargetBitmap(300, 600, 96, 96, PixelFormats.Pbgra32); bitmap.Render(control);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var file = File.Create(Path.Combine(previews, dark ? "collection-dark.png" : "collection-light.png")); encoder.Save(file);
            }
        }
    });

    [Fact]
    public Task CancelPreservesBrowsingIdentitySearchAndExpansion() => OnSta(() =>
    {
        var root = new CategoryNode { Id = "Assemblies", IsExpanded = true };
        root.SetArtifactCategory("Assemblies");
        var child = new CustomApiNode { Id = "a", DisplayName = "API", Parent = root }; root.Children.Add(child);
        using var vm = new DataverseExplorerViewModel([new Provider(root)], Mock.Of<ILogger>(), getEnvironmentUrl: () => Task.FromResult<Uri?>(Environment));
        vm.InitializeAsync().GetAwaiter().GetResult(); vm.SelectedNode = child;
        vm.ApplySearchAsync("API", false, false).GetAwaiter().GetResult();
        var control = new DataverseExplorerWindowControl { DataContext = vm };
        control.Measure(new Size(300, 600)); control.Arrange(new Rect(0, 0, 300, 600)); control.UpdateLayout();
        vm.BeginCollectionAsync(new Operation(), root).GetAwaiter().GetResult();
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal(Visibility.Collapsed, ((FrameworkElement)control.FindName("BrowseTree")).Visibility);
        Assert.NotNull(vm.Collection); vm.Collection!.CancelCommand.Execute(null);
        Assert.Null(vm.Collection); Assert.Same(root, vm.RootNodes[0]); Assert.Same(child, vm.SelectedNode);
        Assert.True(root.IsExpanded); Assert.Equal("API", vm.SearchText);
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal(Visibility.Visible, ((FrameworkElement)control.FindName("BrowseTree")).Visibility);
        Assert.Equal("API", ((TextBox)control.FindName("BrowseSearch")).Text);
    });

    private static ExplorerCollectionSession Session(Operation op, Action? close = null) => new(op, new CategoryNode { DisplayName = "Assemblies" }, Environment, Mock.Of<ILogger>(), close ?? (() => { }));
    private static ExplorerCollectionCandidate Candidate(string id, string label, string name) => new(id, label, name, "assembly", "Assembly", id);
    private sealed class Provider(CategoryNode root) : IExplorerCategoryProvider { public int Order => 0; public CategoryNode CreateRoot() => root; }
    private sealed class Operation : IExplorerCollectionOperation
    {
        public string Title => "Generate OpenAPI specification"; public string ActionLabel => "Generate";
        public IReadOnlyList<ExplorerCollectionCandidate> Candidates = [Candidate("a", "API A", "new_A"), Candidate("b", "API B", "new_B")];
        public IReadOnlyList<ExplorerCollectionCandidate> Executed = [];
        public bool FailExecution; public Task<IReadOnlyList<ExplorerCollectionCandidate>>? Pending; public CancellationToken LastToken;
        public Task? ExecutionPending; public CancellationToken ExecutionToken;
        public bool SupportsScope(ExplorerNodeBase scope) => true;
        public Task<IReadOnlyList<ExplorerCollectionCandidate>> LoadAsync(ExplorerNodeBase scope, Uri environment, CancellationToken token)
        { LastToken = token; return Pending ?? Task.FromResult(Candidates); }
        public Task ExecuteAsync(IReadOnlyList<ExplorerCollectionCandidate> selection, Uri environment, CancellationToken token)
        { Executed = selection; ExecutionToken = token; return ExecutionPending ?? (FailExecution ? Task.FromException(new InvalidOperationException("test failure")) : Task.CompletedTask); }
    }
    private static Task OnSta(Action action)
    {
        var completion = new TaskCompletionSource<bool>();
        var thread = new Thread(() => { try { action(); completion.SetResult(true); } catch (Exception ex) { completion.SetException(ex); } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); return completion.Task;
    }
}
