using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public class MetadataCache<T>(Func<CancellationToken,Task<T>> dataFetcher)
{
    private T _data;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private bool _isInitialized = false;

    public async Task<T> GetDataAsync(CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            if (!_isInitialized)
            {
                _data = await dataFetcher(cancellationToken);
                _isInitialized = true;
            }
            return _data;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    //TODO: We need a new MetadataCache that can handle multiple values
    // Example of returning:
    // await foreach (var value in metadataCache.GetValuesAsync()){
    //   _collection.Add(value);
    //   yield return value;
    // }
    public async IAsyncEnumerable<T> GetValuesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            if (!_isInitialized)
            {
                _data = await dataFetcher(cancellationToken);
                _isInitialized = true;
            }
            yield return _data;
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
                    _data = await dataFetcher(CancellationToken.None);
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
            _data = await dataFetcher(CancellationToken.None);
            _isInitialized = true;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}