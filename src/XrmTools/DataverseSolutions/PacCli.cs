#nullable enable
namespace XrmTools.DataverseSolutions;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[Export(typeof(IPacCli))]
[method: ImportingConstructor]
internal sealed class PacCli(IProcessCommandRunner processCommandRunner) : IPacCli
{
    private readonly IProcessCommandRunner _processCommandRunner = processCommandRunner;

    public async Task<PacVersionResult> GetVersionAsync(CancellationToken cancellationToken)
    {
        var output = new List<ProcessOutputLine>();
        await RunPacAsync(["help"], null, null, output, cancellationToken).ConfigureAwait(false);

        var rawText = string.Join(Environment.NewLine, output.Select(line => line.Text));
        var version = output
            .Select(line => line.Text)
            .FirstOrDefault(line => line.StartsWith("Version:", StringComparison.OrdinalIgnoreCase))?
            .Split([':'], 2)[1]
            .Trim();

        return new PacVersionResult
        {
            RawText = rawText,
            Version = version
        };
    }

    public async Task<IReadOnlyList<PacAuthProfile>> ListAuthProfilesAsync(CancellationToken cancellationToken)
    {
        var output = new List<ProcessOutputLine>();
        await RunPacAsync(["auth", "list"], null, null, output, cancellationToken).ConfigureAwait(false);
        return PacAuthListParser.Parse(string.Join(Environment.NewLine, output.Select(line => line.Text)));
    }

