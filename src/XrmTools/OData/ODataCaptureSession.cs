#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// One session belongs to one text buffer, not to the exported execution service.
internal sealed class ODataCaptureSession
{
    private readonly object gate = new();
    private readonly Dictionary<string, ODataCapturedResponse> captures = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> fingerprints = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> versions = new(StringComparer.Ordinal);
    private string? connection;
    private string? environmentName;
    private long epoch;
    private bool closed;
    private const long CaptureBudget = 200L * 1024 * 1024;
    private readonly LinkedList<string> captureOrder = new();
    private long capturedBytes;

    internal readonly record struct Run(string Name, long Version, long Epoch);

    public void SetEnvironment(DataverseEnvironment? environment)
    {
        lock (gate)
        {
            var next = environment?.IsValid == true ? environment.ConnectionString : null;
            var nextName = environment?.IsValid == true ? environment.Name : null;
            if (connection == next && environmentName == nextName) return;
            connection = next;
            environmentName = nextName;
            ClearCaptures();
            epoch++;
        }
    }

    public void Update(ODataDocument document)
    {
        lock (gate)
        {
            if (closed) return;
            var next = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var group in document.Requests.Where(r => !string.IsNullOrEmpty(r.RequestName)).GroupBy(r => r.RequestName, StringComparer.Ordinal))
            {
                if (group.Count() != 1) continue;
                try { next[group.Key] = document.Fingerprint(group.First()); }
                catch (FormatException) { /* Incomplete requests cannot retain a capture. */ }
            }
            foreach (var name in fingerprints.Keys.Union(next.Keys, StringComparer.Ordinal).ToArray())
            {
                if (!fingerprints.TryGetValue(name, out var before) || !next.TryGetValue(name, out var after) || before != after)
                    Invalidate(name);
            }
            fingerprints.Clear();
            foreach (var pair in next) fingerprints.Add(pair.Key, pair.Value);
        }
    }

    public Run Begin(ODataDocument document, ODataRequest request, DataverseEnvironment? environment)
    {
        lock (gate)
        {
            SetEnvironment(environment);
            Update(document);
            var name = request.RequestName;
            if (name.Length == 0) return new Run("", 0, epoch);
            Invalidate(name);
            return new Run(name, versions[name], epoch);
        }
    }

    public ODataCapturedResponse? Get(string name)
    {
        lock (gate) return !closed && captures.TryGetValue(name, out var value) ? value : null;
    }

    public bool AreCurrent(IEnumerable<ODataCapturedResponse> used, long expectedEpoch)
    {
        lock (gate) return !closed && epoch == expectedEpoch && used.All(value => captures.Values.Any(current => ReferenceEquals(current, value)));
    }

    public void Complete(Run run, ODataCapturedResponse response, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            if (!closed && !cancellationToken.IsCancellationRequested && run.Name.Length != 0 && epoch == run.Epoch &&
                versions.TryGetValue(run.Name, out var version) && version == run.Version && fingerprints.ContainsKey(run.Name))
            {
                RemoveCapture(run.Name);
                while (capturedBytes + response.ContentByteLength > CaptureBudget && captureOrder.First != null)
                    RemoveCapture(captureOrder.First.Value);
                captures[run.Name] = response;
                captureOrder.AddLast(run.Name);
                capturedBytes += response.ContentByteLength;
            }
            else response.Dispose();
        }
    }

    public void Close()
    {
        lock (gate)
        {
            closed = true;
            ClearCaptures();
            fingerprints.Clear();
            versions.Clear();
            connection = null;
            environmentName = null;
            epoch++;
        }
    }

    private void Invalidate(string name)
    {
        RemoveCapture(name);
        versions[name] = versions.TryGetValue(name, out var version) ? version + 1 : 1;
    }

    private void ClearCaptures()
    {
        foreach (var capture in captures.Values) capture.Dispose();
        captures.Clear();
        captureOrder.Clear();
        capturedBytes = 0;
    }

    private void RemoveCapture(string name)
    {
        if (!captures.TryGetValue(name, out var capture)) return;
        captures.Remove(name);
        captureOrder.Remove(name);
        capturedBytes -= capture.ContentByteLength;
        capture.Dispose();
    }
}

internal sealed class ODataBufferSession : IDisposable
{
    private readonly ITextBuffer buffer;
    private int views;
    public ODataCaptureSession Captures { get; } = new();

    private ODataBufferSession(ITextBuffer buffer)
    {
        this.buffer = buffer;
        Captures.Update(ODataDocument.Parse(buffer.CurrentSnapshot.GetText()));
        buffer.Changed += Changed;
    }

    public static ODataBufferSession Attach(ITextBuffer buffer)
    {
        var session = buffer.Properties.GetOrCreateSingletonProperty(() => new ODataBufferSession(buffer));
        session.views++;
        return session;
    }

    public static ODataCaptureSession? GetCaptures(ITextBuffer buffer)
        => buffer.Properties.TryGetProperty(typeof(ODataBufferSession), out ODataBufferSession session) ? session.Captures : null;

    private void Changed(object sender, TextContentChangedEventArgs e) => Captures.Update(ODataDocument.Parse(e.After.GetText()));

    public void Dispose()
    {
        if (--views != 0) return;
        buffer.Changed -= Changed;
        Captures.Close();
        buffer.Properties.RemoveProperty(typeof(ODataBufferSession));
    }
}
