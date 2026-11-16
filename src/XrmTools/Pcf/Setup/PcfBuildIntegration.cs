#nullable enable
namespace XrmTools.Pcf.Setup;

using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.DataverseSolutions;
using XrmTools.Logging;

/// <summary>Places the generated build targets for a loaded PCF project. Loading a project never installs anything.</summary>
[Export]
[AppliesTo(PcfProjectRegistration.UniqueCapability)]
internal sealed class PcfProjectBuildIntegration
{
    private readonly UnconfiguredProject _project;
    private readonly IOutputLoggerService _output;
    private readonly PcfBuildCoordinator _coordinator;
    private bool _reported;

    [ImportingConstructor]
    public PcfProjectBuildIntegration(UnconfiguredProject project, IOutputLoggerService output, PcfBuildCoordinator coordinator)
    {
        _project = project; _output = output; _coordinator = coordinator;
    }

    [ProjectAutoLoad(ProjectLoadCheckpoint.ProjectFactoryCompleted)]
    [AppliesTo(PcfProjectRegistration.UniqueCapability)]
    public Task OnProjectLoadedAsync()
    {
        // Builds must be seen from the first one, even before the package has finished loading. Not awaited: project
        // load may itself be holding the UI thread.
        ThreadHelper.JoinableTaskFactory.RunAsync(_coordinator.EnsureAdvisedAsync).FileAndForget("XrmTools/Pcf/Advise");
        return EnsureTargetsAsync();
    }

    public async Task EnsureTargetsAsync()
    {
        try
        {
            string? extensions = null;
            var configured = await _project.GetSuggestedConfiguredProjectAsync().ConfigureAwait(false);
            if (configured?.Services.ProjectPropertiesProvider is { } provider)
                extensions = await provider.GetCommonProperties().GetEvaluatedPropertyValueAsync("MSBuildProjectExtensionsPath").ConfigureAwait(false);
            var file = PcfBuildTargets.FileFor(_project.FullPath, extensions);
            if (PcfBuildTargets.Ensure(file) || _reported) return;
            _reported = true;
            _output.LogWarning($"Xrm Tools could not write '{file}'. PCF builds of '{_project.FullPath}' run without setup checks.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            if (_reported) return;
            _reported = true;
            _output.LogWarning($"Xrm Tools could not prepare PCF build checks for '{_project.FullPath}': {ex.Message}");
        }
    }
}

/// <summary>Hands each real (not design-time) build of a PCF project the decision made for the current solution build.</summary>
[ExportBuildGlobalPropertiesProvider]
[AppliesTo(PcfProjectRegistration.UniqueCapability)]
internal sealed class PcfBuildGlobalPropertiesProvider : StaticGlobalPropertiesProviderBase
{
    private readonly UnconfiguredProject _project;
    private readonly PcfBuildCoordinator _coordinator;
    private readonly PcfProjectBuildIntegration _integration;

    [ImportingConstructor]
    public PcfBuildGlobalPropertiesProvider(UnconfiguredProject project, PcfBuildCoordinator coordinator, PcfProjectBuildIntegration integration) : base(project.Services)
    {
        _project = project; _coordinator = coordinator; _integration = integration;
    }

    public override async Task<IImmutableDictionary<string, string>> GetGlobalPropertiesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // obj may have been deleted since the project loaded; MSBuild evaluates the project again for this build.
            await _integration.EnsureTargetsAsync().ConfigureAwait(false);
            var properties = Empty.PropertiesMap;
            foreach (var property in await _coordinator.GetBuildPropertiesAsync(_project.FullPath, cancellationToken).ConfigureAwait(false))
            {
                // The list of target names keeps its semicolons; every other value is text for a message.
                properties = properties.SetItem(property.Key,
                    property.Key == PcfBuildTargets.GateTargetsProperty ? property.Value : ProjectCollection.Escape(property.Value));
            }
            return properties;
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not OutOfMemoryException)
        {
            // Without these properties the generated targets stay inert and the build behaves as it did before.
            return Empty.PropertiesMap;
        }
    }
}
#nullable restore
