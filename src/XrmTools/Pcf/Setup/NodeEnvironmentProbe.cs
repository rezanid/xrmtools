#nullable enable
namespace XrmTools.Pcf.Setup;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

internal enum NodeEnvironmentSource
{
    /// <summary>Chosen for this project in Set Up PCF Build.</summary>
    Selected,
    /// <summary>Found on the PATH that Visual Studio builds inherit.</summary>
    BuildEnvironment,
    /// <summary>On the registered Windows PATH but not in this Visual Studio's own, typically installed after it started.</summary>
    WindowsPath,
    Fnm
}

internal sealed class NodeInstallation
{
    public NodeInstallation(string nodeDirectory, string? npmLauncher, NodeVersion version, string? npmVersion, NodeEnvironmentSource source, string label)
    {
        NodeDirectory = nodeDirectory; NpmLauncher = npmLauncher; Version = version; NpmVersion = npmVersion; Source = source; Label = label;
    }

    /// <summary>The directory that contains <c>node.exe</c>; it is what a build needs first on PATH.</summary>
    public string NodeDirectory { get; }
    public string? NpmLauncher { get; }
    public NodeVersion Version { get; }
    public string? NpmVersion { get; }
    public NodeEnvironmentSource Source { get; }
    public string Label { get; }

    public IEnumerable<string> PathEntries
    {
        get
        {
            yield return NodeDirectory;
            var npm = Path.GetDirectoryName(NpmLauncher);
            if (!string.IsNullOrEmpty(npm) && !SameDirectory(npm!, NodeDirectory)) yield return npm!;
        }
    }

    public string Describe() => $"Node.js {Version}{(NpmVersion is null ? string.Empty : $", npm {NpmVersion}")} ({Label})";
    public NodeInstallation WithSource(NodeEnvironmentSource source, string label) => new(NodeDirectory, NpmLauncher, Version, NpmVersion, source, label);
    public bool IsSameEnvironment(NodeInstallation? other)
        => other is not null && SameDirectory(NodeDirectory, other.NodeDirectory) && string.Equals(NpmLauncher, other.NpmLauncher, StringComparison.OrdinalIgnoreCase);

    internal static bool SameDirectory(string left, string right)
        => string.Equals(left.TrimEnd('\\', '/'), right.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase);
}

internal sealed class FnmVersion(string name, bool isDefault)
{
    public string Name { get; } = name;
    public bool IsDefault { get; } = isDefault;
}

internal sealed class FnmResolution
{
    public NodeInstallation? Installation { get; init; }
    /// <summary>fnm understood the request, but that version is not installed.</summary>
    public bool NotInstalled { get; init; }
    public string? Error { get; init; }
}

/// <summary>Finds Node.js installations without changing any of them. Every probe runs outside the project directory.</summary>
internal interface INodeEnvironmentProbe
{
    /// <summary>The PATH that Visual Studio builds inherit, before any build-scoped activation.</summary>
    string BuildPath { get; }
    /// <summary>The PATH a newly started process would get from Windows.</summary>
    string WindowsPath { get; }
    string? FindNodeDirectory(string path);
    /// <param name="npmSearchPath">Where a bare <c>npm</c> would be found once this installation is in use.</param>
    Task<NodeInstallation?> InspectAsync(string nodeDirectory, string npmSearchPath, NodeEnvironmentSource source, string label, CancellationToken cancellationToken);
    Task<IReadOnlyList<FnmVersion>?> ListFnmVersionsAsync(CancellationToken cancellationToken);
    /// <param name="using">A version, an alias such as <c>default</c>, or the path of a version file.</param>
    Task<FnmResolution> ResolveFnmAsync(string @using, CancellationToken cancellationToken);
}

[Export(typeof(INodeEnvironmentProbe))]
[PartCreationPolicy(CreationPolicy.Shared)]
internal sealed class NodeEnvironmentProbe : INodeEnvironmentProbe
{
    private static readonly string[] NpmLaunchers = ["npm.com", "npm.exe", "npm.bat", "npm.cmd"];
    private readonly ConcurrentDictionary<string, string?> _versions = new(StringComparer.OrdinalIgnoreCase);
    private readonly PcfBuildSession _session;

    [ImportingConstructor]
    public NodeEnvironmentProbe(PcfBuildSession session) => _session = session;

    public string BuildPath => _session.BasePath;

