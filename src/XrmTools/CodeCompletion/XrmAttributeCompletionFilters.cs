namespace XrmTools.CodeCompletion;

//using Microsoft.CodeAnalysis.Completion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text.Adornments;
using System.Collections.Immutable;

internal static class XrmAttributeCompletionFilters
{
    public static readonly ImageElement BooleanColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 296), "Boolean");
    public static readonly ImageElement DateTimeColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 371), "DateTime");
    public static readonly ImageElement NumberColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1017), "Number");
    public static readonly ImageElement LookupColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1724), "Lookup");
    public static readonly ImageElement MoneyColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 803), "Money");
    public static readonly ImageElement StringColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2985), "String");
    public static readonly ImageElement PickListColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 982), "PickList");
    public static readonly ImageElement MiscColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1217), "Other");
    public static readonly ImageElement KeyColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 1654), "Key");
    public static readonly ImageElement StateColumnIcon = new(new(new("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2919), "State");

    public static readonly ImmutableArray<CompletionFilter> BooleanColumnFilters = [new("Boolean", "B", BooleanColumnIcon)];
    public static readonly ImmutableArray<CompletionFilter> DateTimeColumnFilters = [new("DateTime", "D", DateTimeColumnIcon)];
    public static readonly ImmutableArray<CompletionFilter> NumberColumnFilters = [new("Number", "N", NumberColumnIcon)];
    public static readonly ImmutableArray<CompletionFilter> LookupColumnFilters = [new("Lookup", "L", LookupColumnIcon)];
    public static readonly ImmutableArray<CompletionFilter> MoneyColumnFilters = [new("Currency", "C", MoneyColumnIcon)];
    public static readonly ImmutableArray<CompletionFilter> StringColumnFilters = [new("String", "S", StringColumnIcon)];
    public static readonly ImmutableArray<CompletionFilter> PickListColumnFilters = [new("PickList", "P", PickListColumnIcon)];
    public static readonly ImmutableArray<CompletionFilter> MiscColumnFilters = [new("Misc", "M", MiscColumnIcon)];//, OtherColumnIcon)]
}
