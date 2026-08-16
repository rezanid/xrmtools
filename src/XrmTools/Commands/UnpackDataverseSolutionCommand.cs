#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using XrmTools.DataverseSolutions;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.UnpackDataverseSolutionCmdId)]
internal sealed class UnpackDataverseSolutionCommand : CdsProjectCommandBase<UnpackDataverseSolutionCommand>
{
    protected override DataverseSolutionCommandKind CommandKind => DataverseSolutionCommandKind.Unpack;
}
#nullable restore
