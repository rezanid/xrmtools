namespace XrmTools.Classifiers;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.Text;
using System.Linq;

[Export(typeof(IClassifierProvider))]
[ContentType("CSharp")]
internal class XrmStringClassifierProvider : IClassifierProvider
{
    [Import]
    internal IClassificationTypeRegistryService ClassificationRegistry = null;

    public IClassifier GetClassifier(ITextBuffer textBuffer)
    {
        return textBuffer.Properties.GetOrCreateSingletonProperty(() => new XrmStringClassifier(ClassificationRegistry));
    }
}

internal class XrmStringClassifier(IClassificationTypeRegistryService registry) : IClassifier
{
    private const int CacheCapacity = 100;

    private readonly IClassificationType _specialStringType = registry.GetClassificationType(PredefinedClassificationTypeNames.MarkupAttribute);
    private Microsoft.CodeAnalysis.Document document;
    private readonly (SnapshotSpan span, List<ClassificationSpan> classifications)[] lastSpans
        = new (SnapshotSpan span, List<ClassificationSpan> classifications)[CacheCapacity];
    private int lastIndex = 0;

    public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

    public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
    {
        var snapshot = span.Snapshot;
        var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();

        if (document == null)
        {
            return [];
        }
        var lastspan = lastSpans.FirstOrDefault(s => s.span == span);
        if (this.document?.Id == document.Id && lastspan != default)
        {
            return lastspan.classifications;
        }

        var classifications = new List<ClassificationSpan>();
        var text = span.GetText();

        // Regular expression to match the Step attribute with three arguments
        var match = Regex.Match(text, @"\[(?:Image)\(.+?,\s*""([^""]+)""|\[(?:Step)\(.+?,.+?,\s*""([^""]+)""");

        if (match.Success && match.Groups.Count > 1)
        {
            // Extract the third argument, which is the comma-delimited field list
            var group = match.Groups[0].Value.StartsWith("[Image") ? match.Groups[1] : match.Groups[2];
            var fieldList = group.Value;
            var start = group.Index;

            // Split the fields by comma and apply highlighting to each field
            var fields = fieldList.Split(',');
            foreach (var field in fields)
            {
                var trimmedField = field.Trim();
                var fieldStart = text.IndexOf(trimmedField, start);
                if (fieldStart >= 0)
                {
                    var fieldSpan = new SnapshotSpan(span.Snapshot, span.Start + fieldStart, trimmedField.Length);
                    classifications.Add(new ClassificationSpan(fieldSpan,   _specialStringType));
                    start = fieldStart + trimmedField.Length;
                }
            }
        }

        this.document = document;
        lastSpans[lastIndex] = (span, classifications);
        lastIndex = (lastIndex + 1) % CacheCapacity;

        return classifications;
    }
}
