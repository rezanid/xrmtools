#nullable enable
namespace XrmTools.DataverseSolutions;

using Community.VisualStudio.Toolkit;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

[Export(typeof(IPacAuthUserInteraction))]
internal sealed class VsPacAuthUserInteraction : IPacAuthUserInteraction
{
    public Task<bool> ConfirmProfileCreationAsync(string environmentUrl, bool browserSignInMayBeRequired)
    {
        var browserLine = browserSignInMayBeRequired
            ? "PAC may open a browser for sign-in."
            : "PAC will use the configured non-interactive credentials."
            ;

        var message = $"XrmTools is connected to {environmentUrl}.\r\n\r\nPAC needs a matching auth profile. XrmTools can create/select a PAC profile so PAC commands target the current environment.\r\n\r\n{browserLine}";
        return VS.MessageBox.ShowConfirmAsync(Vsix.Name, message);
    }
}
#nullable restore
