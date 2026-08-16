#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using XrmTools.DataverseSolutions;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.ImportAndOpenDataverseSolutionCmdId)]
internal sealed class ImportAndOpenDataverseSolutionCommand : CdsProjectCommandBase<ImportAndOpenDataverseSolutionCommand>
{
    protected override DataverseSolutionCommandKind CommandKind => DataverseSolutionCommandKind.ImportAndOpen;
}
#nullable restore
