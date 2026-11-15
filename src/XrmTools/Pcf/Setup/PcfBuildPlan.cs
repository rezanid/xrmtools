#nullable enable
namespace XrmTools.Pcf.Setup;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// The decisions for one Visual Studio solution build. A build has a single process environment, so it can use one
/// Node.js installation: the plan picks it, and every project that needs a different one is stopped with an
/// explanation instead of being built with the wrong version.
/// </summary>
internal sealed class PcfBuildPlan
{
    private readonly Dictionary<string, PcfSetupState> _states;

    private PcfBuildPlan(Dictionary<string, PcfSetupState> states, NodeInstallation? active) { _states = states; Active = active; }

    /// <summary>The installation this build uses; <c>null</c> when no PCF project is ready to build.</summary>
    public NodeInstallation? Active { get; }
    /// <summary>The PATH entries to put first for this build; empty when Visual Studio's own PATH already provides <see cref="Active"/>.</summary>
    public IEnumerable<string> Activation => Active is null || Active.Source == NodeEnvironmentSource.BuildEnvironment ? [] : Active.PathEntries;
    public IEnumerable<PcfSetupState> States => _states.Values;

    public static PcfBuildPlan Create(IEnumerable<PcfSetupState> states)
    {
        var map = states.ToDictionary(state => state.ProjectFile, StringComparer.OrdinalIgnoreCase);
        // Keeping an environment that already works comes first; then the choice most projects share. The last
        // key only makes the result repeatable.
        var active = map.Values.Where(state => state.Block is null)
            .GroupBy(state => state.Environment.Installation!, InstallationComparer.Instance)
            .OrderByDescending(group => group.Key.Source == NodeEnvironmentSource.BuildEnvironment)
            .ThenByDescending(group => group.Count())
            .ThenBy(group => group.Min(state => state.ProjectFile.ToUpperInvariant()), StringComparer.Ordinal)
            .FirstOrDefault()?.Key;
        return new PcfBuildPlan(map, active);
    }

    public PcfSetupState? Find(string projectFile) => _states.TryGetValue(projectFile, out var state) ? state : null;

    /// <summary>Why this project must not reach Microsoft's npm commands in this build; <c>null</c> when it may build.</summary>
    public PcfBuildBlock? BlockFor(PcfSetupState state)
    {
        if (state.Block is not null) return state.Block;
        var needed = state.Environment.Installation!;
        if (Active is null || needed.IsSameEnvironment(Active)) return null;
        var others = string.Join(", ", _states.Values.Where(other => other.Block is null && other.Environment.Installation!.IsSameEnvironment(Active)).Select(other => $"'{other.ProjectName}'"));
        return new PcfBuildBlock("XTPCF007", $"'{state.ProjectName}' needs a different Node.js version than the other PCF projects in this build.",
            $"PCF project '{state.ProjectName}' needs {needed.Describe()}, but this build is using {Active.Describe()} for {others}. " +
            "One Visual Studio build can use only one Node.js installation. If the projects can share one, select it for each of them with 'Set Up PCF Build'; " +
            "otherwise unload the other PCF projects before building this one, or open it in a separate Visual Studio window.");
    }

    /// <summary>The MSBuild global properties that carry this build's decision for one project to the generated targets.</summary>
    public IReadOnlyDictionary<string, string> PropertiesFor(PcfSetupState state)
    {
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { [PcfBuildTargets.EnabledProperty] = "true" };
        var block = BlockFor(state);
        if (block is not null)
        {
            properties["XrmToolsPcfSetupErrorCode"] = block.Code;
            properties["XrmToolsPcfSetupError"] = block.Message;
            return properties;
        }
        if (state.PackageManagerNotice is { } notice) properties["XrmToolsPcfPackageManagerError"] = notice;
        if (state.Dependencies.Status == PcfDependencyStatus.Incomplete) properties["XrmToolsPcfRestoreRequired"] = "true";
        var node = state.Environment.Installation!;
        if (node.Source != NodeEnvironmentSource.BuildEnvironment)
        {
            // The build itself confirms that the selection reached it, on whichever MSBuild node runs the project.
            properties["XrmToolsPcfExpectedNode"] = node.Version.ToString();
            properties["XrmToolsPcfEnvironmentNote"] = $"Xrm Tools: building '{state.ProjectName}' with {node.Describe()} from {node.NodeDirectory}";
        }
        return properties;
    }