    /// <summary>Replaces the registered Windows PATH for this process; lets tests model a machine without changing it.</summary>
    internal const string WindowsPathOverride = "XRMTOOLS_PCF_WINDOWS_PATH";

    public string WindowsPath
    {
        get
        {
            if (Environment.GetEnvironmentVariable(WindowsPathOverride) is { } overridden) return overridden;
            var machine = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? string.Empty;
            var user = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? string.Empty;
            return machine.TrimEnd(';') + ";" + user;
        }
    }

    public string? FindNodeDirectory(string path) => Path.GetDirectoryName(FindOnPath(path, "node.exe"));

    public async Task<NodeInstallation?> InspectAsync(string nodeDirectory, string npmSearchPath, NodeEnvironmentSource source, string label, CancellationToken cancellationToken)
    {
        var node = Path.Combine(nodeDirectory, "node.exe");
        if (!File.Exists(node)) return null;
        // Version managers can switch the version behind an unchanged launcher, so Node itself is asked every time.
        var output = await RunAsync(node, ["--version"], null, 10, cancellationToken).ConfigureAwait(false);
        if (output.ExitCode != 0 || !NodeVersion.TryParse(output.Text.Trim(), out var version)) return null;
        var npm = FindOnPath(npmSearchPath, NpmLaunchers);
        return new NodeInstallation(nodeDirectory, npm, version, npm is null ? null : await NpmVersionAsync(npm, nodeDirectory, version, cancellationToken).ConfigureAwait(false), source, label);
    }

    public async Task<IReadOnlyList<FnmVersion>?> ListFnmVersionsAsync(CancellationToken cancellationToken)
    {
        var fnm = FindFnm();
        if (fnm is null) return null;
        var output = await RunAsync(fnm, ["list"], null, 15, cancellationToken).ConfigureAwait(false);
        if (output.ExitCode != 0) return null;
        var versions = new List<FnmVersion>();
        foreach (var line in output.Text.Split('\n'))
        {
            var match = Regex.Match(line, @"\A\*?\s*(v\d+\.\d+\.\d+)\b(.*)\z");
            if (match.Success) versions.Add(new FnmVersion(match.Groups[1].Value, Regex.IsMatch(match.Groups[2].Value, @"\bdefault\b")));
        }
        return versions;
    }

    public async Task<FnmResolution> ResolveFnmAsync(string @using, CancellationToken cancellationToken)
    {
        var fnm = FindFnm();
        if (fnm is null) return new FnmResolution { Error = "fnm was not found." };
        // fnm selects the version for this one child process; nothing in the shell, profile or fnm configuration changes.
        var output = await RunAsync(fnm, ["exec", "--using", @using, "node", "-p", "process.execPath"], null, 20, cancellationToken).ConfigureAwait(false);
        var executable = output.Text.Split('\n').Select(line => line.Trim()).LastOrDefault(line => line.EndsWith("node.exe", StringComparison.OrdinalIgnoreCase));
        if (output.ExitCode != 0 || executable is null || !File.Exists(executable))
            return new FnmResolution { NotInstalled = output.Text.IndexOf("not currently installed", StringComparison.OrdinalIgnoreCase) >= 0, Error = output.Text.Trim() };
        var directory = Path.GetDirectoryName(executable)!;
        var label = File.Exists(@using) ? $"fnm, from {Path.GetFileName(@using)}" : $"fnm {@using}";
        var installation = await InspectAsync(directory, directory, NodeEnvironmentSource.Fnm, label, cancellationToken).ConfigureAwait(false);
        return installation is null ? new FnmResolution { Error = $"fnm selected '{executable}', but it could not be run." } : new FnmResolution { Installation = installation };
    }

    internal static string? FindOnPath(string path, params string[] names)
    {
        foreach (var entry in path.Split(Path.PathSeparator))
        {
            var directory = Environment.ExpandEnvironmentVariables(entry.Trim().Trim('"'));
            if (directory.Length == 0 || !Path.IsPathRooted(directory)) continue;
            foreach (var name in names)
            {
                try
                {
                    var candidate = Path.Combine(directory, name);
                    if (File.Exists(candidate)) return candidate;
                }
                catch (ArgumentException) { }
            }
        }
        return null;
    }

