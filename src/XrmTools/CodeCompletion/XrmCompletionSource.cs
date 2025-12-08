namespace XrmTools.CodeCompletion;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Repositories;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using XrmTools.Xrm;
using XrmTools.Xrm.Repositories;

internal abstract class XrmCompletionSource(
    ILogger logger, IRepositoryFactory repositoryFactory, VisualStudioWorkspace workspace) : IAsyncCompletionSource
{
    protected readonly ILogger logger = logger;
    protected readonly IRepositoryFactory repositoryFactory = repositoryFactory;
    protected readonly VisualStudioWorkspace workspace = workspace;

    /// <summary>
    /// List of all entities.
    /// </summary>
    protected async Task<CompletionContext> GetEntityCompletionsAsync(CancellationToken cancellationToken)
    {
        using var entityMetadataRepository = repositoryFactory.CreateRepository<IEntityMetadataRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        try
        {
            var entities = await entityMetadataRepository.GetAsync(cancellationToken).ConfigureAwait(false);
            return new CompletionContext([.. entities.Select(ToCompletionItem)]);
        }
        catch (Exception ex)
        {
            logger.LogError("An error occurred while retrieving entity metadata. " + ex.ToString());
            return CompletionContext.Empty;
        }
    }

    /// <summary>
    /// List of all entities supported by the given SdkMessage.
    /// </summary>
    protected async Task<CompletionContext> GetEntityCompletionsAsync(
        string messageName,
        CancellationToken cancellationToken)
    {
        using var entityMetadataRepository = repositoryFactory.CreateRepository<IEntityMetadataRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var entityNames = await entityMetadataRepository.GetEntityNamesAsync(messageName, cancellationToken).ConfigureAwait(false);
        var entities = await entityMetadataRepository.GetAsync(cancellationToken).ConfigureAwait(false);

        return new CompletionContext([.. entities.Where(e => entityNames.Contains(e.LogicalName)).Select(ToCompletionItem)]);
    }

    /// <summary>
    /// List of all attributes supported by the given entity, excluding the ones already specified.
    /// </summary>
    protected async Task<CompletionContext> GetAttributeCompletionsAsync(
        string entityName,
        List<string> excludeAttributesNames,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(entityName)) return CompletionContext.Empty;

        using var entityMetadataRepository = repositoryFactory.CreateRepository<IEntityMetadataRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        EntityMetadata entity;
        try
        {
            entity = await entityMetadataRepository.GetAsync(entityName, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return CompletionContext.Empty;
        }
        var availableAttributes = entity?.Attributes?.Where(IsSupportedAttribute).ToArray();
        if (availableAttributes == null || availableAttributes.Length == 0) return CompletionContext.Empty;

        // Filter and provide completions
        var suggestedAttributes = availableAttributes
            .Where(attr => !excludeAttributesNames.Contains(attr.LogicalName))
            .Select(ToCompletionItem)
            .ToList();

        return new CompletionContext([.. suggestedAttributes]);
    }

    /// <summary>
    /// List of all SdkMessages from Power Platform environment.
    /// </summary>
    protected async Task<CompletionContext> GetMessageCompletionsWhenProcessingStepAllowedAsync(CancellationToken cancellationToken)
    {
        using var entityMetadataRepository = repositoryFactory.CreateRepository<ISdkMessageRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var messages = await entityMetadataRepository.GetCustomProcessingStepAllowedAsync(cancellationToken).ConfigureAwait(false);
        return new CompletionContext([.. messages.Select(message => new CompletionItem(message.Name, this))]);
    }

    protected async Task<CompletionContext> GetMessageCompletionsWhenVisibleAsync(CancellationToken cancellationToken)
    {
        using var entityMetadataRepository = repositoryFactory.CreateRepository<ISdkMessageRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var messages = await entityMetadataRepository.GetVisibleWithoutDescendantsAsync(cancellationToken).ConfigureAwait(false);
        return new CompletionContext([.. messages.Select(message => new CompletionItem(message.Name, this))]);
    }

    /// <summary>
    /// List of all unmanaged solutions from Power Platform environment.
    /// </summary>
    protected async Task<CompletionContext> GetSolutionCompletionsAsync(CancellationToken cancellationToken)
    {
        using var solutionRepository = repositoryFactory.CreateRepository<ISolutionRepository>();
        if (solutionRepository == null) return CompletionContext.Empty;
        try
        {
            var solutions = await solutionRepository.GetUnmanagedAsync(cancellationToken).ConfigureAwait(false);
            return new CompletionContext([.. solutions.Select(ToCompletionItem)]);
        }
        catch (Exception ex)
        {
            logger.LogError("An error occurred while retrieving solution metadata. " + ex.ToString());
            return CompletionContext.Empty;
        }
    }

    private CompletionItem ToCompletionItem(EntityMetadata entity)
    {
        var (filters, icon) = entity switch
        {
            { IsActivity: true } => (XrmEntityCompletionFilters.ActivityTableFilters, XrmEntityCompletionFilters.ActivityTableIcon),
            { TableType: "Elastic" } => (XrmEntityCompletionFilters.ElasticTableFilters, XrmEntityCompletionFilters.ElasticTableIcon),
            { TableType: "Virtual" } => (XrmEntityCompletionFilters.VirtualTableFilters, XrmEntityCompletionFilters.VirtualTableIcon),
            _ => (XrmEntityCompletionFilters.StandardTableFilters, XrmEntityCompletionFilters.StandardTableIcon),
        };
        return new(entity.LogicalName, this, icon, filters);
    }

    private CompletionItem ToCompletionItem(AttributeMetadata attribute)
    {
        var (filter, icon) = attribute.AttributeType switch
        {
            AttributeTypeCode.Boolean => (XrmAttributeCompletionFilters.BooleanColumnFilters, XrmAttributeCompletionFilters.BooleanColumnIcon),
            AttributeTypeCode.DateTime => (XrmAttributeCompletionFilters.DateTimeColumnFilters, XrmAttributeCompletionFilters.DateTimeColumnIcon),
            AttributeTypeCode.Decimal => (XrmAttributeCompletionFilters.NumberColumnFilters, XrmAttributeCompletionFilters.NumberColumnIcon),
            AttributeTypeCode.Double => (XrmAttributeCompletionFilters.NumberColumnFilters, XrmAttributeCompletionFilters.NumberColumnIcon),
            AttributeTypeCode.Integer => (XrmAttributeCompletionFilters.NumberColumnFilters, XrmAttributeCompletionFilters.NumberColumnIcon),
            AttributeTypeCode.Lookup => (XrmAttributeCompletionFilters.LookupColumnFilters, XrmAttributeCompletionFilters.LookupColumnIcon),
            AttributeTypeCode.Memo => (XrmAttributeCompletionFilters.StringColumnFilters, XrmAttributeCompletionFilters.StringColumnIcon),
            AttributeTypeCode.Money => (XrmAttributeCompletionFilters.MoneyColumnFilters, XrmAttributeCompletionFilters.MoneyColumnIcon),
            AttributeTypeCode.Owner => (XrmAttributeCompletionFilters.MiscColumnFilters, XrmAttributeCompletionFilters.MiscColumnIcon),
            AttributeTypeCode.PartyList => (XrmAttributeCompletionFilters.MiscColumnFilters, XrmAttributeCompletionFilters.MiscColumnIcon),
            AttributeTypeCode.Picklist => (XrmAttributeCompletionFilters.PickListColumnFilters, XrmAttributeCompletionFilters.PickListColumnIcon),
            AttributeTypeCode.State => (XrmAttributeCompletionFilters.MiscColumnFilters, XrmAttributeCompletionFilters.StateColumnIcon),
            AttributeTypeCode.Status => (XrmAttributeCompletionFilters.MiscColumnFilters, XrmAttributeCompletionFilters.StateColumnIcon),
            AttributeTypeCode.String => (XrmAttributeCompletionFilters.StringColumnFilters, XrmAttributeCompletionFilters.StringColumnIcon),
            AttributeTypeCode.Uniqueidentifier => (XrmAttributeCompletionFilters.MiscColumnFilters, XrmAttributeCompletionFilters.KeyColumnIcon),
            AttributeTypeCode.CalendarRules => (XrmAttributeCompletionFilters.PickListColumnFilters, XrmAttributeCompletionFilters.PickListColumnIcon),
            AttributeTypeCode.Virtual => (XrmAttributeCompletionFilters.PickListColumnFilters, XrmAttributeCompletionFilters.PickListColumnIcon),
            AttributeTypeCode.BigInt => (XrmAttributeCompletionFilters.NumberColumnFilters, XrmAttributeCompletionFilters.MiscColumnIcon),
            AttributeTypeCode.ManagedProperty => (XrmAttributeCompletionFilters.MiscColumnFilters, XrmAttributeCompletionFilters.MiscColumnIcon),
            AttributeTypeCode.EntityName => (XrmAttributeCompletionFilters.MiscColumnFilters, XrmAttributeCompletionFilters.MiscColumnIcon),
            AttributeTypeCode.Customer => (XrmAttributeCompletionFilters.MiscColumnFilters, XrmAttributeCompletionFilters.MiscColumnIcon),
            _ => (XrmAttributeCompletionFilters.MiscColumnFilters, XrmAttributeCompletionFilters.MiscColumnIcon)
        };
        return new CompletionItem(attribute.LogicalName, this, icon, filter);
    }

    private CompletionItem ToCompletionItem(Solution solution)
    {
        // Use the UniqueName as the display text for the completion item
        var displayText = solution.UniqueName ?? string.Empty;
        return new CompletionItem(displayText, this);
    }

    private bool IsSupportedAttribute(AttributeMetadata attribute) => attribute.IsValidForRead && attribute.AttributeOf is null;
    public abstract Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token);
    public abstract Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token);
    public abstract CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token);
}
