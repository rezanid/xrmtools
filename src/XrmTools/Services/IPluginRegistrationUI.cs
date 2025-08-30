#nullable enable
namespace XrmTools.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IPluginRegistrationUI
{
    Task<bool> ConfirmRemovePluginsAsync(IEnumerable<string> removedTypeNames, CancellationToken cancellationToken);
}
#nullable restore