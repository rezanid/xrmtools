namespace XrmTools.FetchXml.Margin;

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XrmTools.Core.Repositories;
using XrmTools.Logging;
using XrmTools.Options;
using XrmTools.WebApi;
using XrmTools.WebApi.Methods;
using XrmTools.Xrm.Repositories;

internal class BrowserMargin : DockPanel, IWpfTextViewMargin
{
    private readonly IWebApiService webApi;
    private readonly IRepositoryFactory repositoryFactory;
    private readonly FetchXmlDocument document;
    private readonly ITextView textView;
    private bool _isDisposed;

    public FrameworkElement VisualElement => this;
    public double MarginSize => GeneralOptions.Instance.FetchXmlPreviewWindowWidth;
    public bool Enabled => true;
    public Browser Browser { get; private set; }

    public BrowserMargin(ITextView textView, IWebApiService webApi, IRepositoryFactory repositoryFactory, IOutputLoggerService logger)
    {
        this.webApi = webApi ?? throw new ArgumentNullException(nameof(webApi));
        this.repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
        this.textView = textView;
        document = textView.TextBuffer.GetFetchXmlDocument(logger);
        Visibility = GeneralOptions.Instance.EnableFetchXmlPreviewWindow ? Visibility.Visible : Visibility.Collapsed;

        SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);

        Browser = new Browser();
        Browser.webView.CoreWebView2InitializationCompleted += OnBrowserInitCompleted;

        CreateMarginControls(Browser.webView);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Browser.webView.CoreWebView2InitializationCompleted -= OnBrowserInitCompleted;
        document.Parsed -= UpdateBrowser;
        VSColorTheme.ThemeChanged -= OnThemeChange;
        GeneralOptions.Saved -= Options_Saved;

        Browser.Dispose();

        // Do NOT dispose the buffer-scoped debouncer here.

