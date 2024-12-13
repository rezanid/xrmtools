namespace XrmTools.CodeCompletion;

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

internal class XrmPluginDefinitionCompletionSource(
    IOutputLoggerService logger, IRepositoryFactory repositoryFactory, VisualStudioWorkspace workspace) : IAsyncCompletionSource
{
    public async Task<CompletionContext> GetCompletionContextAsync(
        IAsyncCompletionSession session,
        CompletionTrigger trigger,
        SnapshotPoint triggerLocation,
        SnapshotSpan applicableToSpan,
        CancellationToken cancellationToken)
    {
        // Get the VisualStudioWorkspace service
        //var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        //var workspace = componentModel.GetService<VisualStudioWorkspace>();

        // Retrieve the document associated with the trigger location
        //var documentId = workspace.CurrentSolution.GetDocumentId(triggerLocation.Snapshot.TextBuffer.AsTextContainer());
        //if (documentId == null) return CompletionContext.Empty;

        //var document = workspace.CurrentSolution.GetDocument(documentId);
        //if (document == null) return CompletionContext.Empty;

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
        if (attributeSyntax == null || !IsStepAttribute(attributeSyntax, semanticModel)) return CompletionContext.Empty;

        // Determine which argument is being edited
        var argumentList = attributeSyntax.ArgumentList?.Arguments;
        if (argumentList == null || argumentList.Value.Count == 0) return CompletionContext.Empty;

        var argumentIndex = GetArgumentIndexAtPosition(argumentList.Value, triggerLocation.Position);
        if (argumentIndex == -1) return CompletionContext.Empty;

        return argumentIndex switch
        {
            0 => await GetEntityCompletionsAsync(cancellationToken),
            1 => await GetMessageCompletionsAsync(argumentList.Value[0], semanticModel, cancellationToken),
            2 => await GetAttributeCompletionsAsync(argumentList.Value[0], semanticModel, triggerLocation, cancellationToken),
            _ => CompletionContext.Empty,
        };
    }

    private async Task<CompletionContext> GetEntityCompletionsAsync(CancellationToken cancellationToken)
    {
        var entityMetadataRepository = await repositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var entities = await entityMetadataRepository.GetAsync(cancellationToken).ConfigureAwait(false);
        return new CompletionContext([..entities.Select(entity => new CompletionItem(entity.LogicalName, this))]);
    }

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
        var messages = await entityMetadataRepository.GetAvailableMessageAsync(entityName, cancellationToken).ConfigureAwait(false);
        return new CompletionContext([..messages.Select(message => new CompletionItem(message.Name, this))]);
    }

    public async Task<CompletionContext> GetAttributeCompletionsAsync(
        AttributeArgumentSyntax entityArgument,
        SemanticModel semanticModel,
        SnapshotPoint triggerLocation,
        CancellationToken cancellationToken)
    {
        var entityName = semanticModel.GetConstantValue(entityArgument.Expression).Value as string;
        if (string.IsNullOrEmpty(entityName)) return CompletionContext.Empty;

        var entityMetadataRepository = await repositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var entity = await entityMetadataRepository.GetAsync(entityName, cancellationToken).ConfigureAwait(false);
        var availableAttributes = entity.Attributes?.Select(a =>a.LogicalName).ToArray();
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
            ? literal.Token.ValueText.SplitAndTrim( ',')
            : literal.Token.ValueText.SplitAndTrim(',', currentWord);

        // Filter and provide completions
        var suggestedAttributes = availableAttributes
            .Where(attr => !currentXrmAttributes.Contains(attr))
            .Select(attr => new CompletionItem(attr, this))
            .ToList();

        return new CompletionContext([.. suggestedAttributes]);
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
        return text.Substring(start, end - start);
    }
}