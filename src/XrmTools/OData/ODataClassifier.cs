#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

[Export(typeof(IClassifierProvider))]
[ContentType(ODataContentType.Name)]
internal sealed class ODataClassifierProvider : IClassifierProvider
{
    [Import] internal IClassificationTypeRegistryService ClassificationRegistry { get; set; } = null!;

    public IClassifier GetClassifier(ITextBuffer textBuffer)
        => textBuffer.Properties.GetOrCreateSingletonProperty(() => new ODataClassifier(textBuffer, ClassificationRegistry));
}

internal sealed class ODataClassifier : IClassifier
{
    private readonly object gate = new();
    private readonly Dictionary<ODataSyntaxKind, IClassificationType> types;
    private ITextSnapshot? cachedSnapshot;
    private IReadOnlyList<ODataSyntaxSpan> cachedSpans = Array.Empty<ODataSyntaxSpan>();

    public ODataClassifier(ITextBuffer buffer, IClassificationTypeRegistryService registry)
    {
        types = new()
        {
            [ODataSyntaxKind.Heading] = registry.GetClassificationType(PredefinedClassificationTypeNames.PreprocessorKeyword),
            [ODataSyntaxKind.Comment] = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment),
            [ODataSyntaxKind.NameDirective] = registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword),
            [ODataSyntaxKind.VariableDeclaration] = registry.GetClassificationType(PredefinedClassificationTypeNames.Identifier),
            [ODataSyntaxKind.VariableReference] = registry.GetClassificationType(PredefinedClassificationTypeNames.Identifier),
            [ODataSyntaxKind.HttpMethod] = registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword),
            [ODataSyntaxKind.RequestTarget] = registry.GetClassificationType(PredefinedClassificationTypeNames.String),
            [ODataSyntaxKind.HttpVersion] = registry.GetClassificationType(PredefinedClassificationTypeNames.Number),
            [ODataSyntaxKind.HeaderName] = registry.GetClassificationType(PredefinedClassificationTypeNames.Identifier)
        };
        buffer.Changed += Changed;
    }

    public event EventHandler<ClassificationChangedEventArgs>? ClassificationChanged;

    private void Changed(object sender, TextContentChangedEventArgs e)
    {
        lock (gate)
        {
            cachedSnapshot = null;
            cachedSpans = Array.Empty<ODataSyntaxSpan>();
        }
        ClassificationChanged?.Invoke(this, new ClassificationChangedEventArgs(new SnapshotSpan(e.After, 0, e.After.Length)));
    }

    public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
    {
        var result = new List<ClassificationSpan>();
        if (span.Length == 0) return result;
        IReadOnlyList<ODataSyntaxSpan> syntax;
        lock (gate)
        {
            if (cachedSnapshot != span.Snapshot)
            {
                cachedSpans = ODataSyntax.Scan(span.Snapshot.GetText());
                cachedSnapshot = span.Snapshot;
            }
            syntax = cachedSpans;
        }

        int low = 0, high = syntax.Count;
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            if (syntax[middle].End <= span.Start.Position) low = middle + 1;
            else high = middle;
        }
        for (int i = low; i < syntax.Count && syntax[i].Start < span.End.Position; i++)
        {
            var token = syntax[i];
            int start = Math.Max(token.Start, span.Start.Position);
            int end = Math.Min(token.End, span.End.Position);
            result.Add(new ClassificationSpan(new SnapshotSpan(span.Snapshot, start, end - start), types[token.Kind]));
        }
        return result;
    }
}
