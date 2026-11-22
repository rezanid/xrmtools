#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
    private ITextSnapshot? snapshot;
    private IReadOnlyList<ODataDiagnostic> diagnostics = [];

    private ODataDiagnosticTagger(ITextBuffer buffer)
    {
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
            if (snapshot != current)
            {
                diagnostics = ODataDiagnostics.Analyze(current.GetText());
                snapshot = current;
            }
            return diagnostics;
        }
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
    public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
        => new ODataSuggestedActions(textBuffer);
}

internal sealed class ODataSuggestedActions : ISuggestedActionsSource
{
    private readonly ODataDiagnosticTagger tagger;
    public ODataSuggestedActions(ITextBuffer buffer)
    {
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

    public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet categories, SnapshotSpan range, CancellationToken token)
        => Task.FromResult(Fixes(range, token).Any());

    public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet categories, SnapshotSpan range, CancellationToken token)
    {
        var actions = Fixes(range, token)
            .Select(diagnostic => new ReplaceReferenceAction(new SnapshotSpan(range.Snapshot, diagnostic.Start, diagnostic.Length), diagnostic.Replacement!))
            .ToArray();
        return actions.Length == 0 ? [] : [new SuggestedActionSet(PredefinedSuggestedActionCategoryNames.CodeFix, actions)];
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
