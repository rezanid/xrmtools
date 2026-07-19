#nullable enable
namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using System;
using System.IO;
using Xunit;
using XrmTools.DataverseSolutions;
using XrmTools.Options;

public sealed class DataverseSolutionProjectFileServiceTests
{
    [Fact]
    public void GeneralOptions_DefaultsToAlbanianXrmProjectSdk()
    {
        var options = new GeneralOptions();

        options.DataverseSolutionProjectSdk.Should().Be(DataverseSolutionProjectSdk.AlbanianXrm);
    }

    [Fact]
    public void ReadUniqueName_ReadsNamespacedSolutionManifest()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var otherDirectory = Path.Combine(root, "src", "Other");
            Directory.CreateDirectory(otherDirectory);
            File.WriteAllText(
                Path.Combine(otherDirectory, "Solution.xml"),
                "<ImportExportXml xmlns=\"urn:test\"><SolutionManifest><UniqueName>ContosoCore</UniqueName></SolutionManifest></ImportExportXml>");

            SolutionManifestReader.ReadUniqueName(Path.Combine(root, "src")).Should().Be("ContosoCore");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ReplaceSolutionContent_RestoresOriginalContent_AndRetainsBackup_WhenReplacementFails()
    {
        var root = CreateTemporaryDirectory();
        string? backupPath = null;
        try
        {
            var source = Path.Combine(root, "clone", "src");
            var target = Path.Combine(root, "project", "src");
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(target);
            var sourceFile = Path.Combine(source, "replacement.xml");
            File.WriteAllText(sourceFile, "replacement");
            File.WriteAllText(Path.Combine(target, "original.xml"), "original");
            var service = new DataverseSolutionProjectFileService();

            using (new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var action = () => service.ReplaceSolutionContent(source, target);

                var exception = action.Should().Throw<SolutionContentReplacementException>().Which;
                backupPath = exception.BackupPath;
            }

            File.ReadAllText(Path.Combine(target, "original.xml")).Should().Be("original");
            File.Exists(Path.Combine(target, "replacement.xml")).Should().BeFalse();
            Directory.Exists(backupPath).Should().BeTrue();
            File.ReadAllText(Path.Combine(backupPath!, "original.xml")).Should().Be("original");
        }
        finally
        {
            Directory.Delete(ToExtendedPath(root), recursive: true);
        }
    }

    [Fact]
    public void ReplaceSolutionContent_HandlesPathsBeyondLegacyMaxPath()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var source = Path.Combine(root, "clone", "src");
            var target = Path.Combine(root, "project", "src");
            var deepRelativePath = Path.Combine(
                "CustomAPI",
                new string('a', 70),
                new string('b', 70),
                new string('c', 70),
                "customapirequestparameter.xml");
            var sourceFile = Path.Combine(source, deepRelativePath);
            var targetFile = Path.Combine(target, deepRelativePath);
            Directory.CreateDirectory(ToExtendedPath(Path.GetDirectoryName(sourceFile)!));
            Directory.CreateDirectory(ToExtendedPath(Path.GetDirectoryName(targetFile)!));
            File.WriteAllText(ToExtendedPath(sourceFile), "new");
            File.WriteAllText(ToExtendedPath(targetFile), "old");
            var service = new DataverseSolutionProjectFileService();

            service.ReplaceSolutionContent(source, target);

