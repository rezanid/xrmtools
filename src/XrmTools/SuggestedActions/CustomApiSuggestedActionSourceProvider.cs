namespace XrmTools.SuggestedActions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Meta.Attributes;
using XrmTools.WebApi;

[Export(typeof(ISuggestedActionsSourceProvider))]
[Name("Custom API Actions")]
[ContentType("CSharp")]
internal class CustomApiSuggestedActionSourceProvider : ISuggestedActionsSourceProvider
{
    [Import(typeof(ITextStructureNavigatorSelectorService))]
    internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

    [Import(typeof(IWebApiService))]
    internal IWebApiService WebApiService { get; set; }

    public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
    {
        if (textBuffer == null || textView == null)
        {
            return null;
        }
        return new CustomApiSuggestedActionsSource(this, textView, textBuffer, WebApiService);
    }
}

internal class CustomApiSuggestedActionsSource(
    CustomApiSuggestedActionSourceProvider provider,
    ITextView textView,
    ITextBuffer textBuffer,
    IWebApiService webApi) : ISuggestedActionsSource
{
    private string customApiName;
    private Span customApiSpan;
    private Span customApiClassSpan;

    public event EventHandler<EventArgs> SuggestedActionsChanged;

    public async Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
    {
        (bool isValid, customApiName, customApiSpan, customApiClassSpan) = await IsCustomApiWithNameButNoPropertiesAsync(cancellationToken);
        return isValid;
    }

    public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
    {
        if (customApiName == null)
        {
            (bool isValid, customApiName, customApiSpan, customApiClassSpan) = ThreadHelper.JoinableTaskFactory.Run(async () => await IsCustomApiWithNameButNoPropertiesAsync(cancellationToken));
        }
        if (customApiName == null)
        {
            return [];
        }

        ITrackingSpan attributeTrackingSpan = range.Snapshot.CreateTrackingSpan(customApiSpan, SpanTrackingMode.EdgeInclusive);
        ITrackingSpan classTrackingSpan = range.Snapshot.CreateTrackingSpan(customApiClassSpan, SpanTrackingMode.EdgeInclusive);

        var basicAction = new FixCustomApiAttributeAction(attributeTrackingSpan, customApiName, webApi);
        var extendedAction = new FixCustomApiAttributeWithRequestResponseAction(classTrackingSpan, customApiName, webApi);

        return [
            new SuggestedActionSet(
                PredefinedSuggestedActionCategoryNames.Refactoring,
                [basicAction, extendedAction],
                title: "Expand Custom API",
                priority: SuggestedActionSetPriority.Medium)
        ];
    }

    public void Dispose() { }

    public bool TryGetTelemetryId(out Guid telemetryId)
    {
        // It doesn't participate in LightBulb telemetry
        telemetryId = Guid.Empty;
        return false;
    }

    private bool TryGetWordUnderCaret(out TextExtent wordExtent)
    {
        ITextCaret caret = textView.Caret;
        SnapshotPoint point;

        if (caret.Position.BufferPosition > 0)
        {
            point = caret.Position.BufferPosition - 1;
        }
        else
        {
            wordExtent = default;
            return false;
        }

        ITextStructureNavigator navigator = provider.NavigatorService.GetTextStructureNavigator(textBuffer);

        wordExtent = navigator.GetExtentOfWord(point);
        return true;
    }

    private async Task<(bool isValid, string apiName, Span attributeSpan, Span classSpan)> IsCustomApiWithNameButNoPropertiesAsync(CancellationToken cancellationToken = default)
    {
        // Retrieve the document text and syntax tree
        var document = textBuffer.GetRelatedDocuments().FirstOrDefault();
        if (document == null) return (false, null, default, default);

        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (syntaxTree == null || semanticModel == null) return (false, null, default, default);

        // Get the attribute at the current cursor position
        var root = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
        var node = root.FindToken(textView.Caret.Position.BufferPosition.Position).Parent;
        if (node == null) return (false, null, default, default);

        var attributeSyntax =
            node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.AttributeList) ?
            ((AttributeListSyntax)node).Attributes.FirstOrDefault(a => IsCustomApiAttribute(a, semanticModel)) :
            node.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault(a => IsCustomApiAttribute(a, semanticModel));
        if (attributeSyntax == null) return (false, null, default, default);


        // Check if the attribute only has one argument
        var arguments = attributeSyntax.ArgumentList?.Arguments;
        if (arguments == null || arguments.Value.Count != 1) return (false, null, default, default);

        var customApiName = semanticModel.GetConstantValue(arguments.Value[0].Expression).Value as string;

        // Get the class span
        var classSyntax = attributeSyntax.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();

        return (true, customApiName, new Span(attributeSyntax.SpanStart, attributeSyntax.Span.Length), new Span(classSyntax.SpanStart, classSyntax.Span.Length));
    }

    private bool IsCustomApiAttribute(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);
        var symbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
        return symbol?.ContainingType.Name == nameof(CustomApiAttribute);
    }
}