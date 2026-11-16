namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools.DataverseSolutions;

public class ProcessCommandRunnerTests
{
    private static readonly string CmdExePath = Path.Combine(Environment.SystemDirectory, "cmd.exe");

    [Fact]
    public async Task RunAsync_CapturesStandardOutput_StandardError_AndExitCode()
    {
        var runner = new ProcessCommandRunner();
        var output = new List<ProcessOutputLine>();

        var result = await runner.RunAsync(
            new ProcessCommandRequest
            {
                FileName = CmdExePath,
                WorkingDirectory = Environment.CurrentDirectory,
                Arguments = ["/c", "echo stdout-line & echo stderr-line 1>&2 & exit /b 3"]
            },
            new Progress<ProcessOutputLine>(line => output.Add(line)),
            CancellationToken.None);

        result.ExitCode.Should().Be(3);
        result.Output.Should().Contain(line => line.Source == ProcessOutputSource.StandardOutput && line.Text.Contains("stdout-line"));
        result.Output.Should().Contain(line => line.Source == ProcessOutputSource.StandardError && line.Text.Contains("stderr-line"));
        output.Should().Contain(line => line.Source == ProcessOutputSource.StandardOutput && line.Text.Contains("stdout-line"));
        output.Should().Contain(line => line.Source == ProcessOutputSource.StandardError && line.Text.Contains("stderr-line"));
    }

    [Fact]
    public async Task RunAsync_GivesEnvironmentVariablesToTheChildProcessOnly()
    {
        var runner = new ProcessCommandRunner();
        var path = Environment.GetEnvironmentVariable("PATH");

        var result = await runner.RunAsync(
            new ProcessCommandRequest
            {
                FileName = CmdExePath,
                WorkingDirectory = Environment.CurrentDirectory,
                Arguments = ["/d", "/c", "echo %PATH%"],
                EnvironmentVariables = new Dictionary<string, string> { ["PATH"] = @"C:\xrmtools-selected-node;" + path }
            },
            new Progress<ProcessOutputLine>(_ => { }),
            CancellationToken.None);

        result.Output.Single(line => !line.IsError).Text.Should().StartWith(@"C:\xrmtools-selected-node;");
        Environment.GetEnvironmentVariable("PATH").Should().Be(path);
    }

    [Fact]
    public async Task RunAsync_MasksSensitiveOutput()
    {
        var runner = new ProcessCommandRunner();
        var output = new List<ProcessOutputLine>();

        await runner.RunAsync(
            new ProcessCommandRequest
            {
                FileName = CmdExePath,
                WorkingDirectory = Environment.CurrentDirectory,
                Arguments = ["/c", "echo secret-value & echo secret-value 1>&2"],
                SensitiveValues = ["secret-value"]
            },
            new Progress<ProcessOutputLine>(line => output.Add(line)),
            CancellationToken.None);

        output.Select(line => line.Text).Should().OnlyContain(text => text.Contains("***"));
    }

    [Fact]
    public async Task RunAsync_CancelsLongRunningProcess()
    {
        var runner = new ProcessCommandRunner();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        var action = async () => await runner.RunAsync(
            new ProcessCommandRequest
            {
                FileName = CmdExePath,
                WorkingDirectory = Environment.CurrentDirectory,
                Arguments = ["/c", "ping -n 6 127.0.0.1 >nul"]
            },
            new Progress<ProcessOutputLine>(_ => { }),
            cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }
}
