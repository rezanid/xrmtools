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

    private CancellationTokenSource _activeFetchCts;
    private Guid? _activeRequestId;

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

        document.Parsed -= UpdateBrowser;
        VSColorTheme.ThemeChanged -= OnThemeChange;
        GeneralOptions.Saved -= Options_Saved;
        if (Browser != null)
        {
            Browser.webView.CoreWebView2InitializationCompleted -= OnBrowserInitCompleted;
            Browser.WebMessageReceived -= Browser_WebMessageReceived;
        }

        _activeFetchCts?.Cancel();
        _activeFetchCts?.Dispose();
        _activeFetchCts = null;
        _activeRequestId = null;

        Browser.Dispose();

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
        view.CoreWebView2.Profile.PreferredColorScheme = IsVsDarkTheme() ? CoreWebView2PreferredColorScheme.Dark : CoreWebView2PreferredColorScheme.Light;

        document.Parsed += UpdateBrowser;
        GeneralOptions.Saved += Options_Saved;
        VSColorTheme.ThemeChanged += OnThemeChange;
        Browser.WebMessageReceived += Browser_WebMessageReceived;

        var isDark = IsVsDarkTheme();
        _ = Browser.SetHostThemeAsync(isDark);

        // Seed initial preview if parse already happened
        if (document.XmlDocument != null && !document.IsParsing)
        {
            UpdateBrowser(document);
        }
    }

    private void Browser_WebMessageReceived(object sender, string json)
    {
        // Lightweight parse for cancellation or manual refresh
        try
        {
            var env = Newtonsoft.Json.JsonConvert.DeserializeObject<WebEnvelope>(json);
            if (env == null || env.V != 1) return;
            switch (env.Kind?.ToLowerInvariant())
            {
                case "fetchxml/cancel":
                    if (env.RequestId != Guid.Empty)
                    {
                        TryCancelActiveFetch(env.RequestId);
                    }
                    break;
                case "fetchxml/refresh":
                    // Manual refresh request from SPA (button when idle)
                    if (!_activeRequestId.HasValue)
                    {
                        // Trigger immediate fetch bypassing parse debounce (use current document state)
                        ScheduleFetch(immediate: true);
                    }
                    break;
            }
        }
        catch { }
    }

    private bool TryCancelActiveFetch(Guid requestId)
    {
        if (_activeRequestId.HasValue && _activeRequestId.Value == requestId && _activeFetchCts != null && !_activeFetchCts.IsCancellationRequested)
        {
            _activeFetchCts.Cancel();
            return true;
        }
        return false;
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
                double newWidth = textView.ViewportWidth + grid.ActualWidth - 150;
                if (newWidth < 150)
                {
                    if (grid.ColumnDefinitions[2].MinWidth != 0)
                    {
                        grid.ColumnDefinitions[2].MinWidth = 0;
                        grid.ColumnDefinitions[2].MaxWidth = 0;
                    }
                }
                else
                {
                    grid.ColumnDefinitions[2].MaxWidth = newWidth;
                    if (grid.ColumnDefinitions[2].MinWidth == 0)
                    {
                        grid.ColumnDefinitions[2].MinWidth = 150;
                    }
                }
            });

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
        if (document.IsParsing) return;
        ScheduleFetch(immediate: false);
    }

    private void ScheduleFetch(bool immediate)
    {
        _ = ThreadHelper.JoinableTaskFactory.StartOnIdle(() =>
        {
            var execDebouncer = textView.TextBuffer.GetDebouncer("fetchxml-exec", millisecondsToWait: immediate ? 0 : 350);
            execDebouncer.Debounce(ct => ExecuteAndRenderAsync(ct), key: "exec");
        }, VsTaskRunContext.UIThreadIdlePriority);
    }

    private async Task ExecuteAndRenderAsync(CancellationToken debounceToken)
    {
        // cancel existing
        _activeFetchCts?.Cancel();
        _activeFetchCts?.Dispose();
        _activeFetchCts = CancellationTokenSource.CreateLinkedTokenSource(debounceToken);
        var token = _activeFetchCts.Token;
        var requestId = Guid.NewGuid();
        _activeRequestId = requestId;

        //TODO: Removed for now: await Browser.SetLoadingStateAsync(true).ConfigureAwait(false);
        await Browser.NotifyFetchStartedAsync(requestId).ConfigureAwait(false);
        await Browser.PostMessageAsync(new { v = 1, kind = "fetchxml/started", requestId }).ConfigureAwait(false);

        Stopwatch sw = Stopwatch.StartNew();
        FetchQueryResultModel result = null;
        Exception error = null;
        try
        {
            result = await ExecuteFetchXmlAsync(document, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // expected on cancellation
        }
        catch (Exception ex)
        {
            error = ex;
        }
        finally
        {
            sw.Stop();
            bool isCurrent = _activeRequestId == requestId;
            if (token.IsCancellationRequested)
            {
                if (isCurrent)
                {
                    await Browser.NotifyFetchCancelledAsync(requestId).ConfigureAwait(false);
                    await Browser.SetLoadingStateAsync(false).ConfigureAwait(false);
                }
            }
            else if (error != null)
            {
                if (isCurrent)
                {
                    await Browser.RenderFetchXmlResultAsync(new FetchQueryResultModel { Result = null, Error = error.Message }).ConfigureAwait(false);
                    await Browser.PostMessageAsync(new { v = 1, kind = "fetchxml/error", requestId, error = new { message = error.Message } }).ConfigureAwait(false);
                    await Browser.SetLoadingStateAsync(false).ConfigureAwait(false);
                }
            }
            else
            {
                if (isCurrent)
                {
                    await Browser.RenderFetchXmlResultAsync(result).ConfigureAwait(false);
                    await Browser.PostMessageAsync(new { v = 1, kind = "fetchxml/result", requestId, elapsedMs = (long)sw.Elapsed.TotalMilliseconds }).ConfigureAwait(false);
                    await Browser.SetLoadingStateAsync(false).ConfigureAwait(false);
                }
            }

            if (isCurrent)
            {
                _activeFetchCts?.Dispose();
                _activeFetchCts = null;
                _activeRequestId = null;
            }
        }
    }

    private async Task<FetchQueryResultModel> ExecuteFetchXmlAsync(FetchXmlDocument document, CancellationToken cancellationToken)
    {
        if (document == null || string.IsNullOrWhiteSpace(document.XmlDocument?.ToFullString()))
        {
            return new FetchQueryResultModel { Result = "null", ElapsedMs = 0 };
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

    private class WebEnvelope
    {
        public int V { get; set; }
        public string Kind { get; set; }
        public Guid RequestId { get; set; }
    }
}
