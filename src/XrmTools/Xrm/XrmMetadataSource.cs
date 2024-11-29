using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Xrm.Model;

namespace XrmTools.Xrm;


internal class XrmMetadataSource : IXrmSchemaProvider
{
    private bool disposed;

    public DataverseEnvironment Environment => throw new NotImplementedException();

    public bool IsReady => throw new NotImplementedException();

    public Exception LastException => throw new NotImplementedException();

    public string LastError => throw new NotImplementedException();

    public Task<IEnumerable<EntityMetadata>> GetEntitiesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<EntityMetadata> GetEntityAsync(string entityLogicalName, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IEnumerable<PluginAssemblyConfig>> GetPluginAssembliesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IEnumerable<PluginTypeConfig>> GetPluginTypesAsync(Guid assemblyid, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task RefreshCacheAsync() => throw new NotImplementedException();

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~XrmMetadataSource()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}