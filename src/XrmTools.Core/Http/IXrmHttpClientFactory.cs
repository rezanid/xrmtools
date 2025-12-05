#nullable enable
namespace XrmTools.Http;

using Microsoft.Identity.Client;
using System.Threading;
using System.Threading.Tasks;

public interface IXrmHttpClientFactory
{
    Task<XrmHttpClient> CreateClientAsync();
    Task<XrmHttpClient> CreateClientAsync(DataverseEnvironment environment);
    Task<AuthenticationResult?> PreAuthenticateAsync(DataverseEnvironment environment, bool allowInteraction, CancellationToken cancellationToken = default);
}
#nullable restore