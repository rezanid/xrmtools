#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using XrmTools.DataverseSolutions;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.RecloneDataverseSolutionCmdId)]
internal sealed class RecloneDataverseSolutionCommand : CdsProjectCommandBase<RecloneDataverseSolutionCommand>
{
    protected override DataverseSolutionCommandKind CommandKind => DataverseSolutionCommandKind.Reclone;
}
#nullable restore
