#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Helpers;
using XrmTools.DataverseExplorer.Models;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Methods;
using XrmTools.WebApi.Types;

/// <summary>
/// Implementation of the explorer data service.
/// Loads built-in artifacts. Loaded state belongs to the tree session, not a shared cache.
/// </summary>
[Export(typeof(IExplorerDataService))]
[method: ImportingConstructor]
internal sealed class ExplorerDataService(
    [Import] IWebApiService webApi,
    [Import] ILogger<ExplorerDataService> logger) : IExplorerDataService
{
    private const string assembliesQuery = "pluginassemblies?$select=name,version,isolationmode,publickeytoken,sourcetype,description,modifiedon";
    private const string plugintypesQuery = "plugintypes?" +
        "$filter=_pluginassemblyid_value eq '{0}'" +
        "&$select=name,typename,friendlyname,description,workflowactivitygroupname,modifiedon&" +
        "$expand=plugintype_sdkmessageprocessingstep(" +
            "$select=name,stage,asyncautodelete,description,filteringattributes,invocationsource,mode,rank,sdkmessageid,statecode,supporteddeployment,modifiedon;" +
            "$expand=sdkmessageprocessingstepid_sdkmessageprocessingstepimage(" +
                "$select=name,imagetype,messagepropertyname,attributes,entityalias,modifiedon))," +
        "CustomAPIId($select=name,displayname,uniquename,isfunction,bindingtype,workflowsdkstepenabled,isprivate,statecode,allowedcustomprocessingsteptype,executeprivilegename,boundentitylogicalname,description,statuscode,modifiedon;" +
            "$expand=CustomAPIRequestParameters($select=displayname,uniquename,name,statecode,statuscode,logicalentityname,description,type,isoptional)," +
            "CustomAPIResponseProperties($select=displayname,uniquename,name,statecode,statuscode,logicalentityname,description,type))";
    private const string tablesQuery = "EntityDefinitions?$select=MetadataId,LogicalName,SchemaName,EntitySetName,DisplayName,Description,TableType,PrimaryIdAttribute,PrimaryNameAttribute,OwnershipType,ModifiedOn";
    private const string tableDefinitionQuery = "EntityDefinitions(LogicalName='{0}')?$select=MetadataId,LogicalName,SchemaName,EntitySetName,DisplayName,Description,TableType,PrimaryIdAttribute,PrimaryNameAttribute,OwnershipType,ModifiedOn&$expand=Attributes($select=LogicalName,SchemaName,AttributeType,DisplayName,Description,IsPrimaryId,IsPrimaryName,IsCustomAttribute,ModifiedOn),ManyToOneRelationships($select=SchemaName,ReferencedEntity,ReferencedAttribute,ReferencingEntity,ReferencingAttribute),OneToManyRelationships($select=SchemaName,ReferencedEntity,ReferencedAttribute,ReferencingEntity,ReferencingAttribute),ManyToManyRelationships($select=SchemaName,Entity1LogicalName,Entity2LogicalName,IntersectEntityName),Keys($select=LogicalName,SchemaName,EntityLogicalName,KeyAttributes)";
    private const string tableFormQuery = "systemforms({0})?$select=formid,formxmlmanaged,formpresentation,uniquename,ismanaged,isdefault,formjson,formxml,name,publishedon,description,type,canbedeleted,iscustomizable,formactivationstate";
    private const string tableFormsQuery = "systemforms?$select=formid,formxmlmanaged,formpresentation,uniquename,ismanaged,isdefault,name,publishedon,description,type,canbedeleted,iscustomizable,formactivationstate&$filter=objecttypecode eq '{0}'";
    private const string tableViewQuery = "savedqueries({0})?$select=savedqueryid,name,description,querytype,isquickfindquery,isdefault,advancedgroupby,columnsetxml,conditionalformatting,enablecrosspartition,fetchxml,iscustom,isdefault,isuserdefined,layoutjson,offlinesqlquery,roledisplayconditionsxml,statecode,statuscode,modifiedon,createdon";
    private const string tableViewsQuery = "savedqueries?$select=savedqueryid,name,description,querytype,isquickfindquery,isdefault,advancedgroupby,conditionalformatting,enablecrosspartition,iscustom,isdefault,isuserdefined,offlinesqlquery,statecode,statuscode,modifiedon,createdon&$filter=returnedtypecode eq '{0}'";

    private readonly ILogger _logger = logger;
    public async Task<IEnumerable<PackageNode>> LoadPackagesAsync(CancellationToken cancellationToken)
    {
        var response = await webApi.RetrieveMultipleAsync<PluginPackage>(
            "pluginpackages?$select=pluginpackageid,name,version,modifiedon&$orderby=name", cancellationToken: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return response.Value.Select(package =>
        {
            var node = new PackageNode
            {
                Id = package.Id.ToString(), PackageId = package.Id ?? Guid.Empty,
                DisplayName = package.Name ?? "Unknown Package", Version = package.Version,
                ModifiedOn = package.ModifiedOn, ImageMoniker = KnownMonikers.NuGet,
            };
            node.LoadChildrenAsync = async token =>
            {
                var assemblies = await LoadAssembliesAsync(token, node.PackageId);
                token.ThrowIfCancellationRequested();
                node.Children.Clear();
                foreach (var assembly in assemblies)
                {
                    assembly.Parent = node;
                    node.Children.Add(assembly);
                }
            };
            return node;
        }).ToList();
    }

    public async Task<IEnumerable<AssemblyNode>> LoadAssembliesAsync(CancellationToken cancellationToken, Guid? packageId = null)
    {
        ODataQueryResponse<PluginAssembly>? queryResponse;
        try
        {
            queryResponse = await webApi.RetrieveMultipleAsync<PluginAssembly>(
                assembliesQuery + "&$filter=_packageid_value eq " + (packageId?.ToString() ?? "null") + "&$orderby=name", cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assemblies from Dataverse.");
            throw;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var assemblies = new List<AssemblyNode>();
        foreach (var assembly in queryResponse.Value)
        {
            var assemblyId = assembly.Id ?? Guid.Empty;
            var node = new AssemblyNode
            {
                ImageMoniker = KnownMonikers.Assembly,
                Id = assemblyId.ToString(),
                AssemblyId = assemblyId,
                DisplayName = assembly.Name ?? "Unknown Assembly",
                Description = assembly.Description ?? string.Empty,
                PublicKeyToken = assembly.PublicKeyToken,
                Version = assembly.Version,
                IsolationMode = assembly.IsolationMode?.ToString(),
                SourceType = assembly.SourceType?.ToString(),
                ModifiedOn = assembly.ModifiedOn,
                AreChildrenLoaded = false
            };
            node.LoadChildrenAsync = token => LoadAssemblyChildrenAsync(node, token);
            assemblies.Add(node);
        }

        return assemblies;
    }

    public async Task<IEnumerable<ExplorerNodeBase>> LoadAssemblyChildrenAsync(AssemblyNode assembly, CancellationToken cancellationToken)
    {
        ODataQueryResponse<PluginType>? queryResponse;
        try
        {
            queryResponse= await webApi.RetrieveMultipleAsync<PluginType>(
                plugintypesQuery.FormatWith(assembly.AssemblyId), cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plugin types for assembly {0}", assembly.DisplayName);
            throw;
        }
        cancellationToken.ThrowIfCancellationRequested();
        assembly.Children.Clear();
        foreach (var plugin in queryResponse.Value)
        {
            if (plugin.CustomApi is { Count: > 0 })
            {
                foreach (var api in plugin.CustomApi)
                {
                    assembly.Children.Add(ConvertToCustomApi(api, plugin, assembly));
                }
            }
            else
            {
                assembly.Children.Add(ConvertToPluginNode(plugin, assembly));
            }
        }
        assembly.AreChildrenLoaded = true;
        return assembly.Children;
    }

    public async Task<IEnumerable<TableNode>> LoadTablesAsync(CancellationToken cancellationToken)
    {
        ODataQueryResponse<EntityMetadata>? queryResponse;
        try
        {
            queryResponse = await webApi.RetrieveMultipleAsync<EntityMetadata>(
                tablesQuery, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tables from Dataverse.");
            throw;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var tables = new List<TableNode>();
        foreach (var table in queryResponse.Value.OrderBy(e => GetEntityDisplayName(e), StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(table.LogicalName))
            {
                continue;
            }

            var tableNode = new TableNode
            {
                ImageMoniker = KnownMonikers.Table,
                Id = table.MetadataId?.ToString() ?? table.LogicalName,
                LogicalName = table.LogicalName,
                DisplayName = GetEntityDisplayName(table),
                Description = GetLabelText(table.Description),
                SchemaName = table.SchemaName,
                EntitySetName = table.EntitySetName,
                TableType = table.TableType,
                PrimaryIdAttribute = table.PrimaryIdAttribute,
                PrimaryNameAttribute = table.PrimaryNameAttribute,
                OwnershipType = table.OwnershipType?.ToString(),
                ModifiedOn = table.ModifiedOn,
                AreChildrenLoaded = false,
            };

            tableNode.LoadChildrenAsync = token => LoadTableChildrenAsync(tableNode, token);
            tables.Add(tableNode);
        }

        return tables;
    }

    public async Task<IEnumerable<ExplorerNodeBase>> LoadTableChildrenAsync(TableNode table, CancellationToken cancellationToken)
    {
        EntityMetadata metadata;
        try
        {
            using var response = await webApi.GetAsync(
                tableDefinitionQuery.FormatWith(EscapeODataString(table.LogicalName)),
                cancellationToken);

            var root = await response.Content.ReadRootAsync();
            metadata = root.ToObject<EntityMetadata>() ?? throw new InvalidOperationException(
                $"Unable to deserialize entity metadata for table '{table.LogicalName}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading metadata for table {0}", table.LogicalName);
            throw;
        }

        // Fetch everything before publishing children, so failures do not leave a partial tree.
        var forms = await LoadTableFormsAsync(table.LogicalName, cancellationToken);
        var views = await LoadTableViewsAsync(table.LogicalName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        table.Children.Clear();

        var columnsGroup = CreateTableGroupNode(table, "Columns", KnownMonikers.Column);
        foreach (var attribute in metadata.Attributes?.OrderBy(a => GetAttributeDisplayName(a), StringComparer.OrdinalIgnoreCase) ?? Enumerable.Empty<AttributeMetadata>())
        {
            columnsGroup.Children.Add(new TableColumnNode
            {
                ImageMoniker = KnownMonikers.Column,
                Id = $"{table.LogicalName}:column:{attribute.LogicalName ?? attribute.SchemaName ?? Guid.NewGuid().ToString()}",
                LogicalName = attribute.LogicalName ?? string.Empty,
                DisplayName = GetAttributeDisplayName(attribute),
                Description = GetLabelText(attribute.Description),
                SchemaName = attribute.SchemaName,
                ColumnType = attribute.AttributeTypeName?.ToString() ?? attribute.AttributeType.ToString(),
                IsPrimaryId = attribute.IsPrimaryId,
                IsPrimaryName = attribute.IsPrimaryName,
                IsCustom = attribute.IsCustomAttribute,
                ModifiedOn = attribute.ModifiedOn,
                Parent = columnsGroup,
            });
        }
        table.Children.Add(columnsGroup);

        var relationshipsGroup = CreateTableGroupNode(table, "Relations", KnownMonikers.Relationship);
        foreach (var relation in metadata.ManyToOneRelationships?.OrderBy(r => r.SchemaName ?? string.Empty, StringComparer.OrdinalIgnoreCase) ?? Enumerable.Empty<OneToManyRelationshipMetadata>())
        {
            relationshipsGroup.Children.Add(CreateRelationshipNode("Many-To-One", relation.SchemaName, relation.ReferencedEntity, relation.ReferencedAttribute, relation.ReferencingEntity, relation.ReferencingAttribute, null, relationshipsGroup));
        }
        foreach (var relation in metadata.OneToManyRelationships?.OrderBy(r => r.SchemaName ?? string.Empty, StringComparer.OrdinalIgnoreCase) ?? Enumerable.Empty<OneToManyRelationshipMetadata>())
        {
            relationshipsGroup.Children.Add(CreateRelationshipNode("One-To-Many", relation.SchemaName, relation.ReferencedEntity, relation.ReferencedAttribute, relation.ReferencingEntity, relation.ReferencingAttribute, null, relationshipsGroup));
        }
        foreach (var relation in metadata.ManyToManyRelationships?.OrderBy(r => r.SchemaName ?? string.Empty, StringComparer.OrdinalIgnoreCase) ?? Enumerable.Empty<ManyToManyRelationshipMetadata>())
        {
            relationshipsGroup.Children.Add(CreateRelationshipNode("Many-To-Many", relation.SchemaName, relation.Entity1LogicalName, null, relation.Entity2LogicalName, null, relation.IntersectEntityName, relationshipsGroup));
        }
        table.Children.Add(relationshipsGroup);

        var keysGroup = CreateTableGroupNode(table, "Keys", KnownMonikers.Key);
        foreach (var key in metadata.Keys?.OrderBy(k => k.LogicalName ?? string.Empty, StringComparer.OrdinalIgnoreCase) ?? Enumerable.Empty<EntityKeyMetadata>())
        {
            keysGroup.Children.Add(new TableKeyNode
            {
                ImageMoniker = KnownMonikers.Key,
                Id = $"{table.LogicalName}:key:{key.LogicalName ?? key.SchemaName ?? Guid.NewGuid().ToString()}",
                LogicalName = key.LogicalName ?? string.Empty,
                DisplayName = key.DisplayName?.UserLocalizedLabel?.Label ?? key.LogicalName ?? "Key",
                Description = string.Empty,
                SchemaName = key.SchemaName,
                KeyAttributes = key.KeyAttributes is { Length: > 0 } ? string.Join(", ", key.KeyAttributes) : string.Empty,
                ModifiedOn = null,
                Parent = keysGroup,
            });
        }
        table.Children.Add(keysGroup);

        var formsGroup = CreateTableGroupNode(table, "Forms", KnownMonikers.Dialog);
        foreach (var form in forms)
        {
            formsGroup.Children.Add(new TableFormNode
            {
                ImageMoniker = KnownMonikers.Dialog,
                Id = form.Id.ToString(),
                FormId = form.Id ??Guid.Empty,
                DisplayName = string.IsNullOrWhiteSpace(form.Name) ? "Form" : form.Name,
                Description = form.Description ?? string.Empty,
                FormType = form.Type.ToString(),
                FormActivationState = form.FormActivationState.ToString(),
                FormPresentation = form.FormPresentation.ToString(),
                IsDefault = form.IsDefault.ToString(),
                PublishedOn = form.PublishedOn.ToString(),
                UniqueName = form.UniqueName,
                Parent = formsGroup,
            });
        }
        table.Children.Add(formsGroup);

        var viewsGroup = CreateTableGroupNode(table, "Views", KnownMonikers.QueryView);
        foreach (var view in views)
        {
            viewsGroup.Children.Add(new TableViewNode
            {
                ImageMoniker = KnownMonikers.QueryView,
                Id = view.Id.ToString(),
                ViewId = view.Id ?? Guid.Empty,
                DisplayName = string.IsNullOrWhiteSpace(view.Name) ? "View" : view.Name,
                Description = view.Description ?? string.Empty,
                QueryType = view.QueryType.ToString(),
                IsQuickFindQuery = view.IsQuickFindQuery,
                IsDefault = view.IsDefault,
                ModifiedOn = view.ModifiedOn,
                Parent = viewsGroup,
            });
        }
        table.Children.Add(viewsGroup);

        table.AreChildrenLoaded = true;
        return table.Children;
    }

    private static PluginTypeNode ConvertToPluginNode(PluginType plugin, AssemblyNode assembly)
    {
        var pluginNode = new PluginTypeNode
        {
            ImageMoniker = KnownMonikers.Part,
            Id = (plugin.Id ?? Guid.Empty).ToString(),
            PluginTypeId = plugin.Id ?? Guid.Empty,
            DisplayName = plugin.FriendlyName ?? plugin.TypeName ?? "Unknown Type",
            Description = plugin.Description ?? string.Empty,
            TypeName = plugin.TypeName,
            FriendlyName = plugin.FriendlyName,
            WorkflowActivityGroupName = plugin.WorkflowActivityGroupName,
            ModifiedOn = plugin.ModifiedOn,
            Parent = assembly,
            AreChildrenLoaded = false
        };
        pluginNode.Children.Clear();
        foreach (var step in plugin.Steps)
        {
            pluginNode.Children.Add(ConvertToPluginStepNode(step, pluginNode));
        }
        pluginNode.AreChildrenLoaded = true;
        return pluginNode;
    }

    private static CustomApiNode ConvertToCustomApi(CustomApi api, PluginType plugin, AssemblyNode assembly)
    {
        var apiNode = new CustomApiNode
        {
            ImageMoniker = KnownMonikers.WebAPI,
            Id = (api.Id ?? Guid.Empty).ToString(),
            CustomApiId = api.Id ?? Guid.Empty,
            DisplayName = api.DisplayName ?? api.Name ?? "Unknown API",
            Description = api.Description ?? string.Empty,
            Name = api.Name,
            ModifiedOn = api.ModifiedOn,
            Parent = assembly,
            TypeName = plugin.TypeName,
            AreChildrenLoaded = false
        };
        apiNode.Children.Clear();
        foreach (var input in api.RequestParameters)
        {
            apiNode.Children.Add(new CustomApiParameterNode
            {
                ImageMoniker = KnownMonikers.Parameter,
                Id = (input.Id ?? Guid.Empty).ToString(),
                ParameterId = input.Id ?? Guid.Empty,
                DisplayName = input.Name ?? "Unknown Input Parameter",
                Description = string.Empty,
                Name = input.Name,
                ParameterType = input.Type.ToString(),
                IsOptional = input.IsOptional,
                ModifiedOn = null,
                Parent = apiNode
            });
        }
        foreach (var output in api.ResponseProperties)
        {
            apiNode.Children.Add(new CustomApiResponseNode
            {
                ImageMoniker = KnownMonikers.Property,
                Id = (output.Id ?? Guid.Empty).ToString(),
                ResponseId = output.Id ?? Guid.Empty,
                DisplayName = output.Name ?? "Unknown Response Property",
                Description = string.Empty,
                Name = output.Name,
                PropertyType = output.Type.ToString(),
                ModifiedOn = null,
                Parent = apiNode
            });
        }
        apiNode.AreChildrenLoaded = true;
        return apiNode;
    }

    private static PluginStepNode ConvertToPluginStepNode(SdkMessageProcessingStep step, PluginTypeNode plugin)
    {
        var stepNode = new PluginStepNode
        {
            ImageMoniker = KnownMonikers.Step,
            Id = (step.Id ?? Guid.Empty).ToString(),
            StepId = step.Id ?? Guid.Empty,
            DisplayName = step.Name ?? "Unknown Step",
            Description = step.Description ?? string.Empty,
            FilteringAttributes = step.FilteringAttributes,
            AsyncAutoDelete = step.AsyncAutoDelete,
            InvocationSource = step.InvocationSource.ToString(),
            Mode = step.Mode.ToString(),
            Rank = step.Rank,
            Parent = plugin,
            SdkMessageId = step.SdkMessageFilter?.SdkMessageId?.ToString(),
            Stage = step.Stage.ToString(),
            StateCode = step.StateCode.ToString(),
            SupportedDeployment = step.SupportedDeployment.ToString(),
            ModifiedOn = step.ModifiedOn,
            AreChildrenLoaded = true
        };
        stepNode.Children.Clear();
        foreach (var image in step.Images)
        {
            stepNode.Children.Add(ConvertToStepImage(image, stepNode));
        }
        stepNode.AreChildrenLoaded = true;
        return stepNode;
    }

    private static PluginImageNode ConvertToStepImage(SdkMessageProcessingStepImage image, PluginStepNode step) => new()
    {
        ImageMoniker = KnownMonikers.Image,
        Id = (image.Id ?? Guid.Empty).ToString(),
        ImageId = image.Id ?? Guid.Empty,
        ImageType = image.ImageType.ToString(),
        Description = image.Description ?? string.Empty,
        DisplayName = image.Name ?? "Unknown Image",
        EntityAlias = image.EntityAlias ?? string.Empty,
        Attributes = image.Attributes ?? string.Empty,
        MessagePropertyName = image.MessagePropertyName ?? string.Empty,
        ModifiedOn = image.ModifiedOn,
        Parent = step,
    };

    private static string GetEntityDisplayName(EntityMetadata entity)
    {
        return GetLabelText(entity.DisplayName) is string displayName && !string.IsNullOrWhiteSpace(displayName)
            ? displayName
            : entity.LogicalName;
    }

    private static string GetAttributeDisplayName(AttributeMetadata attribute)
    {
        return GetLabelText(attribute.DisplayName) is string displayName && !string.IsNullOrWhiteSpace(displayName)
            ? displayName
            : attribute.LogicalName ?? "Column";
    }

    private static string GetLabelText(Label? label)
    {
        return label?.UserLocalizedLabel?.Label
            ?? label?.LocalizedLabels?.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l.Label))?.Label
            ?? string.Empty;
    }

    private static string EscapeODataString(string value) => value.Replace("'", "''");

    private static TableGroupNode CreateTableGroupNode(TableNode table, string groupName, ImageMoniker imageMoniker)
    {
        return new TableGroupNode
        {
            Id = $"{table.LogicalName}:{groupName}",
            DisplayName = groupName,
            Description = string.Empty,
            GroupName = groupName,
            ImageMoniker = imageMoniker,
            Parent = table,
        };
    }

    private static TableRelationshipNode CreateRelationshipNode(
        string relationType,
        string? schemaName,
        string? referencedEntity,
        string? referencedAttribute,
        string? referencingEntity,
        string? referencingAttribute,
        string? intersectEntity,
        ExplorerNodeBase parent)
    {
        var safeSchemaName = string.IsNullOrWhiteSpace(schemaName) ? "Relationship" : schemaName;
        return new TableRelationshipNode
        {
            ImageMoniker = KnownMonikers.Relationship,
            Id = $"{relationType}:{safeSchemaName}",
            DisplayName = safeSchemaName,
            Description = relationType,
            SchemaName = safeSchemaName,
            RelationType = relationType,
            ReferencedEntity = referencedEntity,
            ReferencedAttribute = referencedAttribute,
            ReferencingEntity = referencingEntity,
            ReferencingAttribute = referencingAttribute,
            IntersectEntityName = intersectEntity,
            Parent = parent,
        };
    }

    private async Task<IEnumerable<SystemForm>> LoadTableFormsAsync(string logicalName, CancellationToken cancellationToken)
    {
        ODataQueryResponse<SystemForm>? queryResponse;
        try
        {
            queryResponse = await webApi.RetrieveMultipleAsync<SystemForm>(
                tableFormsQuery.FormatWith(EscapeODataString(logicalName)), cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading forms for table {0}", logicalName);
            throw;
        }

        return queryResponse.Value;
    }

    private async Task<IEnumerable<SavedQuery>> LoadTableViewsAsync(string logicalName, CancellationToken cancellationToken)
    {
        ODataQueryResponse<SavedQuery>? queryResponse;
        try
        {
            queryResponse = await webApi.RetrieveMultipleAsync<SavedQuery>(
                tableViewsQuery.FormatWith(EscapeODataString(logicalName)), cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading views for table {0}", logicalName);
            throw;
        }

        return queryResponse.Value;
    }
}

#nullable restore
