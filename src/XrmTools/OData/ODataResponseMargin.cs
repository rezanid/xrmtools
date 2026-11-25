#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.Win32;
using System.IO;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows.Input;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel;
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
using ContextMenu = XrmTools.Shell.Controls.ContextMenu;
using MenuItem = XrmTools.Shell.Controls.MenuItem;
using ScrollViewer = XrmTools.Shell.Controls.ScrollViewer;

internal sealed class ODataResponseMargin : Grid, IWpfTextViewMargin
{
    private readonly IWpfTextView view;
    private readonly ODataExecutionService execution;
    private readonly ODataBufferSession session;
    private readonly ODataExecutionCoordinator coordinator;
    private readonly JoinableTaskFactory tasks = ThreadHelper.JoinableTaskContext.CreateFactory(ThreadHelper.JoinableTaskContext.CreateCollection());
    private readonly TextBlock environmentText = new() { Margin = new Thickness(6), VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly TextBlock status = new() { Text = "Select a request and choose Send Request.", Margin = new Thickness(6), TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock statusCode = new() { FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
    private readonly Border statusBadge = new() { CornerRadius = new CornerRadius(4), Padding = new Thickness(10, 4, 10, 4), Margin = new Thickness(0, 0, 12, 0), Visibility = Visibility.Collapsed };
    private readonly TextBlock time = new() { Text = "--", FontWeight = FontWeights.Medium, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock size = new() { Text = "--", FontWeight = FontWeights.Medium, VerticalAlignment = VerticalAlignment.Center };
    private readonly Button cancel = new() { Content = "Cancel", IsEnabled = false, Margin = new Thickness(4) };
    private readonly Button send = new() { Content = "Send at caret", Margin = new Thickness(4) };
    private readonly Button runDocument = new() { Content = "Run Document", Margin = new Thickness(4) };
    private readonly DataGrid results = ResultTable();
    private readonly RowDefinition resultRow = new() { Height = new GridLength(0) };
    private readonly RowDefinition splitterRow = new() { Height = new GridLength(0) };
    private readonly GridSplitter resultSplitter = new() { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, ResizeDirection = GridResizeDirection.Rows, ResizeBehavior = GridResizeBehavior.PreviousAndNext, Visibility = Visibility.Collapsed };
    private double resultHeight = 60;
    private bool updatingResults;
    private bool selectionPinned;
    private ODataExecutionResult? displayedResult;
    private ResultItem[] resultItems = [];
    private ODataExecutionRow? renderedRow;
    private ODataResponse? renderedResponse;
    private bool renderedBodyAvailable;
    private string? renderedState;
    private string? renderedMessage;
    private Guid? trackedRequestId;
    private readonly TextBox body = ResponseText();
    private readonly DockPanel bodyPage = new();
    private readonly Button saveBody = new() { Content = "Save full body", Visibility = Visibility.Collapsed };
    private IWpfTextViewHost? jsonHost;
    private ODataStoredBody? displayedBody;
    private readonly DataGrid headers = ResponseHeaders();
    private readonly TextBox requestInfo = ResponseText();
    private readonly RadioButton[] responseTabs = new RadioButton[3];
    private ITrackingPoint? activeRequestPosition;
    internal Guid? ActiveRequestId => coordinator.ActiveId;
    internal bool IsExecuting => coordinator.IsExecuting;
    internal event EventHandler? ExecutionStateChanged;

    internal bool IsRequestExecuting(ITextSnapshot snapshot, int line) =>
        !coordinator.IsDocumentRun && activeRequestPosition != null &&
        activeRequestPosition.GetPoint(snapshot).GetContainingLine().LineNumber == line;

    internal void Cancel(Guid? requestId)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (!disposed) coordinator.Cancel(requestId);
    }
    private DataverseEnvironment? displayedEnvironment;
    private bool environmentUnavailable;
    private bool disposed;
    private int environmentRevision;
    private double sideWidth = 600;
    private double bottomHeight = 280;
    internal ODataMarginPlacement Placement { get; }
    private Community.VisualStudio.Toolkit.InfoBar? configurationNotice;
    private string? configurationNoticeKey;
    private int configurationNoticeRevision;
    private const string OpenEnvironmentFileText = "Open environment file";
    // Capture once per text view: both MEF providers and request adornments share this instance.
    internal bool IsRightLayout { get; private set; } = GeneralOptions.Instance.ODataResponseLayout == ODataResponseLayout.Right;

    public ODataResponseMargin(IWpfTextView view, ODataExecutionService execution)
    {
        this.view = view;
        this.execution = execution;
        string? filePath = view.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDocument)
            ? textDocument.FilePath : null;
        session = ODataBufferSession.Attach(view.TextBuffer, filePath);
        coordinator = session.GetCoordinator(execution);
        coordinator.Changed += CoordinatorChanged;
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
        actions.Children.Add(runDocument);
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
        results.PreviewMouseLeftButtonDown += (_, e) =>
        {
            if (e.OriginalSource is not DependencyObject source ||
                ItemsControl.ContainerFromElement(results, source) is not DataGridRow row) return;
            selectionPinned = true;
            if (row.IsSelected)
            {
                ShowSelectedResult();
                NavigateToSelectedRequest();
            }
        };
        results.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Home || e.Key == Key.End ||
                e.Key == Key.PageUp || e.Key == Key.PageDown || e.Key == Key.Space) selectionPinned = true;
        };
        results.SelectionChanged += (_, _) =>
        {
            if (updatingResults) return;
            selectionPinned = true;
            ShowSelectedResult();
            NavigateToSelectedRequest();
        };
        SetRow(summary, 2);
        Children.Add(summary);
        var resultContent = new Grid();
        resultContent.RowDefinitions.Add(resultRow);
        resultContent.RowDefinitions.Add(splitterRow);
        resultContent.RowDefinitions.Add(new RowDefinition());
        resultContent.Children.Add(results);
        resultSplitter.SetResourceReference(BackgroundProperty, ShellColors.ControlStrokeDefaultBrushKey);
        resultSplitter.DragCompleted += (_, _) => { if (resultRow.ActualHeight > 0) resultHeight = resultRow.ActualHeight; };
        SetRow(resultSplitter, 1);
        resultContent.Children.Add(resultSplitter);
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
        DockPanel.SetDock(saveBody, Dock.Top);
        saveBody.Click += SaveBody;
        bodyPage.Children.Add(saveBody);
        bodyPage.Children.Add(ResponseScroll(body));
        var pages = new FrameworkElement[] { bodyPage, headers, ResponseScroll(requestInfo) };
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
        SetRow(response, 2);
        resultContent.Children.Add(response);
        SetRow(resultContent, 3);
        Children.Add(resultContent);
        response.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Escape) return;
            view.VisualElement.Focus();
            e.Handled = true;
        };
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(IsRightLayout ? 5 : 0) });
        ColumnDefinitions.Add(new ColumnDefinition());
        foreach (UIElement child in Children) SetColumn(child, 1);
        if (IsRightLayout)
        {
            SetColumn(resize, 0);
            SetRowSpan(resize, RowDefinitions.Count);
            DockPanel.SetDock(actions, Dock.Top);
        }
        Placement = new ODataMarginPlacement(this, IsRightLayout, right =>
        {
            if (IsRightLayout) sideWidth = Width; else bottomHeight = Height;
            IsRightLayout = right;
            MinWidth = right ? 320 : 0;
            MinHeight = right ? 0 : 120;
            Width = right ? sideWidth : double.NaN;
            Height = right ? double.NaN : bottomHeight;
            RowDefinitions[0].Height = new GridLength(right ? 0 : 5);
            ColumnDefinitions[0].Width = new GridLength(right ? 5 : 0);
            resize.Cursor = right ? Cursors.SizeWE : Cursors.SizeNS;
            SetColumn(resize, right ? 0 : 1);
            SetRowSpan(resize, right ? RowDefinitions.Count : 1);
            DockPanel.SetDock(actions, right ? Dock.Top : Dock.Right);
        });
        ContextMenu = LayoutMenu(() => Placement.IsRight, () => Placement.CanSwitch, Placement.Toggle);
        cancel.Click += (_, _) => Cancel(ActiveRequestId);
        runDocument.Click += (_, _) => RunDocument();
        refresh.Click += (_, _) => RefreshEnvironment();
        send.Click += (_, _) =>
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var document = ODataDocument.Parse(view.TextSnapshot.GetText());
            int line = view.Caret.Position.BufferPosition.GetContainingLine().LineNumber;
            var request = document.FindRequestAtLine(line);
            if (request != null) Send(document, request);
            else ShowError("Error", "No request found. Start with GET /WhoAmI.");
        };
        view.GotAggregateFocus += OnViewFocus;
        view.Closed += Closed;
        session.VariablesChanged += VariablesChanged;
        DataverseEnvironmentProvider.EnvironmentChanged += EnvironmentChanged;
        GeneralOptions.Saved += OptionsSaved;
        if (coordinator.Result != null) UpdateExecution();
        RefreshEnvironment();
    }

    public void Send(ODataDocument document, ODataRequest request)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (disposed || coordinator.IsExecuting) return;
        Visibility = Visibility.Visible;
        if (!TrySnapshot(out var captured, out var effectiveDocument)) return;
        var effectiveRequest = effectiveDocument.FindRequestAtLine(request.Line);
        if (effectiveRequest == null) { ShowError("Error", "The request changed. Select it again and send."); return; }
        if (coordinator.TryStartManual(effectiveDocument, effectiveRequest, captured))
        {
            session.ClearRunNavigation();
            activeRequestPosition = view.TextSnapshot.CreateTrackingPoint(
                view.TextSnapshot.GetLineFromLineNumber(effectiveRequest.Line).Start.Position, PointTrackingMode.Positive);
            trackedRequestId = coordinator.ActiveId;
            UpdateExecution();
        }
    }

    internal void RunDocument()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (disposed || coordinator.IsExecuting) return;
        Visibility = Visibility.Visible;
        var sourceSnapshot = view.TextSnapshot;
        if (!TrySnapshot(out var captured, out var document)) return;
        if (document.Requests.Count == 0) { ShowError("Error", "No requests found. Start with GET /WhoAmI."); return; }
        var writes = document.Requests.Where(r => !string.Equals(r.Method, "GET", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(r.Method, "HEAD", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(r.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase)).ToArray();
        string writeList = writes.Length == 0 ? "No write methods." : string.Join(Environment.NewLine, writes.Select(r => $"{r.Method} {r.Target}"));
        var dialog = new ODataRunConfirmation(captured, document.Requests.Count, writeList);
        if (dialog.ShowModal() != true) return;
        if (coordinator.TryStartDocument(document, captured) && coordinator.Result is ODataExecutionResult result)
            session.SetRunNavigation(result, sourceSnapshot);
    }

    private bool TrySnapshot(out DataverseEnvironment environment, out ODataDocument document)
    {
        environment = displayedEnvironment!;
        document = null!;
        if (displayedEnvironment?.IsValid != true)
        {
            ShowError("Error", "Select an environment in Xrm Tools, then refresh the environment.");
            return false;
        }
        environment = displayedEnvironment with { };
        if (session.TryCreateDocument(view.TextSnapshot.GetText(), environment.Name, out document, out var error)) return true;
        ShowError("Error", error ?? "Environment variables are not ready. Refresh and try again.");
        return false;
    }

    internal sealed class ResultItem : INotifyPropertyChanged
    {
        public ODataExecutionRow Row { get; }
        public ResultItem(ODataExecutionRow row) => Row = row;
        public event PropertyChangedEventHandler? PropertyChanged;
        public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        public string Method => Row.Request.Method;
        public string Name => !string.IsNullOrEmpty(Row.Request.Name) ? Row.Request.Name :
            !string.IsNullOrEmpty(Row.Request.RequestName) ? Row.Request.RequestName : Row.Request.Target;
        public string Elapsed => Row.State == "Pending" || Row.State == "Skipped" ? "--" : ODataResponseFormatter.Time(Row.ElapsedMilliseconds);
        public string Outcome => Row.Response == null ? Row.State :
            $"{Row.State} ({(Row.BodyAvailable ? Row.Response.Status : Row.Response.StatusCode.ToString())})";
        public ImageMoniker Icon => Row.State == "Succeeded" ? KnownMonikers.StatusOK : Row.State == "Failed" ? KnownMonikers.StatusError : default;
        public Visibility IconVisibility => Row.State == "Succeeded" || Row.State == "Failed" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void NavigateToSelectedRequest()
    {
        if (displayedResult == null || results.SelectedItem is not ResultItem item) return;
        var point = session.GetRunNavigation(displayedResult, item.Row.Id, view.TextSnapshot);
        if (!point.HasValue) return;
        view.Caret.MoveTo(point.Value);
        view.ViewScroller.EnsureSpanVisible(new SnapshotSpan(point.Value, 0));
    }

    private void CoordinatorChanged(object? sender, EventArgs e)
    {
        tasks.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (disposed) return;
            UpdateExecution();
        }).FileAndForget("XrmTools/OData/ExecutionProgress");
    }

    private void UpdateExecution()
    {
        var result = coordinator.Result;
        if (result == null) return;
        Visibility = Visibility.Visible;
        if (!ReferenceEquals(displayedResult, result))
        {
            displayedResult = result;
            selectionPinned = false;
            trackedRequestId = null;
            renderedRow = null;
            resultItems = result.IsDocument ? result.Rows.Select(row => new ResultItem(row)).ToArray() : [];
            updatingResults = true;
            results.ItemsSource = resultItems;
            updatingResults = false;
            responseTabs[0].IsChecked = true;
        }
        if (coordinator.IsExecuting && !result.IsDocument && trackedRequestId != coordinator.ActiveId)
        {
            trackedRequestId = coordinator.ActiveId;
            int line = result.Rows[0].Request.Line;
            activeRequestPosition = line < view.TextSnapshot.LineCount
                ? view.TextSnapshot.CreateTrackingPoint(view.TextSnapshot.GetLineFromLineNumber(line).Start.Position, PointTrackingMode.Positive) : null;
        }
        if (!coordinator.IsExecuting) { activeRequestPosition = null; trackedRequestId = null; }
        send.IsEnabled = !coordinator.IsExecuting;
        runDocument.IsEnabled = !coordinator.IsExecuting;
        cancel.IsEnabled = coordinator.IsExecuting;
        cancel.Content = result.IsDocument ? "Cancel Run" : "Cancel";
        bool showingTable = results.Visibility == Visibility.Visible;
        if (result.IsDocument != showingTable)
        {
            if (showingTable && resultRow.ActualHeight > 0) resultHeight = resultRow.ActualHeight;
            results.Visibility = result.IsDocument ? Visibility.Visible : Visibility.Collapsed;
            resultSplitter.Visibility = results.Visibility;
            resultRow.Height = new GridLength(result.IsDocument ? resultHeight : 0);
            splitterRow.Height = new GridLength(result.IsDocument ? 5 : 0);
        }
        status.Visibility = result.IsDocument ? Visibility.Collapsed : Visibility.Visible;
        UpdateEnvironmentLabel();
        if (result.IsDocument)
        {
            var follow = result.Rows.LastOrDefault(row => row.State != "Pending" && row.State != "Skipped") ?? result.Rows.FirstOrDefault();
            updatingResults = true;
            foreach (var item in resultItems) item.Refresh();
            if (!selectionPinned)
            {
                var selected = resultItems.FirstOrDefault(item => item.Row.Id == follow?.Id);
                if (!ReferenceEquals(results.SelectedItem, selected))
                {
                    results.SelectedItem = selected;
                    if (selected != null) results.ScrollIntoView(selected);
                }
            }
            updatingResults = false;
        }
        ShowSelectedResult();
        ExecutionStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ShowSelectedResult()
    {
        var result = displayedResult;
        var row = result?.IsDocument == true ? (results.SelectedItem as ResultItem)?.Row : result?.Rows.FirstOrDefault();
        if (row == null) return;
        status.Visibility = result?.IsDocument == true ? Visibility.Collapsed : Visibility.Visible;
        var response = row.Response;
        if (ReferenceEquals(renderedRow, row) && ReferenceEquals(renderedResponse, response) &&
            renderedBodyAvailable == row.BodyAvailable && renderedState == row.State && renderedMessage == row.Message) return;
        renderedRow = row;
        renderedResponse = response;
        renderedBodyAvailable = row.BodyAvailable;
        renderedState = row.State;
        renderedMessage = row.Message;
        if (response == null)
        {
            headers.ItemsSource = null;
            string message = row.State == "Canceled" ? "Canceled or timed out. A request already sent may still complete on the server." :
                row.Message ?? (row.State == "Running" ? "Authenticating and sending…" : row.State);
            ShowError(row.State, message);
            if (result?.IsDocument == true) status.Visibility = Visibility.Collapsed;
            return;
        }
        ShowBadge(row.BodyAvailable ? response.Status : response.StatusCode.ToString(), response.StatusCode);
        time.Text = ODataResponseFormatter.Time(response.ElapsedMilliseconds);
        size.Text = ODataResponseFormatter.Size(response.ContentByteLength);
        status.Text = response.Request;
        headers.ItemsSource = response.Headers.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.IndexOf(':') is int colon && colon > 0
                ? new KeyValuePair<string, string>(line.Substring(0, colon), line.Substring(colon + 1).Trim())
                : new KeyValuePair<string, string>(line, "")).ToArray();
        requestInfo.Text = row.BodyAvailable ? response.Raw : "The raw response is no longer available (retention limit reached).";
        if (!row.BodyAvailable) { SetBody("The response body is no longer available (retention limit reached).", false, null); return; }
        string preview = response.IsJson && response.StoredBody == null ? PrettyPrint(response.Body) : response.Body;
        SetBody(preview.Length <= ODataTransport.PreviewLimit ? preview : response.Body, response.IsJson, response.StoredBody);
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
                var selected = environment is null ? null : environment with { };
                await session.SetEnvironmentAsync(selected);
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (disposed || revision != environmentRevision) return;
                displayedEnvironment = selected;
                environmentUnavailable = false;
                UpdateEnvironmentLabel();
            }
            catch (Exception)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (disposed || revision != environmentRevision) return;
                displayedEnvironment = null;
                environmentUnavailable = true;
                _ = session.SetEnvironmentAsync(null);
                UpdateEnvironmentLabel();
            }
        }).FileAndForget("XrmTools/OData/Environment");
    }

    private void UpdateEnvironmentLabel()
    {
        environmentText.Text = EnvironmentLabel(displayedEnvironment, environmentUnavailable, session.IsLoading, session.ConfigurationError, session.ProfileWarning, displayedResult);
        string? detail = session.ConfigurationError ?? session.ProfileWarning;
        environmentText.ToolTip = detail == null ? environmentText.Text : environmentText.Text + Environment.NewLine + detail;
        UpdateConfigurationNotice();
    }

    internal static string EnvironmentLabel(DataverseEnvironment? environment, bool unavailable, bool loading, string? error, string? warning, ODataExecutionResult? result)
    {
        bool valid = environment?.IsValid == true;
        string text = valid ? environment!.Name ?? environment.Url! :
            unavailable ? "Environment unavailable — refresh to try again" : "No environment selected";
        if (result?.IsDocument == true)
        {
            bool differentTarget = !valid || environment!.Name != result.Environment.Name || environment.ConnectionString != result.Environment.ConnectionString;
            if (differentTarget) text += $" · Run target: {result.Environment.Name ?? result.Environment.Url}";
            int finished = result.Rows.Count(row => row.State != "Pending" && row.State != "Running");
            int succeeded = result.Rows.Count(row => row.State == "Succeeded");
            int failed = result.Rows.Count(row => row.State == "Failed");
            int skipped = result.Rows.Count(row => row.State == "Skipped");
            text += $" — {finished}/{result.Rows.Count} finished · {succeeded} succeeded · {failed} failed · {skipped} skipped";
            if (!string.IsNullOrEmpty(result.StopReason)) text += " · " + result.StopReason;
            if (valid && environment!.Name != null) text += $" · {environment.Url}";
            if (differentTarget && result.Environment.Name != null) text += $" · Run URL: {result.Environment.Url}";
        }
        else if (valid && environment!.Name != null) text += $" · {environment.Url}";
        if (loading) text += " · Loading environment variables…";
        else if (error != null) text += " · Environment variable configuration error";
        else if (warning != null) text += " · No matching variable profile";
        return text;
    }

    private void UpdateConfigurationNotice()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (disposed || session.IsLoading) return;
        if (session.ConfigurationNoticeOwner == null) session.ConfigurationNoticeOwner = this;
        if (!ReferenceEquals(session.ConfigurationNoticeOwner, this)) return;

        string? error = session.ConfigurationError;
        string? warning = session.ProfileWarning;
        string? path = session.EnvironmentFilePath;
        string? key = error == null && warning == null ? null : path + "\n" + error + "\n" + warning;
        if (key == configurationNoticeKey) return;
        configurationNoticeKey = key;
        int revision = ++configurationNoticeRevision;
        CloseConfigurationNotice();
        if (error == null && warning == null) return;

        tasks.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (disposed || revision != configurationNoticeRevision) return;
            string message = error != null
                ? $"Environment variables are unavailable because {(path == null ? "the environment file" : Path.GetFileName(path))} is invalid: {error} "
                : $"Environment variables are unavailable: {warning} ";
            var spans = new List<InfoBarTextSpan> { new(message) };
            if (path != null) spans.Add(new InfoBarHyperlink(OpenEnvironmentFileText));
            var model = new InfoBarModel(spans, Microsoft.VisualStudio.Imaging.KnownMonikers.StatusWarning, isCloseButtonVisible: true);
            var notice = await view.CreateInfoBarAsync(model);
            if (notice == null) return;
            if (disposed || revision != configurationNoticeRevision) { notice.Close(); return; }
            notice.ActionItemClicked += (_, args) =>
            {
                if (path == null || args.ActionItem is not InfoBarHyperlink { Text: OpenEnvironmentFileText }) return;
                tasks.RunAsync(async () =>
                {
                    // Let the click finish before opening another document.
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(alwaysYield: true);
                    if (File.Exists(path)) await Community.VisualStudio.Toolkit.VS.Documents.OpenAsync(path);
                    else await Community.VisualStudio.Toolkit.VS.StatusBar.ShowMessageAsync("The environment file no longer exists.");
                }).FileAndForget("XrmTools/OData/OpenEnvironmentFile");
            };
            configurationNotice = notice;
            if (!await notice.TryShowInfoBarUIAsync() && ReferenceEquals(configurationNotice, notice)) configurationNotice = null;
        }).FileAndForget("XrmTools/OData/ConfigurationNotice");
    }

    private void CloseConfigurationNotice()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        configurationNotice?.Close();
        configurationNotice = null;
    }

    private static FrameworkElement Metric(string label, TextBlock value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 16, 0), VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(new TextBlock { Text = label, Opacity = 0.7, VerticalAlignment = VerticalAlignment.Center });
        panel.Children.Add(value);
        return panel;
    }

    internal static DataGrid ResultTable()
    {
        var grid = new DataGrid
        {
            Visibility = Visibility.Collapsed, Margin = new Thickness(6, 0, 6, 0), AutoGenerateColumns = false,
            IsReadOnly = true, CanUserAddRows = false, CanUserDeleteRows = false, CanUserSortColumns = false,
            CanUserResizeRows = false, SelectionMode = DataGridSelectionMode.Single, SelectionUnit = DataGridSelectionUnit.FullRow,
            HeadersVisibility = DataGridHeadersVisibility.Column, GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        var image = new FrameworkElementFactory(typeof(CrispImage));
        image.SetValue(WidthProperty, 16.0);
        image.SetValue(HeightProperty, 16.0);
        image.SetBinding(CrispImage.MonikerProperty, new Binding(nameof(ResultItem.Icon)));
        image.SetBinding(VisibilityProperty, new Binding(nameof(ResultItem.IconVisibility)));
        grid.Columns.Add(new DataGridTemplateColumn { Header = "", Width = 32, CellTemplate = new DataTemplate { VisualTree = image } });
        grid.Columns.Add(new DataGridTextColumn { Header = "Method", Binding = new Binding(nameof(ResultItem.Method)), Width = 70 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Request", Binding = new Binding(nameof(ResultItem.Name)), Width = new DataGridLength(1, DataGridLengthUnitType.Star), MinWidth = 120 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new Binding(nameof(ResultItem.Outcome)), Width = 150 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Time", Binding = new Binding(nameof(ResultItem.Elapsed)), Width = 90 });
        return grid;
    }

    internal static TextBox ResponseText()
    {
        var text = new TextBox { IsReadOnly = true, IsReadOnlyCaretVisible = false,
            AcceptsReturn = true, TextWrapping = TextWrapping.NoWrap, IsTabStop = false,
            BorderThickness = new Thickness(0), FontFamily = new FontFamily("Consolas"), FontSize = 13 };
        text.SetResourceReference(StyleProperty, "RawTextBoxStyle");
        text.ContextMenu = ClipboardMenu(text);
        return text;
    }

    internal static ContextMenu LayoutMenu(Func<bool> isRight, Func<bool> canSwitch, Action toggle)
    {
        var menu = new ContextMenu();
        var move = new MenuItem();
        menu.Items.Add(move);
        menu.Opened += (_, _) =>
        {
            move.Header = isRight() ? "Move response below document" : "Move response to side";
            move.IsEnabled = canSwitch();
        };
        move.Click += (_, _) => toggle();
        return menu;
    }

    private static ContextMenu ClipboardMenu(UIElement target)
    {
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = "_Copy", Command = ApplicationCommands.Copy, CommandTarget = target, InputGestureText = "Ctrl+C" });
        menu.Items.Add(new MenuItem { Header = "Select _All", Command = ApplicationCommands.SelectAll, CommandTarget = target, InputGestureText = "Ctrl+A" });
        return menu;
    }

    private static ScrollViewer ResponseScroll(TextBox text) => new()
    {
        Content = text, Focusable = true, IsTabStop = true,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
    };

    internal static DataGrid ResponseHeaders()
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
        grid.ContextMenu = ClipboardMenu(grid);
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
        renderedRow = null;
        ShowBadge(label, label == "Error" || label == "Failed" ? 500 : 0);
        time.Text = "--";
        size.Text = "--";
        status.Text = message;
        status.Visibility = Visibility.Visible;
        headers.ItemsSource = null;
        SetBody(message, false, null);
        requestInfo.Text = "=== " + label.ToUpperInvariant() + " ===" + Environment.NewLine + message;
    }
    private void OnViewFocus(object sender, EventArgs e) => RefreshEnvironment();
    private void VariablesChanged(object? sender, EventArgs e)
    {
        tasks.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!disposed) UpdateEnvironmentLabel();
        }).FileAndForget("XrmTools/OData/EnvironmentVariables");
    }
    private void EnvironmentChanged(DataverseEnvironment environment)
    {
        _ = session.SetEnvironmentAsync(environment);
        RefreshEnvironment();
    }
    private void OptionsSaved(GeneralOptions options) => RefreshEnvironment();
    private void Closed(object sender, EventArgs e) => Dispose();

    private void SetBody(string text, bool json, ODataStoredBody? stored)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        displayedBody?.Release();
        displayedBody = stored?.Retain();
        saveBody.Visibility = stored != null ? Visibility.Visible : Visibility.Collapsed;
        saveBody.Content = "Preview truncated · Save full body";
        if (bodyPage.Children.Count > 1) bodyPage.Children.RemoveAt(1);
        jsonHost?.Close();
        jsonHost = null;
        if (json && text.Length > 0 && Package.GetGlobalService(typeof(SComponentModel)) is IComponentModel services &&
            services.GetService<IContentTypeRegistryService>().GetContentType("JSON") is IContentType jsonType)
        {
            var buffer = services.GetService<ITextBufferFactoryService>().CreateTextBuffer(text, jsonType);
            using var edit = buffer.CreateReadOnlyRegionEdit();
            edit.CreateReadOnlyRegion(new Microsoft.VisualStudio.Text.Span(0, buffer.CurrentSnapshot.Length), SpanTrackingMode.EdgeInclusive, EdgeInsertionMode.Deny);
            edit.Apply();
            var factory = services.GetService<ITextEditorFactoryService>();
            var roles = factory.CreateTextViewRoleSet(PredefinedTextViewRoles.Analyzable, PredefinedTextViewRoles.Interactive);
            jsonHost = factory.CreateTextViewHost(factory.CreateTextView(buffer, roles), setFocus: false);
            EnableJsonCopy(jsonHost.TextView, services.GetService<IEditorOperationsFactoryService>());
            bodyPage.Children.Add(jsonHost.HostControl);
        }
        else
        {
            body.Text = text;
            bodyPage.Children.Add(ResponseScroll(body));
        }
    }

    internal static void EnableJsonCopy(IWpfTextView textView, IEditorOperationsFactoryService factory)
    {
        var operations = factory.GetEditorOperations(textView);
        var element = textView.VisualElement;
        var copy = new MenuItem { Header = "_Copy", InputGestureText = "Ctrl+C" };
        var selectAll = new MenuItem { Header = "Select _All", InputGestureText = "Ctrl+A" };
        var menu = new ContextMenu();
        menu.Items.Add(copy);
        menu.Items.Add(selectAll);
        menu.Opened += (_, _) => copy.IsEnabled = !textView.Selection.IsEmpty;
        copy.Click += (_, _) => operations.CopySelection();
        selectAll.Click += (_, _) => operations.SelectAll();
        element.ContextMenu = menu;
        element.AddHandler(UIElement.PreviewMouseRightButtonUpEvent, new MouseButtonEventHandler((_, args) =>
        {
            args.Handled = true;
            menu.PlacementTarget = element;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            menu.IsOpen = true;
        }), handledEventsToo: true);

        element.PreviewKeyDown += (_, args) =>
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            if (args.Key == Key.C)
            {
                args.Handled = true;
                if (!textView.Selection.IsEmpty) operations.CopySelection();
            }
            else if (args.Key == Key.A)
            {
                args.Handled = true;
                operations.SelectAll();
            }
        };
    }

    private async void SaveBody(object sender, RoutedEventArgs args)
    {
        var dialog = new SaveFileDialog { FileName = "response", Filter = "All files (*.*)|*.*" };
        if (displayedBody == null || dialog.ShowDialog() != true || displayedBody == null) return;
        try
        {
            using var input = displayedBody.Open();
            using var output = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None, 65536, useAsync: true);
            await input.CopyToAsync(output);
        }
        catch (Exception ex) { if (!disposed) status.Text = "Could not save the response: " + ex.Message; }
    }
    public FrameworkElement VisualElement => this;
    public double MarginSize => Visibility == Visibility.Collapsed ? 0 : IsRightLayout ? ActualWidth : ActualHeight;
    public bool Enabled => !disposed;
    public ITextViewMargin? GetTextViewMargin(string name) => name == (IsRightLayout ? "XrmTools.OData.Response.Right" : "XrmTools.OData.Response") ? this : null;
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        jsonHost?.Close();
        displayedBody?.Release();
        coordinator.Changed -= CoordinatorChanged;
        session.VariablesChanged -= VariablesChanged;
        configurationNoticeRevision++;
        CloseConfigurationNotice();
        if (ReferenceEquals(session.ConfigurationNoticeOwner, this)) session.ConfigurationNoticeOwner = null;
        session.Dispose();
        view.GotAggregateFocus -= OnViewFocus;
        view.Closed -= Closed;
        DataverseEnvironmentProvider.EnvironmentChanged -= EnvironmentChanged;
        GeneralOptions.Saved -= OptionsSaved;
    }
}

