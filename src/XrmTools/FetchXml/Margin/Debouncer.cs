namespace XrmTools.FetchXml.Margin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal class Debouncer(int millisecondsToWait = 500) : IDisposable
{
    private readonly ConcurrentDictionary<object, CancellationTokenSource> _debouncers = new();
    private readonly int _millisecondsToWait = millisecondsToWait;
    private readonly object _lockThis = new(); // retained for the sync Debounce
    private readonly SemaphoreSlim _semaphore = new(1, 1); // async-friendly lock replacement

    public void Debounce(Action func, object key = null)
    {
        key ??= "default";

        // Cancel previous debouncer for this key
        if (_debouncers.TryGetValue(key, out CancellationTokenSource existingToken))
        {
            existingToken.Cancel();
            existingToken.Dispose();
        }

        CancellationTokenSource newTokenSrc = new();
        _debouncers[key] = newTokenSrc;

        _ = Task.Delay(_millisecondsToWait, newTokenSrc.Token).ContinueWith(task =>
        {
            if (!newTokenSrc.IsCancellationRequested)
            {
                // Remove from dictionary and cleanup
                _debouncers.TryRemove(key, out _);
                lock (_lockThis)
                {
                    if (!newTokenSrc.IsCancellationRequested)
                    {
                        func(); // run
                    }
                }
            }
            newTokenSrc.Dispose();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    public void Debounce(Func<Task> func, object key = null)
    {
        key ??= "default";

        // Cancel previous debouncer for this key
        if (_debouncers.TryGetValue(key, out CancellationTokenSource existingToken))
        {
            existingToken.Cancel();
            existingToken.Dispose();
        }

        CancellationTokenSource newTokenSrc = new();
        _debouncers[key] = newTokenSrc;

        _ = Task.Delay(_millisecondsToWait, newTokenSrc.Token).ContinueWith(async task =>
        {
            if (!newTokenSrc.IsCancellationRequested)
            {
                // Remove from dictionary and cleanup
                _debouncers.TryRemove(key, out _);

                bool lockTaken = false;
                //await _semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    await _semaphore.WaitAsync(newTokenSrc.Token).ConfigureAwait(false);
                    lockTaken = true;
                    if (!newTokenSrc.IsCancellationRequested)
                    {
                        try
                        {
                            await func().ConfigureAwait(false); // run
                        }
                        catch
                        {
                            // Intentionally swallow to avoid UnobservedTaskException.
                            //TODO: add logging.
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Superseded or disposed.
                }
                catch (ObjectDisposedException)
                {
                    // Semaphore disposed during shutdown.
                }
                finally
                {
                    if (lockTaken)
                    {
                        try
                        {
                            _semaphore.Release();
                        }
                        catch (ObjectDisposedException) { }
                    }
                }
            }

            newTokenSrc.Dispose();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Unwrap();
    }

    // Overload to support cancellable tasks: Func<CancellationToken, Task>
    public void Debounce(Func<CancellationToken, Task> func, object key = null)
    {
        key ??= "default";

        if (_debouncers.TryGetValue(key, out CancellationTokenSource existingToken))
        {
            // Cancel previous debounced operation; if it already started, it can observe this token.
            existingToken.Cancel();
            // Intentionally not disposing here; allow the running operation to finish and dispose its own CTS.
        }

        CancellationTokenSource newTokenSrc = new();
        _debouncers[key] = newTokenSrc;

        _ = Task.Delay(_millisecondsToWait, newTokenSrc.Token).ContinueWith(async task =>
        {
            if (!newTokenSrc.IsCancellationRequested)
            {
                _debouncers.TryRemove(key, out _);

                //await _semaphore.WaitAsync().ConfigureAwait(false);
                bool lockTaken = false;
                try
                {
                    await _semaphore.WaitAsync(newTokenSrc.Token).ConfigureAwait(false);
                    lockTaken = true;
                    if (!newTokenSrc.IsCancellationRequested)
                    {
                        try
                        {
                            await func(newTokenSrc.Token).ConfigureAwait(false);
                        }
                        catch
                        {
                            // Intentionally swallow to avoid UnobservedTaskException.
                            // TODO: add logging.
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Superseded or disposed.
                }
                catch (ObjectDisposedException)
                {
                    // Semaphore disposed during shutdown.
                }
                finally
                {
                    if (lockTaken)
                    {
                        try { _semaphore.Release(); } catch (ObjectDisposedException) { }
                    }
                }
            }

            newTokenSrc.Dispose();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Unwrap();
    }

    public void Dispose()
    {
        foreach (KeyValuePair<object, CancellationTokenSource> kvp in _debouncers)
        {
            kvp.Value.Cancel();
            kvp.Value.Dispose();
        }
        _debouncers.Clear();
        _semaphore.Dispose();
    }
}