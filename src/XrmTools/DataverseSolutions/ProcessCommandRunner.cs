#nullable enable
namespace XrmTools.DataverseSolutions;

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

[Export(typeof(IProcessCommandRunner))]
internal sealed class ProcessCommandRunner : IProcessCommandRunner
{
    public async Task<ProcessCommandResult> RunAsync(
        ProcessCommandRequest request,
        IProgress<ProcessOutputLine> output,
        CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (output is null) throw new ArgumentNullException(nameof(output));

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new ArgumentException("Process file name is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.WorkingDirectory))
        {
            throw new ArgumentException("Process working directory is required.", nameof(request));
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = request.FileName,
            WorkingDirectory = request.WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8,
            CreateNoWindow = true
        };

        ProcessCommandFormatting.ApplyArguments(startInfo, request.Arguments);

        using var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        var exitCodeSource = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        process.Exited += (_, _) => exitCodeSource.TrySetResult(process.ExitCode);

        try
        {
            if (!process.Start())
            {
                throw new ProcessCommandStartException(request.FileName, new InvalidOperationException("Process failed to start."));
            }
        }
        catch (Exception ex) when (ex is Win32Exception or FileNotFoundException or InvalidOperationException)
        {
            throw new ProcessCommandStartException(request.FileName, ex);
        }

        if (process.HasExited)
        {
            exitCodeSource.TrySetResult(process.ExitCode);
        }

        using var cancellationRegistration = cancellationToken.Register(() => TryKillProcessTree(process));

        var outputTask = ReadLinesAsync(process.StandardOutput, ProcessOutputSource.StandardOutput, request.SensitiveValues, output, cancellationToken);
        var errorTask = ReadLinesAsync(process.StandardError, ProcessOutputSource.StandardError, request.SensitiveValues, output, cancellationToken);

        var exitCode = await exitCodeSource.Task.ConfigureAwait(false);
        await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return new ProcessCommandResult
        {
            ExitCode = exitCode
        };
    }

    private static async Task ReadLinesAsync(
        StreamReader reader,
        ProcessOutputSource source,
        IReadOnlyList<string>? sensitiveValues,
        IProgress<ProcessOutputLine> output,
        CancellationToken cancellationToken)
    {
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (line is null)
            {
                break;
            }

            output.Report(new ProcessOutputLine(source, ProcessCommandFormatting.SanitizeOutput(line, sensitiveValues)));

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private static void TryKillProcessTree(Process process)
    {
        try
        {
            if (process.HasExited)
            {
                return;
            }
        }
        catch (InvalidOperationException)
        {
            return;
        }

        try
        {
            var killMethod = typeof(Process).GetMethod("Kill", [typeof(bool)]);
            if (killMethod is not null)
            {
                killMethod.Invoke(process, [true]);
                return;
            }

            process.Kill();
        }
        catch (Exception)
        {
            try
            {
                process.Kill();
            }
            catch (Exception)
            {
            }
        }
    }
}
#nullable restore
