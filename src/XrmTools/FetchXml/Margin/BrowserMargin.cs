namespace XrmTools.FetchXml.Margin;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using XrmTools.Core.Repositories;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;
using XrmTools.Shell.Styles;
using XrmTools.WebApi;
using XrmTools.WebApi.Methods;
using XrmTools.Xrm.Repositories;
using ShellButton = XrmTools.Shell.Controls.Button;
using ShellContextMenu = XrmTools.Shell.Controls.ContextMenu;
using ShellDataGrid = XrmTools.Shell.Controls.DataGrid;
using ShellDataGridTextColumn = XrmTools.Shell.Controls.DataGridTextColumn;
using ShellMenuItem = XrmTools.Shell.Controls.MenuItem;
using ShellProgressControl = XrmTools.Shell.Controls.ProgressControl;
using ShellTextBlock = XrmTools.Shell.Controls.TextBlock;

internal class BrowserMargin : DockPanel, IWpfTextViewMargin
{
    private readonly IWebApiService webApi;
    private readonly IRepositoryFactory repositoryFactory;
    private readonly FetchXmlDocument document;
    private readonly ITextView textView;
    private readonly Dictionary<DataGridColumn, int> columnIndexes = [];
    private ShellDataGrid resultsGrid = null!;
    private ShellTextBlock statusText = null!;
    private ShellButton actionButton = null!;
    private ShellProgressControl progressIndicator = null!;
    private CancellationTokenSource? activeFetchCts;
    private Guid? activeRequestId;
    private bool isDisposed;
    private CancellationTokenSource? pendingFetchCts;
    private Border statusBadge = null!;
    private TextBlock statusLabel = null!;
    private TextBox errorDetails = null!;
    private bool hasResults;
    private bool explicitlyShown;
    private bool previewEnabled = FetchXmlOptions.Instance.EnableFetchXmlPreviewWindow;
    internal FetchXmlPreviewLocation PreviewLocation { get; } = FetchXmlOptions.Instance.PreviewWindowLocation;
    internal bool IsExecuting => activeFetchCts is { IsCancellationRequested: false };
    internal event EventHandler? ExecutionStateChanged;

    public BrowserMargin(ITextView textView, IWebApiService webApi, IRepositoryFactory repositoryFactory, ILogger logger)
    {
        this.webApi = webApi ?? throw new ArgumentNullException(nameof(webApi));
        this.repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
        this.textView = textView ?? throw new ArgumentNullException(nameof(textView));
        document = textView.TextBuffer.GetFetchXmlDocument(logger);

        Visibility = previewEnabled ? Visibility.Visible : Visibility.Collapsed;
        SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);

        CreateMarginControls(CreateResultsView());

