#nullable enable
namespace XrmTools.Tests.Pcf;

using FluentAssertions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools.Pcf.Setup;

public sealed class NodeEnvironmentProbeTests : IDisposable
{
    private readonly string _root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "node-probe-" + Guid.NewGuid().ToString("N"));
    private readonly NodeEnvironmentProbe _probe = new();

    public NodeEnvironmentProbeTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private string Folder(string name, params (string File, string Content)[] files)
    {
        var folder = Directory.CreateDirectory(Path.Combine(_root, name)).FullName;
        foreach (var (file, content) in files) File.WriteAllText(Path.Combine(folder, file), content);
        return folder;
    }

    [Fact]
    public void BuildPath_IsThisProcesssOwnPath()
        => _probe.BuildPath.Should().Be(Environment.GetEnvironmentVariable("PATH"));

    [Fact]
    public void FindNodeDirectory_TakesTheFirstFolderOnPathThatCmdWouldUseForNode()
    {
        // Microsoft's PCF targets run a bare "npm" and "node" through cmd.exe, which also accepts launcher scripts.
        var shims = Folder("shims", ("node.cmd", "@echo v20.11.1"));
        var installed = Folder("installed", ("node.exe", "not really node"));

        _probe.FindNodeDirectory($@"C:\no-such-folder;{shims};{installed}").Should().Be(shims);
        _probe.FindNodeDirectory($"{installed};{shims}").Should().Be(installed);
        _probe.FindNodeDirectory(@"C:\no-such-folder").Should().BeNull();
    }

    [Fact]
    public async Task Inspect_AsksALauncherScriptForItsVersion_LikeTheBuildWould()
    {
        var shims = Folder("shims", ("node.cmd", "@echo v20.11.1"), ("npm.cmd", "@echo 10.2.4"));

        var node = await _probe.InspectAsync(shims, shims, NodeEnvironmentSource.BuildEnvironment, "Visual Studio's PATH", CancellationToken.None);

        node!.Version.ToString().Should().Be("v20.11.1");
        node.NpmLauncher.Should().Be(Path.Combine(shims, "npm.cmd"));
        node.NpmVersion.Should().Be("10.2.4");
        node.PathWith(@"C:\Windows").Should().Be(shims + @";C:\Windows");
    }

    [Fact]
    public async Task Inspect_FolderWithoutNode_OrWithOneThatDoesNotAnswer_IsNotAnInstallation()
    {
        (await _probe.InspectAsync(Folder("empty"), _root, NodeEnvironmentSource.Selected, "selected", CancellationToken.None)).Should().BeNull();
        var broken = Folder("broken", ("node.cmd", "@echo this is not a version"));
        (await _probe.InspectAsync(broken, broken, NodeEnvironmentSource.Selected, "selected", CancellationToken.None)).Should().BeNull();
    }
}
#nullable restore