            File.ReadAllText(ToExtendedPath(targetFile)).Should().Be("new");
        }
        finally
        {
            Directory.Delete(ToExtendedPath(root), recursive: true);
        }
    }

    [Fact]
    public void CopyGeneratedProjectDirectory_CopiesNestedPacProjectWithoutWrapperDirectory()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var outputDirectory = Path.Combine(root, ".xpc-output");
            var generatedProjectDirectory = Path.Combine(outputDirectory, "ContosoCore");
            var solutionManifestDirectory = Path.Combine(generatedProjectDirectory, "src", "Other");
            Directory.CreateDirectory(solutionManifestDirectory);
            File.WriteAllText(Path.Combine(solutionManifestDirectory, "Solution.xml"), "<ImportExportXml />");
            File.WriteAllText(Path.Combine(generatedProjectDirectory, "ContosoCore.cdsproj"), "generated");
            var projectDirectory = Path.Combine(root, "ContosoCore");
            var service = new DataverseSolutionProjectFileService();

            var result = service.CopyGeneratedProjectDirectory(outputDirectory, projectDirectory);

            result.Should().Be(projectDirectory);
            File.Exists(Path.Combine(projectDirectory, "ContosoCore.cdsproj")).Should().BeTrue();
            File.Exists(Path.Combine(projectDirectory, "src", "Other", "Solution.xml")).Should().BeTrue();
            Directory.Exists(Path.Combine(projectDirectory, "ContosoCore")).Should().BeFalse();
            Directory.Exists(generatedProjectDirectory).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void CopyGeneratedProjectDirectory_SupportsProjectGeneratedDirectlyInOutputDirectory()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var outputDirectory = Path.Combine(root, ".xpc-output");
            var solutionManifestDirectory = Path.Combine(outputDirectory, "src", "Other");
            Directory.CreateDirectory(solutionManifestDirectory);
            File.WriteAllText(Path.Combine(solutionManifestDirectory, "Solution.xml"), "<ImportExportXml />");
            File.WriteAllText(Path.Combine(outputDirectory, "ContosoCore.cdsproj"), "generated");
            var projectDirectory = Path.Combine(root, "ContosoCore");
            var service = new DataverseSolutionProjectFileService();

            service.CopyGeneratedProjectDirectory(outputDirectory, projectDirectory);

            Directory.Exists(outputDirectory).Should().BeTrue();
            File.Exists(Path.Combine(projectDirectory, "ContosoCore.cdsproj")).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void CopyGeneratedProjectDirectory_CopiesProjectWithRestrictiveDirectoryAttributes()
    {
        var root = CreateTemporaryDirectory();
        var generatedProjectDirectory = Path.Combine(root, ".xpc-output", "ContosoCore");
        try
        {
            var solutionManifestDirectory = Path.Combine(generatedProjectDirectory, "src", "Other");
            Directory.CreateDirectory(solutionManifestDirectory);
            File.WriteAllText(Path.Combine(solutionManifestDirectory, "Solution.xml"), "<ImportExportXml />");
            File.WriteAllText(Path.Combine(generatedProjectDirectory, "ContosoCore.cdsproj"), "generated");
            File.SetAttributes(generatedProjectDirectory, FileAttributes.Hidden | FileAttributes.ReadOnly);
            var projectDirectory = Path.Combine(root, "ContosoCore");
            var service = new DataverseSolutionProjectFileService();

            service.CopyGeneratedProjectDirectory(Path.Combine(root, ".xpc-output"), projectDirectory);

            File.Exists(Path.Combine(projectDirectory, "ContosoCore.cdsproj")).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(generatedProjectDirectory))
            {
                File.SetAttributes(generatedProjectDirectory, FileAttributes.Normal);
            }

            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void CopyGeneratedProjectDirectory_RemovesPartialDestinationAndRetainsStagingWhenCopyFails()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var outputDirectory = Path.Combine(root, ".xpc-output");
            var generatedProjectDirectory = Path.Combine(outputDirectory, "ContosoCore");
            var solutionManifestDirectory = Path.Combine(generatedProjectDirectory, "src", "Other");
            Directory.CreateDirectory(solutionManifestDirectory);
            File.WriteAllText(Path.Combine(solutionManifestDirectory, "Solution.xml"), "<ImportExportXml />");
            var lockedFilePath = Path.Combine(generatedProjectDirectory, "locked.cdsproj");
            File.WriteAllText(lockedFilePath, "generated");
            var projectDirectory = Path.Combine(root, "ContosoCore");
            var service = new DataverseSolutionProjectFileService();

            using (new FileStream(lockedFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var action = () => service.CopyGeneratedProjectDirectory(outputDirectory, projectDirectory);

                action.Should().Throw<IOException>()
                    .WithMessage($"*retained at '{generatedProjectDirectory}'*");
            }

            Directory.Exists(projectDirectory).Should().BeFalse();
            File.Exists(lockedFilePath).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void FinalizeProjectFile_AlbanianXrm_ReplacesPacGeneratedProjectFile()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            File.WriteAllText(Path.Combine(root, "Generated.cdsproj"), "generated");
            var service = new DataverseSolutionProjectFileService();

            var projectFilePath = service.FinalizeProjectFile(
                root,
                "ContosoCore",
                DataverseSolutionProjectSdk.AlbanianXrm);

            Path.GetFileName(projectFilePath).Should().Be("ContosoCore.cdsproj");
            Directory.GetFiles(root, "*.cdsproj").Should().ContainSingle();
            File.ReadAllText(projectFilePath).Should().Be(
                $"<Project Sdk=\"{DataverseSolutionProjectFileService.ProjectSdk}\">{Environment.NewLine}</Project>{Environment.NewLine}");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void FinalizeProjectFile_MicrosoftPowerApps_RetainsPacGeneratedProjectFile()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            const string generatedProject = "<Project><ItemGroup><PackageReference Include=\"Microsoft.PowerApps.MSBuild.Solution\" /></ItemGroup></Project>";
            var generatedProjectPath = Path.Combine(root, "ContosoCore.cdsproj");
            File.WriteAllText(generatedProjectPath, generatedProject);
            var service = new DataverseSolutionProjectFileService();

            var projectFilePath = service.FinalizeProjectFile(
                root,
                "ContosoCore",
                DataverseSolutionProjectSdk.MicrosoftPowerApps);

            projectFilePath.Should().Be(generatedProjectPath);
            File.ReadAllText(projectFilePath).Should().Be(generatedProject);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void WriteMinimalProjectFile_ReplacesPacGeneratedProjectFile()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            File.WriteAllText(Path.Combine(root, "Generated.cdsproj"), "generated");
            var service = new DataverseSolutionProjectFileService();

            var projectFilePath = service.WriteMinimalProjectFile(root, "ContosoCore");

            Path.GetFileName(projectFilePath).Should().Be("ContosoCore.cdsproj");
            Directory.GetFiles(root, "*.cdsproj").Should().ContainSingle();
            File.ReadAllText(projectFilePath).Should().Be(
                $"<Project Sdk=\"{DataverseSolutionProjectFileService.ProjectSdk}\">{Environment.NewLine}</Project>{Environment.NewLine}");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ReplaceSolutionContent_ReplacesOnlyTargetDirectoryContents()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var source = Path.Combine(root, "clone", "src");
            var target = Path.Combine(root, "project", "src");
            Directory.CreateDirectory(Path.Combine(source, "Other"));
            Directory.CreateDirectory(Path.Combine(target, "Old"));
            File.WriteAllText(Path.Combine(source, "Other", "Solution.xml"), "new");
            File.WriteAllText(Path.Combine(target, "Old", "obsolete.txt"), "old");
            var projectFile = Path.Combine(root, "project", "ContosoCore.cdsproj");
            File.WriteAllText(projectFile, "project");
            var service = new DataverseSolutionProjectFileService();

            service.ReplaceSolutionContent(source, target);

            File.Exists(Path.Combine(target, "Other", "Solution.xml")).Should().BeTrue();
            Directory.Exists(Path.Combine(target, "Old")).Should().BeFalse();
            File.ReadAllText(projectFile).Should().Be("project");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "xrmtools-cdsproj-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string ToExtendedPath(string path)
        => path.StartsWith(@"\\", StringComparison.Ordinal)
            ? @"\\?\UNC\" + path.Substring(2)
            : @"\\?\" + Path.GetFullPath(path);
}
#nullable restore
