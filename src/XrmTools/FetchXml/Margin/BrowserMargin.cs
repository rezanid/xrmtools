namespace XrmTools.FetchXml.Margin;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
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
using XrmTools.Core.Repositories;
using XrmTools.FetchXml.CodeGen;
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
    private FrameworkElement resultsView;
    private ShellDataGrid resultsGrid = null!;
    private ShellTextBlock statusText = null!;
    private ShellButton actionButton = null!;
    private ShellProgressControl progressIndicator = null!;
    private CancellationTokenSource? activeFetchCts;
    private Guid? activeRequestId;
    private bool isDisposed;

    public BrowserMargin(ITextView textView, IWebApiService webApi, IRepositoryFactory repositoryFactory, ILogger logger)
    {
        this.webApi = webApi ?? throw new ArgumentNullException(nameof(webApi));
        this.repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
        this.textView = textView ?? throw new ArgumentNullException(nameof(textView));
        document = textView.TextBuffer.GetFetchXmlDocument(logger);

        Visibility = FetchXmlOptions.Instance.EnableFetchXmlPreviewWindow ? Visibility.Visible : Visibility.Collapsed;
        SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);

        resultsView = CreateResultsView();
        CreateMarginControls(resultsView);

        Loaded += OnLoaded;
        document.Parsed += UpdateResults;
        FetchXmlOptions.Saved += OptionsSaved;
    }

    public FrameworkElement VisualElement => this;

    public double MarginSize => FetchXmlOptions.Instance.PreviewWindowLocation == FetchXmlPreviewLocation.Vertical
        ? FetchXmlOptions.Instance.FetchXmlPreviewWindowWidth
        : FetchXmlOptions.Instance.FetchXmlPreviewWindowHeight;

    public bool Enabled => true;

    public void Dispose()
    {
        if (isDisposed) return;

        isDisposed = true;
        Loaded -= OnLoaded;
        document.Parsed -= UpdateResults;
        FetchXmlOptions.Saved -= OptionsSaved;
        resultsGrid.Sorting -= ResultsGridSorting;
        actionButton.Click -= ActionButtonClick;

        activeFetchCts?.Cancel();
        activeFetchCts?.Dispose();
        activeFetchCts = null;
        activeRequestId = null;
    }

    public async Task RefreshAsync()
    {
        var options = await FetchXmlOptions.GetLiveInstanceAsync();
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        Visibility = options.EnableFetchXmlPreviewWindow ? Visibility.Visible : Visibility.Collapsed;
    }

    public void TriggerFetch(bool immediate = true) => ScheduleFetch(immediate ? 0 : 350);

    public ITextViewMargin GetTextViewMargin(string marginName) => this;

    private FrameworkElement CreateResultsView()
    {
        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var toolbar = new DockPanel { Margin = Spacings.S };
        statusText = new ShellTextBlock
        {
            Text = "Records: 0 | Time: -",
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
        toolbar.Children.Add(statusText);
        root.Children.Add(toolbar);

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
        Grid.SetRow(resultsGrid, 1);
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

    private void ActionButtonClick(object sender, RoutedEventArgs e)
    {
        if (activeFetchCts is { IsCancellationRequested: false })
        {
            activeFetchCts.Cancel();
            return;
        }

        ScheduleFetch(0);
    }

    private void OptionsSaved(FetchXmlOptions options) => RefreshAsync().FireAndForget();

    private void UpdateResults(FetchXmlDocument parsedDocument)
    {
        if (!parsedDocument.IsParsing && FetchXmlOptions.Instance.FetchXmlExecution == FetchXmlExecutionMode.OnChange)
        {
            ScheduleFetch();
        }
    }

    private void ScheduleFetch(int delayMilliseconds = 350)
    {
        if (isDisposed) return;
        _ = ThreadHelper.JoinableTaskFactory.StartOnIdle(() =>
        {
            var debouncer = textView.TextBuffer.GetDebouncer("fetchxml-exec", millisecondsToWait: delayMilliseconds);
            debouncer.Debounce(token => ExecuteAndRenderAsync(token), key: "exec");
        }, VsTaskRunContext.UIThreadIdlePriority);
    }

    private async Task ExecuteAndRenderAsync(CancellationToken debounceToken)
    {
        activeFetchCts?.Cancel();
        activeFetchCts?.Dispose();
        activeFetchCts = CancellationTokenSource.CreateLinkedTokenSource(debounceToken);
        var cancellationToken = activeFetchCts.Token;
        var requestId = Guid.NewGuid();
        activeRequestId = requestId;

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        SetLoading(true);

        FetchQueryResultModel? result = null;
        FetchXmlResultSet? resultSet = null;
        Exception? error = null;
        try
        {
            result = await ExecuteFetchXmlAsync(document, cancellationToken).ConfigureAwait(false);
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
                    statusText.Text = "Cancelled";
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
                SetLoading(false);

                activeFetchCts?.Dispose();
                activeFetchCts = null;
                activeRequestId = null;
            }
        }
    }

    private async Task<FetchQueryResultModel> ExecuteFetchXmlAsync(FetchXmlDocument? parsedDocument, CancellationToken cancellationToken)
    {
        var xmlDocument = parsedDocument?.XmlDocument;
        if (parsedDocument is null || xmlDocument is null || string.IsNullOrWhiteSpace(xmlDocument.ToFullString()))
        {
            return new FetchQueryResultModel();
        }

        var parser = new FetchXmlParser();
        var query = await parser.ParseAsync(xmlDocument, parsedDocument.RawXml, cancellationToken).ConfigureAwait(false);
        var queryToExecute = string.IsNullOrEmpty(query.Defaulted) ? parsedDocument.RawXml : query.Defaulted;

        using var repository = repositoryFactory.CreateRepository<IEntityMetadataRepository>();
        var entity = await repository.GetAsync(parsedDocument.EntityName, cancellationToken).ConfigureAwait(false);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await webApi.FetchXmlAsync(entity.EntitySetName, queryToExecute, false, cancellationToken).ConfigureAwait(false);
            if (response is null) return new FetchQueryResultModel { Error = "The Web API returned no response." };
            stopwatch.Stop();
            return new FetchQueryResultModel
            {
                Records = response.Records,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                MoreRecords = response.MoreRecords,
            };
        }
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
        statusText.Text = $"Records: {resultSet.Rows.Count:N0} | Time: {elapsedMilliseconds:N0} ms"
            + (moreRecords ? " | More records available" : string.Empty);
    }

    private void ShowError(string message) => statusText.Text = $"Error: {message}";

    private void SetLoading(bool loading)
    {
        progressIndicator.IsRunning = loading;
        progressIndicator.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
        actionButton.Content = loading ? "Cancel" : "Execute";
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
        if (FetchXmlOptions.Instance.PreviewWindowLocation == FetchXmlPreviewLocation.Vertical)
        {
            var width = FetchXmlOptions.Instance.FetchXmlPreviewWindowWidth;
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width), MinWidth = 150 });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);
            Children.Add(grid);

            grid.Children.Add(content);
            Grid.SetColumn(content, 2);

            var splitter = new GridSplitter
            {
                Width = 5,
                ResizeDirection = GridResizeDirection.Columns,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            splitter.SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);
            splitter.DragCompleted += SplitterDragCompleted;
            grid.Children.Add(splitter);
            Grid.SetColumn(splitter, 1);

            void FixWidth()
            {
                var newWidth = textView.ViewportWidth + grid.ActualWidth - 150;
                if (newWidth < 150)
                {
                    grid.ColumnDefinitions[2].MinWidth = 0;
                    grid.ColumnDefinitions[2].MaxWidth = 0;
                }
                else
                {
                    grid.ColumnDefinitions[2].MaxWidth = newWidth;
                    if (grid.ColumnDefinitions[2].MinWidth == 0) grid.ColumnDefinitions[2].MinWidth = 150;
                }
            }

            grid.SizeChanged += (_, _) => FixWidth();
            textView.ViewportWidthChanged += (_, _) => FixWidth();
        }
        else
        {
            var height = FetchXmlOptions.Instance.FetchXmlPreviewWindowHeight;
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(height), MinHeight = 100 });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);
            Children.Add(grid);

            grid.Children.Add(content);
            Grid.SetRow(content, 2);

            var splitter = new GridSplitter
            {
                Height = 5,
                ResizeDirection = GridResizeDirection.Rows,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            splitter.SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);
            splitter.DragCompleted += SplitterDragCompleted;
            grid.Children.Add(splitter);
            Grid.SetRow(splitter, 1);
        }
    }

    private void SplitterDragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (FetchXmlOptions.Instance.PreviewWindowLocation == FetchXmlPreviewLocation.Vertical && !double.IsNaN(resultsView.ActualWidth))
        {
            FetchXmlOptions.Instance.FetchXmlPreviewWindowWidth = (int)resultsView.ActualWidth;
        }
        else if (!double.IsNaN(resultsView.ActualHeight))
        {
            FetchXmlOptions.Instance.FetchXmlPreviewWindowHeight = (int)resultsView.ActualHeight;
        }
        FetchXmlOptions.Instance.Save();
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
