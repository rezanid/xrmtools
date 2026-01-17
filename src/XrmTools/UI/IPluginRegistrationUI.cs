#nullable enable
namespace XrmTools.UI;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IPluginRegistrationUI
{
    Task<bool> ConfirmDeleteRemovedPluginsAsync(IEnumerable<string> removedTypeNames);
    Task<bool> ConfirmUnregsiterAssemblyAsync(string assemblyName);
}
#nullable restore