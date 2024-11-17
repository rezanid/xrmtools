namespace XrmTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Threading;
using System.Threading.Tasks;

public class AsyncTimer : IDisposable
{
    private readonly ITimer timer;
    private readonly Func<CancellationToken, Task> callback;
    private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    private volatile bool isRunning;

    public AsyncTimer(Func<CancellationToken, Task> callback, TimeProvider timeProvider)
    {
        this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        this.timer = timeProvider.CreateTimer(this.OnTimerElapsed);
    }

    public void Start(TimeSpan interval)
    {
        timer.Change(interval, interval);
    }

    public void Stop()
    {
        timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    private async void OnTimerElapsed(object state)
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
        timer.Dispose();
        cancellationSource.Dispose();
        semaphore.Dispose();
    }
}

//public class TimerTimer : ITimer
//{
//    private readonly Timer timer;

//    public TimerTimer(TimerCallback callback)
//    {
//        timer = new Timer(callback);
//    }

//    public void Change(TimeSpan dueTime, TimeSpan period)
//    {
//        timer.Change(dueTime, period);
//    }

//    public void Dispose()
//    {
//        timer.Dispose();
//    }
//}
