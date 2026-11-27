#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

[Export(typeof(ICommandHandler))]
[Name(nameof(ODataInlineRename))]
[Order(Before = PredefinedCompletionNames.CompletionCommandHandler)]
[ContentType(ODataContentType.Name)]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class ODataInlineRename : ICommandHandler<RenameCommandArgs>,
    ICommandHandler<ReturnKeyCommandArgs>, ICommandHandler<EscapeKeyCommandArgs>,
    ICommandHandler<BackspaceKeyCommandArgs>, ICommandHandler<DeleteKeyCommandArgs>, ICommandHandler<TypeCharCommandArgs>,
    ICommandHandler<LeftKeyCommandArgs>, ICommandHandler<RightKeyCommandArgs>,
    ICommandHandler<LineStartCommandArgs>, ICommandHandler<LineEndCommandArgs>,
    ICommandHandler<LineStartExtendCommandArgs>, ICommandHandler<LineEndExtendCommandArgs>
{
    [Import] internal ITextBufferUndoManagerProvider UndoManagers { get; set; } = null!;
    [Import] internal IEditorOperationsFactoryService EditorOperations { get; set; } = null!;
    [Import] internal IAsyncCompletionBroker CompletionBroker { get; set; } = null!;

    public string DisplayName => "Rename OData symbol";

    public CommandState GetCommandState(RenameCommandArgs args)
        => args.TextView is IWpfTextView && args.TextView.TextBuffer == args.SubjectBuffer
            ? CommandState.Available : CommandState.Unspecified;

    public bool ExecuteCommand(RenameCommandArgs args, CommandExecutionContext context)
    {
        if (args.TextView is not IWpfTextView view || view.TextBuffer != args.SubjectBuffer) return false;
        if (view.Properties.ContainsProperty(typeof(Session))) return true;
        var snapshot = view.TextSnapshot;
        var caret = view.Caret.Position.BufferPosition;
        if (caret.Snapshot != snapshot) return false;
        var line = caret.GetContainingLine();
        var rename = ODataRename.Find(snapshot.GetText(), line.LineNumber, caret.Position - line.Start.Position);
        if (rename == null) return false;

        var spans = rename.Occurrences.Select(occurrence => new SnapshotSpan(
            snapshot.GetLineFromLineNumber(occurrence.Line).Start + occurrence.Column, occurrence.Name.Length)).ToArray();
        if (spans.Any(span => args.SubjectBuffer.IsReadOnly(span.Span))) return false;
        var anchor = snapshot.GetLineFromLineNumber(rename.Active.Line).Start + rename.Active.Column;
        view.Caret.EnsureVisible();
        var bounds = view.TextViewLines.GetCharacterBounds(anchor);
        var history = UndoManagers.GetTextBufferUndoManager(args.SubjectBuffer).TextBufferUndoHistory;
        CompletionBroker.GetSession(view)?.Dismiss();
        new Session(view, rename, spans, bounds.Left - view.ViewportLeft, bounds.Bottom - view.ViewportTop,
            history, EditorOperations.GetEditorOperations(view)).Open();
        return true;
    }

    public CommandState GetCommandState(ReturnKeyCommandArgs args) => StateForSession(args.TextView);
    public CommandState GetCommandState(EscapeKeyCommandArgs args) => StateForSession(args.TextView);
    public bool ExecuteCommand(ReturnKeyCommandArgs args, CommandExecutionContext context) => EndSession(args.TextView, commit: true);
    public bool ExecuteCommand(EscapeKeyCommandArgs args, CommandExecutionContext context) => EndSession(args.TextView, commit: false);

    public CommandState GetCommandState(BackspaceKeyCommandArgs args) => StateForSession(args.TextView);
    public CommandState GetCommandState(DeleteKeyCommandArgs args) => StateForSession(args.TextView);
    public CommandState GetCommandState(TypeCharCommandArgs args) => StateForSession(args.TextView);

    public bool ExecuteCommand(BackspaceKeyCommandArgs args, CommandExecutionContext context)
        => EditInput(args.TextView, input => EditingCommands.Backspace.Execute(null, input));

    public bool ExecuteCommand(DeleteKeyCommandArgs args, CommandExecutionContext context)
        => EditInput(args.TextView, input => EditingCommands.Delete.Execute(null, input));

    public bool ExecuteCommand(TypeCharCommandArgs args, CommandExecutionContext context)
        => EditInput(args.TextView, input =>
        {
            var start = input.SelectionStart;
            input.SelectedText = args.TypedChar.ToString();
            input.Select(start + 1, 0);
        });

    public CommandState GetCommandState(LeftKeyCommandArgs args) => StateForSession(args.TextView);
    public CommandState GetCommandState(RightKeyCommandArgs args) => StateForSession(args.TextView);
    public CommandState GetCommandState(LineStartCommandArgs args) => StateForSession(args.TextView);
    public CommandState GetCommandState(LineEndCommandArgs args) => StateForSession(args.TextView);
    public CommandState GetCommandState(LineStartExtendCommandArgs args) => StateForSession(args.TextView);
    public CommandState GetCommandState(LineEndExtendCommandArgs args) => StateForSession(args.TextView);

    public bool ExecuteCommand(LeftKeyCommandArgs args, CommandExecutionContext context)
        => NavigateInput(args.TextView, Key.Left);

    public bool ExecuteCommand(RightKeyCommandArgs args, CommandExecutionContext context)
        => NavigateInput(args.TextView, Key.Right);

    public bool ExecuteCommand(LineStartCommandArgs args, CommandExecutionContext context)
        => NavigateInput(args.TextView, Key.Home);

    public bool ExecuteCommand(LineEndCommandArgs args, CommandExecutionContext context)
        => NavigateInput(args.TextView, Key.End);

    public bool ExecuteCommand(LineStartExtendCommandArgs args, CommandExecutionContext context)
        => NavigateInput(args.TextView, Key.Home, extend: true);

    public bool ExecuteCommand(LineEndExtendCommandArgs args, CommandExecutionContext context)
        => NavigateInput(args.TextView, Key.End, extend: true);

    private static bool NavigateInput(ITextView view, Key key, bool extend = false)
    {
        var modifiers = Keyboard.Modifiers;
        var select = extend || (modifiers & ModifierKeys.Shift) != 0;
        var byWord = (modifiers & ModifierKeys.Control) != 0;
        var command = (key, select, byWord) switch
        {
            (Key.Left, false, false) => EditingCommands.MoveLeftByCharacter,
            (Key.Left, true, false) => EditingCommands.SelectLeftByCharacter,
            (Key.Left, false, true) => EditingCommands.MoveLeftByWord,
            (Key.Left, true, true) => EditingCommands.SelectLeftByWord,
            (Key.Right, false, false) => EditingCommands.MoveRightByCharacter,
            (Key.Right, true, false) => EditingCommands.SelectRightByCharacter,
            (Key.Right, false, true) => EditingCommands.MoveRightByWord,
            (Key.Right, true, true) => EditingCommands.SelectRightByWord,
            (Key.Home or Key.Up, false, _) => EditingCommands.MoveToLineStart,
            (Key.Home or Key.Up, true, _) => EditingCommands.SelectToLineStart,
            (Key.End or Key.Down, false, _) => EditingCommands.MoveToLineEnd,
            (Key.End or Key.Down, true, _) => EditingCommands.SelectToLineEnd,
            _ => null
        };
        return command != null && EditInput(view, input => command.Execute(null, input));
    }

    internal static bool IsActive(ITextView view) => view.Properties.ContainsProperty(typeof(Session));

    private static bool EditInput(ITextView view, Action<TextBox> edit)
    {
        if (!view.Properties.TryGetProperty(typeof(Session), out Session session)) return false;
        session.EditInput(edit);
        return true;
    }

    private static CommandState StateForSession(ITextView view)
        => IsActive(view) ? CommandState.Available : CommandState.Unspecified;

    private static bool EndSession(ITextView view, bool commit)
    {
        if (!view.Properties.TryGetProperty(typeof(Session), out Session session)) return false;
        session.EndFromKeyboard(commit);
        return true;
    }

    private sealed class Session
    {
        private readonly IWpfTextView view;
        private readonly ODataRename rename;
        private readonly ITrackingSpan[] spans;
        private readonly ITextUndoHistory history;
        private readonly IEditorOperations operations;
        private readonly Popup popup;
        private readonly KeyEventHandler keyHandler;
        private readonly TextBox input = new() { MinWidth = 160, MaxWidth = 360 };
        private readonly TextBlock message = new() { Margin = new Thickness(0, 4, 0, 0) };
        private bool finished;

        public Session(IWpfTextView view, ODataRename rename, SnapshotSpan[] source, double left, double top,
            ITextUndoHistory history, IEditorOperations operations)
        {
            this.view = view;
            this.rename = rename;
            this.history = history;
            this.operations = operations;
            spans = source.Select(span => span.Snapshot.CreateTrackingSpan(span.Span, SpanTrackingMode.EdgeInclusive)).ToArray();
            input.Text = rename.Active.Name;
            message.Text = "Enter to rename · Esc to cancel";
            var panel = new StackPanel { Margin = new Thickness(8), Children = { input, message } };
            var border = new Border { Child = panel, BorderThickness = new Thickness(1) };
            border.SetResourceReference(Border.BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey);
            border.SetResourceReference(Border.BorderBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey);
            message.SetResourceReference(TextBlock.ForegroundProperty, EnvironmentColors.ToolWindowTextBrushKey);
            popup = new Popup
            {
                Placement = PlacementMode.Relative,
                PlacementTarget = view.VisualElement,
                HorizontalOffset = left,
                VerticalOffset = top,
                StaysOpen = false,
                Child = border
            };
            input.TextChanged += NameChanged;
            keyHandler = KeyDown;
            border.AddHandler(UIElement.PreviewKeyDownEvent, keyHandler, handledEventsToo: true);
            popup.Opened += (_, _) =>
            {
                Keyboard.Focus(input);
                input.SelectAll();
            };
            popup.Closed += (_, _) => Finish();
            view.Closed += ViewClosed;
            view.TextBuffer.Changed += BufferChanged;
        }

        public void Open()
        {
            view.Properties.AddProperty(typeof(Session), this);
            view.VisualElement.AddHandler(UIElement.PreviewKeyDownEvent, keyHandler, handledEventsToo: true);
            popup.IsOpen = true;
        }

        public void EditInput(Action<TextBox> edit)
        {
            Keyboard.Focus(input);
            edit(input);
        }

        private void NameChanged(object sender, TextChangedEventArgs args)
        {
            if (finished) return;
            message.Text = rename.Validate(input.Text) ?? "Enter to rename · Esc to cancel";
        }

        private void KeyDown(object sender, KeyEventArgs args)
        {
            if (finished) return;
            if (args.Key is not (Key.Enter or Key.Escape))
            {
                if (!input.IsKeyboardFocusWithin && NavigateInput(view, args.Key))
                    args.Handled = true;
                return;
            }
            args.Handled = true;
            EndFromKeyboard(commit: args.Key == Key.Enter);
        }

        public void EndFromKeyboard(bool commit)
        {
            if (finished) return;
            var name = input.Text;
            if (commit && rename.Validate(name) != null) return;
            if (commit && name != rename.Active.Name && !ApplyRename(name)) return;
            Finish();
            if (!view.IsClosed) view.VisualElement.Focus();
        }

        private bool ApplyRename(string name)
        {
            var snapshot = view.TextBuffer.CurrentSnapshot;
            using var edit = view.TextBuffer.CreateEdit();
            foreach (var span in spans)
            {
                if (edit.Replace(span.GetSpan(snapshot).Span, name)) continue;
                edit.Cancel();
                message.Text = "One or more occurrences are read-only.";
                return false;
            }

            using var transaction = history.CreateTransaction("Rename OData symbol");
            operations.AddBeforeTextBufferChangePrimitive();
            view.TextBuffer.Changed -= BufferChanged;
            try
            {
                edit.Apply();
                if (edit.Canceled)
                {
                    transaction.Cancel();
                    message.Text = "The document rejected the rename.";
                    return false;
                }

                operations.AddAfterTextBufferChangePrimitive();
                transaction.Complete();
                return true;
            }
            finally
            {
                if (!finished) view.TextBuffer.Changed += BufferChanged;
            }
        }

        private void BufferChanged(object sender, TextContentChangedEventArgs args) => Finish();

        private void ViewClosed(object sender, EventArgs args) => Finish();

        private void Finish()
        {
            if (finished) return;
            finished = true;
            view.Properties.RemoveProperty(typeof(Session));
            view.VisualElement.RemoveHandler(UIElement.PreviewKeyDownEvent, keyHandler);
            view.Closed -= ViewClosed;
            view.TextBuffer.Changed -= BufferChanged;
            popup.IsOpen = false;
        }
    }
}
