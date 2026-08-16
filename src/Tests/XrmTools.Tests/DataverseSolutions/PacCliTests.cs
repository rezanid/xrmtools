#nullable enable
namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools.DataverseSolutions;

public class PacCliTests
{
    [Fact]
    public async Task CreateAuthProfileAsync_ConstructsExpectedCommand_AndMasksSensitiveValues()
    {
        var runner = new CapturingProcessCommandRunner();
        var pacCli = new PacCli(runner);
    var tempDirectoryPath = Path.Combine(Path.GetTempPath(), "xrmtools-paccli-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(tempDirectoryPath);
    var fakePacPath = Path.Combine(tempDirectoryPath, "pac.cmd");
    File.WriteAllText(fakePacPath, "@echo off");
    var originalPath = Environment.GetEnvironmentVariable("PATH");

        try
        {
            Environment.SetEnvironmentVariable("PATH", tempDirectoryPath);

            await pacCli.CreateAuthProfileAsync(
                new PacAuthCreateRequest
                {
                    ProfileName = "XrmTools DEV",
                    EnvironmentUrl = "https://org.crm4.dynamics.com",
                    ApplicationId = "app-id",
                    ClientSecret = "super-secret",
                    TenantId = "tenant-id"
                },
                CancellationToken.None);

            runner.LastRequest.Should().NotBeNull();
            runner.LastRequest!.Arguments.Should().ContainInOrder("auth", "create", "--name", "XrmTools DEV");
            runner.LastRequest.Arguments.Should().Contain("--clientSecret");
            runner.LastRequest.Arguments.Should().Contain("super-secret");
            runner.LastRequest.SensitiveValues.Should().ContainSingle("super-secret");
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
            Directory.Delete(tempDirectoryPath, recursive: true);
        }
    }

    [Fact]
    public async Task SolutionProjectCommands_ConstructExpectedArguments_WithoutPackageType()
    {
        var runner = new CapturingProcessCommandRunner();
        var pacCli = new PacCli(runner);
        var tempDirectoryPath = Path.Combine(Path.GetTempPath(), "xrmtools-paccli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectoryPath);
        File.WriteAllText(Path.Combine(tempDirectoryPath, "pac.cmd"), "@echo off");
        var originalPath = Environment.GetEnvironmentVariable("PATH");

        try
        {
            Environment.SetEnvironmentVariable("PATH", tempDirectoryPath);
            var output = new Progress<ProcessOutputLine>();

            await pacCli.InitializeSolutionAsync(
                new PacSolutionInitRequest
                {
                    OutputDirectory = Path.Combine(tempDirectoryPath, "Empty"),
                    PublisherName = "Contoso",
                    PublisherPrefix = "cts"
                },
                output,
                CancellationToken.None);

            runner.LastRequest!.Arguments.Should().ContainInOrder(
                "solution", "init", "--publisher-name", "Contoso", "--publisher-prefix", "cts", "--outputDirectory");

            await pacCli.CloneSolutionAsync(
                new PacSolutionCloneRequest
                {
                    OutputDirectory = Path.Combine(tempDirectoryPath, "Clone"),
                    SolutionUniqueName = "ContosoCore",
                    EnvironmentUrl = "https://contoso.crm.dynamics.com"
                },
                output,
                CancellationToken.None);

            runner.LastRequest!.Arguments.Should().ContainInOrder(
                "solution", "clone", "--name", "ContosoCore", "--outputDirectory");
            runner.LastRequest.Arguments.Should().ContainInOrder(
                "--environment", "https://contoso.crm.dynamics.com");
            runner.LastRequest.Arguments.Should().NotContain("--packagetype");
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
            Directory.Delete(tempDirectoryPath, recursive: true);
        }
    }

    [Fact]
    public async Task CloneSolutionAsync_IncludesPacDiagnostic_WhenCommandFails()
    {
        var runner = new CapturingProcessCommandRunner(new ProcessCommandResult
        {
            ExitCode = 1,
            Output =
            [
                new ProcessOutputLine(ProcessOutputSource.StandardError, "Error: An error occurred while exporting a solution."),
                new ProcessOutputLine(ProcessOutputSource.StandardError, "Managed solutions cannot be exported.")
            ]
        });
        var pacCli = new PacCli(runner);
        var tempDirectoryPath = Path.Combine(Path.GetTempPath(), "xrmtools-paccli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectoryPath);
        File.WriteAllText(Path.Combine(tempDirectoryPath, "pac.cmd"), "@echo off");
        var originalPath = Environment.GetEnvironmentVariable("PATH");

        try
        {
            Environment.SetEnvironmentVariable("PATH", tempDirectoryPath);

            var action = async () => await pacCli.CloneSolutionAsync(
                new PacSolutionCloneRequest
                {
                    OutputDirectory = Path.Combine(tempDirectoryPath, "Clone"),
                    SolutionUniqueName = "ManagedSolution"
                },
                new Progress<ProcessOutputLine>(),
                CancellationToken.None);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Managed solutions cannot be exported.*");
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
            Directory.Delete(tempDirectoryPath, recursive: true);
        }
    }

    private sealed class CapturingProcessCommandRunner : IProcessCommandRunner
    {
        private readonly ProcessCommandResult _result;

        public CapturingProcessCommandRunner(ProcessCommandResult? result = null)
        {
            _result = result ?? new ProcessCommandResult { ExitCode = 0 };
        }

        public ProcessCommandRequest? LastRequest { get; private set; }

        public Task<ProcessCommandResult> RunAsync(ProcessCommandRequest request, IProgress<ProcessOutputLine> output, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_result);
        }
    }
}
#nullable restore
