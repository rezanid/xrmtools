namespace XrmTools.Core.Tests;
using System.Threading.Tasks;
using System.Threading;
using System;
using Xunit;

public class AsyncDictionaryTests
{
    [Fact]
    public async Task GetOrAddAsync_AddsAndReturnsValue()
    {
        var dictionary = new AsyncDictionary<string, int>();
        int value = await dictionary.GetOrAddAsync("key", async _ => 42);

        Assert.Equal(42, value);
    }

    [Fact]
    public async Task GetOrAddAsync_ReturnsExistingValueIfPresent()
    {
        var dictionary = new AsyncDictionary<string, int>();
        await dictionary.GetOrAddAsync("key", async _ => 42);
        int value = await dictionary.GetOrAddAsync("key", async _ => 84);

        Assert.Equal(42, value);
    }

    [Fact]
    public async Task GetOrAddAsync_CallsValueFactoryOnlyOnce_PerKey()
    {
        var dictionary = new AsyncDictionary<string, int>();
        int factoryCallCount = 0;
        Func<string, Task<int>> factory = async _ =>
        {
            factoryCallCount++;
            await Task.Delay(10);
            return 42;
        };

        var tasks = new[]
        {
            dictionary.GetOrAddAsync("key", factory),
            dictionary.GetOrAddAsync("key", factory),
            dictionary.GetOrAddAsync("key", factory)
        };

        int[] results = await Task.WhenAll(tasks);

        Assert.All(results, r => Assert.Equal(42, r));
        Assert.Equal(1, factoryCallCount); // Factory should be called only once
    }

    [Fact]
    public async Task GetOrAddAsync_WithCancellationToken_CancelsCorrectly()
    {
        var dictionary = new AsyncDictionary<string, int>();
        var cts = new CancellationTokenSource();

        Func<string, CancellationToken, Task<int>> factory = async (_, token) =>
        {
            await Task.Delay(1000, token); // Simulate long-running task
            return 42;
        };

        var task = dictionary.GetOrAddAsync("key", factory, cts.Token);

        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() => task);
    }

    [Fact]
    public async Task TryRemove_RemovesExistingValue()
    {
        var dictionary = new AsyncDictionary<string, int>();
        await dictionary.GetOrAddAsync("key", async _ => 42);

        bool removed = dictionary.TryRemove("key", out int value);

        Assert.True(removed);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryRemove_ReturnsFalseWhenKeyDoesNotExist()
    {
        var dictionary = new AsyncDictionary<string, int>();

        bool removed = dictionary.TryRemove("nonexistentKey", out int value);

        Assert.False(removed);
        Assert.Equal(0, value); // Default int value
    }

    [Fact]
    public async Task GetOrAddAsync_AllowsReAddingAfterRemoval()
    {
        var dictionary = new AsyncDictionary<string, int>();
        await dictionary.GetOrAddAsync("key", async _ => 42);
        dictionary.TryRemove("key", out _);

        int newValue = await dictionary.GetOrAddAsync("key", async _ => 84);

        Assert.Equal(84, newValue);
    }

    [Theory]
    [InlineData("key1", 42)]
    [InlineData("key2", 84)]
    [InlineData("key3", 126)]
    public async Task GetOrAddAsync_WithDifferentKeys_AddsAndReturnsCorrectValues(string key, int expectedValue)
    {
        var dictionary = new AsyncDictionary<string, int>();
        int value = await dictionary.GetOrAddAsync(key, async _ => expectedValue);

        Assert.Equal(expectedValue, value);
    }
}