#nullable enable
namespace XrmTools.OData;

using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

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

    public void AdvanceEpoch()
    {
        lock (gate) epoch++;
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

    internal long Epoch { get { lock (gate) return epoch; } }

    internal bool AreInspectionsCurrent(IReadOnlyDictionary<string, ODataCapturedResponse?> inspected, long expectedEpoch)
    {
        lock (gate) return !closed && epoch == expectedEpoch && inspected.All(pair =>
            ReferenceEquals(pair.Value, captures.TryGetValue(pair.Key, out var current) ? current : null));
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
    private readonly object gate = new();
    private readonly ITextBuffer buffer;
    private string? documentPath;
    private string? sidecarPath;
    private FileSystemWatcher[] watchers = [];
    private string text;
    private Dictionary<string, string> environmentVariables = new(StringComparer.Ordinal);
    private string? environmentName;
    private string? configurationError;
    private Task profileLoad = Task.CompletedTask;
    private int revision;
    private bool loading;
    private bool disposed;
    private int views;
    public ODataCaptureSession Captures { get; } = new();
    public event EventHandler? VariablesChanged;

    private ODataBufferSession(ITextBuffer buffer, string? filePath)
    {
        this.buffer = buffer;
        text = buffer.CurrentSnapshot.GetText();
        Captures.Update(ODataDocument.Parse(text));
        buffer.Changed += Changed;
        SetDocumentPath(filePath);
    }

    public static ODataBufferSession Attach(ITextBuffer buffer, string? filePath = null)
    {
        var session = buffer.Properties.GetOrCreateSingletonProperty(() => new ODataBufferSession(buffer, filePath));
        session.views++;
        return session;
    }

    public void SetDocumentPath(string? filePath)
    {
        string? nextDocumentPath = string.IsNullOrWhiteSpace(filePath) ? null : Path.GetFullPath(filePath);
        lock (gate)
            if (string.Equals(documentPath, nextDocumentPath, StringComparison.OrdinalIgnoreCase)) return;

        var nextWatchers = nextDocumentPath == null ? [] : ODataEnvironmentFile.GetSearchDirectories(nextDocumentPath)
            .Select(directory => CreateWatcher(directory)).ToArray();
        FileSystemWatcher[] previousWatchers;
        bool refresh;
        lock (gate)
        {
            if (disposed || string.Equals(documentPath, nextDocumentPath, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in nextWatchers) item.Dispose();
                return;
            }
            previousWatchers = watchers;
            watchers = nextWatchers;
            documentPath = nextDocumentPath;
            sidecarPath = null;
            refresh = environmentName != null;
        }
        foreach (var item in previousWatchers)
        {
            item.EnableRaisingEvents = false;
            item.Dispose();
        }
        foreach (var item in nextWatchers) item.EnableRaisingEvents = true;
        if (refresh) ScheduleReload();
    }

    private FileSystemWatcher CreateWatcher(string directory)
    {
        var item = new FileSystemWatcher(directory, "odata-client.env.json")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
        };
        item.Changed += SidecarChanged;
        item.Created += SidecarChanged;
        item.Deleted += SidecarChanged;
        item.Renamed += SidecarRenamed;
        item.Error += WatcherError;
        return item;
    }

    public string? SavedDocumentPath { get { lock (gate) return documentPath; } }
    public string? SelectedEnvironmentName { get { lock (gate) return environmentName; } }
    public static ODataBufferSession? Get(ITextBuffer buffer)
        => buffer.Properties.TryGetProperty(typeof(ODataBufferSession), out ODataBufferSession session) ? session : null;

    public static ODataCaptureSession? GetCaptures(ITextBuffer buffer) => Get(buffer)?.Captures;

    public ODataDocument CreateDocument(string source)
    {
        lock (gate)
        {
            var document = ODataDocument.Parse(source);
            document.SetEnvironmentVariables(environmentVariables);
            return document;
        }
    }

    public bool TryCreateDocument(string source, string? expectedEnvironment, out ODataDocument document, out string? error)
    {
        lock (gate)
        {
            document = ODataDocument.Parse(source);
            document.SetEnvironmentVariables(environmentVariables);
            error = configurationError;
            if (loading)
            {
                error = "Environment variables are still loading. Wait and send again.";
                return false;
            }
            if (!string.Equals(environmentName, expectedEnvironment, StringComparison.OrdinalIgnoreCase))
            {
                error = "The selected environment changed. Refresh the environment and send again.";
                return false;
            }
            return true;
        }
    }

    internal async Task<ODataInspection?> InspectAsync(string source, string expression, CancellationToken token)
    {
        ODataDocument document;
        string? selected;
        int expectedRevision;
        long expectedEpoch;
        bool variablesLoading;
        lock (gate)
        {
            if (disposed || source != text) return null;
            document = CreateDocument(source);
            selected = environmentName;
            expectedRevision = revision;
            expectedEpoch = Captures.Epoch;
            variablesLoading = loading;
        }
        var inspected = new Dictionary<string, ODataCapturedResponse?>(StringComparer.Ordinal);
        var result = variablesLoading && !ODataBuiltInVariables.All.ContainsKey(expression)
            ? new ODataInspection("Environment variables", "Environment variables are still loading. Hover again when loading completes.") { IsValue = false }
            : await ODataInspector.InspectAsync(document, expression, selected, name =>
            {
                var capture = Captures.Get(name);
                inspected[name] = capture;
                return capture;
            }, _ => { }, token).ConfigureAwait(false);
        token.ThrowIfCancellationRequested();
        return IsCurrent() ? result with { IsCurrent = IsCurrent } : null;

        bool IsCurrent()
        {
            lock (gate)
                return !disposed && source == text && expectedRevision == revision && loading == variablesLoading &&
                    Captures.AreInspectionsCurrent(inspected, expectedEpoch);
        }
    }

    public string? ConfigurationError { get { lock (gate) return configurationError; } }
    public string? EnvironmentFilePath { get { lock (gate) return sidecarPath; } }
    public bool IsLoading { get { lock (gate) return loading; } }
    // UI-thread only: lets one view of a shared buffer own the configuration InfoBar, avoiding duplicates in split views.
    internal object? ConfigurationNoticeOwner { get; set; }

    public Task SetEnvironmentAsync(DataverseEnvironment? environment)
    {
        string? selected = environment?.IsValid == true ? environment.Name : null;
        TaskCompletionSource<bool>? completion = null;
        int loadRevision = 0;
        string? loadDocumentPath = null;
        lock (gate)
        {
            Captures.SetEnvironment(environment);
            if (string.Equals(environmentName, selected, StringComparison.OrdinalIgnoreCase))
                return loading ? profileLoad : Task.CompletedTask;

            environmentName = selected;
            environmentVariables = new Dictionary<string, string>(StringComparer.Ordinal);
            configurationError = null;
            loading = selected != null && documentPath != null;
            loadRevision = ++revision;
            if (loading)
            {
                loadDocumentPath = documentPath;
                completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                profileLoad = completion.Task;
            }
            UpdateCapturesLocked();
        }
        VariablesChanged?.Invoke(this, EventArgs.Empty);
        if (completion == null) return Task.CompletedTask;
        _ = LoadProfileAsync(selected!, loadDocumentPath, loadRevision, completion);
        return completion.Task;
    }

    private async Task LoadProfileAsync(string selected, string? sourcePath, int loadRevision, TaskCompletionSource<bool> completion)
    {
        var loaded = await Task.Run(() =>
        {
            string? path = sourcePath == null ? null : ODataEnvironmentFile.FindNearest(sourcePath);
            return (Path: path, Profile: ReadProfile(selected, path));
        }).ConfigureAwait(false);
        bool changed = false;
        lock (gate)
        {
            if (!disposed && loadRevision == revision)
            {
                sidecarPath = loaded.Path;
                environmentVariables = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var variable in loaded.Profile.Variables) environmentVariables.Add(variable.Key, variable.Value);
                configurationError = loaded.Profile.Error;
                loading = false;
                UpdateCapturesLocked();
                changed = true;
            }
        }
        if (changed) VariablesChanged?.Invoke(this, EventArgs.Empty);
        completion.TrySetResult(true);
    }

    private static ODataEnvironmentProfile ReadProfile(string selected, string? path)
    {
        if (path == null) return ODataEnvironmentVariables.Missing();
        try { return ODataEnvironmentVariables.Parse(File.ReadAllText(path), selected); }
        catch (FileNotFoundException) { return ODataEnvironmentVariables.Missing(); }
        catch (DirectoryNotFoundException) { return ODataEnvironmentVariables.Missing(); }
        catch (IOException) { return ODataEnvironmentVariables.Invalid("The file could not be read."); }
        catch (UnauthorizedAccessException) { return ODataEnvironmentVariables.Invalid("The file could not be read."); }
        catch (SecurityException) { return ODataEnvironmentVariables.Invalid("The file could not be read."); }
    }

    private void Changed(object sender, TextContentChangedEventArgs e)
    {
        lock (gate)
        {
            text = e.After.GetText();
            UpdateCapturesLocked();
        }
    }

    private void UpdateCapturesLocked()
    {
        var document = ODataDocument.Parse(text);
        document.SetEnvironmentVariables(environmentVariables);
        Captures.Update(document);
    }

    private void SidecarChanged(object sender, FileSystemEventArgs e) => ScheduleReload();
    private void SidecarRenamed(object sender, RenamedEventArgs e) => ScheduleReload();

    private void ScheduleReload()
    {
        string? selected;
        string? sourcePath;
        int loadRevision;
        TaskCompletionSource<bool> completion;
        lock (gate)
        {
            if (disposed || environmentName == null) return;
            selected = environmentName;
            sourcePath = documentPath;
            loading = true;
            loadRevision = ++revision;
            completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            profileLoad = completion.Task;
        }
        Captures.AdvanceEpoch();
        VariablesChanged?.Invoke(this, EventArgs.Empty);
        _ = ReloadAfterDelayAsync(selected, sourcePath, loadRevision, completion);
    }

    private async Task ReloadAfterDelayAsync(string selected, string? sourcePath, int loadRevision, TaskCompletionSource<bool> completion)
    {
        await Task.Delay(150).ConfigureAwait(false);
        await LoadProfileAsync(selected, sourcePath, loadRevision, completion).ConfigureAwait(false);
    }

    private void WatcherError(object sender, ErrorEventArgs e)
    {
        lock (gate)
        {
            if (disposed) return;
            revision++;
            loading = false;
            environmentVariables.Clear();
            configurationError = "The environment configuration file could not be monitored.";
            UpdateCapturesLocked();
        }
        Captures.AdvanceEpoch();
        VariablesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (--views != 0) return;
            disposed = true;
        }
        buffer.Changed -= Changed;
        foreach (var item in watchers)
        {
            item.EnableRaisingEvents = false;
            item.Dispose();
        }
        Captures.Close();
        buffer.Properties.RemoveProperty(typeof(ODataBufferSession));
    }
}
