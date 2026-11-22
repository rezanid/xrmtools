#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

[Export(typeof(ICommandHandler))]
[Name(nameof(ODataGoToDefinition))]
[ContentType(ODataContentType.Name)]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class ODataGoToDefinition : ICommandHandler<GoToDefinitionCommandArgs>
{
    public string DisplayName => "Go to OData definition";

    public CommandState GetCommandState(GoToDefinitionCommandArgs args)
    {
        var caret = args.TextView.Caret.Position.BufferPosition;
        return caret.Snapshot.TextBuffer == args.SubjectBuffer && ODataDocument.Variable.IsMatch(caret.GetContainingLine().GetText())
            ? CommandState.Available : CommandState.Unspecified;
    }

    public bool ExecuteCommand(GoToDefinitionCommandArgs args, CommandExecutionContext context)
    {
        var view = args.TextView;
        var caret = view.Caret.Position.BufferPosition;
        if (caret.Snapshot.TextBuffer != args.SubjectBuffer) return false;

        var line = caret.GetContainingLine();
        var definition = ODataNavigation.FindDefinition(caret.Snapshot.GetText(), line.LineNumber, caret.Position - line.Start.Position);
        if (definition is not { } location) return false;

        var destination = caret.Snapshot.GetLineFromLineNumber(location.Line).Start + location.Column;
        view.Selection.Clear();
        view.Caret.MoveTo(destination);
        view.Caret.EnsureVisible();
        return true;
    }
}