    public async Task SelectAuthProfileAsync(PacAuthProfile profile, CancellationToken cancellationToken)
    {
        if (profile is null) throw new ArgumentNullException(nameof(profile));

        await RunPacAsync(
            ["auth", "select", "--index", profile.Index.ToString(CultureInfo.InvariantCulture)],
            null,
            null,
            (IProgress<ProcessOutputLine>?)null,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task CreateAuthProfileAsync(PacAuthCreateRequest request, CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var arguments = new List<string>
        {
            "auth",
            "create",
            "--name",
            request.ProfileName,
            "--environment",
            request.EnvironmentUrl
        };

        var sensitiveValues = new List<string>();
        AddOptionalArgument(arguments, "--username", request.UserName);
        AddOptionalSensitiveArgument(arguments, sensitiveValues, "--password", request.Password);
        AddOptionalArgument(arguments, "--applicationId", request.ApplicationId);
        AddOptionalSensitiveArgument(arguments, sensitiveValues, "--clientSecret", request.ClientSecret);
        AddOptionalArgument(arguments, "--tenant", request.TenantId);
        AddOptionalArgument(arguments, "--certificateDiskPath", request.CertificateDiskPath);
        AddOptionalSensitiveArgument(arguments, sensitiveValues, "--certificatePassword", request.CertificatePassword);

        if (request.UseDeviceCode)
        {
            arguments.Add("--deviceCode");
        }

        await RunPacAsync(arguments, null, sensitiveValues, (IProgress<ProcessOutputLine>?)null, cancellationToken).ConfigureAwait(false);
    }

    public Task<ProcessCommandResult> InitializeSolutionAsync(
        PacSolutionInitRequest request,
        IProgress<ProcessOutputLine> output,
        CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (output is null) throw new ArgumentNullException(nameof(output));

        ValidateRequiredValue(request.OutputDirectory, nameof(request.OutputDirectory));
        ValidateRequiredValue(request.PublisherName, nameof(request.PublisherName));
        ValidateRequiredValue(request.PublisherPrefix, nameof(request.PublisherPrefix));

        return RunPacAsync(
            [
                "solution",
                "init",
                "--publisher-name",
                request.PublisherName,
                "--publisher-prefix",
                request.PublisherPrefix,
                "--outputDirectory",
                request.OutputDirectory
            ],
            GetWorkingDirectory(request.OutputDirectory),
            null,
            output,
            cancellationToken);
    }

    public Task<ProcessCommandResult> CloneSolutionAsync(
        PacSolutionCloneRequest request,
        IProgress<ProcessOutputLine> output,
        CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (output is null) throw new ArgumentNullException(nameof(output));

        ValidateRequiredValue(request.OutputDirectory, nameof(request.OutputDirectory));
        ValidateRequiredValue(request.SolutionUniqueName, nameof(request.SolutionUniqueName));

        var arguments = new List<string>
        {
            "solution",
            "clone",
            "--name",
            request.SolutionUniqueName,
            "--outputDirectory",
            request.OutputDirectory
        };
        AddOptionalArgument(arguments, "--environment", request.EnvironmentUrl);
        AddOptionalArgument(arguments, "--map", request.MapFilePath);

        return RunPacAsync(
            arguments,
            GetWorkingDirectory(request.OutputDirectory),
            null,
            output,
            cancellationToken);
    }

    public Task<ProcessCommandResult> RunSolutionCommandAsync(
        CdsProjectInfo project,
        IReadOnlyList<string> arguments,
        IProgress<ProcessOutputLine> output,
        CancellationToken cancellationToken)
    {
        if (project is null) throw new ArgumentNullException(nameof(project));
        if (arguments is null) throw new ArgumentNullException(nameof(arguments));
        if (output is null) throw new ArgumentNullException(nameof(output));

        return RunPacAsync(arguments, project.ProjectDirectory, null, output, cancellationToken);
    }

    internal ProcessCommandRequest CreatePacCommandRequest(
        IReadOnlyList<string> arguments,
        string? workingDirectory,
        IReadOnlyList<string>? sensitiveValues)
    {
        return new ProcessCommandRequest
        {
            FileName = FindPacExecutablePath(),
            Arguments = arguments,
            WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory) ? Environment.CurrentDirectory : workingDirectory,
            SensitiveValues = sensitiveValues
        };
    }

    private async Task<ProcessCommandResult> RunPacAsync(
        IReadOnlyList<string> arguments,
        string? workingDirectory,
        IReadOnlyList<string>? sensitiveValues,
        IList<ProcessOutputLine>? capturedOutput,
        CancellationToken cancellationToken)
    {
        var progress = new Progress<ProcessOutputLine>(line => capturedOutput?.Add(line));
        return await RunPacAsync(arguments, workingDirectory, sensitiveValues, progress, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ProcessCommandResult> RunPacAsync(
        IReadOnlyList<string> arguments,
        string? workingDirectory,
        IReadOnlyList<string>? sensitiveValues,
        IProgress<ProcessOutputLine>? output,
        CancellationToken cancellationToken)
    {
        ProcessCommandRequest request;
        try
        {
            request = CreatePacCommandRequest(arguments, workingDirectory, sensitiveValues);
        }
        catch (PacCliNotFoundException)
        {
            throw;
        }

        try
        {
            var result = await _processCommandRunner.RunAsync(
                request,
                output ?? new Progress<ProcessOutputLine>(_ => { }),
                cancellationToken).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var diagnostic = GetFailureDiagnostic(result.Output);
                var message = $"PAC command failed with exit code {result.ExitCode}: {ProcessCommandFormatting.FormatCommand(request)}";
                if (!string.IsNullOrWhiteSpace(diagnostic))
                {
                    message += $"{Environment.NewLine}{diagnostic}";
                }

                throw new InvalidOperationException(message);
            }

            return result;
        }
        catch (ProcessCommandStartException ex) when (string.Equals(ex.FileName, request.FileName, StringComparison.OrdinalIgnoreCase))
        {
            throw new PacCliNotFoundException();
        }
    }

    private static string? GetFailureDiagnostic(IReadOnlyList<ProcessOutputLine> output)
    {
        var errorLines = output
            .Where(line => line.Source == ProcessOutputSource.StandardError && !string.IsNullOrWhiteSpace(line.Text))
            .Select(line => line.Text)
            .ToArray();
        if (errorLines.Length > 0)
        {
            return string.Join(Environment.NewLine, errorLines.Skip(Math.Max(0, errorLines.Length - 5)));
        }

        return output
            .Where(line => !string.IsNullOrWhiteSpace(line.Text)
                && line.Text.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0)
            .Select(line => line.Text)
            .LastOrDefault();
    }

    private static void AddOptionalArgument(List<string> arguments, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        arguments.Add(name);
        arguments.Add(value);
    }

    private static void AddOptionalSensitiveArgument(List<string> arguments, List<string> sensitiveValues, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        arguments.Add(name);
        arguments.Add(value);
        sensitiveValues.Add(value);
    }

    private static string GetWorkingDirectory(string outputDirectory)
        => Path.GetDirectoryName(Path.GetFullPath(outputDirectory)) ?? Environment.CurrentDirectory;

    private static void ValidateRequiredValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }
    }

    private static string FindPacExecutablePath()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new PacCliNotFoundException();
        }

        var pathExtensions = (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
            .Split([';'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var directory in path.Split([Path.PathSeparator], StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmedDirectory = directory.Trim().Trim('"');
            foreach (var extension in pathExtensions)
            {
                var candidate = Path.Combine(trimmedDirectory, "pac" + extension.ToLowerInvariant());
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        throw new PacCliNotFoundException();
    }
}
#nullable restore
