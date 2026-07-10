#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using XrmTools.DataverseSolutions;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.SynchronizeDataverseSolutionCmdId)]
internal sealed class SynchronizeDataverseSolutionCommand : CdsProjectCommandBase<SynchronizeDataverseSolutionCommand>
{
    protected override DataverseSolutionCommandKind CommandKind => DataverseSolutionCommandKind.Synchronize;
}
#nullable restore
