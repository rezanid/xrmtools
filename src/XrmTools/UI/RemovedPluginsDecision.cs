#nullable enable
namespace XrmTools.UI;

/// <summary>
/// Represents the user's decision when existing Dataverse plugin registrations are found whose
/// plugin types no longer exist in the compiled assembly (renamed or removed in code).
/// </summary>
public enum RemovedPluginsDecision
{
    /// <summary>
    /// Authoritative sync: delete the registrations whose plugin types no longer exist in the compiled
    /// assembly (renamed or removed in code) so that code remains the single source of truth.
    /// </summary>
    Delete,

    /// <summary>
    /// Abort the whole registration operation (e.g. so the developer can restore or rename the type in code).
    /// </summary>
    Cancel
}

/// <summary>
/// Summarizes an existing Dataverse plugin registration that is no longer present in code,
/// used to inform the user before any deletion takes place.
/// </summary>
/// <param name="TypeName">The plugin type name (or display name when the type name is unavailable).</param>
/// <param name="StepCount">Number of SDK message processing steps that would be removed.</param>
/// <param name="HasCustomApi">Whether the plugin is associated with a Custom API that would be removed.</param>
public sealed record RemovedPluginSummary(string TypeName, int StepCount, bool HasCustomApi);
#nullable restore
