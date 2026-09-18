#nullable enable
namespace XrmTools.Environments;
using System.Threading.Tasks;

/// <summary>Reads configuration without authenticating or displaying sign-in UI.</summary>
internal interface IEnvironmentSelection
{
    Task<DataverseEnvironment?> GetSelectedEnvironmentAsync();
}
