#nullable enable
namespace XrmTools.Http;
using System.Threading.Tasks;

internal interface IXrmHttpClientFactory
{
    Task<XrmHttpClient> CreateClientAsync();
    Task<XrmHttpClient> CreateClientAsync(DataverseEnvironment environment);
}
#nullable restore