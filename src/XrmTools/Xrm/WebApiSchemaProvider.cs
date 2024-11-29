namespace XrmTools.Xrm;

using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XrmTools;
using XrmTools.Xrm.Model;

internal class WebApiSchemaProvider(
    DataverseEnvironment environment
    ) : IXrmSchemaProvider
{
    private readonly DataverseEnvironment environment = environment;
    public DataverseEnvironment Environment {get => environment; }

    public bool IsReady => throw new NotImplementedException();

    public Exception LastException => throw new NotImplementedException();

    public string LastError => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
    public Task<IEnumerable<EntityMetadata>> GetEntitiesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<EntityMetadata> GetEntityAsync(string entityLogicalName, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IEnumerable<PluginAssemblyConfig>> GetPluginAssembliesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IEnumerable<PluginTypeConfig>> GetPluginTypesAsync(Guid assemblyid, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task RefreshCacheAsync() => throw new NotImplementedException();
}
