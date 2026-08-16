#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using XrmTools.DataverseSolutions;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.PackDataverseSolutionCmdId)]
internal sealed class PackDataverseSolutionCommand : CdsProjectCommandBase<PackDataverseSolutionCommand>
{
    protected override DataverseSolutionCommandKind CommandKind => DataverseSolutionCommandKind.Pack;
}
#nullable restore
