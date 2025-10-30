namespace XrmTools.Core.Repositories;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Helpers;
using XrmTools.Environments;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Messages;

internal interface IMetadataSynchronizer
{
    Task SyncAsync(CancellationToken cancellationToken = default);
}

internal sealed class LocalEntityMetadataRepository : XrmRepository, IEntityMetadataRepository
{
    private readonly ILogger _logger;
    private readonly IEnvironmentProvider _environmentProvider;
    private readonly IMetadataSynchronizer _synchronizer;
    private readonly ReaderWriterLockSlim _cacheLock = new(LockRecursionPolicy.NoRecursion);

    // Cache paths
    private string _envKey = string.Empty; // folder-friendly environment key
    private string _rootFolder = string.Empty; // base path for env metadata
    private string EntitiesFilePath => Path.Combine(_rootFolder, "entities.json");
    private string TimestampFilePath => Path.Combine(_rootFolder, "version.json");
    private const string MetadataFolderName = "metadata";

    // Same queries as the original repository for live calls
    private const string entityMessagesQuery =
        "sdkmessages?$select=name,isprivate,executeprivilegename,isvalidforexecuteasync,autotransact,introducedversion" +
        "&$filter=sdkmessageid_sdkmessagefilter/any(n:n/primaryobjecttypecode eq '{0}' and n/iscustomprocessingstepallowed eq true))" +
        "&$expand=sdkmessageid_sdkmessagefilter($filter=primaryobjecttypecode eq '{0}' and iscustomprocessingstepallowed eq true)";
    private const string sdkMessageEntityNamesQuery = "sdkmessagefilters?$filter=sdkmessageid/name eq '{0}'&$select=primaryobjecttypecode";

    [ImportingConstructor]
    public LocalEntityMetadataRepository(
        IWebApiService service,
        IEnvironmentProvider environmentProvider,
        ILogger logger,
        IMetadataSynchronizer synchronizer
    ) : base(service)
    {
        _logger = logger;
        _environmentProvider = environmentProvider;
        _synchronizer = synchronizer;
    }

