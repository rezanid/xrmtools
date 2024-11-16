#nullable enable
namespace XrmTools;
using System.Net.Http;
using System.Threading.Tasks;

public interface IXrmHttpClientFactory
{
    Task<HttpClient> CreateHttpClientAsync();
}
#nullable restore