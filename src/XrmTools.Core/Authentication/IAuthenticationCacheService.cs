namespace XrmTools.Authentication;

using System.Threading;
using System.Threading.Tasks;

internal interface IAuthenticationCacheService
{
    Task ClearEnvironmentTokenCacheAsync(DataverseEnvironment environment, CancellationToken cancellationToken = default);
}
