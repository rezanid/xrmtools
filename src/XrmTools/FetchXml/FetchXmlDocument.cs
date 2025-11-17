namespace XrmTools.FetchXml;

using Microsoft.Language.Xml;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;

public class FetchXmlDocument : IDisposable
{
    private readonly ILogger _logger;
    private readonly ITextBuffer _buffer;
    private readonly SemaphoreSlim _parseSemaphore = new(1, 1);
    private CancellationTokenSource _parseCts = new();
    private readonly CancellationTokenSource _disposalTokenSource = new();
    private bool _isDisposed;
    private string _lastParsedText;
    private int _lastParsedVersion;
    public XmlDocumentSyntax XmlDocument { get; private set; }
    public string FileName { get; }
    public string EntityName { get; private set; }
    public bool IsParsing { get; private set; }
    public event Action<FetchXmlDocument> Parsed;

    public FetchXmlDocument(ITextBuffer buffer, ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        _buffer.Changed += BufferChanged;
        FileName = buffer.GetFileName();

        // Initial eager parse
        ParseAsync().FireAndForget();
        GeneralOptions.Saved += OptionsSaved;
    }


    private void BufferChanged(object sender, TextContentChangedEventArgs e)
    {
        // Debounce parsing until the user pauses typing
        // Use a named, per-buffer debouncer to avoid interfering with other debounced tasks
        var parseDebouncer = _buffer.GetDebouncer("fetchxml-parse", millisecondsToWait: 400);
        parseDebouncer.Debounce(ct => ParseAsync(ct), key: "parse");
    }

    // Original parse flow kept for initial parse (constructor) and any direct calls
    private async Task ParseAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        // Cancel any in-flight parse (we will ignore its result if already running)
        _parseCts.Cancel();
        _parseCts.Dispose();
        _parseCts = new CancellationTokenSource();
        CancellationToken localToken = _parseCts.Token;

        // Use semaphore to prevent multiple concurrent parsing operations
        if (!await _parseSemaphore.WaitAsync(0, _disposalTokenSource.Token))
        {
            return; // Another parse operation is already in progress (it will get cancelled if outdated)
        }

        int snapshotVersion = _buffer.CurrentSnapshot.Version.VersionNumber;

        try
        {
            IsParsing = true;
            bool success = false;

            try
            {
                await TaskScheduler.Default; // move to a background thread

                if (localToken.IsCancellationRequested)
                {
                    return;
                }

                ITextSnapshot snapshot = _buffer.CurrentSnapshot; // capture snapshot
                string text = snapshot.GetText();

                // Skip parsing if text hasn't changed based on snapshot version & content
                if (snapshotVersion == _lastParsedVersion && string.Equals(text, _lastParsedText, StringComparison.Ordinal))
                {
                    success = true; // treat as success so consumers can continue
                    return;
                }

                var doc = Parser.ParseText(text);
                EntityName = ParseEntityName(doc);

                if (localToken.IsCancellationRequested)
                {
                    return; // abandon
                }

                // Only publish results if the snapshot hasn't advanced further
                if (_buffer.CurrentSnapshot.Version.VersionNumber != snapshotVersion)
                {
                    return; // stale result
                }

                XmlDocument = doc;
                _lastParsedText = text;
                _lastParsedVersion = snapshotVersion;
                success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(FileName + " parse error: " + ex.Message);
            }
            finally
            {
                IsParsing = false;

                if (success && !localToken.IsCancellationRequested)
                {
                    Parsed?.Invoke(this);
                }
            }
        }
        finally
        {
            _parseSemaphore.Release();
        }
    }

    private async Task ParseAsync(CancellationToken externalToken)
    {
        if (_isDisposed)
        {
            return;
        }

        if (!await _parseSemaphore.WaitAsync(0, _disposalTokenSource.Token))
        {
            return;
        }

        int snapshotVersion = _buffer.CurrentSnapshot.Version.VersionNumber;

        try
        {
            IsParsing = true;
            bool success = false;

            try
            {
                await TaskScheduler.Default;

                if (externalToken.IsCancellationRequested)
                {
                    return;
                }

                ITextSnapshot snapshot = _buffer.CurrentSnapshot;
                string text = snapshot.GetText();

                if (snapshotVersion == _lastParsedVersion && string.Equals(text, _lastParsedText, StringComparison.Ordinal))
                {
                    success = true;
                    return;
                }

                var doc = Parser.ParseText(text);

                EntityName = ParseEntityName(doc);

                if (externalToken.IsCancellationRequested)
                {
                    return;
                }

                if (_buffer.CurrentSnapshot.Version.VersionNumber != snapshotVersion)
                {
                    return;
                }

                XmlDocument = doc;
                _lastParsedText = text;
                _lastParsedVersion = snapshotVersion;
                success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(FileName + " parse error: " + ex.Message);
            }
            finally
            {
                IsParsing = false;

                if (success && !externalToken.IsCancellationRequested)
                {
                    Parsed?.Invoke(this);
                }
            }
        }
        finally
        {
            _parseSemaphore.Release();
        }
    }

    private string ParseEntityName(XmlDocumentSyntax doc)
    {
        if (doc is null || doc.RootSyntax is null)
        {
            return string.Empty;
        }
        var fetchElement = doc.Root;
        if (fetchElement is null || !string.Equals(fetchElement.Name, "fetch", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }
        var entityElement = fetchElement.Elements.First();
        if (entityElement is null || !string.Equals(entityElement.Name, "entity", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }
        var nameAttr = entityElement.Attributes.FirstOrDefault(kv => string.Equals(kv.Key, "name", StringComparison.OrdinalIgnoreCase));
        return nameAttr.Value ?? string.Empty;
    }

    private void OptionsSaved(GeneralOptions obj)
    {
        //ParseAsync().FireAndForget();
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _buffer.Changed -= BufferChanged;
            GeneralOptions.Saved -= OptionsSaved;
            _disposalTokenSource.Cancel();
            _disposalTokenSource.Dispose();
            _parseCts.Cancel();
            _parseCts.Dispose();
            _parseSemaphore.Dispose();
        }

        _isDisposed = true;
    }
}