internal sealed class ODataRunConfirmation : XrmTools.Shell.Controls.DialogWindow
{
    public ODataRunConfirmation(DataverseEnvironment environment, int requestCount, string writeList)
    {
        Title = "Run Document";
        Width = 620;
        MinWidth = 500;
        SizeToContent = SizeToContent.Height;
        Padding = new Thickness(24, 12, 24, 12);
        var footer = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var run = new Button { Content = "_Run", Kind = ButtonKind.Accent, MinWidth = 100, IsDefault = true };
        var cancel = new Button { Content = "Cancel", MinWidth = 100, IsCancel = true, Margin = new Thickness(8, 0, 0, 0) };
        run.Click += (_, _) => DialogResult = true;
        footer.Children.Add(run);
        footer.Children.Add(cancel);
        FooterContent = footer;
        var content = new StackPanel();
        content.Children.Add(new XrmTools.Shell.Controls.TextBlock { Text = $"Environment: {environment.Name} · {environment.Url}", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) });
        content.Children.Add(new XrmTools.Shell.Controls.TextBlock { Text = $"Requests: {requestCount}", Margin = new Thickness(0, 0, 0, 16) });
        content.Children.Add(new XrmTools.Shell.Controls.TextBlock { Text = "Write methods", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 8) });
        content.Children.Add(new ScrollViewer
        {
            MaxHeight = 240,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = new XrmTools.Shell.Controls.TextBlock { Text = writeList, TextWrapping = TextWrapping.Wrap }
        });
        content.Children.Add(new XrmTools.Shell.Controls.TextBlock { Text = "Requests run in order and stop on failure. Continue?", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 16, 0, 0) });
        Content = content;
    }
}