    private sealed class InstallationComparer : IEqualityComparer<NodeInstallation>
    {
        public static readonly InstallationComparer Instance = new();
        public bool Equals(NodeInstallation? left, NodeInstallation? right) => left is not null && left.IsSameEnvironment(right);
        public int GetHashCode(NodeInstallation installation) => StringComparer.OrdinalIgnoreCase.GetHashCode(installation.NodeDirectory.TrimEnd('\\', '/'));
    }
}

/// <summary>
/// The build logic Xrm Tools adds to a PCF project. It is written to the project's MSBuild extensions folder (normally
/// <c>obj</c>), which Microsoft.Common.targets imports by wildcard: the same route by which NuGet brings in Microsoft's
/// PCF targets. Nothing in the project file, its imports or Microsoft's targets is changed or replaced.
/// </summary>
internal static class PcfBuildTargets
{
    public const string EnabledProperty = "XrmToolsPcfBuild";

    public const string Content = """
        <Project>
          <!--
            Generated by Xrm Tools for Visual Studio; rewritten when the project loads, so do not edit.
            Every target here is inert unless a Visual Studio build passes XrmToolsPcfBuild=true, which leaves
            command-line builds exactly as they were. The targets only run in front of Microsoft's PCF targets;
            none of Microsoft's targets or tasks is replaced.
          -->
          <PropertyGroup>
            <_XrmToolsPcfScriptsFile>$(MSBuildProjectDirectory)\node_modules\pcf-scripts\package.json</_XrmToolsPcfScriptsFile>
          </PropertyGroup>

          <!-- Setup needs a decision: stop before any of Microsoft's npm commands instead of failing inside them. -->
          <Target Name="XrmToolsPcfSetupGate" BeforeTargets="RestoreNPM;PcfClean;PcfBuild;PcfRebuild"
                  Condition="'$(XrmToolsPcfBuild)' == 'true' and '$(XrmToolsPcfSetupError)' != ''">
            <Error Code="$(XrmToolsPcfSetupErrorCode)" Text="$(XrmToolsPcfSetupError)" File="$(MSBuildProjectFullPath)" />
          </Target>

          <!-- The project belongs to another package manager: Microsoft's "npm install" must not add npm files to it. -->
          <Target Name="XrmToolsPcfPackageManagerGate" BeforeTargets="RestoreNPM"
                  Condition="'$(XrmToolsPcfBuild)' == 'true' and '$(XrmToolsPcfPackageManagerError)' != ''">
            <Error Code="XTPCF004" Text="$(XrmToolsPcfPackageManagerError)" File="$(MSBuildProjectFullPath)" />
          </Target>

          <!-- Confirms, inside the MSBuild node that runs this project, that the selected Node.js is the one npm will use. -->
          <Target Name="XrmToolsPcfVerifyNode" BeforeTargets="RestoreNPM;PcfClean;PcfBuild;PcfRebuild"
                  Condition="'$(XrmToolsPcfBuild)' == 'true' and '$(XrmToolsPcfExpectedNode)' != ''">
            <Exec Command="node --version" ConsoleToMSBuild="true" IgnoreExitCode="true" EchoOff="true"
                  StandardOutputImportance="low" StandardErrorImportance="low">
              <Output TaskParameter="ConsoleOutput" PropertyName="_XrmToolsPcfActualNode" />
            </Exec>
            <Error Condition="'$(_XrmToolsPcfActualNode)' != '$(XrmToolsPcfExpectedNode)'" Code="XTPCF008" File="$(MSBuildProjectFullPath)"
                   Text="PCF project '$(MSBuildProjectName)': this build was set up to use Node.js $(XrmToolsPcfExpectedNode), but the build process reported '$(_XrmToolsPcfActualNode)'. Nothing was built with the wrong version. Close all Visual Studio windows, reopen the solution and build again." />
            <Message Importance="high" Text="$(XrmToolsPcfEnvironmentNote)" Condition="'$(XrmToolsPcfEnvironmentNote)' != ''" />
          </Target>

          <!--
            Microsoft's Rebuild runs "npm run clean" before its own automatic "npm install", which fails on a fresh
            checkout. Run Microsoft's install target first; MSBuild then skips its later, normal invocation.
          -->
          <Target Name="XrmToolsPcfRestoreBeforeRebuild" BeforeTargets="BeforeRebuild"
                  Condition="'$(XrmToolsPcfBuild)' == 'true' and '$(XrmToolsPcfSetupError)' == '' and '$(XrmToolsPcfPackageManagerError)' == '' and '$(PcfEnableAutoNpmInstall)' == 'true' and !Exists('$(_XrmToolsPcfScriptsFile)')">
            <Message Importance="high" Text="Xrm Tools: npm dependencies are not installed yet; running Microsoft's npm restore before Clean." />
            <CallTarget Targets="RestoreNPM" />
          </Target>

          <!-- A setup that was canceled or failed can leave node_modules half installed; let Microsoft's install finish it. -->
          <Target Name="XrmToolsPcfRepairIncompleteSetup" BeforeTargets="BeforeBuild;BeforeRebuild"
                  Condition="'$(XrmToolsPcfBuild)' == 'true' and '$(XrmToolsPcfRestoreRequired)' == 'true' and '$(XrmToolsPcfSetupError)' == '' and '$(XrmToolsPcfPackageManagerError)' == '' and '$(PcfEnableAutoNpmInstall)' == 'true'">
            <Message Importance="high" Text="Xrm Tools: an earlier dependency setup did not finish; running Microsoft's npm restore." />
            <CallTarget Targets="RestoreNPM" />
          </Target>

          <!-- Microsoft's npm scripts need pcf-scripts; say so instead of letting them fail with "'pcf-scripts' is not recognized". -->
          <Target Name="XrmToolsPcfDependencyGate" BeforeTargets="PcfClean;PcfBuild;PcfRebuild"
                  Condition="'$(XrmToolsPcfBuild)' == 'true' and !Exists('$(_XrmToolsPcfScriptsFile)')">
            <Error Condition="'$(XrmToolsPcfPackageManagerError)' != ''" Code="XTPCF004" Text="$(XrmToolsPcfPackageManagerError)" File="$(MSBuildProjectFullPath)" />
            <Error Condition="'$(PcfEnableAutoNpmInstall)' != 'true'" Code="XTPCF005" File="$(MSBuildProjectFullPath)"
                   Text="PCF project '$(MSBuildProjectName)': npm dependencies are not installed (node_modules\pcf-scripts is missing), and automatic install is turned off for this project (PcfEnableAutoNpmInstall). Choose 'Set Up PCF Build' on the project's context menu to install them, then build again." />
            <Error Code="XTPCF005" File="$(MSBuildProjectFullPath)"
                   Text="PCF project '$(MSBuildProjectName)': npm dependencies are not installed (node_modules\pcf-scripts is missing), so Microsoft's PCF script for this step cannot run. Build the project first to install them, or choose 'Set Up PCF Build' on the project's context menu." />
          </Target>
        </Project>
        """;

    public static string FileFor(string projectFile, string? extensionsPath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(projectFile))!;
        var extensions = string.IsNullOrWhiteSpace(extensionsPath) ? Path.Combine(directory, "obj") : Path.GetFullPath(Path.Combine(directory, extensionsPath!));
        return Path.Combine(extensions, Path.GetFileName(projectFile) + ".xrmtools.g.targets");
    }

    /// <returns><c>true</c> when the file is in place; <c>false</c> when it could not be written, which leaves builds as they were.</returns>
    public static bool Ensure(string file)
    {
        try
        {
            if (File.Exists(file) && File.ReadAllText(file) == Content) return true;
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, Content, new UTF8Encoding(false));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException) { return false; }
    }
}
#nullable restore
