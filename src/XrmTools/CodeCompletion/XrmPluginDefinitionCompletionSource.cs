﻿namespace XrmTools.CodeCompletion;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Logging;
using XrmTools.Core.Repositories;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.LanguageServices;
using XrmTools.Meta.Attributes;
using XrmTools.Xrm.Repositories;
using System.Collections.Generic;
using System;
using XrmTools.Core.Helpers;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Text.Adornments;
using System.Collections.Immutable;
using Microsoft.Xrm.Sdk.Metadata;
using XrmTools.Xrm;

internal class XrmPluginDefinitionCompletionSource(
    IOutputLoggerService logger, IRepositoryFactory repositoryFactory, VisualStudioWorkspace workspace) : IAsyncCompletionSource
{
    const int StepCtorArgumentMessageIndex = 0;
    const int StepCtorArgumentEntityIndex = 1;
    const int StepCtorArgumentFilteringAttributesIndex = 2;
    const int ImageCtorArgumentFilteringAttributesIndex = 1;
    const int EntityCtorArgumentEntityIndex = 0;
    const int EntityCtorArgumentAttributesIndex = 1;

    static readonly ImageElement StandardTableIcon = new(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 3032), "Standard");
    static readonly ImageElement ActivityTableIcon = new (new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1157), "Activity");
    static readonly ImageElement ElasticTableIcon = new(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1060), "Elastic");
    static readonly ImageElement VirtualTableIcon = new(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 887), "Virtual");

    static readonly ImmutableArray<CompletionFilter> StandardTableFilters = [new("Standard", "S", StandardTableIcon)];
    static readonly ImmutableArray<CompletionFilter> ActivityTableFilters = [new("Activity", "A", ActivityTableIcon)];
    static readonly ImmutableArray<CompletionFilter> ElasticTableFilters = [new("Elastic", "E", ElasticTableIcon)];
    static readonly ImmutableArray<CompletionFilter> VirtualTableFilters = [new("Virtual", "V", VirtualTableIcon)];

    static readonly ImageElement BooleanColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 296), "Boolean");
    static readonly ImageElement DateTimeColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 371), "DateTime");
    static readonly ImageElement NumberColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1017), "Number");
    static readonly ImageElement LookupColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1724), "Lookup");
    static readonly ImageElement MoneyColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 803), "Money");
    static readonly ImageElement StringColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2985), "String");
    static readonly ImageElement PickListColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 982), "PickList");
    static readonly ImageElement MiscColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1217), "Other");
    static readonly ImageElement KeyColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1654), "Key");
    static readonly ImageElement StateColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2919), "State");

    static readonly ImmutableArray<CompletionFilter> BooleanColumnFilters = [new("Boolean", "B", BooleanColumnIcon)];
    static readonly ImmutableArray<CompletionFilter> DateTimeColumnFilters = [new("DateTime", "D", DateTimeColumnIcon)];
    static readonly ImmutableArray<CompletionFilter> NumberColumnFilters = [new("Number", "N", NumberColumnIcon)];
    static readonly ImmutableArray<CompletionFilter> LookupColumnFilters = [new("Lookup", "L", LookupColumnIcon)];
    static readonly ImmutableArray<CompletionFilter> MoneyColumnFilters = [new("Currency", "C", MoneyColumnIcon)];
    static readonly ImmutableArray<CompletionFilter> StringColumnFilters = [new("String", "S", StringColumnIcon)];
    static readonly ImmutableArray<CompletionFilter> PickListColumnFilters = [new("PickList", "P", PickListColumnIcon)];
    static readonly ImmutableArray<CompletionFilter> MiscColumnFilters = [new("Misc", "M", MiscColumnIcon)];//, OtherColumnIcon)]

    public async Task<CompletionContext> GetCompletionContextAsync(
        IAsyncCompletionSession session,
        CompletionTrigger trigger,
        SnapshotPoint triggerLocation,
        SnapshotSpan applicableToSpan,
        CancellationToken cancellationToken)
    {
        // Retrieve the document text and syntax tree
        var document = triggerLocation.Snapshot.TextBuffer.GetRelatedDocuments().FirstOrDefault();
        if (document == null) return CompletionContext.Empty;

        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (syntaxTree == null || semanticModel == null) return CompletionContext.Empty;

        // Get the attribute at the current cursor position
        var root = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
        var node = root.FindToken(triggerLocation.Position).Parent;
        if (node == null) return CompletionContext.Empty;

        var attributeSyntax = node.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
        if (attributeSyntax == null) return CompletionContext.Empty;

        // Determine which argument is being edited
        var argumentList = attributeSyntax.ArgumentList?.Arguments;
        if (argumentList == null || argumentList.Value.Count == 0) return CompletionContext.Empty;

        var argumentIndex = GetArgumentIndexAtPosition(argumentList.Value, triggerLocation.Position);
        if (argumentIndex == -1) return CompletionContext.Empty;

        if (IsStepAttribute(attributeSyntax, semanticModel))
        {
            return argumentIndex switch
            {
                StepCtorArgumentMessageIndex => await GetMessageCompletionsWhenProcessingStepAllowedAsync(cancellationToken),
                StepCtorArgumentEntityIndex =>
                    node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression) ?
                    await GetEntityCompletionsAsync(argumentList.Value[StepCtorArgumentMessageIndex], semanticModel, cancellationToken) :
                    CompletionContext.Empty,
                StepCtorArgumentFilteringAttributesIndex =>
                    node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression) ?
                    await GetAttributeCompletionsAsync(argumentList.Value[StepCtorArgumentEntityIndex], semanticModel, triggerLocation, cancellationToken) :
                    CompletionContext.Empty,
                _ => CompletionContext.Empty,
            };
        }

        if (IsImageAttribute(attributeSyntax, semanticModel))
        {
            return argumentIndex switch
            {
                ImageCtorArgumentFilteringAttributesIndex =>
                    node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression) ?
                    await GetAttributeCompletionsAsync(FindEntityNameFromClosestStep(attributeSyntax, semanticModel), semanticModel, triggerLocation, cancellationToken) :
                    CompletionContext.Empty,
                _ => CompletionContext.Empty,
            };
        }

        if (IsEntityAttribute(attributeSyntax, semanticModel))
        {
            return argumentIndex switch
            {
                EntityCtorArgumentEntityIndex => await GetEntityCompletionsAsync(cancellationToken).ConfigureAwait(false),
                EntityCtorArgumentAttributesIndex => await GetAttributeCompletionsAsync(argumentList.Value[EntityCtorArgumentEntityIndex], semanticModel, triggerLocation, cancellationToken).ConfigureAwait(false),
                _ => CompletionContext.Empty,
            };
        }

        if (IsCustomApiOrRequestOrResponseProxyAttribute(attributeSyntax, semanticModel) && argumentIndex == 0)
        {
            return await GetMessageCompletionsWhenVisibleAsync(cancellationToken).ConfigureAwait(false);
        }

        return CompletionContext.Empty;
    }


    /// <summary>
    /// List of all SdkMessages from Power Platform environment.
    /// </summary>
    private async Task<CompletionContext> GetMessageCompletionsWhenProcessingStepAllowedAsync(CancellationToken cancellationToken)
    {
        var entityMetadataRepository = await repositoryFactory.CreateRepositoryAsync<ISdkMessageRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var messages = await entityMetadataRepository.GetCustomProcessingStepAllowedAsync(cancellationToken).ConfigureAwait(false);
        return new CompletionContext([.. messages.Select(message => new CompletionItem(message.Name, this))]);
    }

    private async Task<CompletionContext> GetMessageCompletionsWhenVisibleAsync(CancellationToken cancellationToken)
    {
        var entityMetadataRepository = await repositoryFactory.CreateRepositoryAsync<ISdkMessageRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var messages = await entityMetadataRepository.GetVisibleWithoutDescendantsAsync(cancellationToken).ConfigureAwait(false);
        return new CompletionContext([.. messages.Select(message => new CompletionItem(message.Name, this))]);
    }

    /// <summary>
    /// List of all SdkMessages supported by the given entity.
    /// </summary>
    /// <returns></returns>
    private async Task<CompletionContext> GetMessageCompletionsAsync(
        AttributeArgumentSyntax entityArgument,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var entityName = semanticModel.GetConstantValue(entityArgument.Expression).Value as string;
        if (string.IsNullOrEmpty(entityName))
            return CompletionContext.Empty;
        var entityMetadataRepository = await repositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var messages = await entityMetadataRepository.GetAvailableMessagesAsync(entityName, cancellationToken).ConfigureAwait(false);
        return new CompletionContext([.. messages.Select(message => new CompletionItem(message.Name, this))]);
    }

    /// <summary>
    /// List of all entities.
    /// </summary>
    private async Task<CompletionContext> GetEntityCompletionsAsync(CancellationToken cancellationToken)
    {
        var entityMetadataRepository = await repositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var entities = await entityMetadataRepository.GetAsync(cancellationToken).ConfigureAwait(false);
        return new CompletionContext([.. entities.Select(ToCompletionItem)]);
    }

    /// <summary>
    /// List of all entities supported by the given SdkMessage.
    /// </summary>
    private async Task<CompletionContext> GetEntityCompletionsAsync(
        AttributeArgumentSyntax messageArgument,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var messageName = semanticModel.GetConstantValue(messageArgument.Expression).Value as string;
        var entityMetadataRepository = await repositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var entityNames = await entityMetadataRepository.GetEntityNamesAsync(messageName, cancellationToken).ConfigureAwait(false);
        var entities = await entityMetadataRepository.GetAsync(cancellationToken).ConfigureAwait(false);

        return new CompletionContext([.. entities.Where(e => entityNames.Contains(e.LogicalName)).Select(ToCompletionItem)]);
    }

    public async Task<CompletionContext> GetAttributeCompletionsAsync(
        AttributeArgumentSyntax entityArgument,
        SemanticModel semanticModel,
        SnapshotPoint triggerLocation,
        CancellationToken cancellationToken)
    {
        if (entityArgument == null) return CompletionContext.Empty;
        var entityName = semanticModel.GetConstantValue(entityArgument.Expression).Value as string;
        if (string.IsNullOrEmpty(entityName)) return CompletionContext.Empty;

        var entityMetadataRepository = await repositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var entity = await entityMetadataRepository.GetAsync(entityName, cancellationToken).ConfigureAwait(false);
        var availableAttributes = entity?.Attributes?.Where(IsSupportedAttribute).ToArray();
        if (availableAttributes == null || availableAttributes.Length == 0) return CompletionContext.Empty;

        // Extract current attribute list
        var root = await entityArgument.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
        var node = root.FindToken(triggerLocation).Parent;
        if (node is not LiteralExpressionSyntax literal)
        {
            return CompletionContext.Empty;
        }
        var currentWord = CurrentWord(triggerLocation, literal);
        var currentXrmAttributes = string.IsNullOrWhiteSpace(currentWord)
            ? literal.Token.ValueText.SplitAndTrim(',')
            : literal.Token.ValueText.SplitAndTrim(',', currentWord);

        // Filter and provide completions
        var suggestedAttributes = availableAttributes
            .Where(attr => !currentXrmAttributes.Contains(attr.LogicalName))
            .Select(ToCompletionItem)
            .ToList();

        return new CompletionContext([.. suggestedAttributes]);
    }

    private CompletionItem ToCompletionItem(EntityMetadata entity)
    {
        var (filters, icon) = entity switch
        {
            { IsActivity: true } => (ActivityTableFilters, ActivityTableIcon),
            { TableType: "Elastic" } => (ElasticTableFilters, ElasticTableIcon),
            { TableType: "Virtual" } => (VirtualTableFilters, VirtualTableIcon),
            _ => (StandardTableFilters, StandardTableIcon),
        };
        return new(entity.LogicalName, this, icon, filters);
    }

    private CompletionItem ToCompletionItem(AttributeMetadata attribute)
    {
        var (filter, icon) = attribute.AttributeType switch
        {
            AttributeTypeCode.Boolean => (BooleanColumnFilters, BooleanColumnIcon),
            AttributeTypeCode.DateTime => (DateTimeColumnFilters, DateTimeColumnIcon),
            AttributeTypeCode.Decimal => (NumberColumnFilters, NumberColumnIcon),
            AttributeTypeCode.Double => (NumberColumnFilters, NumberColumnIcon),
            AttributeTypeCode.Integer => (NumberColumnFilters, NumberColumnIcon),
            AttributeTypeCode.Lookup => (LookupColumnFilters, LookupColumnIcon),
            AttributeTypeCode.Memo => (StringColumnFilters, StringColumnIcon),
            AttributeTypeCode.Money => (MoneyColumnFilters, MoneyColumnIcon),
            AttributeTypeCode.Owner => (MiscColumnFilters, MiscColumnIcon),
            AttributeTypeCode.PartyList => (MiscColumnFilters, MiscColumnIcon),
            AttributeTypeCode.Picklist => (PickListColumnFilters, PickListColumnIcon),
            AttributeTypeCode.State => (MiscColumnFilters, StateColumnIcon),
            AttributeTypeCode.Status => (MiscColumnFilters, StateColumnIcon),
            AttributeTypeCode.String => (StringColumnFilters, StringColumnIcon),
            AttributeTypeCode.Uniqueidentifier => (MiscColumnFilters, KeyColumnIcon),
            AttributeTypeCode.CalendarRules => (PickListColumnFilters, PickListColumnIcon),
            AttributeTypeCode.Virtual => (PickListColumnFilters, PickListColumnIcon),
            AttributeTypeCode.BigInt => (NumberColumnFilters, MiscColumnIcon),
            AttributeTypeCode.ManagedProperty => (MiscColumnFilters, MiscColumnIcon),
            AttributeTypeCode.EntityName => (MiscColumnFilters, MiscColumnIcon),
            AttributeTypeCode.Customer => (MiscColumnFilters, MiscColumnIcon),
            _ => (MiscColumnFilters, MiscColumnIcon)
        };
        return new CompletionItem(attribute.LogicalName, this, icon, filter);
    }


    public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken cancellationToken)
    {
        // Optional: Provide a description for each completion item if needed
        return Task.FromResult<object>(null);
    }

    private bool IsStepAttribute(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);
        var symbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
        return symbol?.ContainingType.Name == nameof(StepAttribute);
    }

    private bool IsImageAttribute(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);
        var symbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
        return symbol?.ContainingType.Name == nameof(ImageAttribute);
    }

    private bool IsEntityAttribute(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);
        var symbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
        return symbol?.ContainingType.Name == nameof(EntityAttribute);
    }

    private bool IsCustomApiOrRequestOrResponseProxyAttribute(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);
        var symbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
        return symbol?.ContainingType.Name is "CustomApiAttribute" or "RequestProxyAttribute" or "ResponseProxyAttribute";
    }

    private bool IsSupportedAttribute(AttributeMetadata attribute) => attribute.IsValidForRead.HasValue && attribute.IsValidForRead.Value && attribute.AttributeOf is null;

    private int GetArgumentIndexAtPosition(SeparatedSyntaxList<AttributeArgumentSyntax> arguments, int position)
    {
        for (int i = 0; i < arguments.Count; i++)
        {
            var argument = arguments[i];
            if (argument.Span.Start <= position && argument.Span.End >= position)
            {
                return i;
            }
        }
        return -1;
    }

    private AttributeArgumentSyntax? FindEntityNameFromClosestStep(AttributeSyntax currentAttribute, SemanticModel semanticModel)
    {
        var classDecl = currentAttribute.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDecl == null)
            return null;

        // Flatten all attributes on the class
        var allAttributes = classDecl.AttributeLists.SelectMany(list => list.Attributes).ToList();

        // We only want Step attributes declared before the current Image attribute
        foreach (var attr in allAttributes)
        {
            if (attr.SpanStart >= currentAttribute.SpanStart)
                break;

            if (IsStepAttribute(attr, semanticModel))
            {
                var args = attr.ArgumentList?.Arguments;
                if (args != null && args.Value.Count > StepCtorArgumentEntityIndex)
                    return args.Value[StepCtorArgumentEntityIndex];
            }
        }

        return null;
    }

    public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken cancellationToken)
    {
        // Indicate when completion should be triggered
        var span = triggerLocation.Snapshot.CreateTrackingSpan(triggerLocation.Position, 0, SpanTrackingMode.EdgeInclusive);
        return new CompletionStartData(CompletionParticipation.ProvidesItems, new SnapshotSpan(span.GetStartPoint(triggerLocation.Snapshot), span.GetEndPoint(triggerLocation.Snapshot)));
    }

    public static string CurrentWord(SnapshotPoint triggerLocation, LiteralExpressionSyntax literal)
    {
        var text = literal.Token.Text;
        var index = triggerLocation.Position - literal.SpanStart;
        var start = index - 1;
        var end = index;
        var length = text.Length;
        var terminators = new List<char>(['\"', ' ', ',']);
        while (!terminators.Contains(text[start]) && start != 0) --start;
        while (!terminators.Contains(text[end]) && end != length) ++end;
        if (terminators.Contains(text[start]) && start < length && index != start) ++start;
        return text[start..end];
    }
}