#nullable enable
namespace XrmTools.Shell.Controls;

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

/// <summary>A lazily hosted VS editor with native classification, fonts and theme updates.</summary>
public sealed class ReadOnlyCodeView : ContentControl, IDisposable
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(ReadOnlyCodeView), new PropertyMetadata("", TextChanged));
    public static readonly DependencyProperty ContentTypeProperty = DependencyProperty.Register(nameof(ContentType), typeof(string), typeof(ReadOnlyCodeView), new PropertyMetadata("JSON", ContentTypeChanged));
    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(ReadOnlyCodeView), new PropertyMetadata(TextWrapping.Wrap, WrappingChanged));
    private readonly TextBox fallback;
    private IWpfTextViewHost? host;
    private IContentTypeRegistryService? contentTypes;
    private bool disposed;

    static ReadOnlyCodeView() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ReadOnlyCodeView), new FrameworkPropertyMetadata(typeof(ReadOnlyCodeView)));

    public ReadOnlyCodeView()
    {
        fallback = new TextBox
        {
            IsReadOnly = true, AcceptsReturn = true, FontFamily = new FontFamily("Consolas"),
            TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, BorderThickness = new Thickness(0), CornerRadius = new CornerRadius(0)
        };
        fallback.SetResourceReference(BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey);
        Content = fallback;
        Loaded += OnLoaded;
        // A hosted editor is not an IVsCodeWindow. Handle the viewer's read-only clipboard commands explicitly.
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (_, e) => { CopySelection(); e.Handled = true; },
            (_, e) => { e.CanExecute = SelectionLength > 0; e.Handled = true; }));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, (_, e) => { SelectAll(); e.Handled = true; }));
        PreviewKeyDown += OnPreviewKeyDown;
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = "Copy", Command = ApplicationCommands.Copy, CommandTarget = this, InputGestureText = "Ctrl+C" });
        menu.Items.Add(new MenuItem { Header = "Select All", Command = ApplicationCommands.SelectAll, CommandTarget = this, InputGestureText = "Ctrl+A" });
        ContextMenu = menu;
    }

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value ?? ""); }
    public string ContentType { get => (string)GetValue(ContentTypeProperty); set => SetValue(ContentTypeProperty, value); }
    public TextWrapping TextWrapping { get => (TextWrapping)GetValue(TextWrappingProperty); set => SetValue(TextWrappingProperty, value); }
    internal int SelectionLength => host?.TextView.Selection.StreamSelectionSpan.SnapshotSpan.Length ?? fallback.SelectionLength;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (host != null || disposed || System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
        try
        {
            if (Package.GetGlobalService(typeof(SComponentModel)) is not IComponentModel components) return;
            InitializeEditor(components.GetService<ITextEditorFactoryService>(), components.GetService<ITextBufferFactoryService>(), components.GetService<IContentTypeRegistryService>());
        }
        catch (Exception ex)
        {
            // Optional editor services must not prevent the record from being read or copied.
            ActivityLog.TryLogWarning("XrmTools.ReadOnlyCodeView", ex.Message);
        }
    }

    internal void InitializeEditor(ITextEditorFactoryService editors, ITextBufferFactoryService buffers, IContentTypeRegistryService types)
    {
        if (host != null || disposed) return;
        contentTypes = types;
        var type = types.GetContentType(ContentType) ?? types.GetContentType("text");
        if (type == null) return; // Keep the selectable, Shell-themed fallback when the language service is unavailable.
        var buffer = buffers.CreateTextBuffer(Text, type);
        var roles = editors.CreateTextViewRoleSet(PredefinedTextViewRoles.Document, PredefinedTextViewRoles.ChangePreview, PredefinedTextViewRoles.Interactive);
        var view = editors.CreateTextView(buffer, roles);
        try
        {
            view.Options.SetOptionValue(DefaultTextViewOptions.ViewProhibitUserInputId, true);
            view.Options.SetOptionValue(DefaultTextViewOptions.WordWrapStyleId, TextWrapping == TextWrapping.NoWrap ? WordWrapStyles.None : WordWrapStyles.WordWrap);
            view.Options.SetOptionValue(DefaultTextViewHostOptions.LineNumberMarginId, false);
            view.Options.SetOptionValue(DefaultTextViewHostOptions.GlyphMarginId, false);
            view.Options.SetOptionValue(DefaultTextViewHostOptions.SuggestionMarginId, false);
            host = editors.CreateTextViewHost(view, false);
            Content = host.HostControl;
        }
        catch
        {
            view.Close();
            throw;
        }
    }

    private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ReadOnlyCodeView)d;
        if (control.disposed) return;
        var text = e.NewValue as string ?? "";
        control.fallback.Text = text;
        var buffer = control.host?.TextView.TextBuffer;
        // Identical snapshots do not reset the caret, selection or scroll position.
        if (buffer != null && buffer.CurrentSnapshot.GetText() != text)
            buffer.Replace(new Span(0, buffer.CurrentSnapshot.Length), text);
    }

    private static void ContentTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ReadOnlyCodeView)d;
        var type = control.contentTypes?.GetContentType((string)e.NewValue) ?? control.contentTypes?.GetContentType("text");
        if (!control.disposed && type != null) control.host?.TextView.TextBuffer.ChangeContentType(type, null);
    }

    private static void WrappingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ReadOnlyCodeView)d;
        control.fallback.TextWrapping = (TextWrapping)e.NewValue;
        if (!control.disposed) control.host?.TextView.Options.SetOptionValue(DefaultTextViewOptions.WordWrapStyleId,
            (TextWrapping)e.NewValue == TextWrapping.NoWrap ? WordWrapStyles.None : WordWrapStyles.WordWrap);
    }

    public bool FindNext(string query)
    {
        if (disposed || string.IsNullOrEmpty(query)) return false;
        var view = host?.TextView;
        var text = view?.TextSnapshot.GetText() ?? Text;
        var start = view?.Selection.StreamSelectionSpan.SnapshotSpan.End.Position ?? fallback.SelectionStart + fallback.SelectionLength;
        int index = text.IndexOf(query, Math.Min(start, text.Length), StringComparison.OrdinalIgnoreCase);
        if (index < 0) index = text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (index < 0) return false;
        if (view != null)
        {
            var span = new SnapshotSpan(view.TextSnapshot, index, query.Length);
            view.VisualElement.Focus();
            view.Caret.MoveTo(span.Start);
            view.Selection.Select(span, false);
            view.ViewScroller.EnsureSpanVisible(span);
        }
        else
        {
            fallback.Focus();
            fallback.Select(index, query.Length);
            int line = fallback.GetLineIndexFromCharacterIndex(index);
            if (line >= 0) fallback.ScrollToLine(line);
        }
        return true;
    }

    private void CopySelection()
    {
        try
        {
            var selected = host?.TextView.Selection.StreamSelectionSpan.SnapshotSpan.GetText() ?? fallback.SelectedText;
            if (selected.Length > 0) Clipboard.SetText(selected);
        }
        catch (System.Runtime.InteropServices.ExternalException) { /* Clipboard temporarily owned by another process. */ }
    }

    private void SelectAll()
    {
        if (host == null) fallback.SelectAll();
        else host.TextView.Selection.Select(new SnapshotSpan(host.TextView.TextSnapshot, 0, host.TextView.TextSnapshot.Length), false);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control) return;
        if (e.Key == Key.C) { CopySelection(); e.Handled = true; }
        else if (e.Key == Key.A) { SelectAll(); e.Handled = true; }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        Loaded -= OnLoaded;
        PreviewKeyDown -= OnPreviewKeyDown;
        host?.Close();
        host = null;
        contentTypes = null;
        Content = null;
    }
}
