#nullable enable
namespace XrmTools;
using System;
using System.Threading.Tasks;
using System.Threading;

public class AsyncTimer(Func<CancellationToken, Task> callback, TimeProvider? timeProvider = null) : IDisposable
{
    private readonly Func<CancellationToken, Task> callback = callback ?? throw new ArgumentNullException(nameof(callback));
    private readonly TimeProvider timeProvider = timeProvider ?? TimeProvider.System;
    private readonly CancellationTokenSource cancellationSource = new();
    private readonly SemaphoreSlim semaphore = new(1, 1);
    private IDisposable? internalTimer;
    private bool isRunning;

    public AsyncTimer(Func<CancellationToken, Task> callback, TimeSpan interval, TimeProvider? timeProvider = null) : this(callback, timeProvider)
        => Start(interval);
    public AsyncTimer(Func<CancellationToken, Task> callback, TimeSpan dueTime, TimeSpan period, TimeProvider? timeProvider = null) : this(callback, timeProvider)
        => Start(dueTime, period);

    public void Start(TimeSpan interval) => Start(interval, interval);

    public void Start(TimeSpan dueTime, TimeSpan period)
    {
        if (internalTimer != null)
        {
            throw new InvalidOperationException("Timer is already running.");
        }
        internalTimer = timeProvider.CreateTimer(OnTimerElapsed, null, dueTime, period);
    }

    public void Stop()
    {
        cancellationSource.Cancel();
        internalTimer?.Dispose();
        internalTimer = null;
    }

    private async void OnTimerElapsed(object? state)
    {
        if (isRunning) return;

        isRunning = true;

        try
        {
            await semaphore.WaitAsync();

            if (cancellationSource.IsCancellationRequested)
                return;

            await callback(cancellationSource.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AsyncTimer callback: {ex}");
        }
        finally
        {
            isRunning = false;
            semaphore.Release();
        }
    }

    public void Dispose()
    {
        Stop();
        cancellationSource.Cancel();
        cancellationSource.Dispose();
        semaphore.Dispose();
    }
}
#nullable restore