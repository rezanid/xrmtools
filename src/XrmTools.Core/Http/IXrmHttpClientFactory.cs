#nullable enable
namespace XrmTools.Http;
using System.Threading.Tasks;

internal interface IXrmHttpClientFactory
{
    Task<XrmHttpClient> CreateClientAsync();
}
#nullable restore