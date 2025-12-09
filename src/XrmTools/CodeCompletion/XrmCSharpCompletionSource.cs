#nullable enable
namespace XrmTools.CodeCompletion;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Helpers;
using XrmTools.Core.Repositories;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Attributes;
using XrmTools.Xrm.Repositories;

internal sealed class XrmCSharpCompletionSource(
    ILogger logger, IRepositoryFactory repositoryFactory, VisualStudioWorkspace workspace) 
    : XrmCompletionSourceBase(logger, repositoryFactory, workspace)
{
    const int StepCtorArgumentMessageIndex = 0;
    const int StepCtorArgumentEntityIndex = 1;
    const int StepCtorArgumentFilteringAttributesIndex = 2;
    const int ImageCtorArgumentFilteringAttributesIndex = 1;
    const int EntityCtorArgumentEntityIndex = 0;
    const int EntityCtorArgumentAttributesIndex = 1;
    const int SolutionCtorArgumentUniqueNameIndex = 0;

    public override async Task<CompletionContext> GetCompletionContextAsync(
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

        if (IsSolutionAttribute(attributeSyntax, semanticModel) && argumentIndex == SolutionCtorArgumentUniqueNameIndex)
        {
            return node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression) ?
                await GetSolutionCompletionsAsync(cancellationToken).ConfigureAwait(false) :
                CompletionContext.Empty;
        }

        return CompletionContext.Empty;
    }

    private async Task<CompletionContext> GetEntityCompletionsAsync(
        AttributeArgumentSyntax messageArgument,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var messageName = semanticModel.GetConstantValue(messageArgument.Expression).Value as string;
        return await GetEntityCompletionsAsync(messageName, cancellationToken).ConfigureAwait(false);
    }

    private async Task<CompletionContext> GetAttributeCompletionsAsync(
        AttributeArgumentSyntax entityArgument,
        SemanticModel semanticModel,
        SnapshotPoint triggerLocation,
        CancellationToken cancellationToken)
    {
        if (entityArgument == null) return CompletionContext.Empty;
        var entityName = semanticModel.GetConstantValue(entityArgument.Expression).Value as string;
        if (string.IsNullOrEmpty(entityName)) return CompletionContext.Empty;

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

        return await GetAttributeCompletionsAsync(entityName, currentXrmAttributes, cancellationToken).ConfigureAwait(false);
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
        using var entityMetadataRepository = repositoryFactory.CreateRepository<IEntityMetadataRepository>();
        if (entityMetadataRepository == null) return CompletionContext.Empty;
        var messages = await entityMetadataRepository.GetAvailableMessagesAsync(entityName, cancellationToken).ConfigureAwait(false);
        return new CompletionContext([.. messages.Select(message => new CompletionItem(message.Name, this))]);
    }

    public override Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken cancellationToken)
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

    private bool IsSolutionAttribute(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);
        var symbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
        return symbol?.ContainingType.Name == nameof(SolutionAttribute);
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

    public override CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken cancellationToken)
    {
        // Indicate when completion should be triggered
        var span = triggerLocation.Snapshot.CreateTrackingSpan(triggerLocation.Position, 0, SpanTrackingMode.EdgeInclusive);
        return new CompletionStartData(CompletionParticipation.ProvidesItems, new SnapshotSpan(span.GetStartPoint(triggerLocation.Snapshot), span.GetEndPoint(triggerLocation.Snapshot)));
    }

    public static string CurrentWord(SnapshotPoint triggerLocation, LiteralExpressionSyntax literal)
    {
        var text = literal.Token.Text;
        var index = triggerLocation.Position - literal.SpanStart;

        if (index < 0) index = 0;
        if (index > text.Length) index = text.Length;

        var start = index - 1;
        var end = index;
        var length = text.Length;

        var terminators = new List<char>(['\"', ' ', ',']);

        // Move start backward while within bounds and not a terminator
        while (start > 0 && !terminators.Contains(text[start]))
            start--;

        // If we stopped at a terminator and haven't gone past index, move forward
        if (terminators.Contains(text[start]) && start < length - 1)
            start++;

        // Move end forward while within bounds and not a terminator
        while (end < length && !terminators.Contains(text[end]))
            end++;

        // Ensure valid slicing
        if (start >= end || start >= length)
            return string.Empty;

        return text[start..end];
    }
}
#nullable restore