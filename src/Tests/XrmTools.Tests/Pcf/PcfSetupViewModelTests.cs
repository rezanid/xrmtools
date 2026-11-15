#nullable enable
namespace XrmTools.Tests.Pcf;

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools.DataverseSolutions;
using XrmTools.Pcf.Setup;
using XrmTools.UI;

public sealed class PcfSetupViewModelTests : IDisposable
{
    private readonly PcfSetupHarness _h = new();
    private readonly List<string> _log = [];
    private readonly List<PcfCreateRequest> _requests = [];

    public void Dispose() => _h.Dispose();

    private PcfSetupViewModel Setup() => new(_h.Service, _log.Add);

    private async Task<PcfCreateDialogViewModel> WizardAsync()
    {
        var wizard = new PcfCreateDialogViewModel(Path.GetDirectoryName(_h.Project.ProjectDirectory)!, Setup())
        {
            ProjectName = "Sample", ControlName = "SampleControl", Namespace = "Contoso", Template = "field", Framework = "none"
        };
        await wizard.Setup!.RefreshAsync(null, wizard.TargetDirectory, CancellationToken.None);
        return wizard;
    }

    // Stands in for "pac pcf init": the fixture already holds the generated project.
    private Task<string> Create(PcfCreateRequest request, CancellationToken token)
    {
        _requests.Add(request);
        return Task.FromResult(_h.Project.ProjectFile);
    }

    [Fact]
    public async Task Wizard_WithUsableNode_StaysSimple_ShowsTheVersion_AndInstallsByDefault()
    {
        _h.OnBuildPath("v22.15.0");

        var wizard = await WizardAsync();

        wizard.Setup!.IsReady.Should().BeTrue();
        wizard.Setup.IsExpanded.Should().BeFalse("nothing needs the developer's attention");
        wizard.Setup.InstallDependencies.Should().BeTrue();
        wizard.Setup.Summary.Should().Contain("v22.15.0").And.Contain("npm 10.9.2").And.Contain("Visual Studio's PATH");
        wizard.Setup.InstallSummary.Should().Contain("will be installed after the project is created");
        wizard.Setup.HasProblem.Should().BeFalse();

        var project = await wizard.CreateAsync(Create, CancellationToken.None);

        project.Should().Be(_h.Project.ProjectFile);
        _requests.Single().InstallDependencies.Should().BeFalse("the install is its own cancelable step with the selected Node.js");
        _h.Npm.Commands.Should().Equal("install");
        wizard.SetupIncomplete.Should().BeFalse();
        _h.Store.GetSelection(_h.Project.ProjectFile).Should().BeNull("the automatic choice is not stored as a selection");
        _log.Should().Contain(line => line.Contains("npm install"));
    }

    [Fact]
    public async Task Wizard_OptOut_CreatesWithoutInstalling_AndDoesNotReportIncompleteSetup()
    {
        _h.OnBuildPath();
        var wizard = await WizardAsync();
        wizard.Setup!.InstallDependencies = false;
        wizard.Setup.InstallSummary.Should().Contain("will not be installed now");

        var project = await wizard.CreateAsync(Create, CancellationToken.None);

        project.Should().Be(_h.Project.ProjectFile);
        _h.Npm.Commands.Should().BeEmpty();
        wizard.SetupIncomplete.Should().BeFalse();
        (await _h.InspectAsync()).Dependencies.Status.Should().Be(PcfDependencyStatus.Missing);
    }

    [Fact]
    public async Task Wizard_WithoutNode_ExplainsAndExpandsSetup_ButStillCreatesTheProject()
    {
        var wizard = await WizardAsync();

        wizard.Setup!.IsReady.Should().BeFalse();
        wizard.Setup.IsExpanded.Should().BeTrue();
        wizard.Setup.Summary.Should().Contain("needs your attention");
        wizard.Setup.Problem.Should().Contain("Node.js was not found").And.Contain("Install Node.js");
        wizard.Setup.InstallSummary.Should().Contain("cannot be installed until Node.js is set up");

        var project = await wizard.CreateAsync(Create, CancellationToken.None);

        project.Should().BeNull("the dialog stays open to offer Retry setup and Open project");
        wizard.CreatedProject.Should().Be(_h.Project.ProjectFile);
        wizard.IsCreated.Should().BeTrue();
        wizard.SetupIncomplete.Should().BeTrue();
        wizard.Status.Should().Contain("project was created").And.Contain("Node.js was not found").And.Contain("Retry setup");
        _h.Npm.Commands.Should().BeEmpty();
    }

