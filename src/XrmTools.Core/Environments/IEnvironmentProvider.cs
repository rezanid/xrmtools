#nullable enable
namespace XrmTools.Environments;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IEnvironmentProvider
{
    Task<DataverseEnvironment?> GetActiveEnvironmentAsync();
    Task<IList<DataverseEnvironment>> GetAvailableEnvironmentsAsync();
    Task SetActiveEnvironmentAsync(DataverseEnvironment environment);
}
#nullable restore