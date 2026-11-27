#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ODataExecutionRow
{
    public int Id { get; internal set; }
    public ODataRequest Request { get; internal set; } = null!;
    public string State { get; internal set; } = "Pending";
    public string? Message { get; internal set; }
    public ODataResponse? Response { get; internal set; }
    public double ElapsedMilliseconds { get; internal set; }
    public bool BodyAvailable { get; internal set; }
}

internal sealed class ODataExecutionResult : IDisposable
{
    public bool IsDocument { get; internal set; }
    public DataverseEnvironment Environment { get; internal set; } = null!;
    public IReadOnlyList<ODataExecutionRow> Rows { get; internal set; } = Array.Empty<ODataExecutionRow>();
    public string? StopReason { get; internal set; }
    public void Dispose()
    {
        foreach (var row in Rows)
        {
            row.Response?.Dispose();
            row.Response = null;
            row.BodyAvailable = false;
        }
    }
}

internal sealed class ODataExecutionCoordinator : IDisposable
{
    internal delegate Task<ODataResponse> SendDelegate(ODataDocument document, ODataRequest request, DataverseEnvironment environment,
        CancellationToken token, ODataCaptureSession captures, ODataCaptureSession.Run run);

    private const long ResultBudget = 200L * 1024 * 1024;
    private const string EnvironmentStop = "The environment changed. Review the selected environment and send again.";
    private const string Evicted = "[Response data unavailable: retention limit reached.]";
    private readonly object gate = new();
    private readonly SendDelegate send;
    private readonly Func<Task<DataverseEnvironment?>> selectedEnvironment;
    private readonly ODataCaptureSession manualCaptures;
    private CancellationTokenSource? cancellation;
    private SynchronizationContext? context;
    private bool closed;
    private bool executing;
    private bool environmentChanged;
    private Guid? activeId;
    private ODataExecutionResult? result;

    public ODataExecutionCoordinator(ODataExecutionService execution, ODataCaptureSession captures)
        : this(execution.SendAsync, execution.GetEnvironmentAsync, captures) { }

    internal ODataExecutionCoordinator(SendDelegate send, Func<Task<DataverseEnvironment?>> selectedEnvironment, ODataCaptureSession captures)
    {
        this.send = send;
        this.selectedEnvironment = selectedEnvironment;
        manualCaptures = captures;
    }

    public event EventHandler? Changed;
    public bool IsExecuting { get { lock (gate) return executing; } }
    public bool IsDocumentRun { get { lock (gate) return executing && result?.IsDocument == true; } }
    public Guid? ActiveId { get { lock (gate) return activeId; } }
    public ODataExecutionResult? Result { get { lock (gate) return result; } }

    public bool TryStartManual(ODataDocument document, ODataRequest request, DataverseEnvironment environment)
        => Start(document, request, environment, false);
    public bool TryStartDocument(ODataDocument document, DataverseEnvironment environment)
        => Start(document, null, environment, true);

    private bool Start(ODataDocument document, ODataRequest? request, DataverseEnvironment environment, bool isDocument)
    {
        lock (gate)
        {
            if (closed || executing) return false;
            var snapshot = Snapshot(document);
            var requests = isDocument ? snapshot.Requests.ToArray() : new[] { CloneRequest(request!) };
            var next = new ODataExecutionResult
            {
                IsDocument = isDocument,
                Environment = environment with { },
                Rows = Array.AsReadOnly(requests.Select((item, index) => new ODataExecutionRow { Id = index, Request = item }).ToArray())
            };
            // Manual publication is versioned against the live buffer; document runs never touch it.
            var captures = isDocument ? new ODataCaptureSession() : manualCaptures;
            ODataCaptureSession.Run manualRun = default;
            if (!isDocument) manualRun = captures.Begin(snapshot, requests[0], next.Environment);
            result?.Dispose();
            result = next;
            context = SynchronizationContext.Current;
            cancellation = new CancellationTokenSource();
            activeId = Guid.NewGuid();
            environmentChanged = false;
            executing = true;
            _ = ExecuteAsync(snapshot, next, captures, manualRun, cancellation);
        }
        Notify();
        return true;
    }

