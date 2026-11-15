#nullable enable
namespace XrmTools.Tests.Pcf;

using FluentAssertions;
using System.IO;
using Xunit;
using XrmTools.Pcf.Setup;

public sealed class PcfProjectRequirementsTests
{
    private static NodeVersion Version(string text) { NodeVersion.TryParse(text, out var version).Should().BeTrue(); return version; }

    [Theory]
    [InlineData("v22.15.0", 22, 15, 0)]
    [InlineData("18.19.1", 18, 19, 1)]
    [InlineData(" v24.0.0-rc.1 ", 24, 0, 0)]
    public void NodeVersion_ParsesWhatNodeReports(string text, int major, int minor, int patch)
        => Version(text).Should().Be(new NodeVersion(major, minor, patch));

    [Theory]
    [InlineData("22")]
    [InlineData("lts/*")]
    [InlineData("")]
    [InlineData(null)]
    public void NodeVersion_RejectsAnythingElse(string? text) => NodeVersion.TryParse(text, out _).Should().BeFalse();

    [Theory]
    [InlineData("v22.15.0", ">=18", true)]
    [InlineData("v16.20.2", ">=18", false)]
    [InlineData("v22.15.0", ">= 18.0.0 < 23", true)]
    [InlineData("v24.1.0", ">=18 <23", false)]
    [InlineData("v20.11.0", "^20.10.0", true)]
    [InlineData("v21.0.0", "^20.10.0", false)]
    [InlineData("v20.9.0", "^20.10.0", false)]
    [InlineData("v18.19.5", "~18.19.0", true)]
    [InlineData("v18.20.0", "~18.19.0", false)]
    [InlineData("v18.20.0", "~18", true)]
    [InlineData("v20.1.0", "18.x || 20.x", true)]
    [InlineData("v19.1.0", "18.x || 20.x", false)]
    [InlineData("v22.15.0", "22", true)]
    [InlineData("v22.15.0", "22.14", false)]
    [InlineData("v22.15.0", "=22.15.0", true)]
    [InlineData("v22.15.0", "*", true)]
    [InlineData("v22.15.0", "", true)]
    [InlineData("v20.5.0", "18 - 20", true)]
    [InlineData("v21.0.0", "18 - 20", false)]
    [InlineData("v18.0.0", ">18", false)]
    [InlineData("v19.0.0", ">18", true)]
    [InlineData("v18.19.0", "<=18", true)]
    [InlineData("v19.0.0", "<=18", false)]
    [InlineData("v0.2.5", "^0.2.3", true)]
    [InlineData("v0.3.0", "^0.2.3", false)]
    public void Range_FollowsNpmSemantics(string version, string range, bool expected)
        => NodeVersionRange.Satisfies(Version(version), range).Should().Be(expected);

    [Theory]
    [InlineData("latest")]
    [InlineData(">=abc")]
    [InlineData("1.2.3.4")]
    public void Range_ThatIsNotUnderstood_IsNotTreatedAsAMismatch(string range)
        => NodeVersionRange.Satisfies(Version("v22.15.0"), range).Should().BeNull();

    [Theory]
    [InlineData("22", true)]
    [InlineData("v22.15", true)]
    [InlineData("22.15.0", true)]
    [InlineData("20", false)]
    [InlineData("22.14", false)]
    public void VersionFileSpec_MatchesByPrefix(string spec, bool expected)
        => NodeVersionRange.MatchesPartial(Version("v22.15.0"), spec).Should().Be(expected);

    [Theory]
    [InlineData("lts/*")]
    [InlineData("lts/jod")]
    [InlineData("node")]
    public void VersionFileAlias_CannotBeJudgedWithoutAVersionManager(string spec)
        => NodeVersionRange.MatchesPartial(Version("v22.15.0"), spec).Should().BeNull();

