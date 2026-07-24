#nullable enable
namespace XrmTools.UI;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

/// <summary>
/// View model for <see cref="RemovedPluginsDialog"/>.
/// </summary>
internal sealed class RemovedPluginsViewModel
{
    public ReadOnlyCollection<RemovedPluginRow> RemovedPlugins { get; }

    public string Headline { get; }

    public RemovedPluginsViewModel(IReadOnlyList<RemovedPluginSummary> removedPlugins)
    {
        RemovedPlugins = new ReadOnlyCollection<RemovedPluginRow>(
            removedPlugins.Select(p => new RemovedPluginRow(p)).ToList());

        var pluginCount = removedPlugins.Count;
        var stepCount = removedPlugins.Sum(p => p.StepCount);
        var pluginWord = pluginCount == 1 ? "plugin" : "plugins";
        var stepWord = stepCount == 1 ? "step" : "steps";

        Headline =
            $"The following {pluginCount} {pluginWord} ({stepCount} {stepWord}) no longer exist in your " +
            "compiled assembly. This usually means the plugin type was renamed or removed in code.";
    }
}

/// <summary>
/// Row-friendly projection of a <see cref="RemovedPluginSummary"/>.
/// </summary>
internal sealed class RemovedPluginRow(RemovedPluginSummary summary)
{
    public string TypeName { get; } = summary.TypeName;

    public int StepCount { get; } = summary.StepCount;

    public string CustomApiText { get; } = summary.HasCustomApi ? "Yes" : "No";
}
#nullable restore
