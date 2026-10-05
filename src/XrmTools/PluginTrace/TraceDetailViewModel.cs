#nullable enable
namespace XrmTools.PluginTrace;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.UI;

internal sealed class TraceDetailViewModel : ViewModelBase, IDisposable
{
    private readonly ITraceExplorerService service;
    private CancellationTokenSource? loadCancellation;
    private TraceRecord? record, updated;
    private string raw = "", notice = "";
    private int preferredTab;
    private bool requiresReload;
    public TraceDetailViewModel(ITraceExplorerService service) => this.service = service;
    public TraceRecord? Record => record;
    public bool RequiresReload => requiresReload;
    public bool IsOpen => record != null;
    public string Title => record?.ShortTypeName ?? "";
    public string TypeName => record?.TypeName ?? "";
    public string Summary => record == null ? "" : $"{record.Time} · {record.MessageName} · {record.Entity}\n{record.Duration} ms · Depth {record.Depth} · {record.Status}";
    public bool HasException => !string.IsNullOrWhiteSpace(record?.Exception);
    public string Exception => HasException ? record!.Exception : "No exception recorded.";
    public string Message => string.IsNullOrEmpty(record?.Message) ? "No trace message recorded." : record!.Message;
    public bool CanShowRelated => record?.CorrelationId is Guid id && id != Guid.Empty;
    public bool CanUpdate => updated != null && updated.Raw != record?.Raw;
    public int PreferredTab { get => preferredTab; private set => SetProperty(ref preferredTab, value); }
    public string Raw { get => raw; private set => SetProperty(ref raw, value); }
    public string Notice { get => notice; private set => SetProperty(ref notice, value); }
    public bool HasNotice => !string.IsNullOrEmpty(Notice);
    public TraceRecord? UpdatedRecord => CanUpdate ? updated : null;

    public async Task OpenAsync(DataverseEnvironment? environment, TraceRecord selected)
    {
        Cancel();
        record = selected;
        requiresReload = true;
        PreferredTab = HasException ? 0 : 1;
        NotifyRecord();
        Raw = "Loading full record…";
        if (environment == null) { Raw = selected.Raw; requiresReload = false; return; }
        var cancellation = new CancellationTokenSource();
        loadCancellation = cancellation;
        try
        {
            var value = await service.DetailAsync(environment, selected.Id, cancellation.Token);
            if (!cancellation.IsCancellationRequested && ReferenceEquals(record, selected)) { Raw = value; requiresReload = false; }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!cancellation.IsCancellationRequested && ReferenceEquals(record, selected))
            {
                Raw = "Could not load the full record: " + ex.Message + "\n\nLoaded fields:\n" + selected.Raw;
                requiresReload = false;
            }
        }
        finally
        {
            if (ReferenceEquals(loadCancellation, cancellation)) loadCancellation = null;
            cancellation.Dispose();
        }
    }

    public void UpdateNotice(IReadOnlyList<TraceRecord>? latest)
    {
        updated = latest?.FirstOrDefault(r => r.Id == record?.Id);
        Notice = record == null ? "" : updated == null ? "Outside the current results. This trace remains open for inspection."
            : CanUpdate ? "An updated version is available. Your current view is preserved." : "";
        OnPropertyChanged(nameof(HasNotice)); OnPropertyChanged(nameof(CanUpdate));
    }

    public void Restore(TraceRecord selected, string fullRecord)
    {
        Cancel(); record = selected; requiresReload = false; PreferredTab = HasException ? 0 : 1; Raw = fullRecord; NotifyRecord();
    }
    public void Close() { Cancel(); record = updated = null; requiresReload = false; Raw = Notice = ""; NotifyRecord(); }
    public void Cancel() { loadCancellation?.Cancel(); loadCancellation = null; }
    public void Dispose() => Close();
    private void NotifyRecord()
    {
        foreach (var property in new[] { nameof(Record), nameof(IsOpen), nameof(Title), nameof(TypeName), nameof(Summary), nameof(HasException), nameof(Exception), nameof(Message), nameof(CanShowRelated), nameof(CanUpdate), nameof(HasNotice), nameof(PreferredTab) })
            OnPropertyChanged(property);
    }
}
