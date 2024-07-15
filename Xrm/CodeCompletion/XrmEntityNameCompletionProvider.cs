using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using Nito.AsyncEx.Synchronous;
using XrmGen._Core;
using System.Text.Json;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.PlatformUI;
using System.Runtime.InteropServices;


namespace XrmGen.Xrm.CodeCompletion;

[Export(typeof(ICompletionSourceProvider))]
[ContentType("json")]
[Name("jsonCompletion")]
internal class XrmEntityNameCompletionHandlerProvider : ICompletionSourceProvider
{
    [Import]
    private ITextStructureNavigatorSelectorService navigatorService = null;

    public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        => new XrmEntityNameCompletionHandler(textBuffer, navigatorService);
}

internal class XrmEntityNameCompletionHandler(ITextBuffer textBuffer, ITextStructureNavigatorSelectorService navigatorService) : ICompletionSource
{
    private bool _isDisposed = false;
    private readonly XrmSchemaProvider schemaProvider = XrmSchemaProvider.Instance;



    public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
    {
        if (_isDisposed)
            throw new ObjectDisposedException("JsonCompletionHandler");

        var triggerPoint = (SnapshotPoint)session.GetTriggerPoint(textBuffer.CurrentSnapshot);

        if (triggerPoint == null)
            return;

        //var caretPosition = triggerPoint.Position - triggerPoint.GetContainingLine().Start.Position;
        //textBuffer.CurrentSnapshot.TextBuffer.Properties.TryGetProperty(typeof(ITextStructureNavigator), out ITextStructureNavigator textNavigator);

        var textNavigator = navigatorService.GetTextStructureNavigator(textBuffer);

        var extent = textNavigator.GetExtentOfWord(triggerPoint);
        var span = textNavigator.GetSpanOfEnclosing(extent.Span);

        // Get the parent of the current block.
        var startObjectPosition = textBuffer.CurrentSnapshot.GetText(0, triggerPoint.Position).LastIndexOf('{');
        var endObjectPosition = textBuffer.CurrentSnapshot.GetText(triggerPoint.Position, textBuffer.CurrentSnapshot.Length).IndexOf('}');
        var obj = JObject.Parse(textBuffer.CurrentSnapshot.GetText(startObjectPosition, triggerPoint.Position + endObjectPosition));
        if (obj.TryGetValue("PrimaryEntityName", out var entityName))
        {
            var trackingSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(
                new SnapshotSpan(triggerPoint, 0),
                SpanTrackingMode.EdgeInclusive
            );

            completionSets.Add(new CompletionSet(
                "JSON",
                "JSON",
                trackingSpan,
                GetEntityNameCompletionsAsync(entityName.ToString()).WaitAndUnwrapException(),
                null
            ));
        }


        var text = textBuffer.CurrentSnapshot.GetText(span);
        if (!text.StartsWith("\"PrimaryEntityName\""))
            return;

        text = text.Substring(text.IndexOf(':')).Trim([' ', '\"', ':', ',']);

        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            var completions = await GetEntityNameCompletionsAsync(text);
            var trackingSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(
                new SnapshotSpan(triggerPoint, 0),
                SpanTrackingMode.EdgeInclusive
            );

            completionSets.Add(new CompletionSet(
                "JSON",
                "JSON",
                trackingSpan,
                completions,
                null
            ));
        });
    }

    private async Task<List<Completion>> GetEntityNameCompletionsAsync(string text)
    {
        var entityNames = await schemaProvider.GetEntityNamesAsync();
        return entityNames
            .Where(name => string.IsNullOrWhiteSpace(text) || name.StartsWith(text))
            .Select(name => new Completion(name)).ToList();
    }

    private async Task<List<Completion>> GetAttributeNameCompletionsAsync(string text)
    {
        var entityNames = await schemaProvider.GetEntityNamesAsync();
        return entityNames
            .Where(name => string.IsNullOrWhiteSpace(text) || name.StartsWith(text))
            .Select(name => new Completion(name)).ToList();
    }

    public void Dispose() => _isDisposed = true;
}
