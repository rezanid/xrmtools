using System;
using System.Threading;
using System.Threading.Tasks;

public class MetadataCache<T>(Func<Task<T>> dataFetcher)
{
    private T _data;
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
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
