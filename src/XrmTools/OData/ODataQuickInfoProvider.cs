#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

[Export(typeof(IAsyncQuickInfoSourceProvider))]
[Name(nameof(ODataQuickInfoProvider))]
[ContentType(ODataContentType.Name)]
internal sealed class ODataQuickInfoProvider : IAsyncQuickInfoSourceProvider
{
    public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        => new ODataQuickInfoSource(textBuffer);
}

internal sealed class ODataQuickInfoSource(ITextBuffer buffer) : IAsyncQuickInfoSource
{
    private volatile bool disposed;

    public async Task<QuickInfoItem?> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (disposed) return null;
        var snapshot = buffer.CurrentSnapshot;
        var trigger = session.GetTriggerPoint(snapshot);
        if (!trigger.HasValue) return null;
        var line = trigger.Value.GetContainingLine();
        string source = snapshot.GetText();
        var reference = ODataNavigation.FindReference(source, line.LineNumber, trigger.Value.Position - line.Start.Position);
        if (!reference.HasValue) return null;
        var context = ODataBufferSession.Get(buffer);
        var inspection = context != null
            ? await context.InspectAsync(source, reference.Value.Expression, cancellationToken).ConfigureAwait(false)
            : await ODataInspector.InspectAsync(ODataDocument.Parse(source), reference.Value.Expression, null,
                _ => null, _ => { }, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (disposed || inspection == null || !ReferenceEquals(snapshot, buffer.CurrentSnapshot)) return null;
        var span = snapshot.CreateTrackingSpan(line.Start.Position + reference.Value.Column, reference.Value.Length, SpanTrackingMode.EdgeExclusive);
        var content = CreateContent(inspection);
        cancellationToken.ThrowIfCancellationRequested();
        return disposed || !ReferenceEquals(snapshot, buffer.CurrentSnapshot) || inspection.IsCurrent?.Invoke() == false
            ? null : new QuickInfoItem(span, content);
    }

    internal static ContainerElement CreateContent(ODataInspection inspection)
    {
        var elements = new System.Collections.Generic.List<object>
        {
            new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, inspection.Source, ClassifiedTextRunStyle.Bold)),
            new ClassifiedTextElement(new ClassifiedTextRun(inspection.IsValue
                ? PredefinedClassificationTypeNames.String : PredefinedClassificationTypeNames.NaturalLanguage, inspection.Preview))
        };
        if (inspection.Description != null)
            elements.Add(new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.NaturalLanguage,
                System.Environment.NewLine + inspection.Description)));
        return new ContainerElement(ContainerElementStyle.Stacked, elements);
    }

    public void Dispose() => disposed = true;
}