    private string? FindFnm()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var found = FindOnPath(BuildPath + Path.PathSeparator + WindowsPath, "fnm.exe");
        if (found is not null) return found;
        var candidates = new List<string>
        {
            Path.Combine(local, "Microsoft", "WinGet", "Links", "fnm.exe"),
            Path.Combine(profile, ".cargo", "bin", "fnm.exe"),
            Path.Combine(profile, "scoop", "shims", "fnm.exe"),
            Path.Combine(Environment.GetEnvironmentVariable("ChocolateyInstall") ?? @"C:\ProgramData\chocolatey", "bin", "fnm.exe")
        };
        try
        {
            var packages = Path.Combine(local, "Microsoft", "WinGet", "Packages");
            if (Directory.Exists(packages))
                candidates.AddRange(Directory.GetDirectories(packages, "Schniz.fnm_*").Select(directory => Path.Combine(directory, "fnm.exe")));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        return candidates.FirstOrDefault(File.Exists);
    }

    private async Task<string?> NpmVersionAsync(string launcher, string nodeDirectory, NodeVersion node, CancellationToken cancellationToken)
    {
        try
        {
            // Standard installations keep npm beside its launcher; reading its manifest avoids starting npm.
            var manifest = Path.Combine(Path.GetDirectoryName(launcher)!, "node_modules", "npm", "package.json");
            if (File.Exists(manifest) && (string?)JObject.Parse(File.ReadAllText(manifest))["version"] is { Length: > 0 } version) return version;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Newtonsoft.Json.JsonException) { }
        string key;
        try
        {
            var info = new FileInfo(launcher);
            key = $"{info.FullName}|{info.Length}|{info.LastWriteTimeUtc.Ticks}|{node}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return null; }
        if (_versions.TryGetValue(key, out var cached)) return cached;
        var output = await RunAsync(launcher, ["--version"], nodeDirectory, 30, cancellationToken).ConfigureAwait(false);
        var text = output.ExitCode == 0 ? output.Text.Split('\n').Select(line => line.Trim()).FirstOrDefault(line => Regex.IsMatch(line, @"\A\d+\.\d+\.\d+")) : null;
        // A failed probe is not cached: the installation may be repaired without its launcher changing.
        if (text is not null) _versions[key] = text;
        return text;
    }

    private async Task<(int ExitCode, string Text)> RunAsync(string executable, string[] arguments, string? pathPrefix, int timeoutSeconds, CancellationToken cancellationToken)
    {
        var batch = Path.GetExtension(executable).ToLowerInvariant() is ".cmd" or ".bat";
        var info = new ProcessStartInfo(batch ? Path.Combine(Environment.SystemDirectory, "cmd.exe") : executable)
        {
            // A neutral directory keeps project files such as .npmrc out of every probe.
            WorkingDirectory = Environment.SystemDirectory,
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
        };
        if (batch)
        {
            info.EnvironmentVariables["XRMTOOLS_PCF_TOOL"] = executable;
            info.Arguments = "/d /s /c \"\"%XRMTOOLS_PCF_TOOL%\" " + string.Join(" ", arguments) + "\"";
        }
        else info.Arguments = string.Join(" ", arguments.Select(Quote));
        info.EnvironmentVariables["PATH"] = (pathPrefix is null ? string.Empty : pathPrefix + Path.PathSeparator) + BuildPath;
        var text = new StringBuilder();
        using var process = new Process { StartInfo = info, EnableRaisingEvents = true };
        var exited = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        process.Exited += (_, _) => exited.TrySetResult(0);
        DataReceivedEventHandler collect = (_, e) => { if (e.Data is not null) lock (text) text.Append(e.Data).Append('\n'); };
        process.OutputDataReceived += collect; process.ErrorDataReceived += collect;
        try { if (!process.Start()) return (-1, string.Empty); }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException) { return (-1, ex.Message); }
        process.BeginOutputReadLine(); process.BeginErrorReadLine();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
        using (timeout.Token.Register(() => { try { if (!process.HasExited) process.Kill(); } catch (Exception ex) when (ex is InvalidOperationException or Win32Exception) { } exited.TrySetResult(0); }))
            await exited.Task.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (timeout.IsCancellationRequested) return (-1, "The command did not finish in time.");
        process.WaitForExit();
        lock (text) return (process.ExitCode, text.ToString());
    }

    private static string Quote(string argument) => argument.IndexOfAny([' ', '\t', '"']) < 0 ? argument : "\"" + argument.Replace("\"", "\\\"") + "\"";
}
#nullable restore
