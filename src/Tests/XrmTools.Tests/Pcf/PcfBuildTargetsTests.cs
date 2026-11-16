#nullable enable
namespace XrmTools.Tests.Pcf;

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using XrmTools.Pcf.Setup;

public sealed class PcfBuildTargetsTests : IDisposable
{
    private const string StockScripts = """{ "scripts": { "build": "pcf-scripts build", "clean": "pcf-scripts clean", "rebuild": "pcf-scripts rebuild" } }""";
    private static readonly PcfNodeRequirement NoRequirement = PcfNodeRequirement.Read(Path.Combine(Path.GetTempPath(), "xrmtools-no-such-project-" + Guid.NewGuid().ToString("N")));
    private readonly string _root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pcf-build-" + Guid.NewGuid().ToString("N"));

    public PcfBuildTargetsTests() => Directory.CreateDirectory(Path.Combine(_root, ".git"));

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private static NodeInstallation Node(string directory, string version, NodeEnvironmentSource source, string label)
    {
        NodeVersion.TryParse(version, out var parsed);
        return new NodeInstallation(directory, Path.Combine(directory, "npm.cmd"), parsed, "10.9.2", source, label);
    }

    private static NodeInstallation Instance(string version = "v22.15.0") => Node(@"C:\nodejs", version, NodeEnvironmentSource.BuildEnvironment, "Visual Studio's PATH");

    /// <param name="node">The Node.js resolved for the project; <c>null</c> when none was found.</param>
    /// <param name="instance">The Node.js on the PATH of the Visual Studio instance that builds.</param>
    private PcfSetupState State(string name, NodeInstallation? node, NodeInstallation? instance, string packageJson = StockScripts, bool incomplete = false)
    {
        var directory = Path.Combine(_root, name);
        Directory.CreateDirectory(Path.Combine(directory, "node_modules", "pcf-scripts"));
        File.WriteAllText(Path.Combine(directory, "package.json"), packageJson);
        File.WriteAllText(Path.Combine(directory, "node_modules", "pcf-scripts", "package.json"), "{}");
        return new PcfSetupState
        {
            ProjectFile = Path.Combine(directory, name + ".pcfproj"),
            ProjectDirectory = directory,
            PackageManager = PcfPackageManagerInfo.Detect(directory),
            Dependencies = PcfDependencyState.Read(directory, incomplete),
            Environment = node is null
                ? new PcfEnvironmentResolution { Status = PcfEnvironmentStatus.NodeMissing, Requirement = NoRequirement, InstanceNode = instance, Problem = "Node.js was not found.", Remedy = "Install Node.js." }
                : new PcfEnvironmentResolution { Status = PcfEnvironmentStatus.Ready, Requirement = NoRequirement, InstanceNode = instance, Installation = node }
        };
    }

