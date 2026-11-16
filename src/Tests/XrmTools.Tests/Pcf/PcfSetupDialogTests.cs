#nullable enable
namespace XrmTools.Tests.Pcf;

using FluentAssertions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Xunit;
using XrmTools.UI;

public sealed class PcfSetupDialogTests
{
    [Fact]
    public Task ReadyDialog_HasOnlyDone_ChangeRevealsExistingControls_AndProjectPathIsSelectable() => OnStaThread(() =>
    {
        using var harness = new PcfSetupHarness();
        harness.OnBuildPath();
        harness.Project.InstallDependencies();
        var model = new PcfSetupDialogViewModel(harness.Project.ProjectFile, harness.Service, new PcfSetupViewModel(harness.Service));
        var dialog = Create(model);
        try
        {
            dialog.Show();
            Flush();
            model.IsIdle.Should().BeTrue();
            model.IsComplete.Should().BeTrue();
            ((TextBlock)dialog.FindName("ProjectText")).Text.Should().Be("Sample");
            ((Expander)dialog.FindName("SetupOptions")).IsExpanded.Should().BeFalse();
            ((Expander)dialog.FindName("InstallationOutput")).IsExpanded.Should().BeFalse();
            ((Expander)dialog.FindName("ProjectDetails")).IsExpanded.Should().BeFalse();
            var footer = (Panel)dialog.FooterContent;
            footer.Children.OfType<Button>().Where(button => button.IsVisible).Select(button => button.Content)
                .Should().Equal("_Done");

            ((Button)dialog.FindName("ChangeButton")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Flush();

            ((Expander)dialog.FindName("SetupOptions")).IsExpanded.Should().BeTrue();
            var view = (PcfSetupView)dialog.FindName("SetupView");
            view.NodeChoices.IsVisible.Should().BeTrue();
            view.NodeChoices.IsKeyboardFocusWithin.Should().BeTrue();
            view.NodeChoices.SelectedItem.Should().BeSameAs(model.Setup.Selected);
            harness.Npm.Commands.Should().BeEmpty("Change only reveals existing controls");
            view.DownloadInstructions.Visibility.Should().Be(Visibility.Collapsed);

            model.Setup.InstallDependencies = false;
            Flush();
            ((Button)dialog.FindName("SetUpButton")).Content.Should().Be("_Set up");
            ((Button)dialog.FindName("CloseButton")).IsVisible.Should().BeTrue();

            ((Expander)dialog.FindName("ProjectDetails")).IsExpanded = true;
            dialog.Width = dialog.MinWidth;
            dialog.Height = dialog.MinHeight;
            Flush();
            var path = (TextBox)dialog.FindName("ProjectPath");
            path.IsReadOnly.Should().BeTrue();
            path.Text.Should().Be(harness.Project.ProjectFile);
            path.SelectAll();
            path.SelectedText.Should().Be(harness.Project.ProjectFile);
            dialog.ResizeMode.Should().Be(ResizeMode.CanResize);
        }
        finally { dialog.Close(); }
    });

    [Theory]
    [InlineData("missing")]
    [InlineData("repair")]
    [InlineData("restart")]
    public Task RequiredActions_ExpandOptions_ButExplanationsRemainVisibleWhenCollapsed(string scenario) => OnStaThread(() =>
    {
        using var harness = new PcfSetupHarness();
        if (scenario == "repair")
        {
            harness.OnBuildPath();
            harness.Store.SetSetupIncomplete(harness.Project.ProjectDirectory, true);
        }
        else if (scenario == "restart") harness.Probe.InstallWithFnm("v22.15.0", isDefault: true);
        var model = new PcfSetupDialogViewModel(harness.Project.ProjectFile, harness.Service, new PcfSetupViewModel(harness.Service));
        var dialog = Create(model);
        try
        {
            dialog.Show();
            Flush();
            model.IsIdle.Should().BeTrue();
            model.IsComplete.Should().BeFalse();
            var options = (Expander)dialog.FindName("SetupOptions");
            options.IsExpanded.Should().BeTrue();
            var view = (PcfSetupView)dialog.FindName("SetupView");
            view.DownloadInstructions.Visibility.Should().Be(scenario == "missing" ? Visibility.Visible : Visibility.Collapsed);
            options.IsExpanded = false;
            Flush();

            var status = (TextBlock)dialog.FindName("StatusText");
            status.IsVisible.Should().BeTrue();
            status.Text.Should().Be(model.Status).And.NotBeNullOrWhiteSpace();
            ((TextBlock)dialog.FindName("DependenciesText")).IsVisible.Should().BeTrue();
            ((Button)dialog.FindName("CloseButton")).IsVisible.Should().BeTrue();
            if (scenario == "missing")
            {
                var problem = (TextBlock)dialog.FindName("ProblemText");
                problem.IsVisible.Should().BeTrue();
                problem.Text.Should().Be(model.Setup.Problem).And.Contain("Install Node.js");
            }
            else if (scenario == "repair")
            {
                status.Text.Should().Contain("Repair dependencies");
                ((TextBlock)dialog.FindName("DependenciesText")).Text.Should().Contain("removes this project's node_modules");
            }
            else
            {
                var notice = (TextBlock)dialog.FindName("InstanceNoticeText");
                notice.IsVisible.Should().BeTrue();
                notice.Text.Should().Be(model.Setup.InstanceNotice).And.Contain("restart Visual Studio");
                ((Button)dialog.FindName("RestartButton")).IsVisible.Should().BeTrue();
                ((Button)dialog.FindName("SecondWindowButton")).IsVisible.Should().BeTrue();
            }
        }
        finally { dialog.Close(); }
    });

    private static PcfSetupDialog Create(PcfSetupDialogViewModel model)
    {
        var dialog = (PcfSetupDialog)Activator.CreateInstance(typeof(PcfSetupDialog), nonPublic: true)!;
        dialog.DataContext = model;
        return dialog;
    }

    private static void Flush() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

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
