#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

[Export(typeof(ITaggerProvider))]
[ContentType(ODataContentType.Name)]
[TagType(typeof(IOutliningRegionTag))]
internal sealed class ODataOutliningProvider : ITaggerProvider
{
    public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag
        => buffer.Properties.GetOrCreateSingletonProperty(() => new ODataOutliningTagger(buffer)) as ITagger<T>;
}

internal sealed class ODataOutliningTagger : ITagger<IOutliningRegionTag>
{
    private readonly object gate = new();
    private ITextSnapshot? snapshot;
    private IReadOnlyList<ITagSpan<IOutliningRegionTag>> regions = [];

    public ODataOutliningTagger(ITextBuffer buffer)
    {
        buffer.Changed += (_, args) =>
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(args.After, 0, args.After.Length)));
    }

    public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

    public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
        if (spans.Count == 0) yield break;
        foreach (var region in GetRegions(spans[0].Snapshot))
            if (spans.IntersectsWith(region.Span)) yield return region;
    }

    private IReadOnlyList<ITagSpan<IOutliningRegionTag>> GetRegions(ITextSnapshot current)
    {
        lock (gate)
        {
            if (snapshot == current) return regions;

            var result = new List<ITagSpan<IOutliningRegionTag>>();
            var document = ODataDocument.Parse(current.GetText());
            foreach (var request in document.Requests)
            {
                var section = current.GetLineFromLineNumber(request.StartLine);
                int visibleLine = ODataSyntax.IsHeading(section.GetText().Trim()) ? request.StartLine : request.Line;
                int lastLine = Math.Min(request.EndLine, current.LineCount) - 1;
                while (lastLine > visibleLine && string.IsNullOrWhiteSpace(current.GetLineFromLineNumber(lastLine).GetText()))
                    lastLine--;

                if (lastLine <= visibleLine) continue;
                var start = current.GetLineFromLineNumber(visibleLine).End;
                var end = current.GetLineFromLineNumber(lastLine).End;
                var tag = new OutliningRegionTag(isDefaultCollapsed: false, isImplementation: false, " …", "Collapsed OData request");
                result.Add(new TagSpan<IOutliningRegionTag>(new SnapshotSpan(start, end), tag));
            }

            snapshot = current;
            regions = result;
            return regions;
        }
    }
}
