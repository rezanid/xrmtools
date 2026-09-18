#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Hyperlink = XrmTools.Shell.Controls.Hyperlink;

internal static class ODataContentType
{
    public const string Name = "XrmTools.OData";
    [Export, Name(Name), BaseDefinition("code")]
    internal static readonly ContentTypeDefinition Definition = null!;
    [Export, FileExtension(".odata"), ContentType(Name)]
    internal static readonly FileExtensionToContentTypeDefinition Extension = null!;
}

[Export(typeof(IWpfTextViewMarginProvider))]
[Name("XrmTools.OData.Response"), ContentType(ODataContentType.Name)]
[MarginContainer(PredefinedMarginNames.Bottom), Order(After = PredefinedMarginNames.BottomControl)]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class ODataMarginProvider : IWpfTextViewMarginProvider
{
    [Import] internal ODataExecutionService Execution { get; set; } = null!;
    public IWpfTextViewMargin CreateMargin(IWpfTextViewHost host, IWpfTextViewMargin container)
        {
        var margin = host.TextView.Properties.GetOrCreateSingletonProperty(() => new ODataResponseMargin(host.TextView, Execution));
        return margin.IsRightLayout ? null! : margin;
    }
}

[Export(typeof(IWpfTextViewMarginProvider))]
[Name("XrmTools.OData.Response.Right"), ContentType(ODataContentType.Name)]
[MarginContainer(PredefinedMarginNames.Right)]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class ODataRightMarginProvider : IWpfTextViewMarginProvider
{
    [Import] internal ODataExecutionService Execution { get; set; } = null!;
    public IWpfTextViewMargin CreateMargin(IWpfTextViewHost host, IWpfTextViewMargin container)
    {
        var margin = host.TextView.Properties.GetOrCreateSingletonProperty(() => new ODataResponseMargin(host.TextView, Execution));
        return margin.IsRightLayout ? margin : null!;
    }
}

[Export(typeof(IViewTaggerProvider)), ContentType(ODataContentType.Name)]
[TagType(typeof(InterLineAdornmentTag)), TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class ODataActionProvider : IViewTaggerProvider
{
    [Import] internal ODataExecutionService Execution { get; set; } = null!;
    public ITagger<T>? CreateTagger<T>(ITextView view, ITextBuffer buffer) where T : ITag
    {
        if (buffer != view.TextBuffer || view is not IWpfTextView wpf) return null;
        var margin = view.Properties.GetOrCreateSingletonProperty(() => new ODataResponseMargin(wpf, Execution));
        return view.Properties.GetOrCreateSingletonProperty(() => new ODataActionTagger(wpf, margin)) as ITagger<T>;
    }
}

internal sealed class ODataActionTagger : ITagger<InterLineAdornmentTag>, IDisposable
{
    private readonly IWpfTextView view;
    private readonly ODataResponseMargin margin;
    private readonly Dictionary<int, InterLineAdornmentTag> tags = [];
    private ITextSnapshot snapshot;
    private ODataDocument document;
    private bool disposed;
    public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

    public ODataActionTagger(IWpfTextView view, ODataResponseMargin margin)
    {
        this.view = view;
        this.margin = margin;
        snapshot = view.TextSnapshot;
        document = ODataDocument.Parse(snapshot.GetText());
        view.TextBuffer.Changed += Changed;
        view.Closed += Closed;
        margin.ExecutionStateChanged += ExecutionChanged;
    }

    private void Changed(object sender, TextContentChangedEventArgs e)
    {
        snapshot = e.After;
        document = ODataDocument.Parse(snapshot.GetText());
        tags.Clear();
        TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
    }

    private void ExecutionChanged(object? sender, EventArgs e)
    {
        tags.Clear();
        TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
    }

    public IEnumerable<ITagSpan<InterLineAdornmentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
        if (disposed || spans.Count == 0 || spans[0].Snapshot != snapshot) yield break;
        foreach (var request in document.Requests)
        {
            var line = snapshot.GetLineFromLineNumber(request.Line);
            var span = new SnapshotSpan(line.Start, 0);
            if (!spans.Any(s => s.IntersectsWith(span))) continue;
            if (!tags.TryGetValue(request.Line, out var tag))
            {
                var capturedSnapshot = snapshot;
                var capturedDocument = document;
                tag = new InterLineAdornmentTag((_, _, _) =>
                {
                    var running = margin.IsRequestExecuting(capturedSnapshot, request.Line);
                    var capturedRequestId = running ? margin.ActiveRequestId : null;
                    var link = new Hyperlink(new Run(running ? "Cancel Request" : "Send Request"))
                    {
                        TextDecorations = null,
                        IsEnabled = running || !margin.IsExecuting,
                        ToolTip = running ? "Cancel this request. Work already sent may still complete on the server." :
                            "Send using the selected Xrm Tools environment. No automatic retries."
                    };
                    link.Click += (_, _) =>
                    {
                        ThreadHelper.ThrowIfNotOnUIThread();
                        if (disposed) return;
                        // Cancellation belongs to the dispatched operation, even if the document was edited.
                        if (capturedRequestId.HasValue) { margin.Cancel(capturedRequestId); return; }
                        if (margin.IsExecuting || view.TextSnapshot != capturedSnapshot) return;
                        margin.Send(capturedDocument, request);
                    };
                    var text = new TextBlock { FontSize = 12, Padding = new Thickness(2, 0, 2, 0) };
                    text.Inlines.Add(link);
                    if (request.Error != null) text.ToolTip = request.Error;
                    return text;
                }, true, 15.0, HorizontalPositioningMode.TextRelative, 0.0, null);
                tags.Add(request.Line, tag);
            }
            yield return new TagSpan<InterLineAdornmentTag>(span, tag);
        }
    }

    private void Closed(object sender, EventArgs e) => Dispose();
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        margin.ExecutionStateChanged -= ExecutionChanged;
        view.TextBuffer.Changed -= Changed;
        view.Closed -= Closed;
        tags.Clear();
    }
}
