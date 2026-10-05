#nullable enable
namespace XrmTools.PluginTrace;

using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.UI;

internal sealed record TraceLoggingOption(string Label, TraceLoggingMode Mode, bool Timed = false);

/// <summary>Separates the user's logging choice from the confirmed Dataverse setting.</summary>
internal sealed class TraceLoggingViewModel : ViewModelBase, IDisposable
{
    private readonly ITraceExplorerService service;
    private readonly Func<DateTimeOffset> now;
    private DataverseEnvironment? environment;
    private Guid organizationId;
    private TraceLoggingMode mode;
    private CancellationTokenSource? loadCancellation, changeCancellation;
    private bool available, autoRefresh = true;
    private TraceLoggingOption selected;
    public event Action<string>? StatusChanged;
    public static IReadOnlyList<TraceLoggingOption> Options { get; } = new[]
    {
        new TraceLoggingOption("Off", TraceLoggingMode.Off), new TraceLoggingOption("Exceptions", TraceLoggingMode.Exceptions),
        new TraceLoggingOption("All", TraceLoggingMode.All), new TraceLoggingOption("All for 1 hour", TraceLoggingMode.All, true)
    };

    public TraceLoggingViewModel(ITraceExplorerService service, Func<DateTimeOffset> now)
    {
        this.service = service; this.now = now; selected = Options[0];
        ChangeCommand = new AsyncRelayCommand<TraceLoggingOption>(ChangeAsync, option => IsAvailable && option != null);
    }
    public IAsyncRelayCommand<TraceLoggingOption> ChangeCommand { get; }
    public IReadOnlyList<TraceLoggingOption> AvailableOptions => Options;
    public TraceLoggingOption SelectedOption
    {
        get => selected;
        set
        {
            if (value == null || value == selected) return;
            if (IsAvailable) ChangeCommand.Execute(value);
            // Selection is confirmed by ChangeAsync, not by a two-way UI assignment.
            OnPropertyChanged(nameof(SelectedOption));
        }
    }
    public bool IsAvailable { get => available; private set { if (SetProperty(ref available, value)) NotifyState(); } }
    public bool AutoRefresh
    {
        get => autoRefresh;
        set { if (SetProperty(ref autoRefresh, value)) OnPropertyChanged(nameof(CanPoll)); }
    }
    public bool CanPoll => IsAvailable && mode != TraceLoggingMode.Off && AutoRefresh;
    public string AutoRefreshLabel => mode == TraceLoggingMode.Off ? "Auto-refresh (paused)" : "Auto-refresh";
    public string AutoRefreshToolTip => mode == TraceLoggingMode.Off
        ? "Auto-refresh is paused while trace logging is Off. Apply and Refresh remain available." : "Automatically refresh the displayed results";
    private TraceLoggingLease? MatchingLease => environment != null && service.CurrentTraceLoggingLease is { } lease
        && TraceLoggingPolicy.Matches(lease, environment, organizationId) ? lease : null;
    public bool HasTimer => MatchingLease != null;
    public string TimerText => MatchingLease is not { } lease ? "" : lease.ExpiresAtUtc <= now() ? "Restore pending"
        : $"{Math.Ceiling((lease.ExpiresAtUtc - now()).TotalMinutes):0}m left";
    public string TimerToolTip
    {
        get
        {
            if (MatchingLease is not { } lease) return "";
            if (lease.ExpiresAtUtc > now())
                return $"Tracing returns to {lease.RestoreMode} at {lease.ExpiresAtUtc.ToLocalTime():t}. Refresh extends it by one hour.";
            var text = "Timed tracing expired. XrmTools checks for restoration every 30 seconds while Visual Studio is running.";
            if (lease.LastAttemptAtUtc is { } attempt) text += $"\nLast attempt: {attempt.ToLocalTime():G}.";
            if (!string.IsNullOrEmpty(lease.LastError)) text += "\nLast error: " + lease.LastError;
            if (lease.RetryAtUtc is { } retry) text += $"\nNext retry at or shortly after {retry.ToLocalTime():T}.";
            return text;
        }
    }

    public async Task LoadAsync(DataverseEnvironment target)
    {
        // A query refresh must not re-enable the selector or overwrite state during a logging change.
        if (changeCancellation != null) return;
        loadCancellation?.Cancel();
        var cancellation = new CancellationTokenSource(); loadCancellation = cancellation;
        IsAvailable = false;
        try
        {
            var configuration = await service.GetTraceLoggingAsync(target, cancellation.Token);
            if (!cancellation.IsCancellationRequested && SameEnvironment(target))
            {
                Confirm(target, configuration);
                IsAvailable = true;
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { if (!cancellation.IsCancellationRequested) StatusChanged?.Invoke("Could not read trace logging: " + ex.Message); }
        finally
        {
            if (ReferenceEquals(loadCancellation, cancellation)) loadCancellation = null;
            cancellation.Dispose();
        }
    }
    public void SetEnvironment(DataverseEnvironment? target)
    {
        Cancel(); environment = target; organizationId = Guid.Empty; mode = TraceLoggingMode.Off; IsAvailable = false; NotifyState();
    }
    public void Confirm(DataverseEnvironment target, TraceLoggingConfiguration configuration)
    {
        if (!SameEnvironment(target)) return;
        organizationId = configuration.OrganizationId; mode = configuration.Mode; NotifyState();
    }
    public void Tick() => NotifyState();
    public Task ExtendAsync() => environment != null && organizationId != Guid.Empty
        ? service.ExtendTimedTraceLoggingAsync(environment, organizationId) : Task.CompletedTask;

    private async Task ChangeAsync(TraceLoggingOption? option)
    {
        if (option == null || !IsAvailable || environment == null || organizationId == Guid.Empty) return;
        var target = environment; var id = organizationId; var previous = mode;
        var cancellation = new CancellationTokenSource(); changeCancellation = cancellation; IsAvailable = false;
        try
        {
            if (option.Timed) await service.StartTimedTraceLoggingAsync(target, id, previous, cancellation.Token);
            else
            {
                if (option.Mode != previous) await service.SetTraceLoggingAsync(target, id, option.Mode, cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();
                await service.CancelTimedTraceLoggingAsync(target, id);
            }
            if (!cancellation.IsCancellationRequested && SameEnvironment(target))
            {
                mode = option.Mode;
                StatusChanged?.Invoke("Trace logging set to " + option.Label + ".");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { if (!cancellation.IsCancellationRequested) StatusChanged?.Invoke("Could not change trace logging: " + ex.Message); }
        finally
        {
            if (ReferenceEquals(changeCancellation, cancellation)) { changeCancellation = null; IsAvailable = true; }
            cancellation.Dispose(); NotifyState();
        }
    }
    private bool SameEnvironment(DataverseEnvironment target) => environment?.ConnectionString == target.ConnectionString;
    private void NotifyState()
    {
        selected = Options[mode == TraceLoggingMode.All && MatchingLease != null ? 3 : (int)mode];
        foreach (var property in new[] { nameof(SelectedOption), nameof(CanPoll), nameof(AutoRefreshLabel), nameof(AutoRefreshToolTip), nameof(HasTimer), nameof(TimerText), nameof(TimerToolTip) })
            OnPropertyChanged(property);
        ChangeCommand.NotifyCanExecuteChanged();
    }
    public void Cancel()
    {
        loadCancellation?.Cancel(); loadCancellation = null;
        changeCancellation?.Cancel(); changeCancellation = null;
        IsAvailable = false;
    }
    public void Dispose() => Cancel();
}
