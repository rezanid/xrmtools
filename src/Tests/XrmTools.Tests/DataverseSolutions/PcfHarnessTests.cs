namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using Xunit;
using XrmTools.Pcf;

public sealed class PcfHarnessTests
{
    [Theory]
    [InlineData("[Browsersync]        Local: http://localhost:8181", "http://localhost:8181/")]
    [InlineData("Local: http://127.0.0.1:9342/", "http://127.0.0.1:9342/")]
    [InlineData("\u001b[32mLocal:\u001b[0m http://localhost:8182", "http://localhost:8182/")]
    [InlineData("External: http://192.168.1.1:8181", null)]
    [InlineData("Local: https://example.com/", null)]
    [InlineData("Local: file:///C:/test", null)]
    [InlineData("Local: http://user:password@localhost:8181", null)]
    [InlineData("build succeeded", null)]
    public void ParseAddress_UsesOnlyTheHarnessReportedLoopbackUrl(string line, string expected)
        => (PcfHarnessProcess.ParseAddress(line)?.AbsoluteUri).Should().Be(expected);

    [Fact]
    public void ParseAddress_RejectsUserInfoEvenForLoopback()
    {
        var address = new UriBuilder("http", "localhost", 8181) { UserName = "test-user" };
        PcfHarnessProcess.ParseAddress("Local: " + address.Uri.AbsoluteUri).Should().BeNull();
    }

