#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using XrmTools.DataverseSolutions;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.ImportDataverseSolutionCmdId)]
internal sealed class ImportDataverseSolutionCommand : CdsProjectCommandBase<ImportDataverseSolutionCommand>
{
    protected override DataverseSolutionCommandKind CommandKind => DataverseSolutionCommandKind.Import;
}
#nullable restore
