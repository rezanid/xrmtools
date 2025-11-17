namespace XrmTools.CodeCompletion;

using Microsoft.VisualStudio.Core.Imaging;
//using Microsoft.CodeAnalysis.Completion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Immutable;

internal static class XrmEntityCompletionFilters
{
    public static readonly ImageElement StandardTableIcon = new(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 3032), "Standard");
    public static readonly ImageElement ActivityTableIcon = new(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1157), "Activity");
    public static readonly ImageElement ElasticTableIcon = new(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1060), "Elastic");
    public static readonly ImageElement VirtualTableIcon = new(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 887), "Virtual");

    public static readonly ImmutableArray<CompletionFilter> StandardTableFilters = [new("Standard", "S", StandardTableIcon)];
    public static readonly ImmutableArray<CompletionFilter> ActivityTableFilters = [new("Activity", "A", ActivityTableIcon)];
    public static readonly ImmutableArray<CompletionFilter> ElasticTableFilters = [new("Elastic", "E", ElasticTableIcon)];
    public static readonly ImmutableArray<CompletionFilter> VirtualTableFilters = [new("Virtual", "V", VirtualTableIcon)];
}
