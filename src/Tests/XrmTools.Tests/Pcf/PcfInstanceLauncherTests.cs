#nullable enable
namespace XrmTools.Tests.Pcf;

using FluentAssertions;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;
using XrmTools.Pcf.Setup;

public sealed class PcfInstanceLauncherTests
{
    [Theory]
    [InlineData("Exp", "/rootsuffix Exp \"C:\\src\\My Solution\\App.slnx\"")]
    [InlineData(" Exp ", "/rootsuffix Exp \"C:\\src\\My Solution\\App.slnx\"")]
    [InlineData(null, "\"C:\\src\\My Solution\\App.slnx\"")]
    [InlineData("", "\"C:\\src\\My Solution\\App.slnx\"")]
    public void Arguments_KeepTheRootSuffixOfThisInstance_AndQuoteTheSolution(string? rootSuffix, string expected)
        => PcfInstanceProcess.Arguments(rootSuffix, @"C:\src\My Solution\App.slnx").Should().Be(expected);

    [Fact]
    public void Environment_PutsTheSelectedNodeFirstOnPath_AndKeepsEverythingElse()
    {
        var current = new Hashtable { ["Path"] = @"C:\Windows\System32", ["TEMP"] = @"C:\Temp", ["NPM_CONFIG_USERCONFIG"] = @"C:\team\.npmrc" };

        var environment = PcfInstanceProcess.EnvironmentWith(current, [@"C:\fnm\node", @"C:\fnm\npm"]);

        environment.Should().HaveCount(3, "PATH is replaced under the spelling it already has, not added a second time");
        environment["Path"].Should().Be(@"C:\fnm\node;C:\fnm\npm;C:\Windows\System32");
        environment["NPM_CONFIG_USERCONFIG"].Should().Be(@"C:\team\.npmrc");
        current["Path"].Should().Be(@"C:\Windows\System32", "the caller's own environment is only read");
        PcfInstanceProcess.EnvironmentBlock(environment).Should().Be("NPM_CONFIG_USERCONFIG=C:\\team\\.npmrc\0Path=C:\\fnm\\node;C:\\fnm\\npm;C:\\Windows\\System32\0TEMP=C:\\Temp\0\0");
    }

    [Fact]
    public void Environment_WithoutAnyPath_GetsOne()
        => PcfInstanceProcess.EnvironmentWith(new Hashtable(), [@"C:\node"])["PATH"].Should().Be(@"C:\node;");

    [Fact]
    public void Start_GivesTheNewProcessTheSelectedNodeFirst_WithoutChangingThisProcess()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        var report = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pcf-instance-" + Guid.NewGuid().ToString("N") + ".txt");
        try
        {
            // cmd.exe stands in for devenv.exe and writes down the PATH it was really started with.
            var id = PcfInstanceProcess.Start(Path.Combine(Environment.SystemDirectory, "cmd.exe"), $"/d /s /c \"echo %PATH%> \"{report}\"\"",
                PcfInstanceProcess.EnvironmentWith(Environment.GetEnvironmentVariables(), [@"C:\xrmtools-selected-node"]), Environment.SystemDirectory);

            id.Should().BePositive().And.NotBe(Process.GetCurrentProcess().Id);
            var timer = Stopwatch.StartNew();
            while (timer.Elapsed < TimeSpan.FromSeconds(15) && (!File.Exists(report) || new FileInfo(report).Length == 0)) Thread.Sleep(50);
            File.ReadAllText(report).Should().StartWith(@"C:\xrmtools-selected-node;" + path!.Substring(0, Math.Min(20, path.Length)));
            Environment.GetEnvironmentVariable("PATH").Should().Be(path);
        }
        finally { File.Delete(report); }
    }

    [Fact]
    public void Start_ThatCannotStart_SaysWhichProgramAndWhy()
    {
        Action start = () => PcfInstanceProcess.Start(@"C:\no-such-folder\devenv.exe", "/rootsuffix Exp", PcfInstanceProcess.EnvironmentWith(new Hashtable(), [@"C:\node"]), Environment.SystemDirectory);
        start.Should().Throw<System.ComponentModel.Win32Exception>().WithMessage(@"'C:\no-such-folder\devenv.exe' could not be started:*");
    }
}
#nullable restore
