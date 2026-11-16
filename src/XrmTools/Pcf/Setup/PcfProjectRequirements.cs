#nullable enable
namespace XrmTools.Pcf.Setup;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal enum NodeCompatibility { Compatible, Incompatible, Unverified }

/// <summary>The Node.js version a repository asks for through its version file and <c>engines.node</c>.</summary>
internal sealed class PcfNodeRequirement
{
    private static readonly string[] VersionFiles = [".node-version", ".nvmrc"];

    public string? VersionFilePath { get; private set; }
    public string? VersionSpec { get; private set; }
    public string? EnginesRange { get; private set; }
    public bool HasAny => VersionSpec is not null || EnginesRange is not null;

    /// <summary>Set when the version file and <c>engines.node</c> cannot both be satisfied.</summary>
    public string? Conflict { get; private set; }

    public static PcfNodeRequirement Read(string projectDirectory)
    {
        var requirement = new PcfNodeRequirement();
        // Version files commonly live at the repository root, above the PCF project.
        var directory = projectDirectory;
        for (var depth = 0; depth < 24 && !string.IsNullOrEmpty(directory) && requirement.VersionSpec is null; depth++)
        {
            foreach (var name in VersionFiles)
            {
                var file = Path.Combine(directory, name);
                var spec = ReadSpec(file);
                if (spec is null) continue;
                requirement.VersionFilePath = file;
                requirement.VersionSpec = spec;
                break;
            }
            if (Directory.Exists(Path.Combine(directory, ".git")) || File.Exists(Path.Combine(directory, ".git"))) break;
            directory = Path.GetDirectoryName(directory);
        }
        var engines = PcfPackageJson.Read(projectDirectory)?["engines"]?["node"];
        requirement.EnginesRange = engines?.Type == JTokenType.String ? ((string?)engines)?.Trim() : null;
        if (requirement.EnginesRange?.Length == 0) requirement.EnginesRange = null;
        requirement.Conflict = requirement.FindConflict();
        return requirement;
    }

    public NodeCompatibility Check(NodeVersion version) => Check(version, out _);

    public NodeCompatibility Check(NodeVersion version, out string? reason)
    {
        reason = null;
        var file = VersionSpec is null ? true : NodeVersionRange.MatchesPartial(version, VersionSpec);
        var engines = NodeVersionRange.Satisfies(version, EnginesRange);
        if (file == false) { reason = $"{DescribeVersionFile()} asks for Node.js {VersionSpec}"; return NodeCompatibility.Incompatible; }
        if (engines == false) { reason = $"package.json engines.node asks for {EnginesRange}"; return NodeCompatibility.Incompatible; }
        return file is null || engines is null ? NodeCompatibility.Unverified : NodeCompatibility.Compatible;
    }

    public string Describe()
    {
        var parts = new List<string>();
        if (VersionSpec is not null) parts.Add($"{DescribeVersionFile()}: {VersionSpec}");
        if (EnginesRange is not null) parts.Add($"package.json engines.node: {EnginesRange}");
        return parts.Count == 0 ? "No Node.js version is declared" : string.Join("; ", parts);
    }

    public string Fingerprint() => $"{VersionFilePath}|{VersionSpec}|{EnginesRange}";

    private string DescribeVersionFile() => VersionFilePath is null ? "the version file" : Path.GetFileName(VersionFilePath);

    private string? FindConflict()
    {
        if (VersionSpec is null || EnginesRange is null) return null;
        var text = VersionSpec.TrimStart('v', 'V');
        var parts = text.Split('.');
        if (parts.Length > 3 || !parts.All(part => part.Length is > 0 and < 10 && part.All(char.IsDigit))) return null;
        var numbers = parts.Select(int.Parse).ToArray();
        // A partial version such as 22 covers a range; it conflicts only when neither end fits.
        var lowest = new NodeVersion(numbers[0], numbers.ElementAtOrDefault(1), numbers.ElementAtOrDefault(2));
        var highest = new NodeVersion(numbers[0], numbers.Length > 1 ? numbers[1] : 999999, numbers.Length > 2 ? numbers[2] : 999999);
        return NodeVersionRange.Satisfies(lowest, EnginesRange) == false && NodeVersionRange.Satisfies(highest, EnginesRange) == false
            ? $"{DescribeVersionFile()} asks for Node.js {VersionSpec}, but package.json engines.node asks for {EnginesRange}"
            : null;
    }

    private static string? ReadSpec(string file)
    {
        try
        {
            if (!File.Exists(file)) return null;
            return File.ReadLines(file).Select(line => line.Split('#')[0].Trim()).FirstOrDefault(line => line.Length > 0);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return null; }
    }
}

internal static class PcfPackageJson
{
    public static JObject? Read(string directory)
    {
        try
        {
            var file = Path.Combine(directory, "package.json");
            return File.Exists(file) ? JObject.Parse(File.ReadAllText(file)) : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Newtonsoft.Json.JsonException) { return null; }
    }
}

internal enum PcfPackageManagerKind { Npm, Other, Conflict }

