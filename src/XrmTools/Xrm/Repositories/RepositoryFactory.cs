namespace XrmTools.Xrm.Repositories;

using System;
using System.ComponentModel.Composition;
using XrmTools.Core;
using XrmTools.Core.Repositories;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi;

internal interface IRepositoryFactory
{
    T CreateRepository<T>() where T : class, IXrmRepository;
    T CreateRepository<T>(DataverseEnvironment environment) where T : class, IXrmRepository;
}

[Export(typeof(IRepositoryFactory))]
[method: ImportingConstructor]
internal class RepositoryFactory(IWebApiService service, ILogger<RepositoryFactory> logger, XrmTools.Environments.IEnvironmentProvider envProvider) : IRepositoryFactory
{
    private readonly ILogger<RepositoryFactory> logger = logger;
    private readonly XrmTools.Environments.IEnvironmentProvider environmentProvider = envProvider;

    public T CreateRepository<T>() where T : class, IXrmRepository
        => CreateRepositoryInstance<T>(service, typeof(T), logger, environmentProvider);

    // Keep overload for compatibility but avoid creating a client here as well.
    public T CreateRepository<T>(DataverseEnvironment environment) where T : class, IXrmRepository
        => CreateRepositoryInstance<T>(service, typeof(T), logger, environmentProvider);

    private static T CreateRepositoryInstance<T>(IWebApiService service, Type type, ILogger logger, XrmTools.Environments.IEnvironmentProvider envProvider) where T : class, IXrmRepository
    {
        if (type.IsAssignableFrom(typeof(IPluginAssemblyRepository))) return new PluginAssemblyRepository(service, logger) as T;
        if (type.IsAssignableFrom(typeof(IPluginTypeRepository))) return new PluginTypeRepository(service, logger) as T;
        if (type.IsAssignableFrom(typeof(IEntityMetadataRepository)))
        {
            var synchronizer = new XrmTools.Core.Repositories.RetrieveMetadataChangesSynchronizer(service, envProvider, logger);
            return new XrmTools.Core.Repositories.LocalEntityMetadataRepository(service, envProvider, logger, synchronizer) as T;
        }
        if (type.IsAssignableFrom(typeof(ISystemRepository))) return new SystemRepository(service, logger) as T;
        if (type.IsAssignableFrom(typeof(ISdkMessageRepository))) return new SdkMessageRepository(service, logger) as T;
        throw new NotSupportedException($"{type.Name} is not a supported repository.");
    }
}