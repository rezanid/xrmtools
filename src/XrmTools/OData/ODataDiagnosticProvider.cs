#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Community.VisualStudio.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;

[Export(typeof(ITaggerProvider))]
[ContentType(ODataContentType.Name)]
[TagType(typeof(IErrorTag))]
internal sealed class ODataDiagnosticProvider : ITaggerProvider
{
    public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag
        => ODataDiagnosticTagger.For(buffer) as ITagger<T>;
}

internal sealed class ODataDiagnosticTagger : ITagger<IErrorTag>
{
    private readonly object gate = new();
    private readonly ITextBuffer buffer;
    private ITextSnapshot? snapshot;
    private ODataBufferSession? session;
    private IReadOnlyList<ODataDiagnostic> diagnostics = [];

    private ODataDiagnosticTagger(ITextBuffer buffer)
    {
        this.buffer = buffer;
        buffer.Changed += (_, args) =>
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(args.After, 0, args.After.Length)));
    }

    public static ODataDiagnosticTagger For(ITextBuffer buffer)
        => buffer.Properties.GetOrCreateSingletonProperty(() => new ODataDiagnosticTagger(buffer));

    public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

    public IReadOnlyList<ODataDiagnostic> GetDiagnostics(ITextSnapshot current)
    {
        lock (gate)
        {
            var currentSession = ODataBufferSession.Get(buffer);
            if (!ReferenceEquals(session, currentSession))
            {
                if (session != null) session.VariablesChanged -= VariablesChanged;
                session = currentSession;
                if (session != null) session.VariablesChanged += VariablesChanged;
                snapshot = null;
            }
            if (snapshot != current)
            {
                string text = current.GetText();
                var document = session?.CreateDocument(text);
                diagnostics = ODataDiagnostics.Analyze(text, document?.EnvironmentVariables, session?.ConfigurationError);
                snapshot = current;
            }
            return diagnostics;
        }
    }

    private void VariablesChanged(object? sender, EventArgs args)
    {
        lock (gate) snapshot = null;
        var current = buffer.CurrentSnapshot;
        TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(current, 0, current.Length)));
    }

    public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
        if (spans.Count == 0) yield break;
        var current = spans[0].Snapshot;
        foreach (var diagnostic in GetDiagnostics(current))
        {
            var span = new SnapshotSpan(current, diagnostic.Start, diagnostic.Length);
            if (spans.IntersectsWith(span))
                yield return new TagSpan<IErrorTag>(span, new ErrorTag(PredefinedErrorTypeNames.SyntaxError, diagnostic.Message));
        }
    }
}

[Export(typeof(ISuggestedActionsSourceProvider))]
[Name("OData reference fixes")]
[ContentType(ODataContentType.Name)]
internal sealed class ODataSuggestedActionsProvider : ISuggestedActionsSourceProvider
{
    [Import] private IEnvironmentSelection Environments { get; set; } = null!;
    [Import] private ITextBufferUndoManagerProvider UndoManagers { get; set; } = null!;

    public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
        => new ODataSuggestedActions(textBuffer, Environments, UndoManagers);
}

internal sealed class ODataSuggestedActions : ISuggestedActionsSource
{
    private readonly ITextBuffer buffer;
    private readonly IEnvironmentSelection environments;
    private readonly ITextBufferUndoManagerProvider undoManagers;
    private readonly ODataDiagnosticTagger tagger;
    public ODataSuggestedActions(ITextBuffer buffer, IEnvironmentSelection environments, ITextBufferUndoManagerProvider undoManagers)
    {
        this.buffer = buffer;
        this.environments = environments;
        this.undoManagers = undoManagers;
        tagger = ODataDiagnosticTagger.For(buffer);
        tagger.TagsChanged += Changed;
    }

    public event EventHandler<EventArgs>? SuggestedActionsChanged;
    private void Changed(object sender, SnapshotSpanEventArgs args) => SuggestedActionsChanged?.Invoke(this, EventArgs.Empty);
    public void Dispose() => tagger.TagsChanged -= Changed;
    public bool TryGetTelemetryId(out Guid telemetryId) { telemetryId = Guid.Empty; return false; }

    private IEnumerable<ODataDiagnostic> Fixes(SnapshotSpan range, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return tagger.GetDiagnostics(range.Snapshot).Where(diagnostic =>
            diagnostic.Replacement != null && diagnostic.Start <= range.End.Position &&
            diagnostic.Start + diagnostic.Length >= range.Start.Position);
    }

    private bool TryGetMoveDeclaration(SnapshotSpan range, out ODataVariableDeclaration declaration, out ODataBufferSession session)
    {
        declaration = default;
        var currentSession = ODataBufferSession.Get(buffer);
        if (currentSession == null || string.IsNullOrWhiteSpace(currentSession.SavedDocumentPath) ||
            string.IsNullOrWhiteSpace(currentSession.SelectedEnvironmentName) || currentSession.IsLoading)
        {
            session = null!;
            return false;
        }
        session = currentSession;

        var snapshot = range.Snapshot;
        var line = snapshot.GetLineFromPosition(range.Start.Position);
        if (!TryParseDeclaration(snapshot.GetText(), line.LineNumber, out declaration)) return false;
        var span = new SnapshotSpan(line.Start, line.Length);
        return span.IntersectsWith(range) || range.IsEmpty && span.Contains(range.Start);
    }

