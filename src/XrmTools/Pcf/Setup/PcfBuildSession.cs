#nullable enable
namespace XrmTools.Pcf.Setup;

using System.ComponentModel.Composition;
using System.Threading;

/// <summary>
/// Whether a Visual Studio solution build is running. Setup checks it so that npm never installs into a project
/// while a build may be installing into the same folder. Nothing here touches the process environment.
/// </summary>
[Export]
[PartCreationPolicy(CreationPolicy.Shared)]
internal sealed class PcfBuildSession
{
    private int _building;

    public bool IsBuilding => Volatile.Read(ref _building) != 0;

    /// <summary>Call before a build inspects any project, so a setup that starts afterwards sees the build.</summary>
    public void BeginBuild() => Volatile.Write(ref _building, 1);

    public void EndBuild() => Volatile.Write(ref _building, 0);
}
#nullable restore
