namespace XrmTools.CodeCompletion;

using Microsoft.VisualStudio.Core.Imaging;
//using Microsoft.CodeAnalysis.Completion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Immutable;

internal static class XrmSolutionCompletionFilters
{
    public static readonly ImageElement SolutionIcon = new(new ImageId(PackageGuids.AssetsGuid, PackageIds.DataverseSolution), "Unmanaged");

    public static readonly ImmutableArray<CompletionFilter> SolutionFilters = [new("Unmanaged", "S", SolutionIcon)];
}
