namespace XrmTools.Tests.Helpers;

using FluentAssertions;
using System;
using System.IO;
using Xunit;
using XrmTools.Helpers;

public sealed class ProjectExtensionsTests
{
    [Fact]
    public void FindOutputPackagePath_Should_Use_PackageOutputPath_And_Ignore_Symbols_Package()
    {
        var projectDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var packageDirectory = Path.Combine(projectDirectory, "bin", "Release");
        Directory.CreateDirectory(packageDirectory);

        try
        {
            var packagePath = Path.Combine(packageDirectory, "Plugin.1.0.0.nupkg");
            File.WriteAllText(packagePath, string.Empty);
            File.WriteAllText(Path.Combine(packageDirectory, "Plugin.1.0.0.symbols.nupkg"), string.Empty);

            var result = ProjectExtensions.FindOutputPackagePath(
                Path.Combine(projectDirectory, "Plugin.csproj"),
                Path.Combine("bin", "Release"));

            result.Should().Be(packagePath);
        }
        finally
        {
            Directory.Delete(projectDirectory, recursive: true);
        }
    }
}
