#nullable enable
namespace XrmTools.DataverseSolutions;

using Microsoft.VisualStudio.ProjectSystem;
using System.ComponentModel.Composition;

[Export]
[AppliesTo(CdsProjectRegistration.UniqueCapability)]
internal sealed class CdsProjectUnconfigured
{
    [ImportingConstructor]
    public CdsProjectUnconfigured(UnconfiguredProject unconfiguredProject)
    {
        UnconfiguredProject = unconfiguredProject;
    }

    internal UnconfiguredProject UnconfiguredProject { get; }
}
#nullable restore