    private static bool TryParseDeclaration(string text, int requestedLine, out ODataVariableDeclaration declaration)
    {
        declaration = default;
        var declarations = new List<ODataVariableDeclaration>();
        int lineStart = 0;
        var lines = System.Text.RegularExpressions.Regex.Split(text, "\\r\\n|\\r|\\n");
        var newlines = System.Text.RegularExpressions.Regex.Matches(text, "\\r\\n|\\r|\\n");
        for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
        {
            string line = lines[lineNumber];
            string trimmed = line.Trim();
            if (trimmed.StartsWith("@", StringComparison.Ordinal))
            {
                int equals = trimmed.IndexOf('=');
                string name = equals > 1 ? trimmed.Substring(1, equals - 1).Trim() : "";
                if (System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-z_][A-Za-z0-9_]*$"))
                {
                    string value = trimmed.Substring(equals + 1).Trim();
                    int column = line.IndexOf(name, StringComparison.Ordinal);
                    int newlineLength = lineNumber < newlines.Count ? newlines[lineNumber].Length : 0;
                    declarations.Add(new ODataVariableDeclaration(name, value, lineNumber, lineStart + column,
                        lineStart, line.Length + newlineLength));
                }
            }
            lineStart += line.Length + (lineNumber < newlines.Count ? newlines[lineNumber].Length : 0);
        }
        var match = declarations.FirstOrDefault(item => item.Line == requestedLine);
        var parsedDocument = ODataDocument.Parse(text);
        if (string.IsNullOrEmpty(match.Name) || declarations.Count(item => item.Name == match.Name) != 1 ||
            ODataDocument.Variable.IsMatch(match.Value) ||
            !parsedDocument.Variables.TryGetValue(match.Name, out var parsedValue) || parsedValue != match.Value) return false;
        declaration = match;
        return true;
    }

