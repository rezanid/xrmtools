#nullable enable
namespace XrmTools.Tests.Pcf;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Pcf.Setup;

/// <summary>A disposable PCF project folder inside a pretend repository root.</summary>
internal sealed class PcfProjectFixture : IDisposable
{
    public PcfProjectFixture()
    {
        Root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pcf-setup-test-" + Guid.NewGuid().ToString("N"));
        ProjectDirectory = Path.Combine(Root, "controls", "Sample");
        Directory.CreateDirectory(ProjectDirectory);
        Directory.CreateDirectory(Path.Combine(Root, ".git"));
        ProjectFile = Path.Combine(ProjectDirectory, "Sample.pcfproj");
        File.WriteAllText(ProjectFile, "<Project />");
        Write("package.json", """{ "name": "sample", "devDependencies": { "pcf-scripts": "^1" } }""");
    }

    public string Root { get; }
    public string ProjectDirectory { get; }
    public string ProjectFile { get; }
    public string StateDirectory => Path.Combine(Root, "state");

    public void Write(string relativePath, string content)
    {
        var file = Path.Combine(ProjectDirectory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, content);
    }

    public void WriteAtRoot(string name, string content) => File.WriteAllText(Path.Combine(Root, name), content);
    public bool Exists(string relativePath) => File.Exists(Path.Combine(ProjectDirectory, relativePath));
    public void InstallDependencies() => Write(Path.Combine("node_modules", "pcf-scripts", "package.json"), "{}");
    public void Dispose() => Directory.Delete(Root, recursive: true);
}

internal sealed class FakeNodeProbe : INodeEnvironmentProbe
{
    private readonly Dictionary<string, (NodeVersion Version, bool HasNpm)> _installations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _fnm = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<FnmVersion> _fnmVersions = [];

    public string BuildPath { get; set; } = string.Empty;
    public string WindowsPath { get; set; } = string.Empty;
    public bool HasFnm { get; set; }
    public List<string> FnmRequests { get; } = [];

    public string Install(string directory, string version, bool npm = true)
    {
        NodeVersion.TryParse(version, out var parsed);
        _installations[directory] = (parsed, npm);
        return directory;
    }

    /// <summary>Registers a version installed with fnm, optionally also reachable through a version file or alias.</summary>
    public string InstallWithFnm(string version, bool isDefault = false, params string[] aliases)
    {
        HasFnm = true;
        var directory = Install($@"C:\fnm\node-versions\{version}\installation", version);
        _fnmVersions.Add(new FnmVersion(version, isDefault));
        _fnm[version] = directory;
        if (isDefault) _fnm["default"] = directory;
        foreach (var alias in aliases) _fnm[alias] = directory;
        return directory;
    }

    public string? FindNodeDirectory(string path) => path.Split(';').FirstOrDefault(_installations.ContainsKey);

    public Task<NodeInstallation?> InspectAsync(string nodeDirectory, string npmSearchPath, NodeEnvironmentSource source, string label, CancellationToken cancellationToken)
    {
        if (!_installations.TryGetValue(nodeDirectory, out var node)) return Task.FromResult<NodeInstallation?>(null);
        var npm = npmSearchPath.Split(';').FirstOrDefault(directory => _installations.TryGetValue(directory, out var found) && found.HasNpm);
        return Task.FromResult<NodeInstallation?>(new NodeInstallation(nodeDirectory, npm is null ? null : Path.Combine(npm, "npm.cmd"), node.Version, npm is null ? null : "10.9.2", source, label));
    }

    public Task<IReadOnlyList<FnmVersion>?> ListFnmVersionsAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<FnmVersion>?>(HasFnm ? _fnmVersions : null);

    public async Task<FnmResolution> ResolveFnmAsync(string @using, CancellationToken cancellationToken)
    {
        FnmRequests.Add(@using);
        if (!HasFnm) return new FnmResolution { Error = "fnm was not found." };
        if (!_fnm.TryGetValue(@using, out var directory)) return new FnmResolution { NotInstalled = true, Error = "error: Requested version is not currently installed" };
        return new FnmResolution { Installation = await InspectAsync(directory, directory, NodeEnvironmentSource.Fnm, "fnm", cancellationToken) };
    }
}

internal sealed class FakeNpmRunner : IPcfNpmRunner
{
    public List<string> Commands { get; } = [];
    public List<NodeInstallation> Nodes { get; } = [];
    /// <summary>What each run does; the default creates pcf-scripts and succeeds.</summary>
    public Func<string, string, CancellationToken, Task<PcfNpmResult>>? Behavior { get; set; }

    public async Task<PcfNpmResult> RunAsync(NodeInstallation node, string projectDirectory, string command, IProgress<string> output, CancellationToken cancellationToken)
    {
        Commands.Add(command);
        Nodes.Add(node);
        output.Report($"npm {command} started");
        if (Behavior is not null) return await Behavior(projectDirectory, command, cancellationToken);
        Succeed(projectDirectory);
        return new PcfNpmResult { ExitCode = 0, Output = ["added 1 package"] };
    }

    public static void Succeed(string projectDirectory)
    {
        var scripts = Path.Combine(projectDirectory, "node_modules", "pcf-scripts");
        Directory.CreateDirectory(scripts);
        File.WriteAllText(Path.Combine(scripts, "package.json"), "{}");
    }

    public static PcfNpmResult Fail(params string[] lines) => new() { ExitCode = 1, Output = lines };
}

internal sealed class PcfSetupHarness : IDisposable
{
    public PcfSetupHarness()
    {
        Store = new PcfSetupStore(Project.StateDirectory);
        Session = new PcfBuildSession();
        Probe.BuildPath = @"C:\Windows\System32";
        Service = new PcfSetupService(Probe, Store, Npm, Session);
    }

    public PcfProjectFixture Project { get; } = new();
    public FakeNodeProbe Probe { get; } = new();
    public FakeNpmRunner Npm { get; } = new();
    public PcfSetupStore Store { get; }
    public PcfBuildSession Session { get; }
    public PcfSetupService Service { get; }

    /// <summary>Puts a Node.js installation on the PATH of the Visual Studio instance, which is what its builds use.</summary>
    public NodeInstallation OnBuildPath(string version = "v22.15.0", bool npm = true)
    {
        var directory = Probe.Install(@"C:\nodejs", version, npm);
        Probe.BuildPath = directory + ";" + Probe.BuildPath;
        return Probe.InspectAsync(directory, Probe.BuildPath, NodeEnvironmentSource.BuildEnvironment, "Visual Studio's PATH", CancellationToken.None).Result!;
    }

    public Task<PcfSetupState> InspectAsync() => Service.InspectAsync(Project.ProjectFile, CancellationToken.None);
    public void Dispose() => Project.Dispose();
}
#nullable restore