    private async Task EnsurePathsAsync()
    {
        if (!string.IsNullOrEmpty(_rootFolder)) return;
        var env = await _environmentProvider.GetActiveEnvironmentAsync().ConfigureAwait(false);
        if (env?.BaseServiceUrl == null)
            throw new InvalidOperationException("No active Dataverse environment.");
        var folderKey = ToFolderKey(env.BaseServiceUrl);
        _envKey = folderKey;
        _rootFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory, MetadataFolderName, folderKey);
        Directory.CreateDirectory(_rootFolder);
    }

    private static string ToFolderKey(Uri baseServiceUrl)
    {
        // Prefer host + optional path fragments sanitized
        var host = baseServiceUrl.Host; // e.g. contoso.crm.dynamics.com
        var path = baseServiceUrl.AbsolutePath.Trim('/').Replace('/', '_'); // api_data_v9.2
        var key = string.IsNullOrEmpty(path) ? host : host + "_" + path;
        var invalid = Path.GetInvalidFileNameChars();
        foreach (var ch in invalid)
            key = key.Replace(ch, '_');
        return key.ToLowerInvariant();
    }

    public async Task<IEnumerable<EntityMetadata>> GetEntitiesAsync(CancellationToken cancellationToken)
    {
        await EnsurePathsAsync().ConfigureAwait(false);
        await _synchronizer.SyncAsync(cancellationToken);
        _cacheLock.EnterReadLock();
        try
        {
            if (!File.Exists(EntitiesFilePath)) return Enumerable.Empty<EntityMetadata>();
            using var fs = File.OpenRead(EntitiesFilePath);
            var list = fs.Deserialize<List<EntityMetadata>>(isFile: true) ?? new List<EntityMetadata>();
            return list;
        }
        finally
        {
            _cacheLock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<EntityMetadata>> GetEntitiesByMessageNameAsync(string messageName, CancellationToken cancellationToken)
    {
        // Not supported by cache; fallback to all entities (caller likely filters later)
        return await GetEntitiesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<EntityMetadata> GetEntityAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var all = await GetEntitiesAsync(cancellationToken).ConfigureAwait(false);
        return all.FirstOrDefault(e => string.Equals(e.LogicalName, entityLogicalName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<SdkMessage>> GetAvailableMessagesAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, string.Format(entityMessagesQuery, entityLogicalName));
        var response = await service.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
        if (typed is not null && typed.Value is not null)
        {
            return typed.Value;
        }
        return Array.Empty<SdkMessage>();
    }

    public async Task<EntityMetadata> GetEntityWithRelationshipsAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        // If relationships are included in cached entities, this will return them.
        return await GetEntityAsync(entityLogicalName, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<string>> GetEntityNamesByMessageAsync(string messageName, CancellationToken cancellationToken = default)
    {
        var response = await service.GetAsync(string.Format(sdkMessageEntityNamesQuery, messageName), cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var typed = content.Deserialize<ODataQueryResponse<SdkMessageFilter>>().Value;
            if (typed is not null && typed.Any())
            {
                return typed.Select(e => e.PrimaryObjectTypeCode).Distinct();
            }
        }
        return Array.Empty<string>();
    }

    // Expose a way to trigger sync from outside if needed
    public Task SyncAsync(CancellationToken cancellationToken = default) => _synchronizer.SyncAsync(cancellationToken);
}

internal sealed class RetrieveMetadataChangesSynchronizer : IMetadataSynchronizer
{
    private readonly IWebApiService _service;
    private readonly IEnvironmentProvider _environmentProvider;
    private readonly ILogger _logger;
    private readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.NoRecursion);
    private string _rootFolder = string.Empty;
    private string _entitiesFile = string.Empty;
    private string _stampFile = string.Empty;
    private const string BaseUrl = "RetrieveMetadataChanges?$select=Timestamp&$expand=EntityMetadata($select=LogicalName,EntitySetName,PrimaryIdAttribute,PrimaryNameAttribute,ObjectTypeCode,SchemaName,LogicalCollectionName,ExternalCollectionName,Attributes,ManyToOneRelationships,OneToManyRelationships,ManyToManyRelationships)";

    public RetrieveMetadataChangesSynchronizer(IWebApiService service, IEnvironmentProvider environmentProvider, ILogger logger)
    {
        _service = service;
        _environmentProvider = environmentProvider;
        _logger = logger;
    }

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        await EnsurePathsAsync().ConfigureAwait(false);

        // Read last stamp
        string clientVersion = null;
        _rwLock.EnterReadLock();
        try
        {
            if (File.Exists(_stampFile)) clientVersion = File.ReadAllText(_stampFile);
        }
        finally { _rwLock.ExitReadLock(); }

        // Build URL with version stamp if present
        var url = BaseUrl;
        if (!string.IsNullOrWhiteSpace(clientVersion))
        {
            url += "&ClientVersionStamp='" + Uri.EscapeDataString(clientVersion) + "'";
        }

        // Call service via GET to avoid overload confusion
        var response = await _service.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var dto = await response.CastAsync<RetrieveMetadataChangesResponse>().ConfigureAwait(false);
        if (dto == null)
        {
            _logger.LogWarning("RetrieveMetadataChanges returned null");
            return;
        }

        // Persist entities and new stamp
        _rwLock.EnterWriteLock();
        try
        {
            dto.EntityMetadata.SerializeToFile(_entitiesFile, isFile: true);
            File.WriteAllText(_stampFile, dto.Timestamp ?? string.Empty);
        }
        finally { _rwLock.ExitWriteLock(); }
    }

    private async Task EnsurePathsAsync()
    {
        if (!string.IsNullOrEmpty(_rootFolder)) return;
        var env = await _environmentProvider.GetActiveEnvironmentAsync().ConfigureAwait(false);
        if (env?.BaseServiceUrl == null)
            throw new InvalidOperationException("No active Dataverse environment.");
        var folderKey = ToFolderKey(env.BaseServiceUrl);
        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory;
        _rootFolder = Path.Combine(baseDir, "metadata", folderKey);
        Directory.CreateDirectory(_rootFolder);
        _entitiesFile = Path.Combine(_rootFolder, "entities.json");
        _stampFile = Path.Combine(_rootFolder, "version.json");
    }

    private static string ToFolderKey(Uri baseServiceUrl)
    {
        var host = baseServiceUrl.Host; // e.g. contoso.crm.dynamics.com
        var path = baseServiceUrl.AbsolutePath.Trim('/').Replace('/', '_'); // api_data_v9.2
        var key = string.IsNullOrEmpty(path) ? host : host + "_" + path;
        var invalid = Path.GetInvalidFileNameChars();
        foreach (var ch in invalid)
            key = key.Replace(ch, '_');
        return key.ToLowerInvariant();
    }
}