    [Fact]
    public void ProjectThatFitsThisVisualStudio_Builds_AndTheBuildVerifiesTheVersionItWasCheckedAgainst()
    {
        var state = State("A", Instance(), Instance());

        state.BuildBlock.Should().BeNull();
        PcfBuildProperties.For(state).Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["XrmToolsPcfBuild"] = "true",
            ["XrmToolsPcfExpectedNode"] = "v22.15.0",
            ["XrmToolsPcfDependencyGateTargets"] = "PcfClean;PcfBuild;PcfRebuild"
        });
    }

    [Theory]
    [InlineData(true, "has Node.js v22.15.0 on its PATH")]
    [InlineData(false, "has no Node.js on its PATH")]
    public void ProjectThatNeedsANodeThisVisualStudioDoesNotHave_IsStopped_AndPointedToTheRestart(bool instanceHasNode, string instanceText)
    {
        var fnm = Node(@"C:\fnm\node-versions\v24.21.0\installation", "v24.21.0", NodeEnvironmentSource.Fnm, "fnm, from .nvmrc");
        var state = State("P24", fnm, instanceHasNode ? Instance() : null);

        state.Block.Should().BeNull("setup and other processes Xrm Tools starts itself can use that Node.js");
        var block = state.BuildBlock!;
        block.Code.Should().Be("XTPCF007");
        block.Summary.Should().Be("'P24' needs Visual Studio restarted with Node.js v24.21.0 to build.");
        block.Message.Should().StartWith("PCF project 'P24': this Visual Studio window " + instanceText)
            .And.Contain("Node.js v24.21.0, npm 10.9.2 (fnm, from .nvmrc)").And.Contain("restart Visual Studio").And.Contain("Set Up PCF Build");
        var properties = PcfBuildProperties.For(state);
        properties["XrmToolsPcfSetupErrorCode"].Should().Be("XTPCF007");
        properties["XrmToolsPcfSetupError"].Should().Be(block.Message);
        properties.Should().NotContainKey("XrmToolsPcfExpectedNode", "a stopped project is never told to build with any version");
    }

    [Fact]
    public void SameNodeVersionFromAnotherFolder_IsWhatThisVisualStudioAlreadyHas()
    {
        var selected = Node(@"C:\fnm\node-versions\v22.15.0\installation", "v22.15.0", NodeEnvironmentSource.Selected, "selected for this project");

        State("A", selected, Instance("v22.15.0")).BuildBlock.Should().BeNull();
        State("B", selected, Instance("v22.14.0")).BuildBlock!.Code.Should().Be("XTPCF007");
    }

    [Fact]
    public void EachProjectIsJudgedOnItsOwn_SoOneThatCannotBuildHereDoesNotStopAnother()
    {
        var needsOther = State("P24", Node(@"C:\n24", "v24.21.0", NodeEnvironmentSource.Fnm, "fnm"), Instance());
        var fits = State("P22", Instance(), Instance());

        PcfBuildProperties.For(needsOther).Should().ContainKey("XrmToolsPcfSetupError");
        PcfBuildProperties.For(fits).Should().NotContainKey("XrmToolsPcfSetupError").And.ContainKey("XrmToolsPcfExpectedNode");
    }

    [Fact]
    public void ProjectWithoutAnyUsableNode_IsStopped_ForBuildsAndForXrmToolsOwnProcesses()
    {
        var state = State("Broken", null, null);

        state.Block!.Code.Should().Be("XTPCF001");
        state.BuildBlock!.Message.Should().Be(state.Block.Message);
        var properties = PcfBuildProperties.For(state);
        properties["XrmToolsPcfSetupErrorCode"].Should().Be("XTPCF001");
        properties["XrmToolsPcfSetupError"].Should().StartWith("PCF project 'Broken': Node.js was not found.").And.Contain("Set Up PCF Build");
    }

    [Fact]
    public void OtherPackageManagerIsPassedToTheBuild_AndAnUnfinishedInstallStopsIt()
    {
        var pnpm = State("Pnpm", Instance(), Instance(), """{ "packageManager": "pnpm@9.12.0", "scripts": { "build": "pcf-scripts build" } }""");
        var interrupted = State("Interrupted", Instance(), Instance(), incomplete: true);

        PcfBuildProperties.For(pnpm)["XrmToolsPcfPackageManagerError"].Should().Contain("pnpm").And.Contain("no files were changed");
        var properties = PcfBuildProperties.For(interrupted);
        properties["XrmToolsPcfSetupErrorCode"].Should().Be("XTPCF009");
        properties["XrmToolsPcfSetupError"].Should().Contain("'Interrupted'").And.Contain("Repair dependencies");
        properties.Keys.Should().BeEquivalentTo(["XrmToolsPcfBuild", "XrmToolsPcfSetupErrorCode", "XrmToolsPcfSetupError"], "a stopped build is given nothing to install or repair with");
    }

    [Theory]
    [InlineData("""{ "scripts": { "build": "pcf-scripts build", "clean": "rimraf out", "rebuild": "npm run clean && pcf-scripts build" } }""", "PcfBuild;PcfRebuild")]
    [InlineData("""{ "scripts": { "build": "node tools/build.js", "clean": "node tools/clean.js" } }""", null)]
    [InlineData("""{ "scripts": "not an object" }""", null)]
    public void OnlyStepsWhoseNpmScriptUsesPcfScripts_AreGatedOnInstalledDependencies(string packageJson, string? expected)
    {
        var properties = PcfBuildProperties.For(State("Custom", Instance(), Instance(), packageJson));

        if (expected is null) properties.Should().NotContainKey("XrmToolsPcfDependencyGateTargets", "a project's own scripts run as they always did");
        else properties["XrmToolsPcfDependencyGateTargets"].Should().Be(expected);
    }

    [Fact]
    public void Session_OnlyRecordsThatABuildIsRunning()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        var session = new PcfBuildSession();

        session.IsBuilding.Should().BeFalse();
        session.BeginBuild();
        session.IsBuilding.Should().BeTrue();
        Environment.GetEnvironmentVariable("PATH").Should().Be(path, "a build never changes the process environment");
        session.EndBuild();
        session.IsBuilding.Should().BeFalse();
    }

    [Fact]
    public void Targets_AreInertWithoutVisualStudio_AndOnlyRunInFrontOfMicrosoftsTargets()
    {
        XNamespace none = XNamespace.None;
        var document = XDocument.Parse(PcfBuildTargets.Content);
        var targets = document.Root!.Elements(none + "Target").ToArray();

        targets.Should().NotBeEmpty();
        targets.Should().OnlyContain(target => ((string)target.Attribute("Condition")!).Contains("'$(XrmToolsPcfBuild)' == 'true'"),
            "a command-line build never sets that property");
        targets.Should().OnlyContain(target => target.Attribute("BeforeTargets") != null && target.Attribute("AfterTargets") == null && target.Attribute("DependsOnTargets") == null);
        targets.Select(target => (string)target.Attribute("Name")!).Should().OnlyContain(name => name.StartsWith("XrmToolsPcf"), "none of Microsoft's targets may be redefined");
        document.Descendants().Select(element => element.Name.LocalName).Should().NotContain(["UsingTask", "Import"]);
        // The only process the targets start is a version query; nothing sets or passes an environment.
        document.Descendants(none + "Exec").Select(exec => (string)exec.Attribute("Command")!).Should().Equal("node --version");
        document.Descendants(none + "Exec").Attributes("EnvironmentVariables").Should().BeEmpty();
        PcfBuildTargets.Content.Should().NotContain("PATH=").And.NotContain("SetEnv");
        // The only npm work is Microsoft's own install target, called earlier than Microsoft calls it.
        document.Descendants(none + "CallTarget").Select(call => (string)call.Attribute("Targets")!).Distinct().Should().Equal("RestoreNPM");
        // A build never removes anything: not node_modules, and not the record of an unfinished install.
        document.Descendants(none + "Delete").Should().BeEmpty();
        document.Descendants(none + "RemoveDir").Should().BeEmpty();
        PcfBuildTargets.Content.Should().NotContain("IncompleteMarker");
        // The dependency gate hooks only the steps the build names, so it cannot reach a project's own clean script.
        var gate = targets.Single(target => (string)target.Attribute("Name")! == "XrmToolsPcfDependencyGate");
        ((string)gate.Attribute("BeforeTargets")!).Should().Be("$(XrmToolsPcfDependencyGateTargets)");
    }

    [Fact]
    public void Targets_AreWrittenToTheProjectsExtensionsFolder_AndRewrittenOnlyWhenDifferent()
    {
        var directory = Path.Combine(_root, "targets");
        var project = Path.Combine(directory, "Sample.pcfproj");
        PcfBuildTargets.FileFor(project, null).Should().Be(Path.Combine(directory, "obj", "Sample.pcfproj.xrmtools.g.targets"));
        PcfBuildTargets.FileFor(project, @"..\artifacts\obj\Sample\").Should().Be(Path.GetFullPath(Path.Combine(directory, @"..\artifacts\obj\Sample\Sample.pcfproj.xrmtools.g.targets")));

        var file = PcfBuildTargets.FileFor(project, "obj\\");
        PcfBuildTargets.Ensure(file).Should().BeTrue();
        File.ReadAllText(file).Should().Be(PcfBuildTargets.Content);
        var written = File.GetLastWriteTimeUtc(file);
        File.SetLastWriteTimeUtc(file, written.AddDays(-1));
        PcfBuildTargets.Ensure(file).Should().BeTrue();
        File.GetLastWriteTimeUtc(file).Should().Be(written.AddDays(-1), "an unchanged file is left alone so it never looks like a new build input");

        File.WriteAllText(file, "<Project />");
        PcfBuildTargets.Ensure(file).Should().BeTrue();
        File.ReadAllText(file).Should().Be(PcfBuildTargets.Content);
    }
}
#nullable restore
