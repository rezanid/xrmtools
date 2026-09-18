#nullable enable
namespace XrmTools.FetchXml;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using XrmTools.FetchXml.Margin;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi;
using XrmTools.Xrm.Repositories;
using Hyperlink = XrmTools.Shell.Controls.Hyperlink;

[Export(typeof(IViewTaggerProvider)), ContentType(FetchXmlContentTypeDefinitions.ContentTypeName)]
[TagType(typeof(InterLineAdornmentTag)), TextViewRole(PredefinedTextViewRoles.Debuggable)]
internal sealed class FetchXmlActionProvider : IViewTaggerProvider
{
    [Import] internal IWebApiService WebApi { get; set; } = null!;
    [Import] internal IRepositoryFactory Repositories { get; set; } = null!;
    [Import] internal ILogger<FetchXmlActionProvider> Logger { get; set; } = null!;
    public ITagger<T>? CreateTagger<T>(ITextView view, ITextBuffer buffer) where T : ITag
    {
        if (view.TextBuffer != buffer || view is not IWpfTextView) return null;
        var margin = view.Properties.GetOrCreateSingletonProperty(() => new BrowserMargin(view, WebApi, Repositories, Logger));
        return view.Properties.GetOrCreateSingletonProperty(() => new FetchXmlActionTagger(view, margin)) as ITagger<T>;
    }
}

/// <summary>A single action for the document's fetch root, using the preview's execution command.</summary>
internal sealed class FetchXmlActionTagger : ITagger<InterLineAdornmentTag>, IDisposable
{
    private readonly ITextView view;
    private readonly BrowserMargin margin;
    private ITextSnapshot snapshot;
    private int? position;
    private InterLineAdornmentTag? tag;
    private bool disposed;
    public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

    internal FetchXmlActionTagger(ITextView view, BrowserMargin margin)
    {
        this.view = view;
        this.margin = margin;
        snapshot = view.TextSnapshot;
        UpdatePosition();
        view.TextBuffer.Changed += Changed;
        view.Closed += Closed;
        margin.ExecutionStateChanged += ExecutionChanged;
    }

    private void UpdatePosition()
    {
        position = FetchXmlExecutionInput.FindActionPosition(snapshot.GetText());
        if (position.HasValue)
            position = snapshot.GetLineFromPosition(position.Value).Start.Position;
        tag = null;
    }

    private void Changed(object sender, TextContentChangedEventArgs e)
    {
        snapshot = e.After;
        UpdatePosition();
        Invalidate();
    }

    private void ExecutionChanged(object? sender, EventArgs e) { tag = null; Invalidate(); }
    private void Invalidate() => TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));

    public IEnumerable<ITagSpan<InterLineAdornmentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
        if (disposed || !position.HasValue || spans.Count == 0 || spans[0].Snapshot != snapshot) yield break;
        var span = new SnapshotSpan(snapshot, position.Value, 0);
        if (!spans.Any(s => s.IntersectsWith(span))) yield break;
        if (tag == null)
        {
            tag = new InterLineAdornmentTag((_, _, _) =>
            {
                var link = new Hyperlink(new Run(margin.IsExecuting ? "Cancel query" : "Execute query"))
                { TextDecorations = null, ToolTip = "Execute the FetchXML document using the selected Xrm Tools environment." };
                link.Click += (_, _) =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    if (!disposed) margin.ExecuteOrCancel();
                };
                var text = new TextBlock { FontSize = 12, Padding = new System.Windows.Thickness(2, 0, 2, 0) };
                text.Inlines.Add(link);
                return text;
            }, true, 15.0, HorizontalPositioningMode.TextRelative, 0.0, null);
        }
        yield return new TagSpan<InterLineAdornmentTag>(span, tag);
    }

    private void Closed(object sender, EventArgs e) => Dispose();
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        view.TextBuffer.Changed -= Changed;
        view.Closed -= Closed;
        margin.ExecutionStateChanged -= ExecutionChanged;
    }
}
