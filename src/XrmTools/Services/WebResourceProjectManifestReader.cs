#nullable enable
namespace XrmTools.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.DataverseSolutions;
using XrmTools.WebApi.Types;

internal sealed class WebResourceProjectManifest(
    string projectFilePath,
    string outputFolder,
    string solutionUniqueName,
    string namePrefix,
    IReadOnlyList<WebResourceDefinition> resources)
{
    public string ProjectFilePath { get; } = projectFilePath;
    public string OutputFolder { get; } = outputFolder;
    public string SolutionUniqueName { get; } = solutionUniqueName;
    public string NamePrefix { get; } = namePrefix;
    public IReadOnlyList<WebResourceDefinition> Resources { get; } = resources;
}

internal sealed class WebResourceDefinition(
    string filePath,
    string name,
    string displayName,
    string? description,
    WebResourceType type)
{
    public string FilePath { get; } = filePath;
    public string Name { get; } = name;
    public string DisplayName { get; } = displayName;
    public string? Description { get; } = description;
    public WebResourceType Type { get; } = type;
}

internal interface IWebResourceProjectManifestReader
{
    Task<WebResourceProjectManifest> ReadAsync(
        string projectFilePath,
        string configurationName,
        CancellationToken cancellationToken = default);
}

[Export(typeof(IWebResourceProjectManifestReader))]
[method: ImportingConstructor]
internal sealed class WebResourceProjectManifestReader(
    IMsBuildProjectPropertyEvaluator evaluator) : IWebResourceProjectManifestReader
{
    private const string WebResourceItemName = "WebResource";

    public async Task<WebResourceProjectManifest> ReadAsync(
        string projectFilePath,
        string configurationName,
        CancellationToken cancellationToken = default)
    {
        var properties = await evaluator.EvaluateAsync(
            projectFilePath,
            configurationName,
            ["BuildOutputFolder", "DataverseSolutionUniqueName", "WebResourceNamePrefix"],
            cancellationToken).ConfigureAwait(false);

        var projectDirectory = Path.GetDirectoryName(projectFilePath)
            ?? throw new InvalidOperationException($"Could not determine the directory of '{projectFilePath}'.");
        var outputFolderValue = RequiredProperty(properties, "BuildOutputFolder");
        var outputFolder = Path.GetFullPath(Path.Combine(projectDirectory, outputFolderValue));
        var solutionUniqueName = RequiredProperty(properties, "DataverseSolutionUniqueName");
        var namePrefix = NormalizeName(RequiredProperty(properties, "WebResourceNamePrefix"));

        if (!Directory.Exists(outputFolder))
            throw new DirectoryNotFoundException($"The web-resource build output folder was not found: '{outputFolder}'.");

        var evaluatedItems = await evaluator.EvaluateItemsAsync(
            projectFilePath,
            configurationName,
            WebResourceItemName,
            cancellationToken).ConfigureAwait(false);

        var resources = evaluatedItems.Select(item => ToDefinition(item, projectDirectory, outputFolder, namePrefix)).ToArray();
        if (resources.Length == 0)
            throw new InvalidOperationException($"No supported web-resource files were found in '{outputFolder}'.");

        var duplicate = resources.GroupBy(resource => resource.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new InvalidOperationException($"More than one output maps to the web-resource name '{duplicate.Key}'.");

        return new WebResourceProjectManifest(
            projectFilePath,
            outputFolder,
            solutionUniqueName,
            namePrefix,
            resources);
    }

    private static WebResourceDefinition ToDefinition(
        MsBuildProjectItem item,
        string projectDirectory,
        string outputFolder,
        string namePrefix)
    {
        var filePath = item.Metadata.TryGetValue("FullPath", out var fullPath)
            && !string.IsNullOrWhiteSpace(fullPath)
                ? Path.GetFullPath(fullPath!)
                : Path.GetFullPath(Path.Combine(projectDirectory, item.Identity));
        EnsureInsideOutputFolder(filePath, outputFolder);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("An evaluated web-resource output file was not found.", filePath);

        var relativePath = filePath.Substring(AppendDirectorySeparator(outputFolder).Length)
            .Replace(Path.DirectorySeparatorChar, '/');
        var configuredName = GetMetadata(item, "Name");
        var name = NormalizeName(string.IsNullOrWhiteSpace(configuredName)
            ? namePrefix + relativePath
            : configuredName!);
        if (!name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Web resource '{name}' is outside the configured ownership prefix '{namePrefix}'.");
        if (name.Length > 256)
            throw new InvalidOperationException($"Web-resource name '{name}' exceeds Dataverse's 256-character limit.");

        var displayName = GetMetadata(item, "DisplayName");
        if (string.IsNullOrWhiteSpace(displayName)) displayName = name;
        var description = GetMetadata(item, "Description");
        var configuredType = GetMetadata(item, "WebResourceType");
        var type = string.IsNullOrWhiteSpace(configuredType)
            ? WebResourceTypes.FromExtension(Path.GetExtension(filePath))
            : int.TryParse(configuredType, out var parsedType) && WebResourceTypes.IsSupported(parsedType)
                ? (WebResourceType)parsedType
                : throw new InvalidOperationException($"'{configuredType}' is not a supported WebResourceType for '{name}'.");

        return new WebResourceDefinition(filePath, name, displayName!, description, type);
    }

    private static string RequiredProperty(IReadOnlyDictionary<string, string?> properties, string name)
        => properties.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value!
            : throw new InvalidOperationException($"The required MSBuild property '{name}' is not configured.");

    private static string? GetMetadata(MsBuildProjectItem item, string name)
        => item.Metadata.TryGetValue(name, out var value) ? value : null;

    private static string NormalizeName(string value) => value.Trim().Replace('\\', '/');

    private static void EnsureInsideOutputFolder(string filePath, string outputFolder)
    {
        var root = AppendDirectorySeparator(outputFolder);
        if (!filePath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Web-resource output '{filePath}' is outside '{outputFolder}'.");
    }

    private static string AppendDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? path
            : path + Path.DirectorySeparatorChar;
}

internal static class WebResourceTypes
{
    public static WebResourceType FromExtension(string extension) => extension.ToLowerInvariant() switch
    {
        ".html" or ".htm" => WebResourceType.Webpage,
        ".css" => WebResourceType.StyleSheet,
        ".js" => WebResourceType.Script,
        ".xml" => WebResourceType.Data,
        ".png" => WebResourceType.Png,
        ".jpg" or ".jpeg" => WebResourceType.Jpg,
        ".gif" => WebResourceType.Gif,
        ".xap" => WebResourceType.Silverlight,
        ".xsl" or ".xslt" => WebResourceType.Xsl,
        ".ico" => WebResourceType.Ico,
        ".svg" => WebResourceType.Svg,
        ".resx" => WebResourceType.Resx,
        _ => throw new InvalidOperationException($"'{extension}' is not a supported Dataverse web-resource extension.")
    };

    public static bool IsSupported(int type) => Enum.IsDefined(typeof(WebResourceType), type);
}
#nullable restore
