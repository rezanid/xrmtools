#nullable enable
namespace XrmTools.DataverseSolutions;

using Microsoft.VisualStudio.ProjectSystem;
using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

[Export(ExportContractNames.Scopes.ConfiguredProject, typeof(IProjectCapabilitiesProvider))]
[AppliesTo(CdsProjectRegistration.UniqueCapability + " & !.NET")]
[SupportsFileExtension(".cdsproj")]
internal sealed class CdsPackageReferenceCapabilitiesProvider : ConfiguredProjectCapabilitiesProviderBase
{
    [ImportingConstructor]
    public CdsPackageReferenceCapabilitiesProvider(ConfiguredProject project)
        : base(nameof(CdsPackageReferenceCapabilitiesProvider), project)
    {
    }

    protected override Task<ImmutableHashSet<string>> GetCapabilitiesAsync(CancellationToken cancellationToken)
        => ProjectLockService.ReadLockAsync(async access =>
        {
            var project = await access.GetProjectAsync(ConfiguredProject);
            return GetCapabilities(project.GetPropertyValue("RestoreProjectStyle"), project.FullPath);
        }, cancellationToken);

    internal static ImmutableHashSet<string> GetCapabilities(string restoreProjectStyle, string projectPath)
    {
        // NuGet's CPS provider requires this capability in addition to RestoreProjectStyle.
        // Do not opt projects with existing packages.config installations into a different provider.
        return string.Equals(restoreProjectStyle, "PackageReference", StringComparison.OrdinalIgnoreCase)
            && !File.Exists(Path.Combine(Path.GetDirectoryName(projectPath)!, "packages.config"))
            ? Empty.CapabilitiesSet.Add("PackageReferences")
            : Empty.CapabilitiesSet;
    }
}
#nullable restore
