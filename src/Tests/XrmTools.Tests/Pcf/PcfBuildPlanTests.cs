#nullable enable
namespace XrmTools.Tests.Pcf;

using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using XrmTools.Pcf.Setup;

public sealed class PcfBuildPlanTests : IDisposable
{
    private static readonly PcfNodeRequirement NoRequirement = PcfNodeRequirement.Read(Path.Combine(Path.GetTempPath(), "xrmtools-no-such-project-" + Guid.NewGuid().ToString("N")));
    private readonly string _root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pcf-plan-" + Guid.NewGuid().ToString("N"));

    public PcfBuildPlanTests() => Directory.CreateDirectory(Path.Combine(_root, ".git"));

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private static NodeInstallation Node(string directory, string version, NodeEnvironmentSource source)
    {
        NodeVersion.TryParse(version, out var parsed);
        return new NodeInstallation(directory, Path.Combine(directory, "npm.cmd"), parsed, "10.9.2", source, source.ToString());
    }

    private PcfSetupState State(string name, NodeInstallation? node, string packageJson = "{}", bool incomplete = false, bool installed = true)
    {
        var directory = Path.Combine(_root, name);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "package.json"), packageJson);
        if (installed)
        {
            Directory.CreateDirectory(Path.Combine(directory, "node_modules", "pcf-scripts"));
            File.WriteAllText(Path.Combine(directory, "node_modules", "pcf-scripts", "package.json"), "{}");
        }
        return new PcfSetupState
        {
            ProjectFile = Path.Combine(directory, name + ".pcfproj"),
            ProjectDirectory = directory,
            PackageManager = PcfPackageManagerInfo.Detect(directory),
            Dependencies = PcfDependencyState.Read(directory, incomplete),
            Environment = node is null
                ? new PcfEnvironmentResolution { Status = PcfEnvironmentStatus.NodeMissing, Requirement = NoRequirement, Problem = "Node.js was not found.", Remedy = "Install Node.js." }
                : new PcfEnvironmentResolution { Status = PcfEnvironmentStatus.Ready, Requirement = NoRequirement, Installation = node }
        };
    }

    [Fact]
    public void BuildEnvironmentForEveryProject_NeedsNoActivationAndNoVerification()
    {
        var ambient = Node(@"C:\nodejs", "v22.15.0", NodeEnvironmentSource.BuildEnvironment);
        var a = State("A", ambient);
        var plan = PcfBuildPlan.Create([a, State("B", ambient)]);

        plan.Activation.Should().BeEmpty();
        plan.PropertiesFor(a).Should().BeEquivalentTo(new System.Collections.Generic.Dictionary<string, string> { ["XrmToolsPcfBuild"] = "true" });
    }

    [Fact]
    public void SelectionOutsideVisualStudiosPath_IsActivatedForTheBuild_AndVerifiedInsideIt()
    {
        var fnm = Node(@"C:\fnm\aliases\default", "v22.15.0", NodeEnvironmentSource.Fnm);
        var a = State("A", fnm);
        var plan = PcfBuildPlan.Create([a]);

        plan.Activation.Should().Equal(@"C:\fnm\aliases\default");
        var properties = plan.PropertiesFor(a);
        properties["XrmToolsPcfExpectedNode"].Should().Be("v22.15.0");
        properties["XrmToolsPcfEnvironmentNote"].Should().Contain("'A'").And.Contain("v22.15.0").And.Contain(@"C:\fnm\aliases\default");
        properties.Should().NotContainKey("XrmToolsPcfSetupError");
    }

    [Fact]
    public void ProjectsThatNeedDifferentNodeVersions_GetOneEnvironment_AndTheOtherIsStoppedWithTheLimitation()
    {
        var node22 = Node(@"C:\fnm\node-versions\v22.15.0\installation", "v22.15.0", NodeEnvironmentSource.Fnm);
        var node24 = Node(@"C:\fnm\node-versions\v24.21.0\installation", "v24.21.0", NodeEnvironmentSource.Fnm);
        var a = State("A", node22);
        var b = State("B", node24);
        var c = State("C", node22);
        var plan = PcfBuildPlan.Create([b, a, c]);

        plan.Active.Should().BeSameAs(node22, "it is what most projects in the build need");
        plan.BlockFor(a).Should().BeNull();
        var block = plan.BlockFor(b)!;
        block.Code.Should().Be("XTPCF007");
        block.Message.Should().Contain("'B'").And.Contain("v24.21.0").And.Contain("v22.15.0").And.Contain("'A'").And.Contain("'C'")
            .And.Contain("only one Node.js installation").And.Contain("Set Up PCF Build");
        var properties = plan.PropertiesFor(b);
        properties["XrmToolsPcfSetupErrorCode"].Should().Be("XTPCF007");
        properties.Should().NotContainKey("XrmToolsPcfExpectedNode", "a stopped project is never told to build with any version");
    }

    [Fact]
    public void AWorkingBuildEnvironment_IsKept_EvenWhenMoreProjectsNeedSomethingElse()
    {
        var ambient = Node(@"C:\nodejs", "v22.15.0", NodeEnvironmentSource.BuildEnvironment);
        var node24 = Node(@"C:\fnm\node-versions\v24.21.0\installation", "v24.21.0", NodeEnvironmentSource.Fnm);
        var plan = PcfBuildPlan.Create([State("A", node24), State("B", node24), State("C", ambient)]);

        plan.Active.Should().BeSameAs(ambient);
        plan.Activation.Should().BeEmpty();
        plan.States.Count(state => plan.BlockFor(state) is not null).Should().Be(2);
    }

    [Fact]
    public void TiedGroups_AreDecidedTheSameWayOnEveryBuild()
    {
        var node22 = Node(@"C:\n22", "v22.15.0", NodeEnvironmentSource.Fnm);
        var node24 = Node(@"C:\n24", "v24.21.0", NodeEnvironmentSource.Fnm);
        var a = State("A", node24);
        var b = State("B", node22);
        var expected = string.Compare(a.ProjectFile, b.ProjectFile, StringComparison.OrdinalIgnoreCase) < 0 ? node24 : node22;

        PcfBuildPlan.Create([a, b]).Active.Should().BeSameAs(expected);
        PcfBuildPlan.Create([b, a]).Active.Should().BeSameAs(expected);
    }

    [Fact]
    public void ProjectThatNeedsSetup_IsStopped_WhileReadyProjectsStillBuild()
    {
        var ambient = Node(@"C:\nodejs", "v22.15.0", NodeEnvironmentSource.BuildEnvironment);
        var ready = State("Ready", ambient);
        var broken = State("Broken", null);
        var plan = PcfBuildPlan.Create([ready, broken]);

        plan.BlockFor(ready).Should().BeNull();
        var properties = plan.PropertiesFor(broken);
        properties["XrmToolsPcfSetupErrorCode"].Should().Be("XTPCF001");
        properties["XrmToolsPcfSetupError"].Should().StartWith("PCF project 'Broken': Node.js was not found.").And.Contain("Set Up PCF Build");
    }

    [Fact]
    public void NoReadyProject_ActivatesNothing()
    {
        var plan = PcfBuildPlan.Create([State("Broken", null)]);
        plan.Active.Should().BeNull();
        plan.Activation.Should().BeEmpty();
    }

    [Fact]
    public void OtherPackageManagerAndInterruptedSetup_ArePassedToTheBuild()
    {
        var ambient = Node(@"C:\nodejs", "v22.15.0", NodeEnvironmentSource.BuildEnvironment);
        var pnpm = State("Pnpm", ambient, """{ "packageManager": "pnpm@9.12.0" }""");
        var interrupted = State("Interrupted", ambient, incomplete: true);
        var plan = PcfBuildPlan.Create([pnpm, interrupted]);

        plan.PropertiesFor(pnpm)["XrmToolsPcfPackageManagerError"].Should().Contain("pnpm").And.Contain("no files were changed");
        plan.PropertiesFor(pnpm).Should().NotContainKey("XrmToolsPcfRestoreRequired");
        plan.PropertiesFor(interrupted)["XrmToolsPcfRestoreRequired"].Should().Be("true");
    }

    [Fact]
    public void Session_ActivatesOnlyForTheBuild_AndRestoresTheOriginalPath()
    {
        var path = @"C:\Windows\System32";
        var session = new PcfBuildSession(() => path, value => path = value);

        session.BeginBuild();
        session.IsBuilding.Should().BeTrue();
        session.Activate([@"C:\fnm\node", @"C:\fnm\npm"]);
        path.Should().Be(@"C:\fnm\node;C:\fnm\npm;C:\Windows\System32");
        session.BasePath.Should().Be(@"C:\Windows\System32", "probes during a build must see what Visual Studio started with");

        session.EndBuild();
        path.Should().Be(@"C:\Windows\System32");
        session.IsBuilding.Should().BeFalse();
    }

    [Fact]
    public void Session_DoesNotOverwriteAPathSomethingElseChangedDuringTheBuild()
    {
        var path = "base";
        var session = new PcfBuildSession(() => path, value => path = value);
        session.BeginBuild();
        session.Activate([@"C:\node"]);
        path = @"C:\other;" + path;

        session.EndBuild();

        path.Should().Be(@"C:\other;C:\node;base");
    }

    [Fact]
    public void Session_WithoutActivation_LeavesPathUntouched()
    {
        var writes = 0;
        var session = new PcfBuildSession(() => "base", _ => writes++);
        session.BeginBuild();
        session.Activate([]);
        session.EndBuild();
        writes.Should().Be(0);
    }

    [Fact]
    public void Targets_AreInertWithoutVisualStudio_AndOnlyRunInFrontOfMicrosoftsTargets()
    {
        XNamespace none = XNamespace.None;
        var targets = XDocument.Parse(PcfBuildTargets.Content).Root!.Elements(none + "Target").ToArray();

        targets.Should().NotBeEmpty();
        targets.Should().OnlyContain(target => ((string)target.Attribute("Condition")!).Contains("'$(XrmToolsPcfBuild)' == 'true'"),
            "a command-line build never sets that property");
        targets.Should().OnlyContain(target => target.Attribute("BeforeTargets") != null && target.Attribute("AfterTargets") == null && target.Attribute("DependsOnTargets") == null);
        var names = targets.Select(target => (string)target.Attribute("Name")!).ToArray();
        names.Should().OnlyContain(name => name.StartsWith("XrmToolsPcf"), "none of Microsoft's targets may be redefined");
        XDocument.Parse(PcfBuildTargets.Content).Descendants().Select(element => element.Name.LocalName).Should().NotContain(["UsingTask", "Import"]);
        // The only npm work is Microsoft's own install target, called earlier than Microsoft calls it.
        XDocument.Parse(PcfBuildTargets.Content).Descendants(none + "Exec").Select(exec => (string)exec.Attribute("Command")!).Should().Equal("node --version");
        XDocument.Parse(PcfBuildTargets.Content).Descendants(none + "CallTarget").Select(call => (string)call.Attribute("Targets")!).Distinct().Should().Equal("RestoreNPM");
    }

    [Fact]
    public void Targets_AreWrittenToTheProjectsExtensionsFolder_AndRewrittenOnlyWhenDifferent()
    {
        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pcf-targets-" + Guid.NewGuid().ToString("N"));
        try
        {
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
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }
}
#nullable restore
