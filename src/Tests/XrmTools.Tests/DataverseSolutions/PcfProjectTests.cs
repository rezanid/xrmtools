#nullable enable
namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;
using XrmTools.DataverseSolutions;
using XrmTools.UI;

public class PcfProjectTests
{
    private static PcfCreateRequest CreateRequest() => new()
    {
        ParentDirectory = Path.GetFullPath("PCF projects"), ProjectName = "My Project",
        Namespace = "Contoso.Controls", ControlName = "MyControl", Template = "dataset", Framework = "react"
    };

    [Fact]
    public void CreateArguments_UseExplicitScaffoldOptions_AndOptionalInstall()
    {
        var request = CreateRequest();
        var args = PcfArguments.Create(request);
        args.Should().Equal("pcf", "init", "--namespace", "Contoso.Controls", "--name", "MyControl", "--template", "dataset", "--framework", "react", "--outputDirectory", Path.Combine(request.ParentDirectory, request.ProjectName));
        request.InstallDependencies = true;
        PcfArguments.Create(request).Should().Contain("--run-npm-install");
    }

    [Theory]
    [InlineData("1Control", "Contoso")]
    [InlineData("Control_1", "Contoso")]
    [InlineData("Control", "Contoso..Controls")]
    [InlineData("Control", "Contoso.1Controls")]
    public void CreateArguments_RejectInvalidIdentifiers(string name, string ns)
    {
        var request = CreateRequest(); request.ControlName = name; request.Namespace = ns;
        Action action = () => PcfArguments.Create(request);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("..")]
    [InlineData("sub\\folder")]
    [InlineData("project.")]
    public void CreateArguments_RejectUnsafeProjectPaths(string name)
    {
        var request = CreateRequest(); request.ProjectName = name;
        Action action = () => PcfArguments.Create(request);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("relative")]
    [InlineData("C:relative")]
    [InlineData("\\relative")]
    public void CreateArguments_RejectNonAbsoluteLocations(string location)
    {
        var request = CreateRequest(); request.ParentDirectory = location;
        Action action = () => PcfArguments.Create(request);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PushArguments_AlwaysIncludeEnvironment_AndOnlyRequestedOptions()
    {
        var request = new PcfPushRequest { EnvironmentUrl = "https://org.crm.dynamics.com", PublisherPrefix = "dev" };
        PcfArguments.Push(request).Should().Equal("pcf", "push", "--environment", request.EnvironmentUrl, "--publisher-prefix", "dev");
        request.Incremental = true; request.PublisherPrefix = string.Empty; request.SolutionUniqueName = " MySolution ";
        PcfArguments.Push(request).Should().Equal("pcf", "push", "--environment", request.EnvironmentUrl, "--solution-unique-name", "MySolution", "--incremental");
    }

    [Theory]
    [InlineData("dev", "MySolution")]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    public void PushArguments_RequireExactlyOneDestination(string prefix, string solution)
    {
        var request = new PcfPushRequest { EnvironmentUrl = "https://org.crm.dynamics.com", PublisherPrefix = prefix, SolutionUniqueName = solution };
        Action action = () => PcfArguments.Push(request);
        action.Should().Throw<ArgumentException>().WithMessage("*either*not both*");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PushDialog_SubmitsOnlyTheSelectedDestination(bool existingSolution)
    {
        var model = new PcfPushDialogViewModel(@"C:\projects\Control.pcfproj", "https://org.crm.dynamics.com")
        {
            PublisherPrefix = "dev", UseExistingSolution = existingSolution
        };
        if (existingSolution)
            await model.LoadSolutionsAsync(_ => Task.FromResult<System.Collections.Generic.IEnumerable<XrmTools.WebApi.Entities.Solution>>(
                [new() { UniqueName = "MySolution", FriendlyName = "My Solution" }]), CancellationToken.None);
        PcfPushRequest? submitted = null;
        var result = await model.PushAsync((request, _, _) => { submitted = request; return Task.CompletedTask; }, CancellationToken.None);
        result.Should().Be("done");
        submitted.Should().NotBeNull();
        submitted!.PublisherPrefix.Should().Be(existingSolution ? string.Empty : "dev");
        submitted.SolutionUniqueName.Should().Be(existingSolution ? "MySolution" : null);
        PcfArguments.Push(submitted).Should().NotContain(existingSolution ? "--publisher-prefix" : "--solution-unique-name");
        model.IsIdle.Should().BeTrue();
    }

    [Fact]
    public async Task PushDialog_InvalidInput_DoesNotStartAuthenticationOrDeployment()
    {
        var model = new PcfPushDialogViewModel(@"C:\projects\Control.pcfproj", "https://org.crm.dynamics.com");
        var called = false;
        var result = await model.PushAsync((_, _, _) => { called = true; return Task.CompletedTask; }, CancellationToken.None);
        result.Should().BeNull();
        called.Should().BeFalse();
        model.Status.Should().Contain("Choose either");
        model.IsIdle.Should().BeTrue();
    }

    [Fact]
    public async Task SolutionPicker_LoadsLazily_CachesSelection_AndRefreshes()
    {
        var model = new PcfPushDialogViewModel(@"C:\projects\Control.pcfproj", "https://org.crm.dynamics.com");
        var calls = 0;
        Func<CancellationToken, Task<System.Collections.Generic.IEnumerable<XrmTools.WebApi.Entities.Solution>>> load = _ =>
        {
            calls++;
            return Task.FromResult<System.Collections.Generic.IEnumerable<XrmTools.WebApi.Entities.Solution>>(
                [new() { UniqueName = "One", FriendlyName = "First solution" }, new() { UniqueName = "Two" }, new()]);
        };
        await model.LoadSolutionsAsync(load, CancellationToken.None);
        calls.Should().Be(0);
        model.UseExistingSolution = true;
        await model.LoadSolutionsAsync(load, CancellationToken.None);
        model.Solutions.Should().HaveCount(2);
        model.SelectedSolution!.UniqueName.Should().Be("One");
        model.SelectedSolution = model.Solutions[1];
        await model.LoadSolutionsAsync(load, CancellationToken.None);
        calls.Should().Be(1);
        await model.LoadSolutionsAsync(load, CancellationToken.None, refresh: true);
        calls.Should().Be(2);
        model.SelectedSolution!.UniqueName.Should().Be("Two");
        model.Status.Should().BeNull();
    }

    [Fact]
    public async Task SolutionPicker_FailureAndEmptyList_AllowRetryAndPrefixPush()
    {
        var model = new PcfPushDialogViewModel(@"C:\projects\Control.pcfproj", "https://org.crm.dynamics.com") { UseExistingSolution = true };
        await model.LoadSolutionsAsync(_ => throw new InvalidOperationException("network unavailable"), CancellationToken.None);
        model.Status.Should().Contain("network unavailable").And.Contain("Refresh");
        model.IsIdle.Should().BeTrue();
        await model.LoadSolutionsAsync(_ => Task.FromResult<System.Collections.Generic.IEnumerable<XrmTools.WebApi.Entities.Solution>>([]), CancellationToken.None, true);
        model.Status.Should().Contain("No unmanaged solutions");
        var called = false;
        await model.PushAsync((_, _, _) => { called = true; return Task.CompletedTask; }, CancellationToken.None);
        called.Should().BeFalse();
        model.Status.Should().Contain("Select an unmanaged solution");
        model.UsePublisherPrefix = true;
        model.PublisherPrefix = "dev";
        (await model.PushAsync((_, _, _) => Task.CompletedTask, CancellationToken.None)).Should().Be("done");
    }

    [Fact]
    public async Task SolutionPicker_Cancellation_DiscardsLateResultsAndAllowsRetry()
    {
        var model = new PcfPushDialogViewModel(@"C:\projects\Control.pcfproj", "https://org.crm.dynamics.com") { UseExistingSolution = true };
        using var cts = new CancellationTokenSource();
        var pending = new TaskCompletionSource<System.Collections.Generic.IEnumerable<XrmTools.WebApi.Entities.Solution>>();
        var loading = model.LoadSolutionsAsync(_ => pending.Task, cts.Token);
        model.IsBusy.Should().BeTrue();
        model.Status.Should().Contain("Loading unmanaged");
        cts.Cancel();
        pending.SetResult([new() { UniqueName = "Late" }]);
        await loading;
        model.Solutions.Should().BeEmpty();
        model.IsIdle.Should().BeTrue();
        await model.LoadSolutionsAsync(_ => Task.FromResult<System.Collections.Generic.IEnumerable<XrmTools.WebApi.Entities.Solution>>([new() { UniqueName = "Retry" }]), CancellationToken.None);
        model.SelectedSolution!.UniqueName.Should().Be("Retry");
    }

    [Theory]
    [InlineData("x")]
    [InlineData("mscrm1")]
    [InlineData("1abc")]
    [InlineData("tooLong99")]
    public void PushArguments_RejectInvalidPrefix(string prefix)
    {
        var request = new PcfPushRequest { EnvironmentUrl = "https://org.crm.dynamics.com", PublisherPrefix = prefix };
        Action action = () => PcfArguments.Push(request);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("http://org.crm.dynamics.com")]
    [InlineData("https://user:password@org.crm.dynamics.com")]
    [InlineData("https://org.crm.dynamics.com?x=1")]
    public void PushArguments_RejectUnsafeEnvironmentUrls(string url)
    {
        Action action = () => PcfArguments.Push(new PcfPushRequest { EnvironmentUrl = url, PublisherPrefix = "dev" });
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public Task PushDialog_DisplaysReadOnlyEnvironment_WithoutTwoWayBinding()
    {
        var completion = new TaskCompletionSource<bool>();
        var thread = new Thread(() =>
        {
            try
            {
                var dialog = (PcfPushDialog)Activator.CreateInstance(typeof(PcfPushDialog), nonPublic: true)!;
                try
                {
                    var model = new PcfPushDialogViewModel(@"C:\projects\Control.pcfproj", "https://org.crm.dynamics.com");
                    dialog.DataContext = model;
                    dialog.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    var panel = ((Grid)dialog.Content).Children.OfType<StackPanel>().Single();
                    var environment = panel.Children.OfType<TextBox>().Single(box => box.IsReadOnly);
                    var binding = BindingOperations.GetBindingExpression(environment, TextBox.TextProperty);
                    binding.Should().NotBeNull();
                    binding!.ParentBinding.Mode.Should().Be(BindingMode.OneWay);
                    binding.UpdateTarget();
                    environment.Text.Should().Be(model.EnvironmentUrl);
                    binding.HasError.Should().BeFalse();
                    var prefix = (TextBox)dialog.FindName("Prefix");
                    prefix.Text = "dev";
                    BindingOperations.GetBindingExpression(prefix, TextBox.TextProperty)!.UpdateSource();
                    model.PublisherPrefix.Should().Be("dev");
                    dialog.Show();
                    dialog.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    dialog.SizeToContent.Should().Be(System.Windows.SizeToContent.Height);
                    var originalWidth = dialog.ActualWidth;
                    var originalHeight = dialog.ActualHeight;
                    var progressControl = (XrmTools.Shell.Controls.ProgressControl)dialog.Template.FindName("FooterProgress", dialog);
                    var footerBorder = (Border)dialog.Template.FindName("FooterBorder", dialog);
                    var idleFooterBrush = footerBorder.BorderBrush;
                    var idleFooterHeight = footerBorder.ActualHeight;
                    var statusScroll = (ScrollViewer)dialog.FindName("StatusScroll");
                    progressControl.Visibility.Should().Be(System.Windows.Visibility.Collapsed);
                    progressControl.IsRunning.Should().BeFalse();
                    statusScroll.Visibility.Should().Be(System.Windows.Visibility.Collapsed);
                    var formBottom = panel.TransformToAncestor(dialog).Transform(new System.Windows.Point(0, panel.ActualHeight)).Y;
                    (originalHeight - formBottom).Should().BeLessThan(110, "the idle dialog should not reserve a large empty status area");
                    var pushButton = (Button)dialog.FindName("PushButton");
                    var cancelButton = (Button)dialog.FindName("CancelButton");
                    var solutionPicker = (ComboBox)dialog.FindName("Solution");
                    var solutionCalls = 0;
                    Func<CancellationToken, Task<System.Collections.Generic.IEnumerable<XrmTools.WebApi.Entities.Solution>>> loadSolutions = _ =>
                    {
                        solutionCalls++;
                        return Task.FromResult<System.Collections.Generic.IEnumerable<XrmTools.WebApi.Entities.Solution>>(
                            [new() { UniqueName = "SelectedSolution", FriendlyName = "Selected solution" }]);
                    };
                    typeof(PcfPushDialog).GetField("loadSolutions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(dialog, loadSolutions);
                    solutionPicker.IsVisible.Should().BeFalse();
                    model.UseExistingSolution = true;
                    dialog.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    solutionCalls.Should().Be(1);
                    solutionPicker.IsVisible.Should().BeTrue();
                    solutionPicker.IsEditable.Should().BeFalse();
                    solutionPicker.SelectedItem.Should().BeSameAs(model.SelectedSolution);
                    model.SelectedSolution!.UniqueName.Should().Be("SelectedSolution");
                    var refreshButton = ((Grid)solutionPicker.Parent).Children.OfType<Button>().Single();
                    refreshButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
                    dialog.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    solutionCalls.Should().Be(2);
                    model.SelectedSolution!.UniqueName.Should().Be("SelectedSolution");
                    model.UsePublisherPrefix = true;
                    dialog.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    solutionPicker.IsVisible.Should().BeFalse();
                    IProgress<string>? lastProgress = null;
                    Func<PcfPushRequest, CancellationToken, IProgress<string>?, Task> waitForAuthentication = async (_, token, progress) =>
                    {
                        lastProgress = progress;
                        progress!.Report("Complete sign-in in your browser.");
                        try { await Task.Delay(Timeout.Infinite, token); }
                        finally
                        {
                            // Cancellation cleanup need not finish before the dispatcher becomes idle.
                            await dialog.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.SystemIdle);
                        }
                    };
                    var pushField = typeof(PcfPushDialog).GetField("push", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
                    pushField.SetValue(dialog, waitForAuthentication);
                    pushButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
                    dialog.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    model.IsBusy.Should().BeTrue();
                    pushButton.IsEnabled.Should().BeFalse();
                    prefix.IsEnabled.Should().BeFalse();
                    cancelButton.IsEnabled.Should().BeTrue();
                    model.Status.Should().Contain("sign-in");
                    progressControl.Visibility.Should().Be(System.Windows.Visibility.Visible);
                    progressControl.IsRunning.Should().BeTrue();
                    footerBorder.BorderBrush.Should().BeSameAs(System.Windows.Media.Brushes.Transparent);
                    footerBorder.ActualHeight.Should().BeApproximately(idleFooterHeight, 0.1);
                    progressControl.ActualWidth.Should().BeApproximately(footerBorder.ActualWidth, 0.1);
                    progressControl.ActualHeight.Should().Be(3);
                    var progressTop = progressControl.TranslatePoint(new System.Windows.Point(), dialog).Y;
                    var footerTop = footerBorder.TranslatePoint(new System.Windows.Point(), dialog).Y;
                    progressTop.Should().BeApproximately(footerTop - 1, 0.1);
                    progressControl.Kind.Should().Be(XrmTools.Shell.Styles.ProgressKind.BarIndeterminate);
                    progressControl.Template.Should().NotBeNull();
                    var indicator = (System.Windows.FrameworkElement)progressControl.Template.FindName("PART_Indicator", progressControl);
                    indicator.ActualWidth.Should().BeGreaterThan(0);
                    indicator.RenderTransform.HasAnimatedProperties.Should().BeTrue();
                    dialog.Resources[XrmTools.Shell.Styles.ShellColors.AccentFillDefaultBrushKey] = System.Windows.Media.Brushes.CornflowerBlue;
                    progressControl.Background.Should().BeSameAs(System.Windows.Media.Brushes.CornflowerBlue);
                    dialog.Resources[XrmTools.Shell.Styles.ShellColors.AccentFillDefaultBrushKey] = System.Windows.Media.Brushes.Orange;
                    progressControl.Background.Should().BeSameAs(System.Windows.Media.Brushes.Orange);

                    cancelButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
                    WaitForIdle(model, dialog.Dispatcher);
                    model.IsIdle.Should().BeTrue();
                    model.Status.Should().Contain("canceled").And.Contain("Dataverse");
                    progressControl.Visibility.Should().Be(System.Windows.Visibility.Collapsed);
                    progressControl.IsRunning.Should().BeFalse();
                    footerBorder.BorderBrush.Should().BeSameAs(idleFooterBrush);
                    footerBorder.ActualHeight.Should().BeApproximately(idleFooterHeight, 0.1);
                    indicator.RenderTransform.Should().BeNull();
                    lastProgress!.Report("Late output must not overwrite cancellation.");
                    dialog.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    model.Status.Should().Contain("canceled");
                    pushButton.IsEnabled.Should().BeTrue();

                    pushButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
                    dialog.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    model.IsBusy.Should().BeTrue();
                    dialog.Close();
                    WaitForIdle(model, dialog.Dispatcher);
                    model.IsIdle.Should().BeTrue();
                    model.Status.Should().Contain("canceled");
                    dialog.IsVisible.Should().BeTrue();

                    var error = "PAC failed: " + new string('x', 4000);
                    Func<PcfPushRequest, CancellationToken, IProgress<string>?, Task> fail = (_, _, _) => throw new InvalidOperationException(error);
                    pushField.SetValue(dialog, fail);
                    pushButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
                    dialog.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    model.Status.Should().Be(error);
                    dialog.ActualWidth.Should().BeApproximately(originalWidth, 1);
                    dialog.ActualHeight.Should().BeLessThanOrEqualTo(originalHeight + 100);
                    progressControl.Visibility.Should().Be(System.Windows.Visibility.Collapsed);
                    footerBorder.BorderBrush.Should().BeSameAs(idleFooterBrush);
                    statusScroll.ActualHeight.Should().BeLessThanOrEqualTo(96);
                    statusScroll.HorizontalScrollBarVisibility.Should().Be(ScrollBarVisibility.Disabled);
                    statusScroll.ExtentWidth.Should().BeLessThanOrEqualTo(statusScroll.ViewportWidth + 1);
                    statusScroll.ScrollableHeight.Should().BeGreaterThan(0);
                }
                finally
                {
                    dialog.Close();
                }
                completion.SetResult(true);
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }

    private static void WaitForIdle(PcfPushDialogViewModel model, System.Windows.Threading.Dispatcher dispatcher)
    {
        var frame = new System.Windows.Threading.DispatcherFrame();
        var timeout = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Send, dispatcher)
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        PropertyChangedEventHandler onChanged = (_, args) =>
        {
            if (args.PropertyName == nameof(model.IsIdle) && model.IsIdle)
                frame.Continue = false;
        };
        EventHandler onTimeout = (_, _) => frame.Continue = false;
        model.PropertyChanged += onChanged;
        timeout.Tick += onTimeout;
        try
        {
            timeout.Start();
            if (!model.IsIdle) System.Windows.Threading.Dispatcher.PushFrame(frame);
        }
        finally
        {
            timeout.Stop();
            timeout.Tick -= onTimeout;
            model.PropertyChanged -= onChanged;
        }
        model.IsIdle.Should().BeTrue("cancellation should complete within ten seconds");
        dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
    }

    [Fact]
    public async Task CreateDialog_SurfacesFailure_AndAllowsRetry()
    {
        var model = new PcfCreateDialogViewModel("C:\\projects");
        var first = await model.CreateAsync((_, _) => throw new InvalidOperationException("failed"), CancellationToken.None);
        first.Should().BeNull(); model.Status.Should().Be("failed"); model.IsIdle.Should().BeTrue();
        var second = await model.CreateAsync((_, _) => Task.FromResult("project.pcfproj"), CancellationToken.None);
        second.Should().Be("project.pcfproj"); model.IsIdle.Should().BeTrue();
    }
}
#nullable restore
