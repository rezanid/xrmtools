#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[Export(typeof(IAsyncCompletionSourceProvider))]
[Name(nameof(ODataCompletionProvider))]
[ContentType(ODataContentType.Name)]
internal sealed class ODataCompletionProvider : IAsyncCompletionSourceProvider
{
    public IAsyncCompletionSource GetOrCreate(ITextView textView)
        => textView.Properties.GetOrCreateSingletonProperty(() => new ODataCompletionSource(textView));
}

internal sealed class ODataCompletionSource : IAsyncCompletionSource
{
    private readonly ITextView textView;

    public ODataCompletionSource(ITextView textView) => this.textView = textView;

    private static readonly object DescriptionKey = new();
    private static readonly (ImageElement Icon, ImmutableArray<CompletionFilter> Filters) DocumentVariables =
        CreateCategory("Document variables", "V", KnownMonikers.LocalVariable);
    private static readonly (ImageElement Icon, ImmutableArray<CompletionFilter> Filters) BuiltInVariables =
        CreateCategory("Built-in variables", "B", KnownMonikers.Constant);
    private static readonly (ImageElement Icon, ImmutableArray<CompletionFilter> Filters) NamedRequests =
        CreateCategory("Named requests", "N", KnownMonikers.Web);
    private static readonly (ImageElement Icon, ImmutableArray<CompletionFilter> Filters) ResponseMembers =
        (CreateIcon("Response member", KnownMonikers.Property), ImmutableArray<CompletionFilter>.Empty);

    private static (ImageElement Icon, ImmutableArray<CompletionFilter> Filters) CreateCategory(string label, string accessKey, ImageMoniker moniker)
    {
        var icon = CreateIcon(label, moniker);
        return (icon, [new CompletionFilter(label, accessKey, icon)]);
    }

    private static ImageElement CreateIcon(string label, ImageMoniker moniker)
        => new(new ImageId(moniker.Guid, moniker.Id), label);

    public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken cancellationToken)
        => !ODataInlineRename.IsActive(textView) && TryGetSpan(triggerLocation, out var span, out _)
            ? new CompletionStartData(CompletionParticipation.ExclusivelyProvidesItems, span)
            : CompletionStartData.DoesNotParticipateInCompletion;

    public async Task<CompletionContext> GetCompletionContextAsync(
        IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation,
        SnapshotSpan applicableToSpan, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var line = triggerLocation.GetContainingLine();
        int column = triggerLocation.Position - line.Start.Position;
        if (ODataCompletion.TryGetCapturedSpan(line.GetText(), column, out _, out _, out var name, out var path))
        {
            var captures = ODataBufferSession.GetCaptures(triggerLocation.Snapshot.TextBuffer);
            var capture = captures?.Get(name);
            if (capture == null) return CompletionContext.Empty;
            string[] entries = await Task.Run(() => path.Length == 0
                ? capture.Headers.Keys.OrderBy(key => key, System.StringComparer.OrdinalIgnoreCase)
                    .Take(128).Select(key => "." + key).ToArray()
                : capture.GetBodyCompletion(path, cancellationToken), cancellationToken).ConfigureAwait(false);
            if (!ReferenceEquals(captures?.Get(name), capture)) return CompletionContext.Empty;
            return MakeContext(entries.Select(entry => new ODataCompletionItem(entry, "From the captured response.", ODataCompletionKind.ResponseMember)));
        }
        if (!TryGetSpan(triggerLocation, out _, out var qualifier)) return CompletionContext.Empty;

        var document = ODataDocument.Parse(triggerLocation.Snapshot.GetText());
        return MakeContext(ODataCompletion.GetItems(document, qualifier));
    }

    private CompletionContext MakeContext(IEnumerable<ODataCompletionItem> entries)
    {
        var items = entries.Select(entry =>
        {
            var category = entry.Kind switch
            {
                ODataCompletionKind.DocumentVariable => DocumentVariables,
                ODataCompletionKind.BuiltInVariable => BuiltInVariables,
                ODataCompletionKind.NamedRequest => NamedRequests,
                _ => ResponseMembers
            };
            var item = new CompletionItem(entry.Text, this, category.Icon, category.Filters);
            item.Properties.AddProperty(DescriptionKey, entry.Description);
            return item;
        }).ToImmutableArray();
        return items.Length == 0 ? CompletionContext.Empty : new CompletionContext(items);
    }

    public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken cancellationToken)
        => Task.FromResult<object>(item.Properties.GetProperty<string>(DescriptionKey));

    private static bool TryGetSpan(SnapshotPoint point, out SnapshotSpan span, out string qualifier)
    {
        var line = point.GetContainingLine();
        string text = line.GetText();
        int column = point.Position - line.Start.Position;
        qualifier = "";
        if (ODataCompletion.TryGetCapturedSpan(text, column, out int start, out int length, out _, out _) ||
            ODataCompletion.TryGetSpan(text, column, out start, out length, out qualifier))
        {
            span = new SnapshotSpan(point.Snapshot, line.Start.Position + start, length);
            return true;
        }
        span = default;
        return false;
    }
}
