#nullable enable
namespace XrmTools.CodeCompletion;

using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using XrmTools.Helpers;
using System;
using XrmTools.Logging.Compatibility;
using XrmTools.Xrm.CodeCompletion;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using XrmTools.Xrm.Repositories;

[Export(typeof(IAsyncCompletionSourceProvider))]
[ContentType("CSharp")]
[Name(nameof(XrmCSharpCompletionProvider))]
[TextViewRole(PredefinedTextViewRoles.Editable)]
[method: ImportingConstructor]
internal class XrmCSharpCompletionProvider(
    [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
    [Import] ILogger<XrmCSharpCompletionProvider> logger,
    [Import] IRepositoryFactory repositoryFactory,
    [Import] ITextStructureNavigatorSelectorService structureNavigatorSelector) : IAsyncCompletionSourceProvider
{
    private IVsRunningDocumentTable? _RunningDocumenTable;

    public IAsyncCompletionSource? GetOrCreate(ITextView textView)
    {
        if (textView is null) { return null; }
        if (repositoryFactory is null)
        {
            throw new InvalidOperationException($"{nameof(RepositoryFactory)} is missing in {nameof(XrmPluginDefCompletionProvider)}.");
        }
        return textView.Properties.GetOrCreateSingletonProperty(() =>
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            //if (textView.TextBuffer.Properties.GetProperty(typeof(ITextDocument)) is ITextDocument document)
            //{
            //    return new XrmPluginDefCompletionSource(logger, xrmSchemaProviderFactory, structureNavigatorSelector, textView.TextBuffer);
            //}
            //return null;

            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var workspace = componentModel.GetService<VisualStudioWorkspace>();
            return new XrmCSharpCompletionSource(logger, repositoryFactory, workspace);
        });
    }

    private IVsRunningDocumentTable? RunningDocumenTable
        => _RunningDocumenTable ??= serviceProvider?.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();

    private string? GetProjectProperty(string inputFilePath, string propertyName) => RunningDocumenTable?.TryGetHierarchyAndItemID(inputFilePath, out var hierarchy, out _) == true
        ? hierarchy.GetProjectProperty(propertyName)
        : null;


    /*public override async Task ProvideCompletionsAsync(CompletionContext context)
    {
        var document = context.Document;
        var position = context.Position;
        var cancellationToken = context.CancellationToken;

        // Get the syntax tree and semantic model
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (syntaxTree == null || semanticModel == null) return;

        // Find the attribute syntax at the current position
        var root = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
        var node = root.FindToken(position).Parent;

        if (node == null) return;

        // Check if the node is part of an AttributeSyntax
        var attributeSyntax = node.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
        if (attributeSyntax == null || !IsStepAttribute(attributeSyntax, semanticModel)) return;

        // Determine the argument position
        var argumentList = attributeSyntax.ArgumentList?.Arguments;
        if (argumentList == null || argumentList.Count < 3) return;

        // Identify which argument the cursor is within
        var argumentIndex = GetArgumentIndexAtPosition(argumentList, position);
        if (argumentIndex == -1) return;

        // Provide completions based on the argument index
        switch (argumentIndex)
        {
            case 0:
                await ProvideEntityCompletions(context);
                break;
            case 1:
                await ProvideMessageCompletions(context, argumentList[0], semanticModel);
                break;
            case 2:
                await ProvideAttributeCompletions(context, argumentList[0], semanticModel);
                break;
        }
    }*/

}
#nullable restore
