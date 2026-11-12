#nullable enable
namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Xunit;
using XrmTools.DataverseSolutions;
using XrmTools.UI;
using XrmTools.WebApi.Entities;

public class PcfPushDestinationTests
{
    private static PcfPushDialogViewModel Model() => new(@"C:\projects\Control.pcfproj", "https://org.crm.dynamics.com");
    private static PcfSolutionDestination Ref(string name) => new($@"C:\projects\{name}\{name}.cdsproj", name);
    private static Task<IEnumerable<Solution>> Load(params string[] names)
        => Task.FromResult<IEnumerable<Solution>>(names.Select(name => new Solution { UniqueName = name, FriendlyName = name + " display" }).ToArray());

    [Fact]
    public async Task NoReferencingProject_KeepsManualOptionsAndDoesNotLoadSolutions()
    {
        var model = Model();
        await model.InitializeAsync(_ => Task.FromResult<IReadOnlyList<PcfSolutionDestination>>([]),
            _ => throw new InvalidOperationException("Solution lookup must remain lazy."), CancellationToken.None);
        model.ShowDestinationOptions.Should().BeTrue();
        model.UsePublisherPrefix.Should().BeTrue();
        model.Status.Should().BeNull();
    }

    [Fact]
    public async Task SingleReference_AutoSelectsOnlyMatchingUnmanagedSolutionAndShowsConfirmation()
    {
        var model = Model();
        await model.InitializeAsync(_ => Task.FromResult<IReadOnlyList<PcfSolutionDestination>>([Ref("One")]),
            _ => Load("Unrelated", "One"), CancellationToken.None);
        model.ShowProjectDestination.Should().BeTrue();
        model.ShowSolutionPicker.Should().BeFalse();
        model.SelectedSolution!.UniqueName.Should().Be("One");
        model.DestinationDisplayName.Should().Contain("One display").And.Contain("One");
        model.DestinationNotice.Should().Contain("One.cdsproj");
        PcfPushRequest? submitted = null;
        (await model.PushAsync((request, _, _) => { submitted = request; return Task.CompletedTask; }, CancellationToken.None)).Should().Be("done");
        submitted!.SolutionUniqueName.Should().Be("One");
        submitted.PublisherPrefix.Should().BeEmpty();
        model.ChangeDestination();
        model.SolutionChoices.Select(solution => solution.UniqueName).Should().Contain("Unrelated");
    }

    [Fact]
    public async Task MultipleReferences_RequireChoiceAmongRelatedSolutions()
    {
        var model = Model();
        await model.InitializeAsync(_ => Task.FromResult<IReadOnlyList<PcfSolutionDestination>>([Ref("One"), Ref("Two")]),
            _ => Load("Unrelated", "One", "Two"), CancellationToken.None);
        model.ShowSolutionPicker.Should().BeTrue();
        model.SelectedSolution.Should().BeNull();
        model.SolutionChoices.Select(solution => solution.UniqueName).Should().BeEquivalentTo("One", "Two");
        var calls = 0;
        (await model.PushAsync((_, _, _) => { calls++; return Task.CompletedTask; }, CancellationToken.None)).Should().BeNull();
        calls.Should().Be(0);
        model.SelectedSolution = model.Solutions.Single(solution => solution.UniqueName == "One");
        model.SolutionChoices.Contains(model.SelectedSolution).Should().BeTrue();
    }

    [Fact]
    public async Task MultipleProjectsReferencingSameSolution_AutoSelectsTheSingleUniqueDestination()
    {
        var model = Model();
        await model.InitializeAsync(_ => Task.FromResult<IReadOnlyList<PcfSolutionDestination>>([Ref("One"), Ref("One")]),
            _ => Load("Unrelated", "One"), CancellationToken.None);
        model.SelectedSolution!.UniqueName.Should().Be("One");
        model.ShowSolutionPicker.Should().BeFalse();
    }

