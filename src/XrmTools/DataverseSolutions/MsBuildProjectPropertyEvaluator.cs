#nullable enable
namespace XrmTools.DataverseSolutions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

internal interface IMsBuildProjectPropertyEvaluator
{
    Task<IReadOnlyDictionary<string, string?>> EvaluateAsync(
        string projectFilePath,
        string configurationName,
        IReadOnlyCollection<string> propertyNames,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MsBuildProjectItem>> EvaluateItemsAsync(
        string projectFilePath,
        string configurationName,
        string itemName,
        CancellationToken cancellationToken = default);
}

internal sealed class MsBuildProjectItem(
    string identity,
    IReadOnlyDictionary<string, string?> metadata)
{
    public string Identity { get; } = identity;
    public IReadOnlyDictionary<string, string?> Metadata { get; } = metadata;
}

[Export(typeof(IMsBuildProjectPropertyEvaluator))]
[method: ImportingConstructor]
internal sealed class MsBuildProjectPropertyEvaluator(IProcessCommandRunner processCommandRunner) : IMsBuildProjectPropertyEvaluator
{
    private readonly IProcessCommandRunner _processCommandRunner = processCommandRunner;

    public async Task<IReadOnlyDictionary<string, string?>> EvaluateAsync(
        string projectFilePath,
        string configurationName,
        IReadOnlyCollection<string> propertyNames,
        CancellationToken cancellationToken = default)
    {
        var projectDirectory = Path.GetDirectoryName(projectFilePath)
            ?? throw new InvalidOperationException($"Could not determine the directory of '{projectFilePath}'.");
        var request = new ProcessCommandRequest
        {
            FileName = "dotnet",
            WorkingDirectory = projectDirectory,
            Arguments =
            [
                "msbuild",
                projectFilePath,
                "-nologo",
                "-verbosity:quiet",
                $"-getProperty:{string.Join(",", propertyNames)}",
                $"-property:Configuration={configurationName}"
            ]
        };

        var lines = new ConcurrentQueue<ProcessOutputLine>();
        var result = await _processCommandRunner.RunAsync(
            request,
            new CollectingProgress(lines),
            cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"MSBuild evaluation failed for '{projectFilePath}'.");
        }

        var json = string.Join(
            Environment.NewLine,
            lines.Where(line => line.Source == ProcessOutputSource.StandardOutput).Select(line => line.Text));
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException($"MSBuild evaluation produced no JSON output for '{projectFilePath}'.");
        }

        return ParsePropertiesJson(json);
    }

    public async Task<IReadOnlyList<MsBuildProjectItem>> EvaluateItemsAsync(
        string projectFilePath,
        string configurationName,
        string itemName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            throw new ArgumentException("An MSBuild item name is required.", nameof(itemName));

        var json = await EvaluateJsonAsync(
            projectFilePath,
            configurationName,
            $"-getItem:{itemName}",
            cancellationToken).ConfigureAwait(false);
        return ParseItemsJson(json, itemName);
    }

    private async Task<string> EvaluateJsonAsync(
        string projectFilePath,
        string configurationName,
        string queryArgument,
        CancellationToken cancellationToken)
    {
        var projectDirectory = Path.GetDirectoryName(projectFilePath)
            ?? throw new InvalidOperationException($"Could not determine the directory of '{projectFilePath}'.");
        var request = new ProcessCommandRequest
        {
            FileName = "dotnet",
            WorkingDirectory = projectDirectory,
            Arguments =
            [
                "msbuild",
                projectFilePath,
                "-nologo",
                "-verbosity:quiet",
                queryArgument,
                $"-property:Configuration={configurationName}"
            ]
        };

        var lines = new ConcurrentQueue<ProcessOutputLine>();
        var result = await _processCommandRunner.RunAsync(
            request,
            new CollectingProgress(lines),
            cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
            throw new InvalidOperationException($"MSBuild evaluation failed for '{projectFilePath}'.");

        var json = string.Join(
            Environment.NewLine,
            lines.Where(line => line.Source == ProcessOutputSource.StandardOutput).Select(line => line.Text));
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException($"MSBuild evaluation produced no JSON output for '{projectFilePath}'.");

        return json;
    }

    internal static IReadOnlyDictionary<string, string?> ParsePropertiesJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("Properties", out var propertiesElement))
        {
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        var dictionary = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in propertiesElement.EnumerateObject())
        {
            dictionary[property.Name] = property.Value.ValueKind == JsonValueKind.Null
                ? null
                : property.Value.GetString();
        }

        return dictionary;
    }

    internal static IReadOnlyList<MsBuildProjectItem> ParseItemsJson(string json, string itemName)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("Items", out var itemsElement)
            || !itemsElement.TryGetProperty(itemName, out var itemArray)
            || itemArray.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var items = new List<MsBuildProjectItem>();
        foreach (var item in itemArray.EnumerateArray())
        {
            if (!item.TryGetProperty("Identity", out var identityElement)) continue;
            var identity = identityElement.GetString();
            if (string.IsNullOrWhiteSpace(identity)) continue;

            var metadata = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in item.EnumerateObject())
            {
                if (string.Equals(property.Name, "Identity", StringComparison.OrdinalIgnoreCase)) continue;
                metadata[property.Name] = property.Value.ValueKind == JsonValueKind.Null
                    ? null
                    : property.Value.ToString();
            }
            items.Add(new MsBuildProjectItem(identity, metadata));
        }
        return items;
    }

    private sealed class CollectingProgress(ConcurrentQueue<ProcessOutputLine> lines) : IProgress<ProcessOutputLine>
    {
        private readonly ConcurrentQueue<ProcessOutputLine> _lines = lines;

        public void Report(ProcessOutputLine value)
        {
            _lines.Enqueue(value);
        }
    }
}
#nullable restore
