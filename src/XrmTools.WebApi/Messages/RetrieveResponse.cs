#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;


/// <summary>
/// Contains the data from the RetrieveRequest
/// </summary>
/// <remarks>
/// This class must be instantiated by either <see cref="IWebApiService.SendAsync{T}(HttpRequestMessage)"/>
/// or <see cref="WebApi.Extensions.As{T}(HttpResponseMessage)"/>
/// </remarks>
public sealed class RetrieveResponse : HttpResponseMessage
{
    public async Task<JObject> GetRecordAsync() => JObject.Parse(await Content.ReadAsStringAsync());
}

public sealed class RetrieveResponse<T> : HttpResponseMessage
{
    /// <summary>
    /// The record returned.
    /// </summary>
    public async Task<T?> GetRecordAsync()
        => await JsonSerializer.DeserializeAsync<T>(await Content.ReadAsStreamAsync()).ConfigureAwait(false);
}
#nullable restore