    [Fact]
    public void CreateSettings_LaunchesTheVisualStudioBrowserDebuggerWithSourceMaps()
    {
        var settings = PcfDebugLaunchProvider.CreateSettings(0, new Uri("http://localhost:9321/"), @"C:\Controls\A Control");
        settings.LaunchOperation.Should().Be(DebugLaunchOperation.CreateProcess);
        settings.LaunchDebugEngineGuid.Should().Be(PcfDebugLaunchProvider.JavaScriptEngine);
        settings.Executable.Should().Be("js-debug");
        settings.LaunchOptions.Should().HaveFlag(DebugLaunchOptions.StopDebuggingOnEnd);
        var options = JObject.Parse(settings.Options);
        options.Value<string>("type").Should().Be("pwa-msedge");
        options.Value<string>("request").Should().Be("launch");
        options.Value<string>("url").Should().Be("http://localhost:9321/");
        options.Value<string>("webRoot").Should().Be(@"C:\Controls\A Control");
        options.Value<string>("userDataDir").Should().StartWith(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        options.Value<bool>("sourceMaps").Should().BeTrue();
    }

    [Theory]
    [InlineData("npm.exe")]
    [InlineData("npm.cmd")]
    public void CreateStartInfo_UsesNpmLauncherWithoutAdjacentNpmCli(string launcher)
    {
        var directory = Path.Combine(Path.GetTempPath(), "xrmtools-pcf-tools-" + Guid.NewGuid().ToString("N"), "Node & Tools %test%");
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "node.exe"), string.Empty);
            File.WriteAllText(Path.Combine(directory, launcher), string.Empty);
            var info = PcfHarnessProcess.CreateStartInfo(directory, "\"" + directory + "\"");
            info.WorkingDirectory.Should().Be(directory);
            info.UseShellExecute.Should().BeFalse();
            info.RedirectStandardOutput.Should().BeTrue();
            info.RedirectStandardError.Should().BeTrue();
            info.EnvironmentVariables["PATH"].Should().StartWith(directory + Path.PathSeparator);
            if (launcher == "npm.exe")
            {
                info.FileName.Should().Be(Path.Combine(directory, launcher));
                info.Arguments.Should().Be("start -- watch");
            }
            else
            {
                info.FileName.Should().Be(Path.Combine(Environment.SystemDirectory, "cmd.exe"));
                info.EnvironmentVariables["XRMTOOLS_PCF_NPM"].Should().Be(Path.Combine(directory, launcher));
                info.Arguments.Should().Be("/d /s /c \"\"%XRMTOOLS_PCF_NPM%\" start -- watch\"");
            }
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(directory), recursive: true);
        }
    }

    [Fact]
    public void CreateStartInfo_BatchLauncher_ExecutesWithArgumentsAndQuotedPath()
    {
        var root = Path.Combine(Path.GetTempPath(), "xrmtools-pcf-tools-" + Guid.NewGuid().ToString("N"));
        var directory = Path.Combine(root, "Node & Tools %PATH%");
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "node.exe"), string.Empty);
            File.WriteAllText(Path.Combine(directory, "npm.cmd"), "@echo off\r\necho harness-args:%*\r\n");
            using var process = new System.Diagnostics.Process
            {
                StartInfo = PcfHarnessProcess.CreateStartInfo(directory, directory)
            };
            process.Start().Should().BeTrue();
            var exited = process.WaitForExit(10000);
            if (!exited) process.Kill();
            exited.Should().BeTrue();
            process.StandardError.ReadToEnd().Should().BeEmpty();
            process.StandardOutput.ReadToEnd().Trim().Should().Be("harness-args:start -- watch");
            process.ExitCode.Should().Be(0);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void CreateStartInfo_PreservesPathOrderAndSupportsSeparateNpmDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "xrmtools-pcf-tools-" + Guid.NewGuid().ToString("N"));
        var nodeDirectory = Path.Combine(directory, "node");
        var npmDirectory = Path.Combine(directory, "npm");
        Directory.CreateDirectory(nodeDirectory);
        Directory.CreateDirectory(npmDirectory);
        try
        {
            File.WriteAllText(Path.Combine(nodeDirectory, "node.exe"), string.Empty);
            File.WriteAllText(Path.Combine(nodeDirectory, "npm.cmd"), string.Empty);
            File.WriteAllText(Path.Combine(npmDirectory, "npm.exe"), string.Empty);
            var info = PcfHarnessProcess.CreateStartInfo(directory, npmDirectory + Path.PathSeparator + nodeDirectory);
            info.FileName.Should().Be(Path.Combine(npmDirectory, "npm.exe"));
            info.EnvironmentVariables["PATH"].Should().StartWith(nodeDirectory + Path.PathSeparator + npmDirectory + Path.PathSeparator);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(false, "*Node.js*all Visual Studio instances*Experimental*")]
    [InlineData(true, "*npm*all Visual Studio instances*Experimental*")]
    public void CreateStartInfo_MissingTool_ReportsWhichToolAndRestartScope(bool hasNode, string error)
    {
        var directory = Path.Combine(Path.GetTempPath(), "xrmtools-pcf-tools-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            if (hasNode) File.WriteAllText(Path.Combine(directory, "node.exe"), string.Empty);
            Action create = () => PcfHarnessProcess.CreateStartInfo(directory, directory);
            create.Should().Throw<FileNotFoundException>().WithMessage(error);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Start_MissingPackageJson_ReportsAnActionableError()
    {
        var directory = Path.Combine(Path.GetTempPath(), "xrmtools-pcf-missing-" + Guid.NewGuid().ToString("N"));
        Action start = () => PcfHarnessProcess.Start(directory, _ => { });
        start.Should().Throw<FileNotFoundException>().WithMessage("*package.json*");
    }

    [Theory]
    [InlineData("{}", "*start script*")]
    [InlineData("{\"scripts\":{\"start\":\"pcf-scripts start\"}}", "*npm install*")]
    public void Start_MissingScriptOrDependencies_DoesNotLaunchAProcess(string package, string error)
    {
        var directory = Path.Combine(Path.GetTempPath(), "xrmtools-pcf-prerequisites-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "package.json"), package);
            Action start = () => PcfHarnessProcess.Start(directory, _ => { });
            start.Should().Throw<InvalidOperationException>().WithMessage(error);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
