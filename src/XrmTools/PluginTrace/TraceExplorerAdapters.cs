#nullable enable
namespace XrmTools.PluginTrace;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

internal interface ITraceViewStore
{
    Task<IReadOnlyList<TraceFilter>> LoadAsync();
    Task SaveAsync(IReadOnlyList<TraceFilter> views);
}

internal sealed class TraceViewStore : ITraceViewStore
{
    public async Task<IReadOnlyList<TraceFilter>> LoadAsync()
    {
        var options = await TraceExplorerOptions.GetLiveInstanceAsync();
        return JsonSerializer.Deserialize<List<TraceFilter>>(options.SavedViewsJson) ?? new List<TraceFilter>();
    }

    public async Task SaveAsync(IReadOnlyList<TraceFilter> views)
    {
        var options = await TraceExplorerOptions.GetLiveInstanceAsync();
        options.SavedViewsJson = JsonSerializer.Serialize(views);
        await options.SaveAsync();
    }
}

internal interface ITraceNavigator
{
    Task<string> NavigateAsync(string typeName, CancellationToken cancellation);
}

internal sealed class TraceNavigator : ITraceNavigator
{
    public Task<string> NavigateAsync(string typeName, CancellationToken cancellation) =>
        TraceDefinitionNavigator.NavigateAsync(typeName, cancellation);
}

internal interface ITraceDispatcher
{
    void Post(Action action);
}

internal sealed class TraceDispatcher(Dispatcher dispatcher) : ITraceDispatcher
{
    public void Post(Action action) => _ = dispatcher.BeginInvoke(action);
}
