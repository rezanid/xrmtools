namespace XrmTools.Xrm.Repositories;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.Http;

internal interface IRepositoryFactory
{
    Task<T> CreateRepositoryAsync<T>() where T : class;
}

[Export(typeof(IRepositoryFactory))]
[method: ImportingConstructor]
internal class RepositoryFactory(IXrmHttpClientFactory httpClientFactory) : IRepositoryFactory
{
    private readonly IXrmHttpClientFactory httpClientFactory = httpClientFactory;

    public async Task<T> CreateRepositoryAsync<T>() where T : class
    {
        var client = await httpClientFactory.CreateClientAsync();
        var type = typeof(T);
        if (type.IsAssignableFrom(typeof(IPluginAssemblyRepository))) return new PluginAssemblyRepository(client) as T;
        if (type.IsAssignableFrom(typeof(IPluginTypeRepository))) return new PluginTypeRepository(client) as T;
        throw new NotSupportedException($"{type.Name} is not a supported repository.");
    }
}