    public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet categories, SnapshotSpan range, CancellationToken token)
        => Task.FromResult(Fixes(range, token).Any() || TryGetMoveDeclaration(range, out _, out _));

    public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet categories, SnapshotSpan range, CancellationToken token)
    {
        var actions = Fixes(range, token)
            .Select(diagnostic => (ISuggestedAction)new ReplaceReferenceAction(new SnapshotSpan(range.Snapshot, diagnostic.Start, diagnostic.Length), diagnostic.Replacement!))
            .ToList();
        if (TryGetMoveDeclaration(range, out var declaration, out var session))
            actions.Add(new MoveToEnvironmentVariablesAction(buffer, range.Snapshot, declaration, session,
                session.SelectedEnvironmentName!, environments, undoManagers));
        return actions.Count == 0 ? [] : [new SuggestedActionSet(PredefinedSuggestedActionCategoryNames.CodeFix, actions)];
    }

    private readonly record struct ODataVariableDeclaration(string Name, string Value, int Line, int Start, int SpanStart, int SpanLength);

    private sealed class MoveToEnvironmentVariablesAction : ISuggestedAction
    {
        private readonly ITextBuffer buffer;
        private readonly ITextSnapshot originalSnapshot;
        private readonly ODataVariableDeclaration declaration;
        private readonly ODataBufferSession session;
        private readonly string expectedEnvironment;
        private readonly IEnvironmentSelection environments;
        private readonly ITextBufferUndoManagerProvider undoManagers;

        public MoveToEnvironmentVariablesAction(ITextBuffer buffer, ITextSnapshot originalSnapshot,
            ODataVariableDeclaration declaration, ODataBufferSession session, string expectedEnvironment,
            IEnvironmentSelection environments, ITextBufferUndoManagerProvider undoManagers)
        {
            this.buffer = buffer;
            this.originalSnapshot = originalSnapshot;
            this.declaration = declaration;
            this.session = session;
            this.expectedEnvironment = expectedEnvironment;
            this.environments = environments;
            this.undoManagers = undoManagers;
        }

        public string DisplayText => "Move to Environment Variables";
        public bool HasActionSets => false;
        public bool HasPreview => false;
        public string? IconAutomationText => null;
        public string? InputGestureText => null;
        public ImageMoniker IconMoniker => default;
        public Task<object?> GetPreviewAsync(CancellationToken token) => Task.FromResult<object?>(null);
        public Task<IEnumerable<SuggestedActionSet>?> GetActionSetsAsync(CancellationToken token) => Task.FromResult<IEnumerable<SuggestedActionSet>?>(null);
        public bool TryGetTelemetryId(out Guid telemetryId) { telemetryId = Guid.Empty; return false; }
        public void Dispose() { }

        public void Invoke(CancellationToken token)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            token.ThrowIfCancellationRequested();
            ThreadHelper.JoinableTaskFactory.RunAsync(() => MoveAsync(token)).FileAndForget("XrmTools/OData/MoveVariable");
        }

        private async Task MoveAsync(CancellationToken token)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
                if (!TryValidateCurrentDeclaration(out var span, out var error))
                {
                    await VS.MessageBox.ShowErrorAsync("OData environment variables", error);
                    return;
                }
                var selected = await environments.GetSelectedEnvironmentAsync();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
                if (selected?.IsValid != true || !string.Equals(selected.Name, session.SelectedEnvironmentName, StringComparison.OrdinalIgnoreCase))
                {
                    await VS.MessageBox.ShowErrorAsync("OData environment variables", "The selected environment changed. Refresh the environment and try again.");
                    return;
                }
                if (!TryValidateCurrentDeclaration(out span, out _) || buffer.IsReadOnly(span.Span))
                {
                    await VS.MessageBox.ShowErrorAsync("OData environment variables", "The declaration changed or became read-only. Reopen the light bulb and try again.");
                    return;
                }

                string documentPath = session.SavedDocumentPath!;
                var result = await Task.Run(() => ODataEnvironmentFile.MoveVariable(documentPath, selected.Name!, declaration.Name, declaration.Value, token), token);
                if (result.Kind == ODataEnvironmentFileUpdateKind.Conflict || result.Kind == ODataEnvironmentFileUpdateKind.Failed)
                {
                    await VS.MessageBox.ShowErrorAsync("OData environment variables", result.Error ?? "The environment file could not be updated.");
                    return;
                }

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
                selected = await environments.GetSelectedEnvironmentAsync();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
                if (selected?.IsValid != true || !string.Equals(selected.Name, expectedEnvironment, StringComparison.OrdinalIgnoreCase))
                {
                    await VS.MessageBox.ShowErrorAsync("OData environment variables", "The environment file was updated, but the selected environment changed. The declaration was left intact.");
                    return;
                }
                if (!TryValidateCurrentDeclaration(out span, out _, allowProfileReload: true) || buffer.IsReadOnly(span.Span))
                {
                    await VS.MessageBox.ShowErrorAsync("OData environment variables", "The environment file was updated, but the declaration changed or became read-only. The declaration was left intact.");
                    return;
                }
                var history = undoManagers.GetTextBufferUndoManager(buffer).TextBufferUndoHistory;
                using var transaction = history.CreateTransaction("Move OData variable to environment file");
                using var edit = buffer.CreateEdit();
                edit.Delete(span.Span);
                edit.Apply();
                if (edit.Canceled)
                {
                    transaction.Cancel();
                    await VS.MessageBox.ShowErrorAsync("OData environment variables", "The environment file was updated, but the declaration could not be removed.");
                    return;
                }
                transaction.Complete();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await VS.MessageBox.ShowErrorAsync("OData environment variables", ex.Message);
            }
        }

        private bool TryValidateCurrentDeclaration(out SnapshotSpan span, out string error, bool allowProfileReload = false)
        {
            span = default;
            error = "The document changed. Reopen the light bulb and try again.";
            var snapshot = buffer.CurrentSnapshot;
            if (snapshot != originalSnapshot || session.IsLoading && !allowProfileReload || session.SavedDocumentPath == null ||
                !string.Equals(session.SelectedEnvironmentName, expectedEnvironment, StringComparison.OrdinalIgnoreCase))
                return false;
            if (!TryParseDeclaration(snapshot.GetText(), declaration.Line, out var current) ||
                current.Name != declaration.Name || current.Value != declaration.Value || current.Start != declaration.Start)
                return false;
            var line = snapshot.GetLineFromLineNumber(current.Line);
            span = new SnapshotSpan(snapshot, current.SpanStart, current.SpanLength);
            if (line.Start.Position != current.SpanStart)
            {
                error = "The declaration location changed. Reopen the light bulb and try again.";
                return false;
            }
            error = "";
            return true;
        }
    }

    private sealed class ReplaceReferenceAction(SnapshotSpan original, string replacement) : ISuggestedAction
    {
        public string DisplayText => $"Replace with '{replacement}'";
        public bool HasActionSets => false;
        public bool HasPreview => false;
        public string? IconAutomationText => null;
        public string? InputGestureText => null;
        public ImageMoniker IconMoniker => default;
        public Task<object?> GetPreviewAsync(CancellationToken token) => Task.FromResult<object?>(null);
        public Task<IEnumerable<SuggestedActionSet>?> GetActionSetsAsync(CancellationToken token) => Task.FromResult<IEnumerable<SuggestedActionSet>?>(null);
        public bool TryGetTelemetryId(out Guid telemetryId) { telemetryId = Guid.Empty; return false; }
        public void Dispose() { }

        public void Invoke(CancellationToken token)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            token.ThrowIfCancellationRequested();
            var buffer = original.Snapshot.TextBuffer;
            if (buffer.CurrentSnapshot == original.Snapshot) buffer.Replace(original.Span, replacement);
        }
    }
}