/// <summary>Which package manager a project is configured for. Only npm is prepared by Xrm Tools.</summary>
internal sealed class PcfPackageManagerInfo
{
    private static readonly (string File, string Manager)[] LockFiles =
    [
        ("package-lock.json", "npm"), ("npm-shrinkwrap.json", "npm"), ("yarn.lock", "Yarn"),
        ("pnpm-lock.yaml", "pnpm"), ("pnpm-workspace.yaml", "pnpm"), ("bun.lock", "Bun"), ("bun.lockb", "Bun")
    ];

    private PcfPackageManagerInfo(PcfPackageManagerKind kind, string name, string evidence) { Kind = kind; Name = name; Evidence = evidence; }

    public PcfPackageManagerKind Kind { get; }
    public string Name { get; }
    /// <summary>The file or declaration that decided <see cref="Kind"/>; empty when npm is only the default.</summary>
    public string Evidence { get; }
    public bool IsNpm => Kind == PcfPackageManagerKind.Npm;

    public static PcfPackageManagerInfo Detect(string projectDirectory)
    {
        // The nearest directory that says anything decides; a project's own lock file outranks its repository root.
        var directory = projectDirectory;
        for (var depth = 0; depth < 24 && !string.IsNullOrEmpty(directory); depth++)
        {
            var found = DetectIn(directory, isProjectDirectory: depth == 0);
            if (found is not null) return found;
            if (Directory.Exists(Path.Combine(directory, ".git")) || File.Exists(Path.Combine(directory, ".git"))) break;
            directory = Path.GetDirectoryName(directory);
        }
        return new PcfPackageManagerInfo(PcfPackageManagerKind.Npm, "npm", string.Empty);
    }

    private static PcfPackageManagerInfo? DetectIn(string directory, bool isProjectDirectory)
    {
        var declared = ((string?)PcfPackageJson.Read(directory)?["packageManager"])?.Trim();
        if (!string.IsNullOrEmpty(declared))
        {
            var name = declared!.Split('@')[0].Trim();
            var evidence = $"\"packageManager\": \"{declared}\" in {Path.Combine(directory, "package.json")}";
            return string.Equals(name, "npm", StringComparison.OrdinalIgnoreCase)
                ? new PcfPackageManagerInfo(PcfPackageManagerKind.Npm, "npm", evidence)
                : new PcfPackageManagerInfo(PcfPackageManagerKind.Other, DisplayName(name), evidence);
        }
        var present = LockFiles.Where(entry => File.Exists(Path.Combine(directory, entry.File))).ToArray();
        if (present.Length == 0) return null;
        var managers = present.Select(entry => entry.Manager).Distinct().ToArray();
        var files = string.Join(" and ", present.Select(entry => entry.File).Distinct());
        var location = isProjectDirectory ? files : $"{files} in {directory}";
        if (managers.Length > 1) return new PcfPackageManagerInfo(PcfPackageManagerKind.Conflict, string.Join(" and ", managers), location);
        return managers[0] == "npm"
            ? new PcfPackageManagerInfo(PcfPackageManagerKind.Npm, "npm", location)
            : new PcfPackageManagerInfo(PcfPackageManagerKind.Other, managers[0], location);
    }

    private static string DisplayName(string name) => name.ToLowerInvariant() switch
    {
        "yarn" => "Yarn", "pnpm" => "pnpm", "bun" => "Bun", _ => name
    };
}

internal enum PcfDependencyStatus { Installed, Missing, Incomplete }

internal sealed class PcfDependencyState
{
    private static readonly (string Script, string Target)[] Steps = [("clean", "PcfClean"), ("build", "PcfBuild"), ("rebuild", "PcfRebuild")];

    public PcfDependencyStatus Status { get; private set; }
    public bool HasLockFile { get; private set; }
    /// <summary><c>npm ci</c> reproduces an existing lock file; <c>npm install</c> is used only when there is none.</summary>
    public string InstallCommand => HasLockFile ? "ci" : "install";
    /// <summary>
    /// Microsoft's targets whose npm script runs <c>pcf-scripts</c> and therefore cannot work before dependencies are
    /// installed. A project's own script that does not use it, such as a custom clean, is left to run as it always did.
    /// </summary>
    public IReadOnlyList<string> TargetsThatNeedPcfScripts { get; private set; } = [];

    public static PcfDependencyState Read(string projectDirectory, bool setupIncomplete)
    {
        var installed = File.Exists(Path.Combine(projectDirectory, "node_modules", "pcf-scripts", "package.json"));
        var scripts = PcfPackageJson.Read(projectDirectory)?["scripts"] as JObject;
        return new PcfDependencyState
        {
            HasLockFile = File.Exists(Path.Combine(projectDirectory, "package-lock.json")) || File.Exists(Path.Combine(projectDirectory, "npm-shrinkwrap.json")),
            // An unfinished install outranks what is on disk: pcf-scripts can be present in a folder npm never completed.
            Status = setupIncomplete ? PcfDependencyStatus.Incomplete : installed ? PcfDependencyStatus.Installed : PcfDependencyStatus.Missing,
            TargetsThatNeedPcfScripts = Steps.Where(step => scripts?[step.Script] is JValue { Type: JTokenType.String } script
                    && ((string)script!).IndexOf("pcf-scripts", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(step => step.Target).ToArray()
        };
    }
}
#nullable restore