    [Fact]
    public async Task Wizard_InstallFailure_KeepsTheProject_AndRetryAfterCorrectionSucceedsWithoutCreatingAgain()
    {
        _h.OnBuildPath();
        var wizard = await WizardAsync();
        _h.Npm.Behavior = (_, _, _) => Task.FromResult(FakeNpmRunner.Fail("npm error code E401", "npm error Incorrect or missing password."));

        var failed = await wizard.CreateAsync(Create, CancellationToken.None);

        failed.Should().BeNull();
        wizard.CreatedProject.Should().Be(_h.Project.ProjectFile);
        wizard.SetupIncomplete.Should().BeTrue();
        wizard.Status.Should().Contain("project was created").And.Contain("setup is incomplete").And.Contain("E401").And.Contain("Retry setup");
        wizard.IsIdle.Should().BeTrue();
        File.Exists(_h.Project.ProjectFile).Should().BeTrue();

        _h.Npm.Behavior = null;
        var retried = await wizard.CreateAsync(Create, CancellationToken.None);

        retried.Should().Be(_h.Project.ProjectFile);
        wizard.SetupIncomplete.Should().BeFalse();
        _requests.Should().HaveCount(1, "retrying setup must not run pac pcf init a second time");
    }

    [Fact]
    public async Task Wizard_CancelDuringInstall_ReportsCancellation_NotSuccess()
    {
        _h.OnBuildPath();
        var wizard = await WizardAsync();
        using var cancellation = new CancellationTokenSource();
        _h.Npm.Behavior = async (_, _, token) => { cancellation.Cancel(); await Task.Delay(Timeout.Infinite, token); return new PcfNpmResult(); };

        var project = await wizard.CreateAsync(Create, cancellation.Token);

        project.Should().BeNull();
        wizard.SetupIncomplete.Should().BeTrue();
        wizard.Status.Should().Contain("canceled").And.Contain("project was created");
        (await _h.InspectAsync()).SetupRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Wizard_ExplicitChoice_IsUsedForTheInstall_AndRememberedForTheProject()
    {
        _h.OnBuildPath("v22.15.0");
        _h.Probe.InstallWithFnm("v24.21.0", isDefault: true);
        var wizard = await WizardAsync();
        wizard.Setup!.Choices.Select(choice => choice.Installation.Version.ToString()).Should().Equal("v22.15.0", "v24.21.0");
        wizard.Setup.Selected = wizard.Setup.Choices.Single(choice => choice.Installation.Source == NodeEnvironmentSource.Fnm);
        wizard.Setup.IsExplicitChoice.Should().BeTrue();

        (await wizard.CreateAsync(Create, CancellationToken.None)).Should().NotBeNull();

        _h.Npm.Nodes.Single().Version.ToString().Should().Be("v24.21.0");
        _h.Store.GetSelection(_h.Project.ProjectFile)!.NodeDirectory.Should().Be(@"C:\fnm\node-versions\v24.21.0\installation");
    }

    [Fact]
    public async Task Setup_BrowsedInstallation_IsSelected_AndABadFolderIsRejected()
    {
        var setup = Setup();
        await setup.RefreshAsync(_h.Project.ProjectFile, _h.Project.ProjectDirectory, CancellationToken.None);
        setup.IsReady.Should().BeFalse();

        (await setup.SelectDirectoryAsync(@"D:\not-node", CancellationToken.None)).Should().BeFalse();
        (await setup.SelectDirectoryAsync(_h.Probe.Install(@"D:\portable\node", "v22.15.0"), CancellationToken.None)).Should().BeTrue();

        setup.IsReady.Should().BeTrue();
        setup.Summary.Should().Contain("v22.15.0");
        setup.HasProblem.Should().BeFalse();
    }

    [Fact]
    public async Task Setup_ChoiceThatDoesNotFitTheProject_IsExplained_AndNotReady()
    {
        _h.OnBuildPath("v22.15.0");
        _h.Project.Write(".nvmrc", "24");
        _h.Probe.InstallWithFnm("v24.21.0", isDefault: false, Path.Combine(_h.Project.ProjectDirectory, ".nvmrc"));
        var setup = Setup();
        await setup.RefreshAsync(_h.Project.ProjectFile, _h.Project.ProjectDirectory, CancellationToken.None);

        setup.Selected!.Installation.Version.ToString().Should().Be("v24.21.0");
        setup.Requirement.Should().Contain(".nvmrc: 24");
        setup.Selected = setup.Choices.Single(choice => choice.Installation.Source == NodeEnvironmentSource.BuildEnvironment);

        setup.IsReady.Should().BeFalse();
        setup.Selected.Display.Should().Contain("does not fit this project");
        setup.Problem.Should().Contain("v22.15.0").And.Contain(".nvmrc").And.Contain("24");
    }

    [Fact]
    public async Task ExistingProject_SetUp_InstallsOnce_ThenReportsReady()
    {
        _h.OnBuildPath();
        _h.Project.Write("package-lock.json", "{}");
        var dialog = new PcfSetupDialogViewModel(_h.Project.ProjectFile, _h.Service, Setup());
        await dialog.LoadAsync(CancellationToken.None);

        dialog.IsComplete.Should().BeFalse();
        dialog.Dependencies.Should().Contain("Not installed").And.Contain("npm ci");
        dialog.PackageManager.Should().StartWith("npm");
        dialog.Status.Should().Contain("Choose Set up");

        (await dialog.ApplyAsync(false, CancellationToken.None)).Should().BeTrue();

        dialog.IsComplete.Should().BeTrue();
        dialog.Status.Should().Contain("ready to build").And.Contain("v22.15.0");
        dialog.Dependencies.Should().Be("Installed.");
        (await dialog.ApplyAsync(false, CancellationToken.None)).Should().BeTrue();
        _h.Npm.Commands.Should().Equal(["ci"], "a second Set up has nothing left to install");
    }

    [Fact]
    public async Task ExistingProject_FailedSetup_ShowsTheCause_OffersLockFileUpdateOnlyForAMismatch_AndIsNotReady()
    {
        _h.OnBuildPath();
        _h.Project.Write("package-lock.json", "{}");
        _h.Npm.Behavior = (_, _, _) => Task.FromResult(FakeNpmRunner.Fail("npm error code EUSAGE", "npm error `npm ci` can only install packages when your package.json and package-lock.json or npm-shrinkwrap.json are in sync."));
        var dialog = new PcfSetupDialogViewModel(_h.Project.ProjectFile, _h.Service, Setup());
        await dialog.LoadAsync(CancellationToken.None);

        (await dialog.ApplyAsync(false, CancellationToken.None)).Should().BeFalse();

        dialog.IsComplete.Should().BeFalse();
        dialog.Status.Should().Contain("package-lock.json does not match package.json").And.Contain("npm error code EUSAGE");
        dialog.Setup.CanUpdateLockFile.Should().BeTrue();
        dialog.IsIdle.Should().BeTrue();

        _h.Npm.Behavior = null;
        (await dialog.ApplyAsync(true, CancellationToken.None)).Should().BeTrue();
        _h.Npm.Commands.Should().Equal("ci", "install");
        dialog.Setup.CanUpdateLockFile.Should().BeFalse();
    }

    [Fact]
    public async Task ExistingProject_OtherPackageManager_IsExplained_AndNothingIsInstalled()
    {
        _h.OnBuildPath();
        _h.Project.Write("pnpm-lock.yaml", "");
        var dialog = new PcfSetupDialogViewModel(_h.Project.ProjectFile, _h.Service, Setup());
        await dialog.LoadAsync(CancellationToken.None);

        dialog.PackageManager.Should().Contain("pnpm").And.Contain("will not run npm").And.Contain("PcfEnableAutoNpmInstall");
        dialog.Status.Should().Contain("Install dependencies with pnpm");

        (await dialog.ApplyAsync(false, CancellationToken.None)).Should().BeFalse();
        _h.Npm.Commands.Should().BeEmpty();
        _h.Project.Exists("package-lock.json").Should().BeFalse();
    }

    [Fact]
    public async Task ExistingProject_ChangedSelection_IsSaved_AndAutomaticSelectionCanBeRestored()
    {
        _h.OnBuildPath("v22.15.0");
        _h.Project.InstallDependencies();
        _h.Probe.InstallWithFnm("v24.21.0", isDefault: true);
        var dialog = new PcfSetupDialogViewModel(_h.Project.ProjectFile, _h.Service, Setup());
        await dialog.LoadAsync(CancellationToken.None);
        dialog.IsComplete.Should().BeTrue();
        dialog.Setup.HasStoredSelection.Should().BeFalse();

        dialog.Setup.Selected = dialog.Setup.Choices.Single(choice => choice.Installation.Source == NodeEnvironmentSource.Fnm);
        (await dialog.ApplyAsync(false, CancellationToken.None)).Should().BeTrue();

        dialog.Setup.HasStoredSelection.Should().BeTrue();
        dialog.Status.Should().Contain("v24.21.0");
        (await _h.InspectAsync()).Environment.Installation!.Source.Should().Be(NodeEnvironmentSource.Selected);

        dialog.Setup.ClearSelection();
        await dialog.LoadAsync(CancellationToken.None);
        dialog.Setup.HasStoredSelection.Should().BeFalse();
        dialog.Setup.Installation!.Source.Should().Be(NodeEnvironmentSource.BuildEnvironment);
    }
}
#nullable restore
