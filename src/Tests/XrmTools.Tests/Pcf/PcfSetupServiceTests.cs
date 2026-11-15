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

    private Task<PcfInstallResult> InstallAsync(NodeInstallation node, bool updateLockFile = false, CancellationToken token = default)
        => _h.Service.InstallAsync(_h.Project.ProjectFile, node, updateLockFile, new Lines(_output), token);

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
    public async Task Install_Failure_PreservesTheProject_IsNotReportedAsReady_AndRetrySucceeds()
    {
        var node = _h.OnBuildPath();
        _h.Npm.Behavior = (directory, _, _) =>
        {
            // npm had already unpacked some packages when the registry refused the rest.
            FakeNpmRunner.Succeed(directory);
            return Task.FromResult(FakeNpmRunner.Fail("npm error code E401"));
        };

        (await InstallAsync(node)).Outcome.Should().Be(PcfInstallOutcome.Failed);
        File.Exists(_h.Project.ProjectFile).Should().BeTrue();
        (await _h.InspectAsync()).Dependencies.Status.Should().Be(PcfDependencyStatus.Incomplete);

        _h.Npm.Behavior = null;
        (await InstallAsync(node)).Succeeded.Should().BeTrue();
        (await _h.InspectAsync()).Dependencies.Status.Should().Be(PcfDependencyStatus.Installed);
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

        _h.Npm.Behavior = null;
        (await InstallAsync(node)).Succeeded.Should().BeTrue();
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
