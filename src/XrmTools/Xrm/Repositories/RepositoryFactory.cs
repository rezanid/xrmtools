namespace XrmTools.Xrm.Repositories;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.Core;
using XrmTools.Core.Repositories;
using XrmTools.Http;

internal interface IRepositoryFactory
{
    Task<T> CreateRepositoryAsync<T>() where T : class, IXrmRepository;
    Task<T> CreateRepositoryAsync<T>(DataverseEnvironment environment) where T : class, IXrmRepository;
}

[Export(typeof(IRepositoryFactory))]
[method: ImportingConstructor]
internal class RepositoryFactory(IXrmHttpClientFactory httpClientFactory) : IRepositoryFactory
{
    private readonly IXrmHttpClientFactory httpClientFactory = httpClientFactory;

    public async Task<T> CreateRepositoryAsync<T>() where T : class, IXrmRepository
        => CreateRepositoryInstance<T>(await httpClientFactory.CreateClientAsync(), typeof(T));

    public async Task<T> CreateRepositoryAsync<T>(DataverseEnvironment environment) where T : class, IXrmRepository
        => CreateRepositoryInstance<T>(await httpClientFactory.CreateClientAsync(environment), typeof(T));

    private static T CreateRepositoryInstance<T>(XrmHttpClient client, Type type) where T : class, IXrmRepository
    {
        if (type.IsAssignableFrom(typeof(IPluginAssemblyRepository))) return new PluginAssemblyRepository(client) as T;
        if (type.IsAssignableFrom(typeof(IPluginTypeRepository))) return new PluginTypeRepository(client) as T;
        if (type.IsAssignableFrom(typeof(IEntityMetadataRepository))) return new EntityMetadataRepository(client) as T;
        if (type.IsAssignableFrom(typeof(ISystemRepository))) return new SystemRepository(client) as T;
        if (type.IsAssignableFrom(typeof(ISdkMessageRepository))) return new SdkMessageRepository(client) as T;
        throw new NotSupportedException($"{type.Name} is not a supported repository.");
    }
}