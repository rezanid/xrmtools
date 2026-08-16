#nullable enable
namespace XrmTools.DataverseSolutions;

using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.ProjectSystem;
using System.ComponentModel.Composition;

[Export(typeof(IProjectTreePropertiesProvider))]
[AppliesTo(CdsProjectRegistration.UniqueCapability)]
[Order(100)]
internal sealed class CdsProjectTreePropertiesProvider : IProjectTreePropertiesProvider
{
    public void CalculatePropertyValues(
        IProjectTreeCustomizablePropertyContext propertyContext,
        IProjectTreeCustomizablePropertyValues propertyValues)
    {
        if (!propertyValues.Flags.Contains(ProjectTreeFlags.Common.ProjectRoot))
        {
            return;
        }

        var moniker = new ImageMoniker
        {
            Guid = PackageGuids.AssetsGuid,
            Id = PackageIds.DataverseSolution
        };

        propertyValues.Icon = moniker.ToProjectSystemType();
        propertyValues.ExpandedIcon = moniker.ToProjectSystemType();
    }
}
#nullable restore
