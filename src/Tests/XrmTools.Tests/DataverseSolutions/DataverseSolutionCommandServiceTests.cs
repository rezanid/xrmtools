#nullable enable
namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using XrmTools.DataverseSolutions;

public sealed class DataverseSolutionCommandServiceTests
{
    [Fact]
    public async Task TryDeleteTemporaryDirectoryAsync_DeletesDirectoryRecursively()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        Directory.CreateDirectory(Path.Combine(temporaryDirectory, "src", "Other"));
        File.WriteAllText(Path.Combine(temporaryDirectory, "src", "Other", "Solution.xml"), "content");

        var cleanupException = await DataverseSolutionCommandService.TryDeleteTemporaryDirectoryAsync(temporaryDirectory);

        cleanupException.Should().BeNull();
        Directory.Exists(temporaryDirectory).Should().BeFalse();
    }

    [Fact]
    public async Task TryDeleteTemporaryDirectoryAsync_TreatsMissingDirectoryAsClean()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "xrmtools-missing-" + Guid.NewGuid().ToString("N"));

        var cleanupException = await DataverseSolutionCommandService.TryDeleteTemporaryDirectoryAsync(temporaryDirectory);

        cleanupException.Should().BeNull();
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "xrmtools-command-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
#nullable restore
