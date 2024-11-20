#nullable enable
namespace XrmTools.Http;
using System.Net.Http;
using System.Threading.Tasks;

public interface IXrmHttpClientFactory
{
    Task<XrmHttpClient> CreateHttpClientAsync();
}
#nullable restore