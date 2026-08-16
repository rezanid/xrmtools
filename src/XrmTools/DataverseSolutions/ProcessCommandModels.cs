#nullable enable
namespace XrmTools.DataverseSolutions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public enum ProcessOutputSource
{
    StandardOutput,
    StandardError
}

public sealed class ProcessOutputLine
{
    public ProcessOutputLine(ProcessOutputSource source, string text)
    {
        Source = source;
        Text = text ?? string.Empty;
    }

    public ProcessOutputSource Source { get; }

    public string Text { get; }

    public bool IsError => Source == ProcessOutputSource.StandardError;
}

public sealed class ProcessCommandRequest
{
    public string FileName { get; init; } = string.Empty;

    public IReadOnlyList<string> Arguments { get; init; } = Array.Empty<string>();

    public string WorkingDirectory { get; init; } = string.Empty;

    public IReadOnlyList<string>? SensitiveValues { get; init; }
}

public sealed class ProcessCommandResult
{
    public int ExitCode { get; init; }

    public IReadOnlyList<ProcessOutputLine> Output { get; init; } = Array.Empty<ProcessOutputLine>();

    public bool Succeeded => ExitCode == 0;
}

public interface IProcessCommandRunner
{
    Task<ProcessCommandResult> RunAsync(
        ProcessCommandRequest request,
        IProgress<ProcessOutputLine> output,
        CancellationToken cancellationToken);
}

internal sealed class ProcessCommandStartException : InvalidOperationException
{
    public ProcessCommandStartException(string fileName, Exception innerException)
        : base($"Failed to start process '{fileName}'.", innerException)
    {
        FileName = fileName;
    }

    public string FileName { get; }
}

internal static class ProcessCommandFormatting
{
    private static readonly Regex AnsiEscapeCodePattern = new(@"\x1B\[[0-9;?]*[ -/]*[@-~]", RegexOptions.Compiled);

    public static string FormatCommand(ProcessCommandRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var builder = new StringBuilder(request.FileName ?? string.Empty);
        foreach (var argument in request.Arguments)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(MaskAndQuoteArgument(argument, request.SensitiveValues));
        }

        return builder.ToString();
    }

    public static string BuildArguments(IReadOnlyList<string> arguments)
    {
        if (arguments is null) throw new ArgumentNullException(nameof(arguments));

        var builder = new StringBuilder();
        foreach (var argument in arguments)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(QuoteArgument(argument));
        }

        return builder.ToString();
    }

    public static string SanitizeOutput(string text, IReadOnlyList<string>? sensitiveValues)
    {
        var sanitized = StripAnsiEscapeCodes(text ?? string.Empty);
        if (sensitiveValues is null)
        {
            return sanitized;
        }

        foreach (var sensitiveValue in sensitiveValues)
        {
            if (string.IsNullOrWhiteSpace(sensitiveValue))
            {
                continue;
            }

            sanitized = sanitized.Replace(sensitiveValue, "***");
        }

        return sanitized;
    }

    public static void ApplyArguments(object startInfo, IReadOnlyList<string> arguments)
    {
        if (startInfo is null) throw new ArgumentNullException(nameof(startInfo));
        if (arguments is null) throw new ArgumentNullException(nameof(arguments));

        var argumentListProperty = startInfo.GetType().GetProperty("ArgumentList");
        if (argumentListProperty?.GetValue(startInfo) is IList argumentList)
        {
            foreach (var argument in arguments)
            {
                argumentList.Add(argument);
            }

            return;
        }

        var argumentsProperty = startInfo.GetType().GetProperty("Arguments");
        argumentsProperty?.SetValue(startInfo, BuildArguments(arguments));
    }

    private static string StripAnsiEscapeCodes(string text) => AnsiEscapeCodePattern.Replace(text, string.Empty);

    private static string MaskAndQuoteArgument(string argument, IReadOnlyList<string>? sensitiveValues)
    {
        var value = argument ?? string.Empty;
        if (sensitiveValues is not null)
        {
            foreach (var sensitiveValue in sensitiveValues)
            {
                if (!string.IsNullOrWhiteSpace(sensitiveValue) && string.Equals(value, sensitiveValue, StringComparison.Ordinal))
                {
                    return QuoteArgument("***");
                }
            }
        }

        return QuoteArgument(value);
    }

    private static string QuoteArgument(string argument)
    {
        argument ??= string.Empty;
        if (argument.Length == 0)
        {
            return "\"\"";
        }

        if (argument.IndexOfAny([' ', '\t', '\n', '\v', '"']) < 0)
        {
            return argument;
        }

        var builder = new StringBuilder(argument.Length + 2);
        builder.Append('"');

        var backslashCount = 0;
        foreach (var character in argument)
        {
            if (character == '\\')
            {
                backslashCount++;
                continue;
            }

            if (character == '"')
            {
                builder.Append('\\', backslashCount * 2 + 1);
                builder.Append('"');
                backslashCount = 0;
                continue;
            }

            if (backslashCount > 0)
            {
                builder.Append('\\', backslashCount);
                backslashCount = 0;
            }

            builder.Append(character);
        }

        if (backslashCount > 0)
        {
            builder.Append('\\', backslashCount * 2);
        }

        builder.Append('"');
        return builder.ToString();
    }
}
#nullable restore