        _isDisposed = true;
    }

    private static bool IsVsDarkTheme()
    {
        var brush = (System.Windows.Media.SolidColorBrush)Application.Current.Resources[CommonControlsColors.TextBoxBackgroundBrushKey];
        var contrast = ColorUtilities.CompareContrastWithBlackAndWhite(brush.Color);
        var useLightTheme = contrast == ContrastComparisonResult.ContrastHigherWithBlack;
        return !useLightTheme;
    }

    private void OnBrowserInitCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (!e.IsSuccess)
        {
            throw e.InitializationException;
        }

        WebView2 view = sender as WebView2;

        view.SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);

        document.Parsed += UpdateBrowser;
        GeneralOptions.Saved += Options_Saved;
        VSColorTheme.ThemeChanged += OnThemeChange;

        var isDark = IsVsDarkTheme();
        _ = Browser.SetHostThemeAsync(isDark);

        // Seed initial preview if parse already happened
        if (document.XmlDocument != null && !document.IsParsing)
        {
            UpdateBrowser(document);
        }
    }

    private void CreateMarginControls(WebView2 view)
    {
        if (GeneralOptions.Instance.PreviewWindowLocation == FetchXmlPreviewLocation.Vertical)
        {
            CreateRightMarginControls(view);
        }
        else
        {
            CreateBottomMarginControls(view);
        }

        void CreateRightMarginControls(WebView2 view)
        {
            int width = GeneralOptions.Instance.FetchXmlPreviewWindowWidth;

            Grid grid = new();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(width, GridUnitType.Pixel), MinWidth = 150 });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);

            Children.Add(grid);

            grid.Children.Add(view);
            Grid.SetColumn(view, 2);
            Grid.SetRow(view, 0);

            GridSplitter splitter = new()
            {
                Width = 5,
                ResizeDirection = GridResizeDirection.Columns,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            splitter.SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);
            splitter.DragCompleted += SplitterDragCompleted;

            grid.Children.Add(splitter);
            Grid.SetColumn(splitter, 1);
            Grid.SetRow(splitter, 0);

            Action fixWidth = new(() =>
            {
                // previewWindow maxWidth = current total width - textView width
                double newWidth = textView.ViewportWidth + grid.ActualWidth - 150;

                // preveiwWindow maxWidth < previewWindow minWidth
                if (newWidth < 150)
                {
                    // Call 'get before 'set for performance
                    if (grid.ColumnDefinitions[2].MinWidth != 0)
                    {
                        grid.ColumnDefinitions[2].MinWidth = 0;
                        grid.ColumnDefinitions[2].MaxWidth = 0;
                    }
                }
                else
                {
                    grid.ColumnDefinitions[2].MaxWidth = newWidth;
                    // Call 'get before 'set for performance
                    if (grid.ColumnDefinitions[2].MinWidth == 0)
                    {
                        grid.ColumnDefinitions[2].MinWidth = 150;
                    }
                }
            });

            // Listen sizeChanged event of both marginGrid and textView
            grid.SizeChanged += (e, s) => fixWidth();
            textView.ViewportWidthChanged += (e, s) => fixWidth();
        }

        void CreateBottomMarginControls(WebView2 view)
        {
            int height = GeneralOptions.Instance.FetchXmlPreviewWindowHeight;

            Grid grid = new();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(5, GridUnitType.Pixel) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(height, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);

            Children.Add(grid);

            grid.Children.Add(view);
            Grid.SetColumn(view, 0);
            Grid.SetRow(view, 2);

            GridSplitter splitter = new()
            {
                Height = 5,
                ResizeDirection = GridResizeDirection.Rows,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            splitter.SetResourceReference(BackgroundProperty, VsBrushes.ToolWindowBackgroundKey);
            splitter.DragCompleted += SplitterDragCompleted;

            grid.Children.Add(splitter);
            Grid.SetColumn(splitter, 0);
            Grid.SetRow(splitter, 1);
        }
    }

    private void Options_Saved(GeneralOptions options)
    {
        RefreshAsync().FireAndForget();
    }

    private void OnThemeChange(ThemeChangedEventArgs e)
    {
        var isDark = IsVsDarkTheme();
        _ = Browser.SetHostThemeAsync(isDark);
        RefreshAsync().FireAndForget();
    }

    public async Task RefreshAsync()
    {
        GeneralOptions options = await GeneralOptions.GetLiveInstanceAsync();

        if (options.EnableFetchXmlPreviewWindow && Visibility != Visibility.Visible)
        {
            Visibility = Visibility.Visible;
            await Browser.RefreshAsync();
        }
        else if (Visibility != Visibility.Collapsed)
        {
            Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateBrowser(FetchXmlDocument document)
    {
        if (!document.IsParsing)
        {
            _ = ThreadHelper.JoinableTaskFactory.StartOnIdle(() =>
            {
                // Use a separate, named debouncer for Web API execution/render
                var execDebouncer = textView.TextBuffer.GetDebouncer("fetchxml-exec", millisecondsToWait: 350);
                execDebouncer.Debounce(async (ct) =>
                {
                    await Browser.SetLoadingStateAsync(true).ConfigureAwait(false);
                    var result = await ExecuteFetchXmlAsync(document, ct).ConfigureAwait(false);
                    await Browser.RenderFetchXmlResultAsync(result);
                    await Browser.SetLoadingStateAsync(false).ConfigureAwait(false);
                }, key: "exec");
            }, VsTaskRunContext.UIThreadIdlePriority);
        }
    }

    private async Task<FetchQueryResultModel> ExecuteFetchXmlAsync(FetchXmlDocument document, CancellationToken cancellationToken)
    {
        if (document == null || string.IsNullOrWhiteSpace(document.XmlDocument?.ToFullString()))
        {
            return null;
        }
        var repo = await repositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>().ConfigureAwait(false);
        var entity = await repo.GetAsync(document.EntityName, cancellationToken).ConfigureAwait(false);

        Stopwatch stopwatch = null;
        string fetchXml = document.XmlDocument.ToFullString();
        try
        {
            stopwatch = Stopwatch.StartNew();
            var result = await webApi.FetchXmlAsync(entity.EntitySetName, fetchXml, false, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();
            return new FetchQueryResultModel
            {
                Result = result.Records.ToString(Newtonsoft.Json.Formatting.None),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
            };
        }
        catch (ServiceException ex)
        {
            return new FetchQueryResultModel
            {
                Error = ex.ODataError is not null
                ? System.Text.Json.JsonSerializer.Serialize(ex.ODataError.Error)
                : $"{{\"message\":\"{ex.Message}\"}}"
            };
        }
        catch (Exception ex)
        {
            return new FetchQueryResultModel
            {
                Error = ex.Message
            };
        }
        finally
        {
            if (stopwatch?.IsRunning ?? false) stopwatch?.Stop();
        }
    }

    public ITextViewMargin GetTextViewMargin(string marginName) => this;

    private void SplitterDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
        if (GeneralOptions.Instance.PreviewWindowLocation == FetchXmlPreviewLocation.Vertical && !double.IsNaN(Browser.webView.ActualWidth))
        {
            GeneralOptions.Instance.FetchXmlPreviewWindowWidth = (int)Browser.webView.ActualWidth;
            GeneralOptions.Instance.Save();
        }
        else if (!double.IsNaN(Browser.webView.ActualHeight))
        {
            GeneralOptions.Instance.FetchXmlPreviewWindowHeight = (int)Browser.webView.ActualHeight;
            GeneralOptions.Instance.Save();
        }
    }
}
