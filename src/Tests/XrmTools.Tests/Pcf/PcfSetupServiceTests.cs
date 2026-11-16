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
using XrmTools.Pcf.Setup;

public sealed class PcfSetupServiceTests : IDisposable
{
    private readonly PcfSetupHarness _h = new();
    private readonly List<string> _output = [];

    public void Dispose() => _h.Dispose();

    private Task<PcfInstallResult> InstallAsync(NodeInstallation node, bool updateLockFile = false, bool repair = false, CancellationToken token = default)
        => _h.Service.InstallAsync(_h.Project.ProjectFile, node, updateLockFile, repair, new Lines(_output), token);

    /// <summary>An npm run that unpacked some packages, pcf-scripts among them, and then stopped with an error.</summary>
    private void NpmStopsHalfway()
        => _h.Npm.Behavior = (directory, _, _) =>
        {
            FakeNpmRunner.Succeed(directory);
            File.WriteAllText(Path.Combine(directory, "node_modules", "half-unpacked.txt"), string.Empty);
            return Task.FromResult(FakeNpmRunner.Fail("npm error code E401"));
        };

    private static void Junction(string link, string target)
    {
        using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd.exe", $"/d /c mklink /J \"{link}\" \"{target}\"") { UseShellExecute = false, CreateNoWindow = true })!;
        process.WaitForExit();
        process.ExitCode.Should().Be(0, "the test needs a junction");
    }

    [Theory]
    [InlineData(false, "install")]
    [InlineData(true, "ci")]
    public async Task Install_UsesCiOnlyWhenALockFileExists(bool lockFile, string command)
    {
        var node = _h.OnBuildPath();
        if (lockFile) _h.Project.Write("package-lock.json", "{}");
        var changed = new List<string>();
        _h.Service.Changed += changed.Add;

        var result = await InstallAsync(node);

        result.Outcome.Should().Be(PcfInstallOutcome.Succeeded);
        _h.Npm.Commands.Should().Equal(command);
        _h.Npm.Nodes.Single().Should().BeSameAs(node);
        _output.First().Should().Contain($"npm {command}").And.Contain("v22.15.0");
        changed.Should().Equal(_h.Project.ProjectFile);
        (await _h.InspectAsync()).Dependencies.Status.Should().Be(PcfDependencyStatus.Installed);
    }

    [Fact]
    public async Task Install_MarksPcfScriptsCurrent_SoMicrosoftsIncrementalInstallDoesNotRepeatIt()
    {
        var node = _h.OnBuildPath();
        _h.Npm.Behavior = (directory, _, _) =>
        {
            FakeNpmRunner.Succeed(directory);
            // npm restores the publish timestamp of packaged files.
            File.SetLastWriteTimeUtc(Path.Combine(directory, "node_modules", "pcf-scripts", "package.json"), new DateTime(1985, 10, 26, 8, 15, 0, DateTimeKind.Utc));
            return Task.FromResult(new PcfNpmResult { ExitCode = 0 });
        };

        (await InstallAsync(node)).Succeeded.Should().BeTrue();

        File.GetLastWriteTimeUtc(Path.Combine(_h.Project.ProjectDirectory, "node_modules", "pcf-scripts", "package.json"))
            .Should().BeOnOrAfter(File.GetLastWriteTimeUtc(Path.Combine(_h.Project.ProjectDirectory, "package.json")));
    }

    [Fact]
    public async Task Install_LockFileMismatch_IsReportedWithoutRewritingTheLockFile_UntilTheDeveloperAsks()
    {
        var node = _h.OnBuildPath();
        _h.Project.Write("package-lock.json", "original");
        _h.Npm.Behavior = (directory, command, _) =>
        {
            if (command == "ci")
                return Task.FromResult(FakeNpmRunner.Fail("npm error code EUSAGE", "npm error",
                    "npm error `npm ci` can only install packages when your package.json and package-lock.json or npm-shrinkwrap.json are in sync. Please update your lock file with `npm install` before continuing."));
            File.WriteAllText(Path.Combine(directory, "package-lock.json"), "updated");
            FakeNpmRunner.Succeed(directory);
            return Task.FromResult(new PcfNpmResult { ExitCode = 0 });
        };

        var mismatch = await InstallAsync(node);

        mismatch.Outcome.Should().Be(PcfInstallOutcome.Failed);
        mismatch.FailureKind.Should().Be(PcfSetupFailureKind.LockFileMismatch);
        mismatch.Summary.Should().Contain("package-lock.json does not match package.json");
        mismatch.Remedy.Should().Contain("Update lock file");
        _h.Npm.Commands.Should().Equal("ci");
        File.ReadAllText(Path.Combine(_h.Project.ProjectDirectory, "package-lock.json")).Should().Be("original");

        (await InstallAsync(node, updateLockFile: true)).Succeeded.Should().BeTrue();
        _h.Npm.Commands.Should().Equal("ci", "install");
    }

    [Theory]
    [InlineData("npm error code E401", nameof(PcfSetupFailureKind.RegistryAuthentication), "rejected the request")]
    [InlineData("npm ERR! code E403", nameof(PcfSetupFailureKind.RegistryAuthentication), "rejected the request")]
    [InlineData("npm error code ENOTFOUND", nameof(PcfSetupFailureKind.Network), "could not reach the registry")]
    [InlineData("npm error code ETIMEDOUT", nameof(PcfSetupFailureKind.Network), "could not reach the registry")]
    [InlineData("npm error code E404", nameof(PcfSetupFailureKind.PackageNotFound), "not found")]
    [InlineData("npm error code EBADENGINE", nameof(PcfSetupFailureKind.EngineMismatch), "does not support")]
    [InlineData("npm error code EPERM", nameof(PcfSetupFailureKind.FileAccess), "could not change files")]
    public async Task Install_NamesTheCauseNpmReported(string line, string kind, string summary)
    {
        var node = _h.OnBuildPath();
        _h.Npm.Behavior = (_, _, _) => Task.FromResult(FakeNpmRunner.Fail("npm warn something unrelated", line, "npm error detail line", "npm error A complete log of this run can be found in: C:\\x\\debug-0.log"));

        var result = await InstallAsync(node);

        result.Outcome.Should().Be(PcfInstallOutcome.Failed);
        result.FailureKind.ToString().Should().Be(kind);
        result.Summary.Should().Contain(summary);
        result.Remedy.Should().NotBeNullOrWhiteSpace();
        result.Details.Should().Contain(line).And.NotContain(detail => detail.Contains("debug-0.log"));
    }

    [Fact]
    public async Task Install_UnrecognizedFailure_DoesNotGuessACause()
    {
        var node = _h.OnBuildPath();
        _h.Npm.Behavior = (_, _, _) => Task.FromResult(new PcfNpmResult { ExitCode = 7, Output = ["something went wrong near the network"] });

        var result = await InstallAsync(node);

        result.FailureKind.Should().Be(PcfSetupFailureKind.Other);
        result.Summary.Should().Be("npm install exited with code 7.");
        result.Summary.Should().NotContainAny("registry", "network", "authentication");
    }

    [Fact]
    public async Task UnfinishedInstall_PreservesTheProject_StopsBuilds_AndIsNotEndedByNpmInstallOnTopOfWhatWasLeft()
    {
        var node = _h.OnBuildPath();
        NpmStopsHalfway();

        (await InstallAsync(node)).Outcome.Should().Be(PcfInstallOutcome.Failed);

        File.Exists(_h.Project.ProjectFile).Should().BeTrue();
        var failed = await _h.InspectAsync();
        failed.Dependencies.Status.Should().Be(PcfDependencyStatus.Incomplete, "pcf-scripts being on disk does not make an unfinished install usable");
        failed.NeedsRepair.Should().BeTrue();
        failed.Block!.Code.Should().Be("XTPCF009");
        failed.Block.Message.Should().StartWith("PCF project 'Sample': an earlier npm install did not finish")
            .And.Contain("Set Up PCF Build").And.Contain("Repair dependencies");
        var properties = PcfBuildProperties.For(failed);
        properties["XrmToolsPcfSetupErrorCode"].Should().Be("XTPCF009");
        properties.Should().NotContainKey("XrmToolsPcfExpectedNode");

        // The project has no lock file, so a plain setup would be npm install on top of the leftovers.
        _h.Npm.Behavior = null;
        var plain = await InstallAsync(node);

        plain.Outcome.Should().Be(PcfInstallOutcome.Failed);
        plain.Remedy.Should().Contain("Repair dependencies").And.Contain("removes this project's node_modules folder");
        _h.Npm.Commands.Should().HaveCount(1, "npm was not run a second time");
        _h.Project.Exists(@"node_modules\half-unpacked.txt").Should().BeTrue("nothing is removed without the explicit repair");
        (await _h.InspectAsync()).NeedsRepair.Should().BeTrue();
    }

    [Fact]
    public async Task Repair_WithoutALockFile_RemovesOnlyThisProjectsNodeModules_ThenInstalls()
    {
        var node = _h.OnBuildPath();
        _h.Project.Write(".npmrc", "registry=https://example.test/");
        _h.Project.Write(@"FieldControl\index.ts", "export {};");
        _h.Project.Write(@"..\Other\node_modules\keep.txt", "another project's dependencies");
        NpmStopsHalfway();
        await InstallAsync(node);
        File.SetAttributes(Path.Combine(_h.Project.ProjectDirectory, "node_modules", "half-unpacked.txt"), FileAttributes.ReadOnly);
        _h.Npm.Behavior = null;
        _output.Clear();

        var repaired = await InstallAsync(node, repair: true);

        repaired.Outcome.Should().Be(PcfInstallOutcome.Succeeded);
        _output.Should().Contain(line => line.StartsWith("> remove ") && line.EndsWith(@"\controls\Sample\node_modules"));
        _h.Npm.Commands.Should().Equal("install", "install");
        _h.Project.Exists(@"node_modules\half-unpacked.txt").Should().BeFalse();
        foreach (var kept in new[] { "Sample.pcfproj", "package.json", ".npmrc", @"FieldControl\index.ts", @"..\Other\node_modules\keep.txt" })
            _h.Project.Exists(kept).Should().BeTrue(kept + " is not part of this project's node_modules");
        var state = await _h.InspectAsync();
        state.Dependencies.Status.Should().Be(PcfDependencyStatus.Installed);
        state.Block.Should().BeNull();
    }

    [Fact]
    public async Task Repair_WithALockFile_IsNpmCi_AndXrmToolsRemovesNothingItself()
    {
        var node = _h.OnBuildPath();
        _h.Project.Write("package-lock.json", "{}");
        NpmStopsHalfway();
        await InstallAsync(node);
        _h.Npm.Behavior = null;
        _output.Clear();

        (await InstallAsync(node, repair: true)).Succeeded.Should().BeTrue();

        _h.Npm.Commands.Should().Equal("ci", "ci");
        _output.Should().NotContain(line => line.StartsWith("> remove"));
        _h.Project.Exists(@"node_modules\half-unpacked.txt").Should().BeTrue("npm ci replaces node_modules itself; the fake npm here does not");
        File.ReadAllText(Path.Combine(_h.Project.ProjectDirectory, "package-lock.json")).Should().Be("{}");
        (await _h.InspectAsync()).NeedsRepair.Should().BeFalse();
    }

    [Fact]
    public async Task Repair_ThatFails_LeavesTheProjectStopped_UntilARepairSucceeds()
    {
        var node = _h.OnBuildPath();
        NpmStopsHalfway();
        await InstallAsync(node);

        // npm returns success but pcf-scripts is not there: the existing readiness check decides, not the exit code.
        _h.Npm.Behavior = (_, _, _) => Task.FromResult(new PcfNpmResult { ExitCode = 0 });
        (await InstallAsync(node, repair: true)).Outcome.Should().Be(PcfInstallOutcome.Failed);
        (await _h.InspectAsync()).Block!.Code.Should().Be("XTPCF009");

        _h.Npm.Behavior = null;
        (await InstallAsync(node, repair: true)).Succeeded.Should().BeTrue();
        (await _h.InspectAsync()).Block.Should().BeNull();
    }

    [Fact]
    public async Task Repair_NeverFollowsALink_AndRefusesANodeModulesThatIsOne()
    {
        var node = _h.OnBuildPath();
        var elsewhere = Path.Combine(_h.Project.Root, "elsewhere");
        Directory.CreateDirectory(elsewhere);
        File.WriteAllText(Path.Combine(elsewhere, "keep.txt"), "not in this project");
        var modules = Path.Combine(_h.Project.ProjectDirectory, "node_modules");
        var inner = Path.Combine(modules, "linked-package");
        try
        {
            // A link inside node_modules, as npm link or a workspace creates: the link goes, its target stays.
            Directory.CreateDirectory(modules);
            Junction(inner, elsewhere);
            _h.Store.SetSetupIncomplete(_h.Project.ProjectDirectory, true);

            (await InstallAsync(node, repair: true)).Succeeded.Should().BeTrue();

            Directory.Exists(inner).Should().BeFalse();
            File.Exists(Path.Combine(elsewhere, "keep.txt")).Should().BeTrue();

            // node_modules itself is a link: removing through it would empty a folder somewhere else.
            Directory.Delete(modules, recursive: true);
            Junction(modules, elsewhere);
            _h.Store.SetSetupIncomplete(_h.Project.ProjectDirectory, true);

            var refused = await InstallAsync(node, repair: true);

            refused.Outcome.Should().Be(PcfInstallOutcome.Failed);
            refused.Summary.Should().Contain("was not removed").And.Contain("link to another location");
            refused.Remedy.Should().Contain("Remove it yourself");
            _h.Npm.Commands.Should().HaveCount(1, "npm was not run for the refused repair");
            File.Exists(Path.Combine(elsewhere, "keep.txt")).Should().BeTrue();
            (await _h.InspectAsync()).NeedsRepair.Should().BeTrue();
        }
        finally
        {
            // Remove the links as links, so that cleaning up the fixture cannot reach through one.
            foreach (var link in new[] { inner, modules })
                if (Directory.Exists(link) && (File.GetAttributes(link) & FileAttributes.ReparsePoint) != 0) Directory.Delete(link);
        }
    }

    [Fact]
    public async Task Install_Canceled_ReleasesTheLock_LeavesNoReadyState_AndRetrySucceeds()
    {
        var node = _h.OnBuildPath();
        using var cancellation = new CancellationTokenSource();
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _h.Npm.Behavior = async (directory, _, token) =>
        {
            FakeNpmRunner.Succeed(directory);
            started.SetResult(true);
            await Task.Delay(Timeout.Infinite, token);
            return new PcfNpmResult();
        };

        var pending = InstallAsync(node, token: cancellation.Token);
        await started.Task;
        (await _h.InspectAsync()).SetupRunning.Should().BeTrue();
        cancellation.Cancel();
        var canceled = await pending;

        canceled.Outcome.Should().Be(PcfInstallOutcome.Canceled);
        canceled.Summary.Should().Contain("canceled");
        var state = await _h.InspectAsync();
        state.SetupRunning.Should().BeFalse("the lock must not outlive a canceled setup");
        state.Dependencies.Status.Should().Be(PcfDependencyStatus.Incomplete);
        state.Block!.Code.Should().Be("XTPCF009", "a canceled install stops builds until it is repaired");

        _h.Npm.Behavior = null;
        (await InstallAsync(node, repair: true)).Succeeded.Should().BeTrue();
        (await _h.InspectAsync()).Dependencies.Status.Should().Be(PcfDependencyStatus.Installed);
    }

    [Fact]
    public async Task Install_SecondRequestForTheSameProject_IsRefused_WhileTheFirstRuns()
    {
        var node = _h.OnBuildPath();
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _h.Npm.Behavior = async (directory, _, _) =>
        {
            started.SetResult(true);
            await release.Task;
            FakeNpmRunner.Succeed(directory);
            return new PcfNpmResult();
        };

        var first = InstallAsync(node);
        await started.Task;
        var second = await InstallAsync(node);
        // Another Visual Studio process shares the same per-user state folder and must be refused as well.
        var otherProcess = new PcfSetupStore(_h.Project.StateDirectory).TryLock(_h.Project.ProjectDirectory);
        release.SetResult(true);

        second.Outcome.Should().Be(PcfInstallOutcome.Failed);
        second.Summary.Should().Contain("already running");
        otherProcess.Should().BeNull();
        (await first).Succeeded.Should().BeTrue();
        _h.Npm.Commands.Should().HaveCount(1);
    }

    [Fact]
    public async Task Install_IsRefusedDuringABuild_AndABuildSeesARunningSetup()
    {
        var node = _h.OnBuildPath();

        _h.Session.BeginBuild();
        var duringBuild = await InstallAsync(node);
        _h.Session.EndBuild();

        duringBuild.Outcome.Should().Be(PcfInstallOutcome.Failed);
        duringBuild.Summary.Should().Contain("build is running");
        _h.Npm.Commands.Should().BeEmpty();
        (await _h.InspectAsync()).Dependencies.Status.Should().Be(PcfDependencyStatus.Missing, "a refused setup never touched the project");

        using (_h.Store.TryLock(_h.Project.ProjectDirectory))
        {
            var block = (await _h.InspectAsync()).Block;
            block!.Code.Should().Be("XTPCF006");
            block.Message.Should().Contain("'Sample'").And.Contain("Build again when setup has finished");
        }
        (await _h.InspectAsync()).Block.Should().BeNull();
    }

    [Theory]
    [InlineData("""{ "packageManager": "pnpm@9.12.0" }""", null, "pnpm")]
    [InlineData(null, "yarn.lock", "Yarn")]
    public async Task Install_NeverRunsNpm_InAProjectConfiguredForAnotherPackageManager(string? packageJson, string? lockFile, string name)
    {
        var node = _h.OnBuildPath();
        if (packageJson is not null) _h.Project.Write("package.json", packageJson);
        if (lockFile is not null) _h.Project.Write(lockFile, "");
        var before = Directory.GetFiles(_h.Project.ProjectDirectory).OrderBy(file => file).ToArray();

        var result = await InstallAsync(node);

        result.Outcome.Should().Be(PcfInstallOutcome.Failed);
        result.Summary.Should().Contain(name).And.Contain("npm was not run").And.Contain("no files were changed");
        _h.Npm.Commands.Should().BeEmpty();
        Directory.GetFiles(_h.Project.ProjectDirectory).OrderBy(file => file).Should().Equal(before);
        var state = await _h.InspectAsync();
        state.Block.Should().BeNull("the project can still build with dependencies installed by its own package manager");
        state.PackageManagerNotice.Should().Contain(name).And.Contain("PcfEnableAutoNpmInstall");
    }

    [Fact]
    public async Task Inspect_ConflictingLockFiles_BlockTheBuild_WithoutChangingFiles()
    {
        _h.OnBuildPath();
        _h.Project.Write("package-lock.json", "{}");
        _h.Project.Write("pnpm-lock.yaml", "");

        var block = (await _h.InspectAsync()).Block;

        block!.Code.Should().Be("XTPCF004");
        block.Message.Should().Contain("package-lock.json").And.Contain("pnpm-lock.yaml").And.Contain("No files were changed");
    }

    [Theory]
    [InlineData(false, true, "XTPCF001", "Node.js was not found")]
    [InlineData(true, false, "XTPCF003", "npm was not found")]
    public async Task Inspect_MissingNodeAndMissingNpm_AreDistinctBlocks_ThatNameTheProjectAndTheAction(bool node, bool npm, string code, string text)
    {
        if (node) _h.OnBuildPath(npm: npm);

        var block = (await _h.InspectAsync()).Block;

        block!.Code.Should().Be(code);
        block.Message.Should().StartWith("PCF project 'Sample': ").And.Contain(text).And.Contain("Set Up PCF Build");
    }

    [Fact]
    public async Task Select_PersistsPerProject_AndNullReturnsToAutomaticSelection()
    {
        _h.OnBuildPath("v22.15.0");
        var chosen = (await _h.Service.InspectDirectoryAsync(_h.Probe.Install(@"D:\tools\node24", "v24.21.0"), CancellationToken.None))!;

        _h.Service.Select(_h.Project.ProjectFile, chosen);
        // A new store instance stands in for the next Visual Studio session.
        new PcfSetupStore(_h.Project.StateDirectory).GetSelection(_h.Project.ProjectFile.ToUpperInvariant())!.NodeDirectory.Should().Be(@"D:\tools\node24");
        (await _h.InspectAsync()).Environment.Installation!.Version.ToString().Should().Be("v24.21.0");
        Directory.GetFiles(_h.Project.ProjectDirectory, "*", SearchOption.AllDirectories).Should().NotContain(file => file.Contains("node24") || file.EndsWith(".user"),
            "a machine-specific path is never written into the project");

        _h.Service.Select(_h.Project.ProjectFile, null);
        (await _h.InspectAsync()).Environment.Installation!.Source.Should().Be(NodeEnvironmentSource.BuildEnvironment);
    }

    private sealed class Lines(List<string> lines) : IProgress<string>
    {
        public void Report(string value) => lines.Add(value);
    }
}
#nullable restore
