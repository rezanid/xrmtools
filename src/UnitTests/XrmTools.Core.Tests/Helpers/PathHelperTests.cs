namespace XrmTools.Core.Tests.Helpers;

using FluentAssertions;
using System.IO;
using System.Runtime.InteropServices;
using XrmTools.Helpers;
using Xunit;

public class PathHelperTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void ResolvePath_Should_Normalize_And_Collapse_DotSegments()
    {
        // Arrange
        var root = Directory.GetCurrentDirectory();
        var unresolved = Path.Combine(root, "a", ".", "b", "..", "c");

        // Act
        var resolved = PathHelper.ResolvePath(unresolved);

        // Assert
        resolved.Should().Be(Path.Combine(root, "a", "c"));
    }

    [Fact]
    public void ResolvePath_Should_Trim_Trailing_Separator_For_NonRoot()
    {
        // Arrange
        var root = Directory.GetCurrentDirectory();
        var withTrailing = Path.Combine(root, "folder") + Path.DirectorySeparatorChar;

        // Act
        var resolved = PathHelper.ResolvePath(withTrailing);

        // Assert
        resolved.Should().Be(Path.Combine(root, "folder"));
    }

    [Fact]
    public void ResolvePath_Should_Normalize_AltSeparators()
    {
        // Arrange
        var root = Directory.GetCurrentDirectory();
        var mixed = Path.Combine(root, "x") + Path.AltDirectorySeparatorChar + "y";

        // Act
        var resolved = PathHelper.ResolvePath(mixed);

        // Assert
        resolved.Should().Be(Path.Combine(root, "x", "y"));
    }

    [Fact]
    public void GetRelativePath_FromFile_Uses_ParentDirectory()
    {
        // Arrange
        // Base is a file; target is a file in a subfolder
        var baseFile = Path.Combine("C:", "work", "proj", "readme.md");
        var target = Path.Combine("C:", "work", "proj", "docs", "guide.md");

        if (!IsWindows)
        {
            // Make it portable on non-Windows
            var cwd = Directory.GetCurrentDirectory();
            baseFile = Path.Combine(cwd, "readme.md");
            target = Path.Combine(cwd, "docs", "guide.md");
        }

        // Act
        var rel = PathHelper.GetRelativePath(baseFile, target);

        // Assert
        rel.Should().Be(Path.Combine("docs", "guide.md"));
    }

    [Fact]
    public void GetRelativePath_FromDirectory_To_SiblingFile()
    {
        // Arrange
        var baseDir = Path.Combine("C:", "projects", "app", "bin");
        var target = Path.Combine("C:", "projects", "app", "readme.txt");

        if (!IsWindows)
        {
            var cwd = Directory.GetCurrentDirectory();
            baseDir = Path.Combine(cwd, "bin");
            target = Path.Combine(cwd, "readme.txt");
        }

        // Act
        var rel = PathHelper.GetRelativePath(baseDir, target);

        // Assert
        rel.Should().Be(Path.Combine("..", "readme.txt"));
    }

    [Fact]
    public void GetRelativePath_Resolves_DotDot_In_FromPath()
    {
        // Arrange
        string fromPath, toPath, expected;

        if (IsWindows)
        {
            fromPath = @"C:\Projects\App\src\..\bin";
            toPath = @"C:\Projects\App\readme.txt";
            expected = Path.Combine("..", "readme.txt");
        }
        else
        {
            var cwdParent = Directory.GetCurrentDirectory();
            var root = Path.Combine(cwdParent, "Projects", "App");
            fromPath = Path.Combine(root, "src", "..", "bin");
            toPath = Path.Combine(root, "readme.txt");
            expected = Path.Combine("..", "readme.txt");
        }

        // Act
        var rel = PathHelper.GetRelativePath(fromPath, toPath);

        // Assert
        rel.Should().Be(expected);
    }

    [Fact]
    public void GetRelativePath_SamePath_Returns_Dot_When_Both_Directories()
    {
        // Arrange
        var dir = IsWindows ? @"C:\work\repo" : Path.Combine(Directory.GetCurrentDirectory(), "repo");

        // Act
        var rel = PathHelper.GetRelativePath(dir, dir);

        // Assert
        rel.Should().Be(".");
    }

    [Fact]
    public void GetRelativePath_SameDirectory_FileTarget_Returns_FileName()
    {
        // Arrange
        string baseFile, target, expected;

        if (IsWindows)
        {
            baseFile = @"C:\work\repo\notes.txt";
            target = @"C:\work\repo\notes.txt";
            expected = "notes.txt";
        }
        else
        {
            var cwd = Directory.GetCurrentDirectory();
            baseFile = Path.Combine(cwd, "notes.txt");
            target = baseFile;
            expected = "notes.txt";
        }

        // Act
        var rel = PathHelper.GetRelativePath(baseFile, target);

        // Assert
        rel.Should().Be(expected);
    }

    [Fact]
    public void GetRelativePath_NonExistent_From_Still_Uses_Heuristics()
    {
        // Arrange (paths likely don’t exist, but extension hints file)
        var baseFile = Path.Combine(Path.GetTempPath(), "out", "app.exe");
        var target = Path.Combine(Path.GetTempPath(), "out", "logs", "log.txt");

        // Act
        var rel = PathHelper.GetRelativePath(baseFile, target);

        // Assert
        rel.Should().Be(Path.Combine("logs", "log.txt"));
    }

    [Fact]
    public void GetRelativePath_Should_Return_Absolute_When_Different_Drives_On_Windows()
    {
        if (!IsWindows)
        {
            // Not applicable on non-Windows (no drive letters)
            return;
        }

        // Arrange
        var fromPath = @"C:\work\repo";
        var toPath = @"D:\data\file.txt";

        // Act
        var rel = PathHelper.GetRelativePath(fromPath, toPath);

        // Assert
        rel.Should().Be(PathHelper.ResolvePath(toPath));
    }

    [Fact]
    public void GetRelativePath_Should_Normalize_Slashes_In_Output()
    {
        // Arrange
        // From dir to nested file; ensure output uses OS-native separators
        string baseDir, target;

        if (IsWindows)
        {
            baseDir = @"C:\a\b";
            target = @"C:\a\b\c\d.txt";
        }
        else
        {
            var cwd = Directory.GetCurrentDirectory();
            baseDir = Path.Combine(cwd, "a", "b");
            target = Path.Combine(cwd, "a", "b", "c", "d.txt");
        }

        // Act
        var rel = PathHelper.GetRelativePath(baseDir, target);

        // Assert
        rel.Should().Be(Path.Combine("c", "d.txt")); // platform-native separators
        rel.Should().NotContain(Path.AltDirectorySeparatorChar.ToString());
    }

    [Fact]
    public void GetRelativePath_CaseInsensitive_RootMatch_On_Windows()
    {
        if (!IsWindows) return;

        // Arrange (mixed case)
        var fromPath = @"c:\Work\Proj\README.md";
        var toPath = @"C:\work\proj\docs\guide.md";

        // Act
        var rel = PathHelper.GetRelativePath(fromPath, toPath);

        // Assert
        rel.Should().Be(Path.Combine("docs", "guide.md"));
    }
}