    [Fact]
    public async Task MissingReferencedSolution_DoesNotSilentlyChooseUnrelatedSolution()
    {
        var model = Model();
        await model.InitializeAsync(_ => Task.FromResult<IReadOnlyList<PcfSolutionDestination>>([Ref("Missing")]),
            _ => Load("Unrelated"), CancellationToken.None);
        model.ShowDestinationOptions.Should().BeTrue();
        model.SelectedSolution.Should().BeNull();
        model.DestinationNotice.Should().Contain("Missing").And.Contain("Not available as unmanaged");
        var calls = 0;
        (await model.PushAsync((_, _, _) => { calls++; return Task.CompletedTask; }, CancellationToken.None)).Should().BeNull();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task DiscoveryOrEnvironmentLookupFailure_RequiresExplicitDestination()
    {
        var model = Model();
        await model.InitializeAsync(_ => Task.FromResult<IReadOnlyList<PcfSolutionDestination>>([Ref("One")]),
            _ => throw new InvalidOperationException("network unavailable"), CancellationToken.None);
        model.ShowDestinationOptions.Should().BeTrue();
        model.SelectedSolution.Should().BeNull();
        model.DestinationNotice.Should().Contain("network unavailable").And.Contain("no alternative");
        var calls = 0;
        (await model.PushAsync((_, _, _) => { calls++; return Task.CompletedTask; }, CancellationToken.None)).Should().BeNull();
        calls.Should().Be(0);
        model.UsePublisherPrefix = true;
        model.PublisherPrefix = "dev";
        (await model.PushAsync((_, _, _) => Task.CompletedTask, CancellationToken.None)).Should().Be("done");
    }

    [Fact]
    public async Task RefreshThatRemovesInferredDestination_RequiresExplicitChoiceAndDoesNotRedirect()
    {
        var model = Model();
        await model.InitializeAsync(_ => Task.FromResult<IReadOnlyList<PcfSolutionDestination>>([Ref("One")]),
            _ => Load("One", "Other"), CancellationToken.None);
        await model.LoadSolutionsAsync(_ => Load("Other"), CancellationToken.None, refresh: true);
        model.ShowDestinationOptions.Should().BeTrue();
        model.SelectedSolution.Should().BeNull();
        model.Status.Should().Contain("no longer available");
        await model.LoadSolutionsAsync(_ => Load("Other"), CancellationToken.None, refresh: true);
        model.SelectedSolution.Should().BeNull();
    }

    [Fact]
    public Task CompiledDialog_ShowsInferredConfirmationAndChangeRevealsAllChoices() => OnStaThread(() =>
    {
        var dialog = (PcfPushDialog)Activator.CreateInstance(typeof(PcfPushDialog), nonPublic: true)!;
        try
        {
            var model = Model();
            dialog.DataContext = model;
            var loadCalls = 0;
            typeof(PcfPushDialog).GetField("findReferences", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(dialog,
                new Func<CancellationToken, Task<IReadOnlyList<PcfSolutionDestination>>>(_ => Task.FromResult<IReadOnlyList<PcfSolutionDestination>>([Ref("One")])));
            typeof(PcfPushDialog).GetField("loadSolutions", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(dialog,
                new Func<CancellationToken, Task<IEnumerable<Solution>>>(_ => { loadCalls++; return Load("Unrelated", "One"); }));
            dialog.Show();
            dialog.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            loadCalls.Should().Be(1);
            ((FrameworkElement)dialog.FindName("ProjectDestination")).Visibility.Should().Be(Visibility.Visible);
            ((FrameworkElement)dialog.FindName("DestinationOptions")).Visibility.Should().Be(Visibility.Collapsed);
            ((ComboBox)dialog.FindName("Solution")).IsVisible.Should().BeFalse();
            model.SelectedSolution!.UniqueName.Should().Be("One");
            ((Button)dialog.FindName("ChangeDestinationButton")).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            dialog.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            ((FrameworkElement)dialog.FindName("ProjectDestination")).Visibility.Should().Be(Visibility.Collapsed);
            ((FrameworkElement)dialog.FindName("DestinationOptions")).Visibility.Should().Be(Visibility.Visible);
            var picker = (ComboBox)dialog.FindName("Solution");
            picker.IsVisible.Should().BeTrue();
            picker.Items.Cast<Solution>().Select(solution => solution.UniqueName).Should().Contain("Unrelated");
            model.SelectedSolution!.UniqueName.Should().Be("One");
            loadCalls.Should().Be(1);
        }
        finally { dialog.Close(); }
    });

    [Fact]
    public void ReadDestination_UsesManifestUniqueNameOnlyForMatchingProjectReference()
    {
        var directory = Path.Combine(Path.GetTempPath(), "XrmTools-PcfDestination-" + Guid.NewGuid().ToString("N"));
        var root = Path.Combine(directory, "src");
        Directory.CreateDirectory(Path.Combine(root, "Other"));
        try
        {
            File.WriteAllText(Path.Combine(root, "Other", "Solution.xml"), "<ImportExportXml><SolutionManifest><UniqueName>ManifestName</UniqueName></SolutionManifest></ImportExportXml>");
            var project = Path.Combine(directory, "Solution.cdsproj");
            var pcf = Path.Combine(directory, "Control.pcfproj");
            PcfPushDestinationResolver.ReadDestination(project, pcf, [Path.Combine(directory, "Other.pcfproj")], "src").Should().BeNull();
            PcfPushDestinationResolver.ReadDestination(project, pcf, [pcf], "src")!.SolutionUniqueName.Should().Be("ManifestName");
            Action missing = () => PcfPushDestinationResolver.ReadDestination(project, pcf, [pcf], "other");
            missing.Should().Throw<InvalidOperationException>().WithMessage("*manifest was not found*");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    private static Task OnStaThread(Action action)
    {
        var completion = new TaskCompletionSource<bool>();
        var thread = new Thread(() =>
        {
            try { action(); completion.SetResult(true); }
            catch (Exception ex) { completion.SetException(ex); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }
}
#nullable restore
