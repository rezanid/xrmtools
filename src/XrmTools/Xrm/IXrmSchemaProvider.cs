namespace XrmTools.Xrm;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Meta.Model.Configuration;

internal interface IXrmSchemaProvider : IDisposable
{
    DataverseEnvironment Environment { get; }
    bool IsReady { get; }
    Exception LastException { get; }
    string LastError { get; }
    Task<IEnumerable<EntityMetadata>> GetEntitiesAsync(CancellationToken cancellationToken);
    Task<EntityMetadata> GetEntityAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<IEnumerable<PluginAssemblyConfig>> GetPluginAssembliesAsync(CancellationToken cancellationToken);
    Task<IEnumerable<PluginTypeConfig>> GetPluginTypesAsync(Guid assemblyid, CancellationToken cancellationToken);
    Task RefreshCacheAsync();
}
