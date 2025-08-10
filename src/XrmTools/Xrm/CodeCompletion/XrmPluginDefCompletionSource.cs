using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Operations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Repositories;
using XrmTools.Helpers;
using XrmTools.Logging;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using XrmTools.Xrm.Extensions;
using XrmTools.Xrm.Repositories;

namespace XrmTools.Xrm.CodeCompletion;

internal class XrmPluginDefCompletionSource(
    IOutputLoggerService logger,
    IRepositoryFactory repositoryFactory, 
    ITextStructureNavigatorSelectorService structureNavigatorSelector, 
    ITextBuffer textBuffer) : IAsyncCompletionSource
{
    private static readonly ImageElement StandardEntityIcon = new(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2708), "Standard");
    private static readonly ImageElement CustomEntityIcon = new(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2709), "Custom");
    private static readonly CompletionFilter StandardEntityFilter = new("Standard", "S", StandardEntityIcon);
    private static readonly CompletionFilter CustomEntityFilter = new("Custom", "C", CustomEntityIcon);
    private static readonly ImmutableArray<CompletionFilter> StandardEntityFilters = [StandardEntityFilter];
    private static readonly ImmutableArray<CompletionFilter> CustomEntityFilters = [CustomEntityFilter];

    public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
    {
        using var catalog = ThreadHelper.JoinableTaskFactory.Run(repositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>);
        if (catalog == null)
        { 
            // Catalog is not initialized yet. We can't provide completion.
            logger.LogWarning("Xrm code completion not available. Check if your environment setup is done under Tools > Options > Xrm Tools.");
            return CompletionStartData.DoesNotParticipateInCompletion;
        }
        var navigator = structureNavigatorSelector.GetTextStructureNavigator(triggerLocation.Snapshot.TextBuffer);
        var extent = navigator.GetExtentOfWord(triggerLocation);
        var span = navigator.GetSpanOfEnclosing(extent.Span);
        var spanText = span.GetText();
        // We don't trigger completion when user type:
        // - a number
        // - a punctuation character not between quotes
        // - a new line
        // - a backspace
        // - a deletion
        if (char.IsNumber(trigger.Character)
            || trigger.Character == '\n'
            || trigger.Reason == CompletionTriggerReason.Backspace
            || trigger.Reason == CompletionTriggerReason.Deletion)
        {
            return CompletionStartData.DoesNotParticipateInCompletion;
        }

        // We trigger completion when user type:
        // - a punctuation character between quotes
        if (char.IsPunctuation(trigger.Character)
            && spanText.Length > 3 && spanText[0] == '\"'
            && spanText[^1] == '\"')
        {
            return new (CompletionParticipation.ProvidesItems, new SnapshotSpan(triggerLocation, 0));
        }

        // We don't trigger completion when user type:
        // - a punctuation character not between quotes
        if (char.IsPunctuation(trigger.Character))
        {
            return CompletionStartData.DoesNotParticipateInCompletion;
        }

        // We participate in completion and provide the "applicable to span".
        // This span is used:
        // 1. To search (filter) the list of all completion items
        // 2. To highlight (bold) the matching part of the completion items
        // 3. In standard cases, it is replaced by content of completion item upon commit.

            // If you want to extend a language which already has completion, don't provide a span, e.g.
            // return CompletionStartData.ParticipatesInCompletionIfAny

            // If you provide a language, but don't have any items available at this location,
            // consider providing a span for extenders who can't parse the codem e.g.
            // return CompletionStartData(CompletionParticipation.DoesNotProvideItems, spanForOtherExtensions);

        var tokenSpan = FindTokenSpanAtPosition(triggerLocation);
        return new CompletionStartData(CompletionParticipation.ProvidesItems, tokenSpan);
    }

    public async Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
    {
        // See whether we are in the key or value portion of the pair
        var lineStart = triggerLocation.GetContainingLine().Start;
        var spanBeforeCaret = new SnapshotSpan(lineStart, triggerLocation);
        var textBeforeCaret = triggerLocation.Snapshot.GetText(spanBeforeCaret);
        var colonIndex = textBeforeCaret.IndexOf(':');
        var colonExistsBeforeCaret = colonIndex != -1;

        // User is likely in the key portion of the pair
        if (!colonExistsBeforeCaret)
            return CompletionContext.Empty;

        // User is likely in the value portion of the pair. Try to provide extra items based on the key.
        var KeyExtractingRegex = new Regex(@"\W*(\w+)\W*:");
        var key = KeyExtractingRegex.Match(textBeforeCaret);
        var keyText = key.Success ? key.Groups.Count > 0 && key.Groups[1].Success ? key.Groups[1].Value : string.Empty : string.Empty;
        if (keyText == "PrimaryEntityName")
        {
            return await GetContextForEntityNameAsync(token);
        }
        if (keyText == "FilteringAttributes")
        {
            var objSpan = FindJObjectSpanAtPosition(triggerLocation);
            if (objSpan.Length > 0)
            {
                var objText = objSpan.GetText();
                try
                {
                    var obj = JObject.Parse(objText);
                    if (obj.TryGetValue("PrimaryEntityName", out var entityName))
                    {
                        return await GetContextForAttributeNameAsync(entityName.ToString(), token);
                    }
                }
                catch (Exception) { }
            }
        }

        return CompletionContext.Empty;
    }

    public async Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
    {
        if (item.Properties.TryGetProperty<EntityMetadata>(nameof(EntityMetadata), out var matchingEntity))
        {
            var b = new StringBuilder();
            b.AppendLineWhenNotEmpty(matchingEntity.DisplayName.GetLocalized());
            b.AppendWhenNotEmpty(matchingEntity.Description.GetLocalized());
            return await Task.FromResult(b.ToString());
        }
        if (item.Properties.TryGetProperty<AttributeMetadata>(nameof(AttributeMetadata), out var matchingAttribute))
        {
            var b = new StringBuilder();
            b.AppendLineWhenNotEmpty(matchingAttribute.DisplayName.GetLocalized());
            var typeName = matchingAttribute.AttributeTypeName.Value;
            b.AppendLine("Type: " + typeName.Substring(0, typeName.Length - 4));
            if (matchingAttribute.AttributeTypeName == AttributeTypeDisplayName.LookupType)
            {
                var lookup = (LookupAttributeMetadata)matchingAttribute;
                b.Append("Target: ");
                b.AppendLine(lookup.Targets.Aggregate((a, b) => $"{a}, {b}"));
                if (lookup.Format != null)
                {
                    b.Append("Format: ");
                    b.AppendLine(Enum.GetName(typeof(LookupFormat), lookup.Format));
                }
            }
            else if (matchingAttribute.AttributeTypeName == AttributeTypeDisplayName.PicklistType
                || matchingAttribute.AttributeTypeName == AttributeTypeDisplayName.MultiSelectPicklistType)
            {
                var picklist = (EnumAttributeMetadata)matchingAttribute;
                b.Append("Options: ");
                b.AppendLine(picklist.OptionSet.Options.Select(o => o.Value + ": " + o.Label.GetLocalized()).Aggregate((a, b) => $"{a}, {b}"));
            }
            b.AppendWhenNotEmpty(matchingAttribute.Description.GetLocalized());
            return await Task.FromResult(b.ToString());
        }
        return null;
    }

    private async Task<CompletionContext> GetContextForEntityNameAsync(CancellationToken cancellationToken)
    {
        using var catalog = ThreadHelper.JoinableTaskFactory.Run(repositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>);
        return new CompletionContext((await catalog.GetAsync(cancellationToken)).Where(e => !e.IsLogicalEntity ?? false).Select(MakeItemFromMetadata).ToImmutableArray());
    }

    private async Task<CompletionContext> GetContextForAttributeNameAsync(string entityName, CancellationToken cancellationToken)
    {
        using var catalog = ThreadHelper.JoinableTaskFactory.Run(repositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>);
        return new CompletionContext((await catalog.GetAsync(entityName, cancellationToken)).Attributes.Where(a => !a.IsLogical ?? false).Select(MakeItemFromMetadata).ToImmutableArray());
    }

    private CompletionItem MakeItemFromMetadata(EntityMetadata entity)
    {
        ImageElement icon = null;
        ImmutableArray<CompletionFilter> filters;

        if (entity.IsCustomEntity == true)
        {
            icon = CustomEntityIcon;
            filters = CustomEntityFilters;
        }
        else
        {
            icon = StandardEntityIcon;
            filters = StandardEntityFilters;
        }

        var item = new CompletionItem(
                displayText: entity.LogicalName,
                source: this,
                icon: icon,
                filters: filters,
                suffix: "",
                insertText: entity.LogicalName,
                sortText: entity.LogicalName,
                filterText: $"{entity.LogicalName} {entity.DisplayName}",
        attributeIcons: []);

        // Each completion item we build has a reference to the element in the property bag.
        // We use this information when we construct the tooltip.
        item.Properties.AddProperty(nameof(EntityMetadata), entity);

        return item;
    }

    private CompletionItem MakeItemFromMetadata(AttributeMetadata attribute)
    {
        ImageElement icon = null;
        ImmutableArray<CompletionFilter> filters;

        if (attribute.IsCustomAttribute == true)
        {
            icon = CustomEntityIcon;
            filters = CustomEntityFilters;
        }
        else
        {
            icon = StandardEntityIcon;
            filters = StandardEntityFilters;
        }

        var item = new CompletionItem(
                displayText: attribute.LogicalName,
                source: this,
                icon: icon,
                filters: filters,
                suffix: "",
                insertText: attribute.LogicalName,
                sortText: attribute.LogicalName,
                filterText: $"{attribute.LogicalName} {attribute.DisplayName}",
        attributeIcons: []);

        // Each completion item we build has a reference to the element in the property bag.
        // We use this information when we construct the tooltip.
        item.Properties.AddProperty(nameof(AttributeMetadata), attribute);

        return item;
    }

    private SnapshotSpan FindTokenSpanAtPosition(SnapshotPoint triggerLocation)
    {
        // This method is not really related to completion,
        // we mostly work with the default implementation of ITextStructureNavigator 
        // You will likely use the parser of your language
        ITextStructureNavigator navigator = structureNavigatorSelector.GetTextStructureNavigator(triggerLocation.Snapshot.TextBuffer);
        TextExtent extent = navigator.GetExtentOfWord(triggerLocation);
        if (triggerLocation.Position > 0 && (!extent.IsSignificant || !extent.Span.GetText().Any(c => char.IsLetterOrDigit(c))))
        {
            // Improves span detection over the default ITextStructureNavigation result
            extent = navigator.GetExtentOfWord(triggerLocation - 1);
        }

        var tokenSpan = triggerLocation.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);

        var snapshot = triggerLocation.Snapshot;
        var tokenText = tokenSpan.GetText(snapshot);
        if (string.IsNullOrWhiteSpace(tokenText))
        {
            // The token at this location is empty. Return an empty span, which will grow as user types.
            return new SnapshotSpan(triggerLocation, 0);
        }

        // Trim quotes and new line characters.
        int startOffset = 0;
        int endOffset = 0;

        if (tokenText.Length > 0)
        {
            if (tokenText.StartsWith("\""))
                startOffset = 1;
        }
        if (tokenText.Length - startOffset > 0)
        {
            if (tokenText.EndsWith("\"\r\n"))
                endOffset = 3;
            else if (tokenText.EndsWith("\r\n"))
                endOffset = 2;
            else if (tokenText.EndsWith("\"\n"))
                endOffset = 2;
            else if (tokenText.EndsWith("\n"))
                endOffset = 1;
            else if (tokenText.EndsWith("\""))
                endOffset = 1;
        }

        return new SnapshotSpan(tokenSpan.GetStartPoint(snapshot) + startOffset, tokenSpan.GetEndPoint(snapshot) - endOffset);
    }

    private SnapshotSpan FindJObjectSpanAtPosition(SnapshotPoint triggerLocation)
    {
        var snapshot = triggerLocation.Snapshot;
        var start = snapshot.GetText(0, triggerLocation.Position).LastIndexOf("{");
        if (start == -1)
        {
            return new SnapshotSpan(triggerLocation, 0);
        }
        var length = snapshot.GetText(triggerLocation.Position, snapshot.Length - triggerLocation.Position).IndexOf("}");
        if (length == -1)
        {
            return new SnapshotSpan(triggerLocation, 0);
        }
        return new SnapshotSpan(snapshot, start, length + triggerLocation.Position - start + 1);
    }
}