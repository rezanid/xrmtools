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

    private sealed class CapturingProcessCommandRunner : IProcessCommandRunner
    {
        public ProcessCommandRequest? LastRequest { get; private set; }

        public Task<ProcessCommandResult> RunAsync(ProcessCommandRequest request, IProgress<ProcessOutputLine> output, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new ProcessCommandResult { ExitCode = 0 });
        }
    }
}
#nullable restore
