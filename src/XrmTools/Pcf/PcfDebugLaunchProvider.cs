#nullable enable
namespace XrmTools.Pcf;

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.DataverseSolutions;

[ExportDebugger(DebuggerName)]
[AppliesTo(PcfProjectRegistration.UniqueCapability)]
internal sealed class PcfDebugLaunchProvider : DebugLaunchProviderBase
{
    internal const string DebuggerName = "XrmToolsPcfHarness";
    // Installed Visual Studio JavaScript project systems use this engine with launch.json options.
    internal static readonly Guid JavaScriptEngine = new("394120B6-2FF9-4D0D-8953-913EF5CD0BCD");
    private readonly PcfHarnessSession _session;

    [ExportPropertyXamlRuleDefinition("XrmTools, Version=" + Vsix.Version + ".0, Culture=neutral, PublicKeyToken=null", "XrmTools.Pcf.PcfHarnessDebugger.xml", "Project")]
    [AppliesTo(PcfProjectRegistration.UniqueCapability)]
    private static object DebuggerRule => throw new NotSupportedException();

    [ImportingConstructor]
    public PcfDebugLaunchProvider(ConfiguredProject project, PcfHarnessSession session) : base(project)
        => _session = session;

    public override Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions) => Task.FromResult(true);

    // CPS falls back to LaunchAsync when a provider cannot precompute a target without starting a server.
    public override Task<int> QueryDebugTargetsCountAsync(DebugLaunchOptions launchOptions) => throw new NotImplementedException();
    public override Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions) => throw new NotImplementedException();

    public override async Task LaunchAsync(DebugLaunchOptions launchOptions)
    {
        var started = false;
        try
        {
            var address = await _session.StartAsync((launchOptions & DebugLaunchOptions.NoDebug) == 0, CancellationToken.None);
            started = true;
            if ((launchOptions & DebugLaunchOptions.NoDebug) != 0)
            {
                // The normal harness already opens the user's browser; no debugger is attached.
                return;
            }
            var settings = CreateSettings(launchOptions, address, Path.GetDirectoryName(ConfiguredProject.UnconfiguredProject.FullPath)!);
            await LaunchAsync(new[] { settings });
        }
        catch
        {
            if (started) _session.Stop();
            throw;
        }
    }

    internal static IDebugLaunchSettings CreateSettings(DebugLaunchOptions options, Uri address, string projectDirectory)
        => new DebugLaunchSettings(options | DebugLaunchOptions.StopDebuggingOnEnd)
        {
            LaunchOperation = DebugLaunchOperation.CreateProcess,
            Executable = "js-debug",
            CurrentDirectory = projectDirectory,
            LaunchDebugEngineGuid = JavaScriptEngine,
            Options = JsonConvert.SerializeObject(new
            {
                type = "pwa-msedge",
                request = "launch",
                name = "PCF Local Harness",
                url = address.AbsoluteUri,
                webRoot = projectDirectory,
                userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XrmTools", "PcfDebugBrowser"),
                sourceMaps = true
            })
        };
}

[Export(typeof(IProjectGlobalPropertiesProvider))]
[AppliesTo(PcfProjectRegistration.UniqueCapability)]
internal sealed class PcfDebugGlobalPropertiesProvider : StaticGlobalPropertiesProviderBase
{
    [ImportingConstructor]
    public PcfDebugGlobalPropertiesProvider(UnconfiguredProject project) : base(project.Services) { }

    public override Task<IImmutableDictionary<string, string>> GetGlobalPropertiesAsync(CancellationToken cancellationToken)
        => Task.FromResult<IImmutableDictionary<string, string>>(Empty.PropertiesMap.Add("DebuggerFlavor", PcfDebugLaunchProvider.DebuggerName));
}
#nullable restore
