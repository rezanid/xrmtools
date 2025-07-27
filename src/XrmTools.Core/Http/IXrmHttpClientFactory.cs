#nullable enable
namespace XrmTools.Http;
using System.Threading.Tasks;

public interface IXrmHttpClientFactory
{
    Task<XrmHttpClient> CreateClientAsync();
    Task<XrmHttpClient> CreateClientAsync(DataverseEnvironment environment);
}
#nullable restore