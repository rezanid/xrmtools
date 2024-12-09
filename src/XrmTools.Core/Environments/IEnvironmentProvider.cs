#nullable enable
namespace XrmTools.Environments;
using System.Threading.Tasks;

public interface IEnvironmentProvider
{
    DataverseEnvironment? GetActiveEnvironment();
    Task<DataverseEnvironment?> GetActiveEnvironmentAsync();
    Task SetActiveEnvironmentAsync(DataverseEnvironment environment);
}
#nullable restore