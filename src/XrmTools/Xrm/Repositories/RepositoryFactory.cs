namespace XrmTools.Xrm.Repositories;

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.Core;
using XrmTools.Core.Repositories;
using XrmTools.Http;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi;

internal interface IRepositoryFactory
{
    Task<T> CreateRepositoryAsync<T>() where T : class, IXrmRepository;
    Task<T> CreateRepositoryAsync<T>(DataverseEnvironment environment) where T : class, IXrmRepository;
}

[Export(typeof(IRepositoryFactory))]
[method: ImportingConstructor]
//TODO: Adapt to use IWebApiService in all repositories and eventually remove XrmHttpClientFactory from constructor.
internal class RepositoryFactory(IXrmHttpClientFactory httpClientFactory, IWebApiService service, ILogger<RepositoryFactory> logger) : IRepositoryFactory
{
    private readonly IXrmHttpClientFactory httpClientFactory = httpClientFactory;
    private readonly ILogger<RepositoryFactory> logger = logger;

    public async Task<T> CreateRepositoryAsync<T>() where T : class, IXrmRepository
        => CreateRepositoryInstance<T>(await httpClientFactory.CreateClientAsync(), service, typeof(T), logger);

    public async Task<T> CreateRepositoryAsync<T>(DataverseEnvironment environment) where T : class, IXrmRepository
        => CreateRepositoryInstance<T>(await httpClientFactory.CreateClientAsync(environment), service, typeof(T), logger);

    private static T CreateRepositoryInstance<T>(XrmHttpClient client, IWebApiService service, Type type, ILogger logger) where T : class, IXrmRepository
    {
        if (type.IsAssignableFrom(typeof(IPluginAssemblyRepository))) return new PluginAssemblyRepository(service, logger) as T;
        if (type.IsAssignableFrom(typeof(IPluginTypeRepository))) return new PluginTypeRepository(service, logger) as T;
        if (type.IsAssignableFrom(typeof(IEntityMetadataRepository))) return new EntityMetadataRepository(service, logger) as T;
        if (type.IsAssignableFrom(typeof(ISystemRepository))) return new SystemRepository(service, logger) as T;
        if (type.IsAssignableFrom(typeof(ISdkMessageRepository))) return new SdkMessageRepository(service, logger) as T;
        throw new NotSupportedException($"{type.Name} is not a supported repository.");
    }
}