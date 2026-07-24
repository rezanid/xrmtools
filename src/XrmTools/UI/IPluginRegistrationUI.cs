#nullable enable
namespace XrmTools.UI;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IPluginRegistrationUI
{
    /// <summary>
    /// Prompts the user to decide what to do with existing Dataverse plugin registrations whose plugin
    /// types no longer exist in the compiled assembly (renamed or removed in code). Implementations may
    /// honor user preferences that suppress the prompt.
    /// </summary>
    /// <param name="removedPlugins">The registrations that would be removed by an authoritative sync.</param>
    /// <returns>The user's decision (delete or cancel).</returns>
    Task<RemovedPluginsDecision> ConfirmRemovedPluginsAsync(IReadOnlyList<RemovedPluginSummary> removedPlugins);
    Task<bool> ConfirmUnregsiterAssemblyAsync(string assemblyName);
}
#nullable restore