        Loaded += OnLoaded;
        textView.Closed += OnViewClosed;
        document.Parsed += UpdateResults;
        FetchXmlOptions.Saved += OptionsSaved;
    }

    public FrameworkElement VisualElement => this;

    public double MarginSize => Visibility == Visibility.Collapsed ? 0 :
        PreviewLocation == FetchXmlPreviewLocation.Vertical ? ActualWidth : ActualHeight;

    public bool Enabled => !isDisposed;

    private void OnViewClosed(object sender, EventArgs e) => Dispose();

    public void Dispose()
    {
        if (isDisposed) return;

        isDisposed = true;
        Loaded -= OnLoaded;
        textView.Closed -= OnViewClosed;
        document.Parsed -= UpdateResults;
        FetchXmlOptions.Saved -= OptionsSaved;
        resultsGrid.Sorting -= ResultsGridSorting;
        actionButton.Click -= ActionButtonClick;

        activeFetchCts?.Cancel();
        pendingFetchCts?.Cancel();
        activeFetchCts = null;
        activeRequestId = null;
    }

    public async Task RefreshAsync()
    {
        var options = await FetchXmlOptions.GetLiveInstanceAsync();
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (isDisposed) return;
        if (previewEnabled != options.EnableFetchXmlPreviewWindow) explicitlyShown = false;
        previewEnabled = options.EnableFetchXmlPreviewWindow;
        Visibility = previewEnabled || explicitlyShown ? Visibility.Visible : Visibility.Collapsed;
    }

    public void TriggerFetch(bool immediate = true) => ScheduleFetch(immediate ? 0 : 350);

    public ITextViewMargin GetTextViewMargin(string marginName) => marginName ==
        (PreviewLocation == FetchXmlPreviewLocation.Vertical ? nameof(PreviewMarginVerticalProvider) :
        nameof(PreviewMarginHorizontalProvider)) ? this : null;

    private FrameworkElement CreateResultsView()
    {
        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var toolbar = new DockPanel { Margin = Spacings.S };
        statusText = new ShellTextBlock
        {
            Text = "Time: -- · Records: --",
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
        };
        statusText.SetResourceReference(TextBlock.ForegroundProperty, ShellColors.TextFillPrimaryBrushKey);
        actionButton = new ShellButton
        {
            Content = "Execute",
            Kind = ButtonKind.Standard,
            Margin = Spacings.LeftS,
            MinWidth = 72,
        };
        actionButton.Click += ActionButtonClick;
        DockPanel.SetDock(actionButton, Dock.Right);

        progressIndicator = new ShellProgressControl
        {
            Height = Sizes.IconS,
            Width = Sizes.IconS,
            IsRunning = false,
            Kind = ProgressKind.RingIndeterminate,
            Margin = Spacings.LeftS,
            RingDiameter = Sizes.IconS,
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = Visibility.Collapsed,
        };
        DockPanel.SetDock(progressIndicator, Dock.Right);

        toolbar.Children.Add(actionButton);
        toolbar.Children.Add(progressIndicator);
        statusLabel = new TextBlock { FontWeight = FontWeights.SemiBold };
        statusBadge = new Border { CornerRadius = new CornerRadius(4), Padding = new Thickness(10, 4, 10, 4),
            Margin = new Thickness(0, 0, 12, 0), VerticalAlignment = VerticalAlignment.Center, Child = statusLabel };
        DockPanel.SetDock(statusBadge, Dock.Left);
        toolbar.Children.Add(statusBadge);
        toolbar.Children.Add(statusText);
        SetStatus("Ready", 0);
        root.Children.Add(toolbar);

        errorDetails = new TextBox { IsReadOnly = true, TextWrapping = TextWrapping.Wrap,
            BorderThickness = new Thickness(0), Margin = Spacings.S, MaxHeight = 140,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Visibility = Visibility.Collapsed };
        errorDetails.SetResourceReference(Control.BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey);
        errorDetails.SetResourceReference(Control.ForegroundProperty, EnvironmentColors.ToolWindowTextBrushKey);
        Grid.SetRow(errorDetails, 1);
        root.Children.Add(errorDetails);

        resultsGrid = new ShellDataGrid
        {
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserReorderColumns = true,
            CanUserResizeColumns = true,
            CanUserResizeRows = false,
            CanUserSortColumns = true,
            ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Extended,
            SelectionUnit = DataGridSelectionUnit.FullRow,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
        resultsGrid.SetResourceReference(Control.ForegroundProperty, ShellColors.TextFillPrimaryBrushKey);
        resultsGrid.Sorting += ResultsGridSorting;
        resultsGrid.ContextMenu = CreateResultsContextMenu();
        Grid.SetRow(resultsGrid, 2);
        root.Children.Add(resultsGrid);

        return root;
    }

    private ShellContextMenu CreateResultsContextMenu()
    {
        var menu = new ShellContextMenu();
        menu.Items.Add(new ShellMenuItem
        {
            Header = "Copy selected rows",
            Command = ApplicationCommands.Copy,
            CommandTarget = resultsGrid,
        });
        return menu;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        if (FetchXmlOptions.Instance.RunQueryOnDocumentOpen && document.XmlDocument is not null && !document.IsParsing)
        {
            ScheduleFetch(200);
        }
    }

    private void ActionButtonClick(object sender, RoutedEventArgs e) => ExecuteOrCancel();

    internal void ExecuteOrCancel()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (isDisposed) return;
        explicitlyShown = true;
        Visibility = Visibility.Visible;
        if (IsExecuting)
        {
            pendingFetchCts?.Cancel();
            activeFetchCts?.Cancel();
            return;
        }
        ScheduleFetch(0);
    }

    private void OptionsSaved(FetchXmlOptions options) => RefreshAsync().FireAndForget();

    private void UpdateResults(FetchXmlDocument parsedDocument)
    {
        if (!parsedDocument.IsParsing && FetchXmlOptions.Instance.EnableFetchXmlPreviewWindow && FetchXmlOptions.Instance.FetchXmlExecution == FetchXmlExecutionMode.OnChange)
        {
            ScheduleFetch();
        }
    }

    private void ScheduleFetch(int delayMilliseconds = 350)
    {
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (isDisposed) return;
            pendingFetchCts?.Cancel();
            var pending = new CancellationTokenSource();
            pendingFetchCts = pending;
            try
            {
                await Task.Delay(delayMilliseconds, pending.Token);
                await ExecuteAndRenderAsync(pending.Token);
            }
            catch (OperationCanceledException) { }
            finally
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (ReferenceEquals(pendingFetchCts, pending)) pendingFetchCts = null;
                pending.Dispose();
            }
        }).FileAndForget("XrmTools/FetchXml/Execute");
    }

    private async Task ExecuteAndRenderAsync(CancellationToken debounceToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (isDisposed) return;
        debounceToken.ThrowIfCancellationRequested();
        activeFetchCts?.Cancel();
        using var executionCts = CancellationTokenSource.CreateLinkedTokenSource(debounceToken);
        activeFetchCts = executionCts;
        var cancellationToken = executionCts.Token;
        var queryText = textView.TextSnapshot.GetText();
        var requestId = Guid.NewGuid();
        activeRequestId = requestId;

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        SetLoading(true);

        FetchQueryResultModel? result = null;
        FetchXmlResultSet? resultSet = null;
        Exception? error = null;
        try
        {
            result = await ExecuteFetchXmlAsync(queryText, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(result.Error))
            {
                resultSet = await Task.Run(() => FetchXmlResultSet.Create(result.Records), cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            error = ex;
        }
        finally
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var isCurrent = activeRequestId == requestId;
            if (isCurrent && !isDisposed)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    SetStatus("Canceled", 0);
                    statusText.Text = hasResults ? "Previous results retained." : "Query canceled.";
                }
                else if (error is not null)
                {
                    ShowError(error.Message);
                }
                else if (result?.Error is { Length: > 0 } resultError)
                {
                    ShowError(resultError);
                }
                else if (result is not null)
                {
                    ShowResult(resultSet ?? FetchXmlResultSet.Empty, result.ElapsedMs, result.MoreRecords);
                }
                else
                {
                    ShowError("The query did not return a result.");
                }
                activeFetchCts = null;
                SetLoading(false);
                activeRequestId = null;
            }
        }
    }

    private async Task<FetchQueryResultModel> ExecuteFetchXmlAsync(string rawXml, CancellationToken cancellationToken)
    {
        var query = await Task.Run(() => FetchXmlExecutionInput.ParseAsync(rawXml, cancellationToken),
            cancellationToken).ConfigureAwait(false);
        using var repository = repositoryFactory.CreateRepository<IEntityMetadataRepository>();
        var entity = await repository.GetAsync(query.EntityName, cancellationToken).ConfigureAwait(false);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await webApi.FetchXmlAsync(entity.EntitySetName, query.Xml, false, cancellationToken).ConfigureAwait(false);
            if (response is null) return new FetchQueryResultModel { Error = "The Web API returned no response." };
            stopwatch.Stop();
            return new FetchQueryResultModel
            {
                Records = response.Records,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                MoreRecords = response.MoreRecords,
            };
        }
        catch (OperationCanceledException) { throw; }
        catch (ServiceException ex)
        {
            return new FetchQueryResultModel { Error = ex.ODataError?.Error?.Message ?? ex.Message };
        }
        catch (Exception ex)
        {
            return new FetchQueryResultModel { Error = ex.Message };
        }
        finally
        {
            if (stopwatch.IsRunning) stopwatch.Stop();
        }
    }

    private void ShowResult(FetchXmlResultSet resultSet, long elapsedMilliseconds, bool moreRecords)
    {
        columnIndexes.Clear();
        resultsGrid.ItemsSource = null;
        resultsGrid.Columns.Clear();

        foreach (var resultColumn in resultSet.Columns)
        {
            var valuePath = $"[{resultColumn.Index}]";
            var elementStyle = new Style(typeof(ShellTextBlock));
            elementStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.NoWrap));
            elementStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
            elementStyle.Setters.Add(new Setter(FrameworkElement.ToolTipProperty, new Binding(valuePath)));

            var gridColumn = new ShellDataGridTextColumn
            {
                Header = resultColumn.Name,
                Binding = new Binding(valuePath)
                {
                    Converter = SingleLineTextConverter.Instance,
                    Mode = BindingMode.OneWay,
                },
                ClipboardContentBinding = new Binding(valuePath) { Mode = BindingMode.OneWay },
                ElementStyle = elementStyle,
                IsReadOnly = true,
                MinWidth = 80,
                SortMemberPath = resultColumn.Index.ToString(CultureInfo.InvariantCulture),
                Width = new DataGridLength(160),
            };
            resultsGrid.Columns.Add(gridColumn);
            columnIndexes.Add(gridColumn, resultColumn.Index);
        }

        resultsGrid.ItemsSource = resultSet.Rows;
        hasResults = true;
        SetStatus("Success", 200);
        statusText.Text = $"Records: {resultSet.Rows.Count:N0} | Time: {elapsedMilliseconds:N0} ms"
            + (moreRecords ? " | More records available" : string.Empty);
    }

    private void ShowError(string message)
    {
        SetStatus("Error", 500);
        statusText.Text = hasResults ? "Previous results retained." : "Query failed.";
        errorDetails.Text = message;
        errorDetails.Visibility = Visibility.Visible;
    }

    private void SetStatus(string label, int code)
    {
        statusLabel.Text = label;
        statusBadge.Background = SystemParameters.HighContrast ? SystemColors.HighlightBrush :
            new SolidColorBrush(code >= 400 ? Color.FromRgb(205, 51, 51) :
                code >= 200 ? Color.FromRgb(46, 139, 87) : Color.FromRgb(128, 128, 128));
        statusLabel.Foreground = SystemParameters.HighContrast ? SystemColors.HighlightTextBrush : Brushes.White;
    }

    private void SetLoading(bool loading)
    {
        if (loading)
        {
            SetStatus("Running", 0);
            statusText.Text = hasResults ? "Executing query · Previous results retained." : "Executing query…";
            errorDetails.Clear();
            errorDetails.Visibility = Visibility.Collapsed;
        }
        progressIndicator.IsRunning = loading;
        progressIndicator.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
        actionButton.Content = loading ? "Cancel" : "Execute";
        ExecutionStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ResultsGridSorting(object sender, DataGridSortingEventArgs e)
    {
        if (!columnIndexes.TryGetValue(e.Column, out var columnIndex)) return;

        e.Handled = true;
        var direction = e.Column.SortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;

        foreach (var column in resultsGrid.Columns)
        {
            column.SortDirection = null;
        }

        if (CollectionViewSource.GetDefaultView(resultsGrid.ItemsSource) is ListCollectionView view)
        {
            view.CustomSort = new FetchXmlResultRowComparer(columnIndex, direction);
        }
        e.Column.SortDirection = direction;
    }

    private void CreateMarginControls(FrameworkElement content)
    {
        bool vertical = PreviewLocation == FetchXmlPreviewLocation.Vertical;
        if (vertical) { Width = Math.Max(150, FetchXmlOptions.Instance.FetchXmlPreviewWindowWidth); MinWidth = 150; }
        else { Height = Math.Max(100, FetchXmlOptions.Instance.FetchXmlPreviewWindowHeight); MinHeight = 100; }
        var splitter = new Thumb { Cursor = vertical ? Cursors.SizeWE : Cursors.SizeNS };
        if (vertical) splitter.Width = 5;
        else splitter.Height = 5;
        var template = new ControlTemplate(typeof(Thumb));
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetResourceReference(Border.BackgroundProperty, ShellColors.ControlStrokeDefaultBrushKey);
        template.VisualTree = border;
        splitter.Template = template;
        splitter.DragDelta += (_, e) =>
        {
            if (vertical) Width = Math.Max(150, Math.Min(1600, Width - e.HorizontalChange));
            else Height = Math.Max(100, Math.Min(800, Height - e.VerticalChange));
        };
        splitter.DragCompleted += (_, _) =>
        {
            if (vertical) FetchXmlOptions.Instance.FetchXmlPreviewWindowWidth = (int)Width;
            else FetchXmlOptions.Instance.FetchXmlPreviewWindowHeight = (int)Height;
            FetchXmlOptions.Instance.Save();
        };
        SetDock(splitter, vertical ? Dock.Left : Dock.Top);
        Children.Add(splitter);
        Children.Add(content);
    }

    private sealed class SingleLineTextConverter : IValueConverter
    {
        public static SingleLineTextConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text) return value ?? string.Empty;
            return text.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
