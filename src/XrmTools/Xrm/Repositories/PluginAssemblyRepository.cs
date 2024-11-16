namespace XrmTools.Xrm.Repositories;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Xrm.Auth;
using XrmTools.Xrm.Model;

internal interface IPluginAssemblyRepository
{
    Task<IEnumerable<PluginAssemblyConfig>> GetAsync(CancellationToken cancellationToken);
}

internal abstract class PowerPlatformRepository
{
    protected readonly HttpClient client;
    public PowerPlatformRepository(XrmHttpClientFactory httpClientFactory,
        AuthenticationService authenticationService,
        AuthenticationParameters parameters)
    {
        //var authResult = await authenticationService.AuthenticateAsync(parameters, msg => Console.WriteLine(msg), CancellationToken.None);
        //client = httpClientFactory.CreateClient(parameters.Resource);
        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", );
    }
}

[Export(typeof(IPluginAssemblyRepository))]
internal class PluginAssemblyRepository : IPluginAssemblyRepository
{
    private readonly HttpClient client;

    [ImportingConstructor]
    public PluginAssemblyRepository(XrmHttpClientFactory httpClientFactory,
        AuthenticationService authenticationService,
        AuthenticationParameters parameters)
    {
        //client = httpClientFactory.CreateClient(parameters.Resource);
        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationService.AuthenticateAsync(parameters, msg => Console.WriteLine(msg), CancellationToken.None).Result.AccessToken);
    }

    public async Task<IEnumerable<PluginAssemblyConfig>> GetAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync("api/data/v9.2/pluginassemblies", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<IEnumerable<PluginAssemblyConfig>>();
        }
        return [];
    }

}