    [Fact]
    public void Requirement_ReadsNearestVersionFile_UpToTheRepositoryRoot()
    {
        using var project = new PcfProjectFixture();
        project.WriteAtRoot(".nvmrc", "# team default\nv20.11.0\n");
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(project.Root)!, ".node-version"), "16");
        try
        {
            var requirement = PcfNodeRequirement.Read(project.ProjectDirectory);
            requirement.VersionSpec.Should().Be("v20.11.0");
            requirement.VersionFilePath.Should().Be(Path.Combine(project.Root, ".nvmrc"));
            requirement.Check(Version("v20.11.0")).Should().Be(NodeCompatibility.Compatible);
            requirement.Check(Version("v22.15.0"), out var reason).Should().Be(NodeCompatibility.Incompatible);
            reason.Should().Contain(".nvmrc").And.Contain("v20.11.0");
        }
        finally { File.Delete(Path.Combine(Path.GetDirectoryName(project.Root)!, ".node-version")); }
    }

    [Fact]
    public void Requirement_PrefersNodeVersionFileInTheProject_AndReadsEngines()
    {
        using var project = new PcfProjectFixture();
        project.WriteAtRoot(".nvmrc", "18");
        project.Write(".node-version", "22");
        project.Write("package.json", """{ "engines": { "node": ">=20" } }""");
        var requirement = PcfNodeRequirement.Read(project.ProjectDirectory);
        requirement.VersionSpec.Should().Be("22");
        requirement.EnginesRange.Should().Be(">=20");
        requirement.Conflict.Should().BeNull();
        requirement.Describe().Should().Contain(".node-version: 22").And.Contain("engines.node: >=20");
    }

    [Fact]
    public void Requirement_NoDeclarations_AcceptsAnyVersion()
    {
        using var project = new PcfProjectFixture();
        var requirement = PcfNodeRequirement.Read(project.ProjectDirectory);
        requirement.HasAny.Should().BeFalse();
        requirement.Check(Version("v12.0.0")).Should().Be(NodeCompatibility.Compatible);
    }

    [Theory]
    [InlineData("18", ">=20", true)]
    [InlineData("18.19.0", "^20", true)]
    [InlineData("22", ">=20", false)]
    [InlineData("20", "^20.10.0", false)]
    [InlineData("lts/*", ">=20", false)]
    public void Requirement_ReportsDeclarationsThatCannotBothBeMet(string versionFile, string engines, bool conflict)
    {
        using var project = new PcfProjectFixture();
        project.Write(".nvmrc", versionFile);
        project.Write("package.json", $$"""{ "engines": { "node": "{{engines}}" } }""");
        var requirement = PcfNodeRequirement.Read(project.ProjectDirectory);
        (requirement.Conflict is not null).Should().Be(conflict);
        if (conflict) requirement.Conflict.Should().Contain(".nvmrc").And.Contain(engines);
    }

    [Fact]
    public void PackageManager_DefaultsToNpm_AndIgnoresWhatIsInstalledOnTheMachine()
    {
        using var project = new PcfProjectFixture();
        var manager = PcfPackageManagerInfo.Detect(project.ProjectDirectory);
        manager.Kind.Should().Be(PcfPackageManagerKind.Npm);
        manager.Evidence.Should().BeEmpty();
    }

    [Theory]
    [InlineData("pnpm@9.12.0", nameof(PcfPackageManagerKind.Other), "pnpm")]
    [InlineData("yarn@4.5.0+sha512.abc", nameof(PcfPackageManagerKind.Other), "Yarn")]
    [InlineData("npm@10.9.2", nameof(PcfPackageManagerKind.Npm), "npm")]
    public void PackageManager_HonorsExplicitDeclaration(string declared, string kind, string name)
    {
        using var project = new PcfProjectFixture();
        project.Write("package.json", $$"""{ "packageManager": "{{declared}}" }""");
        // A declaration outranks a stray lock file from another tool.
        project.Write("yarn.lock", "");
        var manager = PcfPackageManagerInfo.Detect(project.ProjectDirectory);
        manager.Kind.ToString().Should().Be(kind);
        manager.Name.Should().Be(name);
        manager.Evidence.Should().Contain(declared);
    }

    [Theory]
    [InlineData("yarn.lock", "Yarn")]
    [InlineData("pnpm-lock.yaml", "pnpm")]
    [InlineData("bun.lockb", "Bun")]
    public void PackageManager_ForeignLockFileAlone_IsAnotherPackageManager(string lockFile, string name)
    {
        using var project = new PcfProjectFixture();
        project.Write(lockFile, "");
        var manager = PcfPackageManagerInfo.Detect(project.ProjectDirectory);
        manager.Kind.Should().Be(PcfPackageManagerKind.Other);
        manager.Name.Should().Be(name);
        manager.Evidence.Should().Be(lockFile);
    }

    [Fact]
    public void PackageManager_TwoLockFiles_AreAConflict()
    {
        using var project = new PcfProjectFixture();
        project.Write("package-lock.json", "{}");
        project.Write("yarn.lock", "");
        var manager = PcfPackageManagerInfo.Detect(project.ProjectDirectory);
        manager.Kind.Should().Be(PcfPackageManagerKind.Conflict);
        manager.Evidence.Should().Contain("package-lock.json").And.Contain("yarn.lock");
    }

    [Fact]
    public void PackageManager_ProjectLockFileOutranksRepositoryRoot()
    {
        using var project = new PcfProjectFixture();
        project.WriteAtRoot("pnpm-lock.yaml", "");
        PcfPackageManagerInfo.Detect(project.ProjectDirectory).Kind.Should().Be(PcfPackageManagerKind.Other);
        project.Write("package-lock.json", "{}");
        PcfPackageManagerInfo.Detect(project.ProjectDirectory).Kind.Should().Be(PcfPackageManagerKind.Npm);
    }

    [Fact]
    public void Dependencies_ChooseCiOnlyWhenALockFileExists_AndNeverReportAnInterruptedInstallAsInstalled()
    {
        using var project = new PcfProjectFixture();
        var missing = PcfDependencyState.Read(project.ProjectDirectory, setupIncomplete: false);
        missing.Status.Should().Be(PcfDependencyStatus.Missing);
        missing.InstallCommand.Should().Be("install");

        project.Write("package-lock.json", "{}");
        project.InstallDependencies();
        PcfDependencyState.Read(project.ProjectDirectory, setupIncomplete: false).Status.Should().Be(PcfDependencyStatus.Installed);
        var interrupted = PcfDependencyState.Read(project.ProjectDirectory, setupIncomplete: true);
        interrupted.Status.Should().Be(PcfDependencyStatus.Incomplete);
        interrupted.InstallCommand.Should().Be("ci");
    }
}
#nullable restore
