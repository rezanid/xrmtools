namespace XrmTools.Analyzers.Test;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

public class XrmToolsMetaAttributesNuGetTests
{
    [Theory]
    [InlineData("net462")]
    [InlineData("net48")]
    public async Task Package_DefaultAccessibility_RemainsInternal(string targetFramework)
    {
        var testRoot = CreateTestRoot();
        try
        {
            var packageVersion = "1.1.3-local";
            var feedPath = Path.Combine(testRoot, "feed");
            Directory.CreateDirectory(feedPath);

            var packResult = await RunDotNetAsync($"pack \"{GetPackageProjectPath()}\" -o \"{feedPath}\" -p:PackageVersion={packageVersion}");
            Assert.True(packResult.ExitCode == 0, packResult.Output);

            var consumerBuildResult = await RunDotNetAsync(
                $"build \"{GetConsumerProjectPath()}\" -p:TargetFramework={targetFramework} -p:XrmToolsMetaAttributesRestoreSources=\"{feedPath}\" -p:XrmToolsMetaAttributesPackageVersion={packageVersion}");

            Assert.True(consumerBuildResult.ExitCode == 0, consumerBuildResult.Output);

            var buildResult = await RunDotNetAsync(
                $"build \"{GetInspectorProjectPath()}\" -p:TargetFramework={targetFramework}");

            Assert.True(buildResult.ExitCode != 0, buildResult.Output);
            Assert.Contains("CS0122", buildResult.Output, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }

    [Theory]
    [InlineData("net462")]
    [InlineData("net48")]
    public async Task Package_PublicAccessibility_MakesTypesPublic(string targetFramework)
    {
        var testRoot = CreateTestRoot();
        try
        {
            var packageVersion = "1.1.3-local";
            var feedPath = Path.Combine(testRoot, "feed");
            Directory.CreateDirectory(feedPath);

            var packResult = await RunDotNetAsync($"pack \"{GetPackageProjectPath()}\" -o \"{feedPath}\" -p:PackageVersion={packageVersion}");
            Assert.True(packResult.ExitCode == 0, packResult.Output);

            var consumerBuildResult = await RunDotNetAsync(
                $"build \"{GetConsumerProjectPath()}\" -p:TargetFramework={targetFramework} -p:XrmToolsMetaAttributesRestoreSources=\"{feedPath}\" -p:XrmToolsMetaAttributesPackageVersion={packageVersion} -p:XrmToolsMetaAttributesUsePublicAccessibility=true");

            Assert.True(consumerBuildResult.ExitCode == 0, consumerBuildResult.Output);

            var buildResult = await RunDotNetAsync(
                $"build \"{GetInspectorProjectPath()}\" -p:TargetFramework={targetFramework}");

            Assert.True(buildResult.ExitCode == 0, buildResult.Output);
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }

    private static string GetConsumerProjectPath() => Path.Combine(GetRepositoryRoot(), "src", "Tests", "XrmTools.Meta.Attributes.NuGet", "Consumer", "Consumer.csproj");

    private static string GetPackageProjectPath() => Path.Combine(GetRepositoryRoot(), "src", "XrmTools.Meta.Attributes.Package", "XrmTools.Meta.Attributes.Package.msbuildproj");

    private static string GetInspectorProjectPath() => Path.Combine(GetRepositoryRoot(), "src", "Tests", "XrmTools.Meta.Attributes.NuGet", "Inspector", "Inspector.csproj");

    private static string GetRepositoryRoot() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));

    private static string CreateTestRoot()
    {
        var path = Path.Combine(Path.GetTempPath(), "XrmTools.Meta.Attributes.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static async Task<CommandResult> RunDotNetAsync(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = GetRepositoryRoot(),
        };

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);

        var standardOutput = await process!.StandardOutput.ReadToEndAsync();
        var standardError = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new CommandResult(process.ExitCode, standardOutput + Environment.NewLine + standardError);
    }

    private readonly struct CommandResult
    {
        public CommandResult(int exitCode, string output)
        {
            ExitCode = exitCode;
            Output = output;
        }

        public int ExitCode { get; }

        public string Output { get; }
    }
}
