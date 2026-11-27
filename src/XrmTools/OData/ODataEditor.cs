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
    [Export, Name(Name), BaseDefinition("code++")]
    internal static readonly ContentTypeDefinition Definition = null!;
    [Export, FileExtension(".odata"), ContentType(Name)]
    internal static readonly FileExtensionToContentTypeDefinition Extension = null!;
}

[Export(typeof(IWpfTextViewCreationListener))]
[ContentType(ODataContentType.Name)]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class ODataEditorOptions : IWpfTextViewCreationListener
{
    [Import] private ITextDocumentFactoryService DocumentFactory { get; set; } = null!;

    public void TextViewCreated(IWpfTextView textView)
    {
        textView.Options.SetOptionValue(DefaultTextViewOptions.ShowErrorSquigglesId, true);

        // This option belongs to Visual Studio's generic word completion provider.
        // Apply it only to this view and only when the provider is installed.
        const string wordSuggestions = "WordBasedSuggestions";
        if (textView.Options.IsOptionDefined(wordSuggestions, localScopeOnly: false))
            textView.Options.SetOptionValue(wordSuggestions, false);

        string? filePath = DocumentFactory.TryGetTextDocument(textView.TextBuffer, out var document)
            ? document.FilePath : null;
        var session = ODataBufferSession.Attach(textView.TextBuffer, filePath);
        bool waitingForDocument = false;
        void DocumentCreated(object sender, TextDocumentEventArgs args)
        {
            if (args.TextDocument?.TextBuffer != textView.TextBuffer) return;
            DocumentFactory.TextDocumentCreated -= DocumentCreated;
            waitingForDocument = false;
            session.SetDocumentPath(args.TextDocument.FilePath);
        }
        void Closed(object sender, EventArgs args)
        {
            textView.Closed -= Closed;
            if (waitingForDocument) DocumentFactory.TextDocumentCreated -= DocumentCreated;
            session.Dispose();
        }
        textView.Closed += Closed;
        if (filePath == null)
        {
            waitingForDocument = true;
            DocumentFactory.TextDocumentCreated += DocumentCreated;
        }
    }
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
        return new ODataMarginHost(margin.Placement, false);
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
        return new ODataMarginHost(margin.Placement, true);
    }
}

internal sealed class ODataMarginPlacement
{
    private readonly FrameworkElement content;
    private readonly Action<bool> configure;
    private ODataMarginHost? bottom;
    private ODataMarginHost? right;
    internal bool IsRight { get; private set; }
    internal bool CanSwitch => bottom != null && right != null;

    internal ODataMarginPlacement(FrameworkElement content, bool isRight, Action<bool> configure)
    {
        this.content = content;
        IsRight = isRight;
        this.configure = configure;
    }

    internal void Attach(ODataMarginHost host, bool isRight)
    {
        if (isRight) right = host; else bottom = host;
        host.Visibility = isRight == IsRight ? Visibility.Visible : Visibility.Collapsed;
        if (isRight == IsRight) host.Children.Add(content);
    }

    internal void Detach(ODataMarginHost host, bool isRight)
    {
        host.Children.Remove(content);
        if (isRight && ReferenceEquals(right, host)) right = null;
        if (!isRight && ReferenceEquals(bottom, host)) bottom = null;
    }

    internal void Toggle()
    {
        if (!CanSwitch) return;
        var oldHost = IsRight ? right! : bottom!;
        var newHost = IsRight ? bottom! : right!;
        oldHost.Children.Remove(content);
        oldHost.Visibility = Visibility.Collapsed;
        IsRight = !IsRight;
        configure(IsRight);
        newHost.Children.Add(content);
        newHost.Visibility = Visibility.Visible;
    }
}

internal sealed class ODataMarginHost : Grid, IWpfTextViewMargin
{
    private readonly ODataMarginPlacement placement;
    private readonly bool isRight;
    private bool disposed;

    internal ODataMarginHost(ODataMarginPlacement placement, bool isRight)
    {
        this.placement = placement;
        this.isRight = isRight;
        placement.Attach(this, isRight);
    }

    public FrameworkElement VisualElement => this;
    public double MarginSize => Visibility != Visibility.Visible ? 0 : isRight ? ActualWidth : ActualHeight;
    public bool Enabled => !disposed;
    public ITextViewMargin? GetTextViewMargin(string name) =>
        name == (isRight ? "XrmTools.OData.Response.Right" : "XrmTools.OData.Response") ? this : null;
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        placement.Detach(this, isRight);
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
                    if (ReferenceEquals(request, capturedDocument.Requests.FirstOrDefault()))
                    {
                        text.Inlines.Add(new Run("   "));
                        var run = new Hyperlink(new Run("Run Document"))
                        {
                            TextDecorations = null,
                            IsEnabled = !margin.IsExecuting,
                            ToolTip = "Review the environment and write requests before running the document in order."
                        };
                        run.Click += (_, _) => { if (!disposed && !margin.IsExecuting) margin.RunDocument(); };
                        text.Inlines.Add(run);
                    }
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
