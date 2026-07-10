#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using XrmTools.DataverseSolutions;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.DeployAndOpenDataverseSolutionCmdId)]
internal sealed class DeployAndOpenDataverseSolutionCommand : CdsProjectCommandBase<DeployAndOpenDataverseSolutionCommand>
{
    protected override DataverseSolutionCommandKind CommandKind => DataverseSolutionCommandKind.DeployAndOpen;
}
#nullable restore
