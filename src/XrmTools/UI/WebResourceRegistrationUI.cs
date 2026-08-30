#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

internal interface IWebResourceRegistrationUI
{
    Task<bool> ConfirmDeleteRemovedWebResourcesAsync(IReadOnlyList<string> removedNames);
}

internal sealed class VsWebResourceRegistrationUI : IWebResourceRegistrationUI
{
    public Task<bool> ConfirmDeleteRemovedWebResourcesAsync(IReadOnlyList<string> removedNames)
    {
        var preview = string.Join("\n", removedNames.Take(12).Select(name => "  • " + name));
        if (removedNames.Count > 12) preview += $"\n  • …and {removedNames.Count - 12} more";
        return VS.MessageBox.ShowConfirmAsync(
            Vsix.Name,
            $"The following {removedNames.Count} web resource(s) exist under this project's ownership prefix " +
            $"but are no longer produced by the project. Delete them from Dataverse?\n\n{preview}");
    }
}
#nullable restore
