#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows.Input;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Documents;
using System.Collections.Generic;
using XrmTools.Options;
using XrmTools.Shell.Styles;
using Button = XrmTools.Shell.Controls.Button;
using ScrollViewer = XrmTools.Shell.Controls.ScrollViewer;

internal sealed class ODataResponseMargin : Grid, IWpfTextViewMargin
{
    private readonly IWpfTextView view;
    private readonly ODataExecutionService execution;
    private readonly JoinableTaskFactory tasks = ThreadHelper.JoinableTaskContext.CreateFactory(ThreadHelper.JoinableTaskContext.CreateCollection());
    private readonly TextBlock environmentText = new() { Margin = new Thickness(6), VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock status = new() { Text = "Select a request and choose Send Request.", Margin = new Thickness(6), TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock statusCode = new() { FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
    private readonly Border statusBadge = new() { CornerRadius = new CornerRadius(4), Padding = new Thickness(10, 4, 10, 4), Margin = new Thickness(0, 0, 12, 0), Visibility = Visibility.Collapsed };
    private readonly TextBlock time = new() { Text = "--", FontWeight = FontWeights.Medium, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock size = new() { Text = "--", FontWeight = FontWeights.Medium, VerticalAlignment = VerticalAlignment.Center };
    private readonly Button cancel = new() { Content = "Cancel", IsEnabled = false, Margin = new Thickness(4) };
    private readonly Button send = new() { Content = "Send at caret", Margin = new Thickness(4) };
    private readonly TextBox body = ResponseText();
    private readonly DataGrid headers = ResponseHeaders();
    private readonly TextBox requestInfo = ResponseText();
    private readonly RadioButton[] responseTabs = new RadioButton[3];
    private CancellationTokenSource? active;
    private DataverseEnvironment? displayedEnvironment;
    private bool disposed;
    private int environmentRevision;
    // Capture once per text view: both MEF providers and request adornments share this instance.
    internal bool IsRightLayout { get; } = GeneralOptions.Instance.ODataResponseLayout == ODataResponseLayout.Right;

    public ODataResponseMargin(IWpfTextView view, ODataExecutionService execution)
    {
        this.view = view;
        this.execution = execution;
        Visibility = Visibility.Collapsed;
        if (IsRightLayout) { Width = 600; MinWidth = 320; }
        else { Height = 280; MinHeight = 120; }
        Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(
            new Uri("/XrmTools;component/OData/ODataResponseStyles.xaml", UriKind.Relative)));
        SetResourceReference(BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey);
        SetResourceReference(TextElement.FontFamilyProperty, VsFonts.EnvironmentFontFamilyKey);
        SetResourceReference(TextElement.FontSizeProperty, VsFonts.EnvironmentFontSizeKey);
        SetResourceReference(TextElement.ForegroundProperty, EnvironmentColors.ToolWindowTextBrushKey);
        environmentText.SetResourceReference(TextBlock.ForegroundProperty, ShellColors.TextFillSecondaryBrushKey);
        status.SetResourceReference(TextBlock.ForegroundProperty, ShellColors.TextFillPrimaryBrushKey);
        time.ToolTip = "HTTP send and response-body download time; excludes authentication and display formatting.";
        size.ToolTip = "Response body bytes, before text decoding or JSON formatting; excludes headers.";
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(IsRightLayout ? 0 : 5) });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition());
        var resize = new System.Windows.Controls.Primitives.Thumb { Cursor = IsRightLayout ? Cursors.SizeWE : Cursors.SizeNS, HorizontalAlignment = HorizontalAlignment.Stretch };
        var resizeTemplate = new ControlTemplate(typeof(System.Windows.Controls.Primitives.Thumb));
        var resizeBorder = new FrameworkElementFactory(typeof(Border));
        resizeBorder.SetResourceReference(Border.BackgroundProperty, ShellColors.ControlStrokeDefaultBrushKey);
        resizeTemplate.VisualTree = resizeBorder;
        resize.Template = resizeTemplate;
        resize.DragDelta += (_, e) =>
        {
            if (IsRightLayout) Width = Math.Max(320, Math.Min(1600, Width - e.HorizontalChange));
            else Height = Math.Max(120, Math.Min(800, Height - e.VerticalChange));
        };
        Children.Add(resize);
        var toolbar = new DockPanel();
        var actions = new StackPanel { Orientation = Orientation.Horizontal };
        var refresh = new Button { Content = "Refresh environment", Margin = new Thickness(4) };
        actions.Children.Add(send);
        actions.Children.Add(cancel);
        actions.Children.Add(refresh);
        DockPanel.SetDock(actions, Dock.Right);
        toolbar.Children.Add(actions);
        toolbar.Children.Add(environmentText);
        SetRow(toolbar, 1);
        Children.Add(toolbar);
        var summary = new StackPanel();
        var metrics = new StackPanel { Orientation = Orientation.Horizontal };
        statusBadge.Child = statusCode;
        metrics.Children.Add(statusBadge);
        metrics.Children.Add(Metric("Time: ", time));
        metrics.Children.Add(Metric("Size: ", size));
        var metricsBorder = new Border { Padding = new Thickness(10, 6, 10, 6),
            BorderThickness = new Thickness(0, 0, 0, 1), Child = metrics };
        metricsBorder.SetResourceReference(BackgroundProperty, EnvironmentColors.CommandBarGradientBrushKey);
        metricsBorder.SetResourceReference(Border.BorderBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey);
        summary.Children.Add(metricsBorder);
        summary.Children.Add(status);
        SetRow(summary, 2);
        Children.Add(summary);
        var response = new Grid();
        response.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        response.RowDefinitions.Add(new RowDefinition());
        var tabBar = new StackPanel { Orientation = Orientation.Horizontal };
        KeyboardNavigation.SetTabNavigation(tabBar, KeyboardNavigationMode.Once);
        KeyboardNavigation.SetDirectionalNavigation(tabBar, KeyboardNavigationMode.Contained);
        var tabBorder = new Border { Padding = new Thickness(10, 8, 10, 0), Child = tabBar };
        tabBorder.SetResourceReference(BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey);
        response.Children.Add(tabBorder);
        var content = new Grid { Margin = new Thickness(1) };
        var pages = new FrameworkElement[] { ResponseScroll(body), headers, ResponseScroll(requestInfo) };
        var names = new[] { "Body", "Headers", "Raw" };
        for (int i = 0; i < names.Length; i++)
        {
            int index = i;
            var tab = new RadioButton { Content = names[i], Style = (Style)FindResource("ViewTabStyle") };
            responseTabs[i] = tab;
            tab.Checked += (_, _) =>
            {
                for (int page = 0; page < pages.Length; page++)
                    pages[page].Visibility = page == index ? Visibility.Visible : Visibility.Collapsed;
            };
            tab.KeyDown += (_, e) =>
            {
                int delta = e.Key == Key.Right || e.Key == Key.Down ? 1 :
                    e.Key == Key.Left || e.Key == Key.Up ? -1 : 0;
                if (delta == 0) return;
                var next = responseTabs[(index + delta + responseTabs.Length) % responseTabs.Length];
                next.IsChecked = true;
                next.Focus();
                e.Handled = true;
            };
            tabBar.Children.Add(tab);
            content.Children.Add(pages[i]);
        }
        responseTabs[2].ToolTip = "Request and response with credential headers redacted. This is not a wire capture.";
        responseTabs[0].IsChecked = true;
        var contentBorder = new Border { Margin = new Thickness(10, 4, 10, 10),
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Child = content };
        contentBorder.SetResourceReference(BackgroundProperty, EnvironmentColors.CommandBarGradientBrushKey);
        contentBorder.SetResourceReference(Border.BorderBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey);
        SetRow(contentBorder, 1);
        response.Children.Add(contentBorder);
        SetRow(response, 3);
        Children.Add(response);
        response.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Escape) return;
            view.VisualElement.Focus();
            e.Handled = true;
        };
        if (IsRightLayout)
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            ColumnDefinitions.Add(new ColumnDefinition());
            foreach (UIElement child in Children) SetColumn(child, 1);
            SetColumn(resize, 0);
            SetRowSpan(resize, RowDefinitions.Count);
            // Keep actions and environment readable in a narrow side pane.
            DockPanel.SetDock(actions, Dock.Top);
        }
        cancel.Click += (_, _) => active?.Cancel();
        refresh.Click += (_, _) => RefreshEnvironment();
        send.Click += (_, _) =>
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var document = ODataDocument.Parse(view.TextSnapshot.GetText());
            int line = view.Caret.Position.BufferPosition.GetContainingLine().LineNumber;
            var request = document.FindRequestAtLine(line);
            if (request != null) Send(document, request);
            else status.Text = "No request found. Start with GET /WhoAmI.";
        };
        view.GotAggregateFocus += OnViewFocus;
        view.Closed += Closed;
        DataverseEnvironmentProvider.EnvironmentChanged += EnvironmentChanged;
        GeneralOptions.Saved += OptionsSaved;
        RefreshEnvironment();
    }

    public void Send(ODataDocument document, ODataRequest request)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (disposed || active != null) return;
        // Reveal before validation so missing-environment and request errors are visible too.
        Visibility = Visibility.Visible;
        if (displayedEnvironment?.IsValid != true) { status.Text = "Select an environment in Xrm Tools, then refresh the environment."; return; }
        var captured = displayedEnvironment with { };
        var cts = new CancellationTokenSource();
        active = cts;
        send.IsEnabled = false;
        cancel.IsEnabled = true;
        responseTabs[0].IsChecked = true;
        body.Clear(); headers.ItemsSource = null; requestInfo.Clear();
        ShowBadge("Sending", 0);
        time.Text = "--";
        size.Text = "--";
        status.Text = $"Authenticating and sending to {captured.Name ?? captured.Url}…";
        tasks.RunAsync(async () =>
        {
            try
            {
                var result = await execution.SendAsync(document, request, captured, cts.Token);
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (disposed) return;
                responseTabs[0].IsChecked = true;
                ShowBadge(result.Status, result.StatusCode);
                time.Text = ODataResponseFormatter.Time(result.ElapsedMilliseconds);
                size.Text = ODataResponseFormatter.Size(result.ContentByteLength);
                status.Text = result.Request;
                headers.ItemsSource = result.Headers.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.IndexOf(':') is int colon && colon > 0
                        ? new KeyValuePair<string, string>(line.Substring(0, colon), line.Substring(colon + 1).Trim())
                        : new KeyValuePair<string, string>(line, "")).ToArray();
                requestInfo.Text = result.Raw;
                body.Text = PrettyPrint(result.Body);
            }
            catch (OperationCanceledException)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (!disposed) ShowError("Canceled", "Canceled or timed out. A request already sent may still complete on the server.");
            }
            catch (Exception ex)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (!disposed) ShowError("Error", ex.Message);
            }
            finally
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                active = null;
                cts.Dispose();
                if (!disposed) { send.IsEnabled = true; cancel.IsEnabled = false; }
            }
        }).FileAndForget("XrmTools/OData/Send");
    }

    private void RefreshEnvironment()
    {
        tasks.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (disposed) return;
            int revision = ++environmentRevision;
            try
            {
                var environment = await execution.GetEnvironmentAsync();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (disposed || revision != environmentRevision) return;
                displayedEnvironment = environment is null ? null : environment with { };
                environmentText.Text = environment?.IsValid == true ? $"{environment.Name} · {environment.Url}" : "No environment selected";
            }
            catch (Exception)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (disposed || revision != environmentRevision) return;
                displayedEnvironment = null;
                environmentText.Text = "Environment unavailable — refresh to try again";
            }
        }).FileAndForget("XrmTools/OData/Environment");
    }

    private static FrameworkElement Metric(string label, TextBlock value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 16, 0), VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(new TextBlock { Text = label, Opacity = 0.7, VerticalAlignment = VerticalAlignment.Center });
        panel.Children.Add(value);
        return panel;
    }

    private static TextBox ResponseText()
    {
        var text = new TextBox { IsReadOnly = true, IsReadOnlyCaretVisible = false,
            AcceptsReturn = true, TextWrapping = TextWrapping.NoWrap, IsTabStop = false,
            BorderThickness = new Thickness(0), FontFamily = new FontFamily("Consolas"), FontSize = 13 };
        text.SetResourceReference(StyleProperty, "RawTextBoxStyle");
        return text;
    }

    private static ScrollViewer ResponseScroll(TextBox text) => new()
    {
        Content = text, Focusable = true, IsTabStop = true,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
    };

    private static DataGrid ResponseHeaders()
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false, IsReadOnly = true, CanUserAddRows = false,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            CanUserDeleteRows = false, CanUserResizeRows = false, HeadersVisibility = DataGridHeadersVisibility.Column,
            SelectionMode = DataGridSelectionMode.Single, SelectionUnit = DataGridSelectionUnit.Cell,
            ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            FontFamily = new FontFamily("Consolas"), FontSize = 13
        };
        KeyboardNavigation.SetTabNavigation(grid, KeyboardNavigationMode.Once);
        grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding("Key"), Width = new DataGridLength(200) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Value", Binding = new Binding("Value"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        return grid;
    }

    private static string PrettyPrint(string value)
    {
        try { using var json = JsonDocument.Parse(value); return JsonSerializer.Serialize(json.RootElement, new JsonSerializerOptions { WriteIndented = true }); }
        catch (JsonException) { return value; }
    }

    private void ShowBadge(string text, int code)
    {
        statusBadge.Visibility = Visibility.Visible;
        statusCode.Text = text;
        // Match the native HTTP editor's badge palette, with system colors in high contrast.
        statusBadge.Background = SystemParameters.HighContrast ? SystemColors.HighlightBrush :
            new SolidColorBrush(code >= 400 ? Color.FromRgb(205, 51, 51) : code >= 200 ? Color.FromRgb(46, 139, 87) : Color.FromRgb(128, 128, 128));
        statusCode.Foreground = SystemParameters.HighContrast ? SystemColors.HighlightTextBrush : Brushes.White;
    }

    private void ShowError(string label, string message)
    {
        responseTabs[0].IsChecked = true;
        ShowBadge(label, label == "Error" ? 500 : 0);
        time.Text = "--";
        size.Text = "--";
        status.Text = message;
        body.Text = message;
        requestInfo.Text = "=== " + label.ToUpperInvariant() + " ===" + Environment.NewLine + message;
    }
    private void OnViewFocus(object sender, EventArgs e) => RefreshEnvironment();
    private void EnvironmentChanged(DataverseEnvironment environment) => RefreshEnvironment();
    private void OptionsSaved(GeneralOptions options) => RefreshEnvironment();
    private void Closed(object sender, EventArgs e) => Dispose();
    public FrameworkElement VisualElement => this;
    public double MarginSize => Visibility == Visibility.Collapsed ? 0 : IsRightLayout ? ActualWidth : ActualHeight;
    public bool Enabled => !disposed;
    public ITextViewMargin? GetTextViewMargin(string name) => name == (IsRightLayout ? "XrmTools.OData.Response.Right" : "XrmTools.OData.Response") ? this : null;
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        active?.Cancel();
        view.GotAggregateFocus -= OnViewFocus;
        view.Closed -= Closed;
        DataverseEnvironmentProvider.EnvironmentChanged -= EnvironmentChanged;
        GeneralOptions.Saved -= OptionsSaved;
    }
}
