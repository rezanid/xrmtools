using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

public class MetadataCache<T>(Func<Task<T>> dataFetcher)
{
    private T _data;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private bool _isInitialized = false;

    public async Task<T> GetDataAsync()
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            if (!_isInitialized)
            {
                _data = await dataFetcher();
                _isInitialized = true;
            }
            return _data;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public T GetData()
    {
        _semaphoreSlim.Wait();
        try
        {
            if (!_isInitialized)
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    _data = await dataFetcher();
                    _isInitialized = true;
                });
            }
            return _data;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task RefreshDataAsync()
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            _data = await dataFetcher();
            _isInitialized = true;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}