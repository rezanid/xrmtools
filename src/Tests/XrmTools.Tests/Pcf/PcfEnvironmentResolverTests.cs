#nullable enable
namespace XrmTools.Tests.Pcf;

using FluentAssertions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools.Pcf.Setup;

public sealed class PcfEnvironmentResolverTests : IDisposable
{
    private readonly PcfSetupHarness _h = new();

    public void Dispose() => _h.Dispose();

    private Task<PcfEnvironmentResolution> ResolveAsync()
        => _h.Service.ResolveEnvironmentAsync(_h.Project.ProjectFile, _h.Project.ProjectDirectory, CancellationToken.None);

    private void Select(string directory)
        => _h.Store.SetSelection(_h.Project.ProjectFile, new PcfNodeSelection { NodeDirectory = directory, Version = "v20.11.0" });

    [Fact]
    public async Task WorkingBuildEnvironment_IsKept_WhenNothingIsDeclared()
    {
        _h.OnBuildPath("v22.15.0");
        _h.Probe.InstallWithFnm("v24.21.0", isDefault: true);

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.Ready);
        resolution.Installation!.Source.Should().Be(NodeEnvironmentSource.BuildEnvironment);
        resolution.Installation.Version.ToString().Should().Be("v22.15.0");
        resolution.AvailableToBuilds.Should().BeTrue();
        resolution.FromSelection.Should().BeFalse();
        _h.Probe.FnmRequests.Should().BeEmpty("fnm is not consulted while Visual Studio's own environment fits");
    }

    [Fact]
    public async Task ExplicitSelection_OutranksBuildEnvironmentAndFnm()
    {
        _h.OnBuildPath("v22.15.0");
        _h.Probe.InstallWithFnm("v24.21.0", isDefault: true);
        Select(_h.Probe.Install(@"D:\tools\node20", "v20.11.0"));

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.Ready);
        resolution.Installation!.Source.Should().Be(NodeEnvironmentSource.Selected);
        resolution.Installation.NodeDirectory.Should().Be(@"D:\tools\node20");
        resolution.FromSelection.Should().BeTrue();
        resolution.InstanceNode!.Version.ToString().Should().Be("v22.15.0");
        resolution.AvailableToBuilds.Should().BeFalse("this Visual Studio instance has another version, and its builds run with that");
    }

    [Fact]
    public async Task ExplicitSelection_OfTheVersionThisVisualStudioAlreadyHas_NeedsNoOtherInstance()
    {
        var instance = _h.OnBuildPath("v22.15.0");
        Select(_h.Probe.InstallWithFnm("v22.15.0"));

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.Ready);
        resolution.FromSelection.Should().BeTrue();
        resolution.AvailableToBuilds.Should().BeTrue();
        resolution.Installation!.NodeDirectory.Should().Be(instance.NodeDirectory, "the build uses the instance's own Node.js, so setup does too");
    }

    [Fact]
    public async Task FnmVersion_WithoutNodeInThisVisualStudio_IsUsableBySetup_ButNotByBuildsHere()
    {
        _h.Probe.InstallWithFnm("v22.15.0", isDefault: true);

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.Ready);
        resolution.Installation!.Source.Should().Be(NodeEnvironmentSource.Fnm);
        resolution.InstanceNode.Should().BeNull();
        resolution.AvailableToBuilds.Should().BeFalse();
        resolution.DescribeInstance().Should().Be("no Node.js on its PATH");
        var state = await _h.InspectAsync();
        state.Block.Should().BeNull();
        state.BuildBlock!.Code.Should().Be("XTPCF007");
    }

    [Fact]
    public async Task ExplicitSelection_ThatWasMovedOrDeleted_AsksAgainInsteadOfFallingBackSilently()
    {
        _h.OnBuildPath("v22.15.0");
        Select(@"D:\tools\gone");

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.SelectionUnavailable);
        resolution.Installation.Should().BeNull();
        resolution.Problem.Should().Contain(@"D:\tools\gone").And.Contain("no longer available");
    }

    [Fact]
    public async Task ExplicitSelection_ThatNoLongerFitsAChangedRequirement_IsReported()
    {
        Select(_h.Probe.Install(@"D:\tools\node20", "v20.11.0"));
        (await ResolveAsync()).Status.Should().Be(PcfEnvironmentStatus.Ready);

        _h.Project.Write(".nvmrc", "22");

        var resolution = await ResolveAsync();
        resolution.Status.Should().Be(PcfEnvironmentStatus.NodeIncompatible);
        resolution.Problem.Should().Contain("v20.11.0").And.Contain(".nvmrc").And.Contain("22");
    }

    [Fact]
    public async Task ExplicitSelection_WithoutNpm_IsNpmMissing()
    {
        Select(_h.Probe.Install(@"D:\tools\bare", "v22.15.0", npm: false));
        var resolution = await ResolveAsync();
        resolution.Status.Should().Be(PcfEnvironmentStatus.NpmMissing);
        resolution.Problem.Should().Contain("no npm");
    }

    [Fact]
    public async Task BuildEnvironmentThatDoesNotFitTheVersionFile_YieldsToTheVersionFnmResolves()
    {
        _h.OnBuildPath("v22.15.0");
        _h.Project.WriteAtRoot(".nvmrc", "24");
        var expected = _h.Probe.InstallWithFnm("v24.21.0", isDefault: false, Path.Combine(_h.Project.Root, ".nvmrc"));
        _h.Probe.InstallWithFnm("v22.15.0", isDefault: true);

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.Ready);
        resolution.Installation!.Source.Should().Be(NodeEnvironmentSource.Fnm);
        resolution.Installation.NodeDirectory.Should().Be(expected);
        _h.Probe.FnmRequests.Should().Equal(Path.Combine(_h.Project.Root, ".nvmrc"));
    }

    [Fact]
    public async Task VersionFileVersionThatIsNotInstalled_IsNeverReplacedByAnotherVersion()
    {
        _h.Project.Write(".node-version", "99.0.0");
        _h.Probe.InstallWithFnm("v22.15.0", isDefault: true);
        _h.Probe.InstallWithFnm("v24.21.0");

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.NodeIncompatible);
        resolution.Installation.Should().BeNull();
        resolution.Problem.Should().Contain("99.0.0").And.Contain("not installed");
        resolution.Remedy.Should().Contain("fnm install");
    }

    [Fact]
    public async Task VersionFileAlias_IsJudgedByFnm_ForTheBuildEnvironment()
    {
        _h.OnBuildPath("v22.15.0");
        _h.Project.Write(".nvmrc", "lts/*");
        _h.Probe.InstallWithFnm("v24.21.0", isDefault: false, Path.Combine(_h.Project.ProjectDirectory, ".nvmrc"));

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.Ready);
        resolution.Installation!.Version.ToString().Should().Be("v24.21.0", "fnm resolves the alias to a different version than Visual Studio's PATH offers");
    }

    [Fact]
    public async Task NoBuildEnvironment_NoDeclaration_UsesTheFnmDefault()
    {
        _h.Probe.InstallWithFnm("v24.21.0");
        var expected = _h.Probe.InstallWithFnm("v22.15.0", isDefault: true);

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.Ready);
        resolution.Installation!.NodeDirectory.Should().Be(expected);
        _h.Probe.FnmRequests.Should().Equal("default");
    }

    [Fact]
    public async Task NoBuildEnvironment_SeveralFnmVersionsAndNoDefault_IsAmbiguous()
    {
        _h.Probe.InstallWithFnm("v22.15.0");
        _h.Probe.InstallWithFnm("v24.21.0");

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.Ambiguous);
        resolution.Problem.Should().Contain("v22.15.0").And.Contain("v24.21.0").And.Contain("no default");
        _h.Probe.FnmRequests.Should().BeEmpty("no version is started when the choice is not clear");
    }

    [Fact]
    public async Task EnginesRange_PrefersFnmDefault_ThenASingleMatch_AndAsksWhenSeveralMatch()
    {
        _h.Project.Write("package.json", """{ "engines": { "node": ">=20 <23" } }""");
        _h.Probe.InstallWithFnm("v18.20.4", isDefault: true);
        var only = _h.Probe.InstallWithFnm("v22.15.0");
        _h.Probe.InstallWithFnm("v24.21.0");

        var single = await ResolveAsync();
        single.Status.Should().Be(PcfEnvironmentStatus.Ready);
        single.Installation!.NodeDirectory.Should().Be(only, "it is the only installed version inside the range");

        _h.Probe.InstallWithFnm("v20.11.0");
        var several = await ResolveAsync();
        several.Status.Should().Be(PcfEnvironmentStatus.Ambiguous);
        several.Problem.Should().Contain("v22.15.0").And.Contain("v20.11.0").And.NotContain("v24.21.0");
    }

    [Fact]
    public async Task EnginesRange_ThatNothingInstalledFits_IsIncompatible()
    {
        _h.OnBuildPath("v16.20.2");
        _h.Project.Write("package.json", """{ "engines": { "node": ">=20" } }""");
        _h.Probe.InstallWithFnm("v18.20.4", isDefault: true);

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.NodeIncompatible);
        resolution.Problem.Should().Contain("v16.20.2").And.Contain(">=20");
    }

    [Fact]
    public async Task DeclarationsThatDisagree_NeedAChoice_AndAnExplicitSelectionSettlesIt()
    {
        _h.OnBuildPath("v22.15.0");
        _h.Project.Write(".nvmrc", "18");
        _h.Project.Write("package.json", """{ "engines": { "node": ">=20" } }""");

        var undecided = await ResolveAsync();
        undecided.Status.Should().Be(PcfEnvironmentStatus.Ambiguous);
        undecided.Problem.Should().Contain("disagree").And.Contain(".nvmrc").And.Contain(">=20");

        Select(@"C:\nodejs");
        (await ResolveAsync()).Status.Should().Be(PcfEnvironmentStatus.Ready);
    }

    [Fact]
    public async Task NodeOnBuildPathWithoutNpm_IsNpmMissing_NotNodeMissing()
    {
        _h.OnBuildPath("v22.15.0", npm: false);
        var resolution = await ResolveAsync();
        resolution.Status.Should().Be(PcfEnvironmentStatus.NpmMissing);
        resolution.Problem.Should().Contain("v22.15.0").And.Contain("npm was not found");
    }

    [Fact]
    public async Task NodeInstalledAfterVisualStudioStarted_IsFoundOnTheWindowsPath()
    {
        _h.Probe.WindowsPath = _h.Probe.Install(@"C:\Program Files\nodejs", "v22.15.0");

        var resolution = await ResolveAsync();

        resolution.Status.Should().Be(PcfEnvironmentStatus.Ready);
        resolution.Installation!.Source.Should().Be(NodeEnvironmentSource.WindowsPath);
    }

    [Fact]
    public async Task NothingInstalled_IsNodeMissing_WithARemedy()
    {
        var resolution = await ResolveAsync();
        resolution.Status.Should().Be(PcfEnvironmentStatus.NodeMissing);
        resolution.Problem.Should().Contain("Node.js was not found");
        resolution.Remedy.Should().Contain("Install Node.js");
    }

    [Fact]
    public async Task Candidates_ListEachInstallationOnce()
    {
        _h.OnBuildPath("v22.15.0");
        _h.Probe.WindowsPath = _h.Probe.BuildPath;
        _h.Probe.InstallWithFnm("v24.21.0", isDefault: true);
        _h.Probe.InstallWithFnm("v20.11.0");

        var candidates = await _h.Service.GetCandidatesAsync(_h.Project.ProjectFile, CancellationToken.None);

        candidates.Should().HaveCount(3);
        candidates.Should().ContainSingle(candidate => candidate.Source == NodeEnvironmentSource.BuildEnvironment);
        candidates.Should().Contain(candidate => candidate.Label == "fnm default" && candidate.Version.ToString() == "v24.21.0");
    }
}
#nullable restore
