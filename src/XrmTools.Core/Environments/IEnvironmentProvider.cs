#nullable enable
namespace XrmTools.Environments;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IEnvironmentProvider
{
    Task<DataverseEnvironment?> GetActiveEnvironmentAsync(bool allowInteraction);
    Task<IList<DataverseEnvironment>> GetAvailableEnvironmentsAsync();
    Task SetActiveEnvironmentAsync(DataverseEnvironment environment, bool allowInteraction);
}
#nullable restore