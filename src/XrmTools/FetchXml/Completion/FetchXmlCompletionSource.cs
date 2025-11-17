#nullable enable
namespace XrmTools.FetchXml.Completion;

using Microsoft.CodeAnalysis;
using Microsoft.Language.Xml;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using XrmTools.CodeCompletion;
using XrmTools.Logging.Compatibility;
using XrmTools.Xrm.Repositories;

internal sealed class FetchXmlCompletionSource(
    ILogger logger,
    IRepositoryFactory repositoryFactory,
    VisualStudioWorkspace workspace) : XrmCompletionSource(logger, repositoryFactory, workspace)
{
    public override CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken cancellationToken)
    {
        var snapshot = triggerLocation.Snapshot;
        var tokenSpan = ComputeTokenSpan(triggerLocation);

        //var doc = Parser.ParseText(snapshot.GetText());
        var fetchXmlDoc = snapshot.TextBuffer.GetFetchXmlDocument(logger);
        if (fetchXmlDoc is null || fetchXmlDoc.IsParsing) return CompletionStartData.DoesNotParticipateInCompletion;
        var doc = fetchXmlDoc.XmlDocument;
        doc ??= Parser.ParseText(snapshot.GetText());
        var currentNode = SyntaxLocator.FindNode(doc, tokenSpan.Start);
        return IsSupported(currentNode)
            ? new CompletionStartData(CompletionParticipation.ExclusivelyProvidesItems, tokenSpan)
            : CompletionStartData.DoesNotParticipateInCompletion;
    }

    private bool IsSupported(Microsoft.Language.Xml.SyntaxNode node)
    {
        if (node.Kind is not SyntaxKind.XmlTextLiteralToken and not SyntaxKind.DoubleQuoteToken || node.Parent.Kind is not SyntaxKind.XmlString) return false;

        var attribute = node.FirstAncestorOrSelf<XmlAttributeSyntax>();
        if (attribute is null) return false;

        var element = attribute.ParentElement;
        if (element is null) return false;

        if (element.Name is "entity" or "link-entity" && attribute.Name is "name") { Debug.Print($"FetchXml-AutoComplete:{element.Name}.{attribute.Name}"); return true; }
        if (element.Name is "order" or "condition" && attribute.Name is "attribute" 
            || element.Name is "attribute" && attribute.Name is "name"
            || element.Name is "link-entity" && attribute.Name is "from" or "to") { Debug.Print($"FetchXml-AutoComplete:{element.Name}.{attribute.Name}"); return true; }

        return false;
    }

    public override async Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken cancellationToken)
    {
        try
        {
           var doc = Parser.ParseText(triggerLocation.Snapshot.GetText());
            var currentNode = SyntaxLocator.FindNode(doc, applicableToSpan.Start);

            var attribute = currentNode.FirstAncestorOrSelf<XmlAttributeSyntax>();
            if (attribute is null) return CompletionContext.Empty;

            var element = attribute.ParentElement;
            if (element is null) return CompletionContext.Empty;

            if (element.Name is "entity" or "link-entity" && attribute.Name is "name") return await GetEntityCompletionsAsync(cancellationToken).ConfigureAwait(false);
            if (element.Name is "order" or "condition" && attribute.Name is "attribute"
                || element.Name is "attribute" && attribute.Name is "name" 
                || element.Name is "link-entity" && attribute.Name is "from" or "to")
            {
                var parentEntityElement = element.AncestorsAndSelf().FirstOrDefault(e => e.Name is "entity" or "link-entity");
                if (parentEntityElement is null) return CompletionContext.Empty;
                if (element.Name is "link-entity" && attribute.Name is "to") parentEntityElement = parentEntityElement.Ancestors().FirstOrDefault(e => e.Name is "entity" or "link-entity");
                if (parentEntityElement is null) return CompletionContext.Empty;

                var entityName = parentEntityElement.Attributes.FirstOrDefault(a => a.Name == "name")?.Value;
                if (string.IsNullOrEmpty(entityName)) return CompletionContext.Empty;

                return await GetAttributeCompletionsAsync(entityName!, [], cancellationToken).ConfigureAwait(false);
            }

            return CompletionContext.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError("FetchXml completion error: " + ex.Message);
            return CompletionContext.Empty;
        }
    }

    private static SnapshotSpan ComputeTokenSpan(SnapshotPoint point)
    {
        var snapshot = point.Snapshot;
        int pos = point.Position;
        if (pos == 0) return new SnapshotSpan(snapshot, pos, 0);
        static bool IsTokenChar(char ch) => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ;
        int start = pos;
        int end = pos;
        if (pos > 0 && IsTokenChar(snapshot[pos - 1]))
        {
            start = pos - 1;
            while (start > 0 && IsTokenChar(snapshot[start - 1])) start--;
        }
        while (end < snapshot.Length && IsTokenChar(snapshot[end])) end++;
        return new SnapshotSpan(snapshot, start, end - start);
    }

    public override Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token) => Task.FromResult<object>(null);
}
#nullable restore