    private async Task ExecuteAsync(ODataDocument document, ODataExecutionResult current, ODataCaptureSession captures,
        ODataCaptureSession.Run manualRun, CancellationTokenSource source)
    {
        await Task.Yield();
        var token = source.Token;
        var retained = new Queue<(ODataExecutionRow Row, long Size)>();
        long retainedBytes = 0;
        try
        {
            for (int i = 0; i < current.Rows.Count; i++)
            {
                var row = current.Rows[i];
                var watch = Stopwatch.StartNew();
                ODataResponse? response = null;
                try
                {
                    token.ThrowIfCancellationRequested();
                    bool changed = false;
                    if (i > 0)
                    {
                        var selected = await selectedEnvironment();
                        token.ThrowIfCancellationRequested();
                        changed = selected?.IsValid != true || selected.ConnectionString != current.Environment.ConnectionString || selected.Name != current.Environment.Name;
                    }
                    lock (gate)
                    {
                        if (closed) return;
                        token.ThrowIfCancellationRequested();
                        if (changed || environmentChanged)
                        {
                            current.StopReason = EnvironmentStop;
                            break;
                        }
                        row.State = "Running";
                    }
                    Notify();
                    var run = current.IsDocument ? captures.Begin(document, row.Request, current.Environment) : manualRun;
                    response = await send(document, row.Request, current.Environment, token, captures, run);
                    token.ThrowIfCancellationRequested();
                    if (response.CanCapture)
                    {
                        var capture = new ODataCapturedResponse
                        {
                            Body = response.StoredBody == null ? response.Body : "",
                            ContentByteLength = Math.Max(response.ContentByteLength, Encoding.UTF8.GetByteCount(response.Body)),
                            Headers = response.CapturableHeaders,
                            Principal = response.Principal
                        };
                        capture.StoredBody = response.StoredBody?.Retain();
                        captures.Complete(run, capture, token);
                    }
                    lock (gate)
                    {
                        if (closed) return;
                        row.Response = response;
                        row.BodyAvailable = true;
                        row.ElapsedMilliseconds = response.ElapsedMilliseconds;
                        row.State = response.StatusCode >= 200 && response.StatusCode < 300 ? "Succeeded" : "Failed";
                        if (row.State == "Failed") row.Message = LimitMessage(response.Status);
                        // Capture metadata has already been published; it is not needed by result viewers.
                        response.CapturableHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        response.Principal = null;
                        long fileSize = response.StoredBody == null ? 0 : Math.Min(Math.Max(response.ContentByteLength, 0), ResultBudget + 1);
                        long size = 256 + fileSize + 2L * (response.Body.Length + (long)response.Raw.Length + response.Headers.Length + response.Request.Length + response.Status.Length);
                        retained.Enqueue((row, size));
                        retainedBytes += size;
                        response = null;
                        while (retainedBytes > ResultBudget && retained.Count != 0)
                        {
                            var old = retained.Dequeue();
                            retainedBytes -= old.Size;
                            var owned = old.Row.Response!;
                            owned.Dispose();
                            owned.Body = "";
                            owned.Raw = Evicted;
                            owned.Headers = Evicted;
                            owned.Request = Evicted;
                            owned.Status = Evicted;
                            old.Row.BodyAvailable = false;
                        }
                    }
                    Notify();
                    if (row.State != "Succeeded") break;
                }
                catch (OperationCanceledException)
                {
                    lock (gate)
                    {
                        if (!closed)
                        {
                            current.StopReason = token.IsCancellationRequested ? "Canceled." : "Timed out or canceled.";
                            if (row.State == "Running") { row.State = "Canceled"; row.Message = current.StopReason; row.ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds; }
                        }
                    }
                    Notify();
                    break;
                }
                catch (Exception ex)
                {
                    lock (gate) { if (!closed) { row.State = "Failed"; row.Message = LimitMessage(ex.Message); row.ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds; } }
                    Notify();
                    break;
                }
                finally { response?.Dispose(); }
            }
        }
        finally
        {
            if (current.IsDocument) captures.Close();
            lock (gate)
            {
                foreach (var row in current.Rows.Where(item => item.State == "Pending")) row.State = "Skipped";
                if (ReferenceEquals(cancellation, source))
                {
                    executing = false;
                    activeId = null;
                    cancellation = null;
                }
            }
            source.Dispose();
            Notify();
        }
    }

    internal void EnvironmentChanged(DataverseEnvironment? environment)
    {
        lock (gate)
            if (executing && result != null && (environment?.IsValid != true ||
                environment.ConnectionString != result.Environment.ConnectionString || environment.Name != result.Environment.Name))
                environmentChanged = true;
    }

    public void Cancel(Guid? id)
    {
        CancellationTokenSource? source;
        lock (gate) source = executing && (id == null || id == activeId) ? cancellation : null;
        try { source?.Cancel(); } catch (ObjectDisposedException) { }
    }

    public void Dispose()
    {
        CancellationTokenSource? source;
        lock (gate)
        {
            if (closed) return;
            closed = true;
            source = cancellation;
            result?.Dispose();
            result = null;
        }
        try { source?.Cancel(); } catch (ObjectDisposedException) { }
    }

    private void Notify()
    {
        SynchronizationContext? target;
        lock (gate) { if (closed) return; target = context; }
        void Raise()
        {
            lock (gate) if (closed) return;
            Changed?.Invoke(this, EventArgs.Empty);
        }
        // Posting also keeps subscriber exceptions outside the execution error handler.
        (target ?? new SynchronizationContext()).Post(_ => Raise(), null);
    }

    private static string LimitMessage(string message) => message.Length <= 1024 ? message : message.Substring(0, 1024) + "…";

    private static ODataDocument Snapshot(ODataDocument source)
    {
        var copy = new ODataDocument();
        foreach (var pair in source.Variables) copy.Variables.Add(pair.Key, pair.Value);
        copy.SetEnvironmentVariables(source.EnvironmentVariables);
        copy.Errors.AddRange(source.Errors);
        foreach (var request in source.Requests) copy.Requests.Add(CloneRequest(request));
        return copy;
    }

    private static ODataRequest CloneRequest(ODataRequest source)
    {
        var copy = new ODataRequest { Line = source.Line, StartLine = source.StartLine, EndLine = source.EndLine,
            Name = source.Name, RequestName = source.RequestName, Method = source.Method, Target = source.Target,
            Body = source.Body, ExecutableText = source.ExecutableText, Error = source.Error };
        copy.Headers.AddRange(source.Headers);
        return copy;
    }
}
