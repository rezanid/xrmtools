namespace XrmTools.Xrm.Repositories;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Xrm.Model;

internal interface IPluginAssemblyRepository
{
    Task<IEnumerable<PluginAssemblyConfig>> GetAsync(CancellationToken cancellationToken);
}

internal class PluginAssemblyRepository(HttpClient client) : IPluginAssemblyRepository
{
    private readonly HttpClient client = client;

    public async Task<IEnumerable<PluginAssemblyConfig>> GetAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync("pluginassemblies", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<IEnumerable<PluginAssemblyConfig>>();
        }
        return [];
    }

}